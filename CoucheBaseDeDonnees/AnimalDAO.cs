using System.Data;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class AnimalDAO : AccesDBBase<Animal>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions en DB
        // -------------------------------------------------------

        protected override string AfficherListe => "animal_afficher_liste_complete";
        protected override string AfficherUnElement => "animal_afficher_un";

        protected override string Insert => "animal_insertion";
        protected override string? InsertOut => "p_identifiant";              

        protected override string Update => "animal_modification";
        protected override string? UpdateOut => null;                         

        protected override string Delete => "animal_suppression";
        protected override string? DeleteOut => null;                         

        protected override string? ClesPrimaire => "p_identifiant";              // Clé primaire utilisée pour éviter le doublon lors d'un INSERT
        protected override bool DeleteUtiliseCleSeule => true;                   // animal_suppression n'attend que p_identifiant
                                                                                 // AssignerParametreSQL ne doit pas être appelée pour le DELETE

        // -------------------------------------------------------
        // Conversion DB → Objet métier (MAPPING)
        // -------------------------------------------------------

        protected override Animal ConvertirEnObjet(IDataReader reader)
        {
            string nom = GetStringSafe(reader, "nom");
            string type = GetStringSafe(reader, "type");
            string sexe = GetStringSafe(reader, "sexe");
            string sterilise = GetValueOrDefault<bool>(reader, "sterilise") ? "oui" : "non";
            string particularite = GetStringSafe(reader, "particularites");
            string description = GetStringSafe(reader, "description");

            DateTime dateNais = GetValueOrDefault<DateTime>(reader, "date_naissance");
            DateTime? dateDeces = GetDateTimeSafe(reader, "date_deces");
            DateTime? dateSteril = GetDateTimeSafe(reader, "date_sterilisation");

            Animal animal = Animal.Create(nom, type, sexe, sterilise, particularite, description,
                                          dateNais, dateDeces, dateSteril);

            animal.Identifiant = GetStringSafe(reader, "identifiant");

            return animal;
        }

        // -------------------------------------------------------
        // Assignation des paramètres SQL pour INSERT et UPDATE
        // p_identifiant est ajouté uniquement s'il n'est pas déjà présent :
        //   - INSERT : AccesDBBase l'ajoute déjà comme OUT → on ne l'ajoute pas
        //   - UPDATE : AccesDBBase n'ajoute rien → on l'ajoute comme IN
        //   - DELETE : AssignerCleSQL est appelée à la place → cette méthode n'est pas appelée
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Animal animal)
        {
            if (!cmd.Parameters.Contains(ClesPrimaire) && animal.Identifiant != null)
                 cmd.Parameters.AddWithValue("p_identifiant", animal.Identifiant);

            cmd.Parameters.AddWithValue("p_nom", animal.Nom);
            cmd.Parameters.AddWithValue("p_type", animal.Type);
            cmd.Parameters.AddWithValue("p_sexe", animal.Sexe);
            cmd.Parameters.AddWithValue("p_sterilise", animal.Sterilise == "oui");

            cmd.Parameters.Add("p_datenaissance", NpgsqlTypes.NpgsqlDbType.Date).Value
                = animal.DateDeNaissance;

            cmd.Parameters.Add("p_particularites", NpgsqlTypes.NpgsqlDbType.Varchar).Value =
                string.IsNullOrWhiteSpace(animal.Particularite)
                    ? DBNull.Value
                    : animal.Particularite;

            cmd.Parameters.Add("p_description", NpgsqlTypes.NpgsqlDbType.Varchar).Value =
                string.IsNullOrWhiteSpace(animal.Description)
                    ? DBNull.Value
                    : animal.Description;

            cmd.Parameters.Add("p_datedeces", NpgsqlTypes.NpgsqlDbType.Date).Value =
                animal.DateDeDeces.HasValue
                    ? animal.DateDeDeces.Value
                    : DBNull.Value;

            cmd.Parameters.Add("p_datesterilisation", NpgsqlTypes.NpgsqlDbType.Date).Value =
                animal.DateDeSterilisation.HasValue
                    ? animal.DateDeSterilisation.Value
                    : DBNull.Value;
        }

        // -------------------------------------------------------
        // Assignation de la clé primaire uniquement pour DELETE
        // animal_suppression n'attend que p_identifiant
        // -------------------------------------------------------

        protected override void AssignerCleSQL(NpgsqlCommand cmd, Animal animal)
        {
            cmd.Parameters.AddWithValue("p_identifiant", animal.Identifiant);
        }

        // -------------------------------------------------------
        // Retourne la liste complète des animaux
        // -------------------------------------------------------

        public async Task<List<Animal>> AfficherListeAnimaux()
        {
            return await ExecuterFonctionRetourListeAsync(AfficherListe);
        }

        // -------------------------------------------------------
        // Retourne un animal par son identifiant
        // -------------------------------------------------------

        public async Task<Animal?> AfficherUnAsync(string identifiant)
        {
            return await ExecuterFonctionRetourObjetAsync(AfficherUnElement, "p_identifiant", identifiant);
        }

        // -------------------------------------------------------
        // Génère l'identifiant de l'animal à partir de sa date de naissance
        // via la fonction SQL animal_generer_identifiant
        // -------------------------------------------------------

        public async Task<string?> GenererIdentifiantAsync(Animal animal)
        {
            string? retVal = null;

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = "SELECT animal_generer_identifiant(@p_date_naissance)";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.CommandType = CommandType.Text;

                    // Forcer NpgsqlDbType.Date pour éviter que Npgsql envoie un timestamp au lieu d'une date
                    cmd.Parameters.Add("@p_date_naissance", NpgsqlTypes.NpgsqlDbType.Date).Value
                        = animal.DateDeNaissance;

                    object? result = await cmd.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                        retVal = result.ToString();
                }
            }

            return retVal;
        }

        // -------------------------------------------------------
        // Override de InsertAsync pour générer l'identifiant avant l'insertion
        // -------------------------------------------------------

        public override async Task<string?> InsertAsync(Animal animal)
        {
            // On génère l'identifiant uniquement s'il n'est pas déjà défini
            if (string.IsNullOrWhiteSpace(animal.Identifiant))
            {
                string? id = await GenererIdentifiantAsync(animal);
                animal.Identifiant = id ?? throw new Exception("Impossible de générer un identifiant.");
            }
            return await base.InsertAsync(animal);
        }
    }
}
using System.Data;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class Famille_AccueilDAO : AccesDBBase<Famille_Accueil>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions SQL
        // -------------------------------------------------------

        protected override string AfficherListe => "famille_accueil_afficher_liste_complete";
        protected override string AfficherUnElement => "famille_accueil_afficher_un";
        protected override string Insert => "famille_accueil_insertion";
        protected override string? InsertOut => "p_fa_id";                

        protected override string Update => "famille_accueil_modification";
        protected override string? UpdateOut => null;                     

        protected override string Delete => "famille_accueil_suppression";
        protected override string? DeleteOut => null;                     

        protected override string? ClesPrimaire => "p_fa_id";                // Clé primaire utilisée pour éviter le doublon lors d'un INSERT
        protected override bool DeleteUtiliseCleSeule => true;                 // famille_accueil_suppression n'attend que p_fa_id
                                                                               // AssignerParametreSQL ne doit pas être appelée pour le DELETE

        // -------------------------------------------------------
        // Conversion DB → Objet métier
        // -------------------------------------------------------

        protected override Famille_Accueil ConvertirEnObjet(IDataReader reader)
        {
            Animal animal = ConstruireAnimal(reader);
            Contact contact = ConstruireContact(reader);

            int id = GetValueOrDefault<int>(reader, "fa_id");
            DateTime dateDebut = GetValueOrDefault<DateTime>(reader, "date_debut");
            DateTime? dateFin = GetDateTimeSafe(reader, "date_fin");

            return Famille_Accueil.Create(id, animal, contact, dateDebut, dateFin);
        }

        // -------------------------------------------------------
        // Construction de l'animal (identique à AdoptionDAO)
        // -------------------------------------------------------

        private Animal ConstruireAnimal(IDataReader reader)
        {
            Animal animal = Animal.Create(
                GetStringSafe(reader, "ani_nom"),
                GetStringSafe(reader, "ani_type"),
                GetStringSafe(reader, "ani_sexe"),
                GetValueOrDefault<bool>(reader, "ani_sterilise") ? "oui" : "non",
                GetStringSafe(reader, "ani_particularites"),
                GetStringSafe(reader, "ani_description"),
                GetValueOrDefault<DateTime>(reader, "ani_date_naissance"),
                GetDateTimeSafe(reader, "ani_date_deces"),
                GetDateTimeSafe(reader, "ani_date_sterilisation")
            );

            animal.Identifiant = GetStringSafe(reader, "ani_identifiant");

            return animal;
        }

        // -------------------------------------------------------
        // Construction du contact (identique à AdoptionDAO)
        // -------------------------------------------------------

        private Contact ConstruireContact(IDataReader reader)
        {
            Contact contact = Contact.Create(
                GetStringSafe(reader, "con_nom"),                // J'ai rencontré une erreur que j'ai fréquemment produite dans le code. N'ayant pas respecté l'ordre des paramètres
                GetStringSafe(reader, "con_prenom"),             // lors de l'appel registre national était envoyé à la propriété nom. Le nom ne pouvant contenir de chiffre il y avait
                GetStringSafe(reader, "con_registre_national"),  // une levée d'exception ....
                GetStringSafe(reader, "con_rue"),
                GetStringSafe(reader, "con_cp"),
                GetStringSafe(reader, "con_localite"),
                GetStringSafe(reader, "con_gsm"),
                GetStringSafe(reader, "con_telephone"),
                GetStringSafe(reader, "con_email")
            );

            contact.Identifiant = GetValueOrDefault<int>(reader, "con_contact_id");

            return contact;
        }

        // -------------------------------------------------------
        // Assignation des paramètres SQL pour INSERT et UPDATE
        // p_fa_id est ajouté uniquement s'il n'est pas déjà présent :
        //   - INSERT : AccesDBBase l'ajoute déjà comme OUT → on ne l'ajoute pas
        //   - UPDATE : AccesDBBase n'ajoute rien → on l'ajoute comme IN
        //   - DELETE : AssignerCleSQL est appelée à la place → cette méthode n'est pas appelée
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Famille_Accueil fa)
        {
            if (!cmd.Parameters.Contains(ClesPrimaire) && fa.Identifiant > 0)
                cmd.Parameters.AddWithValue("p_fa_id", fa.Identifiant);

            // Forcer NpgsqlDbType.Date pour éviter que Npgsql envoie un timestamp
            cmd.Parameters.Add("p_date_debut", NpgsqlTypes.NpgsqlDbType.Date).Value
                = fa.Date;

            // Forcer NpgsqlDbType.Date pour p_date_fin également
            cmd.Parameters.Add("p_date_fin", NpgsqlTypes.NpgsqlDbType.Date).Value =
                fa.DateFin.HasValue
                    ? fa.DateFin.Value
                    : DBNull.Value;

            cmd.Parameters.AddWithValue("p_identifiant_animal", fa.AnimalConcerne.Identifiant);
            cmd.Parameters.AddWithValue("p_identifiant_contact", fa.ContactConcerne.Identifiant);
        }

        // -------------------------------------------------------
        // Assignation de la clé primaire uniquement pour DELETE
        // famille_accueil_suppression n'attend que p_fa_id
        // -------------------------------------------------------

        protected override void AssignerCleSQL(NpgsqlCommand cmd, Famille_Accueil fa)
        {
            cmd.Parameters.AddWithValue("p_fa_id", fa.Identifiant);
        }

        // -------------------------------------------------------
        // Liste complète
        // -------------------------------------------------------

        public async Task<List<Famille_Accueil>> AfficherListeFamilles()
        {
            return await ExecuterFonctionRetourListeAsync(AfficherListe);
        }

        // -------------------------------------------------------
        // Un seul élément
        // -------------------------------------------------------

        public async Task<Famille_Accueil?> AfficherUnAsync(int id)
        {
            return await ExecuterFonctionRetourObjetAsync(AfficherUnElement, "p_fa_id", id);
        }

        // -------------------------------------------------------
        // Liste par animal
        // -------------------------------------------------------

        public async Task<List<Famille_Accueil>> AfficherParAnimalAsync(string idAnimal)
        {
            List<Famille_Accueil> retVal = new List<Famille_Accueil>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = $"SELECT * FROM famille_accueil_afficher_par_animal(@p_identifiant_animal)";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@p_identifiant_animal", idAnimal);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                            retVal.Add(ConvertirEnObjet(reader));
                    }
                }
            }

            return retVal;
        }
        // -------------------------------------------------------
        // Override de InsertAsync pour récupérer l'identifiant
        // généré par le SERIAL après l'insertion
        // -------------------------------------------------------

        public override async Task<string?> InsertAsync(Famille_Accueil objet)
        {
            string? retour = await base.InsertAsync(objet);

            // La procédure famille_accueil_insertion retourne p_fa_id via OUT
            // on l'assigne à l'objet pour pouvoir l'utiliser immédiatement pour un UPDATE
            if (retour != null)
                objet.Identifiant = Convert.ToInt32(retour);

            return retour;
        }
    }
}
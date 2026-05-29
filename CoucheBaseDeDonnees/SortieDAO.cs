using System.Data;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class SortieDAO : AccesDBBase<Sortie>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions en DB
        // -------------------------------------------------------

        protected override string AfficherListe => "ani_sortie_afficher_liste_complete";
        protected override string? AfficherUnElement => null;              
        protected override string Insert => "ani_sortie_insertion";
        protected override string? InsertOut => null;              
        protected override string? Update => null;              
        protected override string? UpdateOut => null;              
        protected override string? Delete => null;              
        protected override string? DeleteOut => null;              

        // -------------------------------------------------------
        // Conversion DB → Objet métier
        // -------------------------------------------------------

        protected override Sortie ConvertirEnObjet(IDataReader reader)
        {
            Animal animal = ConstruireAnimal(reader);
            Contact contact = ConstruireContact(reader);

            int id = GetValueOrDefault<int>(reader, "sortie_id");
            DateTime date = GetDateTimeSafe(reader, "date_sortie") ?? DateTime.MinValue;
            string raison = GetStringSafe(reader, "raison");

            return Sortie.Create(id, animal, contact, date, raison);
        }

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
        // Assignation des paramètres SQL
        // Les noms doivent correspondre exactement aux paramètres de la procédure
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Sortie objet)
        {
            cmd.Parameters.AddWithValue("p_raison", objet.Raison);

            // Forcer NpgsqlDbType.Date pour éviter que Npgsql envoie un timestamp
            cmd.Parameters.Add("p_date_sortie", NpgsqlTypes.NpgsqlDbType.Date).Value
                = objet.Date;

            cmd.Parameters.AddWithValue("p_identifiant_animal", objet.AnimalConcerne.Identifiant);
            cmd.Parameters.AddWithValue("p_identifiant_contact", objet.ContactConcerne.Identifiant);
        }

        // -------------------------------------------------------
        // Retourne la liste complète des sorties
        // -------------------------------------------------------

        public async Task<List<Sortie>> AfficherListeSorties()
        {
            return await ExecuterFonctionRetourListeAsync(AfficherListe);
        }

        // -------------------------------------------------------
        // Retourne toutes les sorties liées à un animal
        // -------------------------------------------------------

        public async Task<List<Sortie>> AfficherParAnimalAsync(string idAnimal)
        {
            List<Sortie> retVal = new List<Sortie>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = "SELECT * FROM ani_sortie_afficher_par_animal(@p_identifiant_animal)";

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
        // Retourne la date de la dernière sortie d'un animal
        // Utilisée pour vérifier si un animal est présent au refuge
        // -------------------------------------------------------

        // -------------------------------------------------------
        // Retourne la date de la dernière sortie d'un animal
        // Utilisée pour vérifier si un animal est présent au refuge
        // -------------------------------------------------------

        public async Task<DateTime?> GetSortieMaxAsync(string identifiant)
        {
            DateTime? retVal = null;

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = "SELECT ani_sortie_date_max(@p_identifiant_animal)";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@p_identifiant_animal", identifiant);

                    // On utilise ExecuteReaderAsync plutôt que ExecuteScalarAsync
                    // pour pouvoir utiliser GetDateTimeSafe qui gère déjà DateOnly → DateTime
                    // via ToDateTime(TimeOnly.MinValue) qui fixe la composante heure à minuit (00:00:00)
                    // la date reste intacte — TimeOnly.MinValue ≠ DateTime.MinValue
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            retVal = GetDateTimeSafe(reader, "ani_sortie_date_max");
                    }
                }
            }
            return retVal;
        }
    }
}
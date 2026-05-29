using System.Data;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class AdoptionDAO : AccesDBBase<Adoption>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions en DB
        // -------------------------------------------------------

        protected override string AfficherListe => "adoption_afficher_liste_complete";
        protected override string AfficherUnElement => "adoption_afficher_un";
        protected override string Insert => "adoption_insertion";
        protected override string? InsertOut => "p_adoption_id";           

        protected override string Update => "adoption_modification";
        protected override string? UpdateOut => null;                      

        protected override string Delete => "adoption_suppression";
        protected override string? DeleteOut => null;                      

        protected override string? ClesPrimaire => "p_adoption_id";           // Clé primaire utilisée pour éviter le doublon lors d'un INSERT
        protected override bool DeleteUtiliseCleSeule => true;                  // adoption_suppression n'attend que p_adoption_id
                                                                                // AssignerParametreSQL ne doit pas être appelée pour le DELETE

        // -------------------------------------------------------
        // Conversion DB → Objet métier
        // Les trois fonctions SQL retournent toutes les colonnes nécessaires
        // pour reconstruire un objet Adoption complet avec Animal et Contact
        // -------------------------------------------------------

        protected override Adoption ConvertirEnObjet(IDataReader reader)
        {
            Animal animal = ConstruireAnimal(reader);
            Contact contact = ConstruireContact(reader);

            int id = GetValueOrDefault<int>(reader, "adoption_id");
            string statut = GetStringSafe(reader, "statut");
            DateTime date = GetDateTimeSafe(reader, "date_demande") ?? DateTime.MinValue;

            return Adoption.Create(id, animal, contact, date, statut);
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
        // Assignation des paramètres SQL pour INSERT et UPDATE
        // p_adoption_id est ajouté uniquement s'il n'est pas déjà présent :
        //   - INSERT : AccesDBBase l'ajoute déjà comme OUT → on ne l'ajoute pas
        //   - UPDATE : AccesDBBase n'ajoute rien → on l'ajoute comme IN
        //   - DELETE : AssignerCleSQL est appelée à la place → cette méthode n'est pas appelée
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Adoption objet)
        {
            if (!cmd.Parameters.Contains(ClesPrimaire) && objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("p_adoption_id", objet.Identifiant);

            cmd.Parameters.AddWithValue("p_statut", objet.Statut);

            // Forcer NpgsqlDbType.Date pour éviter que Npgsql envoie un timestamp
            cmd.Parameters.Add("p_date_demande", NpgsqlTypes.NpgsqlDbType.Date).Value
                = objet.Date;

            cmd.Parameters.AddWithValue("p_identifiant_animal", objet.AnimalConcerne.Identifiant);
            cmd.Parameters.AddWithValue("p_identifiant_contact", objet.ContactConcerne.Identifiant);
        }

        // -------------------------------------------------------
        // Assignation de la clé primaire uniquement pour DELETE
        // adoption_suppression n'attend que p_adoption_id
        // -------------------------------------------------------

        protected override void AssignerCleSQL(NpgsqlCommand cmd, Adoption objet)
        {
            cmd.Parameters.AddWithValue("p_adoption_id", objet.Identifiant);
        }

        // -------------------------------------------------------
        // Retourne la liste complète des adoptions
        // -------------------------------------------------------

        public async Task<List<Adoption>> AfficherListeAdoptions()
        {
            return await ExecuterFonctionRetourListeAsync(AfficherListe);
        }

        // -------------------------------------------------------
        // Retourne une adoption par son id
        // -------------------------------------------------------

        public async Task<Adoption?> AfficherUnAsync(int adoptionId)
        {
            return await ExecuterFonctionRetourObjetAsync(AfficherUnElement, "p_adoption_id", adoptionId);
        }

        // -------------------------------------------------------
        // Retourne toutes les adoptions liées à un animal
        // -------------------------------------------------------

        public async Task<List<Adoption>> AfficherParAnimalAsync(string idAnimal)
        {
            List<Adoption> retVal = new List<Adoption>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = $"SELECT * FROM adoption_afficher_par_animal(@p_identifiant_animal)";

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
        // Recherche la dernière demande en cours ou acceptée pour un animal
        // Utilise AfficherParAnimalAsync et filtre en C#
        // car la logique de filtrage appartient à la couche métier
        // adoption_afficher_par_animal retourne déjà trié par date DESC
        // -------------------------------------------------------

        public async Task<Adoption?> RechercheDemandeAcceptee(string idAnimal)
        {
            Adoption? retVal = null;
            List<Adoption> liste = await AfficherParAnimalAsync(idAnimal);

            foreach (Adoption adoption in liste)
            {
                if (adoption.Statut == "demande" || adoption.Statut == "acceptee")
                {
                    retVal = adoption;
                    break;
                }
            }

            return retVal;
        }
        // -------------------------------------------------------
        // Override de InsertAsync pour récupérer l'identifiant
        // généré par le SERIAL après l'insertion
        // -------------------------------------------------------

        public override async Task<string?> InsertAsync(Adoption objet)
        {
            string? retour = await base.InsertAsync(objet);

            // La procédure adoption_insertion retourne p_adoption_id via OUT
            // on l'assigne à l'objet pour pouvoir l'utiliser immédiatement pour un UPDATE ou DELETE
            if (retour != null)
                objet.Identifiant = Convert.ToInt32(retour);

            return retour;
        }
    }
}
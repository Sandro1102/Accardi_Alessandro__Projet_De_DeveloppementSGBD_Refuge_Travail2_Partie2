using System.Data;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class VaccinationDAO : AccesDBBase<Vaccination>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions en DB
        // -------------------------------------------------------

        protected override string AfficherListe => "vaccination_afficher_liste_complete";
        protected override string? AfficherUnElement => null;              
        protected override string Insert => "vaccination_insertion";
        protected override string? InsertOut => null;              
        protected override string? Update => null;              
        protected override string? UpdateOut => null;              
        protected override string Delete => "vaccination_suppression";
        protected override string? DeleteOut => null;              

        // -------------------------------------------------------
        // Conversion DB → Objet métier
        // -------------------------------------------------------

        protected override Vaccination ConvertirEnObjet(IDataReader reader)
        {
            Animal animal = ConstruireAnimal(reader);
            Vaccin vaccin = ConstruireVaccin(reader);

            DateTime date = GetDateTimeSafe(reader, "vaccination_date") ?? DateTime.MinValue;

            return Vaccination.Create(animal, vaccin, date);
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

        private Vaccin ConstruireVaccin(IDataReader reader)
        {
            int id = GetValueOrDefault<int>(reader, "vac_identifiant");
            string nom = GetStringSafe(reader, "vac_nom");

            return Vaccin.Create(id, nom);
        }

        // -------------------------------------------------------
        // Assignation des paramètres SQL
        // Les noms doivent correspondre exactement aux paramètres des procédures
        // vaccination_insertion et vaccination_suppression attendent tous les deux
        // p_date, p_identifiant_animal et p_identifiant_vaccin
        // AssignerParametreSQL convient donc pour les deux opérations
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Vaccination objet)
        {
            // Forcer NpgsqlDbType.Date pour éviter que Npgsql envoie un timestamp
            cmd.Parameters.Add("p_date", NpgsqlTypes.NpgsqlDbType.Date).Value
                = objet.DateVaccination;

            cmd.Parameters.AddWithValue("p_identifiant_animal", objet.AnimalConcerne.Identifiant);
            cmd.Parameters.AddWithValue("p_identifiant_vaccin", objet.VaccinApplique.Identifiant);
        }

        // -------------------------------------------------------
        // Retourne la liste complète des vaccinations
        // -------------------------------------------------------

        public async Task<List<Vaccination>> AfficherListeVaccinations()
        {
            return await ExecuterFonctionRetourListeAsync(AfficherListe);
        }

        // -------------------------------------------------------
        // Retourne toutes les vaccinations d'un animal
        // -------------------------------------------------------

        public async Task<List<Vaccination>> AfficherParAnimalAsync(string idAnimal)
        {
            List<Vaccination> retVal = new List<Vaccination>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = "SELECT * FROM vaccination_afficher_par_animal(@p_identifiant_animal)";

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
    }
}
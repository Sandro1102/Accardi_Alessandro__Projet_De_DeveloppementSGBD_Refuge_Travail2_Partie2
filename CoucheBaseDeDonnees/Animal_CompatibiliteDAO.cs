using System.Data;
using Npgsql;
using static Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees.Animal_CompatibiliteDAO;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class Animal_CompatibiliteDAO : AccesDBBase<AnimalCompatibilite>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions en DB
        // -------------------------------------------------------

        protected override string AfficherListe => "ani_compatibilite_afficher";  
        protected override string? AfficherUnElement => null;                           
        protected override string Insert => "ani_compatibilite_insertion";
        protected override string? InsertOut => null;                           
        protected override string Update => "ani_compatibilite_modification";
        protected override string? UpdateOut => null;                           
        protected override string Delete => "ani_compatibilite_suppression";
        protected override string? DeleteOut => null;

        protected override bool DeleteUtiliseCleSeule => true;  // ani_compatibilite_suppression n'attend que
                                                                // p_identifiant_compatibilite et p_identifiant_animal
                                                                // AssignerParametreSQL ne doit pas être appelée pour le DELETE

        // -------------------------------------------------------
        // Record interne représentant la compatibilité d'un animal
        // -------------------------------------------------------

        internal record AnimalCompatibilite(
            int CompatibiliteId,
            string CompatibiliteType,
            bool Valeur,
            string Description,
            string AnimalId
        );

        // -------------------------------------------------------
        // Conversion DB → Objet
        // -------------------------------------------------------

        protected override AnimalCompatibilite ConvertirEnObjet(IDataReader reader)
        {
            int compId = GetValueOrDefault<int>(reader, "comp_identifiant");
            string compType = GetStringSafe(reader, "comp_type");
            bool valeur = GetValueOrDefault<bool>(reader, "valeur");
            string description = GetStringSafe(reader, "description");
            string aniId = GetStringSafe(reader, "ani_identifiant");

            return new AnimalCompatibilite(compId, compType, valeur, description, aniId);
        }

        // -------------------------------------------------------
        // Assignation des paramètres SQL
        // Les noms doivent correspondre exactement aux paramètres des procédures
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, AnimalCompatibilite obj)
        {
            cmd.Parameters.AddWithValue("p_valeur", obj.Valeur);
            cmd.Parameters.AddWithValue("p_description", (object?)obj.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_identifiant_compatibilite", obj.CompatibiliteId);
            cmd.Parameters.AddWithValue("p_identifiant_animal", obj.AnimalId);
        }

        // -------------------------------------------------------
        // Retourne toutes les compatibilités d'un animal
        // -------------------------------------------------------

        public async Task<List<AnimalCompatibilite>> AfficherParAnimalAsync(string idAnimal)
        {
            List<AnimalCompatibilite> retVal = new List<AnimalCompatibilite>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = $"SELECT * FROM {AfficherListe}(@p_identifiant_animal)";

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
        protected override void AssignerCleSQL(NpgsqlCommand cmd, AnimalCompatibilite obj)
        {
            cmd.Parameters.AddWithValue("p_identifiant_compatibilite", obj.CompatibiliteId);
            cmd.Parameters.AddWithValue("p_identifiant_animal", obj.AnimalId);
        }
    }
}
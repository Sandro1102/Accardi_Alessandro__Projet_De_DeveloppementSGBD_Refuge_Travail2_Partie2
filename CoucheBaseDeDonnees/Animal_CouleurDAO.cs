using System.Data;
using Npgsql;
using static Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees.Animal_CouleurDAO;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class Animal_CouleurDAO : AccesDBBase<AnimalCouleur>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions en DB
        // -------------------------------------------------------

        protected override string AfficherListe => "animal_couleur_afficher"; 
        protected override string? AfficherUnElement => null;                      
        protected override string Insert => "animal_couleur_insertion";
        protected override string? InsertOut => null;                      
        protected override string Update => null;                      
        protected override string? UpdateOut => null;                      
        protected override string Delete => "animal_couleur_suppression";
        protected override string? DeleteOut => null;

        protected override bool DeleteUtiliseCleSeule => true;  // animal_couleur_suppression n'attend que
                                                                // p_identifiant_couleur et p_identifiant_animal
                                                                // AssignerParametreSQL ne doit pas être appelée pour le DELETE

        // -------------------------------------------------------
        // J'avais créé une classe métier qui n'avais pas de réelle utilité afin l'alléger le code l'IA m'a montré que la ligne ci-dessous remplace
        // la classe que j'avais initialement créée dans la couche métier qui me permettait de lancer le new plus bas dans le code.
        // Cette ligne remplace le code suivant :
        // internal class AnimalCouleur
        // {
        //     public string AniIdentifiant { get; set; }
        //     public int CouleurId { get; set; }
        // }
        // Rappel : un constructeur existe dans les classes même lorsqu'il n'est pas écrit. Exemple : quand j'écris var dao = new VaccinationDAO();
        // Il n'y a aucun constructeur dans cette classe et pourtant il est possible d'écrire la ligne
        // -------------------------------------------------------

        internal record AnimalCouleur(
            int CouleurId,
            string NomCouleur,
            string AniIdentifiant
        );

        // -------------------------------------------------------
        // Conversion DB → Objet
        // -------------------------------------------------------

        protected override AnimalCouleur ConvertirEnObjet(IDataReader reader)
        {
            int couleurId = GetValueOrDefault<int>(reader, "col_identifiant");
            string nomCouleur = GetStringSafe(reader, "nom_couleur");
            string animalId = GetStringSafe(reader, "ani_identifiant");

            return new AnimalCouleur(couleurId, nomCouleur, animalId);
        }

        // -------------------------------------------------------
        // Assignation des paramètres SQL
        // Les noms doivent correspondre exactement aux paramètres des procédures
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, AnimalCouleur objet)
        {
            cmd.Parameters.AddWithValue("p_identifiant_couleur", objet.CouleurId);
            cmd.Parameters.AddWithValue("p_identifiant_animal", objet.AniIdentifiant);
        }

        // -------------------------------------------------------
        // Retourne toutes les couleurs d'un animal
        // -------------------------------------------------------

        public async Task<List<AnimalCouleur>> AfficherParAnimalAsync(string idAnimal)
        {
            List<AnimalCouleur> retVal = new List<AnimalCouleur>();

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
        protected override void AssignerCleSQL(NpgsqlCommand cmd, AnimalCouleur obj)
        {
            cmd.Parameters.AddWithValue("p_identifiant_couleur", obj.CouleurId);
            cmd.Parameters.AddWithValue("p_identifiant_animal", obj.AniIdentifiant);
        }
    }
}
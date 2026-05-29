using System.Data;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class CouleurDAO : AccesDBBase<Couleur>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions en DB
        // -------------------------------------------------------

        protected override string AfficherListe => "couleur_obtenir"; 
        protected override string? AfficherUnElement => null;              
        protected override string? Insert => null;              
        protected override string? InsertOut => null;              
        protected override string? Update => null;              
        protected override string? UpdateOut => null;              
        protected override string? Delete => null;              
        protected override string? DeleteOut => null;              

        // -------------------------------------------------------
        // Conversion DB → Objet métier
        // La fonction couleur_obtenir retourne id et nom
        // -------------------------------------------------------

        protected override Couleur ConvertirEnObjet(IDataReader reader)
        {
            int id = GetValueOrDefault<int>(reader, "id");
            string nom = GetStringSafe(reader, "nom");

            return Couleur.Create(id, nom);
        }

        // -------------------------------------------------------
        // Assignation des paramètres SQL
        // Le IF ci-dessous devrait permettre d'injecter dans la DB par erreur l'id zéro lors d'un insert,
        // même si celui-ci ne figure pas dans les paramètres de la requête SQL.
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Couleur objet)
        {
            cmd.Parameters.AddWithValue("@nom", objet.Nom);

            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@id", objet.Identifiant);
        }

        // -------------------------------------------------------
        // Retourne la liste complète des couleurs
        // -------------------------------------------------------

        public async Task<List<Couleur>> AfficherListeCouleurs()
        {
            return await ExecuterFonctionRetourListeAsync(AfficherListe);
        }
    }
}
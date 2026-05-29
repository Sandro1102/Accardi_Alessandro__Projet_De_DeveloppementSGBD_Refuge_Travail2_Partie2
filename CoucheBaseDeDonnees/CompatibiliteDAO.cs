using System.Data;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class CompatibiliteDAO : AccesDBBase<Compatibilite>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions en DB
        // -------------------------------------------------------

        protected override string AfficherListe => "compatibilite_obtenir"; 
        protected override string? AfficherUnElement => null;                    
        protected override string Insert => "compatibilite";         
        protected override string? InsertOut => null;                    
        protected override string Update => "compatibilite";         
        protected override string? UpdateOut => null;                    
        protected override string Delete => "compatibilite";         
        protected override string? DeleteOut => null;                    

        // -------------------------------------------------------
        // Conversion DB → Objet métier
        // La fonction compatibilite_obtenir retourne id et nom
        // -------------------------------------------------------

        protected override Compatibilite ConvertirEnObjet(IDataReader reader)
        {
            int identifiant = GetValueOrDefault<int>(reader, "id");
            string nom = GetStringSafe(reader, "nom");

            return Compatibilite.Create(identifiant, nom);
        }

        // -------------------------------------------------------
        // Assignation des paramètres SQL
        // Le IF ci-dessous devrait permettre d'injecter dans la DB par erreur l'id zéro lors d'un insert,
        // même si celui-ci ne figure pas dans les paramètres de la requête SQL.
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Compatibilite objet)
        {
            cmd.Parameters.AddWithValue("@type", objet.Type);

            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@identifiant", objet.Identifiant);
        }

        // -------------------------------------------------------
        // Retourne la liste complète des compatibilités
        // -------------------------------------------------------

        public async Task<List<Compatibilite>> AfficherListeCompatibilites()
        {
            return await ExecuterFonctionRetourListeAsync(AfficherListe);
        }
    }
}
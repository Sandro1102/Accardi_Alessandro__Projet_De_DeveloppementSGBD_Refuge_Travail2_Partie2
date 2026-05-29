using System.Data;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class VaccinDAO : AccesDBBase<Vaccin>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions en DB
        // -------------------------------------------------------

        protected override string AfficherListe => "vaccin_obtenir"; 
        protected override string? AfficherUnElement => null;             
        protected override string? Insert => null;             
        protected override string? InsertOut => null;             
        protected override string? Update => null;             
        protected override string? UpdateOut => null;             
        protected override string? Delete => null;             
        protected override string? DeleteOut => null;             

        // -------------------------------------------------------
        // Conversion DB → Objet métier
        // La fonction vaccin_obtenir retourne id et nom
        // -------------------------------------------------------

        protected override Vaccin ConvertirEnObjet(IDataReader reader)
        {
            int id = GetValueOrDefault<int>(reader, "id");
            string nom = GetStringSafe(reader, "nom");

            return Vaccin.Create(id, nom);
        }

        // -------------------------------------------------------
        // Assignation des paramètres SQL
        // Le IF ci-dessous devrait permettre d'injecter dans la DB par erreur l'id zéro lors d'un insert,
        // même si celui-ci ne figure pas dans les paramètres de la requête SQL.
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Vaccin objet)
        {
            cmd.Parameters.AddWithValue("@nom", objet.Nom);

            if (objet.Identifiant > 0)
                cmd.Parameters.AddWithValue("@id", objet.Identifiant);
        }

        // -------------------------------------------------------
        // Retourne la liste complète des vaccins
        // -------------------------------------------------------

        public async Task<List<Vaccin>> AfficherListeVaccins()
        {
            return await ExecuterFonctionRetourListeAsync(AfficherListe);
        }
    }
}
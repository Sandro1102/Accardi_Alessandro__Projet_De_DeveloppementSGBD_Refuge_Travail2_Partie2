using System.Data;
using Npgsql;
using static Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees.Personne_RoleDAO;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class Personne_RoleDAO : AccesDBBase<PersonneRole>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions en DB
        // -------------------------------------------------------

        protected override string AfficherListe => "personne_role_afficher"; 
        protected override string? AfficherUnElement => null;                     
        protected override string Insert => "personne_role_insertion";
        protected override string? InsertOut => null;                     
        protected override string? Update => null;                     
        protected override string? UpdateOut => null;                     
        protected override string Delete => "personne_role_suppression";
        protected override string? DeleteOut => null;                     

        // -------------------------------------------------------
        // Petit record interne pour représenter la relation
        // -------------------------------------------------------

        internal record PersonneRole(
            int PersonneId,
            int RoleId,
            string RoleNom
        );

        // -------------------------------------------------------
        // Conversion DB → Objet
        // -------------------------------------------------------

        protected override PersonneRole ConvertirEnObjet(IDataReader reader)
        {
            int persId = GetValueOrDefault<int>(reader, "pers_identifiant");
            int roleId = GetValueOrDefault<int>(reader, "rol_identifiant");
            string roleNom = GetStringSafe(reader, "rol_nom");

            return new PersonneRole(persId, roleId, roleNom);
        }

        // -------------------------------------------------------
        // Assignation des paramètres SQL
        // Les noms doivent correspondre exactement aux paramètres des procédures
        // Pour INSERT et DELETE : personne_role_insertion et personne_role_suppression
        // attendent tous les deux p_identifiant_contact et p_identifiant_role
        // AssignerParametreSQL convient donc pour les deux opérations
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, PersonneRole obj)
        {
            cmd.Parameters.AddWithValue("p_identifiant_contact", obj.PersonneId);
            cmd.Parameters.AddWithValue("p_identifiant_role", obj.RoleId);
        }

        // -------------------------------------------------------
        // Retourne tous les rôles d'un contact
        // -------------------------------------------------------

        public async Task<List<PersonneRole>> AfficherParContactAsync(int idContact)
        {
            List<PersonneRole> retVal = new List<PersonneRole>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = $"SELECT * FROM {AfficherListe}(@p_identifiant_contact)";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@p_identifiant_contact", idContact);

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
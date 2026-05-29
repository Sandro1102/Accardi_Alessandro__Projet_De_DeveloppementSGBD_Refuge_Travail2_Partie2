using System.Data;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Npgsql;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal class ContactDAO : AccesDBBase<Contact>
    {
        // -------------------------------------------------------
        // Noms des procédures et fonctions en DB
        // -------------------------------------------------------

        protected override string AfficherListe => "contact_afficher_liste_complete";
        protected override string AfficherUnElement => "contact_afficher_un";
        protected override string Insert => "contact_insertion";
        protected override string? InsertOut => "p_contact_identifiant";  
        protected override string Update => "contact_modification";
        protected override string? UpdateOut => null;                     

        protected override string Delete => "contact_suppression";
        protected override string? DeleteOut => null;                     

        protected override bool DeleteUtiliseCleSeule => true;                    // contact_suppression n'attend que p_registre_national
                                                                                  // AssignerParametreSQL ne doit pas être appelée pour le DELETE

        // -------------------------------------------------------
        // Conversion DB → Objet métier (MAPPING)
        // -------------------------------------------------------

        protected override Contact ConvertirEnObjet(IDataReader reader)
        {
            string nom = GetStringSafe(reader, "nom");
            string prenom = GetStringSafe(reader, "prenom");
            string registreNational = GetStringSafe(reader, "registre_national");

            string rue = GetStringSafe(reader, "rue");
            string cp = GetStringSafe(reader, "cp");
            string localite = GetStringSafe(reader, "localite");

            string gsm = GetStringSafe(reader, "gsm");
            string telephoneFixe = GetStringSafe(reader, "telephone");
            string email = GetStringSafe(reader, "email");

            Contact contact = Contact.Create(nom, prenom, registreNational, rue, cp, localite, gsm, telephoneFixe, email);

            contact.Identifiant = GetValueOrDefault<int>(reader, "contact_identifiant");

            return contact;
        }

        // -------------------------------------------------------
        // Assignation des paramètres SQL pour INSERT et UPDATE
        // Les noms doivent correspondre exactement aux paramètres des procédures
        // -------------------------------------------------------

        protected override void AssignerParametreSQL(NpgsqlCommand cmd, Contact objet)
        {
            cmd.Parameters.AddWithValue("p_nom", objet.Nom);
            cmd.Parameters.AddWithValue("p_prenom", objet.Prenom);
            cmd.Parameters.AddWithValue("p_registre_national", objet.RegistreNational);

            cmd.Parameters.AddWithValue("p_rue", objet.Rue);
            cmd.Parameters.AddWithValue("p_cp", objet.Cp);
            cmd.Parameters.AddWithValue("p_localite", objet.Localite);

            cmd.Parameters.AddWithValue("p_gsm", string.IsNullOrWhiteSpace(objet.Gsm) ? DBNull.Value : objet.Gsm);
            cmd.Parameters.AddWithValue("p_telephone", string.IsNullOrWhiteSpace(objet.Telephone) ? DBNull.Value : objet.Telephone);
            cmd.Parameters.AddWithValue("p_email", string.IsNullOrWhiteSpace(objet.Email) ? DBNull.Value : objet.Email);
        }

        // -------------------------------------------------------
        // Assignation de la clé primaire uniquement pour DELETE
        // contact_suppression n'attend que p_registre_national
        // -------------------------------------------------------

        protected override void AssignerCleSQL(NpgsqlCommand cmd, Contact objet)
        {
            cmd.Parameters.AddWithValue("p_registre_national", objet.RegistreNational);
        }

        // -------------------------------------------------------
        // Retourne la liste complète des contacts
        // -------------------------------------------------------

        public async Task<List<Contact>> AfficherListeContacts()
        {
            return await ExecuterFonctionRetourListeAsync(AfficherListe);
        }

        // -------------------------------------------------------
        // Retourne un contact par son registre national
        // -------------------------------------------------------

        public async Task<Contact?> AfficherUnAsync(string registreNational)
        {
            return await ExecuterFonctionRetourObjetAsync(AfficherUnElement, "p_registre_national", registreNational);
        }

        // -------------------------------------------------------
        // Override de InsertAsync pour récupérer l'identifiant
        // généré par le SERIAL après l'insertion
        // -------------------------------------------------------

        public override async Task<string?> InsertAsync(Contact objet)
        {
            string? retour = await base.InsertAsync(objet);

            // La procédure contact_insertion retourne p_contact_identifiant via OUT
            // on l'assigne à l'objet Contact pour pouvoir l'utiliser immédiatement
            if (retour != null)
                objet.Identifiant = Convert.ToInt32(retour);

            return retour;
        }
    }
}
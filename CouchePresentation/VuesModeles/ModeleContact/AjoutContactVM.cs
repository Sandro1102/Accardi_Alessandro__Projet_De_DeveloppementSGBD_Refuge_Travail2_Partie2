using System.Collections.ObjectModel;
using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.GestionContact
{
    public class AjoutContactVM : INotifyPropertyChanged
    {
        // -------------------------------------------------------
        // INotifyPropertyChanged
        // -------------------------------------------------------

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // -------------------------------------------------------
        // DAO
        // -------------------------------------------------------

        private ContactDAO _contactDAO = new ContactDAO();
        private RoleDAO _roleDAO = new RoleDAO();
        private Personne_RoleDAO _personneRoleDAO = new Personne_RoleDAO();

        // -------------------------------------------------------
        // Listes
        // -------------------------------------------------------

        private ObservableCollection<RoleSelectionnable> _roles;
        public ObservableCollection<RoleSelectionnable> Roles
        {
            get { return this._roles; }
            set { this._roles = value; OnPropertyChanged("Roles"); }
        }

        // -------------------------------------------------------
        // Propriété pour pré-remplir le registre national
        // -------------------------------------------------------

        private DateTime? _dateDeNaissance;
        public DateTime? DateDeNaissance
        {
            get { return this._dateDeNaissance; }
            set
            {
                this._dateDeNaissance = value;
                OnPropertyChanged("DateDeNaissance");
                OnPropertyChanged("DebutRegistreNational");
            }
        }

        // Retourne les 6 premiers chiffres du registre national
        // ex : né le 11/02/1988 → "88.02.11-"
        public string DebutRegistreNational
        {
            get
            {
                if (!DateDeNaissance.HasValue)
                    return string.Empty;

                DateTime d = DateDeNaissance.Value;
                return $"{d.Year % 100:D2}.{d.Month:D2}.{d.Day:D2}-";
            }
        }

        // -------------------------------------------------------
        // Constructeur
        // -------------------------------------------------------

        public AjoutContactVM()
        {
            Roles = new ObservableCollection<RoleSelectionnable>();
        }

        // -------------------------------------------------------
        // Chargement des listes depuis la DB
        // -------------------------------------------------------

        public async Task ChargerDonnees()
        {
            List<Role> roles = await _roleDAO.AfficherListeRoles();
            Roles.Clear();
            foreach (Role r in roles)
                Roles.Add(new RoleSelectionnable { Role = r });
        }

        // -------------------------------------------------------
        // Enregistrement complet d'un contact
        // -------------------------------------------------------

        public async Task EnregistrerContact(CoucheMetier.Contact contact)
        {
            // 1 - Insérer le contact
            await _contactDAO.InsertAsync(contact);

            // 2 - Insérer les rôles sélectionnés
            foreach (RoleSelectionnable r in Roles.Where(x => x.Selectionnee))
            {
                var personneRole = new Personne_RoleDAO.PersonneRole(
                    contact.Identifiant,
                    r.Role.Identifiant,
                    r.Role.Nom);
                await _personneRoleDAO.InsertAsync(personneRole);
            }
        }
    }
}
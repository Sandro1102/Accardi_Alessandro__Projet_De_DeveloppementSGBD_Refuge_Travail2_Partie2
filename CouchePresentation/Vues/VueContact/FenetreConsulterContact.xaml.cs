using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreConsulterContact : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private CoucheMetier.Contact _contact;
        private Personne_RoleDAO _roleDAO = new Personne_RoleDAO();

        public CoucheMetier.Contact Contact => _contact;

        private ObservableCollection<string> _roles;
        public ObservableCollection<string> Roles
        {
            get { return this._roles; }
            set { this._roles = value; OnPropertyChanged("Roles"); }
        }

        public FenetreConsulterContact(CoucheMetier.Contact contact)
        {
            InitializeComponent();
            _contact = contact;
            Roles = new ObservableCollection<string>();
            this.DataContext = this;
            ChargerDonnees();
        }

        private async void ChargerDonnees()
        {
            var roles = await _roleDAO.AfficherParContactAsync(_contact.Identifiant);
            Roles.Clear();
            foreach (var r in roles)
                Roles.Add(r.RoleNom);
        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.ViderZoneContenu();
        }
    }
}
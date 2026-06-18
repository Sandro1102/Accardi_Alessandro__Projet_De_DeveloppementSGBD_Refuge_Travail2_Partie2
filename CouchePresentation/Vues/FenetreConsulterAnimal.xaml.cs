using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreConsulterAnimal : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private CoucheMetier.Animal _animal;
        private Animal_CouleurDAO _couleurDAO = new Animal_CouleurDAO();
        private Animal_CompatibiliteDAO _compatibiliteDAO = new Animal_CompatibiliteDAO();

        public CoucheMetier.Animal Animal => _animal;

        private ObservableCollection<string> _couleurs;
        public ObservableCollection<string> Couleurs
        {
            get { return this._couleurs; }
            set { this._couleurs = value; OnPropertyChanged("Couleurs"); }
        }

        private ObservableCollection<string> _compatibilites;
        public ObservableCollection<string> Compatibilites
        {
            get { return this._compatibilites; }
            set { this._compatibilites = value; OnPropertyChanged("Compatibilites"); }
        }

        public FenetreConsulterAnimal(CoucheMetier.Animal animal)
        {
            InitializeComponent();
            _animal = animal;
            Couleurs = new ObservableCollection<string>();
            Compatibilites = new ObservableCollection<string>();
            this.DataContext = this;
            ChargerDonnees();
        }

        private async void ChargerDonnees()
        {
            // Chargement des couleurs
            var couleurs = await _couleurDAO.AfficherParAnimalAsync(_animal.Identifiant);
            Couleurs.Clear();
            foreach (var c in couleurs)
                Couleurs.Add(c.NomCouleur);

            // Chargement des compatibilités
            var compatibilites = await _compatibiliteDAO.AfficherParAnimalAsync(_animal.Identifiant);
            Compatibilites.Clear();
            foreach (var c in compatibilites)
                Compatibilites.Add(c.Valeur ? $"{c.CompatibiliteType} oui" : $"{c.CompatibiliteType} non");
        }
        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.ViderZoneContenu();
        }
    }
}
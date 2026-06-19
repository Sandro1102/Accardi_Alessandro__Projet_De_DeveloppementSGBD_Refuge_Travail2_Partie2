using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreListerFamillesParAnimal : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private CoucheMetier.Animal _animal;
        private Famille_AccueilDAO _faDAO = new Famille_AccueilDAO();

        public CoucheMetier.Animal Animal => _animal;

        private ObservableCollection<Famille_Accueil> _familles;
        public ObservableCollection<Famille_Accueil> Familles
        {
            get { return this._familles; }
            set { this._familles = value; OnPropertyChanged("Familles"); }
        }

        public FenetreListerFamillesParAnimal(CoucheMetier.Animal animal)
        {
            InitializeComponent();
            _animal = animal;
            Familles = new ObservableCollection<Famille_Accueil>();
            this.DataContext = this;
            ChargerDonnees();
        }

        private async void ChargerDonnees()
        {
            List<Famille_Accueil> liste = await _faDAO.AfficherParAnimalAsync(_animal.Identifiant);
            Familles.Clear();
            foreach (Famille_Accueil f in liste)
                Familles.Add(f);
        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.ViderZoneContenu();
        }
    }
}
using System.Windows;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleFamilleAccueil;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreAjoutFamilleAccueil : Window
    {
        private AjoutFamilleAccueilVM _vm;

        public FenetreAjoutFamilleAccueil(CoucheMetier.Animal animal)
        {
            InitializeComponent();
            _vm = new AjoutFamilleAccueilVM(animal);
            this.DataContext = _vm;
            ChargerDonnees();
        }

        private async void ChargerDonnees()
        {
            await _vm.ChargerDonnees();
        }

        private async void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DgFamilles.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez sélectionner une famille d'accueil.");
                    return;
                }

                _vm.ContactSelectionne = (CoucheMetier.Contact)DgFamilles.SelectedItem;

                await _vm.EnregistrerFamilleAccueil();
                MessageBox.Show("Famille d'accueil enregistrée avec succès.");
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.ViderZoneContenu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.ViderZoneContenu();
        }
    }
}
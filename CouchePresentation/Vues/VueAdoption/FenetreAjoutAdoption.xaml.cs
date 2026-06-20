using System.Windows;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleAdoption;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreAjoutAdoption : Window
    {
        private AjoutAdoptionVM _vm;

        public FenetreAjoutAdoption(CoucheMetier.Animal animal)
        {
            InitializeComponent();
            _vm = new AjoutAdoptionVM(animal);
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
                if (DgContacts.SelectedItem == null)
                {
                    MessageBox.Show("Veuillez sélectionner un adoptant.");
                    return;
                }

                _vm.ContactSelectionne = (CoucheMetier.Contact)DgContacts.SelectedItem;

                await _vm.EnregistrerAdoption();
                MessageBox.Show("Adoption enregistrée avec succès.");
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
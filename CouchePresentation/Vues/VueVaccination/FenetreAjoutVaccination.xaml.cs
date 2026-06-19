using System.Windows;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleVaccination;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreAjoutVaccination : Window
    {
        private AjoutVaccinationVM _vm;

        public FenetreAjoutVaccination(CoucheMetier.Animal animal)
        {
            InitializeComponent();
            _vm = new AjoutVaccinationVM(animal);
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
                await _vm.EnregistrerVaccinations();
                MessageBox.Show("Vaccinations enregistrées avec succès.");
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
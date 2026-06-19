using System.Windows;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleVaccination;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreSupprimerVaccination : Window
    {
        private SupprimerVaccinationVM _vm;

        public FenetreSupprimerVaccination(CoucheMetier.Animal animal)
        {
            InitializeComponent();
            _vm = new SupprimerVaccinationVM(animal);
            this.DataContext = _vm;
            ChargerDonnees();
        }

        private async void ChargerDonnees()
        {
            await _vm.ChargerDonnees();
        }

        private async void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LbVaccinations.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Veuillez sélectionner au moins un vaccin à supprimer.");
                    return;
                }

                var aSupprimer = LbVaccinations.SelectedItems.Cast<Vaccination>().ToList();

                foreach (Vaccination v in aSupprimer)
                    await _vm.SupprimerVaccination(v);

                MessageBox.Show("Vaccin(s) supprimé(s) avec succès.");
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
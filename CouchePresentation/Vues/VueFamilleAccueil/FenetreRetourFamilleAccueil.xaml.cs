using System.Windows;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleFamilleAccueil;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreRetourFamilleAccueil : Window
    {
        private RetourFamilleAccueilVM _vm;

        public FenetreRetourFamilleAccueil(CoucheMetier.Animal animal)
        {
            InitializeComponent();
            _vm = new RetourFamilleAccueilVM(animal);
            this.DataContext = _vm;
            ChargerDonnees();
        }

        private async void ChargerDonnees()
        {
            await _vm.ChargerDonnees();
        }

        private async void BtnConfirmer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _vm.ConfirmerRetour();
                MessageBox.Show("Retour confirmé avec succès.");
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
using System.Windows;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleAdoption;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreModifierAdoption : Window
    {
        private ModifierAdoptionVM _vm;

        public FenetreModifierAdoption(Adoption adoption)
        {
            InitializeComponent();
            _vm = new ModifierAdoptionVM(adoption);
            this.DataContext = _vm;
        }

        private async void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _vm.ModifierStatut();
                MessageBox.Show("Statut modifié avec succès.");
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
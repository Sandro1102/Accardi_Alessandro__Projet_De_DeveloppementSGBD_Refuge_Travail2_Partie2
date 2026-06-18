using System.Windows;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.Animal;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreModifierAnimal : Window
    {
        private ModifierAnimalVM _vm;

        public FenetreModifierAnimal(CoucheMetier.Animal animal)
        {
            InitializeComponent();
            _vm = new ModifierAnimalVM(animal);
            this.DataContext = _vm;
        }

        private async void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _vm.ModifierAnimal();
                MessageBox.Show("Animal modifié avec succès.");
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
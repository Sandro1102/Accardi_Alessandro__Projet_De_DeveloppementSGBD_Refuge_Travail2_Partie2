using System.Windows;
using Accardi_Alessandro_Refuge_WPF.VuesModèles;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class MainWindow : Window
    {
        private AnimalVM _vm = new AnimalVM();

        public MainWindow()
        {
            InitializeComponent();
        }

        public void ViderZoneContenu()
        {
            ZoneContenu.Content = null;
            ZoneContenu.DataContext = null;
        }

        private void AjouterAnimal_Click(object sender, RoutedEventArgs e)
        {
            dgAnimaux.Visibility = Visibility.Collapsed;
            BtnRetour.Visibility = Visibility.Collapsed;

            FenetreAjoutAnimal formulaire = new FenetreAjoutAnimal();
            ZoneContenu.Content = formulaire.Content;
            ZoneContenu.DataContext = formulaire.DataContext;
        }

        private async void ListerAnimaux_Click(object sender, RoutedEventArgs e)
        {
            ZoneContenu.Content = null;
            ZoneContenu.DataContext = null;

            await _vm.ChargerAnimaux();
            dgAnimaux.ItemsSource = _vm.Animaux;
            dgAnimaux.Visibility = Visibility.Visible;
            BtnRetour.Visibility = Visibility.Visible;
        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            dgAnimaux.Visibility = Visibility.Collapsed;
            BtnRetour.Visibility = Visibility.Collapsed;
        }
    }
}
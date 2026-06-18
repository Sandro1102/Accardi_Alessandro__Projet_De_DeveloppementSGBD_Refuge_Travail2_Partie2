using System.Windows;
using Accardi_Alessandro_Refuge_WPF.VuesModèles;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.Animal;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class MainWindow : Window
    {
        private AnimalVM _vm = new AnimalVM();
        private string _modeActuel = "consulter";

        public MainWindow()
        {
            InitializeComponent();
        }

        public void ViderZoneContenu()
        {
            ZoneContenu.Content = null;
            ZoneContenu.DataContext = null;
            dgAnimaux.Visibility = Visibility.Collapsed;
            BtnRetour.Visibility = Visibility.Collapsed;
        }

        private void AjouterAnimal_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "consulter";
            dgAnimaux.Visibility = Visibility.Collapsed;
            BtnRetour.Visibility = Visibility.Collapsed;

            FenetreAjoutAnimal formulaire = new FenetreAjoutAnimal();
            ZoneContenu.Content = formulaire.Content;
            ZoneContenu.DataContext = formulaire.DataContext;
        }

        private void ModifierAnimal_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "modifier";
            ChargerListe();
        }

        private void SupprimerAnimal_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "supprimer";
            ChargerListe();
        }

        private async void ListerAnimaux_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "consulter";
            ChargerListe();
        }

        private async void ChargerListe()
        {
            ZoneContenu.Content = null;
            ZoneContenu.DataContext = null;

            await _vm.ChargerAnimaux();
            dgAnimaux.ItemsSource = _vm.Animaux;
            dgAnimaux.Visibility = Visibility.Visible;
            BtnRetour.Visibility = Visibility.Visible;
        }

        private async void DgAnimaux_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgAnimaux.SelectedItem == null) return;

            CoucheMetier.Animal animal = (CoucheMetier.Animal)dgAnimaux.SelectedItem;

            if (_modeActuel == "supprimer")
            {
                MessageBoxResult confirmation = MessageBox.Show(
                    $"Voulez-vous vraiment supprimer {animal.Nom} ?",
                    "Confirmation",
                    MessageBoxButton.YesNo);

                if (confirmation == MessageBoxResult.Yes)
                {
                    await _vm.SupprimerAnimal(animal);
                    MessageBox.Show("Animal supprimé avec succès.");
                    await _vm.ChargerAnimaux();
                    dgAnimaux.ItemsSource = _vm.Animaux;
                }
                _modeActuel = "consulter";
            }
            else if (_modeActuel == "modifier")
            {
                dgAnimaux.Visibility = Visibility.Collapsed;
                BtnRetour.Visibility = Visibility.Collapsed;

                FenetreModifierAnimal formulaire = new FenetreModifierAnimal(animal);
                ZoneContenu.Content = formulaire.Content;
                ZoneContenu.DataContext = formulaire.DataContext;
                _modeActuel = "consulter";
            }
            else if (_modeActuel == "consulter")
            {
                dgAnimaux.Visibility = Visibility.Collapsed;
                BtnRetour.Visibility = Visibility.Collapsed;

                FenetreConsulterAnimal formulaire = new FenetreConsulterAnimal(animal);
                ZoneContenu.Content = formulaire.Content;
                ZoneContenu.DataContext = formulaire.DataContext;
            }
        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "consulter";
            dgAnimaux.Visibility = Visibility.Collapsed;
            BtnRetour.Visibility = Visibility.Collapsed;
        }
    }
}
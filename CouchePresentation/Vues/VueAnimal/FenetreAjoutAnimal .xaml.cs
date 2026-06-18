using System.Windows;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.Animal;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreAjoutAnimal : Window
    {
        private AjoutAnimalVM _vm = new AjoutAnimalVM();

        public FenetreAjoutAnimal()
        {
            InitializeComponent();
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
                CoucheMetier.Animal animal = CoucheMetier.Animal.Create(
                    TbNom.Text,
                    CbType.SelectedItem?.ToString(),
                    CbSexe.SelectedItem?.ToString(),
                    CbSterilise.SelectedItem?.ToString(),
                    TbParticularites.Text,
                    TbDescription.Text,
                    DpDateNaissance.SelectedDate ?? DateTime.Today,
                    null,
                    DpDateSterilisation.SelectedDate
                );

                foreach (var couleur in LbCouleurs.SelectedItems)
                    _vm.CouleursSelectionnees.Add((Couleur)couleur);

                await _vm.EnregistrerAnimal(animal, CbRaison.SelectedItem?.ToString());

                MessageBox.Show("Animal enregistré avec succès.");
                this.Close();
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
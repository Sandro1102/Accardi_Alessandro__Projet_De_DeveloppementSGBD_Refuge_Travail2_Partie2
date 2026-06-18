using System.Windows;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleContact;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreModifierContact : Window
    {
        private ModifierContactVM _vm;

        public FenetreModifierContact(CoucheMetier.Contact contact)
        {
            InitializeComponent();
            _vm = new ModifierContactVM(contact);
            this.DataContext = _vm;
        }

        private async void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _vm.ModifierContact();
                MessageBox.Show("Contact modifié avec succès.");
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
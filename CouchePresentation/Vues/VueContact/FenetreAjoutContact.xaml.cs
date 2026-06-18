using System.Windows;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.GestionContact;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class FenetreAjoutContact : Window
    {
        private AjoutContactVM _vm = new AjoutContactVM();

        public FenetreAjoutContact()
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
                string registreNational = _vm.DebutRegistreNational + TbFinRegistreNational.Text;

                CoucheMetier.Contact contact = CoucheMetier.Contact.Create(
                    TbNom.Text,
                    TbPrenom.Text,
                    registreNational,
                    TbRue.Text,
                    TbCp.Text,
                    TbLocalite.Text,
                    TbGsm.Text,
                    TbTelephone.Text,
                    TbEmail.Text
                );

                await _vm.EnregistrerContact(contact);
                MessageBox.Show("Contact enregistré avec succès.");
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
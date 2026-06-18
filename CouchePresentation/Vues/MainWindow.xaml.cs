using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Accardi_Alessandro_Refuge_WPF.VuesModèles;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.Animal;
using Accardi_Alessandro_Refuge_WPF.VuesModèles.GestionContact;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class MainWindow : Window
    {
        private AnimalVM _vmAnimal = new AnimalVM();
        private ContactVM _vmContact = new ContactVM();

        private string _modeActuel = "consulter";
        private string _entiteActuelle = "animal";

        public MainWindow()
        {
            InitializeComponent();
        }

        // -------------------------------------------------------
        // Utilitaires
        // -------------------------------------------------------

        public void ViderZoneContenu()
        {
            ZoneContenu.Content = null;
            ZoneContenu.DataContext = null;
            dgListe.Visibility = Visibility.Collapsed;
            BtnRetour.Visibility = Visibility.Collapsed;
        }

        private void ConfigurerColonnesAnimaux()
        {
            dgListe.Columns.Clear();
            dgListe.Columns.Add(new DataGridTextColumn { Header = "Identifiant", Binding = new Binding("Identifiant"), Width = 120 });
            dgListe.Columns.Add(new DataGridTextColumn { Header = "Nom", Binding = new Binding("Nom"), Width = 120 });
            dgListe.Columns.Add(new DataGridTextColumn { Header = "Type", Binding = new Binding("Type"), Width = 80 });
            dgListe.Columns.Add(new DataGridTextColumn { Header = "Sexe", Binding = new Binding("Sexe"), Width = 60 });
            dgListe.Columns.Add(new DataGridTextColumn { Header = "Stérilisé", Binding = new Binding("Sterilise"), Width = 80 });
            dgListe.Columns.Add(new DataGridTextColumn { Header = "Naissance", Binding = new Binding("DateDeNaissance") { StringFormat = "dd/MM/yyyy" }, Width = 100 });
        }

        private void ConfigurerColonnesContacts()
        {
            dgListe.Columns.Clear();
            dgListe.Columns.Add(new DataGridTextColumn { Header = "Nom", Binding = new Binding("Nom"), Width = 120 });
            dgListe.Columns.Add(new DataGridTextColumn { Header = "Prénom", Binding = new Binding("Prenom"), Width = 120 });
            dgListe.Columns.Add(new DataGridTextColumn { Header = "Registre national", Binding = new Binding("RegistreNational"), Width = 150 });
            dgListe.Columns.Add(new DataGridTextColumn { Header = "GSM", Binding = new Binding("Gsm"), Width = 110 });
            dgListe.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new Binding("Email"), Width = 150 });
        }

        private async void ChargerListeAnimaux()
        {
            _entiteActuelle = "animal";
            ZoneContenu.Content = null;
            ZoneContenu.DataContext = null;

            ConfigurerColonnesAnimaux();
            await _vmAnimal.ChargerAnimaux();
            dgListe.ItemsSource = _vmAnimal.Animaux;
            dgListe.Visibility = Visibility.Visible;
            BtnRetour.Visibility = Visibility.Visible;
        }

        private async void ChargerListeContacts()
        {
            _entiteActuelle = "contact";
            ZoneContenu.Content = null;
            ZoneContenu.DataContext = null;

            ConfigurerColonnesContacts();
            await _vmContact.ChargerContacts();
            dgListe.ItemsSource = _vmContact.Contacts;
            dgListe.Visibility = Visibility.Visible;
            BtnRetour.Visibility = Visibility.Visible;
        }

        // -------------------------------------------------------
        // Animal
        // -------------------------------------------------------

        private void AjouterAnimal_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "consulter";
            dgListe.Visibility = Visibility.Collapsed;
            BtnRetour.Visibility = Visibility.Collapsed;

            FenetreAjoutAnimal formulaire = new FenetreAjoutAnimal();
            ZoneContenu.Content = formulaire.Content;
            ZoneContenu.DataContext = formulaire.DataContext;
        }

        private void ModifierAnimal_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "modifier";
            ChargerListeAnimaux();
        }

        private void SupprimerAnimal_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "supprimer";
            ChargerListeAnimaux();
        }

        private void ListerAnimaux_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "consulter";
            ChargerListeAnimaux();
        }

        // -------------------------------------------------------
        // Contact
        // -------------------------------------------------------

        private void AjouterContact_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "consulter";
            dgListe.Visibility = Visibility.Collapsed;
            BtnRetour.Visibility = Visibility.Collapsed;

            FenetreAjoutContact formulaire = new FenetreAjoutContact();
            ZoneContenu.Content = formulaire.Content;
            ZoneContenu.DataContext = formulaire.DataContext;
        }

        private void ModifierContact_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "modifier";
            ChargerListeContacts();
        }

        private void SupprimerContact_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "supprimer";
            ChargerListeContacts();
        }

        private void ListerContacts_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "consulter";
            ChargerListeContacts();
        }

        // -------------------------------------------------------
        // Double-clic sur la liste
        // -------------------------------------------------------

        private async void DgListe_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgListe.SelectedItem == null) return;

            if (_entiteActuelle == "animal")
            {
                CoucheMetier.Animal animal = (CoucheMetier.Animal)dgListe.SelectedItem;

                if (_modeActuel == "supprimer")
                {
                    MessageBoxResult confirmation = MessageBox.Show(
                        $"Voulez-vous vraiment supprimer {animal.Nom} ?",
                        "Confirmation",
                        MessageBoxButton.YesNo);

                    if (confirmation == MessageBoxResult.Yes)
                    {
                        await _vmAnimal.SupprimerAnimal(animal);
                        MessageBox.Show("Animal supprimé avec succès.");
                        await _vmAnimal.ChargerAnimaux();
                        dgListe.ItemsSource = _vmAnimal.Animaux;
                    }
                    _modeActuel = "consulter";
                }
                else if (_modeActuel == "modifier")
                {
                    dgListe.Visibility = Visibility.Collapsed;
                    BtnRetour.Visibility = Visibility.Collapsed;

                    FenetreModifierAnimal formulaire = new FenetreModifierAnimal(animal);
                    ZoneContenu.Content = formulaire.Content;
                    ZoneContenu.DataContext = formulaire.DataContext;
                    _modeActuel = "consulter";
                }
                else if (_modeActuel == "consulter")
                {
                    dgListe.Visibility = Visibility.Collapsed;
                    BtnRetour.Visibility = Visibility.Collapsed;

                    FenetreConsulterAnimal formulaire = new FenetreConsulterAnimal(animal);
                    ZoneContenu.Content = formulaire.Content;
                    ZoneContenu.DataContext = formulaire.DataContext;
                }
            }
            else if (_entiteActuelle == "contact")
            {
                CoucheMetier.Contact contact = (CoucheMetier.Contact)dgListe.SelectedItem;

                if (_modeActuel == "supprimer")
                {
                    MessageBoxResult confirmation = MessageBox.Show(
                        $"Voulez-vous vraiment supprimer {contact.Nom} {contact.Prenom} ?",
                        "Confirmation",
                        MessageBoxButton.YesNo);

                    if (confirmation == MessageBoxResult.Yes)
                    {
                        await _vmContact.SupprimerContact(contact);
                        MessageBox.Show("Contact supprimé avec succès.");
                        await _vmContact.ChargerContacts();
                        dgListe.ItemsSource = _vmContact.Contacts;
                    }
                    _modeActuel = "consulter";
                }
                else if (_modeActuel == "modifier")
                {
                    dgListe.Visibility = Visibility.Collapsed;
                    BtnRetour.Visibility = Visibility.Collapsed;

                    FenetreModifierContact formulaire = new FenetreModifierContact(contact);
                    ZoneContenu.Content = formulaire.Content;
                    ZoneContenu.DataContext = formulaire.DataContext;
                    _modeActuel = "consulter";
                }
                else if (_modeActuel == "consulter")
                {
                    dgListe.Visibility = Visibility.Collapsed;
                    BtnRetour.Visibility = Visibility.Collapsed;

                    FenetreConsulterContact formulaire = new FenetreConsulterContact(contact);
                    ZoneContenu.Content = formulaire.Content;
                    ZoneContenu.DataContext = formulaire.DataContext;
                }
            }
        }

        // -------------------------------------------------------
        // Bouton retour
        // -------------------------------------------------------

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            _modeActuel = "consulter";
            dgListe.Visibility = Visibility.Collapsed;
            BtnRetour.Visibility = Visibility.Collapsed;
        }
    }
}
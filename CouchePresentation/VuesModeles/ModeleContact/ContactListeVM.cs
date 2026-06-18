using System.Collections.ObjectModel;
using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles
{
    public class ContactVM : INotifyPropertyChanged
    {
        // -------------------------------------------------------
        // INotifyPropertyChanged
        // -------------------------------------------------------

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // -------------------------------------------------------
        // DAO
        // -------------------------------------------------------

        private ContactDAO _dao = new ContactDAO();

        // -------------------------------------------------------
        // Propriétés
        // -------------------------------------------------------

        private ObservableCollection<Contact> _contacts;
        public ObservableCollection<Contact> Contacts
        {
            get { return this._contacts; }
            set
            {
                this._contacts = value;
                OnPropertyChanged("Contacts");
            }
        }

        private Contact _contactSelectionne;
        public Contact ContactSelectionne
        {
            get { return this._contactSelectionne; }
            set
            {
                this._contactSelectionne = value;
                OnPropertyChanged("ContactSelectionne");
            }
        }

        // -------------------------------------------------------
        // Constructeur
        // -------------------------------------------------------

        public ContactVM()
        {
            Contacts = new ObservableCollection<Contact>();
        }

        // -------------------------------------------------------
        // Méthodes
        // -------------------------------------------------------

        public async Task ChargerContacts()
        {
            List<Contact> liste = await _dao.AfficherListeContacts();
            Contacts.Clear();
            foreach (Contact c in liste)
                Contacts.Add(c);
        }
        public async Task SupprimerContact(CoucheMetier.Contact contact)
        {
            await _dao.DeleteAsync(contact);
            await ChargerContacts();
        }
    }
}
using System.Collections.ObjectModel;
using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleAdoption
{
    public class AjoutAdoptionVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // -------------------------------------------------------
        // DAO
        // -------------------------------------------------------

        private ContactDAO _contactDAO = new ContactDAO();
        private AdoptionDAO _adoptionDAO = new AdoptionDAO();
        private SortieDAO _sortieDAO = new SortieDAO();

        // -------------------------------------------------------
        // Animal concerné
        // -------------------------------------------------------

        public CoucheMetier.Animal Animal { get; set; }

        // -------------------------------------------------------
        // Liste des contacts (adoptants potentiels)
        // -------------------------------------------------------

        private ObservableCollection<CoucheMetier.Contact> _contacts;
        public ObservableCollection<CoucheMetier.Contact> Contacts
        {
            get { return this._contacts; }
            set { this._contacts = value; OnPropertyChanged("Contacts"); }
        }

        private CoucheMetier.Contact _contactSelectionne;
        public CoucheMetier.Contact ContactSelectionne
        {
            get { return this._contactSelectionne; }
            set { this._contactSelectionne = value; OnPropertyChanged("ContactSelectionne"); }
        }

        // -------------------------------------------------------
        // Statuts disponibles
        // -------------------------------------------------------

        public List<string> Statuts => new List<string> { "demande", "acceptee" };

        private string _statutSelectionne = "demande";
        public string StatutSelectionne
        {
            get { return this._statutSelectionne; }
            set { this._statutSelectionne = value; OnPropertyChanged("StatutSelectionne"); }
        }

        // -------------------------------------------------------
        // Date de demande
        // -------------------------------------------------------

        private DateTime _dateDemande = DateTime.Today;
        public DateTime DateDemande
        {
            get { return this._dateDemande; }
            set { this._dateDemande = value; OnPropertyChanged("DateDemande"); }
        }

        public DateTime DateDemandeMin => DateTime.Today.AddMonths(-1);
        public DateTime DateDemandeMax => DateTime.Today;

        // -------------------------------------------------------
        // Constructeur
        // -------------------------------------------------------

        public AjoutAdoptionVM(CoucheMetier.Animal animal)
        {
            Animal = animal;
            Contacts = new ObservableCollection<CoucheMetier.Contact>();
        }

        // -------------------------------------------------------
        // Chargement des contacts
        // -------------------------------------------------------

        public async Task ChargerDonnees()
        {
            List<CoucheMetier.Contact> contacts = await _contactDAO.AfficherListeContacts();
            Contacts.Clear();
            foreach (CoucheMetier.Contact c in contacts)
                Contacts.Add(c);
        }

        // -------------------------------------------------------
        // Enregistrement de l'adoption
        // -------------------------------------------------------

        public async Task EnregistrerAdoption()
        {
            Adoption adoption = Adoption.Create(Animal, ContactSelectionne, DateDemande, StatutSelectionne);
            await _adoptionDAO.InsertAsync(adoption);

            // Si le statut est "acceptee" dès la création, on insère automatiquement la sortie
            if (StatutSelectionne == "acceptee")
            {
                Sortie sortie = Sortie.Create(Animal, ContactSelectionne, DateDemande, "adoption");
                await _sortieDAO.InsertAsync(sortie);
            }
        }
    }
}
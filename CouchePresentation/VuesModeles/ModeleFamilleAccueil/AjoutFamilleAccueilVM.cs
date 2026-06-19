using System.Collections.ObjectModel;
using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleFamilleAccueil
{
    public class AjoutFamilleAccueilVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ContactDAO _contactDAO = new ContactDAO();
        private Personne_RoleDAO _roleDAO = new Personne_RoleDAO();
        private Famille_AccueilDAO _faDAO = new Famille_AccueilDAO();
        private SortieDAO _sortieDAO = new SortieDAO();

        public CoucheMetier.Animal Animal { get; set; }

        private ObservableCollection<CoucheMetier.Contact> _famillesAccueil;
        public ObservableCollection<CoucheMetier.Contact> FamillesAccueil
        {
            get { return this._famillesAccueil; }
            set { this._famillesAccueil = value; OnPropertyChanged("FamillesAccueil"); }
        }

        private CoucheMetier.Contact _contactSelectionne;
        public CoucheMetier.Contact ContactSelectionne
        {
            get { return this._contactSelectionne; }
            set { this._contactSelectionne = value; OnPropertyChanged("ContactSelectionne"); }
        }

        private DateTime _dateDebut = DateTime.Today;
        public DateTime DateDebut
        {
            get { return this._dateDebut; }
            set { this._dateDebut = value; OnPropertyChanged("DateDebut"); }
        }

        public DateTime DateDebutMin => DateTime.Today.AddMonths(-1);
        public DateTime DateDebutMax => DateTime.Today.AddDays(5);

        public AjoutFamilleAccueilVM(CoucheMetier.Animal animal)
        {
            Animal = animal;
            FamillesAccueil = new ObservableCollection<CoucheMetier.Contact>();
        }

        public async Task ChargerDonnees()
        {
            List<CoucheMetier.Contact> contacts = await _contactDAO.AfficherListeContacts();
            FamillesAccueil.Clear();

            foreach (CoucheMetier.Contact c in contacts)
            {
                var roles = await _roleDAO.AfficherParContactAsync(c.Identifiant);
                if (roles.Any(r => r.RoleNom == "famille_accueil"))
                    FamillesAccueil.Add(c);
            }
        }

        public async Task EnregistrerFamilleAccueil()
        {
            // 1 - Insérer la famille d'accueil
            Famille_Accueil fa = Famille_Accueil.Create(Animal, ContactSelectionne, DateDebut, null);
            await _faDAO.InsertAsync(fa);

            // 2 - Insérer automatiquement la sortie
            Sortie sortie = Sortie.Create(Animal, ContactSelectionne, DateDebut, "famille_accueil");
            await _sortieDAO.InsertAsync(sortie);
        }
    }
}
using System.Collections.ObjectModel;
using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.Animal
{
    public class AjoutAnimalVM : INotifyPropertyChanged
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

        private AnimalDAO _animalDAO = new AnimalDAO();
        private CouleurDAO _couleurDAO = new CouleurDAO();
        private CompatibiliteDAO _compatibiliteDAO = new CompatibiliteDAO();
        private EntreeDAO _entreeDAO = new EntreeDAO();
        private ContactDAO _contactDAO = new ContactDAO();

        // -------------------------------------------------------
        // Listes prédéfinies chargées depuis la DB
        // -------------------------------------------------------

        private ObservableCollection<Couleur> _couleurs;
        public ObservableCollection<Couleur> Couleurs
        {
            get { return this._couleurs; }
            set { this._couleurs = value; OnPropertyChanged("Couleurs"); }
        }

        private ObservableCollection<CompatibiliteSelectionnable> _compatibilites;
        public ObservableCollection<CompatibiliteSelectionnable> Compatibilites
        {
            get { return this._compatibilites; }
            set { this._compatibilites = value; OnPropertyChanged("Compatibilites"); }
        }

        private ObservableCollection<Contact> _contacts;
        public ObservableCollection<Contact> Contacts
        {
            get { return this._contacts; }
            set { this._contacts = value; OnPropertyChanged("Contacts"); }
        }

        // -------------------------------------------------------
        // Sélections de l'utilisateur
        // -------------------------------------------------------

        public ObservableCollection<Couleur> CouleursSelectionnees { get; set; }

        private Contact _contactSelectionne;
        public Contact ContactSelectionne
        {
            get { return this._contactSelectionne; }
            set { this._contactSelectionne = value; OnPropertyChanged("ContactSelectionne"); }
        }

        // -------------------------------------------------------
        // Valeurs fixes pour les ComboBox
        // -------------------------------------------------------

        public List<string> Types => new List<string> { "chien", "chat" };
        public List<string> Sexes => new List<string> { "M", "F" };
        public List<string> Sterilises => new List<string> { "oui", "non" };
        public List<string> Raisons => new List<string>
        {
            "abandon", "errant", "deces_proprietaire",
            "saisie", "retour_adoption", "retour_famille_accueil"
        };

        // -------------------------------------------------------
        // Date d'entrée — aujourd'hui par défaut
        // -------------------------------------------------------

        private DateTime _dateEntree = DateTime.Today;
        public DateTime DateEntree
        {
            get { return this._dateEntree; }
            set
            {
                if (value > DateTime.Today)
                    throw new ArgumentException("La date d'entrée ne peut pas être dans le futur.");

                if (value < DateTime.Today.AddMonths(-1))
                    throw new ArgumentException("La date d'entrée ne peut pas dépasser un mois en arrière.");

                this._dateEntree = value;
                OnPropertyChanged("DateEntree");
            }
        }

        // Bornes du DatePicker
        public DateTime DateEntreeMin => DateTime.Today.AddMonths(-1);
        public DateTime DateEntreeMax => DateTime.Today;

        // -------------------------------------------------------
        // Constructeur
        // -------------------------------------------------------

        public AjoutAnimalVM()
        {
            Couleurs = new ObservableCollection<Couleur>();
            Compatibilites = new ObservableCollection<CompatibiliteSelectionnable>();
            Contacts = new ObservableCollection<Contact>();
            CouleursSelectionnees = new ObservableCollection<Couleur>();
        }

        // -------------------------------------------------------
        // Chargement des listes depuis la DB
        // -------------------------------------------------------

        public async Task ChargerDonnees()
        {
            List<Couleur> couleurs = await _couleurDAO.AfficherListeCouleurs();
            Couleurs.Clear();
            foreach (Couleur c in couleurs)
                Couleurs.Add(c);

            List<Compatibilite> compatibilites = await _compatibiliteDAO.AfficherListeCompatibilites();
            Compatibilites.Clear();
            foreach (Compatibilite c in compatibilites)
                Compatibilites.Add(new CompatibiliteSelectionnable { Compatibilite = c });

            List<Contact> contacts = await _contactDAO.AfficherListeContacts();
            Contacts.Clear();
            foreach (Contact c in contacts)
                Contacts.Add(c);
        }

        // -------------------------------------------------------
        // Enregistrement complet d'un animal
        // -------------------------------------------------------

        public async Task EnregistrerAnimal(CoucheMetier.Animal animal, string raison)
        {
            // 1 - Insérer l'animal et récupérer son identifiant
            await _animalDAO.InsertAsync(animal);

            // 2 - Insérer les couleurs sélectionnées
            Animal_CouleurDAO couleurDAO = new Animal_CouleurDAO();
            foreach (Couleur c in CouleursSelectionnees)
            {
                var animalCouleur = new Animal_CouleurDAO.AnimalCouleur(
                    c.Identifiant, c.Nom, animal.Identifiant);
                await couleurDAO.InsertAsync(animalCouleur);
            }

            // 3 - Insérer les compatibilités sélectionnées
            Animal_CompatibiliteDAO compatibiliteDAO = new Animal_CompatibiliteDAO();
            foreach (CompatibiliteSelectionnable c in Compatibilites.Where(x => x.Selectionnee || x.Incompatible))
            {
                var animalComp = new Animal_CompatibiliteDAO.AnimalCompatibilite(
                    c.Compatibilite.Identifiant,
                    c.Compatibilite.Type,
                    c.Valeur,
                    string.Empty,
                    animal.Identifiant);
                await compatibiliteDAO.InsertAsync(animalComp);
            }

            // 4 - Insérer l'entrée
            Entree entree = Entree.Create(animal, ContactSelectionne, DateEntree, raison);
            await _entreeDAO.InsertAsync(entree);
        }
    }
}
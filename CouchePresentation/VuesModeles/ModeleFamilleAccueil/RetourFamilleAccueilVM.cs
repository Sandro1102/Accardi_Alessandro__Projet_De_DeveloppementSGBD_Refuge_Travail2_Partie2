using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleFamilleAccueil
{
    public class RetourFamilleAccueilVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Famille_AccueilDAO _faDAO = new Famille_AccueilDAO();
        private EntreeDAO _entreeDAO = new EntreeDAO();

        public CoucheMetier.Animal Animal { get; set; }

        private Famille_Accueil _faActive;
        public Famille_Accueil FaActive
        {
            get { return this._faActive; }
            set { this._faActive = value; OnPropertyChanged("FaActive"); }
        }

        public RetourFamilleAccueilVM(CoucheMetier.Animal animal)
        {
            Animal = animal;
        }

        public async Task ChargerDonnees()
        {
            List<Famille_Accueil> liste = await _faDAO.AfficherParAnimalAsync(Animal.Identifiant);
            FaActive = liste.FirstOrDefault(f => f.EstActive());
        }

        public async Task ConfirmerRetour()
        {
            if (FaActive == null)
                throw new Exception("Aucune famille d'accueil active pour cet animal.");

            // 1 - Mettre à jour la famille d'accueil avec la date de fin
            FaActive.DateFin = DateTime.Today;
            await _faDAO.UpdateAsync(FaActive);

            // 2 - Insérer automatiquement l'entrée de retour
            Entree entree = Entree.Create(Animal, FaActive.ContactConcerne, DateTime.Today, "retour_famille_accueil");
            await _entreeDAO.InsertAsync(entree);
        }
    }
}
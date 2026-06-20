using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleAdoption
{
    public class ModifierAdoptionVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private AdoptionDAO _adoptionDAO = new AdoptionDAO();
        private EntreeDAO _entreeDAO = new EntreeDAO();
        private SortieDAO _sortieDAO = new SortieDAO();

        private Adoption _adoption;
        public Adoption Adoption
        {
            get { return this._adoption; }
            set { this._adoption = value; OnPropertyChanged("Adoption"); }
        }

        public List<string> Statuts => new List<string>
        {
            "demande", "acceptee", "rejet_environnement", "rejet_comportement"
        };

        private string _ancienStatut;

        public ModifierAdoptionVM(Adoption adoption)
        {
            Adoption = adoption;
            _ancienStatut = adoption.Statut;
        }

        public async Task ModifierStatut()
        {
            string nouveauStatut = Adoption.Statut;

            // Mise à jour de l'adoption
            await _adoptionDAO.UpdateAsync(Adoption);

            // Passage de "demande" à "acceptee" → sortie automatique
            if (_ancienStatut == "demande" && nouveauStatut == "acceptee")
            {
                Sortie sortie = Sortie.Create(Adoption.AnimalConcerne, Adoption.ContactConcerne, DateTime.Today, "adoption");
                await _sortieDAO.InsertAsync(sortie);
            }
            // Passage de "acceptee" à un rejet → retour automatique
            else if (_ancienStatut == "acceptee" &&
                    (nouveauStatut == "rejet_environnement" || nouveauStatut == "rejet_comportement"))
            {
                Entree entree = Entree.Create(Adoption.AnimalConcerne, Adoption.ContactConcerne, DateTime.Today, "retour_adoption");
                await _entreeDAO.InsertAsync(entree);
            }
        }
    }
}
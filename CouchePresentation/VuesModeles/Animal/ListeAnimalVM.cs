using System.Collections.ObjectModel;
using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles
{
    public class AnimalVM : INotifyPropertyChanged
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

        private AnimalDAO _dao = new AnimalDAO();

        // -------------------------------------------------------
        // Propriétés
        // -------------------------------------------------------

        private ObservableCollection<CoucheMetier.Animal> _animaux;
        public ObservableCollection<CoucheMetier.Animal> Animaux
        {
            get { return this._animaux; }
            set
            {
                this._animaux = value;
                OnPropertyChanged("Animaux");
            }
        }

        private CoucheMetier.Animal _animalSelectionne;
        public CoucheMetier.Animal AnimalSelectionne
        {
            get { return this._animalSelectionne; }
            set
            {
                this._animalSelectionne = value;
                OnPropertyChanged("AnimalSelectionne");
            }
        }

        // -------------------------------------------------------
        // Constructeur
        // -------------------------------------------------------

        public AnimalVM()
        {
            Animaux = new ObservableCollection<CoucheMetier.Animal>();
        }

        // -------------------------------------------------------
        // Méthodes
        // -------------------------------------------------------

        public async Task ChargerAnimaux()
        {
            List<CoucheMetier.Animal> liste = await _dao.AfficherListeAnimaux();
            Animaux.Clear();
            foreach (CoucheMetier.Animal a in liste)
                Animaux.Add(a);
        }

        public async Task AjouterAnimal(CoucheMetier.Animal animal)
        {
            await _dao.InsertAsync(animal);
            await ChargerAnimaux();
        }

        public async Task ModifierAnimal(CoucheMetier.Animal animal)
        {
            await _dao.UpdateAsync(animal);
            await ChargerAnimaux();
        }

        public async Task SupprimerAnimal(CoucheMetier.Animal animal)
        {
            await _dao.DeleteAsync(animal);
            await ChargerAnimaux();
        }

        public async Task<CoucheMetier.Animal?> ConsulterAnimal(string identifiant)
        {
            return await _dao.AfficherUnAsync(identifiant);
        }
    }
}
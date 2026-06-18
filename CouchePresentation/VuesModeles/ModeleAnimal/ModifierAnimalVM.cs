using System.Collections.ObjectModel;
using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.Animal
{
    public class ModifierAnimalVM : INotifyPropertyChanged
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
        // Animal à modifier
        // -------------------------------------------------------

        private CoucheMetier.Animal _animal;
        public CoucheMetier.Animal Animal
        {
            get { return this._animal; }
            set { this._animal = value; OnPropertyChanged("Animal"); }
        }

        // -------------------------------------------------------
        // Valeurs fixes pour les ComboBox
        // -------------------------------------------------------

        public List<string> Sterilises => new List<string> { "oui", "non" };

        // -------------------------------------------------------
        // Constructeur — reçoit l'animal sélectionné dans la liste
        // -------------------------------------------------------

        public ModifierAnimalVM(CoucheMetier.Animal animal)
        {
            Animal = animal;
        }

        // -------------------------------------------------------
        // Modification de l'animal
        // -------------------------------------------------------

        public async Task ModifierAnimal()
        {
            await _dao.UpdateAsync(Animal);
        }
    }
}
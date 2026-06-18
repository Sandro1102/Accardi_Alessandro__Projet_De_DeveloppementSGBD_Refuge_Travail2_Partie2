using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.Animal
{
    public class CompatibiliteSelectionnable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Compatibilite Compatibilite { get; set; }

        private bool _selectionnee;
        public bool Selectionnee
        {
            get { return this._selectionnee; }
            set
            {
                this._selectionnee = value;
                if (value)
                    this._incompatible = false;
                OnPropertyChanged("Selectionnee");
                OnPropertyChanged("Incompatible");
            }
        }

        private bool _incompatible = false;
        public bool Incompatible
        {
            get { return this._incompatible; }
            set
            {
                this._incompatible = value;
                if (value)
                    this._selectionnee = false;
                OnPropertyChanged("Incompatible");
                OnPropertyChanged("Selectionnee");
            }
        }

        public bool Valeur => !Incompatible;
    }
}
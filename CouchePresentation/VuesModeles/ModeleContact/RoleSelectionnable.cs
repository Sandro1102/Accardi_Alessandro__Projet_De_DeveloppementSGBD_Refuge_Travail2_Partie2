using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.GestionContact
{
    public class RoleSelectionnable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Role Role { get; set; }

        private bool _selectionnee;
        public bool Selectionnee
        {
            get { return this._selectionnee; }
            set
            {
                this._selectionnee = value;
                OnPropertyChanged("Selectionnee");
            }
        }
    }
}
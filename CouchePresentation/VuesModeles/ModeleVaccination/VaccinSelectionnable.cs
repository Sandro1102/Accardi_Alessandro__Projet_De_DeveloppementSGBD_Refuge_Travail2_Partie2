using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleVaccination
{
    public class VaccinSelectionnable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Vaccin Vaccin { get; set; }

        private DateTime? _date;
        public DateTime? Date
        {
            get { return this._date; }
            set
            {
                this._date = value;
                OnPropertyChanged("Date");
            }
        }
    }
}
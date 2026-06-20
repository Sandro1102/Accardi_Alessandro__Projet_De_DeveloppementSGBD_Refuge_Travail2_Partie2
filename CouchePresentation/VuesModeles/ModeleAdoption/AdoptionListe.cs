using System.Collections.ObjectModel;
using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleAdoption
{
    public class AdoptionListe : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private AdoptionDAO _dao = new AdoptionDAO();

        private ObservableCollection<Adoption> _adoptions;
        public ObservableCollection<Adoption> Adoptions
        {
            get { return this._adoptions; }
            set { this._adoptions = value; OnPropertyChanged("Adoptions"); }
        }

        public AdoptionListe()
        {
            Adoptions = new ObservableCollection<Adoption>();
        }

        public async Task ChargerAdoptions()
        {
            List<Adoption> liste = await _dao.AfficherListeAdoptions();
            Adoptions.Clear();
            foreach (Adoption a in liste)
                Adoptions.Add(a);
        }
    }
}
using System.Collections.ObjectModel;
using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleVaccination
{
    public class SupprimerVaccinationVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private VaccinationDAO _vaccinationDAO = new VaccinationDAO();

        public CoucheMetier.Animal Animal { get; set; }

        private ObservableCollection<Vaccination> _vaccinations;
        public ObservableCollection<Vaccination> Vaccinations
        {
            get { return this._vaccinations; }
            set { this._vaccinations = value; OnPropertyChanged("Vaccinations"); }
        }

        public SupprimerVaccinationVM(CoucheMetier.Animal animal)
        {
            Animal = animal;
            Vaccinations = new ObservableCollection<Vaccination>();
        }

        public async Task ChargerDonnees()
        {
            List<Vaccination> liste = await _vaccinationDAO.AfficherParAnimalAsync(Animal.Identifiant);
            Vaccinations.Clear();
            foreach (Vaccination v in liste)
                Vaccinations.Add(v);
        }

        public async Task SupprimerVaccination(Vaccination vaccination)
        {
            await _vaccinationDAO.DeleteAsync(vaccination);
        }
    }
}
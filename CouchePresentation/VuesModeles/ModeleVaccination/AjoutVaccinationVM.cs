using System.Collections.ObjectModel;
using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;
using Accardi_Alessandro_Refuge_WPF.CoucheMetier;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleVaccination
{
    public class AjoutVaccinationVM : INotifyPropertyChanged
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

        private VaccinDAO _vaccinDAO = new VaccinDAO();
        private VaccinationDAO _vaccinationDAO = new VaccinationDAO();

        // -------------------------------------------------------
        // Animal concerné
        // -------------------------------------------------------

        public CoucheMetier.Animal Animal { get; set; }

        // -------------------------------------------------------
        // Liste des vaccins disponibles
        // -------------------------------------------------------

        private ObservableCollection<VaccinSelectionnable> _vaccins;
        public ObservableCollection<VaccinSelectionnable> Vaccins
        {
            get { return this._vaccins; }
            set { this._vaccins = value; OnPropertyChanged("Vaccins"); }
        }

        // -------------------------------------------------------
        // Constructeur
        // -------------------------------------------------------

        public AjoutVaccinationVM(CoucheMetier.Animal animal)
        {
            Animal = animal;
            Vaccins = new ObservableCollection<VaccinSelectionnable>();
        }

        // -------------------------------------------------------
        // Chargement des vaccins disponibles
        // -------------------------------------------------------

        public async Task ChargerDonnees()
        {
            List<Vaccin> vaccins = await _vaccinDAO.AfficherListeVaccins();
            Vaccins.Clear();
            foreach (Vaccin v in vaccins)
                Vaccins.Add(new VaccinSelectionnable { Vaccin = v });
        }

        // -------------------------------------------------------
        // Enregistrement des vaccinations
        // -------------------------------------------------------

        public async Task EnregistrerVaccinations()
        {
            foreach (VaccinSelectionnable v in Vaccins.Where(x => x.Date.HasValue))
            {
                Vaccination vaccination = Vaccination.Create(Animal, v.Vaccin, v.Date.Value);
                await _vaccinationDAO.InsertAsync(vaccination);
            }
        }
    }
}
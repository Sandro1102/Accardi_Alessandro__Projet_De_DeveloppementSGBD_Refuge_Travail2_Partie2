using System.ComponentModel;
using Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees;

namespace Accardi_Alessandro_Refuge_WPF.VuesModèles.ModeleContact
{
    public class ModifierContactVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ContactDAO _dao = new ContactDAO();

        private CoucheMetier.Contact _contact;
        public CoucheMetier.Contact Contact
        {
            get { return this._contact; }
            set { this._contact = value; OnPropertyChanged("Contact"); }
        }

        public ModifierContactVM(CoucheMetier.Contact contact)
        {
            Contact = contact;
        }

        public async Task ModifierContact()
        {
            await _dao.UpdateAsync(Contact);
        }
    }
}
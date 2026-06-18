using System.Windows;
using Accardi_Alessandro_Refuge_WPF.VuesModèles;

namespace Accardi_Alessandro_Refuge_WPF.Vues
{
    public partial class MainWindow : Window
    {
        private AnimalVM _vm = new AnimalVM();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ListerAnimaux_Click(object sender, RoutedEventArgs e)
        {
            await _vm.ChargerAnimaux();
            dgAnimaux.ItemsSource = _vm.Animaux;
            dgAnimaux.Visibility = Visibility.Visible;
        }
    }
}
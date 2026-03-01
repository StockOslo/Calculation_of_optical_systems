using System.Windows;

namespace Calculation_of_optical_systems
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new OpticalPage());
        }

        private void OpenOptical(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new OpticalPage());

        private void OpenGrip(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new GripPage());

        private void OpenLenses(object sender, RoutedEventArgs e)
            => MainFrame.Navigate(new LensesPage());
    }
}
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Calculation_of_optical_systems
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ParseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParseButton.IsEnabled = false;
                ParseButton.Content = "Загрузка...";

                LensesListBox.Items.Clear();

                var parser = new LensParser();
                int count = 0;

                await foreach (var lens in parser.ParseLensesAsync())
                {
                    LensesListBox.Items.Add(
                        $"{lens.Model} | {lens.SensorFormat} | {lens.FocalLength}");

                    count++;

                    ParseButton.Content = $"Загружено {count}...";
                }

                MessageBox.Show(
                    $"Загружено {count} объективов!\nФайл сохранен как objectivy.csv");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            finally
            {
                ParseButton.IsEnabled = true;
                ParseButton.Content = "Загрузить объективы с сайта";
            }
        }
    }
}
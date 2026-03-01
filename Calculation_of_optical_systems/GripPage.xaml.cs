using System.Globalization;
using System.Windows.Controls;

namespace Calculation_of_optical_systems
{
    public partial class GripPage : Page
    {
        public GripPage()
        {
            InitializeComponent();
        }

        private void InputChanged(object sender,
            System.Windows.RoutedEventArgs e)
        {
            Recalculate();
        }

        private void Recalculate()
        {
            var input = new GripCalculationInput
            {
                R = Parse(RBox.Text),
                f = Parse(FBox.Text),
                z = Parse(ZBox.Text),
                K = Parse(KBox.Text)
            };

            var result = GripCalculator.Calculate(input);


            // вывод результатов


            HResult.Text = $"Гиперфокальное расстояние H = {result.H:F3} м";
            R1Result.Text = $"Передняя граница R1 = {result.R1:F3} м";
            R2Result.Text = $"Задняя граница R2 = {result.R2:F3} м";
        }

        private double Parse(string text)
        {
            double.TryParse(
                text.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double value);

            return value;
        }
    }
}
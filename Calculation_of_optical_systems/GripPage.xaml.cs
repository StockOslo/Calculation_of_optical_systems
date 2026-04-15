using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Calculation_of_optical_systems
{
    public partial class GripPage : Page
    {
        public GripPage()
        {
            InitializeComponent();
        }

        // пересчет при изменении любого поля
        private void InputChanged(object sender, RoutedEventArgs e)
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

            // защита от некорректных данных
            if (input.f <= 0 || input.K <= 0 || input.z <= 0)
            {
                HResult.Text = "введите корректные данные";
                R1Result.Text = "";
                R2Result.Text = "";
                return;
            }

            var result = GripCalculator.Calculate(input);

            // гиперфокальное расстояние
            HResult.Text =
                $"гиперфокальное расстояние (h) = {result.H:0.00000} м";

            // передняя граница
            R1Result.Text =
                $"передняя граница (r1) = {result.R1:0.00000} м";

            // задняя граница
            if (result.R2 < 0)
            {
                R2Result.Text = "задняя граница (r2) = ∞";
            }
            else
            {
                R2Result.Text =
                    $"задняя граница (r2) = {result.R2:0.00000} м";
            }
        }

        // безопасный парсинг
        public static double Parse(string text)
        {
            double.TryParse(
                text?.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double value);

            return value;
        }
    }
}
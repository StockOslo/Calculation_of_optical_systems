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

        // Пересчёт при изменении любого поля
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

            var result = GripCalculator.Calculate(input);

            // Гиперфокальное расстояние
            HResult.Text =
                $"Гиперфокальное расстояние (H) = {result.H:0.00000} м";

            // Передняя граница резкости
            R1Result.Text =
                $"Передняя граница (R₁) = {result.R1:0.00000} м";

            // Задняя граница резкости
            // Если отрицательная — значит уходит в бесконечность
            if (result.R2 < 0)
            {
                R2Result.Text =
                    $"Задняя граница (R₂) = ∞";
            }
            else
            {
                R2Result.Text =
                    $"Задняя граница (R₂) = {result.R2:0.00000} м";
            }
        }

        // Безопасный парсинг числа (поддержка точки и запятой)
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
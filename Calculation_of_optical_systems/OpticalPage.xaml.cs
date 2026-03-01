using System.Globalization;
using System.Reflection;
using System.Windows.Controls;

namespace Calculation_of_optical_systems
{
    public partial class OpticalPage : Page
    {
        public OpticalPage()
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
            var input = new FirstFileCalculationInput
            {
                Nh = Parse(NhBox.Text),
                Nv = Parse(NvBox.Text),
                Δh = Parse(DhBox.Text),
                Δv = Parse(DvBox.Text),
                f = Parse(FBox.Text)
            };

            var result = FirstFileCalculator.Calculate(input);

            ResultPanel.Children.Clear();

            foreach (PropertyInfo prop in
                     typeof(FirstFileCalculationResult).GetProperties())
            {
                // 🚫 скрываем все *_2
                if (prop.Name.Contains("2"))
                    continue;

                var value = prop.GetValue(result);

                ResultPanel.Children.Add(new TextBlock
                {
                    Text = $"{prop.Name} : {value:F4}",
                    FontSize = 15,
                    Margin = new System.Windows.Thickness(0, 6, 0, 6)
                });
            }
        }

        private double Parse(string t)
        {
            double.TryParse(
                t.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double v);

            return v;
        }
    }
}
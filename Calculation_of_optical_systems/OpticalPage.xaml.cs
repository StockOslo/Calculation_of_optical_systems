using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calculation_of_optical_systems
{
    public partial class OpticalPage : Page
    {
        private bool _isLoaded = false;

        // запоминаем предыдущий режим
        private int _previousMode = 0;

        public OpticalPage()
        {
            InitializeComponent();
            Loaded += OpticalPage_Loaded;
        }

        private void OpticalPage_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;

            ApplyModeUI();
            Recalculate();
        }

        private void InputChanged(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
                return;

            Recalculate();
        }

        private CalculationMode GetMode()
        {
            return ModeBox.SelectedIndex switch
            {
                0 => CalculationMode.Base,
                1 => CalculationMode.Base,
                2 => CalculationMode.SolveResolution,
                3 => CalculationMode.SolvePixelSize,
                _ => CalculationMode.Base
            };
        }

        private void Recalculate()
        {
            if (!_isLoaded || ResultPanel == null)
                return;

            var input = new FirstFileCalculationInput
            {
                Nh = Parse(NhBox?.Text),
                Nv = Parse(NvBox?.Text),
                Δh = Parse(DhBox?.Text),
                Δv = Parse(DvBox?.Text),
                f = Parse(FBox?.Text),
                δh = Parse(AngleHBox?.Text),
                δv = Parse(AngleVBox?.Text)
            };

            int uiMode = ModeBox.SelectedIndex;
            var mode = GetMode();

            var result = FirstFileCalculator.Calculate(input, mode);

            ResultPanel.Children.Clear();

            string[] visibleProps;

            if (uiMode == 0)
            {
                visibleProps = new[]
                {
                    "h","i","d",
                    "δh","δv","δd",
                    "δh_min","δv_min","δd_min"
                };
            }
            else if (uiMode == 1)
            {
                visibleProps = new[]
                {
                    "δh_pix_grad","δv_pix_grad",
                    "δh_pix_angle","δv_pix_angle"
                };
            }
            else if (uiMode == 2)
            {
                visibleProps = new[]
                {
                    "Nh","Nv"
                };
            }
            else
            {
                visibleProps = new[]
                {
                    "Δh","Δv"
                };
            }

            foreach (string name in visibleProps)
            {
                var prop = typeof(FirstFileCalculationResult).GetProperty(name);
                if (prop == null) continue;

                var value = prop.GetValue(result);
                if (value == null) continue;

                if (value is double d && d == 0)
                    continue;

                var block = new StackPanel
                {
                    Margin = new Thickness(0, 8, 0, 8)
                };

                block.Children.Add(new TextBlock
                {
                    Text = GetLabel(name),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold
                });

                block.Children.Add(new TextBlock
                {
                    Text = $"{value:0.00000} {GetUnit(name)}",
                    FontSize = 15
                });

                block.Children.Add(new Border
                {
                    Height = 1,
                    Background = Brushes.LightGray,
                    Margin = new Thickness(0, 6, 0, 0)
                });

                ResultPanel.Children.Add(block);
            }
        }

        private double Parse(string t)
        {
            if (string.IsNullOrWhiteSpace(t))
                return 0;

            double.TryParse(
                t.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double v);

            return v;
        }

        private string GetLabel(string name)
        {
            return name switch
            {
                "h" => "Высота матрицы (h)",
                "i" => "Ширина матрицы (i)",
                "d" => "Диагональ (d)",

                "δh" => "Вертикальный угол (δh)",
                "δv" => "Горизонтальный угол (δv)",
                "δd" => "Диагональный угол (δd)",

                "δh_min" => "Минуты δh",
                "δv_min" => "Минуты δv",
                "δd_min" => "Минуты δd",

                "δh_pix_grad" => "Угол пикселя δh",
                "δv_pix_grad" => "Угол пикселя δv",

                "δh_pix_angle" => "Пиксель δh (сек)",
                "δv_pix_angle" => "Пиксель δv (сек)",

                "Nh" => "Nh",
                "Nv" => "Nv",

                "Δh" => "Δh",
                "Δv" => "Δv",

                _ => name
            };
        }

        private string GetUnit(string name)
        {
            if (name == "h" || name == "i" || name == "d")
                return "мм";

            if (name.Contains("pix_angle"))
                return "″";

            if (name.Contains("min"))
                return "′";

            if (name.StartsWith("δ"))
                return "°";

            if (name == "Δh" || name == "Δv")
                return "мкм";

            if (name == "Nh" || name == "Nv")
                return "пикс";

            return "";
        }

        private void ApplyModeUI()
        {
            int m = ModeBox.SelectedIndex;

            ResolutionPanel.Visibility = Visibility.Visible;
            PixelPanel.Visibility = Visibility.Visible;
            AnglePanel.Visibility = Visibility.Visible;

            if (m == 0 || m == 1)
                AnglePanel.Visibility = Visibility.Collapsed;
            else if (m == 2)
                ResolutionPanel.Visibility = Visibility.Collapsed;
            else if (m == 3)
                PixelPanel.Visibility = Visibility.Collapsed;
        }

        private void ModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded)
                return;

            int currentMode = ModeBox.SelectedIndex;

            ApplyModeUI();

            
            bool sameInput =
                (_previousMode == 0 && currentMode == 1) ||
                (_previousMode == 1 && currentMode == 0);

            if (!sameInput)
            {
                NhBox.Text = "";
                NvBox.Text = "";
                DhBox.Text = "";
                DvBox.Text = "";
                AngleHBox.Text = "";
                AngleVBox.Text = "";
            }

            _previousMode = currentMode;

            Recalculate();
        }
    }
}
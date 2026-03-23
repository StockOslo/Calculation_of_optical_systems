using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calculation_of_optical_systems
{
    public partial class OpticalPage : Page
    {
        private bool _isLoaded = false; // флаг чтобы не считать пока страница не загрузилась

        private int _previousMode = 0; // запоминаем предыдущий режим чтобы понимать очищать поля или нет

        public OpticalPage()
        {
            InitializeComponent();
            Loaded += OpticalPage_Loaded; // подписка на событие загрузки страницы
        }

        private void OpticalPage_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true; // теперь можно работать с логикой

            ApplyModeUI(); // применяем видимость блоков
            Recalculate(); // первый расчет сразу при открытии
        }

        private void InputChanged(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
                return; // если еще не загрузилось то ничего не делаем

            Recalculate(); // пересчет при любом изменении поля
        }

        private CalculationMode GetMode()
        {
            return ModeBox.SelectedIndex switch
            {
                0 => CalculationMode.Base, // базовый режим
                1 => CalculationMode.Base, // режим пикселя использует ту же формулу
                2 => CalculationMode.SolveResolution, // поиск Nh Nv
                3 => CalculationMode.SolvePixelSize, // поиск Δh Δv
                _ => CalculationMode.Base
            };
        }

        private void Recalculate()
        {
            if (!_isLoaded || ResultPanel == null)
                return; // защита от null и преждевременного вызова

            var input = new FirstFileCalculationInput
            {
                Nh = Parse(NhBox?.Text), // парсим Nh
                Nv = Parse(NvBox?.Text), // парсим Nv
                Δh = Parse(DhBox?.Text), // парсим Δh
                Δv = Parse(DvBox?.Text), // парсим Δv
                f = Parse(FBox?.Text), // парсим фокус
                δh = Parse(AngleHBox?.Text), // парсим угол h
                δv = Parse(AngleVBox?.Text) // парсим угол v
            };

            int uiMode = ModeBox.SelectedIndex; // текущий режим UI
            var mode = GetMode(); // логический режим

            var result = FirstFileCalculator.Calculate(input, mode); // считаем

            ResultPanel.Children.Clear(); // очищаем старые результаты

            string[] visibleProps;

            if (uiMode == 0)
            {
                visibleProps = new[]
                {
                    "h","i","d",
                    "δh","δv","δd",
                    "δh_min","δv_min","δd_min"
                }; // базовые значения
            }
            else if (uiMode == 1)
            {
                visibleProps = new[]
                {
                    "δh_pix_grad","δv_pix_grad",
                    "δh_pix_angle","δv_pix_angle"
                }; // режим пикселя
            }
            else if (uiMode == 2)
            {
                visibleProps = new[]
                {
                    "Nh","Nv"
                }; // вывод разрешения
            }
            else
            {
                visibleProps = new[]
                {
                    "Δh","Δv"
                }; // вывод размера пикселя
            }

            foreach (string name in visibleProps)
            {
                var prop = typeof(FirstFileCalculationResult).GetProperty(name); // получаем свойство через рефлексию
                if (prop == null) continue;

                var value = prop.GetValue(result); // берем значение
                if (value == null) continue;

                if (value is double d && d == 0)
                    continue; // не показываем нули

                var block = new StackPanel
                {
                    Margin = new Thickness(0, 8, 0, 8)
                };

                block.Children.Add(new TextBlock
                {
                    Text = GetLabel(name), // подпись
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold
                });

                block.Children.Add(new TextBlock
                {
                    Text = $"{value:0.00000} {GetUnit(name)}", // значение + единица
                    FontSize = 15
                });

                block.Children.Add(new Border
                {
                    Height = 1,
                    Background = Brushes.LightGray,
                    Margin = new Thickness(0, 6, 0, 0)
                });

                ResultPanel.Children.Add(block); // добавляем в UI
            }
        }

        private double Parse(string t)
        {
            if (string.IsNullOrWhiteSpace(t))
                return 0; // если пусто возвращаем 0

            double.TryParse(
                t.Replace(",", "."), // меняем запятую на точку
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
            int m = ModeBox.SelectedIndex; // текущий режим

            ResolutionPanel.Visibility = Visibility.Visible;
            PixelPanel.Visibility = Visibility.Visible;
            AnglePanel.Visibility = Visibility.Visible;

            if (m == 0 || m == 1)
                AnglePanel.Visibility = Visibility.Collapsed; // скрываем углы

            else if (m == 2)
                ResolutionPanel.Visibility = Visibility.Collapsed; // скрываем разрешение

            else if (m == 3)
                PixelPanel.Visibility = Visibility.Collapsed; // скрываем пиксель
        }

        private void ModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded)
                return;

            int currentMode = ModeBox.SelectedIndex;

            ApplyModeUI(); // обновляем UI

            bool sameInput =
                (_previousMode == 0 && currentMode == 1) ||
                (_previousMode == 1 && currentMode == 0); // проверяем одинаковые ли входные данные

            if (!sameInput)
            {
                // очищаем поля если режим реально другой
                NhBox.Text = "";
                NvBox.Text = "";
                DhBox.Text = "";
                DvBox.Text = "";
                AngleHBox.Text = "";
                AngleVBox.Text = "";
            }

            _previousMode = currentMode; // сохраняем текущий режим

            Recalculate(); // пересчет
        }
    }
}
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calculation_of_optical_systems
{
    public partial class OpticalPage : Page
    {
        private bool _isLoaded = false; // флаг загрузки страницы

        private int _previousMode = 0; // предыдущий выбранный режим

        public OpticalPage()
        {
            InitializeComponent();
            Loaded += OpticalPage_Loaded; // подписка на событие загрузки
        }

        private void OpticalPage_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true; // страница загружена

            ApplyModeUI(); // применяем отображение режима
            Recalculate(); // выполняем первый расчет
        }

        private void InputChanged(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
                return; // игнор до полной загрузки

            Recalculate(); // пересчет при изменении ввода
        }

        private CalculationMode GetMode()
        {
            return ModeBox.SelectedIndex switch // определяем режим по выбранному индексу
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
                return; // защита от вызова до инициализации

            var input = new FirstFileCalculationInput // собираем входные данные
            {
                Nh = Parse(NhBox?.Text),
                Nv = Parse(NvBox?.Text),
                Δh = Parse(DhBox?.Text),
                Δv = Parse(DvBox?.Text),
                f = Parse(FBox?.Text),
                δh = Parse(AngleHBox?.Text),
                δv = Parse(AngleVBox?.Text)
            };

            if (input.f <= 0) // проверка фокусного расстояния
            {
                ResultPanel.Children.Clear();

                ResultPanel.Children.Add(new TextBlock
                {
                    Text = "Введите корректное фокусное расстояние",
                    Foreground = Brushes.Black,
                    FontSize = 14
                });

                return;
            }

            FirstFileCalculationResult result;

            try
            {
                result = FirstFileCalculator.Calculate(input, GetMode()); // основной расчет
            }
            catch
            {
                ResultPanel.Children.Clear();

                ResultPanel.Children.Add(new TextBlock
                {
                    Text = "ошибка вычисления",
                    Foreground = Brushes.Red
                });

                return;
            }

            int uiMode = ModeBox.SelectedIndex; // текущий режим интерфейса

            ResultPanel.Children.Clear();

            string[] visibleProps;

            if (uiMode == 0) // режим базовых расчетов
            {
                visibleProps = new[]
                {
                    "h","i","d",
                    "δh","δv","δd",
                    "δh_min","δv_min","δd_min"
                };
            }
            else if (uiMode == 1) // режим пикселя
            {
                visibleProps = new[]
                {
                    "δh_pix_grad","δv_pix_grad",
                    "δh_pix_angle","δv_pix_angle"
                };
            }
            else if (uiMode == 2) // расчет разрешения
            {
                visibleProps = new[]
                {
                    "Nh","Nv"
                };
            }
            else // расчет размера пикселя
            {
                visibleProps = new[]
                {
                    "Δh","Δv"
                };
            }

            foreach (string name in visibleProps) // вывод результатов
            {
                var prop = typeof(FirstFileCalculationResult).GetProperty(name);
                if (prop == null) continue;

                var value = prop.GetValue(result);
                if (value == null) continue;

                if (value is double d && d == 0)
                    continue; // пропускаем нулевые значения

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

        public static double Parse(string t)
        {
            if (string.IsNullOrWhiteSpace(t))
                return 0; // пустое значение

            double.TryParse(
                t.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double v);

            return v; // результат парсинга
        }

        public static string GetLabel(string name)
        {
            return name switch // подписи для параметров
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

        public static string GetUnit(string name)
        {
            if (name == "h" || name == "i" || name == "d")
                return "мм"; // миллиметры

            if (name.Contains("pix_angle"))
                return "″"; // секунды

            if (name.Contains("min"))
                return "′"; // минуты

            if (name.StartsWith("δ"))
                return "°"; // градусы

            if (name == "Δh" || name == "Δv")
                return "мкм"; // микрометры

            if (name == "Nh" || name == "Nv")
                return "пикс"; // пиксели

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
                return; // игнор до загрузки

            int currentMode = ModeBox.SelectedIndex; // новый режим

            ApplyModeUI();

            bool sameInput =
                (_previousMode == 0 && currentMode == 1) ||
                (_previousMode == 1 && currentMode == 0); // проверка одинакового ввода

            if (!sameInput)
            {
                NhBox.Text = "";
                NvBox.Text = "";
                DhBox.Text = "";
                DvBox.Text = "";
                AngleHBox.Text = "";
                AngleVBox.Text = ""; // очистка полей
            }

            _previousMode = currentMode; // обновляем режим

            Recalculate(); // пересчет
        }
    }
}
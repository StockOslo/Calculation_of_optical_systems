using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calculation_of_optical_systems
{
    public partial class OpticalPage : Page
    {
        public OpticalPage()
        {
            InitializeComponent();
        }

        // Срабатывает при изменении любого поля ввода
        // Каждый раз пересчитываем всё заново
        private void InputChanged(object sender, RoutedEventArgs e)
        {
            Recalculate();
        }

        // Основной метод пересчёта
        // Забираем данные из TextBox → считаем → красиво выводим
        private void Recalculate()
        {
            // Собираем входные данные из UI
            var input = new FirstFileCalculationInput
            {
                Nh = Parse(NhBox.Text),
                Nv = Parse(NvBox.Text),
                Δh = Parse(DhBox.Text),
                Δv = Parse(DvBox.Text),
                f = Parse(FBox.Text)
            };

            // Считаем оптику
            var result = FirstFileCalculator.Calculate(input);

            // Очищаем старые результаты перед новым выводом
            ResultPanel.Children.Clear();

            // Явно указываем какие поля выводим
            // Без дублей *_2 и без фокусного расстояния
            string[] visibleProps =
            {
                "h","i","d",
                "δh","δv","δd",
                "δh_min","δv_min","δd_min",
                "δh_pix_grad","δv_pix_grad",
                "δh_pix_angle","δv_pix_angle"
            };

            foreach (string name in visibleProps)
            {
                var prop = typeof(FirstFileCalculationResult).GetProperty(name);
                if (prop == null) continue;

                var value = prop.GetValue(result);
                if (value == null) continue;

                // Создаём визуальный блок для одного параметра
                var block = new StackPanel
                {
                    Margin = new Thickness(0, 8, 0, 8)
                };

                // Подпись параметра (человеческая)
                block.Children.Add(new TextBlock
                {
                    Text = GetLabel(name),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold
                });

                // Само значение с 5 знаками после запятой
                block.Children.Add(new TextBlock
                {
                    Text = $"{value:0.00000} {GetUnit(name)}",
                    FontSize = 15
                });

                // Линия-разделитель, чтобы визуально всё не сливалось
                block.Children.Add(new Border
                {
                    Height = 1,
                    Background = Brushes.LightGray,
                    Margin = new Thickness(0, 6, 0, 0)
                });

                ResultPanel.Children.Add(block);
            }
        }

        // Безопасный парсинг числа
        // Разрешаем и точку, и запятую
        private double Parse(string t)
        {
            double.TryParse(
                t.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double v);

            return v;
        }

        // Преобразуем техническое имя свойства в нормальную подпись
        private string GetLabel(string name)
        {
            return name switch
            {
                "h" => "Высота матрицы (h)",
                "i" => "Ширина матрицы (i)",
                "d" => "Диагональ матрицы (d)",

                "δh" => "Вертикальный угол поля зрения (δh)",
                "δv" => "Горизонтальный угол поля зрения (δv)",
                "δd" => "Диагональный угол поля зрения (δd)",

                "δh_min" => "Минуты вертикального угла",
                "δv_min" => "Минуты горизонтального угла",
                "δd_min" => "Минуты диагонального угла",

                "δh_pix_grad" => "Угол одного пикселя по вертикали",
                "δv_pix_grad" => "Угол одного пикселя по горизонтали",

                "δh_pix_angle" => "Угол пикселя по вертикали (в секундах)",
                "δv_pix_angle" => "Угол пикселя по горизонтали (в секундах)",

                _ => name
            };
        }

        // Определяем единицу измерения для каждого типа параметра
        private string GetUnit(string name)
        {
            // размеры матрицы и фокусное расстояние — миллиметры
            if (name == "h" || name == "i" || name == "d")
                return "мм";

            // углы пикселя в секундах
            if (name.Contains("pix_angle"))
                return "″";

            // минуты
            if (name.Contains("min"))
                return "′";

            // остальные δ — градусы
            if (name.StartsWith("δ"))
                return "°";

            return "";
        }
    }
}
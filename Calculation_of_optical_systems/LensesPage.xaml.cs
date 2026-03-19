using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Calculation_of_optical_systems
{
    public partial class LensesPage : Page
    {
        private string currentSource = "cctv";

        public LensesPage()
        {
            InitializeComponent();
        }

        // 🔹 Выбор источника
        private void SelectSource(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                BtnCctv.Background = Brushes.LightGray;
                BtnCameraLab.Background = Brushes.LightGray;
                BtnAzimp.Background = Brushes.LightGray;

                switch (btn.Name)
                {
                    case "BtnCctv":
                        currentSource = "cctv";
                        StatusText.Text = "Источник: CCTVLens";
                        break;
                    case "BtnCameraLab":
                        currentSource = "cameralab";
                        StatusText.Text = "Источник: CameraLab";
                        break;
                    case "BtnAzimp":
                        currentSource = "azimp";
                        StatusText.Text = "Источник: Azimp";
                        break;
                }

                btn.Background = Brushes.LightBlue;
            }
        }

        // 🔹 Применение фильтров
        private async void ApplyFilter(object sender, RoutedEventArgs e)
        {
            bool isOffline = OfflineToggle.IsChecked == true;
            StatusText.Text = isOffline ? "Загрузка из файла..." : "Загрузка с сервера...";

            var lenses = await LoadLenses(isOffline);

            var filtered = lenses.Where(l => LensMatchesFilter(l)).ToList();

            StatusText.Text = $"Найдено: {filtered.Count}";

            LensPanel.Children.Clear();

            foreach (var lens in filtered)
                AddLensCard(lens);
        }

        // 🔹 Загрузка линз
        private async Task<List<LensParserPython.Lens>> LoadLenses(bool isOffline)
        {
            switch (currentSource)
            {
                case "cctv":
                    {
                        // Получаем обычный список CCTV
                        var cctvList = await new LensParser().GetCctvLensesAsync(isOffline);

                        // Конвертируем в LensParserPython.Lens
                        var converted = cctvList.Select(l => new LensParserPython.Lens
                        {
                            title = l.Model,
                            link = l.ProductUrl,
                            characteristics = new Dictionary<string, string>
                {
                    { "Фокусное расстояние, мм", l.FocalLength ?? "" },
                    { "Формат сенсора", l.SensorFormat ?? "" },
                    { "ImageUrl", l.ImageUrl ?? "" }
                }
                        }).ToList();

                        return converted;
                    }

                case "cameralab":
                    {
                        // Для CameraLab сразу используем Python
                        return isOffline
                            ? await LensParserPython.LoadFromJsonAsync()
                            : await LensParserPython.UpdateFromPythonAsync();
                    }

                case "azimp":
                    return await LoadAzimpAsync(isOffline);

                default:
                    return new List<LensParserPython.Lens>();
            }
        }

        // 🔹 Загрузка/обновление Azimp через Python
        private async Task<List<LensParserPython.Lens>> LoadAzimpAsync(bool isOffline)
        {
            if (isOffline)
                return await LensParserPython.LoadFromJsonAsync();
            else
                return await LensParserPython.UpdateFromPythonAsync();
        }

        // 🔹 Нормализация строки для фильтра
        private string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";
            return value.ToLower().Replace("/", "").Replace(".", "").Replace(",", "").Replace(" ", "").Trim();
        }

        // 🔹 Проверка фильтра
        private bool LensMatchesFilter(LensParserPython.Lens lens)
        {
            string sensorInput = SensorBox.Text ?? "";
            string focalInput = FocalBox.Text ?? "";

            string normSensorInput = Normalize(sensorInput);
            string normLensSensor = Normalize(lens.characteristics.ContainsKey("Формат сенсора") ? lens.characteristics["Формат сенсора"] : "");

            string normFocalInput = focalInput.ToLower().Trim();
            string normLensFocal = lens.characteristics.ContainsKey("Фокусное расстояние, мм") ? lens.characteristics["Фокусное расстояние, мм"].ToLower() : "";

            if (string.IsNullOrWhiteSpace(normSensorInput) && string.IsNullOrWhiteSpace(normFocalInput))
                return true;

            bool sensorOk = string.IsNullOrWhiteSpace(normSensorInput) || normLensSensor.Contains(normSensorInput);
            bool focalOk = string.IsNullOrWhiteSpace(normFocalInput) || normLensFocal.Contains(normFocalInput);

            return sensorOk && focalOk;
        }

        // 🔹 Отображение карточки линзы
        private void AddLensCard(LensParserPython.Lens lens)
        {
            var card = new Border
            {
                Width = 240,
                Margin = new Thickness(12),
                Padding = new Thickness(14),
                CornerRadius = new CornerRadius(14),
                Background = Brushes.White,
                Cursor = System.Windows.Input.Cursors.Hand,
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 18, ShadowDepth = 3, Opacity = 0.18 }
            };

            card.MouseEnter += (_, __) => card.RenderTransform = new ScaleTransform(1.03, 1.03);
            card.MouseLeave += (_, __) => card.RenderTransform = new ScaleTransform(1, 1);

            var stack = new StackPanel();

            // 🔹 Картинка (если есть)
            if (lens.characteristics.ContainsKey("ImageUrl") && !string.IsNullOrEmpty(lens.characteristics["ImageUrl"]))
            {
                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(lens.characteristics["ImageUrl"]);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();

                    stack.Children.Add(new Border
                    {
                        CornerRadius = new CornerRadius(10),
                        Background = new SolidColorBrush(Color.FromRgb(245, 247, 255)),
                        Padding = new Thickness(6),
                        Child = new Image { Height = 130, Stretch = Stretch.Uniform, Source = image }
                    });
                }
                catch { }
            }

            // 🔹 Заголовок
            stack.Children.Add(new TextBlock
            {
                Text = lens.title,
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                Margin = new Thickness(0, 10, 0, 6),
                TextWrapping = TextWrapping.Wrap
            });

            // 🔹 Характеристики
            foreach (var kv in lens.characteristics)
            {
                stack.Children.Add(new TextBlock { Text = $"{kv.Key}: {kv.Value}" });
            }

            // 🔹 Кнопка открыть
            var openBtn = new Button { Content = "Открыть", Margin = new Thickness(0, 10, 0, 0) };
            openBtn.Click += (_, __) =>
            {
                if (!string.IsNullOrEmpty(lens.link))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = lens.link,
                        UseShellExecute = true
                    });
                }
            };
            stack.Children.Add(openBtn);

            card.Child = stack;
            LensPanel.Children.Add(card);
        }
    }
}
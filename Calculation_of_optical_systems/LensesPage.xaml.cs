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
        // текущий источник
        private string currentSource = "cctv";
        // Добавьте эти строки в начало класса

        public LensesPage()
        {
            InitializeComponent();
        }

        // =========================
        // 🔥 ВЫБОР САЙТА
        // =========================
        private void SelectSource(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                // сброс подсветки
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

        // =========================
        // 🔥 КНОПКА ПОДБОРА
        // =========================
        private async void ApplyFilter(object sender, RoutedEventArgs e)
        {
            bool isOffline = OfflineToggle.IsChecked == true;

            StatusText.Text = isOffline
                ? "Загрузка из файла..."
                : "Загрузка с сервера...";

            var lenses = await LoadLenses(isOffline);

            var filtered = lenses.Where(l => LensMatchesFilter(l)).ToList();

            StatusText.Text = $"Найдено: {filtered.Count}";

            LensPanel.Children.Clear();

            foreach (var lens in filtered)
            {
                AddLensCard(lens);
            }
        }

        // =========================
        // 🔥 ЕДИНАЯ ЗАГРУЗКА
        // =========================
        private async Task<List<LensParser.Lens>> LoadLenses(bool isOffline)
        {
            switch (currentSource)
            {
                case "cctv":
                    return await new LensParser().GetCctvLensesAsync(isOffline);

                case "cameralab":
                    return await new LensParser().GetAzureLensesAsync(isOffline);

                case "azimp":
                    return await LoadFromAzimp(isOffline);

                default:
                    return new List<LensParser.Lens>();
            }
        }


        private async Task<List<LensParser.Lens>> LoadFromAzimp(bool isOffline)
        {
            var parser = new AzimpParser();

            if (isOffline)
            {
                return await parser.LoadFromJsonAsync();
            }

            var lenses = await parser.ParseAllPagesAsync();
            await parser.SaveToJsonAsync(lenses);

            return lenses;
        }

        // =========================
        // 🔍 НОРМАЛИЗАЦИЯ
        // =========================
        private string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return value
                .ToLower()
                .Replace("/", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(" ", "")
                .Trim();
        }

        // =========================
        // 🔍 ФИЛЬТР
        // =========================
        private bool LensMatchesFilter(LensParser.Lens lens)
        {
            string sensorInput = SensorBox.Text ?? "";
            string focalInput = FocalBox.Text ?? "";

            string normalizedSensorInput = Normalize(sensorInput);
            string normalizedLensSensor = Normalize(lens.SensorFormat);

            string normalizedFocalInput = focalInput.ToLower().Trim();
            string normalizedLensFocal = (lens.FocalLength ?? "").ToLower();

            if (string.IsNullOrWhiteSpace(normalizedSensorInput) &&
                string.IsNullOrWhiteSpace(normalizedFocalInput))
                return true;

            bool sensorOk =
                string.IsNullOrWhiteSpace(normalizedSensorInput) ||
                normalizedLensSensor.Contains(normalizedSensorInput);

            bool focalOk =
                string.IsNullOrWhiteSpace(normalizedFocalInput) ||
                normalizedLensFocal.Contains(normalizedFocalInput);

            return sensorOk && focalOk;
        }

        // =========================
        // 🎴 КАРТОЧКА
        // =========================
        private void AddLensCard(LensParser.Lens lens)
        {
            var card = new Border
            {
                Width = 240,
                Margin = new Thickness(12),
                Padding = new Thickness(14),
                CornerRadius = new CornerRadius(14),
                Background = Brushes.White,
                Cursor = System.Windows.Input.Cursors.Hand,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 18,
                    ShadowDepth = 3,
                    Opacity = 0.18
                }
            };

            card.MouseEnter += (_, __) =>
                card.RenderTransform = new ScaleTransform(1.03, 1.03);

            card.MouseLeave += (_, __) =>
                card.RenderTransform = new ScaleTransform(1, 1);

            var stack = new StackPanel();

            if (!string.IsNullOrEmpty(lens.ImageUrl))
            {
                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(lens.ImageUrl);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();

                    stack.Children.Add(new Border
                    {
                        CornerRadius = new CornerRadius(10),
                        Background =
                            new SolidColorBrush(Color.FromRgb(245, 247, 255)),
                        Padding = new Thickness(6),
                        Child = new Image
                        {
                            Height = 130,
                            Stretch = Stretch.Uniform,
                            Source = image
                        }
                    });
                }
                catch
                {
                    // игнор
                }
            }

            stack.Children.Add(new TextBlock
            {
                Text = lens.Model,
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                Margin = new Thickness(0, 10, 0, 6),
                TextWrapping = TextWrapping.Wrap
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"Матрица: {lens.SensorFormat}"
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"Фокусное: {lens.FocalLength}"
            });

            var openBtn = new Button
            {
                Content = "Открыть",
                Margin = new Thickness(0, 10, 0, 0)
            };

            openBtn.Click += (_, __) =>
            {
                if (!string.IsNullOrEmpty(lens.ProductUrl))
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = lens.ProductUrl,
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
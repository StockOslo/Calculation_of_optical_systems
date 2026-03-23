using System;
using System.Net.Http;
using System.Text.Json;
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
        private readonly HttpClient _client;
        private const string API_URL = "http://localhost:5220/api/lenses";

        public LensesPage()
        {
            InitializeComponent();

            var handler = new HttpClientHandler()
            {
                UseProxy = false,
                UseDefaultCredentials = true
            };

            _client = new HttpClient(handler);
        }

        private async void ApplyFilter(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Загрузка...";

            try
            {
                var lenses = await LoadFromApi();

                LensPanel.Children.Clear();

                foreach (var lens in lenses)
                    AddLensCard(lens);

                StatusText.Text = $"Найдено: {lenses.Count}";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка";
                MessageBox.Show(ex.Message);
            }
        }

        private async Task<List<LensDto>> LoadFromApi()
        {
            string source = (SourceBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "cctv";
            string sensor = SensorBox.Text ?? "";
            string focal = FocalBox.Text ?? "";
            string category = (CategoryBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(source))
                query.Add($"source={Uri.EscapeDataString(source)}");

            if (!string.IsNullOrWhiteSpace(sensor))
                query.Add($"sensor={Uri.EscapeDataString(sensor)}");

            if (!string.IsNullOrWhiteSpace(focal))
                query.Add($"focal={Uri.EscapeDataString(focal)}");

            if (!string.IsNullOrWhiteSpace(category))
                query.Add($"category={Uri.EscapeDataString(category)}");

            string url = API_URL + "?" + string.Join("&", query);

            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ошибка API: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();

            // 🔥 ВОТ ГЛАВНЫЙ ФИКС
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<LensDto>>(json, options)
                   ?? new List<LensDto>();
        }

        private void AddLensCard(LensDto lens)
        {
            var card = new Border
            {
                Width = 260,
                Margin = new Thickness(12),
                Padding = new Thickness(16),
                CornerRadius = new CornerRadius(16),
                Background = Brushes.White,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 20,
                    ShadowDepth = 4,
                    Opacity = 0.15
                }
            };

            var stack = new StackPanel();
            // КАРТИНКА
            if (!string.IsNullOrEmpty(lens.ImageUrl))
            {
                try
                {
                    stack.Children.Add(new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(245, 247, 255)),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(8),
                        Child = new Image
                        {
                            Height = 140,
                            Stretch = Stretch.Uniform,
                            Source = new BitmapImage(new Uri(lens.ImageUrl))
                        }
                    });
                }
                catch { }
            }

            // НАЗВАНИЕ
            stack.Children.Add(new TextBlock
            {
                Text = lens.Title ?? "Без названия",
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 10, 0, 8),
                TextWrapping = TextWrapping.Wrap
            });

            // ХАРАКТЕРИСТИКИ
            stack.Children.Add(CreateInfo("Фокус:", lens.Focal));
            stack.Children.Add(CreateInfo("Сенсор:", lens.Sensor));
            stack.Children.Add(CreateInfo("Категория:", lens.Category));

            // КНОПКА
            var btn = new Button
            {
                Content = "Открыть",
                Height = 36,
                Margin = new Thickness(0, 12, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(80, 110, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };

            btn.Click += (_, __) =>
            {
                if (!string.IsNullOrEmpty(lens.Link))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = lens.Link,
                        UseShellExecute = true
                    });
                }
            };

            stack.Children.Add(btn);

            card.Child = stack;
            LensPanel.Children.Add(card);
        }

        private UIElement CreateInfo(string label, string value)
        {
            return new TextBlock
            {
                Text = $"{label} {value ?? "-"}",
                Margin = new Thickness(0, 2, 0, 2),
                Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60))
            };
        }

        public class LensDto
        {
            public string Title { get; set; }
            public string Link { get; set; }
            public string Sensor { get; set; }
            public string Focal { get; set; }
            public string ImageUrl { get; set; }
            public string Category { get; set; }
        }
    }
}
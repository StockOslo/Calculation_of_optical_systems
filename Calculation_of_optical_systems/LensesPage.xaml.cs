using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
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
        private bool _isLoaded = false;

        public LensesPage()
        {
            InitializeComponent();

            _client = new HttpClient(new HttpClientHandler()
            {
                UseProxy = false
            });

            Loaded += PageLoaded;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;

            AzimpIrComboBox.Items.Clear();
            AzimpIrComboBox.Items.Add("Все");
            AzimpIrComboBox.Items.Add("ИК фильтр");
            AzimpIrComboBox.Items.Add("Без ИК фильтра");
            AzimpIrComboBox.SelectedIndex = 0;

            SourceBox.SelectionChanged += SourceChanged;
            SourceChanged(null, null);
        }

        private string GetSource()
        {
            return (SourceBox.SelectedItem as ComboBoxItem)?.Content?.ToString()?.ToLower() ?? "cctv";
        }

        private void SourceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;

            string source = GetSource();

            bool isAzimp = source == "azimp";

            SensorPanel.Visibility = isAzimp ? Visibility.Collapsed : Visibility.Visible;
            FocalPanel.Visibility = isAzimp ? Visibility.Collapsed : Visibility.Visible;
            CategoryPanel.Visibility = isAzimp ? Visibility.Collapsed : Visibility.Visible;

            AzimpPanel.Visibility = isAzimp ? Visibility.Visible : Visibility.Collapsed;
        }

        // =========================
        // 🔥 КНОПКА
        // =========================
        private async void ApplyFilter(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Загрузка...";

            try
            {
                var data = await LoadFromApi();

                // 🔥 ВАЖНО: сначала чистим UI
                LensPanel.Children.Clear();

                foreach (var lens in data)
                    AddCard(lens);

                StatusText.Text = $"Найдено: {data.Count}";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка";
                MessageBox.Show(ex.Message);
            }
        }

        // =========================
        // 🔥 API
        // =========================
        private async Task<List<LensDto>> LoadFromApi()
        {
            string source = GetSource();

            var url = $"{API_URL}?source={source}";

            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API ошибка: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<List<LensDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<LensDto>();

            // 🔥 ФИЛЬТР ТОЛЬКО ДЛЯ AZIMP
            if (source == "azimp")
            {
                string focalInput = AzimpFocalBox?.Text ?? "";
                string search = AzimpSearchBox?.Text?.ToLower() ?? "";
                string irMode = AzimpIrComboBox?.SelectedItem?.ToString() ?? "Все";

                var (userMin, userMax) = ParseFocal(focalInput);

                data = data.Where(l =>
                {
                    string title = (l.Title ?? "").ToLower();

                    // 🔹 поиск
                    bool searchOk = string.IsNullOrWhiteSpace(search) || title.Contains(search);

                    // 🔹 фокус
                    var (min, max) = ParseFocal(title);
                    bool focalOk = string.IsNullOrWhiteSpace(focalInput) ||
                                   (userMax >= min && userMin <= max);

                    // 🔹 ИК
                    bool irOk = true;

                    if (irMode == "ИК фильтр")
                        irOk = HasIr(title);

                    if (irMode == "Без ИК фильтра")
                        irOk = !HasIr(title);

                    return searchOk && focalOk && irOk;
                }).ToList();
            }

            return data;
        }

        // =========================
        // 🔥 ПАРС ФОКУСА (УЛУЧШЕННЫЙ)
        // =========================
        private (double min, double max) ParseFocal(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return (0, double.MaxValue);

            text = text.ToLower()
                .Replace("мм", "")
                .Replace("mm", "")
                .Replace("f=", "")
                .Replace("–", "-");

            var numbers = new string(text
                .Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-')
                .ToArray())
                .Replace(',', '.');

            var parts = numbers.Split('-', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1 && double.TryParse(parts[0], out double val))
                return (val, val);

            if (parts.Length >= 2 &&
                double.TryParse(parts[0], out double min) &&
                double.TryParse(parts[1], out double max))
                return (min, max);

            return (0, double.MaxValue);
        }

        // =========================
        // 🔥 ОПРЕДЕЛЕНИЕ ИК
        // =========================
        private bool HasIr(string text)
        {
            text = text.ToLower();

            return text.Contains("ик") ||
                   text.Contains("ir") ||
                   text.Contains("infrared") ||
                   text.Contains("led") ||
                   text.Contains("night");
        }

        // =========================
        // UI
        // =========================
        private void AddCard(LensDto lens)
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
                    BlurRadius = 15,
                    ShadowDepth = 3,
                    Opacity = 0.2
                }
            };

            var stack = new StackPanel();

            if (!string.IsNullOrEmpty(lens.ImageUrl))
            {
                try
                {
                    stack.Children.Add(new Image
                    {
                        Height = 140,
                        Stretch = Stretch.Uniform,
                        Source = new BitmapImage(new Uri(lens.ImageUrl))
                    });
                }
                catch { }
            }

            stack.Children.Add(new TextBlock
            {
                Text = lens.Title ?? "без названия",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 10, 0, 8),
                TextWrapping = TextWrapping.Wrap
            });

            var btn = new Button
            {
                Content = "Открыть",
                Margin = new Thickness(0, 10, 0, 0)
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

        private TextBlock CreateText(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 2, 0, 2)
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
            public string Source { get; set; }
        }
    }
}
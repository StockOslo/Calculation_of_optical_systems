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

    // флаг загрузки страницы чтобы избежать null ошибок
    private bool _isLoaded = false;

        public LensesPage()
        {
            InitializeComponent();

            var handler = new HttpClientHandler()
            {
                UseProxy = false
            };

            _client = new HttpClient(handler);

            // подписываемся на событие загрузки страницы
            Loaded += PageLoaded;
        }

        // вызывается когда страница полностью загружена
        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;

            // только теперь подписываемся на события
            SourceBox.SelectionChanged += SourceChanged;

            // сразу применяем видимость фильтров
            SourceChanged(null, null);
        }

        // безопасное получение выбранного источника
        private string GetSource()
        {
            return (SourceBox.SelectedItem as ComboBoxItem)?.Content?.ToString()?.ToLower() ?? "cctv";
        }

        // переключение фильтров
        private void SourceChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isLoaded) return;

            string source = GetSource();

            if (source == "azimp")
            {
                SensorPanel.Visibility = Visibility.Collapsed;
                FocalPanel.Visibility = Visibility.Collapsed;
                CategoryPanel.Visibility = Visibility.Collapsed;

                AzimpPanel.Visibility = Visibility.Visible;
            }
            else
            {
                SensorPanel.Visibility = Visibility.Visible;
                FocalPanel.Visibility = Visibility.Visible;
                CategoryPanel.Visibility = Visibility.Visible;

                AzimpPanel.Visibility = Visibility.Collapsed;
            }
        }

        // кнопка поиска
        private async void ApplyFilter(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Загрузка...";

            try
            {
                var lenses = await LoadFromApi();

                LensPanel.Children.Clear();

                foreach (var lens in lenses)
                    AddCard(lens);

                StatusText.Text = $"Найдено: {lenses.Count}";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка";
                MessageBox.Show(ex.Message);
            }
        }

        // загрузка данных с api
        private async Task<List<LensDto>> LoadFromApi()
        {
            string source = GetSource();

            var query = new List<string>
        {
            $"source={Uri.EscapeDataString(source)}"
        };

            // безопасное получение категории
            string category = "";
            if (CategoryBox?.SelectedItem is ComboBoxItem item && item.Content != null)
                category = item.Content.ToString();

            // обычные фильтры
            if (source != "azimp")
            {
                if (!string.IsNullOrWhiteSpace(SensorBox?.Text))
                    query.Add($"sensor={Uri.EscapeDataString(SensorBox.Text)}");

                if (!string.IsNullOrWhiteSpace(FocalBox?.Text))
                    query.Add($"focal={Uri.EscapeDataString(FocalBox.Text)}");

                if (!string.IsNullOrWhiteSpace(category))
                    query.Add($"category={Uri.EscapeDataString(category)}");
            }

            string url = API_URL + "?" + string.Join("&", query);

            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API ошибка: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<List<LensDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<LensDto>();

            // фильтрация azimp по тексту
            if (source == "azimp")
            {
                string focal = AzimpFocalBox?.Text?.ToLower() ?? "";
                string ir = AzimpIrBox?.Text?.ToLower() ?? "";
                string search = AzimpSearchBox?.Text?.ToLower() ?? "";

                data = data.Where(l =>
                {
                    string title = (l.Title ?? "").ToLower();

                    return (string.IsNullOrWhiteSpace(focal) || title.Contains(focal)) &&
                           (string.IsNullOrWhiteSpace(ir) || title.Contains(ir)) &&
                           (string.IsNullOrWhiteSpace(search) || title.Contains(search));
                }).ToList();
            }

            return data;
        }

        // универсальная карточка
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

            // картинка
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

            // название
            stack.Children.Add(new TextBlock
            {
                Text = lens.Title ?? "без названия",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 10, 0, 8),
                TextWrapping = TextWrapping.Wrap
            });

            // характеристики (не для azimp)
            if ((lens.Source ?? "").ToLower() != "python")
            {
                stack.Children.Add(CreateText($"Фокус: {lens.Focal}"));
                stack.Children.Add(CreateText($"Сенсор: {lens.Sensor}"));
                stack.Children.Add(CreateText($"Категория: {lens.Category}"));
            }

            // кнопка
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

        // dto
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

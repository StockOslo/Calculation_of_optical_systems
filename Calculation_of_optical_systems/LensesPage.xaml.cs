using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Calculation_of_optical_systems
{
    public partial class LensesPage : Page
    {
        public LensesPage()
        {
            InitializeComponent();
        }


        // КНОПКА ПОДОБРАТЬ

        private async void ApplyFilter(object sender, RoutedEventArgs e)
        {
            SearchButton.IsEnabled = false;
            StatusText.Text = "Загрузка объективов...";

            LensPanel.Children.Clear();

            var parser = new LensParser();
            int count = 0;

            try
            {
                await foreach (var lens in parser.ParseLensesAsync())
                {
                    if (!LensMatchesFilter(lens))
                        continue;

                    AddLensCard(lens);

                    count++;
                    StatusText.Text = $"Загружено: {count}";
                }

                StatusText.Text = $"Готово. Найдено: {count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            SearchButton.IsEnabled = true;
        }


        // ФИЛЬТР

        private bool LensMatchesFilter(LensParser.Lens lens)
        {
            string sensor = SensorBox.Text?.ToLower().Trim() ?? "";
            string focal = FocalBox.Text?.ToLower().Trim() ?? "";

            if (string.IsNullOrWhiteSpace(sensor) &&
                string.IsNullOrWhiteSpace(focal))
                return true;

            bool sensorOk =
                string.IsNullOrWhiteSpace(sensor) ||
                (lens.SensorFormat ?? "").ToLower().Contains(sensor);

            bool focalOk =
                string.IsNullOrWhiteSpace(focal) ||
                (lens.FocalLength ?? "").ToLower().Contains(focal);

            return sensorOk && focalOk;
        }



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
                card.RenderTransform =
                    new ScaleTransform(1.03, 1.03);

            card.MouseLeave += (_, __) =>
                card.RenderTransform =
                    new ScaleTransform(1, 1);

            var stack = new StackPanel();

            if (!string.IsNullOrEmpty(lens.ImageUrl))
            {
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
                        Source = new BitmapImage(new Uri(lens.ImageUrl))
                    }
                });
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
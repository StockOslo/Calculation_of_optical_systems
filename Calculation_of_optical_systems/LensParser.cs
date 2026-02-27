using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Calculation_of_optical_systems
{
    public class LensParser
    {
        public class Lens
        {
            public string Model { get; set; }
            public string SensorFormat { get; set; }
            public string FocalLength { get; set; }
        }

        public async IAsyncEnumerable<Lens> ParseLensesAsync()
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            using var writer = new StreamWriter("objectivy.csv", false);
            await writer.WriteLineAsync("Модель;Формат матрицы;Фокусное расстояние");

            int page = 1;

            while (true)
            {
                string url = page == 1
                    ? "https://cctvlens.ru/catalog/?s=объективы"
                    : $"https://cctvlens.ru/catalog/page/{page}/?s=объективы";

                Console.WriteLine($"Страница {page}");

                string html;
                try
                {
                    html = await client.GetStringAsync(url);
                }
                catch
                {
                    yield break;
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var productNodes = doc.DocumentNode
                    .SelectNodes("//ul[@id='products_feed']//li[contains(@class,'product')]");

                if (productNodes == null || productNodes.Count == 0)
                    yield break;

                foreach (var product in productNodes)
                {
                    var lens = new Lens();

                    var modelNode = product.SelectSingleNode(
                        ".//span[text()='Модель:']/following-sibling::span[@class='attribute-value']/a");

                    if (modelNode != null)
                        lens.Model = modelNode.InnerText.Trim();

                    var sensorNode = product.SelectSingleNode(
                        ".//span[text()='Формат матрицы:']/following-sibling::span[@class='attribute-value']/a");

                    if (sensorNode != null)
                        lens.SensorFormat = sensorNode.InnerText.Trim();

                    var focalNode = product.SelectSingleNode(
                        ".//span[text()='Фокусное расстояние:']/following-sibling::span[@class='attribute-value']/a");

                    if (focalNode != null)
                        lens.FocalLength = focalNode.InnerText.Trim();

                    if (!string.IsNullOrEmpty(lens.Model))
                    {
                        // сразу пишем в CSV
                        await writer.WriteLineAsync(
                            $"{lens.Model};{lens.SensorFormat};{lens.FocalLength}");

                        await writer.FlushAsync();

                        // и сразу отдаём в UI
                        yield return lens;
                    }
                }

                page++;
                await Task.Delay(800); // мягкая пауза
            }
        }
    }
}
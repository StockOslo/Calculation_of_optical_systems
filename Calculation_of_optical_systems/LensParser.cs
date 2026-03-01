using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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

            public string ProductUrl { get; set; }
            public string ImageUrl { get; set; }
        }


        public async IAsyncEnumerable<Lens> ParseLensesAsync()
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            client.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            var parsedLenses = new List<Lens>();

            int page = 1;

            while (true)
            {
                string url = page == 1
                    ? "https://cctvlens.ru/catalog/?s=объективы"
                    : $"https://cctvlens.ru/catalog/page/{page}/?s=объективы";

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
                    break;

                foreach (var product in productNodes)
                {
                    var lens = new Lens();


                    // MODEL

                    var modelNode = product.SelectSingleNode(
                        ".//span[text()='Модель:']/following-sibling::span[@class='attribute-value']/a");

                    lens.Model = modelNode?.InnerText.Trim();


                    // SENSOR FORMAT

                    var sensorNode = product.SelectSingleNode(
                        ".//span[text()='Формат матрицы:']/following-sibling::span[@class='attribute-value']/a");

                    lens.SensorFormat = sensorNode?.InnerText.Trim();


                    // FOCAL LENGTH

                    var focalNode = product.SelectSingleNode(
                        ".//span[text()='Фокусное расстояние:']/following-sibling::span[@class='attribute-value']/a");

                    lens.FocalLength = focalNode?.InnerText.Trim();


                    // PRODUCT LINK

                    var linkNode =
                        product.SelectSingleNode(".//a[contains(@class,'woocommerce-LoopProduct-link')]");

                    if (linkNode != null)
                        lens.ProductUrl =
                            linkNode.GetAttributeValue("href", "");


                    // IMAGE LINK

                    var imgNode = product.SelectSingleNode(".//img");

                    if (imgNode != null)
                        lens.ImageUrl =
                            imgNode.GetAttributeValue("src", "");

   
                    // ADD RESULT

                    if (!string.IsNullOrWhiteSpace(lens.Model))
                    {
                        parsedLenses.Add(lens);
                        yield return lens;
                    }
                }

                page++;
                await Task.Delay(700); // мягкая пауза
            }


            await SaveToJsonAsync(parsedLenses);
        }



        private async Task SaveToJsonAsync(List<Lens> newLenses)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            List<Lens> existing = new();

            if (File.Exists("objectivy.json"))
            {
                try
                {
                    string oldJson =
                        await File.ReadAllTextAsync("objectivy.json");

                    existing =
                        JsonSerializer.Deserialize<List<Lens>>(oldJson)
                        ?? new List<Lens>();
                }
                catch
                {
                    existing = new List<Lens>();
                }
            }

            // объединяем старые и новые
            existing.AddRange(newLenses);

            // удаляем дубликаты
            var merged = existing
                .GroupBy(l => $"{l.Model}_{l.FocalLength}")
                .Select(g => g.First())
                .ToList();

            string json =
                JsonSerializer.Serialize(merged, jsonOptions);

            await File.WriteAllTextAsync("objectivy.json", json);
        }
    }
}
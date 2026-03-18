using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Calculation_of_optical_systems
{
    public class LensParser
    {
        private readonly string CctvFile = "cctv_lenses.json";
        private readonly string AzureFile = "azure_lenses.json";

        // Модель объектива, в которую мы сохраняем данные из API
        public class Lens
        {
            


            public string Model { get; set; }
            public string SensorFormat { get; set; }
            public string FocalLength { get; set; }
            public string ProductUrl { get; set; }
            public string ImageUrl { get; set; }
        }

        public async Task<List<Lens>> GetCctvLensesAsync(bool offlineMode)
        {
            if (offlineMode)
            {
                Console.WriteLine("CCTV ОФЛАЙН");
                return await LoadFromJsonAsync(CctvFile);
            }

            try
            {
                Console.WriteLine("CCTV ОНЛАЙН");

                var lenses = await ParseFromApiAsync();

                await SaveToJsonAsync(lenses, CctvFile);

                return lenses;
            }
            catch
            {
                Console.WriteLine("CCTV fallback → офлайн");
                return await LoadFromJsonAsync(CctvFile);
            }
        }

        // =====================================================
        // 🔥 AZURE (CameraLab)
        // =====================================================
        public async Task<List<Lens>> GetAzureLensesAsync(bool offlineMode)
        {
            if (offlineMode)
            {
                Console.WriteLine("Azure ОФЛАЙН");
                return await LoadFromJsonAsync(AzureFile);
            }

            try
            {
                Console.WriteLine("Azure ОНЛАЙН");

                var parser = new AzureParser();
                var lenses = await parser.ParseAsync();

                await SaveToJsonAsync(lenses, AzureFile);

                return lenses;
            }
            catch
            {
                Console.WriteLine("Azure fallback → офлайн");
                return await LoadFromJsonAsync(AzureFile);
            }
        }

        // =====================================================
        // 🔥 API CCTV
        // =====================================================
        private async Task<List<Lens>> ParseFromApiAsync()
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var result = new List<Lens>();

            int page = 1;
            int perPage = 100;

            while (true)
            {
                string url = $"https://cctvlens.ru/wp-json/wc/store/products?page={page}&per_page={perPage}";
                Console.WriteLine($"CCTV страница {page}");

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    break;

                var json = await response.Content.ReadAsStringAsync();

                var products = JsonSerializer.Deserialize<List<JsonElement>>(json);

                if (products == null || products.Count == 0)
                    break;

                foreach (var product in products)
                {
                    var lens = ParseLens(product);

                    if (!string.IsNullOrWhiteSpace(lens.Model))
                        result.Add(lens);
                }

                page++;
                await Task.Delay(150);
            }

            Console.WriteLine($"CCTV получено: {result.Count}");

            return RemoveDuplicates(result);
        }

        // =====================================================
        // 🔍 ПАРСИНГ API ТОВАРА
        // =====================================================
        private Lens ParseLens(JsonElement product)
        {
            var lens = new Lens();

            if (product.TryGetProperty("name", out var name))
                lens.Model = name.GetString();

            if (product.TryGetProperty("permalink", out var link))
                lens.ProductUrl = link.GetString();

            if (product.TryGetProperty("images", out var images) &&
                images.ValueKind == JsonValueKind.Array &&
                images.GetArrayLength() > 0)
            {
                var img = images[0];
                if (img.TryGetProperty("src", out var src))
                    lens.ImageUrl = src.GetString();
            }

            if (product.TryGetProperty("attributes", out var attributes) &&
                attributes.ValueKind == JsonValueKind.Array)
            {
                foreach (var attr in attributes.EnumerateArray())
                {
                    if (!attr.TryGetProperty("name", out var attrNameProp))
                        continue;

                    string attrName = attrNameProp.GetString();

                    if (!attr.TryGetProperty("terms", out var terms) ||
                        terms.GetArrayLength() == 0)
                        continue;

                    var term = terms[0];

                    if (!term.TryGetProperty("name", out var valueProp))
                        continue;

                    string value = valueProp.GetString();

                    if (attrName.Contains("Фокус"))
                        lens.FocalLength = value;

                    if (attrName.Contains("Формат"))
                        lens.SensorFormat = value;
                }
            }

            return lens;
        }

        public async Task<List<Lens>> LoadFromJsonAsync(string path)
        {
            if (!File.Exists(path))
                return new List<Lens>();

            try
            {
                string json = await File.ReadAllTextAsync(path);
                return JsonSerializer.Deserialize<List<Lens>>(json)
                       ?? new List<Lens>();
            }
            catch
            {
                return new List<Lens>();
            }
        }

        public async Task SaveToJsonAsync(List<Lens> lenses, string path)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(lenses, options);

            await File.WriteAllTextAsync(path, json);
        }

        // =====================================================
        // 🧹 УДАЛЕНИЕ ДУБЛИКАТОВ
        // =====================================================
        private List<Lens> RemoveDuplicates(List<Lens> lenses)
        {
            return lenses
                .Where(l => !string.IsNullOrWhiteSpace(l.Model))
                .GroupBy(l => $"{l.Model?.Trim().ToLower()}_{l.FocalLength?.Trim().ToLower()}")
                .Select(g => g.First())
                .ToList();
        }
    }
}
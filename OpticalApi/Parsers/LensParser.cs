using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OpticalApi
{
    public class LensParser
    {
        public readonly string CctvFile = "cctv_lenses.json";
        public readonly string AzureFile = "azure_lenses.json";

        public class Lens
        {
            public string Model { get; set; }
            public string SensorFormat { get; set; }
            public string FocalLength { get; set; }
            public string ProductUrl { get; set; }
            public string ImageUrl { get; set; }
        }

        // =====================================================
        // CCTV
        // =====================================================
        public async Task<List<Lens>> GetCctvLensesAsync(bool offlineMode)
        {
            if (offlineMode)
                return await LoadFromJsonAsync(CctvFile);

            try
            {
                var lenses = await ParseFromApiAsync();
                await SaveToJsonAsync(lenses, CctvFile);
                return lenses;
            }
            catch
            {
                return await LoadFromJsonAsync(CctvFile);
            }
        }

        private async Task<List<Lens>> ParseFromApiAsync()
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var result = new List<Lens>();
            int page = 1, perPage = 100;

            while (true)
            {
                string url = $"https://cctvlens.ru/wp-json/wc/store/products?page={page}&per_page={perPage}";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) break;

                var json = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<JsonElement>>(json);
                if (products == null || products.Count == 0) break;

                foreach (var product in products)
                {
                    var lens = ParseLens(product);
                    if (!string.IsNullOrWhiteSpace(lens.Model))
                        result.Add(lens);
                }

                page++;
                await Task.Delay(150);
            }

            return RemoveDuplicates(result);
        }

        private Lens ParseLens(JsonElement product)
        {
            var lens = new Lens();

            if (product.TryGetProperty("name", out var name)) lens.Model = name.GetString();
            if (product.TryGetProperty("permalink", out var link)) lens.ProductUrl = link.GetString();
            if (product.TryGetProperty("images", out var images) && images.ValueKind == JsonValueKind.Array && images.GetArrayLength() > 0)
            {
                var img = images[0];
                if (img.TryGetProperty("src", out var src)) lens.ImageUrl = src.GetString();
            }
            if (product.TryGetProperty("attributes", out var attributes) && attributes.ValueKind == JsonValueKind.Array)
            {
                foreach (var attr in attributes.EnumerateArray())
                {
                    if (!attr.TryGetProperty("name", out var attrNameProp)) continue;
                    string attrName = attrNameProp.GetString();
                    if (!attr.TryGetProperty("terms", out var terms) || terms.GetArrayLength() == 0) continue;
                    var term = terms[0];
                    if (!term.TryGetProperty("name", out var valueProp)) continue;
                    string value = valueProp.GetString();

                    if (attrName.Contains("Фокус")) lens.FocalLength = value;
                    if (attrName.Contains("Формат")) lens.SensorFormat = value;
                }
            }

            return lens;
        }

        // =====================================================
        // CameraLab / Azure через Python
        // =====================================================
        public async Task<List<Lens>> GetAzureLensesAsync(bool offlineMode)
        {
            var parser = new PythonParser
            {
                PythonExe = "python",       // или полный путь
                ScriptPath = "azure_parser.py",   // твой скрипт
                JsonFile = AzureFile
            };

            if (offlineMode)
            {
                if (!File.Exists(AzureFile)) return new List<Lens>();
                string json = await File.ReadAllTextAsync(AzureFile);
                return JsonSerializer.Deserialize<List<Lens>>(json) ?? new List<Lens>();
            }

            return await parser.ParseAsync();
        }

        // =====================================================
        // JSON методы
        // =====================================================
        public async Task<List<Lens>> LoadFromJsonAsync(string path)
        {
            if (!File.Exists(path)) return new List<Lens>();
            try
            {
                string json = await File.ReadAllTextAsync(path);
                return JsonSerializer.Deserialize<List<Lens>>(json) ?? new List<Lens>();
            }
            catch { return new List<Lens>(); }
        }

        public async Task SaveToJsonAsync(List<Lens> lenses, string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(lenses, options);
            await File.WriteAllTextAsync(path, json);
        }

        private List<Lens> RemoveDuplicates(List<Lens> lenses)
        {
            return lenses
                .Where(l => !string.IsNullOrWhiteSpace(l.Model))
                .GroupBy(l => $"{l.Model?.Trim().ToLower()}_{l.FocalLength?.Trim().ToLower()}")
                .Select(g => g.First())
                .ToList();
        }
    }

    // =====================================================
    // Класс для запуска Python
    // =====================================================
    public class PythonParser
    {
        public string PythonExe { get; set; }
        public string ScriptPath { get; set; }
        public string JsonFile { get; set; }

        public async Task<List<LensParser.Lens>> ParseAsync()
        {
            var psi = new ProcessStartInfo
            {
                FileName = PythonExe,
                Arguments = $"\"{ScriptPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            if (process == null) return new List<LensParser.Lens>();

            string stdout = await process.StandardOutput.ReadToEndAsync();
            string stderr = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(stderr))
                Console.WriteLine("Python parser error: " + stderr);

            Console.WriteLine(stdout);

            if (!File.Exists(JsonFile)) return new List<LensParser.Lens>();

            string json = await File.ReadAllTextAsync(JsonFile);
            return JsonSerializer.Deserialize<List<LensParser.Lens>>(json) ?? new List<LensParser.Lens>();
        }
    }
}
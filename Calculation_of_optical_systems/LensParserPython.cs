using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Calculation_of_optical_systems
{
    public class LensParserPython
    {
        public class Lens
        {
            public string title { get; set; }
            public string link { get; set; }
            public Dictionary<string, string> characteristics { get; set; } = new();
        }

        private static readonly string JsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "azure_lenses.json");
        private static readonly string PythonScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "azure_parser.py");

        // 🔹 Загрузка данных из JSON (офлайн)
        public static async Task<List<Lens>> LoadFromJsonAsync()
        {
            if (!File.Exists(JsonFile))
                return new List<Lens>();

            try
            {
                string json = await File.ReadAllTextAsync(JsonFile);
                return JsonSerializer.Deserialize<List<Lens>>(json) ?? new List<Lens>();
            }
            catch
            {
                return new List<Lens>();
            }
        }

        // 🔹 Обновление данных через запуск Python-скрипта
        public static async Task<List<Lens>> UpdateFromPythonAsync()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = PythonScript,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync();
            }

            // После завершения Python скрипта читаем JSON
            return await LoadFromJsonAsync();
        }
    }
}
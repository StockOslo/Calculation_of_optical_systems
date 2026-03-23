using System.Diagnostics;
using System.Text.Json;

namespace OpticalApi.Parsers
{
    public class LensParserPython
    {
        public class Lens
        {
            public string? title { get; set; }
            public string? link { get; set; }
            public Dictionary<string, string> characteristics { get; set; } = new();
        }

        private static string BasePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Parsers");

        private static readonly string JsonFile = Path.Combine(BasePath, "azure_lenses.json");
        private static readonly string PythonScript = Path.Combine(BasePath, "azure_parser.py");

        private static readonly string AzimpJsonFile = Path.Combine(BasePath, "azimp_lenses.json");
        private static readonly string AzimpPythonScript = Path.Combine(BasePath, "azimp_parser.py");

        public static async Task<List<Lens>> LoadFromJsonAsync()
        {
            if (!File.Exists(JsonFile))
                return new List<Lens>();

            string json = await File.ReadAllTextAsync(JsonFile);
            return JsonSerializer.Deserialize<List<Lens>>(json) ?? new List<Lens>();
        }

        public static async Task<List<Lens>> UpdateFromPythonAsync()
        {
            await RunPython(PythonScript);
            return await LoadFromJsonAsync();
        }

        public static async Task<List<Lens>> LoadAzimpFromJsonAsync()
        {
            if (!File.Exists(AzimpJsonFile))
                return new List<Lens>();

            string json = await File.ReadAllTextAsync(AzimpJsonFile);
            return JsonSerializer.Deserialize<List<Lens>>(json) ?? new List<Lens>();
        }

        public static async Task<List<Lens>> UpdateAzimpFromPythonAsync()
        {
            await RunPython(AzimpPythonScript);
            return await LoadAzimpFromJsonAsync();
        }

        private static async Task RunPython(string scriptPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);

            if (process != null)
            {
                string stderr = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (!string.IsNullOrWhiteSpace(stderr))
                    Console.WriteLine("Python error: " + stderr);
            }
        }
    }
}
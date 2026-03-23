using OpticalApi.Models;
using OpticalApi.Parsers;

namespace OpticalApi.Services
{
    public class LensService
    {
        private readonly LensParser _parser = new();

        public async Task<List<LensDto>> GetLenses(string source, bool offline)
        {
            return source switch
            {
                "cctv" => await GetCctv(offline),
                "cameralab" => await GetCameraLab(),
                "azimp" => await GetAzimp(),
                _ => new List<LensDto>()
            };
        }

        private async Task<List<LensDto>> GetCctv(bool offline)
        {
            var data = await _parser.GetCctvLensesAsync(offline);

            return data.Select(l => new LensDto
            {
                Title = l.Model,
                Link = l.ProductUrl,
                Sensor = l.SensorFormat,
                Focal = l.FocalLength,
                ImageUrl = l.ImageUrl,
                Source = "CCTV",
                Category = DetectCategory(l.FocalLength)
            }).ToList();
        }

        private async Task<List<LensDto>> GetCameraLab()
        {
            var data = await LensParserPython.LoadFromJsonAsync();
            return data.Select(MapPython).ToList();
        }

        private async Task<List<LensDto>> GetAzimp()
        {
            var data = await LensParserPython.LoadAzimpFromJsonAsync();
            return data.Select(MapPython).ToList();
        }

        private LensDto MapPython(LensParserPython.Lens l)
        {
            string focal = l.characteristics.GetValueOrDefault("Фокусное расстояние, мм");

            return new LensDto
            {
                Title = l.title,
                Link = l.link,
                Sensor = l.characteristics.GetValueOrDefault("Формат сенсора"),
                Focal = focal,
                ImageUrl = l.characteristics.GetValueOrDefault("ImageUrl"),
                Source = "Python",
                Category = DetectCategory(focal)
            };
        }

        private string DetectCategory(string focal)
        {
            if (string.IsNullOrEmpty(focal)) return "Неизвестно";
            return focal.Contains("-") ? "Вариофокал" : "Фикс";
        }
    }
}
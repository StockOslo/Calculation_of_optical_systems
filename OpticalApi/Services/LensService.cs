using OpticalApi.Models;
using OpticalApi.Parsers;

namespace OpticalApi.Services
{
    public class LensService
    {
        private readonly LensParser _parser = new(); // основной парсер для cctv

        public async Task<List<LensDto>> GetLenses(string source, bool offline)
        {
            // выбираем источник данных
            return source switch
            {
                "cctv" => await GetCctv(offline), // cctv может работать онлайн и оффлайн
                "cameralab" => await GetCameraLab(), // только json
                "azimp" => await GetAzimp(), // только json
                _ => new List<LensDto>()
            };
        }

        private async Task<List<LensDto>> GetCctv(bool offline)
        {
            // получаем данные через c# парсер
            var data = await _parser.GetCctvLensesAsync(offline);

            // преобразуем в dto для api
            return data.Select(l => new LensDto
            {
                Title = l.Model,
                Link = l.ProductUrl,
                Sensor = l.SensorFormat,
                Focal = l.FocalLength,
                ImageUrl = l.ImageUrl,
                Source = "CCTV",
                Category = DetectCategory(l.FocalLength) // определяем тип линзы
            }).ToList();
        }

        private async Task<List<LensDto>> GetCameraLab()
        {
            // читаем готовый json (python уже все сделал)
            var data = await LensParserPython.LoadFromJsonAsync();
            return data.Select(MapPython).ToList();
        }

        private async Task<List<LensDto>> GetAzimp()
        {
            // аналогично cameralab
            var data = await LensParserPython.LoadAzimpFromJsonAsync();
            return data.Select(MapPython).ToList();
        }

        private LensDto MapPython(LensParserPython.Lens l)
        {
            // достаем фокусное расстояние из словаря
            string focal = l.characteristics.GetValueOrDefault("Фокусное расстояние, мм");

            return new LensDto
            {
                Title = l.title,
                Link = l.link,
                Sensor = l.characteristics.GetValueOrDefault("Формат сенсора"),
                Focal = focal,
                ImageUrl = l.characteristics.GetValueOrDefault("ImageUrl"),
                Source = "Python",
                Category = DetectCategory(focal) // определяем фикс или вариофокал
            };
        }

        private string DetectCategory(string focal)
        {
            // простая логика: если есть диапазон значит вариофокал
            if (string.IsNullOrEmpty(focal)) return "Неизвестно";

            return focal.Contains("-") ? "Вариофокал" : "Фикс";
        }
    }
}
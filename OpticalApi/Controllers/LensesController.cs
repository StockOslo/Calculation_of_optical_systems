using Microsoft.AspNetCore.Mvc;
using OpticalApi.Parsers;
using OpticalApi.Services;

namespace OpticalApi.Controllers
{
    [ApiController]
    [Route("api/lenses")]
    public class LensesController : ControllerBase
    {
        private readonly LensService _service; // сервис который тянет данные

        public LensesController(LensService service)
        {
            _service = service; // внедрение зависимости
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            string source = "cctv",
            bool offline = false,
            string sensor = "",
            string category = "",
            string focal = "")
        {
            // получаем список линз из выбранного источника
            var lenses = await _service.GetLenses(source, offline);

            // фильтруем по параметрам если они заданы
            var filtered = lenses.Where(l =>
                (string.IsNullOrEmpty(sensor) || (l.Sensor ?? "").Contains(sensor)) && // фильтр по сенсору
                (string.IsNullOrEmpty(category) || (l.Category ?? "") == category) && // фильтр по категории
                (string.IsNullOrEmpty(focal) || (l.Focal ?? "").Contains(focal)) // фильтр по фокусному
            );

            // возвращаем результат клиенту
            return Ok(filtered);
        }
    }
}
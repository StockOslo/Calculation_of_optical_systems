using Microsoft.AspNetCore.Mvc;
using OpticalApi.Parsers;
using OpticalApi.Services;

namespace OpticalApi.Controllers
{
    [ApiController]
    [Route("api/lenses")]
    public class LensesController : ControllerBase
    {
        private readonly LensService _service;

        public LensesController(LensService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            string source = "cctv",
            bool offline = false,
            string sensor = "",
            string category = "",
            string focal = "")
        {
            var lenses = await _service.GetLenses(source, offline);

            var filtered = lenses.Where(l =>
                (string.IsNullOrEmpty(sensor) || (l.Sensor ?? "").Contains(sensor)) &&
                (string.IsNullOrEmpty(category) || (l.Category ?? "") == category) &&
                (string.IsNullOrEmpty(focal) || (l.Focal ?? "").Contains(focal))
            );

            return Ok(filtered);
        }
    }
}
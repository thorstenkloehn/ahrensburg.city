using Microsoft.AspNetCore.Mvc;

namespace mvc.Controllers
{
    public class MapController : Controller
    {
        private readonly IConfiguration _configuration;

        public MapController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("karte")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Karte";
            ViewBag.TileUrl = _configuration["TileServerUrl"] ?? "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
            return View();
        }
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using mvc.Models;

namespace mvc.Controllers;

[Route("[controller]/[action]")]
public class HomeController : Controller
{
    [HttpGet] // Matches /Home/Index
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet] // Matches /Home/Privacy
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet] // Matches /Home/Error
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

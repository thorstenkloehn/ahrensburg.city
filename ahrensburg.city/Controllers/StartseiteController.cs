using Microsoft.AspNetCore.Mvc;

namespace ahrensburg.city.Controllers
{
    public class StartseiteController : Controller
    {
        // GET: StartseiteController
        public ActionResult Index()
        {
            return View();
        }

    }
}

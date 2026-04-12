using Microsoft.AspNetCore.Mvc;

namespace KitapTakasPlatformu.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
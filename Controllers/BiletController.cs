using Microsoft.AspNetCore.Mvc;

namespace KonserBiletim.Controllers
{
    public class BiletController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Biletim()
        {
            return View();
        }
    }
}

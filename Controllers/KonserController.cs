using Microsoft.AspNetCore.Mvc;

namespace KonserBiletim.Controllers
{
    public class KonserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

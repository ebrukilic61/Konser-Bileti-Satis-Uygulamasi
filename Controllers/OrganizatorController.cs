using Microsoft.AspNetCore.Mvc;

namespace KonserBiletim.Controllers
{
    public class OrganizatorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();
        }
    }
}

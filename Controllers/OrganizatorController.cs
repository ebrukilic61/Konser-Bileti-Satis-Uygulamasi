using KonserBiletim.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KonserBiletim.Controllers
{
    [Authorize(Roles = "Organizator")]
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

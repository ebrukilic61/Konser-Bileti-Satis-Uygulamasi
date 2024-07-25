using Microsoft.AspNetCore.Mvc;

namespace KonserBiletim.Controllers
{
    public class SepetController : Controller
    {
        public IActionResult Index()
        {
            //var cart = HttpContext.Session.GetObjectFromJson<Cart>("Cart") ?? new Cart();
            //return View(cart);
            return View();
        }


    }
}

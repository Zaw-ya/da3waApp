using Microsoft.AspNetCore.Mvc;

namespace Da3wa.WebUI.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace Project2_DBFirst.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

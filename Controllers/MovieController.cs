using Microsoft.AspNetCore.Mvc;

namespace Film_website.Controllers
{
    public class MovieController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

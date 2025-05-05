using Microsoft.AspNetCore.Mvc;

namespace CarServer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}

using CattleFarm.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CattleFarm.Controllers
{
    public class HomeController : Controller
    {
        // Public landing page — authenticated users go to dashboard
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            return View("Landing");
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

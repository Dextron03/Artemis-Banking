using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Controllers
{
    public class DashboardCajeroController  : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

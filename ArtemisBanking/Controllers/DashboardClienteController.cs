using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Controllers
{
    public class DashboardClienteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

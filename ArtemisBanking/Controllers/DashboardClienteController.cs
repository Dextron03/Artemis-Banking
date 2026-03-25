using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class DashboardClienteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

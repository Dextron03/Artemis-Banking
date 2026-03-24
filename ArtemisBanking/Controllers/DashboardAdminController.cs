using Application.Interfaces;
using Application.ViewModels.DashboardAdmin;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class DashboardAdminController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly IMapper _mapper;

        public DashboardAdminController(IDashboardService dashboardService, IMapper mapper)
        {
            _dashboardService = dashboardService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _dashboardService.GetDashboardAsync();

            if (data == null)
            {
                return View(new DashboardAdminViewModel());
            }

            var vm = _mapper.Map<DashboardAdminViewModel>(data);

            return View(vm);
        }
    }
}

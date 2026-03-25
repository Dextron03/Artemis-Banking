using Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardAdminDto> GetDashboardAsync();
        Task<DashboardCashierDto> GetCashierDashboardAsync(string userId);
    }
}

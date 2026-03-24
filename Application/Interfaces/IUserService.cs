using Infrastructure.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<AppUser?> GetByIdAsync(string userId);
        Task<List<AppUser>> GetActiveClientsAsync();
        Task<bool> IsUserActiveAsync(string userId);
    }
}

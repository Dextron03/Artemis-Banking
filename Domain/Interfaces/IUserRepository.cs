using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> IsUserActiveAsync(string userId);
        Task SetUserActiveAsync(string userId, bool isActive);
    }
}

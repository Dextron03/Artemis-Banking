using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ISavingsAccountRepository : IGenericRepository<SavingsAccount>
    {
        Task<IEnumerable<SavingsAccount>> GetAllWithUserAsync();
        Task<SavingsAccount> GetByAccountNumberAsync(string accountNumber);
        Task<IEnumerable<SavingsAccount>> GetByUserIdAsync(string userId);
        Task<SavingsAccount> GetPrincipalAccountByUserIdAsync(string userId);
        Task<bool> AccountNumberExistsAsync(string accountNumber);
        Task<SavingsAccount> GetByIdWithTransactionsAsync(string id);
        Task<SavingsAccount> GetPrincipalByUserIdAsync(string userId);
    }
}

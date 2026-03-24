using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICreditCardRepository : IGenericRepository<CreditCard>
    {
        Task<(IEnumerable<CreditCard> Items, int TotalCount)> GetActivePagedAsync(
            int page, int pageSize);
        Task<IEnumerable<CreditCard>> GetByIdentityNumberAsync(string identityNumber);
        Task<(IEnumerable<CreditCard> Items, int TotalCount)> GetByStatusPagedAsync(
            bool isActive, int page, int pageSize);
        Task<bool> IdentifierNumberExistsAsync(string number);
        Task<CreditCard?> GetWithConsumptionsAsync(string id);
    }
}

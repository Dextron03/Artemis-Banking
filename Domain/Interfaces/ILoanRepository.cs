using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ILoanRepository : IGenericRepository<Loan>
    {
        Task<Loan?> GetByIdWithSharesAsync(string id);
        Task<IEnumerable<Loan>> GetAllWithSharesAsync();
        Task<bool> IdentifierExistsAsync(string identifierNumber);
    }
}

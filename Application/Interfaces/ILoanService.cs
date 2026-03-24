using Application.DTOs.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ILoanService
    {
        Task<PagedLoansDto> GetPagedAsync(LoanFilterDto filter);

        Task<List<ShareDto>> GetSharesAsync(string loanId);

        Task<LoanDetailDto> GetLoanDetailAsync(string loanId);

        Task<EligibleClientsResultDto> GetEligibleClientsAsync(string? identityNumber);
        Task<RiskEvaluationDto> EvaluateRiskAsync(string userId, decimal amount, decimal interestRate);
        Task<string> CreateLoanAsync(CreateLoanDto dto);
        Task UpdateInterestRateAsync(UpdateLoanRateDto dto);
    }
}

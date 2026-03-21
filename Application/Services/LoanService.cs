using Application.ViewModels;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Identity.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Services;

namespace Application.Services
{
    public class LoanService
    {
        private readonly IGenericRepository<Loan> _loanRepo;
        private readonly IGenericRepository<SavingsAccount> _accountRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;

        public LoanService(
            IGenericRepository<Loan> loanRepo,
            IGenericRepository<SavingsAccount> accountRepo,
            UserManager<AppUser> userManager,
            IEmailService emailService, ApplicationDbContext context)
        {
            _loanRepo = loanRepo;
            _accountRepo = accountRepo;
            _userManager = userManager;
            _emailService = emailService;
            _context = context;
        }

        public async Task<string> CreateLoan(string userId, decimal amount, decimal interest, int months, string adminId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado");

            var existingLoan = (await _loanRepo.FindAsync(l =>
                l.UserId == userId && l.IsActive)).FirstOrDefault();

            if (existingLoan != null)
                throw new Exception("Cliente ya tiene préstamo activo");

            var loan = new Loan
            {
                UserId = userId,
                LoanAmount = amount,
                InterestRate = interest,
                TermMonths = months,
                OutstandingAmount = amount,
                IdentifierNumber = Loan.GenerateUniqueNumber(),
                CreatedByUserId = adminId
            };

            loan.GenerateAmortizationTable();

            await _loanRepo.AddAsync(loan);
            await _loanRepo.SaveChangesAsync();

            var account = (await _accountRepo.FindAsync(a =>
                a.UserId == userId && a.IsPrincipal)).FirstOrDefault();

            if (account != null)
            {
                account.SetBalance(account.Balance + amount);
                _accountRepo.Update(account);
                await _accountRepo.SaveChangesAsync();
            }

            await _emailService.SendEmailAsync(
                user.Email,
                "Préstamo aprobado",
                $"Monto: {amount} | Meses: {months} | Tasa: {interest}% | Cuota: {loan.MonthlyPayment}"
            );

            return loan.Id;
        }

        public async Task<(List<LoanViewModel>, int)> GetPaged(string identity, bool? isActive, int page, int size)
        {
            var loans = (await _loanRepo.GetAllAsync()).ToList();

            var result = new List<LoanViewModel>();

            foreach (var loan in loans)
            {
                var user = await _userManager.FindByIdAsync(loan.UserId);

                if (user == null) continue;

                if (!string.IsNullOrEmpty(identity) && user.IdentityNumber != identity)
                    continue;

                if (isActive.HasValue && loan.IsActive != isActive.Value)
                    continue;

                result.Add(new LoanViewModel
                {
                    Id = loan.Id,
                    IdentifierNumber = loan.IdentifierNumber,
                    ClientName = $"{user.FirtsName} {user.LastName}",
                    IdentityNumber = user.IdentityNumber,
                    LoanAmount = loan.LoanAmount,
                    TotalInstallments = loan.TermMonths,
                    PaidInstallments = loan.Shares.Count(s => s.IsPaid),
                    PendingAmount = loan.OutstandingAmount,
                    InterestRate = loan.InterestRate,
                    Months = loan.TermMonths,
                    PaymentStatus = loan.PaymentStatus.ToString(),
                    IsActive = loan.IsActive
                });
            }

            result = result
                .OrderByDescending(l => l.IsActive)
                .ThenByDescending(l => l.Id)
                .ToList();

            var total = result.Count;

            var data = result
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            return (data, total);
        }

        public async Task<List<Share>> GetShares(string loanId)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId);
            if (loan == null) throw new Exception("No existe");

            return loan.Shares.OrderBy(s => s.DatePay).ToList();
        }

        public async Task UpdateInterest(string loanId, decimal newRate)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId);
            if (loan == null) throw new Exception("No existe");

            loan.InterestRate = newRate;

            var remaining = loan.Shares
                .Where(s => !s.IsPaid && s.DatePay > DateTime.Now)
                .ToList();

            int months = remaining.Count;

            decimal newPayment = CalculateMonthlyPayment(
                loan.OutstandingAmount,
                newRate,
                months);

            foreach (var s in remaining)
                s.ShareAmount = Math.Round(newPayment, 2);

            _loanRepo.Update(loan);
            await _loanRepo.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(loan.UserId);

            await _emailService.SendEmailAsync(
                user.Email,
                "Tasa actualizada",
                $"Nueva tasa: {newRate}% | Nueva cuota: {newPayment}"
            );
        }

        public async Task<(bool, string)> EvaluateRisk(string userId, decimal amount, decimal interest)
        {
            var debt = await GetClientDebt(userId);
            var avg = await GetAverageDebt();

            decimal totalLoan = amount + (amount * (interest / 100));

            if (debt > avg)
                return (true, "RIESGO_ACTUAL");

            if ((debt + totalLoan) > avg)
                return (true, "RIESGO_NUEVO");

            return (false, "");
        }

        public async Task<decimal> GetClientDebt(string userId)
        {
            var loans = await _loanRepo.FindAsync(l => l.UserId == userId && l.IsActive);
            return loans.Sum(l => l.OutstandingAmount);
        }

        public async Task<decimal> GetAverageDebt()
        {
            var loans = await _loanRepo.GetAllAsync();
            if (!loans.Any()) return 0;
            return loans.Average(l => l.OutstandingAmount);
        }

        private decimal CalculateMonthlyPayment(decimal P, decimal rate, int n)
        {
            decimal r = (rate / 100) / 12;

            return P * (r * (decimal)Math.Pow(1 + (double)r, n)) /
                   ((decimal)Math.Pow(1 + (double)r, n) - 1);
        }

        public async Task<SelectClientViewModel> GetClientsForLoan(string identityNumber = null)
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            var result = new List<ClientLoanViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Cliente"))
                    continue;

                result.Add(new ClientLoanViewModel
                {
                    Id = user.Id,
                    FirtsName = user.FirtsName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IdentityNumber = user.IdentityNumber,
                    Debt = 0 
                });
            }

            if (!string.IsNullOrEmpty(identityNumber))
            {
                result = result
                    .Where(u => u.IdentityNumber.Trim() == identityNumber.Trim())
                    .ToList();
            }

            return new SelectClientViewModel
            {
                Clients = result,
                AverageDebt = result.Any() ? result.Average(x => x.Debt) : 0
            };
        }
    }
}
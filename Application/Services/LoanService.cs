using Application.DTOs.Loan;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Shared.Services;

namespace Application.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepo;
        private readonly ISavingsAccountRepository _accountRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ITransactionRepository _transactionRepo;

        public LoanService( ILoanRepository loanRepo, ITransactionRepository transactionRepo, ISavingsAccountRepository accountRepo, UserManager<AppUser> userManager, IEmailService emailService)
        {
            _loanRepo = loanRepo;
            _accountRepo = accountRepo;
            _userManager = userManager;
            _emailService = emailService;
            _transactionRepo = transactionRepo;
        }

        public async Task<PagedLoansDto> GetPagedAsync(LoanFilterDto filter)
        {
            filter.PageSize = Math.Min(filter.PageSize, 50);

            var allLoans = (await _loanRepo.GetAllWithSharesAsync()).ToList();
            var result = new List<LoanSummaryDto>();

            // Obtener todos los usuarios de una vez para evitar N+1
            var users = _userManager.Users.ToList();

            foreach (var loan in allLoans)
            {
                var user = users.FirstOrDefault(u => u.Id == loan.UserId);
                if (user == null) continue;

                // Filtrado por Cédula (Parcial)
                if (!string.IsNullOrWhiteSpace(filter.IdentityNumber))
                {
                    if (!user.IdentityNumber.Contains(filter.IdentityNumber.Trim()))
                        continue;
                }

                // Filtrado por Estado (Activo/Inactivo/Todos)
                if (filter.IsActive.HasValue)
                {
                    if (loan.IsActive != filter.IsActive.Value)
                        continue;
                }

                result.Add(MapToSummary(loan, user));
            }

            if (string.IsNullOrWhiteSpace(filter.IdentityNumber))
            {
                result = result.OrderByDescending(l => l.IsActive).ThenByDescending(l => l.Id).ToList();
            }
            else
            {
                result = result
                    .OrderByDescending(l => l.IsActive)
                    .ThenByDescending(l => l.Id)
                    .ToList();
            }

            int total = result.Count;
            int pages = (int)Math.Ceiling((double)total / filter.PageSize);

            var items = result
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return new PagedLoansDto
            {
                Items = items,
                TotalItems = total,
                TotalPages = pages,
                CurrentPage = filter.Page
            };
        }

        public async Task<List<ShareDto>> GetSharesAsync(string loanId)
        {
            var loan = await _loanRepo.GetByIdWithSharesAsync(loanId)
                       ?? throw new InvalidOperationException("Préstamo no encontrado.");

            return loan.Shares
                .OrderBy(s => s.QuotaNumber)
                .Select(s => new ShareDto
                {
                    Id = s.Id,
                    QuotaNumber = s.QuotaNumber,
                    ShareAmount = s.ShareAmount,
                    DatePay = s.DatePay,
                    IsPaid = s.IsPaid,
                    IsDelayed = s.IsDelayed
                })
                .ToList();
        }

        public async Task<LoanDetailDto> GetLoanDetailAsync(string loanId)
        {
            var loan = await _loanRepo.GetByIdWithSharesAsync(loanId)
                       ?? throw new InvalidOperationException("Préstamo no encontrado.");

            var user = await _userManager.FindByIdAsync(loan.UserId);
            string clientName = user != null
                ? $"{user.FirtsName} {user.LastName}"
                : "—";

            var shares = loan.Shares
                .OrderBy(s => s.QuotaNumber)
                .Select(s => new ShareDto
                {
                    Id = s.Id,
                    QuotaNumber = s.QuotaNumber,
                    ShareAmount = s.ShareAmount,
                    DatePay = s.DatePay,
                    IsPaid = s.IsPaid,
                    IsDelayed = s.IsDelayed
                })
                .ToList();

            return new LoanDetailDto
            {
                LoanId = loan.Id,
                IdentifierNumber = loan.IdentifierNumber,
                ClientName = clientName,
                LoanAmount = loan.LoanAmount,
                InterestRate = loan.InterestRate,
                TermMonths = loan.TermMonths,
                MonthlyPayment = loan.MonthlyPayment,
                OutstandingAmount = loan.OutstandingAmount,
                Shares = shares
            };
        }

        public async Task<EligibleClientsResultDto> GetEligibleClientsAsync(string? identityNumber)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("Cliente");
            var activeClients = usersInRole.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.Now).ToList();

            var activeLoans = (await _loanRepo.FindAsync(l => l.IsActive)).ToList();
            var clientsWithActiveLoan = activeLoans
                .Select(l => l.UserId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var allLoans = (await _loanRepo.GetAllAsync()).ToList();
            var eligible = new List<EligibleClientDto>();

            foreach (var user in activeClients)
            {
                if (clientsWithActiveLoan.Contains(user.Id)) continue;

                if (!string.IsNullOrWhiteSpace(identityNumber) &&
                    !user.IdentityNumber.Contains(identityNumber.Trim()))
                    continue;

                decimal debt = allLoans
                    .Where(l => l.UserId == user.Id && l.IsActive)
                    .Sum(l => l.OutstandingAmount);

                eligible.Add(new EligibleClientDto
                {
                    Id = user.Id,
                    FirstName = user.FirtsName,
                    LastName = user.LastName,
                    IdentityNumber = user.IdentityNumber,
                    Email = user.Email ?? string.Empty,
                    TotalDebt = debt
                });
            }

            decimal averageDebt = activeLoans.Count > 0
                ? activeLoans.Average(l => l.OutstandingAmount)
                : 0m;

            return new EligibleClientsResultDto
            {
                Clients = eligible.OrderBy(c => c.FirstName).ToList(),
                AverageDebt = Math.Round(averageDebt, 2, MidpointRounding.AwayFromZero)
            };
        }

        public async Task<RiskEvaluationDto> EvaluateRiskAsync(
            string userId, decimal amount, decimal interestRate)
        {
            decimal clientDebt = await GetClientDebtAsync(userId);
            decimal averageDebt = await GetAverageDebtAsync();
            decimal totalNewLoanCost = amount + (amount * (interestRate / 100m));

            bool isHighRisk = false;
            string riskType = string.Empty;

            if (clientDebt > averageDebt)
            {
                isHighRisk = true;
                riskType = "RIESGO_ACTUAL";
            }
            else if ((clientDebt + totalNewLoanCost) > averageDebt)
            {
                isHighRisk = true;
                riskType = "RIESGO_NUEVO";
            }

            return new RiskEvaluationDto
            {
                IsHighRisk = isHighRisk,
                RiskType = riskType,
                AverageDebt = Math.Round(averageDebt, 2, MidpointRounding.AwayFromZero),
                ClientDebt = Math.Round(clientDebt, 2, MidpointRounding.AwayFromZero)
            };
        }

        public async Task<string> CreateLoanAsync(CreateLoanDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId)
                       ?? throw new InvalidOperationException("Usuario no encontrado.");

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Cliente"))
                throw new InvalidOperationException("Solo se pueden asignar préstamos a clientes.");

            if (!user.IsActive)
                throw new InvalidOperationException("El cliente está inactivo.");

            var existing = (await _loanRepo.FindAsync(l =>
                l.UserId == dto.UserId && l.IsActive)).FirstOrDefault();

            if (existing != null)
                throw new InvalidOperationException(
                    "El cliente ya tiene un préstamo activo. Solo se permite uno a la vez.");

            string identifier;
            do { identifier = Loan.GenerateUniqueNumber(); }
            while (await _loanRepo.IdentifierExistsAsync(identifier));

            var loan = new Loan
            {
                IdentifierNumber = identifier,
                UserId = dto.UserId,
                CreatedByUserId = dto.AdminId,
                LoanAmount = dto.Amount,
                InterestRate = dto.InterestRate,
                TermMonths = dto.Months,
                OutstandingAmount = dto.Amount,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            loan.GenerateAmortizationTable();

            await _loanRepo.AddAsync(loan);
            await _loanRepo.SaveChangesAsync();

            var account = await _accountRepo.GetPrincipalByUserIdAsync(dto.UserId);
            if (account != null)
            {
                account.SetBalance(account.Balance + dto.Amount);
                _accountRepo.Update(account);

                var transaction = new Transaction
                {
                    Id = Guid.NewGuid().ToString(),
                    Amount = dto.Amount,
                    Type = TransactionType.Deposit,
                    Origin = loan.IdentifierNumber,     
                    Beneficiary = account.AccountNumber, 
                    Status = "APROBADA",
                    Concept = $"Desembolso de préstamo {loan.IdentifierNumber}",
                    SavingAccountId = account.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepo.AddAsync(transaction); 

                await _accountRepo.SaveChangesAsync();
            }
            string emailBody =
                $"<p>Estimado/a <strong>{user.FirtsName} {user.LastName}</strong>,</p>" +
                $"<p>Su préstamo ha sido aprobado con los siguientes detalles:</p>" +
                $"<ul>" +
                $"  <li><strong>Monto aprobado:</strong> RD${loan.LoanAmount:N2}</li>" +
                $"  <li><strong>Plazo:</strong> {loan.TermMonths} meses</li>" +
                $"  <li><strong>Tasa de interés anual:</strong> {loan.InterestRate:N2}%</li>" +
                $"  <li><strong>Cuota mensual:</strong> RD${loan.MonthlyPayment:N2}</li>" +
                $"</ul>" +
                $"<p>Número de préstamo: <strong>{loan.IdentifierNumber}</strong></p>";

            await _emailService.SendEmailAsync(
                user.Email!,
                "Préstamo aprobado – Banco Digital",
                emailBody);

            return loan.Id;
        }

        public async Task UpdateInterestRateAsync(UpdateLoanRateDto dto)
        {
            var loan = await _loanRepo.GetByIdWithSharesAsync(dto.LoanId) ?? throw new InvalidOperationException("Préstamo no encontrado.");
            loan.RecalculateFutureShares(dto.NewRate);

            _loanRepo.Update(loan);
            await _loanRepo.SaveChangesAsync();
            var user = await _userManager.FindByIdAsync(loan.UserId);
            if (user != null)
            {
                string emailBody =
                    $"<p>Estimado/a <strong>{user.FirtsName} {user.LastName}</strong>,</p>" +
                    $"<p>La tasa de interés de su préstamo <strong>{loan.IdentifierNumber}</strong> " +
                    $"ha sido actualizada.</p>" +
                    $"<ul>" +
                    $"  <li><strong>Nueva tasa anual:</strong> {dto.NewRate:N2}%</li>" +
                    $"  <li><strong>Nueva cuota mensual:</strong> RD${loan.MonthlyPayment:N2}</li>" +
                    $"</ul>" +
                    $"<p>El cambio aplica a partir de su próxima cuota.</p>";

                await _emailService.SendEmailAsync(
                    user.Email!,
                    "Actualización de tasa de interés – Banco Digital",
                    emailBody);
            }
        }

        private async Task<decimal> GetClientDebtAsync(string userId)
        {
            var loans = await _loanRepo.FindAsync(l => l.UserId == userId && l.IsActive);
            return loans.Sum(l => l.OutstandingAmount);
        }

        private async Task<decimal> GetAverageDebtAsync()
        {
            var loans = (await _loanRepo.FindAsync(l => l.IsActive)).ToList();
            return loans.Count == 0 ? 0m : loans.Average(l => l.OutstandingAmount);
        }

        private static LoanSummaryDto MapToSummary(Loan loan, AppUser user)
        {
            // El conteo de cuotas pagadas depende de que las cuotas estén cargadas
            int paidCount = loan.Shares != null ? loan.Shares.Count(s => s.IsPaid) : 0;

            return new LoanSummaryDto
            {
                Id = loan.Id,
                IdentifierNumber = loan.IdentifierNumber,
                ClientName = $"{user.FirtsName} {user.LastName}",
                IdentityNumber = user.IdentityNumber,
                LoanAmount = loan.LoanAmount,
                TotalInstallments = loan.TermMonths,
                PaidInstallments = paidCount,
                PendingAmount = loan.OutstandingAmount,
                InterestRate = loan.InterestRate,
                Months = loan.TermMonths,
                PaymentStatus = loan.PaymentStatus == PaymentStatusType.AlDia ? "Al Día" : "En Mora",
                IsActive = loan.IsActive
            };
        }
    }
}
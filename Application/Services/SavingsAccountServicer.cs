using Application.DTOs.SavingsAccount;
using Application.Interfaces;
using Application.ViewModels.SavingsAccount;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SavingsAccountService : ISavingsAccountService
    {
        private readonly ISavingsAccountRepository _savingsRepo;
        private readonly ILoanRepository _loanRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public SavingsAccountService( ISavingsAccountRepository savingsRepo, ILoanRepository loanRepo, ITransactionRepository transactionRepo, UserManager<AppUser> userManager, IMapper mapper)
        {
            _savingsRepo = savingsRepo;
            _transactionRepo = transactionRepo;
            _userManager = userManager;
            _mapper = mapper;
            _loanRepo = loanRepo;
        }

        public async Task<PagedSavingsAccountDto> GetPagedAsync( int page, int pageSize, string? searchIdentityNumber, string? filterStatus, string? filterType)
        {
            IEnumerable<SavingsAccount> query;

            if (!string.IsNullOrWhiteSpace(searchIdentityNumber))
            {
                var user = _userManager.Users
                    .FirstOrDefault(u => u.IdentityNumber == searchIdentityNumber.Trim());
                if (user == null)
                    return new PagedSavingsAccountDto
                    {
                        SearchIdentityNumber = searchIdentityNumber,
                        FilterStatus = filterStatus,
                        FilterType = filterType
                    };
                query = await _savingsRepo.FindAsync(s => s.UserId == user.Id);
            }
            else
            {
                query = await _savingsRepo.GetAllAsync();
            }

            query = filterStatus switch
            {
                "active" => query.Where(s => s.IsActive),
                "cancelled" => query.Where(s => !s.IsActive),
                _ => string.IsNullOrWhiteSpace(searchIdentityNumber)
                                   ? query.Where(s => s.IsActive)
                                   : query
            };

            query = filterType switch
            {
                "principal" => query.Where(s => s.IsPrincipal),
                "secondary" => query.Where(s => !s.IsPrincipal),
                _ => query
            };

            query = string.IsNullOrWhiteSpace(searchIdentityNumber)
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderByDescending(s => s.IsActive).ThenByDescending(s => s.CreatedAt);

            var list = query.ToList();
            int total = list.Count;
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var paged = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var rows = new List<SavingsAccountListDto>();
            foreach (var acc in paged)
            {
                var dto = _mapper.Map<SavingsAccountListDto>(acc);
                var user = await _userManager.FindByIdAsync(acc.UserId);
                dto.ClientFullName = user != null
                    ? $"{user.FirtsName} {user.LastName}"
                    : "Desconocido";
                rows.Add(dto);
            }

            return new PagedSavingsAccountDto
            {
                Items = rows,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = total,
                PageSize = pageSize,
                SearchIdentityNumber = searchIdentityNumber,
                FilterStatus = filterStatus,
                FilterType = filterType
            };
        }

        public async Task<List<ClientForSavingsAccountDto>> GetActiveClientsAsync(
        string? searchIdentityNumber = null)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("Cliente");
            var users = usersInRole
                .Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.Now);

            if (!string.IsNullOrWhiteSpace(searchIdentityNumber))
                users = users.Where(u =>
                    u.IdentityNumber.Contains(searchIdentityNumber.Trim()));

            var allActiveLoans = (await _loanRepo.FindAsync(l => l.IsActive)).ToList();

            var result = new List<ClientForSavingsAccountDto>();
            foreach (var u in users)
            {
                decimal debt = allActiveLoans
                    .Where(l => l.UserId == u.Id)
                    .Sum(l => l.OutstandingAmount);

                result.Add(new ClientForSavingsAccountDto
                {
                    Id = u.Id,
                    FullName = $"{u.FirtsName} {u.LastName}",
                    IdentityNumber = u.IdentityNumber,
                    Email = u.Email ?? string.Empty,
                    TotalDebt = debt   
                });
            }

            return result.OrderBy(c => c.FullName).ToList();
        }

        public async Task<SavingsAccount_ResultDto> CreateSecondaryAccountAsync(CreateSavingsAccountDto dto)
        {
            string accountNumber = await GenerateUniqueAccountNumberAsync();

            var account = new SavingsAccount
            {
                Id = Guid.NewGuid().ToString(),
                AccountNumber = accountNumber,
                UserId = dto.UserId,
                AdminId = dto.AdminUserId,   
                IsPrincipal = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            account.SetBalance(dto.InitialBalance);

            await _savingsRepo.AddAsync(account);
            await _savingsRepo.SaveChangesAsync();

            return new SavingsAccount_ResultDto
            {
                Success = true,
                Message = "Cuenta de ahorro secundaria creada exitosamente.",
                AccountId = account.Id
            };
        }

        public async Task<SavingsAccountDetailDto?> GetDetailAsync(string id)
        {
            var account = await _savingsRepo.GetByIdWithTransactionsAsync(id);
            if (account == null) return null;

            var user = await _userManager.FindByIdAsync(account.UserId);

            var transactions = account.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    CreatedAt = t.CreatedAt,
                    Amount = t.Amount,
                    Type = t.Type switch
                    {
                        TransactionType.Deposit => "CRÉDITO",
                        TransactionType.Withdrawal => "DÉBITO",
                        TransactionType.Transfer => "DÉBITO",
                        TransactionType.Payment => "DÉBITO",
                        _ => "DÉBITO"
                    },
                    Origin = t.Origin,
                    Beneficiary = t.Beneficiary,
                    Status = t.Status,
                    Concept = t.Concept
                })
                .ToList();

            return new SavingsAccountDetailDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                ClientFullName = user != null
                    ? $"{user.FirtsName} {user.LastName}"
                    : "Desconocido",
                UserId = account.UserId,
                Balance = account.Balance,
                IsPrincipal = account.IsPrincipal,
                IsActive = account.IsActive,
                CreatedAt = account.CreatedAt,
                Transactions = transactions
            };
        }

        public async Task<CancelSavingsAccountInfoDto?> GetCancelInfoAsync(string id)
        {
            var account = await _savingsRepo.GetByIdAsync(id);
            if (account == null) return null;

            var user = await _userManager.FindByIdAsync(account.UserId);
            return new CancelSavingsAccountInfoDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                ClientFullName = user != null
                    ? $"{user.FirtsName} {user.LastName}"
                    : "Desconocido",
                Balance = account.Balance,
                IsPrincipal = account.IsPrincipal
            };
        }

        public async Task CancelAccountAsync(string id)
        {
            var account = await _savingsRepo.GetByIdWithTransactionsAsync(id);
            if (account == null) throw new InvalidOperationException("Cuenta no encontrada.");
            if (account.IsPrincipal)
                throw new InvalidOperationException("Las cuentas principales no pueden cancelarse.");
            if (!account.IsActive)
                throw new InvalidOperationException("La cuenta ya está cancelada.");

            if (account.Balance > 0)
            {
                var principal = await _savingsRepo.GetPrincipalAccountByUserIdAsync(account.UserId)
                    ?? throw new InvalidOperationException(
                        "No se encontró la cuenta principal del cliente.");

                decimal amount = account.Balance;
                var debitTx = new Transaction
                {
                    Id = Guid.NewGuid().ToString(),
                    Amount = amount,
                    Type = TransactionType.Transfer,
                    Origin = account.AccountNumber,          
                    Beneficiary = principal.AccountNumber,    
                    Status = "APROBADA",
                    Concept = $"Transferencia automática por cancelación de cuenta {account.AccountNumber}",
                    SavingAccountId = account.Id,
                    CreatedAt = DateTime.Now
                };
                await _transactionRepo.AddAsync(debitTx);

                var creditTx = new Transaction
                {
                    Id = Guid.NewGuid().ToString(),
                    Amount = amount,
                    Type = TransactionType.Deposit,           
                    Origin = account.AccountNumber,           
                    Beneficiary = principal.AccountNumber,    
                    Status = "APROBADA",
                    Concept = $"Recepción por cancelación de cuenta secundaria {account.AccountNumber}",
                    SavingAccountId = principal.Id,
                    CreatedAt = DateTime.Now
                };
                await _transactionRepo.AddAsync(creditTx);

                principal.SetBalance(principal.Balance + amount);
                _savingsRepo.Update(principal);
                account.SetBalance(0);
            }

            account.IsActive = false;
            _savingsRepo.Update(account);
            await _savingsRepo.SaveChangesAsync();
        }

        private async Task<string> GenerateUniqueAccountNumberAsync()
        {
            var rng = new Random();
            string number;
            bool exists;
            do
            {
                number = rng.Next(100000000, 999999999).ToString();
                exists = await _savingsRepo.AccountNumberExistsAsync(number);
            } while (exists);
            return number;
        }

    }
}
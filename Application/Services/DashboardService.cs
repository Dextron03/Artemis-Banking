using Application.DTOs.Dashboard;
using Application.Interfaces;
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
    public class DashboardService : IDashboardService
    {
        private readonly IGenericRepository<Transaction> _transactionRepo;
        private readonly IGenericRepository<Loan> _loanRepo;
        private readonly IGenericRepository<CreditCard> _cardRepo;
        private readonly IGenericRepository<SavingsAccount> _accountRepo;
        private readonly UserManager<AppUser> _userManager;

        public DashboardService(
            IGenericRepository<Transaction> transactionRepo,
            IGenericRepository<Loan> loanRepo,
            IGenericRepository<CreditCard> cardRepo,
            IGenericRepository<SavingsAccount> accountRepo,
            UserManager<AppUser> userManager)
        {
            _transactionRepo = transactionRepo;
            _loanRepo = loanRepo;
            _cardRepo = cardRepo;
            _accountRepo = accountRepo;
            _userManager = userManager;
        }

        public async Task<DashboardAdminDto> GetDashboardAsync()
        {
            var transactions = await _transactionRepo.GetAllAsync();
            var loans = await _loanRepo.GetAllAsync();
            var cards = await _cardRepo.GetAllAsync();
            var accounts = await _accountRepo.GetAllAsync();

            var today = DateTime.Today;
            var users = _userManager.Users.ToList();

            var clients = new List<AppUser>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Cliente"))
                {
                    clients.Add(user);
                }
            }

            var activeClients = clients.Count(u => u.LockoutEnd == null);
            var inactiveClients = clients.Count(u => u.LockoutEnd != null);

            return new DashboardAdminDto
            {
                TotalTransactions = transactions.Count(),
                TransactionsToday = transactions.Count(t => t.CreatedAt.Date == today),
                TotalPayments = transactions.Count(t => t.Type.ToString() == "Pago"),
                PaymentsToday = transactions.Count(t => t.Type.ToString() == "Pago" && t.CreatedAt.Date == today),
                ActiveClients = activeClients,
                InactiveClients = inactiveClients,
                ActiveLoans = loans.Count(l => l.IsActive),
                ActiveCreditCards = cards.Count(c => c.IsActive),
                TotalSavingsAccounts = accounts.Count(),
                TotalProducts = loans.Count() + cards.Count() + accounts.Count(),
                AverageDebtPerClient = loans
                    .Where(l => l.IsActive)
                    .Any()
                    ? loans.Where(l => l.IsActive).Average(l => l.OutstandingAmount)
                    : 0
            };
        }

        public async Task<DashboardCashierDto> GetCashierDashboardAsync(string userId)
        {
            var today = DateTime.Today;
            var transactions = await _transactionRepo.GetAllAsync();
            
            // Nota: En un sistema real, las transacciones deberían tener el Id del cajero que las realizó.
            // Dado que la entidad Transaction no parece tener un campo CreatedByUserId, 
            // intentaremos filtrar por concepto o asumir que el cajero actual ve lo global de hoy si no hay filtro de autor.
            // INVESTIGACIÓN: Revisar si Transaction tiene autor.
            
            var todayTransactions = transactions
                .Where(t => t.CreatedAt.Date == today)
                .ToList();

            return new DashboardCashierDto
            {
                TotalTransactionsToday = todayTransactions.Count,
                TotalPaymentsToday = todayTransactions.Count(t => t.Type == TransactionType.Payment),
                TotalDepositsToday = todayTransactions.Count(t => t.Type == TransactionType.Deposit),
                TotalWithdrawalsToday = todayTransactions.Count(t => t.Type == TransactionType.Withdrawal)
            };
        }
    }
}
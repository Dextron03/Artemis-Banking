using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Application.Interfaces;
using Application.ViewModels;
using Domain.Entities;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly IGenericService<SavingsAccount> _accountService;
        private readonly UserManager<AppUser> _userManager;


        public CashierService(IGenericService<SavingsAccount> accountService, UserManager<AppUser> userManager)
        {
            _accountService = accountService;
            _userManager = userManager;
        }

        public async Task<ConfirmDepositViewModel> ValidateDepositAsync(DepositViewModel vm)
        {
            // Buscamos por el Account Number
            var accounts = await _accountService.FindAsync(a => a.AccountNumber == vm.AccountNumber);
            var account = accounts.FirstOrDefault();

            if(account == null || !account.IsActive)
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(account.Id);

            return new ConfirmDepositViewModel
            {
                AccountNumber = vm.AccountNumber,
                Amount = vm.Amount,
                ClientName = user.FirtsName,
                ClientLastName = user.LastName
            };

        }
    }
}
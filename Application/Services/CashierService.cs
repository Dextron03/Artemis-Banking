using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.ViewModels;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Identity.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Shared.Services;

namespace Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly IGenericService<SavingsAccount> _accountService;
        private readonly IGenericRepository<SavingsAccount> _accountRepository;
        private readonly IGenericRepository<Transaction> _transactionRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;


        public CashierService(
            IGenericService<SavingsAccount> accountService,
            UserManager<AppUser> userManager,
            IGenericRepository<SavingsAccount> accountRepository,
            IGenericRepository<Transaction> transactionRepository,
            IEmailService emailService)
        {
            _accountService = accountService;
            _userManager = userManager;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _emailService = emailService;
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

        public async Task<ConfirmWithdrawalViewModel> ValidateWithdrawalAsync(WithdrawalViewModel vm)
        {
            var accounts = await _accountRepository.FindAsync(a => a.AccountNumber == vm.AccountNumber);
            var account = accounts.FirstOrDefault();

            if(account == null || !account.IsActive)
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(account.UserId);

            return new ConfirmWithdrawalViewModel
            {
                AccountNumber = vm.AccountNumber,
                Amount = vm.Amount,
                ClientName = user.FirtsName,
                ClientLastName = user.LastName,
                HasInsufficientFunds = account.Balance < vm.Amount  
            };
        }

        public async Task<bool> ProcessWithdrawalAsync(string accountNumber, decimal amount)
        {
            var accounts = await _accountRepository.FindAsync(a => a.AccountNumber == accountNumber);
            var account = accounts.FirstOrDefault();

            if(account == null || !account.IsActive || account.Balance < amount)
            {
                return false;
            }

            account.SetBalance(account.Balance - amount);
            _accountRepository.Update(account);

            var transaction = new Transaction
            {
                SavingAccountId = account.Id,
                Amount = amount,
                Type = TransactionType.Withdrawal,
                Origin = accountNumber,
                Beneficiary = account.AccountNumber,
                Status = "APROBADA"
            };

            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(account.UserId);
            if(user != null && !string.IsNullOrEmpty(user.Email))
            {
                string last4Digits = account.AccountNumber.Length >= 4
                ? account.AccountNumber.Substring(account.AccountNumber.Length - 4)
                : account.AccountNumber;

                string subject = $"Retiro realizado en su cuenta {last4Digits}";

                string body = $@"
                    <h3>Hola {user.FirtsName},</h3>
                    <p>Se ha realizado un retiro de su cuenta de ahorros.</p>
                    <ul>
                        <li><strong>Cuenta:</strong> terminada en {last4Digits}</li>
                        <li><strong>Monto retirado:</strong> RD$ {amount.ToString("N2")}</li>
                        <li><strong>Fecha y hora exacta:</strong> {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}</li>
                    </ul>
                    <p>Si usted no reconce esta transacción, contáctenos de inmediato.</p>";
                
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            return true;
        }

        public async Task<bool> ProcessDepositAsync(string accountNumber, decimal amount)
        {
            var accounts = await _accountRepository
                                .FindAsync(a => a.AccountNumber == accountNumber);
            var account = accounts.FirstOrDefault();

            if(accounts == null || !account.IsActive)
            {
                return false;
            }

            account.SetBalance( account.Balance + amount ); 

            _accountRepository.Update( account );

            var transaction = new Transaction
            {
                SavingAccountId = account.Id,
                Amount = amount,
                Type = TransactionType.Deposit,
                Origin = accountNumber,
                Beneficiary = account.AccountNumber,
                Status = "APROBADA"
            };

            await _transactionRepository.AddAsync(transaction);

            await _transactionRepository.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(account.UserId);
            if(user != null && !string.IsNullOrEmpty(user.Email))
            {
                string last4Digits = account.AccountNumber.Length >= 4 
                    ? account.AccountNumber.Substring(account.AccountNumber.Length - 4) 
                    : account.AccountNumber;

                string subject = $"Depósito realizado a su cuenta {last4Digits}";

                string body = $@"
                    <h3>Hola {user.FirtsName},</h3>
                    <p>Se ha realizado un depósito en su cuenta de ahorros.</p>
                    <ul>
                        <li><strong>Cuenta:</strong> terminada en {last4Digits}</li>
                        <li><strong>Monto depositado:</strong> RD$ {amount.ToString("N2")}</li>
                        <li><strong>Fecha y hora exacta:</strong> {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}</li>
                    </ul>
                    <p>Gracias por utilizar Artemis Banking.</p>";
                
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            return true;
        }
    }
}
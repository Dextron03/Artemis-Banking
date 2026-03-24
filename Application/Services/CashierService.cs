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
        private readonly IGenericRepository<Loan> _loanRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;


        public CashierService(
            IGenericService<SavingsAccount> accountService,
            UserManager<AppUser> userManager,
            IGenericRepository<SavingsAccount> accountRepository,
            IGenericRepository<Transaction> transactionRepository,
            IGenericRepository<Loan> loanRepository,
            IEmailService emailService)
        {
            _accountService = accountService;
            _userManager = userManager;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _loanRepository = loanRepository;
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

        public async Task<ConfirmTransferViewModel> ValidateTransferAsync(TransferViewModel vm)
        {
            // 1. Validar que no se transfiera a sí mismo
            if (vm.OriginAccount == vm.DestinationAccount)
            {
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "No puedes transferir a la misma cuenta." };
            }
        
            // 2. Buscar ambas cuentas
            var originAccounts = await _accountRepository.FindAsync(a => a.AccountNumber == vm.OriginAccount);
            var originAccount = originAccounts.FirstOrDefault();
        
            var destAccounts = await _accountRepository.FindAsync(a => a.AccountNumber == vm.DestinationAccount);
            var destAccount = destAccounts.FirstOrDefault();
        
            // 3. Validar existencia y estado
            if (originAccount == null || !originAccount.IsActive)
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "La cuenta origen no existe o está inactiva." };
        
            if (destAccount == null || !destAccount.IsActive)
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "La cuenta destino no existe o está inactiva." };
        
            // 4. Validar fondos
            if (originAccount.Balance < vm.Amount)
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "La cuenta origen no tiene fondos suficientes para esta transferencia." };
        
            // 5. Buscar a los dueños para mostrar en la confirmación
            var originUser = await _userManager.FindByIdAsync(originAccount.UserId);
            var destUser = await _userManager.FindByIdAsync(destAccount.UserId);
        
            return new ConfirmTransferViewModel
            {
                OriginAccount = vm.OriginAccount,
                DestinationAccount = vm.DestinationAccount,
                Amount = vm.Amount,
                OriginClientName = $"{originUser.FirtsName} {originUser.LastName}",
                DestinationClientName = $"{destUser.FirtsName} {destUser.LastName}",
                HasError = false
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
    
        public async Task<ConfirmTransferViewModel> ValidateTranferAsync(TransferViewModel vm)
        {
            if (vm.OriginAccount == vm.DestinationAccount)
            {
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "No puedes transferir a la misma cuenta." };
            }
        
            var originAccounts = await _accountRepository.FindAsync(a => a.AccountNumber == vm.OriginAccount);
            var originAccount = originAccounts.FirstOrDefault();

            var destAccounts = await _accountRepository.FindAsync(a => a.AccountNumber == vm.DestinationAccount);
            var destAccount = destAccounts.FirstOrDefault();

            if (destAccount == null || !destAccount.IsActive)
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "La cuenta destino no existe o está inactiva." };

            if (originAccount.Balance < vm.Amount)
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "La cuenta origen no tiene fondos suficientes para esta transferencia." };
    
            var originUser = await _userManager.FindByIdAsync(originAccount.UserId);
            var destUser = await _userManager.FindByIdAsync(destAccount.UserId);

            return new ConfirmTransferViewModel
            {
                OriginAccount = vm.OriginAccount,
                DestinationAccount = vm.DestinationAccount,
                Amount = vm.Amount,
                OriginClientName = $"{originUser.FirtsName} {originUser.LastName}",
                DestinationClientName = $"{destUser.FirtsName} {destUser.LastName}",
                HasError = false
            };
        }

        public async Task<bool> ProcessTransferAsync(string originAccountNum, string destAccountNum, decimal amount)
        {   
            var originAccount = (await _accountRepository.FindAsync(a => a.AccountNumber == originAccountNum)).FirstOrDefault();
            var destAccount = (await _accountRepository.FindAsync(a => a.AccountNumber == destAccountNum)).FirstOrDefault();

            if (originAccount == null || destAccount == null || originAccount.Balance < amount) return false;

            originAccount.SetBalance(originAccount.Balance - amount);
            destAccount.SetBalance(destAccount.Balance + amount);
    
            _accountRepository.Update(originAccount);
            _accountRepository.Update(destAccount);
    
            var debitTx = new Transaction
            {
                SavingAccountId = originAccount.Id,
                Amount = amount,
                Type = TransactionType.Transfer,
                Origin = originAccountNum,
                Beneficiary = destAccountNum,
                Status = "APROBADA"
            };
    
            var creditTx = new Transaction
            {
                SavingAccountId = destAccount.Id,
                Amount = amount,
                Type = TransactionType.Transfer,
                Origin = originAccountNum,
                Beneficiary = destAccountNum,
                Status = "APROBADA"
            };
    
            await _transactionRepository.AddAsync(debitTx);
            await _transactionRepository.AddAsync(creditTx);
    
            await _accountRepository.SaveChangesAsync();

            var originUser = await _userManager.FindByIdAsync(originAccount.UserId);
            if (originUser != null && !string.IsNullOrEmpty(originUser.Email))
            {
                string body = $@"
                    <h3>Hola {originUser.FirtsName},</h3>
                    <p>Se ha realizado una transferencia desde su cuenta.</p>
                    <ul>
                        <li><strong>Monto:</strong> RD$ {amount.ToString("N2")}</li>
                        <li><strong>Cuenta Destino:</strong> {destAccountNum}</li>
                        <li><strong>Fecha:</strong> {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}</li>
                    </ul>";
                await _emailService.SendEmailAsync(originUser.Email, "Transferencia Realizada", body);
            }
    

            return true;
        }

        public async Task<ConfirmPaymentViewModel> ValidatePaymentAsync(PaymentViewModel vm)
        {
            // Buscamos el préstamo (Asumo que tu entidad Loan tiene un campo como AccountNumber o Id que el usuario digita)
            // ADAPTA "l.Id.ToString()" o "l.AccountNumber" según cómo identifiques tus préstamos.
            var loans = await _loanRepository.FindAsync(l => l.Id.ToString() == vm.LoanId);
            var loan = loans.FirstOrDefault();

            if (loan == null) return null;

            var user = await _userManager.FindByIdAsync(loan.UserId);

            return new ConfirmPaymentViewModel
            {
                LoanId = vm.LoanId,
                Amount = vm.Amount,
                ClientName = user.FirtsName,
                ClientLastName = user.LastName
            };
        }

        public async Task<bool> ProcessPaymentAsync(string loanNumber, decimal amount)
        {
            var loans = await _loanRepository.FindAsync(l => l.Id.ToString() == loanNumber);
            var loan = loans.FirstOrDefault();

            if (loan == null) return false;

            // 1. Restamos la deuda del préstamo (Asumo que tienes un campo Amount, Debt o Balance en tu entidad Loan)
            // ADAPTA "loan.Amount" por el nombre real de tu campo de deuda.
            loan.LoanAmount -= amount; 
            _loanRepository.Update(loan);

            // 2. Registramos el historial (El dinero entra al banco, así que es un Crédito a favor del banco)
            var transaction = new Transaction
            {
                SavingAccountId = loan.Id, // O déjalo nulo si la transacción solo se ata a cuentas de ahorro
                Amount = amount,
                Type = TransactionType.Payment, // Asegúrate de tener "Payment" o "Pago" en tu Enum
                Origin = "EFECTIVO (CAJA)",
                Beneficiary = $"PRÉSTAMO {loanNumber}",
                Status = "APROBADA"
            };

            await _transactionRepository.AddAsync(transaction);
            await _loanRepository.SaveChangesAsync();

            // 3. El correo de confirmación
            var user = await _userManager.FindByIdAsync(loan.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                string body = $@"
                    <h3>Hola {user.FirtsName},</h3>
                    <p>Hemos recibido el pago de su préstamo.</p>
                    <ul>
                        <li><strong>Préstamo N°:</strong> {loanNumber}</li>
                        <li><strong>Monto pagado:</strong> RD$ {amount.ToString("N2")}</li>
                        <li><strong>Nuevo balance adeudado:</strong> RD$ {loan.LoanAmount.ToString("N2")}</li>
                    </ul>";
                await _emailService.SendEmailAsync(user.Email, "Pago de Préstamo Recibido", body);
            }

            return true;
        }
    }

}
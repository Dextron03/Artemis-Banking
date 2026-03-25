using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.ViewModels.Cashier.Deposits;
using Application.ViewModels.Cashier.Withdrawals;
using Application.ViewModels.Cashier.Payments;
using Application.ViewModels.Cashier.Transfers;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Shared.Services;

namespace Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly ISavingsAccountRepository _accountRepository;
        private readonly IGenericRepository<Transaction> _transactionRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ICreditCardRepository _cardRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;


        public CashierService(
            UserManager<AppUser> userManager,
            ISavingsAccountRepository accountRepository,
            IGenericRepository<Transaction> transactionRepository,
            ILoanRepository loanRepository,
            ICreditCardRepository cardRepository,
            IEmailService emailService)
        {
            _userManager = userManager;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _loanRepository = loanRepository;
            _cardRepository = cardRepository;
            _emailService = emailService;
        }

        public async Task<ConfirmDepositViewModel> ValidateDepositAsync(DepositViewModel vm)
        {
            var accounts = await _accountRepository.FindAsync(a => a.AccountNumber == vm.AccountNumber);
            var account = accounts.FirstOrDefault();

            if(account == null || !account.IsActive)
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(account.UserId);

            if (user == null)
            {
                return null;
            }

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
            if (vm.OriginAccount == vm.DestinationAccount)
            {
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "No puedes transferir a la misma cuenta." };
            }
        
            var originAccounts = await _accountRepository.FindAsync(a => a.AccountNumber == vm.OriginAccount);
            var originAccount = originAccounts.FirstOrDefault();
        
            var destAccounts = await _accountRepository.FindAsync(a => a.AccountNumber == vm.DestinationAccount);
            var destAccount = destAccounts.FirstOrDefault();
        
            if (originAccount == null || !originAccount.IsActive)
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "La cuenta origen no existe o está inactiva." };
        
            if (destAccount == null || !destAccount.IsActive)
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "La cuenta destino no existe o está inactiva." };
        
            if (originAccount.Balance < vm.Amount)
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "La cuenta origen no tiene fondos suficientes para esta transferencia." };
        
            var originUser = await _userManager.FindByIdAsync(originAccount.UserId);
            var destUser = await _userManager.FindByIdAsync(destAccount.UserId);

            if (originUser == null || destUser == null)
            {
                return new ConfirmTransferViewModel { HasError = true, ErrorMessage = "No se pudo encontrar la información de los titulares de las cuentas." };
            }
        
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

            if (user == null)
            {
                return null;
            }

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
                Beneficiary = "RETIRO",
                Status = "APROBADA",
                Concept = "Retiro de efectivo en caja"
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
                    <p>Se ha realizado un retiro de su cuenta de ahorros en caja.</p>
                    <ul>
                        <li><strong>Cuenta:</strong> terminada en {last4Digits}</li>
                        <li><strong>Monto retirado:</strong> RD$ {amount.ToString("N2")}</li>
                        <li><strong>Fecha y hora exacta:</strong> {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}</li>
                    </ul>";
                
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            return true;
        }

        public async Task<bool> ProcessDepositAsync(string accountNumber, decimal amount)
        {
            var accounts = await _accountRepository.FindAsync(a => a.AccountNumber == accountNumber);
            var account = accounts.FirstOrDefault();

            if(account == null || !account.IsActive)
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
                Origin = "DEPÓSITO",
                Beneficiary = accountNumber,
                Status = "APROBADA",
                Concept = "Depósito de efectivo en caja"
            };

            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(account.UserId);
            if(user != null && !string.IsNullOrEmpty(user.Email))
            {
                string last4Digits = account.AccountNumber.Length >= 4 
                    ? account.AccountNumber.Substring(account.AccountNumber.Length - 4) 
                    : account.AccountNumber;

                string subject = $"Depósito realizado a su cuenta {last4Digits}";

                string body = $@"
                    <h3>Hola {user.FirtsName},</h3>
                    <p>Se ha realizado un depósito en su cuenta de ahorros en caja.</p>
                    <ul>
                        <li><strong>Cuenta:</strong> terminada en {last4Digits}</li>
                        <li><strong>Monto depositado:</strong> RD$ {amount.ToString("N2")}</li>
                        <li><strong>Fecha y hora exacta:</strong> {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")}</li>
                    </ul>";
                
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            return true;
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
                Status = "APROBADA",
                Concept = $"Transferencia enviada a cuenta {destAccountNum}"
            };
    
            var creditTx = new Transaction
            {
                SavingAccountId = destAccount.Id,
                Amount = amount,
                Type = TransactionType.Transfer,
                Origin = originAccountNum,
                Beneficiary = destAccountNum,
                Status = "APROBADA",
                Concept = $"Transferencia recibida desde cuenta {originAccountNum}"
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
                    </ul>";
                await _emailService.SendEmailAsync(originUser.Email, "Transferencia Realizada", body);
            }
    

            return true;
        }

        public async Task<ConfirmCardPaymentViewModel> ValidateCardPaymentAsync(CardPaymentViewModel vm)
        {
            var accounts = await _accountRepository.FindAsync(a => a.AccountNumber == vm.OriginAccountNumber);
            var account = accounts.FirstOrDefault();

            var cards = await _cardRepository.FindAsync(c => c.IdentifierNumber == vm.CardNumber);
            var card = cards.FirstOrDefault();

            if (account == null || !account.IsActive || card == null || !card.IsActive)
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(card.UserId);
            if (user == null) return null;

            return new ConfirmCardPaymentViewModel
            {
                OriginAccountNumber = vm.OriginAccountNumber,
                CardNumber = vm.CardNumber,
                DisplayCardNumber = $"**** **** **** {vm.CardNumber.Substring(vm.CardNumber.Length - 4)}",
                Amount = vm.Amount,
                ClientName = user.FirtsName,
                ClientLastName = user.LastName,
                HasInsufficientFunds = account.Balance < vm.Amount
            };
        }

        public async Task<bool> ProcessCardPaymentAsync(string originAccountNum, string cardNumber, decimal amount)
        {
            var accounts = await _accountRepository.FindAsync(a => a.AccountNumber == originAccountNum);
            var account = accounts.FirstOrDefault();

            var cards = await _cardRepository.FindAsync(c => c.IdentifierNumber == cardNumber);
            var card = cards.FirstOrDefault();

            if (account == null || !account.IsActive || card == null || !card.IsActive || account.Balance < amount)
            {
                return false;
            }

            // Regla: No se puede pagar más de la deuda actual
            decimal paymentAmount = Math.Min(amount, card.AmountDebt);
            if (paymentAmount <= 0) return false;

            account.SetBalance(account.Balance - paymentAmount);
            card.PayDebt(paymentAmount);

            _accountRepository.Update(account);
            _cardRepository.Update(card);

            var transaction = new Transaction
            {
                SavingAccountId = account.Id,
                Amount = paymentAmount,
                Type = TransactionType.Payment,
                Origin = originAccountNum,
                Beneficiary = $"TARJETA {card.IdentifierNumber.Substring(card.IdentifierNumber.Length - 4)}",
                Status = "APROBADA",
                Concept = $"Pago a tarjeta de crédito ****{card.IdentifierNumber.Substring(card.IdentifierNumber.Length - 4)} en caja"
            };

            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(card.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                string last4 = card.IdentifierNumber.Substring(card.IdentifierNumber.Length - 4);
                string body = $@"
                    <h3>Hola {user.FirtsName},</h3>
                    <p>Se ha realizado un pago a su tarjeta de crédito.</p>
                    <ul>
                        <li><strong>Tarjeta:</strong> terminada en {last4}</li>
                        <li><strong>Monto pagado:</strong> RD$ {paymentAmount.ToString("N2")}</li>
                        <li><strong>Balance pendiente:</strong> RD$ {card.AmountDebt.ToString("N2")}</li>
                    </ul>";
                await _emailService.SendEmailAsync(user.Email, "Pago de Tarjeta Recibido", body);
            }

            return true;
        }

        public async Task<ConfirmPaymentViewModel> ValidatePaymentAsync(PaymentViewModel vm)
        {
            var loans = await _loanRepository.FindAsync(l => l.IdentifierNumber == vm.LoanId);
            var loan = loans.FirstOrDefault();

            var accounts = await _accountRepository.FindAsync(a => a.AccountNumber == vm.OriginAccountNumber);
            var account = accounts.FirstOrDefault();

            if (loan == null || !loan.IsActive || account == null || !account.IsActive) return null;

            var user = await _userManager.FindByIdAsync(loan.UserId);
            if (user == null) return null;

            return new ConfirmPaymentViewModel
            {
                LoanId = loan.Id,
                OriginAccountNumber = vm.OriginAccountNumber,
                DisplayIdentifier = loan.IdentifierNumber,
                Amount = vm.Amount,
                ClientName = user.FirtsName,
                ClientLastName = user.LastName,
                HasInsufficientFunds = account.Balance < vm.Amount
            };
        }

        public async Task<bool> ProcessPaymentAsync(string originAccountNum, string loanIdOrIdentifier, decimal amount)
        {
            var loan = await _loanRepository.GetByIdWithSharesAsync(loanIdOrIdentifier);
            if (loan == null)
            {
                var loans = await _loanRepository.FindAsync(l => l.IdentifierNumber == loanIdOrIdentifier, l => l.Shares);
                loan = loans.FirstOrDefault();
            }

            var accounts = await _accountRepository.FindAsync(a => a.AccountNumber == originAccountNum);
            var account = accounts.FirstOrDefault();

            if (loan == null || !loan.IsActive || account == null || !account.IsActive || account.Balance < amount) return false;

            decimal remainingAmount = amount;
            var unpaidShares = loan.Shares.Where(s => !s.IsPaid).OrderBy(s => s.QuotaNumber).ToList();

            foreach (var share in unpaidShares)
            {
                if (remainingAmount >= share.ShareAmount)
                {
                    share.IsPaid = true;
                    share.IsDelayed = false;
                    remainingAmount -= share.ShareAmount;
                }
                else break;
            }

            loan.OutstandingAmount -= amount; 
            if (loan.OutstandingAmount <= 0)
            {
                loan.OutstandingAmount = 0;
                loan.IsActive = false;
            }

            if (loan.Shares.All(s => s.IsPaid) && loan.OutstandingAmount == 0) loan.IsActive = false;
            if (!loan.Shares.Any(s => s.IsDelayed)) loan.PaymentStatus = PaymentStatusType.AlDia;

            account.SetBalance(account.Balance - amount);
            _accountRepository.Update(account);
            _loanRepository.Update(loan);

            var transaction = new Transaction
            {
                SavingAccountId = account.Id, 
                Amount = amount,
                Type = TransactionType.Payment, 
                Origin = originAccountNum,
                Beneficiary = $"PRÉSTAMO {loan.IdentifierNumber}",
                Status = "APROBADA",
                Concept = $"Pago de préstamo {loan.IdentifierNumber} en caja"
            };

            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(loan.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                string body = $@"
                    <h3>Hola {user.FirtsName},</h3>
                    <p>Hemos recibido el pago de su préstamo.</p>
                    <ul>
                        <li><strong>Préstamo N°:</strong> {loan.IdentifierNumber}</li>
                        <li><strong>Monto pagado:</strong> RD$ {amount.ToString("N2")}</li>
                    </ul>";
                await _emailService.SendEmailAsync(user.Email, "Pago de Préstamo Recibido", body);
            }

            return true;
        }
    }
}
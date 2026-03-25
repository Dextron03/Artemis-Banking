using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.ViewModels.Cashier.Deposits;
using Application.ViewModels.Cashier.Withdrawals;
using Application.ViewModels.Cashier.Payments;
using Application.ViewModels.Cashier.Transfers;

namespace Application.Interfaces
{
    public interface ICashierService
    {
        public Task<ConfirmDepositViewModel> ValidateDepositAsync(DepositViewModel vm);
        public Task<bool> ProcessDepositAsync(string accountNumber, decimal amount);
        public Task<ConfirmWithdrawalViewModel> ValidateWithdrawalAsync(WithdrawalViewModel vm);
        public Task<bool> ProcessWithdrawalAsync(string accountNumber, decimal amount);
        public Task<ConfirmTransferViewModel> ValidateTransferAsync(TransferViewModel vm);
        public Task<bool> ProcessTransferAsync(string originAccountNum, string destAccountNum, decimal amount);
        public Task<ConfirmPaymentViewModel> ValidatePaymentAsync(PaymentViewModel vm);
        public Task<bool> ProcessPaymentAsync(string originAccountNum, string loanNumber, decimal amount);
        public Task<ConfirmCardPaymentViewModel> ValidateCardPaymentAsync(CardPaymentViewModel vm);
        public Task<bool> ProcessCardPaymentAsync(string originAccountNum, string cardNumber, decimal amount);


    }
}
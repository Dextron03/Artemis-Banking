using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.ViewModels;

namespace Application.Interfaces
{
    public interface ICashierService
    {
        public Task<ConfirmDepositViewModel> ValidateDepositAsync(DepositViewModel vm);
        public Task<bool> ProcessDepositAsync(string accountNumber, decimal amount);
        public Task<ConfirmWithdrawalViewModel> ValidateWithdrawalAsync(WithdrawalViewModel vm);
        public Task<ConfirmTransferViewModel> ValidateTransferAsync(TransferViewModel vm);
        public Task<bool> ProcessTransferAsync(string originAccountNum, string destAccountNum, decimal amount);
        public Task<ConfirmPaymentViewModel> ValidatePaymentAsync(PaymentViewModel vm);
        public Task<bool> ProcessPaymentAsync(string loanNumber, decimal amount);


    }
}
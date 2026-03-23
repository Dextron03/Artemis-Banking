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
    }
}
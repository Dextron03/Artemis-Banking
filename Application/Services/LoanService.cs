using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class LoanService
    {
        private readonly IGenericRepository<Loan> _loanRepo;
        private readonly IGenericRepository<SavingsAccount> _accountRepo;
        private readonly IGenericRepository<Transaction> _transactionRepo;

        public LoanService(
            IGenericRepository<Loan> loanRepo,
            IGenericRepository<SavingsAccount> accountRepo,
            IGenericRepository<Transaction> transactionRepo)
        {
            _loanRepo = loanRepo;
            _accountRepo = accountRepo;
            _transactionRepo = transactionRepo;
        }

        public async Task CreateLoan(string userId, decimal amount, int months, decimal rate)
        {
            if (amount <= 0)
                throw new Exception("El monto del préstamo debe ser mayor a 0.");

            if (months <= 0)
                throw new Exception("El plazo debe ser mayor a 0.");

            if (rate <= 0)
                throw new Exception("La tasa debe ser mayor a 0.");

            var activeLoan = (await _loanRepo
                .FindAsync(x => x.UserId == userId && x.IsActive))
                .FirstOrDefault();

            if (activeLoan != null)
                throw new Exception("El cliente ya tiene un préstamo activo");

            var account = (await _accountRepo
                .FindAsync(x => x.UserId == userId && x.IsPrincipal))
                .FirstOrDefault();

            if (account == null)
                throw new Exception("El usuario no tiene una cuenta principal.");

            string number;

            do
            {
                number = Loan.GenerateUniqueNumber();
            }
            while ((await _loanRepo.FindAsync(x => x.IdentifierNumber == number)).Any());

            var loan = new Loan
            {
                UserId = userId,
                LoanAmount = amount,
                TermMonths = months,
                InterestRate = rate,
                OutstandingAmount = amount,
                IdentifierNumber = number
            };

            loan.GenerateAmortizationTable();

            await _loanRepo.AddAsync(loan);

            account.SetBalance(account.Balance + amount);
            _accountRepo.Update(account);

            var transaction = new Transaction
            {
                Amount = amount,
                Type = TransactionType.Deposit,
                Origin = loan.IdentifierNumber,
                Beneficiary = account.AccountNumber,
                Status = "APROBADA",
                Concept = "Desembolso de préstamo",
                SavingAccountId = account.Id
            };

            await _transactionRepo.AddAsync(transaction);

            await _loanRepo.SaveChangesAsync();
        }
    }
}

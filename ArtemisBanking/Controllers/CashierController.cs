using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.ViewModels.Cashier.Deposits;
using Application.ViewModels.Cashier.Withdrawals;
using Application.ViewModels.Cashier.Payments;
using Application.ViewModels.Cashier.Transfers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Controllers
{
    [Authorize(Roles = "Cajero")]
    public class CashierController : Controller
    {
        private readonly ICashierService _cashierService;

        public CashierController(ICashierService cashierService)
        {
            _cashierService = cashierService;
        }

        public IActionResult Index()
        {
            // Retornará una vista vacía, 
            // luego pondremos el Dashboard del Cajero aquí.
            return View(); 
        }

        [HttpGet]
        public IActionResult Deposit()
        {
            return View(new DepositViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(DepositViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var confirmVm = await _cashierService.ValidateDepositAsync(vm);

            if(confirmVm == null)
            {
                ModelState.AddModelError("", "El numero de cuenta ingresado no es valido");
                return View(vm);
            }

            return View("ConfirmDeposit", confirmVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessDeposit(ConfirmDepositViewModel vm)
        {
            var result = await _cashierService.ProcessDepositAsync(vm.AccountNumber, vm.Amount);

            if (!result)
            {
                ModelState.AddModelError("", "Ocurrió un error al procesar el depósito en la base de datos.");
                return View("ConfirmDeposit", vm);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Withdrawal()
        {
            return View(new WithdrawalViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdrawal(WithdrawalViewModel vm)
        {
            if(!ModelState.IsValid) return View(vm);

            var confirmVm = await _cashierService.ValidateWithdrawalAsync(vm);

            if(confirmVm == null)
            {
                ModelState.AddModelError("", "El número de cuenta ingresado no es válido o está inactivo.");
                return View(vm);
            }

            if (confirmVm.HasInsufficientFunds)
            {
                ModelState.AddModelError("", $"Fondos insuficientes. El monto ingresado (RD$ {vm.Amount}) supera el balance actual de la cuenta.");
                return View(vm);
            }

            return View("ConfirmWithdrawal", confirmVm);
            
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessWithdrawal(ConfirmWithdrawalViewModel vm)
        {
            var result = await _cashierService.ProcessWithdrawalAsync(vm.AccountNumber, vm.Amount);

            if (!result)
            {
                ModelState.AddModelError("", "Ocurrió un error al procesar el retiro (posibles fondos insuficientes).");
                return View("ConfirmWithdrawal", vm);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Transfer()
        {
            return View(new TransferViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(TransferViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var confirmVm = await _cashierService.ValidateTransferAsync(vm);

            if (confirmVm.HasError)
            {
                ModelState.AddModelError("", confirmVm.ErrorMessage);
                return View(vm);
            }

            return View("ConfirmTransfer", confirmVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessTransfer(ConfirmTransferViewModel vm)
        {
            var result = await _cashierService.ProcessTransferAsync(vm.OriginAccount, vm.DestinationAccount, vm.Amount);

            if (!result)
            {
                ModelState.AddModelError("", "Ocurrió un error al procesar la transferencia.");
                return View("ConfirmTransfer", vm);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Payment()
        {
            return View(new PaymentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(PaymentViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var confirmVm = await _cashierService.ValidatePaymentAsync(vm);

            if (confirmVm == null)
            {
                ModelState.AddModelError("", "El número de préstamo ingresado no existe.");
                return View(vm);
            }

            return View("ConfirmPayment", confirmVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(ConfirmPaymentViewModel vm)
        {
            var result = await _cashierService.ProcessPaymentAsync(vm.LoanId, vm.Amount);

            if (!result)
            {
                ModelState.AddModelError("", "Ocurrió un error al procesar el pago del préstamo.");
                return View("ConfirmPayment", vm);
            }

            return RedirectToAction("Index");
        }

    }
}
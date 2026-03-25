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
        private readonly IDashboardService _dashboardService;

        public CashierController(ICashierService cashierService, IDashboardService dashboardService)
        {
            _cashierService = cashierService;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var dashboard = await _dashboardService.GetCashierDashboardAsync(userId);
            return View(dashboard);
        }

        [HttpGet]
        public IActionResult CardPayment()
        {
            return View(new CardPaymentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CardPayment(CardPaymentViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var confirmVm = await _cashierService.ValidateCardPaymentAsync(vm);

            if (confirmVm == null)
            {
                ModelState.AddModelError("", "La información de la cuenta o tarjeta no es válida.");
                return View(vm);
            }

            if (confirmVm.HasInsufficientFunds)
            {
                ModelState.AddModelError("", "La cuenta de origen no tiene fondos suficientes.");
                return View(vm);
            }

            return View("ConfirmCardPayment", confirmVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCardPayment(ConfirmCardPaymentViewModel vm)
        {
            var result = await _cashierService.ProcessCardPaymentAsync(vm.OriginAccountNumber, vm.CardNumber, vm.Amount);

            if (!result)
            {
                ModelState.AddModelError("", "Ocurrió un error al procesar el pago de la tarjeta.");
                return View("ConfirmCardPayment", vm);
            }

            return RedirectToAction("Index");
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
            var result = await _cashierService.ProcessPaymentAsync(vm.OriginAccountNumber, vm.LoanId, vm.Amount);

            if (!result)
            {
                ModelState.AddModelError("", "Ocurrió un error al procesar el pago del préstamo (posibles fondos insuficientes).");
                return View("ConfirmPayment", vm);
            }

            return RedirectToAction("Index");
        }

    }
}
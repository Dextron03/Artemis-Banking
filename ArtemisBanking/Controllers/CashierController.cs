using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.ViewModels;
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

        public IActionResult Deposit()
        {
            return View(new DepositViewModel());
        }

        [HttpPost]
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
        public async Task<IActionResult> ProcessDeposit(DepositViewModel vm)
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
        public async Task<IActionResult> ProcessWithdrawal(WithdrawalViewModel vm)
        {
            var result = await _cashierService.ProcessDepositAsync(vm.AccountNumber, vm.Amount);

            if (!result)
            {
                ModelState.AddModelError("", "Ocurrió un error al procesar el retiro (posibles fondos insuficientes).");
                return View("ConfirmWithdrawal", vm);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            // Por ahora esto cargará una vista vacía, 
            // luego pondremos el Dashboard del Cajero aquí.
            return View(); 
        }
    }
}
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
    }
}
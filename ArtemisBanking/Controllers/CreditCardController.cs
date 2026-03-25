using Application.DTOs;
using Application.DTOs.CreditCard;
using Application.Interfaces;
using ArtemisBanking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ArtemisBanking.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CreditCardController : Controller
    {
        private readonly ICreditCardService _cardService;
        private const int PageSize = 20;

        public CreditCardController(ICreditCardService cardService)
        {
            _cardService = cardService;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            string? searchIdentity = null,
            bool? statusFilter = null)
        {
            var (items, total) = await _cardService.GetPagedAsync(
                page, PageSize, searchIdentity, statusFilter);

            var vm = new CreditCardIndexViewModel
            {
                Cards = items,
                TotalCount = total,
                CurrentPage = page,
                PageSize = PageSize,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize),
                SearchIdentity = searchIdentity,
                StatusFilter = statusFilter
            };

            return View(vm);
        }

        public async Task<IActionResult> Detail(string id)
        {
            var detail = await _cardService.GetDetailAsync(id);
            if (detail is null) return NotFound();

            return View(new CreditCardDetailViewModel { Card = detail });
        }

        [HttpGet]
        public async Task<IActionResult> AssignStep1(string? searchIdentity = null)
        {
            var result = await _cardService.GetClientsForAssignAsync(searchIdentity);
            var vm = new SelectClientWebViewModel
            {
                Clients = result.Clients,
                AverageDebt = result.AverageDebt,
                SearchIdentity = searchIdentity
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignStep1(SelectClientWebViewModel vm)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(vm.SelectedUserId))
            {
                return RedirectToAction(nameof(AssignStep1),
                    new { searchIdentity = vm.SearchIdentity });
            }
            return RedirectToAction(nameof(AssignStep2), new { userId = vm.SelectedUserId });
        }

        [HttpGet]
        public async Task<IActionResult> AssignStep2(string userId)
        {
            var result = await _cardService.GetClientsForAssignAsync();
            string fullName = "Cliente";
            foreach (var c in result.Clients)
                if (c.UserId == userId) { fullName = c.FullName; break; }

            var vm = new AssignCreditCardViewModel
            {
                UserId = userId,
                ClientFullName = fullName
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStep2(AssignCreditCardViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var adminId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

                await _cardService.AssignCardAsync(new CreateCreditCardDto
                {
                    UserId = vm.UserId,
                    AdminId = adminId,
                    CreditLimit = vm.CreditLimit
                });

                TempData["Success"] = "Tarjeta de crédito asignada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var detail = await _cardService.GetForEditAsync(id);
            if (detail is null) return NotFound();

            var vm = new EditCreditCardViewModel
            {
                Id = detail.Id,
                LastFourDigits = detail.LastFourDigits,
                CurrentDebt = detail.AmountDebt,
                CreditLimit = detail.CreditLimit
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCreditCardViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                await _cardService.UpdateLimitAsync(new UpdateCreditCardDto
                {
                    Id = vm.Id,
                    NewCreditLimit = vm.CreditLimit
                });

                TempData["Success"] = "Límite actualizado y correo enviado al cliente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(string id)
        {
            var detail = await _cardService.GetDetailAsync(id);
            if (detail is null) return NotFound();

            var vm = new CancelCreditCardViewModel
            {
                Id = detail.Id,
                LastFourDigits = detail.LastFourDigits,
                AmountDebt = detail.AmountDebt
            };
            return View(vm);
        }

        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(string id)
        {
            try
            {
                await _cardService.CancelCardAsync(id);
                TempData["Success"] = "Tarjeta cancelada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Cancel), new { id });
            }
        }
        private static System.Collections.Generic.IEnumerable<CreditCardListDto> FilterByStatus(
            System.Collections.Generic.IEnumerable<CreditCardListDto> cards, bool isActive)
            => System.Linq.Enumerable.Where(cards, c => c.IsActive == isActive);
    }
}
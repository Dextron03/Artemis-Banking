using Application.DTOs.SavingsAccount;
using Application.Interfaces;
using Application.ViewModels.SavingsAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ArtemisBanking.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SavingsAccountController : Controller
    {
        private readonly ISavingsAccountService _service;

        public SavingsAccountController(ISavingsAccountService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
    int page = 1,
    string? searchIdentityNumber = null,
    string? filterStatus = null,
    string? filterType = null)
        {
            var dto = await _service.GetPagedAsync(
                page, 20, searchIdentityNumber, filterStatus, filterType);

            // Mapeo DTO → ViewModel en la capa de presentación (correcto)
            var vm = new SavingsAccountIndexViewModel
            {
                Accounts = dto.Items.Select(i => new SavingsAccountRowViewModel
                {
                    Id = i.Id,
                    AccountNumber = i.AccountNumber,
                    ClientFullName = i.ClientFullName,
                    UserId = i.UserId,
                    Balance = i.Balance,
                    IsPrincipal = i.IsPrincipal,
                    IsActive = i.IsActive,
                    CreatedAt = i.CreatedAt
                }).ToList(),
                CurrentPage = dto.CurrentPage,
                TotalPages = dto.TotalPages,
                TotalCount = dto.TotalCount,
                PageSize = dto.PageSize,
                SearchIdentityNumber = dto.SearchIdentityNumber,
                FilterStatus = dto.FilterStatus,
                FilterType = dto.FilterType
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> SelectClient(string searchIdentityNumber = null)
        {
            var clients = await _service.GetActiveClientsAsync(searchIdentityNumber);
            var vm = new SelectSavingsClientViewModel
            {
                Clients = clients,
                SearchIdentityNumber = searchIdentityNumber
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectClient(SelectSavingsClientViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.SelectedClientId))
            {
                vm.Clients = await _service.GetActiveClientsAsync(vm.SearchIdentityNumber);
                ModelState.AddModelError("SelectedClientId", "Debe seleccionar un cliente.");
                return View(vm);
            }

            return RedirectToAction(nameof(CreateAccount), new { clientId = vm.SelectedClientId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateAccount(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return RedirectToAction(nameof(SelectClient));

            var clients = await _service.GetActiveClientsAsync();
            var client = clients.Find(c => c.Id == clientId);
            if (client == null)
                return RedirectToAction(nameof(SelectClient));

            var vm = new CreateSavingsAccountViewModel
            {
                ClientId = clientId,
                ClientFullName = client.FullName,
                ClientIdentityNumber = client.IdentityNumber,
                InitialBalance = 0
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(CreateSavingsAccountViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var dto = new CreateSavingsAccountDto
            {
                UserId = vm.ClientId,
                InitialBalance = vm.InitialBalance,
                AdminUserId = adminId
            };

            var result = await _service.CreateSecondaryAccountAsync(dto);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return View(vm);
            }

            TempData["SuccessMessage"] = "Cuenta de ahorro secundaria asignada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            var dto = await _service.GetDetailAsync(id);
            if (dto == null) return NotFound();

            var vm = new SavingsAccountDetailViewModel
            {
                Id = dto.Id,
                AccountNumber = dto.AccountNumber,
                ClientFullName = dto.ClientFullName,
                UserId = dto.UserId,
                Balance = dto.Balance,
                IsPrincipal = dto.IsPrincipal,
                IsActive = dto.IsActive,
                CreatedAt = dto.CreatedAt,
                Transactions = dto.Transactions.Select(t => new TransactionRowViewModel
                {
                    Id = t.Id,
                    CreatedAt = t.CreatedAt,
                    Amount = t.Amount,
                    TransactionType = t.Type,
                    Origin = t.Origin,
                    Beneficiary = t.Beneficiary,
                    Status = t.Status,
                    Concept = t.Concept
                }).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(string id)
        {
            var dto = await _service.GetCancelInfoAsync(id);
            if (dto == null) return NotFound();

            if (dto.IsPrincipal)
            {
                TempData["ErrorMessage"] = "Las cuentas principales no pueden cancelarse.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new CancelSavingsAccountViewModel
            {
                Id = dto.Id,
                AccountNumber = dto.AccountNumber,
                ClientFullName = dto.ClientFullName,
                Balance = dto.Balance,
                IsPrincipal = dto.IsPrincipal
            };

            return View(vm);
        }

        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(string id)
        {
            try
            {
                await _service.CancelAccountAsync(id);
                TempData["SuccessMessage"] = "La cuenta fue cancelada exitosamente.";
            }
            catch (System.InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
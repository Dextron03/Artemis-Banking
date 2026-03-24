using Application.DTOs.Loan;
using Application.Interfaces;
using Application.ViewModels.Loan;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBanking.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class LoanController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly IMapper _mapper;

        public LoanController(ILoanService loanService, IMapper mapper)
        {
            _loanService = loanService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? identity, bool? status, int page = 1)
        {
            var filter = new LoanFilterDto
            {
                IdentityNumber = identity,
                IsActive = status,
                Page = page,
                PageSize = 20
            };

            var paged = await _loanService.GetPagedAsync(filter);

            var vm = new LoanIndexViewModel
            {
                Loans = _mapper.Map<List<LoanViewModel>>(paged.Items),
                TotalPages = paged.TotalPages,
                CurrentPage = paged.CurrentPage,
                Identity = identity,
                Status = status
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> SelectClient(string? identityNumber)
        {
            var result = await _loanService.GetEligibleClientsAsync(identityNumber);

            var vm = new SelectLoanClientViewModel
            {
                Clients = _mapper.Map<List<ClientLoanViewModel>>(result.Clients),
                AverageDebt = result.AverageDebt,
                SearchIdentity = identityNumber
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectClientPost(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["Error"] = "Debe seleccionar un cliente.";
                return RedirectToAction(nameof(SelectClient));
            }

            return RedirectToAction(nameof(Create), new { userId });
        }

        [HttpGet]
        public IActionResult Create(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return RedirectToAction(nameof(SelectClient));

            var vm = new CreateLoanViewModel { UserId = userId };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLoanViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            int[] allowedMonths = { 6, 12, 18, 24, 30, 36, 42, 48, 54, 60 };
            if (!allowedMonths.Contains(vm.Months))
            {
                ModelState.AddModelError(nameof(vm.Months), "El plazo seleccionado no es válido.");
                return View(vm);
            }
            var risk = await _loanService.EvaluateRiskAsync(vm.UserId, vm.Amount, vm.InterestRate);

            if (risk.IsHighRisk)
            {
                TempData["HR_UserId"] = vm.UserId;
                TempData["HR_Amount"] = vm.Amount.ToString("F2");
                TempData["HR_Rate"] = vm.InterestRate.ToString("F2");
                TempData["HR_Months"] = vm.Months.ToString();
                TempData["HR_RiskType"] = risk.RiskType;
                return RedirectToAction(nameof(HighRisk));
            }

            await CreateLoanInternal(vm.UserId, vm.Amount, vm.InterestRate, vm.Months);
            TempData["Success"] = "Préstamo asignado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult HighRisk()
        {
            if (TempData["HR_UserId"] == null)
                return RedirectToAction(nameof(Index));

            var vm = new HighRiskViewModel
            {
                UserId = TempData["HR_UserId"]!.ToString()!,
                Amount = decimal.Parse(TempData["HR_Amount"]!.ToString()!),
                InterestRate = decimal.Parse(TempData["HR_Rate"]!.ToString()!),
                Months = int.Parse(TempData["HR_Months"]!.ToString()!),
                RiskType = TempData["HR_RiskType"]!.ToString()!
            };

            TempData.Keep();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmHighRisk()
        {
            if (TempData["HR_UserId"] == null)
                return RedirectToAction(nameof(Index));

            string userId = TempData["HR_UserId"]!.ToString()!;
            decimal amount = decimal.Parse(TempData["HR_Amount"]!.ToString()!);
            decimal interest = decimal.Parse(TempData["HR_Rate"]!.ToString()!);
            int months = int.Parse(TempData["HR_Months"]!.ToString()!);

            await CreateLoanInternal(userId, amount, interest, months);
            TempData["Success"] = "Préstamo de alto riesgo asignado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelHighRisk()
        {
            TempData.Remove("HR_UserId");
            TempData.Remove("HR_Amount");
            TempData.Remove("HR_Rate");
            TempData.Remove("HR_Months");
            TempData.Remove("HR_RiskType");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var detail = await _loanService.GetLoanDetailAsync(id);

            var vm = new LoanDetailsViewModel
            {
                LoanId = detail.LoanId,
                IdentifierNumber = detail.IdentifierNumber,
                ClientName = detail.ClientName,
                LoanAmount = detail.LoanAmount,
                InterestRate = detail.InterestRate,
                TermMonths = detail.TermMonths,
                MonthlyPayment = detail.MonthlyPayment,
                Shares = _mapper.Map<List<ShareViewModel>>(detail.Shares)
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var detail = await _loanService.GetLoanDetailAsync(id);
            var vm = new EditLoanRateViewModel
            {
                LoanId = id,
                InterestRate = detail.InterestRate
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditLoanRateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var dto = _mapper.Map<UpdateLoanRateDto>(vm);
            await _loanService.UpdateInterestRateAsync(dto);

            TempData["Success"] = "Tasa de interés actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CreateLoanInternal(
            string userId, decimal amount, decimal interest, int months)
        {
            string adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? throw new InvalidOperationException(
                                 "No se pudo determinar el administrador autenticado.");

            var dto = new CreateLoanDto
            {
                UserId = userId,
                AdminId = adminId,
                Amount = amount,
                InterestRate = interest,
                Months = months
            };

            await _loanService.CreateLoanAsync(dto);
        }
    }
}
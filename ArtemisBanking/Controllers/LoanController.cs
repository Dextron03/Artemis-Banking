using Application.Services;
using Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Identity.Entities;

public class LoanController : Controller
{
    private readonly LoanService _service;
    private readonly IMapper _mapper;

    public LoanController(LoanService service, IMapper mapper, UserManager<AppUser> userManager)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index(string identity, bool? status, int page = 1)
    {
        var (loans, total) = await _service.GetPaged(identity, status, page, 20);

        ViewBag.TotalPages = (int)Math.Ceiling((double)total / 20);
        ViewBag.CurrentPage = page;

        var vm = loans;

        return View(vm);
    }

    public async Task<IActionResult> SelectClient(string identityNumber)
    {
        var model = await _service.GetClientsForLoan(identityNumber);
        return View(model);
    }

    [HttpPost]
    public IActionResult SelectClientPost(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["Error"] = "Debe seleccionar un cliente";
            return RedirectToAction("SelectClient");
        }

        return RedirectToAction("Create", new { userId });
    }

    public IActionResult Create(string userId)
    {
        return View(new CreateLoanViewModel { UserId = userId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLoanViewModel vm)
    {
        var (risk, type) = await _service.EvaluateRisk(vm.UserId, vm.Amount, vm.InterestRate);

        if (risk)
        {
            TempData["UserId"] = vm.UserId;
            TempData["Amount"] = vm.Amount;
            TempData["Interest"] = vm.InterestRate;
            TempData["Months"] = vm.Months;
            TempData["Type"] = type;

            return RedirectToAction("HighRisk");
        }

        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        await _service.CreateLoan(vm.UserId, vm.Amount, vm.InterestRate, vm.Months, adminId);
        return RedirectToAction("Index");
    }

    public IActionResult HighRisk()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmHighRisk()
    {
        var userId = TempData["UserId"].ToString();
        var amount = Convert.ToDecimal(TempData["Amount"]);
        var interest = Convert.ToDecimal(TempData["Interest"]);
        var months = Convert.ToInt32(TempData["Months"]);

        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(adminId))
            throw new Exception("Usuario administrador no autenticado");

        await _service.CreateLoan(userId, amount, interest, months, adminId);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(string id)
    {
        var data = await _service.GetShares(id);
        return View(data);
    }

    public IActionResult Edit(string id)
    {
        ViewBag.Id = id;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, decimal rate)
    {
        await _service.UpdateInterest(id, rate);
        return RedirectToAction("Index");
    }
}
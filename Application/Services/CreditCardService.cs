using Application.DTOs;
using Application.DTOs.CreditCard;
using Application.DTOs.Loan;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Identity.Entities;  
using Microsoft.AspNetCore.Identity;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly ICreditCardRepository _cardRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IEmailService? _emailService;
        private readonly ILoanRepository _loanRepo;

        public CreditCardService( ICreditCardRepository cardRepo, ILoanRepository loanRepo, UserManager<AppUser> userManager, IMapper mapper, IEmailService? emailService = null)
        {
            _cardRepo = cardRepo;
            _userManager = userManager;
            _mapper = mapper;
            _emailService = emailService;
            _loanRepo = loanRepo;
        }

        public async Task<(IEnumerable<CreditCardListDto> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? searchIdentity = null, bool? isActive = null)
        {
            var allCards = (await _cardRepo.GetAllAsync()).ToList();
            var users = _userManager.Users.ToList();

            var filtered = new List<CreditCard>();

            foreach (var card in allCards)
            {
                var user = users.FirstOrDefault(u => u.Id == card.UserId);
                if (user == null) continue;

                // Búsqueda por Cédula (Parcial)
                if (!string.IsNullOrWhiteSpace(searchIdentity))
                {
                    if (!user.IdentityNumber.Contains(searchIdentity.Trim()))
                        continue;
                }

                // Filtrado por Estado
                if (isActive.HasValue)
                {
                    if (card.IsActive != isActive.Value)
                        continue;
                }

                filtered.Add(card);
            }

            var sorted = filtered
                .OrderByDescending(c => c.IsActive)
                .ThenByDescending(c => c.ExpireDate)
                .ToList();

            int total = sorted.Count;
            var items = sorted
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = await MapToListDtosAsync(items);
            return (dtos, total);
        }

        public async Task<(IEnumerable<CreditCardListDto> Items, int TotalCount)>
            GetActivePagedAsync(int page, int pageSize)
        {
            var (cards, total) = await _cardRepo.GetActivePagedAsync(page, pageSize);
            var dtos = await MapToListDtosAsync(cards);
            return (dtos, total);
        }

        public async Task<(IEnumerable<CreditCardListDto> Items, int TotalCount)>
            GetByStatusPagedAsync(bool isActive, int page, int pageSize)
        {
            var (cards, total) = await _cardRepo.GetByStatusPagedAsync(isActive, page, pageSize);
            var dtos = await MapToListDtosAsync(cards);
            return (dtos, total);
        }

        public async Task<IEnumerable<CreditCardListDto>> GetByIdentityNumberAsync(string identityNumber)
        {
            var cards = await _cardRepo.GetByIdentityNumberAsync(identityNumber);
            return await MapToListDtosAsync(cards);
        }

        public async Task<CreditCardDetailDto?> GetDetailAsync(string id)
        {
            var card = await _cardRepo.GetWithConsumptionsAsync(id);
            if (card is null) return null;

            var dto = _mapper.Map<CreditCardDetailDto>(card);
            var user = await _userManager.FindByIdAsync(card.UserId);
            dto.ClientFullName = user is not null ? $"{user.FirtsName} {user.LastName}" : "Cliente desconocido";

            dto.Consumptions = dto.Consumptions
                .OrderByDescending(c => c.ConsumptionDate)
                .ToList();

            return dto;
        }

        public async Task AssignCardAsync(CreateCreditCardDto dto)
        {
            var card = new CreditCard
            {
                UserId = dto.UserId,
                AdminId = dto.AdminId,
                CreditLimit = dto.CreditLimit,
                ExpireDate = BuildExpireDate(),
                IdentifierNumber = await GenerateUniqueCardNumberAsync()
            };

            var cvc = GenerateRandomCvc();
            card.SetCvc(cvc);

            await _cardRepo.AddAsync(card);
            await _cardRepo.SaveChangesAsync();
        }

        public async Task<CreditCardDetailDto?> GetForEditAsync(string id) => await GetDetailAsync(id);

        public async Task<ClientListResultDto> GetClientsForAssignAsync(
        string? searchIdentity = null)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("Cliente");
            var activeUsers = usersInRole
                .Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.Now);

            if (!string.IsNullOrWhiteSpace(searchIdentity))
                activeUsers = activeUsers
                    .Where(u => u.IdentityNumber.Contains(searchIdentity.Trim()));

            var allActiveLoans = (await _loanRepo.FindAsync(l => l.IsActive)).ToList();
            var allActiveCards = (await _cardRepo.FindAsync(c => c.IsActive)).ToList();

            var clientDtos = new List<ClientForAssignDto>();

            foreach (var u in activeUsers)
            {
                decimal loanDebt = allActiveLoans
                    .Where(l => l.UserId == u.Id)
                    .Sum(l => l.OutstandingAmount);

                decimal cardDebt = allActiveCards
                    .Where(c => c.UserId == u.Id)
                    .Sum(c => c.AmountDebt);

                clientDtos.Add(new ClientForAssignDto
                {
                    UserId = u.Id,
                    IdentityNumber = u.IdentityNumber,
                    FullName = $"{u.FirtsName} {u.LastName}",
                    Email = u.Email ?? string.Empty,
                    TotalDebt = loanDebt + cardDebt  
                });
            }

            decimal averageDebt = 0m;
            if (allActiveLoans.Any())
                averageDebt = allActiveLoans.Average(l => l.OutstandingAmount);

            return new ClientListResultDto
            {
                Clients = clientDtos.OrderBy(c => c.FullName).ToList(),
                AverageDebt = Math.Round(averageDebt, 2, MidpointRounding.AwayFromZero)
            };
        }

        public async Task<EligibleClientsResultDto> GetEligibleClientsAsync(string? identityNumber)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync("Cliente");
            var activeClients = usersInRole
                .Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.Now)
                .ToList();

            var activeLoans = (await _loanRepo.FindAsync(l => l.IsActive)).ToList();
            var clientsWithActiveLoan = activeLoans
                .Select(l => l.UserId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var allLoans = (await _loanRepo.GetAllAsync()).ToList();
            var eligible = new List<EligibleClientDto>();

            foreach (var user in activeClients)
            {
                if (clientsWithActiveLoan.Contains(user.Id)) continue;

                if (!string.IsNullOrWhiteSpace(identityNumber) &&
                    !user.IdentityNumber.Contains(identityNumber.Trim()))
                    continue;

                decimal debt = allLoans
                    .Where(l => l.UserId == user.Id && l.IsActive)
                    .Sum(l => l.OutstandingAmount);

                eligible.Add(new EligibleClientDto
                {
                    Id = user.Id,
                    FirstName = user.FirtsName,
                    LastName = user.LastName,
                    IdentityNumber = user.IdentityNumber,
                    Email = user.Email ?? string.Empty,
                    TotalDebt = debt 
                });
            }

            decimal averageDebt = activeLoans.Count > 0
                ? activeLoans.Average(l => l.OutstandingAmount)
                : 0m;

            return new EligibleClientsResultDto
            {
                Clients = eligible.OrderBy(c => c.FirstName).ToList(),
                AverageDebt = Math.Round(averageDebt, 2, MidpointRounding.AwayFromZero)
            };
        }

        public async Task UpdateLimitAsync(UpdateCreditCardDto dto)
        {
            var card = await _cardRepo.GetByIdAsync(dto.Id)
                ?? throw new KeyNotFoundException("Tarjeta no encontrada.");

            if (dto.NewCreditLimit < card.AmountDebt)
                throw new InvalidOperationException("El límite no puede ser menor a la deuda actual.");

            card.UpdateCreditLimit(dto.NewCreditLimit);

            _cardRepo.Update(card);
            await _cardRepo.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(card.UserId);
            if (user is not null && !string.IsNullOrWhiteSpace(user.Email))
            {
                var lastFour = card.IdentifierNumber.Length >= 4
                    ? card.IdentifierNumber[^4..] : card.IdentifierNumber;

                var subject = $"Modificación de límite — Tarjeta *{lastFour}";
                var body = $@"Estimado/a {user.FirtsName} {user.LastName}, Le informamos que el límite de crédito de su tarjeta terminada en {lastFour} ha sido actualizado.aprobado: {dto.NewCreditLimit:C2}";

                if (_emailService is not null)
                    await _emailService.SendEmailAsync(user.Email, subject, body);
            }
        }

        public async Task CancelCardAsync(string id)
        {
            var card = await _cardRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Tarjeta no encontrada.");

            if (card.AmountDebt > 0)
                throw new InvalidOperationException("Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente.");

            card.Cancel();

            _cardRepo.Update(card);
            await _cardRepo.SaveChangesAsync();
        }

        private async Task<List<CreditCardListDto>> MapToListDtosAsync(
            IEnumerable<CreditCard> cards)
        {
            var result = new List<CreditCardListDto>();
            foreach (var card in cards)
            {
                var dto = _mapper.Map<CreditCardListDto>(card);
                var user = await _userManager.FindByIdAsync(card.UserId);
                dto.ClientFullName = user is not null
                    ? $"{user.FirtsName} {user.LastName}"
                    : "Cliente desconocido";
                result.Add(dto);
            }
            return result;
        }

        private static string BuildExpireDate()
        {
            var expiry = DateTime.Now.AddYears(3);
            return expiry.ToString("MM/yy");
        }

        private async Task<string> GenerateUniqueCardNumberAsync()
        {
            string number;
            bool exists;
            var rng = new Random();
            do
            {
                number = string.Concat(
                    Enumerable.Range(0, 16).Select(_ => rng.Next(0, 10).ToString()));
                exists = await _cardRepo.IdentifierNumberExistsAsync(number);
            } while (exists);

            return number;
        }

        private static string GenerateRandomCvc()
        {
            var rng = new Random();
            return rng.Next(0, 1000).ToString("D3");
        }
    }

    internal static class QueryableExtensions
    {
        internal static async Task<IList<T>> ToListAsync_Extension<T>(
            this IQueryable<T> source)
        {
            return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .ToListAsync(source);
        }
    }
}
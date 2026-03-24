using Application.DTOs.CreditCard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICreditCardService
    {
        Task<(IEnumerable<CreditCardListDto> Items, int TotalCount)> GetActivePagedAsync(
            int page, int pageSize);

        Task<(IEnumerable<CreditCardListDto> Items, int TotalCount)> GetByStatusPagedAsync(
            bool isActive, int page, int pageSize);

        Task<IEnumerable<CreditCardListDto>> GetByIdentityNumberAsync(string identityNumber);
        Task<CreditCardDetailDto?> GetDetailAsync(string id);
        Task<ClientListResultDto> GetClientsForAssignAsync(string? searchIdentity = null);
        Task AssignCardAsync(CreateCreditCardDto dto);
        Task<CreditCardDetailDto?> GetForEditAsync(string id);
        Task UpdateLimitAsync(UpdateCreditCardDto dto);
        Task CancelCardAsync(string id);
    }
}
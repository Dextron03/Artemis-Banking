using Application.DTOs.SavingsAccount;

namespace Application.Interfaces
{
    public interface ISavingsAccountService
    {
        Task<PagedSavingsAccountDto> GetPagedAsync(
            int page, int pageSize,
            string? searchIdentityNumber,
            string? filterStatus,
            string? filterType);

        Task<List<ClientForSavingsAccountDto>> GetActiveClientsAsync(
            string? searchIdentityNumber = null);

        Task<SavingsAccount_ResultDto> CreateSecondaryAccountAsync(
            CreateSavingsAccountDto dto);
        Task<SavingsAccountDetailDto?> GetDetailAsync(string id);
        Task<CancelSavingsAccountInfoDto?> GetCancelInfoAsync(string id);

        Task CancelAccountAsync(string id);
    }
}
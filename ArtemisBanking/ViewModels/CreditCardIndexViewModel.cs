using Application.DTOs.CreditCard;

namespace ArtemisBanking.ViewModels
{
    public class CreditCardIndexViewModel
    {
        public IEnumerable<CreditCardListDto> Cards { get; set; } = new List<CreditCardListDto>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public string? SearchIdentity { get; set; }
        public bool? StatusFilter { get; set; }
    }
}

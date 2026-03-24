namespace ArtemisBanking.ViewModels
{
    public class CancelCreditCardViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string LastFourDigits { get; set; } = string.Empty;
        public decimal AmountDebt { get; set; }
    }
}

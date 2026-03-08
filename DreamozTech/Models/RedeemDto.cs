namespace DreamozTech.Models
{
    public class RedeemDto
    {
        public DateTime RedeemDateTime { get; set; }
        public decimal RedeemAmount { get; set; }
        public string? TransactionUrl { get; set; } = default!;
        public string RedeemTitle { get; set; } = default!;
        public bool IsValidated { get; set; }
        public DateTime? ValidationDateTime { get; set; }
        public string? RedeemType { get; set; } = default!;
    }
}

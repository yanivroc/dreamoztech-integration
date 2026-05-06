namespace DreamozTech.Models
{
    public class SquareProductVariation
    {
        public string VariationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}

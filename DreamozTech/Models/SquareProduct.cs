namespace DreamozTech.Models
{
    public class SquareProduct
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<SquareProductVariation> Variations { get; set; } = new();
    }
}

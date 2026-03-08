namespace DreamozTech.Models
{
    public class AttributeDto
    {
        public string Title { get; set; } = default!;
        public string Value { get; set; } = default!;
        public string AttributeType { get; set; } = default!;
        public int? DisplayOrder { get; set; }
    }
}

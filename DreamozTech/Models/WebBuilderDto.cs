namespace DreamozTech.Models
{
    public class WebBuilderDto
    {
        public string? WebTitle { get; set; } = default!;
        public bool ShowSocialShare { get; set; }
        public string? DomainName { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public string? EmailId { get; set; } = default!;
        public string? LogoImage { get; set; } = default!;
        public string? LogoFavicon { get; set; } = default!;
        public string? MenuItems { get; set; } = default!;
        public string? ForeColor { get; set; } = default!;
        public string? BackColor { get; set; } = default!;
        public string? FontFamily { get; set; } = default!;
        public string? DemoDomainName { get; set; } = default!;
        public string? WebDisplayPath { get; set; } = default!;
        public List<WebPageDto> WebPages { get; set; } = default!;
        public List<CategoryDto> Categories { get; set; } = default!;
    }
}

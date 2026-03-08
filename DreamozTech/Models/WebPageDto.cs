namespace DreamozTech.Models
{
    public class WebPageDto
    {
        public string? PageTitle { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public DateTime? DateTime { get; set; }
        public string? SeoDescription { get; set; } = default!;
        public string? SeoKeywords { get; set; } = default!;
        public string? PagePath { get; set; } = default!;
        public string? PageUrl { get; set; } = default!;
        public List<PostDto> Posts { get; set; } = default!;
    }
}

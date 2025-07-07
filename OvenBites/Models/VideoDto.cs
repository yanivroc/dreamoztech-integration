namespace OvenBites.Models
{
    public class VideoDto
    {
        public string VideoPath { get; set; } = default!;
        public string? VideoDescription { get; set; } = default!;
        public int? VideoType { get; set; }
        public int? DisplayOrder { get; set; }
        public string? VideoUrl { get; set; } = default!;
    }
}

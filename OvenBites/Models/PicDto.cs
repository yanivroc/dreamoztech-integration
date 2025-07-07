namespace OvenBites.Models
{
    public class PicDto
    {
        public string PicPath { get; set; } = default!;
        public string? PicDescription { get; set; } = default!;
        public string? PicThumbPath { get; set; } = default!;
        public int? PicPathHeight { get; set; }
        public int? PicPathWidth { get; set; }
        public int? PicThumbHeight { get; set; }
        public int? PicThumbWidth { get; set; }
        public int? DisplayOrder { get; set; }
        public string? PicUrl { get; set; } = default!;
    }
}

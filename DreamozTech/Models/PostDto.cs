namespace DreamozTech.Models
{
    public class PostDto
    {
        public string? BizName { get; set; } = default!;
        public string? BizDesc { get; set; } = default!;
        public string? BizAddress { get; set; } = default!;
        public string? BizSuburb { get; set; } = default!;
        public string? BizPostCode { get; set; } = default!;
        public string? BizLandLine { get; set; } = default!;
        public string? BizFaxNumber { get; set; } = default!;
        public string? BizMobilePhone { get; set; } = default!;
        public string? BizEmail { get; set; } = default!;
        public string? BizWeb { get; set; } = default!;
        public string? BizLat { get; set; } = default!;
        public string? BizLong { get; set; } = default!;
        public DateTime? CreateDateTime { get; set; }
        public string? BizDisplayTitle { get; set; } = default!;
        public string? PostType { get; set; } = default!;
        public string? State { get; set; } = default!;
        public string? Region { get; set; } = default!;
        public string? MetaDesc { get; set; } = default!;
        public string? MetaKey { get; set; } = default!;
        public string? BizCustomTitle { get; set; } = default!;
        public bool BizEnable { get; set; } = default!;
        public bool BizPublic { get; set; } = default!;
        public List<AttributeDto> Attributes { get; set; } = default!;
        public List<PicDto> Pics { get; set; } = default!;
        public List<VideoDto> Videos { get; set; } = default!;
        public List<CategoryDto> Categories { get; set; } = default!;
        public List<MessageDto> Messages { get; set; } = default!;
    }
}

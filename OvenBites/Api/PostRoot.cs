namespace OvenBites.Api
{
    public class Post
    {
        public string BizName { get; set; }
        public string BizDesc { get; set; }
        public string BizAddress { get; set; }
        public string BizSuburb { get; set; }
        public string BizPostCode { get; set; }
        public string BizLandLine { get; set; }
        public string BizFaxNumber { get; set; }
        public string BizMobilePhone { get; set; }
        public string BizEmail { get; set; }
        public string BizWeb { get; set; }
        public string BizLat { get; set; }
        public string BizLong { get; set; }
        public string CreateDateTime { get; set; }
        public string BizDisplayTitle { get; set; }
        public string PostType { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string MetaDesc { get; set; }
        public string MetaKey { get; set; }
        public string BizCustomTitle { get; set; }
        public bool BizEnable { get; set; } = default!;
        public bool BizPublic { get; set; } = default!;
        public List<Attribute> Attributes { get; set; }
        public List<Pic> Pics { get; set; }
        public List<Video> Videos { get; set; }
        public List<Category> Categories { get; set; }
    }

    public class Attribute
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string AttributeType { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class Pic
    {
        public string PicPath { get; set; }
        public string PicDescription { get; set; }
        public string PicThumbPath { get; set; }
        public int? PicPathHeight { get; set; }
        public int? PicPathWidth { get; set; }
        public int? PicThumbHeight { get; set; }
        public int? PicThumbWidth { get; set; }
        public int DisplayOrder { get; set; }
        public string PicUrl { get; set; }
    }

    public class Video
    {
        public string VideoPath { get; set; }
        public string VideoDescription { get; set; }
        public int VideoType { get; set; }
        public int DisplayOrder { get; set; }
        public string VideoUrl { get; set; }
    }

    public class Category
    {
        public string CategoryTitle { get; set; }
        public string CategoryDisplayTitle { get; set; }
        public string ImagePath { get; set; }
    }

    public class Message
    {
        public List<Post> Posts { get; set; }
        public Member Member { get; set; }
    }

    public class PostRoot
    {
        public Message Message { get; set; }
    }
}
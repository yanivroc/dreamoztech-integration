using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace OvenBites.Models
{
    /// <summary>
    /// Response Model
    /// </summary>
    [JsonObject(IsReference = false)]
    public class ResponseViewModel : ProblemDetails
    {
        /// <summary>
        /// Response Constructor
        /// </summary>
        public ResponseViewModel(string _title, object _message, string _type, MemberDto? _member = null, List<PointsDto>? _points = null, List<InvoiceDto>? _invoices = null, List<MessageDto>? _messages = null, List<ContactDto>? _contacts = null, List<RedeemDto>? _redeems = null, List<PostDto>? _posts = null, List<WebBuilderDto>? _webs = null, ProductDto? _products = null, GeneralDto? _setting = null, ApiDto? _api = null, string _token = null)
        {
            this.Message = _message;
            this.Title = _title;
            this.Type = _type;
            this.Member = _member ?? new MemberDto();
            this.Points = _points ?? new List<PointsDto>();
            this.Invoices = _invoices ?? new List<InvoiceDto>();
            this.Messages = _messages ?? new List<MessageDto>();
            this.Contacts = _contacts ?? new List<ContactDto>();
            this.Redeems = _redeems ?? new List<RedeemDto>();
            this.Posts = _posts ?? new List<PostDto>();
            this.Webs = _webs ?? new List<WebBuilderDto>();
            this.Products = _products ?? new ProductDto();
            this.Setting = _setting ?? new GeneralDto();
            this.API = _api ?? new ApiDto();
            this.Token = _token;
        }
        /// <summary>
        /// Message
        /// </summary>
        public object Message { get; set; }
        /// <summary>
        /// Type
        /// </summary>
        public new string Type { get; set; }
        /// <summary>
        /// Member
        /// </summary>
        public MemberDto Member { get; set; }
        /// <summary>
        /// Points
        /// </summary>
        public List<PointsDto> Points { get; set; }
        /// <summary>
        /// Invoice
        /// </summary>
        public List<InvoiceDto> Invoices { get; set; }
        /// <summary>
        /// Message
        /// </summary>
        public List<MessageDto> Messages { get; set; }
        /// <summary>
        /// Contact
        /// </summary>
        public List<ContactDto> Contacts { get; set; }
        /// <summary>
        /// Redeem
        /// </summary>
        public List<RedeemDto> Redeems { get; set; }
        /// <summary>
        /// Post
        /// </summary>
        public List<PostDto> Posts { get; set; }
        /// <summary>
        /// Web
        /// </summary>
        public List<WebBuilderDto> Webs { get; set; }
        /// <summary>
        /// Product
        /// </summary>
        public ProductDto Products { get; set; }
        /// <summary>
        /// Setting
        /// </summary>
        public GeneralDto Setting { get; set; }
        /// <summary>
        /// API
        /// </summary>
        public ApiDto API { get; set; }
        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }
    }
}

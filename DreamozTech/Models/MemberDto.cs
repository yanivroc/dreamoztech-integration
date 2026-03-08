using System.Text.Json.Serialization;

namespace DreamozTech.Models
{
    public class MemberDto
    {
        public string? MemberFullName { get; set; } = default!;
        public string MemberEmail { get; set; } = default!;
        public DateTime? CreateDateTime { get; set; }
        public string? MemberDirectory { get; set; } = default!;
        public string? ProfilePicture { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public string? Address { get; set; } = default!;
        public string? Country { get; set; } = default!;
        public string? State { get; set; } = default!;
        public string? Region { get; set; } = default!;
        public string? GoogleProfile { get; set; } = default!;
        public string? MemberDisplayPath { get; set; } = default!;
        public string? MobileNumber { get; set; } = default!;
        public string? LandLine { get; set; } = default!;
        public string? Suburb { get; set; } = default!;
        public string? PostCode { get; set; } = default!;
        public string? FacebookProfile { get; set; } = default!;
        public string? TwitterProfile { get; set; } = default!;
        public string? InstagramProfile { get; set; } = default!;
        public string? YoutubeProfile { get; set; } = default!;
        public string? LinkedinProfile { get; set; } = default!;
        public string? TikTokProfile { get; set; } = default!;
        public string? SnapChatProfile { get; set; } = default!;
        public string? PinInterestProfile { get; set; } = default!;
        public string? RedditProfile { get; set; } = default!;
        public string? DiscordProfile { get; set; } = default!;
        public string? Website { get; set; } = default!;
        public string? MetaDesc { get; set; } = default!;
        public string? MetaKey { get; set; } = default!;
        public string? BizLat { get; set; } = default!;
        public string? BizLong { get; set; } = default!;
        public string? CustomerName { get; set; } = default!;
    }
}

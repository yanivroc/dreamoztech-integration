namespace OvenBites.Api
{
    public class Member
    {
        public string MemberFullName { get; set; }
        public string MemberEmail { get; set; }
        public DateTime CreateDateTime { get; set; }
        public string MemberDirectory { get; set; }
        public string ProfilePicture { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string GoogleProfile { get; set; }
        public string MemberDisplayPath { get; set; }
        public string MobileNumber { get; set; }
        public string LandLine { get; set; }
        public string Suburb { get; set; }
        public string PostCode { get; set; }
        public string FacebookProfile { get; set; }
        public string TwitterProfile { get; set; }
        public string InstagramProfile { get; set; }
        public string YoutubeProfile { get; set; }
        public string LinkedinProfile { get; set; }
        public string TikTokProfile { get; set; }
        public string SnapChatProfile { get; set; }
        public string PinInterestProfile { get; set; }
        public string RedditProfile { get; set; }
        public string DiscordProfile { get; set; }
        public string Website { get; set; }
        public string MetaDesc { get; set; }
        public string MetaKey { get; set; }
        public double BizLat { get; set; }
        public double BizLong { get; set; }
        public string CustomerName { get; set; }
    }

    public class MemberRoot
    {
        public Member Member { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string Detail { get; set; }
        public string Instance { get; set; }
    }
}

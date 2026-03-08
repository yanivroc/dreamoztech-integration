namespace DreamozTech.Models
{
    public class MessageDto
    {
        public string ContactedPersonName { get; set; } = default!;
        public string ContactedPersonEmail { get; set; } = default!;
        public string ContactedPersonDescription { get; set; } = default!;
        public DateTime ContactedDateTime { get; set; }
        public string MobilePhone { get; set; } = default!;
    }
}

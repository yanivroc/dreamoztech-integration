namespace DreamozTech.Models
{
    public class ApiDto
    {
        public string APIKey { get; set; } = default!;
        public string APISecret { get; set; } = default!;
        public string APIType { get; set; } = default!;
        public bool APIStatus { get; set; }
    }
}

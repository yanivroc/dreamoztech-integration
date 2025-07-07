namespace OvenBites.Models
{
    public class PointsDto
    {
        public int PointType { get; set; }
        public string PagePath { get; set; } = default!;
        public decimal Point { get; set; }
        public DateTime PointDateTime { get; set; }
        public bool IsConsumed { get; set; }
        public string RequestUrl { get; set; } = default!;
    }
}

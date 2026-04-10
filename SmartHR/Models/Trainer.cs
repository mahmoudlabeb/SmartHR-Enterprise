namespace SmartHR.Models
{
    public class Trainer : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Expertise { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}

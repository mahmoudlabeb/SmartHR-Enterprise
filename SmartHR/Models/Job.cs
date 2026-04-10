namespace SmartHR.Models
{
    public class Job : BaseEntity
    {
        public string JobTitle { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public virtual Department? Department { get; set; }
        public int NumberOfVacancies { get; set; }
        public string Experience { get; set; } = string.Empty;
        public DateTime ExpireDate { get; set; }
        public string Status { get; set; } = "Open";

        public virtual ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
    }
}

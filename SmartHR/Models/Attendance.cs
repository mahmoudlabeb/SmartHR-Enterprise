namespace SmartHR.Models
{
    public class Attendance : BaseEntity
    {
        public int EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        public DateTime Date { get; set; }
        public TimeSpan? PunchIn { get; set; }
        public TimeSpan? PunchOut { get; set; }
        public string Status { get; set; } = "Present"; // Present, Absent, Late
    }
}

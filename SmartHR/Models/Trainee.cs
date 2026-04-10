namespace SmartHR.Models
{
    public class Trainee : BaseEntity
    {
        public int TrainingId { get; set; }
        public virtual Training? Training { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }
    }
}

namespace SmartHR.Models
{
    public class TaskAssignment : BaseEntity
    {
        public int TaskItemId { get; set; }
        public virtual TaskItem? TaskItem { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }
    }
}

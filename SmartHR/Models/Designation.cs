namespace SmartHR.Models
{
    public class Designation : BaseEntity
    {
        public string Title { get; set; } = string.Empty;

        public int DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}

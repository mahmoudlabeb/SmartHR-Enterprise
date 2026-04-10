namespace SmartHR.Models
{
    public class Department : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        
        public virtual ICollection<Designation> Designations { get; set; } = new List<Designation>();
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}

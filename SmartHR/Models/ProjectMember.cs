namespace SmartHR.Models
{
    public class ProjectMember : BaseEntity
    {
        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        public string RoleInProject { get; set; } = "Team Member";
    }
}

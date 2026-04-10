namespace SmartHR.Models
{
    public class Employee : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal BasicSalary { get; set; }
        public string? PhotoPath { get; set; }
        public bool IsActive { get; set; } = true;

        // العلاقات
        public int DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        public int DesignationId { get; set; }
        public virtual Designation? Designation { get; set; }
        // للعطلات والإجازات السنوية
        public int AnnualLeaveBalance { get; set; } = 21;

        // للربط مع نظام الدخول Identity (AspNetUsers)
        public string? UserId { get; set; }
    }
}

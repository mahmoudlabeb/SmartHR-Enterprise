using Microsoft.AspNetCore.Identity;
using System;
namespace SmartHR.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public decimal BasicSalary { get; set; }
        public DateTime HireDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // يمكنك إضافة العلاقات مع الأقسام هنا
        public int? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        public bool ReceiveEmailNotifications { get; set; } = true;
        public bool ReceivePushNotifications { get; set; } = true;
    }
}

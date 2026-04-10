using System;
using System.ComponentModel.DataAnnotations;

namespace SmartHR.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العميل أو الممثل مطلوب")]
        public string FullName { get; set; } = string.Empty;

        public string? CompanyName { get; set; } // علامة الاستفهام تعني أنه اختياري (يمنع خطأ Null)

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; }

        public bool IsActive { get; set; } = true; // افتراضياً العميل يكون نشطاً عند إضافته

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
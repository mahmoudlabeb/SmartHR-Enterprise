using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHR.Models
{
    public class Candidate
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string Phone { get; set; } = string.Empty;

        // الخصائص التي تسببت في الخطأ
        public string? ResumeUrl { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";

        // اسم الربط الصحيح هو JobPostingId وليس JobId
        [Required]
        public int JobPostingId { get; set; }

        [ForeignKey("JobPostingId")]
        public virtual JobPosting? JobPosting { get; set; }
    }
}
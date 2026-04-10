using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartHR.Models
{
    public class TrainingProgram
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الدورة مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "اسم المدرب أو الجهة مطلوب")]
        public string Instructor { get; set; } = string.Empty;

        public string Status { get; set; } = "Upcoming"; // Upcoming, Ongoing, Completed

        [Required(ErrorMessage = "تاريخ البداية مطلوب")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "تاريخ النهاية مطلوب")]
        public DateTime EndDate { get; set; }

        // العلاقة مع الموظفين (المتدربين) - علاقة متعدد إلى متعدد (Many-to-Many)
        // علامة ? تمنع خطأ Null Reference
        public virtual ICollection<Employee>? Trainees { get; set; }
    }
}
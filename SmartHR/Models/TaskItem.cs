using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHR.Models
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان المهمة مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed

        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // العلاقة مع المشروع (اختيارية)
        public int? ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }

        // الخصائص التي كانت مفقودة (العلاقة مع الموظف)
        [Required(ErrorMessage = "يجب إسناد المهمة لموظف")]
        public int AssignedToId { get; set; }

        [ForeignKey("AssignedToId")]
        public virtual Employee? AssignedEmployee { get; set; }
    }
}
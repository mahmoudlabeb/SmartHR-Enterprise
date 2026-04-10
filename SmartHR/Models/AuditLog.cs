using System;
using System.ComponentModel.DataAnnotations;

namespace SmartHR.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string TableName { get; set; } = null!;

        [StringLength(255)]
        public string? PrimaryKeyField { get; set; }

        [StringLength(255)]
        public string? PrimaryKeyValue { get; set; }

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } = null!; // Insert, Update, Delete

        [StringLength(255)]
        public string? PropertyName { get; set; }

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        [StringLength(255)]
        public string? ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}

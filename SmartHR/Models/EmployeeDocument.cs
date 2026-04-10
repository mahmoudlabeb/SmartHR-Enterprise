using System;
using System.ComponentModel.DataAnnotations;

namespace SmartHR.Models
{
    public class EmployeeDocument : BaseEntity
    {
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } // Passport, National ID, Contract, Training, Health Insurance, etc.

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Relationship
        public virtual Employee? Employee { get; set; }
    }
}

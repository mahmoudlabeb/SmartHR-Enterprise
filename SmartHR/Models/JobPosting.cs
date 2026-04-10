using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartHR.Models
{
    public class JobPosting
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "المسمى الوظيفي مطلوب")]
        public string Title { get; set; } = string.Empty;

        public string? Department { get; set; }

        public string? Location { get; set; }

        public string Type { get; set; } = "Full-Time"; // Full-Time, Part-Time, Remote, Contract

        public string Status { get; set; } = "Open"; // Open, Closed

        [Required(ErrorMessage = "وصف الوظيفة مطلوب")]
        public string Description { get; set; } = string.Empty;

        public string? Requirements { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // العلاقة مع المتقدمين (وظيفة واحدة لها عدة متقدمين)
        public virtual ICollection<Candidate>? Candidates { get; set; }
    }
}
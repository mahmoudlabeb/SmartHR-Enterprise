using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHR.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ãæÖæÚ ÇáÊĞßÑÉ ãØáæÈ")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "ÇáÊİÇÕíá ãØáæÈÉ")]
        public string Description { get; set; } = string.Empty;

        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        public string Status { get; set; } = "Open"; // Open, InProgress, Closed

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? AttachmentUrl { get; set; }

        // --- ÇáÑÈØ ãÚ ÇáãæÙİ (ÇáÃÓãÇÁ ÇáãæÍÏÉ) ---
        [Required(ErrorMessage = "íÌÈ ÊÍÏíÏ ÇáãæÙİ ãŞÏã ÇáØáÈ")]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        // --- ÇáÑÈØ ãÚ ÇáÊÚáíŞÇÊ (SignalR) ---
        public virtual ICollection<TicketComment>? Comments { get; set; }
    }
}

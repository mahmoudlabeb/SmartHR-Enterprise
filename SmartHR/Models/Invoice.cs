// Models/Invoice.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHR.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
        public string Type { get; set; } = "Sales"; // Sales, Purchase, Expense
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(15);
        public string Status { get; set; } = "Unpaid"; // Unpaid, Paid, Overdue

        // الخصائص المالية
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }

        // العلاقة مع العميل (اختيارية في حالة المصروفات العامة)
        public int? ClientId { get; set; }
        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }

        public virtual ICollection<InvoiceItem>? Items { get; set; }
    }
}


namespace SmartHR.Models
{
    public class Leave : BaseEntity
    {
        public int EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        public string LeaveType { get; set; } = "Casual"; // Casual, Medical, Annual
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public int? ApprovedByEmployeeId { get; set; } // المدير الذي وافق
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; } // سبب الرفض إن وُجد
    }
}

namespace SmartHR.Models
{
    // ============================================================
    // Centralized string constants replacing magic strings across
    // the application. Using static classes keeps DB columns as
    // nvarchar while giving full compile-time safety in C# code.
    // ============================================================

    /// <summary>Status values for leave requests.</summary>
    public static class LeaveStatus
    {
        public const string Pending  = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }

    /// <summary>Types of leave an employee can request.</summary>
    public static class LeaveType
    {
        public const string Casual  = "Casual";
        public const string Medical = "Medical";
        public const string Annual  = "Annual";
    }

    /// <summary>Priority levels used by Tasks and Tickets.</summary>
    public static class PriorityLevel
    {
        public const string Low    = "Low";
        public const string Medium = "Medium";
        public const string High   = "High";
    }

    /// <summary>Status values for TaskItem records.</summary>
    public static class TaskStatus
    {
        public const string Pending    = "Pending";
        public const string InProgress = "InProgress";
        public const string Completed  = "Completed";
    }

    /// <summary>Status values for Ticket records.</summary>
    public static class TicketStatus
    {
        public const string Open       = "Open";
        public const string InProgress = "InProgress";
        public const string Closed     = "Closed";
    }

    /// <summary>Status values for Salary/PaySlip records.</summary>
    public static class SalaryStatus
    {
        public const string Pending = "Pending";
        public const string Paid    = "Paid";
    }

    /// <summary>Status values for Project records.</summary>
    public static class ProjectStatus
    {
        public const string Active    = "Active";
        public const string OnHold    = "OnHold";
        public const string Completed = "Completed";
    }

    /// <summary>Status values for Attendance records.</summary>
    public static class AttendanceStatus
    {
        public const string Present = "Present";
        public const string Absent  = "Absent";
        public const string Late    = "Late";
    }

    /// <summary>Status values for Recruitment Candidates.</summary>
    public static class CandidateStatus
    {
        public const string Applied    = "Applied";
        public const string Shortlisted = "Shortlisted";
        public const string Interview  = "Interview";
        public const string Hired      = "Hired";
        public const string Rejected   = "Rejected";
    }

    /// <summary>Status values for Invoice records.</summary>
    public static class InvoiceStatus
    {
        public const string Draft   = "Draft";
        public const string Sent    = "Sent";
        public const string Paid    = "Paid";
        public const string Overdue = "Overdue";
    }

    /// <summary>Application roles — mirrors the seeded Identity roles.</summary>
    public static class AppRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin      = "Admin";
        public const string IT         = "IT";
        public const string HR         = "HR";
        public const string Manager    = "Manager";
        public const string Employee   = "Employee";
        public const string Client     = "Client";
    }
}

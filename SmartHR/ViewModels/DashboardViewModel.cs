namespace SmartHR.ViewModels
{
    /// <summary>
    /// Strongly-typed ViewModel for the Dashboard Index view.
    /// Replaces multiple ViewBag entries with a single, testable model.
    /// </summary>
    public class DashboardViewModel
    {
        public bool IsAdmin { get; set; }

        // Global metrics (Admin / HR)
        public int TotalEmployees   { get; set; }
        public int TotalDepartments { get; set; }
        public int ActiveProjects   { get; set; }
        public int OpenTickets      { get; set; }
        public int PendingTasks     { get; set; }
        public int PendingLeaves    { get; set; }

        // Employee metrics
        public int     MyOpenTasks     { get; set; }
        public int     MyPendingLeaves { get; set; }
        public string? EmployeeName    { get; set; }

        // Chart Data
        public List<int> AttendanceWeeklyData { get; set; } = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
        public decimal TotalSalariesPaid { get; set; }
        public int TotalLeavesApproved { get; set; }
    }
}

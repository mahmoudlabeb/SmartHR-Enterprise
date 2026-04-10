using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartHR.ViewModels
{
    /// <summary>
    /// Employee Report View Model - Filters and displays employee data
    /// </summary>
    public class EmployeeReportViewModel
    {
        [Display(Name = "Department Filter")]
        public string? DepartmentFilter { get; set; }

        [Display(Name = "Status Filter")]
        public string? StatusFilter { get; set; }

        [Display(Name = "Employees")]
        public List<EmployeeReportItem> Employees { get; set; } = new List<EmployeeReportItem>();
    }

    /// <summary>
    /// Employee Report Item - Individual employee record for reporting
    /// </summary>
    public class EmployeeReportItem
    {
        [Required(ErrorMessage = "Employee name is required")]
        [StringLength(200, ErrorMessage = "Employee name cannot exceed 200 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Department is required")]
        [StringLength(150, ErrorMessage = "Department cannot exceed 150 characters")]
        [Display(Name = "Department")]
        public string Department { get; set; } = "";

        [Required(ErrorMessage = "Job title is required")]
        [StringLength(150, ErrorMessage = "Job title cannot exceed 150 characters")]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = "";

        [Display(Name = "Join Date")]
        public DateTime JoinDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        [Display(Name = "Employment Status")]
        public string Status { get; set; } = "";
    }

    /// <summary>
    /// Attendance Report View Model - Filters attendance data by date range
    /// </summary>
    public class AttendanceReportViewModel
    {
        [Required(ErrorMessage = "From date is required")]
        [Display(Name = "From Date")]
        public DateTime FromDate { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        [Required(ErrorMessage = "To date is required")]
        [Display(Name = "To Date")]
        public DateTime ToDate { get; set; } = DateTime.UtcNow.Date;

        [Display(Name = "Attendance Records")]
        public List<AttendanceReportItem> Records { get; set; } = new List<AttendanceReportItem>();
    }

    /// <summary>
    /// Attendance Report Item - Individual attendance record
    /// </summary>
    public class AttendanceReportItem
    {
        [Required(ErrorMessage = "Employee name is required")]
        [StringLength(200, ErrorMessage = "Employee name cannot exceed 200 characters")]
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; } = "";

        [Range(0, 31, ErrorMessage = "Present days must be between 0 and 31")]
        [Display(Name = "Present Days")]
        public int PresentDays { get; set; }

        [Range(0, 31, ErrorMessage = "Absent days must be between 0 and 31")]
        [Display(Name = "Absent Days")]
        public int AbsentDays { get; set; }

        [Range(0, 1440, ErrorMessage = "Late minutes must be between 0 and 1440")]
        [Display(Name = "Late Minutes")]
        public int LateMinutes { get; set; }
    }

    /// <summary>
    /// Salary Report View Model - Filters salary data by month/year
    /// </summary>
    public class SalaryReportViewModel
    {
        [Required(ErrorMessage = "Month is required")]
        [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
        [Display(Name = "Month")]
        public int Month { get; set; } = DateTime.UtcNow.Month;

        [Required(ErrorMessage = "Year is required")]
        [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100")]
        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.UtcNow.Year;

        [Range(0, double.MaxValue, ErrorMessage = "Total payroll must be a positive number")]
        [Display(Name = "Total Payroll")]
        public decimal TotalPayroll { get; set; }

        [Display(Name = "Salary Records")]
        public List<SalaryReportItem> Salaries { get; set; } = new List<SalaryReportItem>();
    }

    /// <summary>
    /// Salary Report Item - Individual salary record
    /// </summary>
    public class SalaryReportItem
    {
        [Required(ErrorMessage = "Employee name is required")]
        [StringLength(200, ErrorMessage = "Employee name cannot exceed 200 characters")]
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; } = "";

        [Range(0, double.MaxValue, ErrorMessage = "Basic salary must be a positive number")]
        [Display(Name = "Basic Salary")]
        public decimal Basic { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Allowances must be a positive number")]
        [Display(Name = "Allowances & Bonuses")]
        public decimal Allowances { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Deductions must be a positive number")]
        [Display(Name = "Deductions")]
        public decimal Deductions { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Net salary must be a positive number")]
        [Display(Name = "Net Salary")]
        public decimal Net { get; set; }
    }

    /// <summary>
    /// Project Report View Model - Filters project data by status
    /// </summary>
    public class ProjectReportViewModel
    {
        [Display(Name = "Status Filter")]
        public string? StatusFilter { get; set; }

        [Display(Name = "Projects")]
        public List<ProjectReportItem> Projects { get; set; } = new List<ProjectReportItem>();
    }

    /// <summary>
    /// Project Report Item - Individual project record
    /// </summary>
    public class ProjectReportItem
    {
        [Required(ErrorMessage = "Project name is required")]
        [StringLength(300, ErrorMessage = "Project name cannot exceed 300 characters")]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; } = "";

        [Required(ErrorMessage = "Client name is required")]
        [StringLength(200, ErrorMessage = "Client name cannot exceed 200 characters")]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; } = "";

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100 percent")]
        [Display(Name = "Progress Percentage")]
        public int Progress { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        [Display(Name = "Project Status")]
        public string Status { get; set; } = "";
    }
}
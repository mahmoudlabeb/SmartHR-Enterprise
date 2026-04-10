using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartHR.ViewModels
{
    /// <summary>
    /// Permission matrix representing the mapping between roles and modules/permissions
    /// Used for role-based access control (RBAC) configuration
    /// </summary>
    public class PermissionMatrixViewModel
    {
        /// <summary>
        /// List of available system modules for permission assignment
        /// These correspond to ApplicationPermission enum values
        /// </summary>
        [Display(Name = "Available Modules")]
        public List<string> AvailableModules { get; set; } = new List<string>();

        /// <summary>
        /// List of roles and their permission assignments
        /// Each role can have different permission grants across modules
        /// </summary>
        [Display(Name = "Role Permissions")]
        public List<RoleMatrixItem> Roles { get; set; } = new List<RoleMatrixItem>();
    }

    /// <summary>
    /// Represents a single role in the permission matrix
    /// with all its module permissions
    /// </summary>
    public class RoleMatrixItem
    {
        [Required(ErrorMessage = "Role ID is required")]
        [StringLength(450, ErrorMessage = "Role ID cannot exceed 450 characters")]
        [Display(Name = "Role ID")]
        public string RoleId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(256, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 256 characters")]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// List of permissions assigned to this role for each module
        /// </summary>
        [Display(Name = "Module Permissions")]
        public List<ModulePermission> Permissions { get; set; } = new List<ModulePermission>();
    }

    /// <summary>
    /// Represents a single permission assignment for a module within a role
    /// </summary>
    public class ModulePermission
    {
        [Required(ErrorMessage = "Module name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Module name must be between 2 and 100 characters")]
        [Display(Name = "Module Name")]
        public string ModuleName { get; set; } = string.Empty;

        [Display(Name = "Permission Granted")]
        public bool IsGranted { get; set; }
    }

    /// <summary>
    /// Enum for type-safe permission handling across the application
    /// Use this instead of string-based permission checks
    /// </summary>
    public enum ApplicationPermission
    {
        /// <summary>
        /// Employee management module - view, create, edit, delete employees
        /// </summary>
        [Display(Name = "Employees")]
        Employees,

        /// <summary>
        /// Department and organizational structure management
        /// </summary>
        [Display(Name = "Departments")]
        Departments,

        /// <summary>
        /// Project and task management
        /// </summary>
        [Display(Name = "Projects")]
        Projects,

        /// <summary>
        /// Leave and attendance tracking
        /// </summary>
        [Display(Name = "Leave & Attendance")]
        LeaveAttendance,

        /// <summary>
        /// Salary and payroll management
        /// </summary>
        [Display(Name = "Salary & Payroll")]
        Salary,

        /// <summary>
        /// Financial management - invoices, expenses, clients
        /// </summary>
        [Display(Name = "Finance")]
        Finance,

        /// <summary>
        /// Recruitment and candidate management
        /// </summary>
        [Display(Name = "Recruitment")]
        Recruitment,

        /// <summary>
        /// Training program and trainee management
        /// </summary>
        [Display(Name = "Training")]
        Training,

        /// <summary>
        /// Support ticket management
        /// </summary>
        [Display(Name = "Tickets")]
        Tickets,

        /// <summary>
        /// System reports and analytics
        /// </summary>
        [Display(Name = "Reports")]
        Reports,

        /// <summary>
        /// System settings configuration
        /// </summary>
        [Display(Name = "Settings")]
        Settings,

        /// <summary>
        /// Role and permission administration
        /// </summary>
        [Display(Name = "Admin")]
        Admin
    }
}
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartHR.ViewModels
{
    public class EmployeeCreateViewModel
    {
        [Required(ErrorMessage = "RequiredField")]
        [Display(Name = "EmployeeName")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "RequiredField")]
        [EmailAddress(ErrorMessage = "InvalidEmail")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "PhoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "RequiredField")]
        [Display(Name = "HireDate")]
        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "RequiredField")]
        [Display(Name = "BasicSalary")]
        public decimal BasicSalary { get; set; }

        [Display(Name = "IsActive")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "RequiredField")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "RequiredField")]
        [Display(Name = "Designation")]
        public int DesignationId { get; set; }

        [Display(Name = "UserAccount")]
        public string? UserId { get; set; }

        public IEnumerable<SelectListItem>? Departments { get; set; }
        public IEnumerable<SelectListItem>? Designations { get; set; }
        public IEnumerable<SelectListItem>? Users { get; set; }
    }
}

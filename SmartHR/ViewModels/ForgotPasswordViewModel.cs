using System.ComponentModel.DataAnnotations;

namespace SmartHR.ViewModels
{
    /// <summary>
    /// Forgot Password View Model - Used for password recovery requests
    /// </summary>
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }
}
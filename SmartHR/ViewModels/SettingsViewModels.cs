using System.ComponentModel.DataAnnotations;

namespace SmartHR.ViewModels
{
    /// <summary>
    /// Company Information Settings View Model
    /// </summary>
    public class CompanyInfoViewModel
    {
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "Company name must be between 3 and 300 characters")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tax number is required")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Tax number must be between 5 and 50 characters")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Tax number must contain only digits")]
        [Display(Name = "Tax ID Number")]
        public string TaxNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, MinimumLength = 7, ErrorMessage = "Phone number must be between 7 and 20 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        [Display(Name = "Company Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Address must be between 10 and 500 characters")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
    }

    /// <summary>
    /// Localization Settings View Model - Language, timezone, and date format
    /// </summary>
    public class LocalizationViewModel
    {
        [Required(ErrorMessage = "Language is required")]
        [StringLength(10, ErrorMessage = "Language code cannot exceed 10 characters")]
        [RegularExpression(@"^[a-z]{2}(-[A-Z]{2})?$", ErrorMessage = "Language must be a valid locale code (e.g., 'en' or 'ar')")]
        [Display(Name = "Language")]
        public string Language { get; set; } = "ar";

        [Required(ErrorMessage = "Time zone is required")]
        [StringLength(100, ErrorMessage = "Time zone cannot exceed 100 characters")]
        [Display(Name = "Time Zone")]
        public string TimeZone { get; set; } = "Asia/Riyadh";

        [Required(ErrorMessage = "Date format is required")]
        [StringLength(50, ErrorMessage = "Date format cannot exceed 50 characters")]
        [RegularExpression(@"^[dMy/\-\.]+$", ErrorMessage = "Date format must contain valid format characters")]
        [Display(Name = "Date Format")]
        public string DateFormat { get; set; } = "dd/MM/yyyy";
    }

    /// <summary>
    /// Theme Settings View Model - Application appearance and color scheme
    /// </summary>
    public class ThemeViewModel
    {
        [Required(ErrorMessage = "Theme mode is required")]
        [StringLength(20, ErrorMessage = "Theme mode cannot exceed 20 characters")]
        [RegularExpression(@"^(Light|Dark|System)$", ErrorMessage = "Theme mode must be Light, Dark, or System")]
        [Display(Name = "Theme Mode")]
        public string ThemeMode { get; set; } = "Light";

        [Required(ErrorMessage = "Primary color is required")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "Color must be a valid hex code")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Primary color must be a valid hex color code (e.g., #0d6efd)")]
        [Display(Name = "Primary Color")]
        public string PrimaryColor { get; set; } = "#0d6efd";
    }
}
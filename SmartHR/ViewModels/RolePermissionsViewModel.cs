using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartHR.ViewModels
{
    /// <summary>
    /// Role Permissions View Model - Used for assigning permissions to roles
    /// </summary>
    public class RolePermissionsViewModel
    {
        /// <summary>
        /// Unique identifier for the role
        /// </summary>
        [Required(ErrorMessage = "Role ID is required")]
        [StringLength(450, ErrorMessage = "Role ID cannot exceed 450 characters")]
        [Display(Name = "Role ID")]
        public string RoleId { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the role
        /// </summary>
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(256, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 256 characters")]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// List of claims/permissions available for this role
        /// </summary>
        [Display(Name = "Role Claims")]
        public List<RoleClaimViewModel> Claims { get; set; } = new List<RoleClaimViewModel>();
    }

    /// <summary>
    /// Individual claim/permission within a role
    /// </summary>
    public class RoleClaimViewModel
    {
        /// <summary>
        /// Type of claim (e.g., "Permission", "Role")
        /// </summary>
        [Required(ErrorMessage = "Claim type is required")]
        [StringLength(256, ErrorMessage = "Claim type cannot exceed 256 characters")]
        [Display(Name = "Claim Type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Value of the claim
        /// </summary>
        [Required(ErrorMessage = "Claim value is required")]
        [StringLength(256, ErrorMessage = "Claim value cannot exceed 256 characters")]
        [Display(Name = "Claim Value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Whether this claim is assigned to the role
        /// </summary>
        [Display(Name = "Assign Permission")]
        public bool IsSelected { get; set; }
    }
}
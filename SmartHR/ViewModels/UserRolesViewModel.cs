namespace SmartHR.ViewModels
{
    /// <summary>
    /// Represents a single role and whether the target user currently holds it.
    /// Used by the Permissions management screen.
    /// Previously defined inline inside PermissionsController — moved here for
    /// correct separation of concerns.
    /// </summary>
    public class UserRolesViewModel
    {
        public string RoleId    { get; set; } = string.Empty;
        public string RoleName  { get; set; } = string.Empty;
        public bool   IsSelected { get; set; }
    }
}

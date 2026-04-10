using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHR.Models;
using SmartHR.ViewModels;

namespace SmartHR.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.IT}")]
    public class PermissionsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole>   _roleManager;

        public PermissionsController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole>   roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. List all users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // 2. Manage a specific user's roles
        public async Task<IActionResult> Manage(string userId)
        {
            ViewBag.UserId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            ViewBag.UserName = user.FullName ?? user.UserName;

            // ✅ UserRolesViewModel now comes from ViewModels/ (S1 fix)
            var model = new List<UserRolesViewModel>();

            foreach (var role in _roleManager.Roles.ToList())
            {
                model.Add(new UserRolesViewModel
                {
                    RoleId     = role.Id,
                    RoleName   = role.Name ?? string.Empty,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name ?? string.Empty)
                });
            }

            return View(model);
        }

        // 3. Save role changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(List<UserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء إزالة الصلاحيات القديمة.");
                return View(model);
            }

            var selectedRoles = model
                .Where(x => x.IsSelected)
                .Select(y => y.RoleName)
                .ToList();

            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);

            if (!addResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء إضافة الصلاحيات الجديدة.");
                return View(model);
            }

            TempData["SuccessMessage"] = "تم تحديث صلاحيات المستخدم بنجاح.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ✅ FIX S1: The UserRolesViewModel class has been REMOVED from here
    //    and moved to ViewModels/UserRolesViewModel.cs
}
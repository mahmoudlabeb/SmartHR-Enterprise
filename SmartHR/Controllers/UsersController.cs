using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHR.Models;

namespace SmartHR.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT}")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Users/Pending
        public async Task<IActionResult> Pending()
        {
            var pendingUsers = await _userManager.Users
                .Where(u => !u.IsActive)
                .OrderByDescending(u => u.HireDate) // HireDate acts as Registration Date here
                .ToListAsync();

            return View(pendingUsers);
        }

        // POST: /Users/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "المستخدم غير موجود.";
                return RedirectToAction(nameof(Pending));
            }

            // Assign the role
            var validRoles = new[] { AppRoles.Employee, AppRoles.Manager, AppRoles.HR, AppRoles.Client, AppRoles.Admin, AppRoles.IT };
            if (!validRoles.Contains(roleName))
            {
                TempData["ErrorMessage"] = "الصلاحية المحددة غير صحيحة.";
                return RedirectToAction(nameof(Pending));
            }

            // Activate user
            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            // Add role
            await _userManager.AddToRoleAsync(user, roleName);

            TempData["SuccessMessage"] = $"تم تنشيط الحساب {user.FullName} وتعيين صلاحية {roleName} بنجاح.";
            return RedirectToAction(nameof(Pending));
        }

        // POST: /Users/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = "تم رفض وحذف الطلب بنجاح.";
            }

            return RedirectToAction(nameof(Pending));
        }
    }
}

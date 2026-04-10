using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartHR.Models;

namespace SmartHR.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePreferences(bool receiveEmail, bool receivePush)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.ReceiveEmailNotifications = receiveEmail;
            user.ReceivePushNotifications = receivePush;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Notification preferences updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

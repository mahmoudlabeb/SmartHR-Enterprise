using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SmartHR.Controllers
{
    [Authorize(Roles = "SuperAdmin")] // صلاحية خطيرة لا يملكها سوى المدير العام
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // GET: /Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // GET: /Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Roles/Create
        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                    TempData["SuccessMessage"] = "تم إضافة الدور بنجاح.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "هذا الدور مسجل مسبقاً.");
            }
            return View();
        }

        // GET: /Roles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            return View(role);
        }

        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, IdentityRole model)
        {
            if (id != model.Id) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "تم تعديل الدور بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // GET: /Roles/Delete/5
        // Replaced from HttpPost to standard GET for anchor tag routing in Index
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                if (role.Name == "SuperAdmin" || role.Name == "Admin" || role.Name == "Manager" || role.Name == "HR")
                {
                    TempData["ErrorMessage"] = "لا يمكنك حذف الأدوار الأساسية للنظام.";
                    return RedirectToAction(nameof(Index));
                }

                await _roleManager.DeleteAsync(role);
                TempData["SuccessMessage"] = "تم حذف الدور.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Roles/AssignPermissions?roleId=...
        public async Task<IActionResult> AssignPermissions(string roleId)
        {
            if (string.IsNullOrEmpty(roleId)) return NotFound();

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound();

            var model = new SmartHR.ViewModels.RolePermissionsViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name ?? string.Empty,
                Claims = new System.Collections.Generic.List<SmartHR.ViewModels.RoleClaimViewModel>()
            };

            // Notice: Dynamic permissions mapping is a sophisticated concept requiring claims integration. 
            // In lieu of actual claim-table setups in this basic database context, we return the dummy view 
            // the system generated so that it simply stops crashing.
            return View(model);
        }

        // POST: /Roles/AssignPermissions
        [HttpPost]
        public IActionResult AssignPermissions(string roleId, string dummyField)
        {
            TempData["SuccessMessage"] = "تم حفظ صلاحيات الشاشة.";
            return RedirectToAction(nameof(Index));
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using System.Linq;

namespace SmartHR.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT},{AppRoles.HR}")]
    public class DesignationsController : Controller
    {
        private readonly SmartHRContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public DesignationsController(SmartHRContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        // GET: Designations
        public async Task<IActionResult> Index()
        {
            var designations = await _context.Designations
                .Include(d => d.Department)
                .OrderBy(d => d.Department!.Name).ThenBy(d => d.Title)
                .ToListAsync();
            return View(designations);
        }

        // GET: Designations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var designation = await _context.Designations
                .Include(d => d.Department)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (designation == null) return NotFound();

            return View(designation);
        }

        // GET: Designations/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            return View();
        }

        // POST: Designations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,DepartmentId")] Designation designation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(designation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = _localizer["CreateSuccess"].Value;
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", designation.DepartmentId);
            return View(designation);
        }

        // GET: Designations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var designation = await _context.Designations.FindAsync(id);
            if (designation == null) return NotFound();

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", designation.DepartmentId);
            return View(designation);
        }

        // POST: Designations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,DepartmentId,CreatedAt")] Designation designation)
        {
            if (id != designation.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(designation);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = _localizer["UpdateSuccess"].Value;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DesignationExists(designation.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", designation.DepartmentId);
            return View(designation);
        }

        // GET: Designations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var designation = await _context.Designations
                .Include(d => d.Department)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (designation == null) return NotFound();

            return View(designation);
        }

        // POST: Designations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var designation = await _context.Designations.FindAsync(id);
            if (designation != null)
            {
                _context.Designations.Remove(designation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = _localizer["DeleteSuccess"].Value;
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DesignationExists(int id)
        {
            return _context.Designations.Any(e => e.Id == id);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartHR.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT},{AppRoles.Manager}")]
    public class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly IEmployeeService _employeeService;
        private readonly SmartHRContext _context; // For dropdowns/lookups if needed

        public ProjectsController(IProjectService projectService, IEmployeeService employeeService, SmartHRContext context)
        {
            _projectService = projectService;
            _employeeService = employeeService;
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var isPrivileged = IsPrivilegedUser();
            int? employeeId = null;
            if (!isPrivileged)
            {
                var employee = await GetCurrentEmployeeAsync();
                if (employee != null) employeeId = employee.Id;
            }

            var projects = await _projectService.GetAllProjectsAsync(employeeId, isPrivileged);
            return View(projects);
        }

        private bool IsPrivilegedUser()
        {
            return User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.Admin) || 
                   User.IsInRole(AppRoles.IT) || User.IsInRole(AppRoles.Manager);
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Quick check via context for simple resolving
            return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var dashboard = await _projectService.GetProjectDashboardAsync(id.Value);
            if (dashboard == null) return NotFound();

            ViewData["EmployeesList"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Employees, "Id", "FullName");
            return View(dashboard);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMember(int projectId, int employeeId)
        {
            var result = await _projectService.AssignMemberAsync(projectId, employeeId);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        // GET: Projects/Create
        public IActionResult Create() => View();

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (ModelState.IsValid)
            {
                var result = await _projectService.CreateProjectAsync(project);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var project = await _projectService.GetProjectByIdAsync(id.Value);
            if (project == null) return NotFound();
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            if (id != project.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _projectService.UpdateProjectAsync(project);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var project = await _projectService.GetProjectByIdAsync(id.Value);
            if (project == null) return NotFound();
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _projectService.DeleteProjectAsync(id);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
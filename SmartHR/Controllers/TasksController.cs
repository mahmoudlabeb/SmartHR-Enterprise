using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartHR.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager},{AppRoles.IT},{AppRoles.Employee}")]
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IEmployeeService _employeeService;
        private readonly SmartHRContext _context; // For dropdowns

        public TasksController(ITaskService taskService, IEmployeeService employeeService, SmartHRContext context)
        {
            _taskService = taskService;
            _employeeService = employeeService;
            _context = context;
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            var isPrivileged = IsPrivilegedUser();
            int? employeeId = null;
            var currentEmployee = await GetCurrentEmployeeAsync();
            if (currentEmployee != null) employeeId = currentEmployee.Id;

            ViewBag.CurrentEmployeeId = employeeId;
            var tasks = await _taskService.GetAllTasksAsync(employeeId, isPrivileged);
            return View(tasks);
        }

        private bool IsPrivilegedUser()
        {
            return User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.Admin) || 
                   User.IsInRole(AppRoles.HR) || User.IsInRole(AppRoles.IT) || User.IsInRole(AppRoles.Manager);
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _taskService.GetTaskByIdAsync(id.Value);
            if (taskItem == null) return NotFound();

            if (!IsPrivilegedUser() && taskItem.AssignedToId != (await GetCurrentEmployeeAsync())?.Id)
                return Forbid();

            return View(taskItem);
        }

        // GET: Tasks/Create
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager}")]
        public IActionResult Create()
        {
            ViewData["ProjectsList"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["EmployeesList"] = new SelectList(_context.Employees, "Id", "FullName");
            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager}")]
        public async Task<IActionResult> Create(TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                var result = await _taskService.CreateTaskAsync(taskItem);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }

            ViewData["ProjectsList"] = new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);
            ViewData["EmployeesList"] = new SelectList(_context.Employees, "Id", "FullName", taskItem.AssignedToId);
            return View(taskItem);
        }

        // GET: Tasks/Edit/5
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _taskService.GetTaskByIdAsync(id.Value);
            if (taskItem == null) return NotFound();

            ViewData["ProjectsList"] = new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);
            ViewData["EmployeesList"] = new SelectList(_context.Employees, "Id", "FullName", taskItem.AssignedToId);
            return View(taskItem);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager}")]
        public async Task<IActionResult> Edit(int id, TaskItem taskItem)
        {
            if (id != taskItem.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _taskService.UpdateTaskAsync(taskItem, false);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }

            ViewData["ProjectsList"] = new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);
            ViewData["EmployeesList"] = new SelectList(_context.Employees, "Id", "FullName", taskItem.AssignedToId);
            return View(taskItem);
        }

        // GET: Tasks/Delete/5
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _taskService.GetTaskByIdAsync(id.Value);
            if (taskItem == null) return NotFound();

            return View(taskItem);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        // GET: Tasks/TaskAssignment (Kanban view)
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
        public async Task<IActionResult> TaskAssignment()
        {
            var tasks = await _taskService.GetAllTasksAsync(null, true);
            ViewData["ProjectsList"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["EmployeesList"] = new SelectList(_context.Employees, "Id", "FullName");
            return View(tasks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var taskItem = await _taskService.GetTaskByIdAsync(id);
            if (taskItem == null) return NotFound();

            var currentEmployee = await GetCurrentEmployeeAsync();
            var IsAssignee = currentEmployee != null && taskItem.AssignedToId == currentEmployee.Id;

            if (!IsPrivilegedUser() && !IsAssignee)
                return Forbid();

            // Validate flow logic: No going back to Pending
            if (status == "Pending")
            {
                TempData["ErrorMessage"] = "لا يمكن إعادة المهمة لحالة الانتظار بمجرد البدء";
                return RedirectToAction(nameof(Index));
            }

            // Only allow logical progression
            if (taskItem.Status == "Completed")
            {
                TempData["ErrorMessage"] = "المهمة مكتملة بالفعل";
                return RedirectToAction(nameof(Index));
            }

            var result = await _taskService.UpdateTaskStatusAsync(id, status);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
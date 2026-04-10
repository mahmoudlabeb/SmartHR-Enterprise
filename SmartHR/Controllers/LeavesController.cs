using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Hubs;
using SmartHR.Models;
using SmartHR.Services;
using System.Security.Claims;

namespace SmartHR.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager},{AppRoles.IT},{AppRoles.Employee}")]
    public class LeavesController : Controller
    {
        private readonly ILeaveService _leaveService;
        private readonly IEmployeeService _employeeService;
        private readonly SmartHRContext _context; // For dropdowns

        public LeavesController(ILeaveService leaveService, IEmployeeService employeeService, SmartHRContext context)
        {
            _leaveService = leaveService;
            _employeeService = employeeService;
            _context = context;
        }

        // GET: Leaves
        public async Task<IActionResult> Index()
        {
            int? employeeId = null;
            if (User.IsInRole(AppRoles.Employee) && 
                !User.IsInRole(AppRoles.Admin) && 
                !User.IsInRole(AppRoles.SuperAdmin) && 
                !User.IsInRole(AppRoles.HR) && 
                !User.IsInRole(AppRoles.Manager))
            {
                var employee = await GetCurrentEmployeeAsync();
                if (employee != null)
                {
                    employeeId = employee.Id;
                    ViewBag.AnnualLeaveBalance = employee.AnnualLeaveBalance;
                    ViewBag.UsedAnnualLeaves = await _leaveService.GetUsedAnnualLeaveDaysAsync(employee.Id);
                }
            }

            var leaves = await _leaveService.GetLeavesAsync(employeeId);
            return View(leaves);
        }

        // GET: Leaves/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var leave = await _leaveService.GetLeaveByIdAsync(id.Value);
            if (leave == null) return NotFound();

            if (leave.ApprovedByEmployeeId.HasValue)
            {
                var approver = await _employeeService.GetEmployeeByIdAsync(leave.ApprovedByEmployeeId.Value);
                ViewBag.ApproverName = approver?.FullName;
            }

            return View(leave);
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Reusing IEmployeeService if we add GetByUserId, or just use context helper for now
            return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        // GET: Leaves/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
            return View();
        }

        // POST: Leaves/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LeaveType,StartDate,EndDate,Reason,EmployeeId")] Leave leave)
        {
            if (ModelState.IsValid)
            {
                var result = await _leaveService.CreateLeaveRequestAsync(leave);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }

            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", leave.EmployeeId);
            return View(leave);
        }

        // GET: Leaves/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var leave = await _leaveService.GetLeaveByIdAsync(id.Value);
            if (leave == null) return NotFound();

            if (leave.Status != LeaveStatus.Pending)
            {
                TempData["ErrorMessage"] = "لا يمكن تعديل الإجازة بعد اتخاذ قرار بشأنها.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", leave.EmployeeId);
            return View(leave);
        }

        // POST: Leaves/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LeaveType,StartDate,EndDate,Reason,EmployeeId")] Leave leave)
        {
            if (id != leave.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Note: Logic for updating after rejection/approval check should be in service
                // For now, let's simplify or move to service if complex. 
                // Currently, the Create logic covers most validations.
                
                if (await _leaveService.CheckOverlapAsync(leave.EmployeeId, leave.StartDate, leave.EndDate, leave.Id))
                {
                    ModelState.AddModelError(string.Empty, "يوجد إجازة أخرى متداخلة مع هذه التواريخ.");
                }
                else
                {
                    _context.Update(leave);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تعديل طلب الإجازة بنجاح.";
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", leave.EmployeeId);
            return View(leave);
        }

        // POST: Leaves/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager}")]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _leaveService.ApproveLeaveAsync(id, userId!);
            
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        // POST: Leaves/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager}")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _leaveService.RejectLeaveAsync(id, userId!, reason);

            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
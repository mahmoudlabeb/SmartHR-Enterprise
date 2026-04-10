using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.Services;
using System.Security.Claims;

namespace SmartHR.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager},{AppRoles.IT},{AppRoles.Employee}")]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IEmployeeService _employeeService;
        private readonly SmartHRContext _context; // For dropdowns

        public AttendanceController(
            IAttendanceService attendanceService,
            IEmployeeService employeeService,
            SmartHRContext context)
        {
            _attendanceService = attendanceService;
            _employeeService = employeeService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee != null)
            {
                var todayAttendance = await _attendanceService.GetTodayAttendanceForEmployeeAsync(employee.Id);
                ViewBag.HasPunchedIn = todayAttendance != null;
                ViewBag.HasPunchedOut = todayAttendance?.PunchOut != null;
            }

            var records = await _attendanceService.GetAllAttendanceAsync();
            return View(records);
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Reusing IEmployeeService if we add GetByUserId, or just use context helper for now
            return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PunchIn()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
            {
                TempData["ErrorMessage"] = "لم يتم العثور على ملف موظف مرتبط بحسابك.";
                return RedirectToAction("Index", "Dashboard");
            }

            var result = await _attendanceService.PunchInAsync(employee.Id);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PunchOut()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
            {
                TempData["ErrorMessage"] = "لم يتم العثور على ملف موظف مرتبط بحسابك.";
                return RedirectToAction("Index", "Dashboard");
            }

            var result = await _attendanceService.PunchOutAsync(employee.Id);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT},{AppRoles.HR}")]
        public async Task<IActionResult> AdminView(DateTime? date)
        {
            var targetDate = date?.Date ?? DateTime.Today;
            ViewBag.SelectedDate = targetDate.ToString("yyyy-MM-dd");

            var records = await _attendanceService.GetAttendanceByDateAsync(targetDate);
            return View(records);
        }

        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR}")]
        public async Task<IActionResult> Create()
        {
            ViewBag.EmployeeId = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR}")]
        public async Task<IActionResult> Create([Bind("EmployeeId,Date,PunchIn,PunchOut,Status")] Attendance attendance)
        {
            if (ModelState.IsValid)
            {
                var result = await _attendanceService.CreateManualRecordAsync(attendance);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            
            ViewBag.EmployeeId = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName", attendance.EmployeeId);
            return View(attendance);
        }
    }
}
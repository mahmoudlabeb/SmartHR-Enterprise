using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartHR.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager},{AppRoles.HR},{AppRoles.Employee}")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly SmartHRContext _context; // For simple lookups (employees list)

        public ReportsController(IReportService reportService, SmartHRContext context)
        {
            _reportService = reportService;
            _context = context;
        }

        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> PnLReport(int? quarter, int? year)
        {
            year ??= DateTime.UtcNow.Year;
            quarter ??= (DateTime.UtcNow.Month - 1) / 3 + 1;

            var model = await _reportService.GetPnLReportAsync(year.Value, quarter.Value);
            return View(model);
        }

        public async Task<IActionResult> EmployeeAgenda(int? employeeId, int? year)
        {
            year ??= DateTime.UtcNow.Year;
            
            var isPrivileged = IsPrivilegedUser();
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);

            int targetEmployeeId = employeeId ?? currentEmployee?.Id ?? 0;

            if (!isPrivileged && targetEmployeeId != currentEmployee?.Id)
                return Forbid();

            var model = await _reportService.GetEmployeeAgendaAsync(targetEmployeeId, year.Value);
            if (model.TargetEmployee == null) return NotFound();

            ViewBag.EmployeesList = isPrivileged ? new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName") : null;

            return View(model);
        }

        private bool IsPrivilegedUser()
        {
            return User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.HR) || User.IsInRole(AppRoles.Manager);
        }
    }
}

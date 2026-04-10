using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using Microsoft.Extensions.Localization;
using SmartHR.ViewModels;
using SmartHR.Services;
using System.Security.Claims;

namespace SmartHR.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly SmartHRContext _context; // For small resolvers

        public DashboardController(IDashboardService dashboardService, SmartHRContext context)
        {
            _dashboardService = dashboardService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(AppRoles.Client))
                return RedirectToAction("Products", "Home");

            bool isAdminOrHR = User.IsInRole(AppRoles.SuperAdmin) || 
                               User.IsInRole(AppRoles.Admin) || 
                               User.IsInRole(AppRoles.Manager) || 
                               User.IsInRole(AppRoles.HR);

            if (isAdminOrHR)
            {
                var model = await _dashboardService.GetAdminDashboardMetricsAsync();
                return View("AdminDashboard", model);
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
                
                if (employee == null) return Forbid();

                var model = await _dashboardService.GetEmployeeDashboardMetricsAsync(employee.Id);
                return View("EmployeeDashboard", model);
            }
        }
    }
}
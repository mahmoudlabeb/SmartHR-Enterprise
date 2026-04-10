using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.Services;
using System.Security.Claims;

namespace SmartHR.Controllers
{
    public class SalaryController : Controller
    {
        private readonly ISalaryService _salaryService;
        private readonly IEmployeeService _employeeService;
        private readonly SmartHRContext _context; // For dropdowns

        public SalaryController(ISalaryService salaryService, IEmployeeService employeeService, SmartHRContext context)
        {
            _salaryService = salaryService;
            _employeeService = employeeService;
            _context = context;
        }

        // GET: Salary
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT},{AppRoles.HR},{AppRoles.Employee}")]
        public async Task<IActionResult> Index()
        {
            int? employeeId = null;
            if (User.IsInRole(AppRoles.Employee) && 
                !User.IsInRole(AppRoles.Admin) && 
                !User.IsInRole(AppRoles.SuperAdmin) && 
                !User.IsInRole(AppRoles.HR))
            {
                var employee = await GetCurrentEmployeeAsync();
                if (employee != null) employeeId = employee.Id;
            }

            var salaries = await _salaryService.GetSalariesAsync(employeeId);
            return View(salaries);
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        // GET: Salary/Create
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR}")]
        public async Task<IActionResult> Create()
        {
            ViewBag.EmployeeId = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName");
            return View();
        }

        // POST: Salary/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR}")]
        public async Task<IActionResult> Create(Salary salary)
        {
            if (ModelState.IsValid)
            {
                var result = await _salaryService.CreateSalaryAsync(salary);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }

            ViewBag.EmployeeId = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName", salary.EmployeeId);
            return View(salary);
        }

        // GET: Salary/Edit/5
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var salary = await _salaryService.GetSalaryByIdAsync(id.Value);
            if (salary == null) return NotFound();
            
            ViewBag.EmployeeId = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName", salary.EmployeeId);
            return View(salary);
        }

        // POST: Salary/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR}")]
        public async Task<IActionResult> Edit(int id, Salary salary)
        {
            if (id != salary.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _salaryService.UpdateSalaryAsync(salary);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }

            ViewBag.EmployeeId = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName", salary.EmployeeId);
            return View(salary);
        }

        // POST: Salary/MarkAsPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR}")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var result = await _salaryService.MarkAsPaidAsync(id);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        // GET: Salary/PrintPaySlipPdf/5
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT},{AppRoles.HR},{AppRoles.Employee}")]
        public async Task<IActionResult> PrintPaySlipPdf(int id)
        {
            var salary = await _salaryService.GetSalaryByIdAsync(id);
            if (salary == null) return NotFound();

            // Security Check for Employees
            if (User.IsInRole(AppRoles.Employee) && 
                !User.IsInRole(AppRoles.Admin) && 
                !User.IsInRole(AppRoles.SuperAdmin))
            {
                var emp = await GetCurrentEmployeeAsync();
                if (emp == null || salary.EmployeeId != emp.Id) return Forbid();
            }

            string employeeName = salary.Employee?.FullName ?? "UnknownEmployee";

            return new ViewAsPdf("PaySlipTemplate", salary)
            {
                FileName = $"PaySlip_{employeeName}_{salary.CreatedAt:MMM_yyyy}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }
    }
}
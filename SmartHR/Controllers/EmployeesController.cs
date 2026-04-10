using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.Services;
using SmartHR.ViewModels;
using AutoMapper;
using System.Security.Claims;
using SmartHR.Services;
using ClosedXML.Excel;
using System.IO;

namespace SmartHR.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT},{AppRoles.HR},{AppRoles.Manager},{AppRoles.Employee}")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly SmartHRContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public EmployeesController(IEmployeeService employeeService, SmartHRContext context, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _employeeService = employeeService;
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string? search)
        {
            var employees = await _employeeService.GetAllEmployeesAsync(search);
            ViewData["Search"] = search;
            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var content = await _employeeService.GetEmployeesExcelDataAsync();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Employees_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            if (!IsAuthorizedForEmployee(employee.Id)) return Forbid();

            ViewBag.Documents = await _employeeService.GetEmployeeDocumentsAsync(employee.Id);
            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            var model = new EmployeeCreateViewModel();
            PopulateDropdowns(model);
            return View(model);
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _employeeService.CreateEmployeeAsync(model);
                TempData["SuccessMessage"] = $"تم إضافة الموظف {model.FullName} بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(model);
            return View(model);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            var model = _mapper.Map<EmployeeEditViewModel>(employee);

            PopulateDropdowns(model);
            return View(model);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _employeeService.UpdateEmployeeAsync(model);
                    TempData["SuccessMessage"] = $"تم تحديث بيانات {model.FullName} بنجاح.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_employeeService.EmployeeExists(model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(model);
            return View(model);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _employeeService.SoftDeleteEmployeeAsync(id);
            TempData["SuccessMessage"] = "تم حذف الموظف بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns(EmployeeCreateViewModel model)
        {
            model.Departments = new SelectList(_context.Departments, "Id", "Name", model.DepartmentId);
            model.Designations = new SelectList(_context.Designations, "Id", "Title", model.DesignationId);
            var users = _userManager.Users.Select(u => new { u.Id, u.FullName }).ToList();
            model.Users = new SelectList(users, "Id", "FullName", model.UserId);
        }

        // ── Document Vault Actions ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int employeeId, string docType, string? description, DateTime? expiryDate, Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (!IsAuthorizedForEmployee(employeeId)) return Forbid();

            var result = await _employeeService.AddDocumentAsync(employeeId, docType, description, expiryDate, file);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Details), new { id = employeeId });
        }

        public async Task<IActionResult> DownloadDocument(int id)
        {
            var document = await _employeeService.GetDocumentByIdAsync(id);
            if (document == null) return NotFound();

            if (!IsAuthorizedForEmployee(document.EmployeeId)) return Forbid();

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath);
            if (!System.IO.File.Exists(fullPath)) return NotFound();

            var contentType = GetContentType(fullPath);
            return File(System.IO.File.OpenRead(fullPath), contentType, document.FileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _employeeService.GetDocumentByIdAsync(id);
            if (document == null) return NotFound();

            if (!IsAuthorizedForEmployee(document.EmployeeId)) return Forbid();

            var result = await _employeeService.DeleteDocumentAsync(id);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Details), new { id = document.EmployeeId });
        }

        private bool IsAuthorizedForEmployee(int employeeId)
        {
            if (User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.HR) || User.IsInRole(AppRoles.Manager))
                return true;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentEmployee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            return currentEmployee != null && currentEmployee.Id == employeeId;
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }
    }
}
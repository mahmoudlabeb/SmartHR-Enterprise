using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.ViewModels;
using System.Security.Claims;

namespace SmartHR.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly SmartHRContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public EmployeeService(SmartHRContext context, IStringLocalizer<SharedResource> localizer, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _context = context;
            _localizer = localizer;
            _env = env;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(string? search)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.FullName.Contains(search) || e.Email.Contains(search));
            }

            return await query.OrderBy(e => e.FullName).ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task CreateEmployeeAsync(EmployeeCreateViewModel model)
        {
            var employee = new Employee
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                HireDate = model.HireDate,
                BasicSalary = model.BasicSalary,
                IsActive = model.IsActive,
                DepartmentId = model.DepartmentId,
                DesignationId = model.DesignationId,
                UserId = model.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployeeAsync(EmployeeEditViewModel model)
        {
            var employee = await _context.Employees.FindAsync(model.Id);
            if (employee == null) return;

            employee.FullName = model.FullName;
            employee.Email = model.Email;
            employee.PhoneNumber = model.PhoneNumber;
            employee.HireDate = model.HireDate;
            employee.BasicSalary = model.BasicSalary;
            employee.IsActive = model.IsActive;
            employee.DepartmentId = model.DepartmentId;
            employee.DesignationId = model.DesignationId;
            employee.UserId = model.UserId;

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                employee.IsDeleted = true;
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();
            }
        }

        public bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        public async Task<byte[]> GetEmployeesExcelDataAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .AsNoTracking()
                .ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Employees");

            // Headers (Localized)
            ws.Cell(1, 1).Value = _localizer["ID"].Value;
            ws.Cell(1, 2).Value = _localizer["EmployeeName"].Value;
            ws.Cell(1, 3).Value = _localizer["Email"].Value;
            ws.Cell(1, 4).Value = _localizer["PhoneNumber"].Value;
            ws.Cell(1, 5).Value = _localizer["HireDate"].Value;
            ws.Cell(1, 6).Value = _localizer["BasicSalary"].Value;
            ws.Cell(1, 7).Value = _localizer["Department"].Value;
            ws.Cell(1, 8).Value = _localizer["Designation"].Value;
            ws.Row(1).Style.Font.Bold = true;

            int row = 2;
            foreach (var emp in employees)
            {
                ws.Cell(row, 1).Value = emp.Id;
                ws.Cell(row, 2).Value = emp.FullName;
                ws.Cell(row, 3).Value = emp.Email;
                ws.Cell(row, 4).Value = emp.PhoneNumber;
                ws.Cell(row, 5).Value = emp.HireDate.ToString("yyyy-MM-dd");
                ws.Cell(row, 6).Value = emp.BasicSalary;
                ws.Cell(row, 7).Value = emp.Department?.Name ?? "";
                ws.Cell(row, 8).Value = emp.Designation?.Title ?? "";
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ── Document Vault Implementation ──
        public async Task<IEnumerable<EmployeeDocument>> GetEmployeeDocumentsAsync(int employeeId)
        {
            return await _context.EmployeeDocuments
                .Where(d => d.EmployeeId == employeeId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<EmployeeDocument?> GetDocumentByIdAsync(int documentId)
        {
            return await _context.EmployeeDocuments
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Id == documentId);
        }

        public async Task<(bool Success, string Message, EmployeeDocument? Document)> AddDocumentAsync(int employeeId, string docType, string? description, DateTime? expiryDate, Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return (false, "No file uploaded", null);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return (false, "Employee not found", null);

            try
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "documents");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var document = new EmployeeDocument
                {
                    EmployeeId = employeeId,
                    DocumentType = docType,
                    Description = description,
                    ExpiryDate = expiryDate,
                    FileName = file.FileName,
                    FilePath = Path.Combine("uploads", "documents", uniqueFileName),
                    CreatedAt = DateTime.UtcNow
                };

                _context.EmployeeDocuments.Add(document);
                await _context.SaveChangesAsync();

                return (true, _localizer["SaveSuccess"], document);
            }
            catch (Exception ex)
            {
                return (false, $"Error uploading file: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteDocumentAsync(int documentId)
        {
            var document = await _context.EmployeeDocuments.FindAsync(documentId);
            if (document == null) return (false, _localizer["NotFound"]);

            try
            {
                // Delete file from disk
                string fullPath = Path.Combine(_env.WebRootPath, document.FilePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                // Delete from DB (Soft delete because of inheritance, but usually documents are hard deleted if the file is gone)
                // However, SmartHR uses soft-delete filters, so we'll respect that.
                document.IsDeleted = true;
                document.DeletedAt = DateTime.UtcNow;
                _context.EmployeeDocuments.Update(document);
                await _context.SaveChangesAsync();

                return (true, _localizer["DeleteSuccess"]);
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting file: {ex.Message}");
            }
        }
    }
}

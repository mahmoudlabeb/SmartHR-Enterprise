using SmartHR.Models;
using SmartHR.ViewModels;

namespace SmartHR.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync(string? search);
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task CreateEmployeeAsync(EmployeeCreateViewModel model);
        Task UpdateEmployeeAsync(EmployeeEditViewModel model);
        Task SoftDeleteEmployeeAsync(int id);
        bool EmployeeExists(int id);
        Task<byte[]> GetEmployeesExcelDataAsync();

        // ── Document Vault ──
        Task<IEnumerable<EmployeeDocument>> GetEmployeeDocumentsAsync(int employeeId);
        Task<EmployeeDocument?> GetDocumentByIdAsync(int documentId);
        Task<(bool Success, string Message, EmployeeDocument? Document)> AddDocumentAsync(int employeeId, string docType, string? description, DateTime? expiryDate, Microsoft.AspNetCore.Http.IFormFile file);
        Task<(bool Success, string Message)> DeleteDocumentAsync(int documentId);
    }
}

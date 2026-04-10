using SmartHR.Models;

namespace SmartHR.Services
{
    public interface ISalaryService
    {
        Task<IEnumerable<Salary>> GetSalariesAsync(int? employeeId = null);
        Task<Salary?> GetSalaryByIdAsync(int id);
        Task<(bool Success, string Message, Salary? Salary)> CreateSalaryAsync(Salary salary);
        Task<(bool Success, string Message)> UpdateSalaryAsync(Salary salary);
        Task<(bool Success, string Message)> MarkAsPaidAsync(int id);
        decimal CalculateNetSalary(Salary s);
        Task GenerateMonthlySalariesAsync(int month, int year);
    }
}

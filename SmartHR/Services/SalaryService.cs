using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SmartHR.Data;
using SmartHR.Models;

namespace SmartHR.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly SmartHRContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public SalaryService(SmartHRContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IEnumerable<Salary>> GetSalariesAsync(int? employeeId = null)
        {
            var query = _context.Salaries.Include(s => s.Employee).AsQueryable();
            if (employeeId.HasValue)
            {
                query = query.Where(s => s.EmployeeId == employeeId.Value);
            }

            return await query
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.Month)
                .ToListAsync();
        }

        public async Task<Salary?> GetSalaryByIdAsync(int id)
        {
            return await _context.Salaries
                .Include(s => s.Employee)
                .ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<(bool Success, string Message, Salary? Salary)> CreateSalaryAsync(Salary salary)
        {
            salary.NetSalary = CalculateNetSalary(salary);
            salary.Status = SalaryStatus.Pending;
            salary.CreatedAt = DateTime.UtcNow;

            _context.Salaries.Add(salary);
            await _context.SaveChangesAsync();

            return (true, _localizer["CreateSuccess"], salary);
        }

        public async Task<(bool Success, string Message)> UpdateSalaryAsync(Salary salary)
        {
            var existing = await _context.Salaries.AsNoTracking().FirstOrDefaultAsync(s => s.Id == salary.Id);
            if (existing == null) return (false, _localizer["NotFound"]);

            salary.NetSalary = CalculateNetSalary(salary);
            _context.Salaries.Update(salary);
            await _context.SaveChangesAsync();

            return (true, _localizer["UpdateSuccess"]);
        }

        public async Task<(bool Success, string Message)> MarkAsPaidAsync(int id)
        {
            var salary = await _context.Salaries.FindAsync(id);
            if (salary == null) return (false, _localizer["NotFound"]);

            salary.Status = SalaryStatus.Paid;
            salary.PaymentDate = DateTime.Now;
            
            _context.Salaries.Update(salary);
            await _context.SaveChangesAsync();

            return (true, _localizer["SaveSuccess"]);
        }

        public decimal CalculateNetSalary(Salary s)
        {
            var gross = s.BasicSalary + s.HousingAllowance + s.TransportAllowance + s.OtherAllowances;
            var deductions = s.TaxDeduction + s.AbsenceDeduction + s.OtherDeductions;
            return gross - deductions;
        }

        public async Task GenerateMonthlySalariesAsync(int month, int year)
        {
            var activeEmployees = await _context.Employees
                .Where(e => e.IsActive && !e.IsDeleted)
                .ToListAsync();

            var existingSalaryIds = await _context.Salaries
                .Where(s => s.Month == month && s.Year == year)
                .Select(s => s.EmployeeId)
                .ToListAsync();

            var employeesToPay = activeEmployees.Where(e => !existingSalaryIds.Contains(e.Id)).ToList();

            foreach (var emp in employeesToPay)
            {
                var salary = new Salary
                {
                    EmployeeId = emp.Id,
                    Month = month,
                    Year = year,
                    BasicSalary = emp.BasicSalary,
                    HousingAllowance = 0,
                    TransportAllowance = 0,
                    OtherAllowances = 0,
                    TaxDeduction = 0,
                    AbsenceDeduction = 0, // In future, calculate based on AttendanceService
                    OtherDeductions = 0,
                    Status = SalaryStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                salary.NetSalary = CalculateNetSalary(salary);
                _context.Salaries.Add(salary);
            }

            if (employeesToPay.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}

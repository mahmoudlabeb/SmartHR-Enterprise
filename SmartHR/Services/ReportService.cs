using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;

namespace SmartHR.Services
{
    public class ReportService : IReportService
    {
        private readonly SmartHRContext _context;

        public ReportService(SmartHRContext context)
        {
            _context = context;
        }

        public async Task<PnLReportViewModel> GetPnLReportAsync(int year, int quarter)
        {
            var startDate = new DateTime(year, (quarter - 1) * 3 + 1, 1);
            var endDate = startDate.AddMonths(3).AddDays(-1);

            var totalSalaries = await _context.Salaries
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate && p.Status == SalaryStatus.Paid)
                .SumAsync(p => p.NetSalary);

            var totalExpenses = await _context.Expenses
                .Where(e => e.PurchaseDate >= startDate && e.PurchaseDate <= endDate && e.Status == "Approved")
                .SumAsync(e => e.Amount);

            var totalProjectRevenue = await _context.Invoices
                .Where(i => i.Date >= startDate && i.Date <= endDate && i.Status == "Paid")
                .SumAsync(i => i.TotalAmount);

            return new PnLReportViewModel
            {
                Year = year,
                Quarter = quarter,
                StartDate = startDate,
                EndDate = endDate,
                TotalSalaries = totalSalaries,
                TotalExpenses = totalExpenses,
                TotalProjectRevenue = totalProjectRevenue
            };
        }

        public async Task<EmployeeAgendaViewModel> GetEmployeeAgendaAsync(int employeeId, int year)
        {
            var targetEmployee = await _context.Employees.FindAsync(employeeId);
            if (targetEmployee == null) return new EmployeeAgendaViewModel { Year = year };

            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            // Get Approved Leaves
            var approvedLeaves = await _context.Leaves
                .Where(l => l.EmployeeId == employeeId && l.Status == LeaveStatus.Approved && l.StartDate >= startDate && l.StartDate <= endDate)
                .ToListAsync();

            // Total Hours Worked (from attendance)
            var attendances = await _context.Attendances
                .Where(a => a.EmployeeId == employeeId && a.Date >= startDate && a.Date <= endDate && a.PunchIn != null && a.PunchOut != null)
                .ToListAsync();

            var totalHoursWorked = attendances.Sum(a => (a.PunchOut!.Value - a.PunchIn!.Value).TotalHours);

            return new EmployeeAgendaViewModel
            {
                Year = year,
                TargetEmployee = targetEmployee,
                TotalHoursWorked = Math.Round(totalHoursWorked, 2),
                ApprovedLeaves = approvedLeaves
            };
        }
    }
}

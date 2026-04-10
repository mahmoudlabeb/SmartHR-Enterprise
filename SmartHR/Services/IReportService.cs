using SmartHR.Models;

namespace SmartHR.Services
{
    public interface IReportService
    {
        Task<PnLReportViewModel> GetPnLReportAsync(int year, int quarter);
        Task<EmployeeAgendaViewModel> GetEmployeeAgendaAsync(int employeeId, int year);
    }

    public class PnLReportViewModel
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalSalaries { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalProjectRevenue { get; set; }
        public decimal NetProfit => TotalProjectRevenue - (TotalSalaries + TotalExpenses);
    }

    public class EmployeeAgendaViewModel
    {
        public int Year { get; set; }
        public Employee? TargetEmployee { get; set; }
        public double TotalHoursWorked { get; set; }
        public List<Leave> ApprovedLeaves { get; set; } = new();
    }
}

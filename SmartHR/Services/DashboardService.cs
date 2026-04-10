using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.ViewModels;

namespace SmartHR.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly SmartHRContext _context;

        public DashboardService(SmartHRContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetAdminDashboardMetricsAsync()
        {
            var model = new DashboardViewModel { IsAdmin = true };

            model.TotalEmployees = await _context.Employees.CountAsync();
            model.TotalDepartments = await _context.Departments.CountAsync();
            model.ActiveProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Active);
            model.OpenTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open);
            model.PendingTasks = await _context.Tasks.CountAsync(t => t.Status == Models.TaskStatus.Pending);
            model.PendingLeaves = await _context.Leaves.CountAsync(l => l.Status == LeaveStatus.Pending);
            
            model.TotalSalariesPaid = await _context.Salaries
                .Where(s => s.Status == SalaryStatus.Paid)
                .SumAsync(s => s.NetSalary);
            
            model.TotalLeavesApproved = await _context.Leaves
                .CountAsync(l => l.Status == LeaveStatus.Approved);

            // Mocked weekly attendance data for visualization
            model.AttendanceWeeklyData = new List<int> { 94, 91, 89, 96, 92, 87, 100 };

            return model;
        }

        public async Task<DashboardViewModel> GetEmployeeDashboardMetricsAsync(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            var model = new DashboardViewModel 
            { 
                IsAdmin = false,
                EmployeeName = employee?.FullName ?? "User"
            };

            model.MyOpenTasks = await _context.Tasks
                .CountAsync(t => t.AssignedToId == employeeId && t.Status == Models.TaskStatus.Pending);

            model.MyPendingLeaves = await _context.Leaves
                .CountAsync(l => l.EmployeeId == employeeId && l.Status == LeaveStatus.Pending);

            return model;
        }
    }
}

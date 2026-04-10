using SmartHR.ViewModels;

namespace SmartHR.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetAdminDashboardMetricsAsync();
        Task<DashboardViewModel> GetEmployeeDashboardMetricsAsync(int employeeId);
    }
}

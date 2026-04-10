using SmartHR.Models;

namespace SmartHR.Services
{
    public interface ILeaveService
    {
        Task<IEnumerable<Leave>> GetLeavesAsync(int? employeeId = null);
        Task<Leave?> GetLeaveByIdAsync(int id);
        Task<(bool Success, string Message, Leave? Leave)> CreateLeaveRequestAsync(Leave leave);
        Task<(bool Success, string Message)> ApproveLeaveAsync(int id, string approverUserId);
        Task<(bool Success, string Message)> RejectLeaveAsync(int id, string approverUserId, string reason);
        Task<int> GetUsedAnnualLeaveDaysAsync(int employeeId);
        Task<bool> CheckOverlapAsync(int employeeId, DateTime start, DateTime end, int? excludeId = null);
    }
}

using SmartHR.Models;

namespace SmartHR.Services
{
    public interface IAttendanceService
    {
        Task<IEnumerable<Attendance>> GetAllAttendanceAsync();
        Task<IEnumerable<Attendance>> GetAttendanceByDateAsync(DateTime date);
        Task<Attendance?> GetTodayAttendanceForEmployeeAsync(int employeeId);
        Task<(bool Success, string Message)> PunchInAsync(int employeeId);
        Task<(bool Success, string Message)> PunchOutAsync(int employeeId);
        Task<(bool Success, string Message)> CreateManualRecordAsync(Attendance attendance);
        bool AttendanceExists(int employeeId, DateTime date);
    }
}

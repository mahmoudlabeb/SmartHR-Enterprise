using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SmartHR.Data;
using SmartHR.Models;

namespace SmartHR.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly SmartHRContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AttendanceService(SmartHRContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IEnumerable<Attendance>> GetAllAttendanceAsync()
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.PunchIn)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByDateAsync(DateTime date)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Date.Date == date.Date)
                .OrderBy(a => a.Employee!.FullName)
                .ToListAsync();
        }

        public async Task<Attendance?> GetTodayAttendanceForEmployeeAsync(int employeeId)
        {
            var today = DateTime.Today;
            return await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == today);
        }

        public async Task<(bool Success, string Message)> PunchInAsync(int employeeId)
        {
            var today = DateTime.Today;
            var existing = await GetTodayAttendanceForEmployeeAsync(employeeId);

            if (existing != null)
            {
                return (false, _localizer["AlreadyPunchedIn"]);
            }

            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                Date = today,
                PunchIn = DateTime.Now.TimeOfDay,
                Status = AttendanceStatus.Present
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            return (true, _localizer["PunchInSuccess"]);
        }

        public async Task<(bool Success, string Message)> PunchOutAsync(int employeeId)
        {
            var today = DateTime.Today;
            var attendance = await GetTodayAttendanceForEmployeeAsync(employeeId);

            if (attendance == null)
            {
                return (false, _localizer["NotFound"]);
            }

            if (attendance.PunchOut != null)
            {
                return (false, _localizer["ErrorOccurred"]);
            }

            attendance.PunchOut = DateTime.Now.TimeOfDay;
            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();
            return (true, _localizer["PunchOutSuccess"]);
        }

        public async Task<(bool Success, string Message)> CreateManualRecordAsync(Attendance attendance)
        {
            var existing = await _context.Attendances
                .AnyAsync(a => a.EmployeeId == attendance.EmployeeId && a.Date.Date == attendance.Date.Date);

            if (existing)
            {
                return (false, _localizer["ErrorOccurred"]);
            }

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            return (true, _localizer["CreateSuccess"]);
        }

        public bool AttendanceExists(int employeeId, DateTime date)
        {
            return _context.Attendances.Any(a => a.EmployeeId == employeeId && a.Date.Date == date.Date);
        }
    }
}

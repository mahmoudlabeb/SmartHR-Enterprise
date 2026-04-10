using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SmartHR.Data;
using SmartHR.Hubs;
using SmartHR.Models;

namespace SmartHR.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly SmartHRContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LeaveService(
            SmartHRContext context,
            IHubContext<NotificationHub> hubContext,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _hubContext = hubContext;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<IEnumerable<Leave>> GetLeavesAsync(int? employeeId = null)
        {
            var query = _context.Leaves.Include(l => l.Employee).AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(l => l.EmployeeId == employeeId.Value);
            }

            return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        }

        public async Task<Leave?> GetLeaveByIdAsync(int id)
        {
            return await _context.Leaves
                .Include(l => l.Employee)
                .ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string Message, Leave? Leave)> CreateLeaveRequestAsync(Leave leave)
        {
            // 1. Basic Validation
            if (leave.EndDate < leave.StartDate)
            {
                return (false, "تاريخ الانتهاء يجب أن يكون بعد تاريخ البداية.", null);
            }

            // 2. Overlap Check
            if (await CheckOverlapAsync(leave.EmployeeId, leave.StartDate, leave.EndDate))
            {
                return (false, "يوجد إجازة أخرى متداخلة مع هذه التواريخ.", null);
            }

            // 3. Balance Check (Annual Only)
            if (leave.LeaveType == LeaveType.Annual)
            {
                var emp = await _context.Employees.FindAsync(leave.EmployeeId);
                if (emp != null)
                {
                    int requestedDays = (leave.EndDate - leave.StartDate).Days + 1;
                    int usedDays = await GetUsedAnnualLeaveDaysAsync(leave.EmployeeId);

                    if (requestedDays + usedDays > emp.AnnualLeaveBalance)
                    {
                        return (false, $"رصيد إجازاتك السنوية لا يسمح. الرصيد المتبقي: {emp.AnnualLeaveBalance - usedDays} يوم.", null);
                    }
                }
            }

            leave.Status = LeaveStatus.Pending;
            leave.CreatedAt = DateTime.UtcNow;

            _context.Leaves.Add(leave);
            await _context.SaveChangesAsync();

            // 4. Notifications
            await SendApprovalNotificationsAsync(leave);

            return (true, _localizer["CreateSuccess"], leave);
        }

        public async Task<(bool Success, string Message)> ApproveLeaveAsync(int id, string approverUserId)
        {
            var leave = await _context.Leaves.Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null) return (false, _localizer["NotFound"]);
            if (leave.Status != LeaveStatus.Pending) return (false, "لا يمكن تغيير حالة هذه الإجازة.");

            var approverEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == approverUserId);
            
            leave.ApprovedByEmployeeId = approverEmployee?.Id;
            leave.ApprovedAt = DateTime.UtcNow;
            leave.Status = LeaveStatus.Approved;

            _context.Leaves.Update(leave);
            
            // Audit Log
            _context.AuditLogs.Add(new AuditLog
            {
                TableName = "Leaves",
                PrimaryKeyField = "Id",
                PrimaryKeyValue = leave.Id.ToString(),
                ActionType = "Approve",
                PropertyName = "Status",
                OldValue = LeaveStatus.Pending,
                NewValue = LeaveStatus.Approved,
                ChangedByUserId = approverUserId,
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // Notification to Employee
            if (leave.Employee != null)
            {
                await NotifyStatusChangeAsync(leave.Employee.UserId, "إجازة مقبولة", "تمت الموافقة على طلب إجازتك.");
            }

            return (true, _localizer["UpdateSuccess"]);
        }

        public async Task<(bool Success, string Message)> RejectLeaveAsync(int id, string approverUserId, string reason)
        {
            var leave = await _context.Leaves.Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null) return (false, _localizer["NotFound"]);
            if (leave.Status != LeaveStatus.Pending) return (false, "لا يمكن تغيير حالة هذه الإجازة.");

            var approverEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == approverUserId);

            leave.ApprovedByEmployeeId = approverEmployee?.Id;
            leave.ApprovedAt = DateTime.UtcNow;
            leave.Status = LeaveStatus.Rejected;
            leave.RejectionReason = string.IsNullOrWhiteSpace(reason) ? "لم يتم تحديد سبب." : reason;

            _context.Leaves.Update(leave);
            
            // Audit Log
            _context.AuditLogs.Add(new AuditLog
            {
                TableName = "Leaves",
                PrimaryKeyField = "Id",
                PrimaryKeyValue = leave.Id.ToString(),
                ActionType = "Reject",
                PropertyName = "Status",
                OldValue = LeaveStatus.Pending,
                NewValue = LeaveStatus.Rejected,
                ChangedByUserId = approverUserId,
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // Notification to Employee
            if (leave.Employee != null)
            {
                await NotifyStatusChangeAsync(leave.Employee.UserId, "إجازة مرفوضة", $"تم رفض طلب إجازتك. السبب: {leave.RejectionReason}");
            }

            return (true, _localizer["UpdateSuccess"]);
        }

        public async Task<int> GetUsedAnnualLeaveDaysAsync(int employeeId)
        {
            return await _context.Leaves
                .Where(l => l.EmployeeId == employeeId && l.LeaveType == LeaveType.Annual && l.Status == LeaveStatus.Approved)
                .SumAsync(l => EF.Functions.DateDiffDay(l.StartDate, l.EndDate) + 1);
        }

        public async Task<bool> CheckOverlapAsync(int employeeId, DateTime start, DateTime end, int? excludeId = null)
        {
            var query = _context.Leaves.Where(l =>
                l.EmployeeId == employeeId &&
                l.Status != LeaveStatus.Rejected &&
                l.StartDate <= end &&
                l.EndDate >= start);

            if (excludeId.HasValue)
            {
                query = query.Where(l => l.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        private async Task SendApprovalNotificationsAsync(Leave leave)
        {
            var approverRoles = new[] { AppRoles.SuperAdmin, AppRoles.Admin, AppRoles.HR, AppRoles.Manager };
            var notifiedUserIds = new HashSet<string>();

            foreach (var role in approverRoles)
            {
                var users = await _userManager.GetUsersInRoleAsync(role);
                foreach (var user in users)
                {
                    if (notifiedUserIds.Contains(user.Id)) continue;

                    _context.Notifications.Add(new Notification
                    {
                        UserId = user.Id,
                        Title = "طلب إجازة جديد",
                        Message = $"تم تقديم طلب إجازة من الموظف (ID: {leave.EmployeeId}) من {leave.StartDate:d} إلى {leave.EndDate:d}.",
                        TargetUrl = "/Leaves"
                    });
                    notifiedUserIds.Add(user.Id);
                }
            }

            await _context.SaveChangesAsync();

            if (notifiedUserIds.Count > 0)
            {
                await _hubContext.Clients.Users(notifiedUserIds).SendAsync("ReceiveNotification", "هناك طلب إجازة جديد بانتظار المراجعة.");
            }
        }

        private async Task NotifyStatusChangeAsync(string userId, string title, string message)
        {
            if (string.IsNullOrEmpty(userId)) return;

            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                TargetUrl = "/Leaves"
            });

            await _context.SaveChangesAsync();
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}

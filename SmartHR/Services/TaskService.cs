using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SmartHR.Data;
using SmartHR.Models;

namespace SmartHR.Services
{
    public class TaskService : ITaskService
    {
        private readonly SmartHRContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public TaskService(SmartHRContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync(int? employeeId = null, bool isPrivileged = false)
        {
            var query = _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedEmployee)
                .AsQueryable();

            if (!isPrivileged && employeeId.HasValue)
            {
                query = query.Where(t => t.AssignedToId == employeeId.Value);
            }

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedEmployee)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string Message, TaskItem? Task)> CreateTaskAsync(TaskItem task)
        {
            task.CreatedAt = DateTime.UtcNow;
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return (true, _localizer["SaveSuccess"], task);
        }

        public async Task<(bool Success, string Message, TaskItem? Task)> UpdateTaskAsync(TaskItem task, bool isEmployee = false)
        {
            var existing = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == task.Id);
            if (existing == null) return (false, _localizer["NotFound"], null);

            if (isEmployee)
            {
                // Employees can only update status
                existing.Status = task.Status;
                _context.Tasks.Update(existing);
            }
            else
            {
                _context.Update(task);
            }

            await _context.SaveChangesAsync();
            return (true, _localizer["UpdateSuccess"], task);
        }

        public async Task<(bool Success, string Message, object? CommentData)> AddCommentAsync(int taskId, string text, int employeeId)
        {
            // Placeholder for Task-specific comments if model exists
            return (false, "Not implemented", null);
        }

        public async Task<(bool Success, string Message)> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return (false, _localizer["NotFound"]);

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return (true, _localizer["DeleteSuccess"]);
        }

        public async Task<(bool Success, string Message)> UpdateTaskStatusAsync(int taskId, string status)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return (false, _localizer["NotFound"]);

            task.Status = status;
            await _context.SaveChangesAsync();
            return (true, _localizer["UpdateSuccess"]);
        }
    }
}

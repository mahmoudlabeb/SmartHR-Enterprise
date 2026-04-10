using SmartHR.Models;

namespace SmartHR.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> GetAllTasksAsync(int? employeeId = null, bool isPrivileged = false);
        Task<TaskItem?> GetTaskByIdAsync(int id);
        Task<(bool Success, string Message, TaskItem? Task)> CreateTaskAsync(TaskItem task);
        Task<(bool Success, string Message, TaskItem? Task)> UpdateTaskAsync(TaskItem task, bool isEmployee = false);
        Task<(bool Success, string Message)> DeleteTaskAsync(int id);
        
        Task<(bool Success, string Message)> UpdateTaskStatusAsync(int taskId, string status);
    }
}

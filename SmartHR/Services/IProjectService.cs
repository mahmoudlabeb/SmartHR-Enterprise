using SmartHR.Models;

namespace SmartHR.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync(int? employeeId = null, bool isPrivileged = false);
        Task<Project?> GetProjectByIdAsync(int id);
        Task<ProjectDashboardViewModel?> GetProjectDashboardAsync(int id);
        Task<(bool Success, string Message, Project? Project)> CreateProjectAsync(Project project);
        Task<(bool Success, string Message, Project? Project)> UpdateProjectAsync(Project project);
        Task<(bool Success, string Message)> DeleteProjectAsync(int id);
        
        Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(int projectId);
        Task<(bool Success, string Message)> AssignMemberAsync(int projectId, int employeeId);
        Task<(bool Success, string Message)> RemoveMemberAsync(int projectId, int employeeId);
    }

    public class ProjectDashboardViewModel
    {
        public Project Project { get; set; } = null!;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public double CompletionPercentage { get; set; }
        
        // Synced Team (Members + anyone with tasks)
        public List<Employee> SyncedTeamMembers { get; set; } = new();
        public List<TaskItem> RecentTasks { get; set; } = new();
    }
}

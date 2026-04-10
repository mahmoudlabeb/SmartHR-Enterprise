using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SmartHR.Data;
using SmartHR.Models;

namespace SmartHR.Services
{
    public class ProjectService : IProjectService
    {
        private readonly SmartHRContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ProjectService(SmartHRContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync(int? employeeId = null, bool isPrivileged = false)
        {
            var query = _context.Projects.AsQueryable();

            if (!isPrivileged && employeeId.HasValue)
            {
                // In a more advanced system, we'd join with ProjectMembers
                // For now, let's assume we filter by members if that relationship exists, 
                // but since the model `Project` doesn't have a direct `Members` collection in the DB yet (only a class), 
                // we'll stick to a basic privileged check or membership join if available.
                
                var projectIds = await _context.ProjectMembers
                    .Where(pm => pm.EmployeeId == employeeId.Value)
                    .Select(pm => pm.ProjectId)
                    .ToListAsync();
                
                query = query.Where(p => projectIds.Contains(p.Id));
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string Message, Project? Project)> CreateProjectAsync(Project project)
        {
            project.CreatedAt = DateTime.UtcNow;
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return (true, _localizer["CreateSuccess"], project);
        }

        public async Task<(bool Success, string Message, Project? Project)> UpdateProjectAsync(Project project)
        {
            var existing = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == project.Id);
            if (existing == null) return (false, _localizer["NotFound"], null);

            _context.Update(project);
            await _context.SaveChangesAsync();
            return (true, _localizer["UpdateSuccess"], project);
        }

        public async Task<(bool Success, string Message)> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return (false, _localizer["NotFound"]);

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return (true, _localizer["DeleteSuccess"]);
        }

        public async Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(int projectId)
        {
            return await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Include(pm => pm.Employee)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> AssignMemberAsync(int projectId, int employeeId)
        {
            var exists = await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.EmployeeId == employeeId);
            if (exists) return (false, _localizer["ErrorOccurred"]);

            var member = new ProjectMember
            {
                ProjectId = projectId,
                EmployeeId = employeeId
            };

            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();
            return (true, _localizer["SaveSuccess"]);
        }

        public async Task<(bool Success, string Message)> RemoveMemberAsync(int projectId, int employeeId)
        {
            var member = await _context.ProjectMembers.FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.EmployeeId == employeeId);
            if (member == null) return (false, _localizer["NotFound"]);

            _context.ProjectMembers.Remove(member);
            await _context.SaveChangesAsync();
            return (true, _localizer["DeleteSuccess"]);
        }
        public async Task<ProjectDashboardViewModel?> GetProjectDashboardAsync(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .ThenInclude(t => t.AssignedEmployee)
                .Include(p => p.TeamMembers)
                .ThenInclude(tm => tm.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return null;

            var tasks = project.Tasks.ToList();
            var totalTasks = tasks.Count;
            var completedTasks = tasks.Count(t => t.Status == "Completed");
            
            // Auto-Sync Team: Combine official members and anyone with tasks assigned
            var officialMembers = project.TeamMembers.Select(tm => tm.Employee).Where(e => e != null).Cast<Employee>();
            var taskAssignedEmployees = tasks.Select(t => t.AssignedEmployee).Where(e => e != null).Cast<Employee>();
            
            var syncedTeam = officialMembers
                .Concat(taskAssignedEmployees)
                .GroupBy(e => e.Id)
                .Select(g => g.First())
                .ToList();

            return new ProjectDashboardViewModel
            {
                Project = project,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = tasks.Count(t => t.Status == "InProgress"),
                PendingTasks = tasks.Count(t => t.Status == "Pending"),
                CompletionPercentage = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0,
                SyncedTeamMembers = syncedTeam,
                RecentTasks = tasks.OrderByDescending(t => t.CreatedAt).Take(5).ToList()
            };
        }
    }
}

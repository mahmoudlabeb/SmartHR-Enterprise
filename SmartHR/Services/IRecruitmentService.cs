using SmartHR.Models;

namespace SmartHR.Services
{
    public interface IRecruitmentService
    {
        Task<IEnumerable<JobPosting>> GetAllJobsAsync();
        Task<JobPosting?> GetJobByIdAsync(int id);
        Task<(bool Success, string Message)> CreateJobAsync(JobPosting job);
        Task<(bool Success, string Message)> UpdateJobAsync(JobPosting job);
        
        Task<IEnumerable<Candidate>> GetAllCandidatesAsync();
        Task<Candidate?> GetCandidateByIdAsync(int id);
        Task<(bool Success, string Message)> UpdateCandidateStatusAsync(int id, string status);
    }
}

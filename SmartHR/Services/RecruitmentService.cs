using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SmartHR.Data;
using SmartHR.Models;

namespace SmartHR.Services
{
    public class RecruitmentService : IRecruitmentService
    {
        private readonly SmartHRContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public RecruitmentService(SmartHRContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IEnumerable<JobPosting>> GetAllJobsAsync()
        {
            return await _context.JobPostings!
                .Include(j => j.Candidates)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<JobPosting?> GetJobByIdAsync(int id)
        {
            return await _context.JobPostings!
                .Include(j => j.Candidates)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string Message)> CreateJobAsync(JobPosting job)
        {
            job.CreatedAt = DateTime.Now;
            _context.JobPostings!.Add(job);
            await _context.SaveChangesAsync();
            return (true, _localizer["CreateSuccess"]);
        }

        public async Task<(bool Success, string Message)> UpdateJobAsync(JobPosting job)
        {
            var existing = await _context.JobPostings!.AsNoTracking().FirstOrDefaultAsync(j => j.Id == job.Id);
            if (existing == null) return (false, _localizer["NotFound"]);

            _context.Update(job);
            await _context.SaveChangesAsync();
            return (true, _localizer["UpdateSuccess"]);
        }

        public async Task<IEnumerable<Candidate>> GetAllCandidatesAsync()
        {
            return await _context.Candidates!
                .Include(c => c.JobPosting)
                .OrderByDescending(c => c.AppliedAt)
                .ToListAsync();
        }

        public async Task<Candidate?> GetCandidateByIdAsync(int id)
        {
            return await _context.Candidates!
                .Include(c => c.JobPosting)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string Message)> UpdateCandidateStatusAsync(int id, string status)
        {
            var allowed = new[] {
                CandidateStatus.Applied, CandidateStatus.Shortlisted,
                CandidateStatus.Interview, CandidateStatus.Hired, CandidateStatus.Rejected
            };
            if (!allowed.Contains(status)) return (false, _localizer["ErrorOccurred"]);

            var candidate = await _context.Candidates!.FindAsync(id);
            if (candidate == null) return (false, _localizer["NotFound"]);

            candidate.Status = status;
            await _context.SaveChangesAsync();

            return (true, _localizer["UpdateSuccess"]);
        }
    }
}

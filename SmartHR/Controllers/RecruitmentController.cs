using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.Services;

namespace SmartHR.Controllers
{
    // ✅ FIX C4: Added [Authorize] with HR/Admin roles — candidate PII is
    //    no longer publicly accessible.
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT},{AppRoles.HR}")]
    public class RecruitmentController : Controller
    {
        private readonly IRecruitmentService _recruitmentService;

        public RecruitmentController(IRecruitmentService recruitmentService)
        {
            _recruitmentService = recruitmentService;
        }

        // 1. List all job postings
        public async Task<IActionResult> Index()
        {
            var jobs = await _recruitmentService.GetAllJobsAsync();
            return View(jobs);
        }

        // 2. Create job posting (GET)
        public IActionResult CreateJob() => View();

        // 3. Create job posting (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJob(JobPosting job)
        {
            if (ModelState.IsValid)
            {
                var result = await _recruitmentService.CreateJobAsync(job);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            return View(job);
        }

        // 4. Edit job posting (GET)
        public async Task<IActionResult> EditJob(int? id)
        {
            if (id == null) return NotFound();
            var job = await _recruitmentService.GetJobByIdAsync(id.Value);
            if (job == null) return NotFound();
            return View(job);
        }

        // 5. Edit job posting (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(int id, JobPosting job)
        {
            if (id != job.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _recruitmentService.UpdateJobAsync(job);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            return View(job);
        }

        // 6. Job details with candidate list
        public async Task<IActionResult> JobDetails(int? id)
        {
            if (id == null) return NotFound();
            var job = await _recruitmentService.GetJobByIdAsync(id.Value);
            if (job == null) return NotFound();
            return View(job);
        }

        // 7. All candidates (ATS view)
        public async Task<IActionResult> Candidates()
        {
            var candidates = await _recruitmentService.GetAllCandidatesAsync();
            return View(candidates);
        }

        // 8. Single candidate details
        public async Task<IActionResult> CandidateDetails(int? id)
        {
            if (id == null) return NotFound();
            var candidate = await _recruitmentService.GetCandidateByIdAsync(id.Value);
            if (candidate == null) return NotFound();
            return View(candidate);
        }

        // 9. Update candidate status (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCandidateStatus(int id, string status)
        {
            var result = await _recruitmentService.UpdateCandidateStatusAsync(id, status);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(CandidateDetails), new { id });
        }
    }
}
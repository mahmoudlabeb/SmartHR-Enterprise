using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.Controllers;

namespace SmartHR.Services
{
    public class PayrollJobService
    {
        private readonly ISalaryService _salaryService;

        public PayrollJobService(ISalaryService salaryService)
        {
            _salaryService = salaryService;
        }

        public async Task GenerateMonthlySalariesAsync()
        {
            var now = DateTime.UtcNow;
            await _salaryService.GenerateMonthlySalariesAsync(now.Month, now.Year);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using System.Threading.Tasks;

namespace SmartHR.Controllers
{
    [Authorize(Roles = "IT")]
    public class AuditLogsController : Controller
    {
        private readonly SmartHRContext _context;

        public AuditLogsController(SmartHRContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.AuditLogs.OrderByDescending(x => x.ChangedAt).Take(500).ToListAsync();
            return View(logs);
        }
    }
}

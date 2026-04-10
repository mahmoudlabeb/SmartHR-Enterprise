using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;

namespace SmartHR.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT},{AppRoles.HR}")]
    public class TrainingController : Controller
    {
        private readonly SmartHRContext _context;

        public TrainingController(SmartHRContext context)
        {
            _context = context;
        }

        // GET: Training
        public async Task<IActionResult> Index()
        {
            var programs = await _context.TrainingPrograms.ToListAsync();
            return View(programs);
        }

        // GET: Training/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var program = await _context.TrainingPrograms
                .Include(p => p.Trainees)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (program == null) return NotFound();

            return View(program);
        }

        // GET: Training/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Training/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Instructor,Status,StartDate,EndDate")] TrainingProgram program)
        {
            if (ModelState.IsValid)
            {
                _context.Add(program);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إضافة البرنامج التدريبي بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            return View(program);
        }

        // GET: Training/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var program = await _context.TrainingPrograms.FindAsync(id);
            if (program == null) return NotFound();
            return View(program);
        }

        // POST: Training/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Instructor,Status,StartDate,EndDate")] TrainingProgram program)
        {
            if (id != program.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(program);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تحديث البرنامج التدريبي بنجاح.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainingProgramExists(program.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(program);
        }

        private bool TrainingProgramExists(int id)
        {
            return _context.TrainingPrograms.Any(e => e.Id == id);
        }
    }
}

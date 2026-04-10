using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Hubs;
using SmartHR.Models;
using SmartHR.Services;

namespace SmartHR.Controllers
{
    // ✅ FIX C3: Added [Authorize] — unauthenticated users can no longer
    //    access ticket data.
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.HR},{AppRoles.Manager},{AppRoles.IT},{AppRoles.Employee}")]
    public class TicketsController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly IEmployeeService _employeeService;
        private readonly IWebHostEnvironment _env;
        private readonly SmartHRContext _context; // For dropdowns

        public TicketsController(
            ITicketService ticketService,
            IEmployeeService employeeService,
            IWebHostEnvironment env,
            SmartHRContext context)
        {
            _ticketService = ticketService;
            _employeeService = employeeService;
            _env = env;
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var isPrivileged = IsPrivilegedUser();

            int? employeeId = null;
            if (!isPrivileged)
            {
                var employee = await GetCurrentEmployeeAsync();
                if (employee != null) employeeId = employee.Id;
            }

            var tickets = await _ticketService.GetTicketsAsync(employeeId, isPrivileged);
            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null) return NotFound();

            // Security check
            if (!IsPrivilegedUser() && ticket.Employee?.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            return View(ticket);
        }

        private bool IsPrivilegedUser()
        {
            return User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.Admin) || 
                   User.IsInRole(AppRoles.IT) || User.IsInRole(AppRoles.HR) || User.IsInRole(AppRoles.Manager);
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            if (IsPrivilegedUser())
            {
                ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
                return View();
            }

            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return Unauthorized();

            ViewData["EmployeeId"] = new SelectList(new[] { employee }, "Id", "FullName", employee.Id);
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket, IFormFile? attachment)
        {
            if (!IsPrivilegedUser())
            {
                var employee = await GetCurrentEmployeeAsync();
                if (employee == null) return Unauthorized();
                ticket.EmployeeId = employee.Id;
            }

            if (ModelState.IsValid)
            {
                var result = await _ticketService.CreateTicketAsync(ticket, attachment, _env.WebRootPath);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }

            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", ticket.EmployeeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT},{AppRoles.HR}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null) return NotFound();

            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", ticket.EmployeeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT},{AppRoles.HR}")]
        public async Task<IActionResult> Edit(int id, Ticket ticket)
        {
            if (id != ticket.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _ticketService.UpdateTicketAsync(ticket);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", ticket.EmployeeId);
            return View(ticket);
        }

        // POST: Tickets/AddComment
        [HttpPost]
        public async Task<IActionResult> AddComment(int ticketId, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return BadRequest("Text cannot be empty.");

            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return Unauthorized();

            var result = await _ticketService.AddCommentAsync(ticketId, text, employee.Id);
            if (result.Success) return Ok(result.CommentData);
            
            return BadRequest(result.Message);
        }

        // GET: Tickets/Delete/5
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null) return NotFound();

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _ticketService.DeleteTicketAsync(id);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id) =>
            _context.Tickets.Any(e => e.Id == id);
    }
}
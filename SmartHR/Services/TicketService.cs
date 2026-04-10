using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SmartHR.Data;
using SmartHR.Hubs;
using SmartHR.Models;

namespace SmartHR.Services
{
    public class TicketService : ITicketService
    {
        private readonly SmartHRContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public TicketService(
            SmartHRContext context,
            IHubContext<NotificationHub> hubContext,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _hubContext = hubContext;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<IEnumerable<Ticket>> GetTicketsAsync(int? employeeId = null, bool isPrivileged = false)
        {
            var query = _context.Tickets.Include(t => t.Employee).AsQueryable();

            if (!isPrivileged && employeeId.HasValue)
            {
                query = query.Where(t => t.EmployeeId == employeeId.Value);
            }

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Employee)
                .Include(t => t.Comments!)
                    .ThenInclude(c => c.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string Message, Ticket? Ticket)> CreateTicketAsync(Ticket ticket, IFormFile? attachment, string webRootPath)
        {
            if (attachment != null && attachment.Length > 0)
            {
                var uploadsFolder = Path.Combine(webRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(attachment.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await attachment.CopyToAsync(stream);
                }
                ticket.AttachmentUrl = "/uploads/" + uniqueFileName;
            }

            ticket.CreatedAt = DateTime.Now;
            ticket.Status = TicketStatus.Open;
            
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Notify IT role
            await NotifyITAboutNewTicketAsync(ticket);

            return (true, _localizer["CreateSuccess"], ticket);
        }

        public async Task<(bool Success, string Message)> UpdateTicketAsync(Ticket ticket)
        {
            var existing = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);
            if (existing == null) return (false, _localizer["NotFound"]);

            _context.Update(ticket);
            await _context.SaveChangesAsync();

            // Notify owner + IT about status change
            await NotifyStatusChangeAsync(ticket);

            return (true, _localizer["UpdateSuccess"]);
        }

        public async Task<(bool Success, string Message, object? CommentData)> AddCommentAsync(int ticketId, string text, int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return (false, _localizer["NotFound"], null);

            var comment = new TicketComment
            {
                TicketId = ticketId,
                CommentText = text,
                CreatedAt = DateTime.Now,
                EmployeeId = employeeId
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            // Real-time notification
            await NotifyNewCommentAsync(ticketId, employee.FullName, text, comment.CreatedAt);

            return (true, _localizer["CreateSuccess"], new
            {
                authorName = employee.FullName,
                commentText = text,
                createdAt = comment.CreatedAt.ToString("g")
            });
        }

        public async Task<(bool Success, string Message)> DeleteTicketAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return (false, _localizer["NotFound"]);

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return (true, _localizer["DeleteSuccess"]);
        }

        private async Task NotifyITAboutNewTicketAsync(Ticket ticket)
        {
            var itUsers = await _userManager.GetUsersInRoleAsync(AppRoles.IT);
            var itUserIds = itUsers.Select(u => u.Id).ToList();
            
            foreach (var u in itUsers)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = u.Id,
                    Title = "تذكرة جديدة",
                    Message = $"وصلت تذكرة دعم فني جديدة (ID: {ticket.Id}) بانتظار المراجعة.",
                    TargetUrl = "/Tickets"
                });
            }
            await _context.SaveChangesAsync();

            if (itUserIds.Any())
            {
                await _hubContext.Clients.Users(itUserIds).SendAsync("ReceiveNotification", "تذكرة دعم فني جديدة بانتظار المراجعة.");
            }
        }

        private async Task NotifyStatusChangeAsync(Ticket ticket)
        {
            var itUsers = await _userManager.GetUsersInRoleAsync(AppRoles.IT);
            var recipients = new HashSet<string>(itUsers.Select(u => u.Id));

            var ownerUserId = await _context.Employees
                .Where(e => e.Id == ticket.EmployeeId)
                .Select(e => e.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(ownerUserId)) recipients.Add(ownerUserId);

            if (recipients.Any())
            {
                await _hubContext.Clients.Users(recipients.ToList()).SendAsync("TicketStatusChanged", ticket.Id, ticket.Status);
            }
        }

        private async Task NotifyNewCommentAsync(int ticketId, string authorName, string text, DateTime createdAt)
        {
            var itUsers = await _userManager.GetUsersInRoleAsync(AppRoles.IT);
            var recipients = new HashSet<string>(itUsers.Select(u => u.Id));

            var ownerUserId = await _context.Tickets
                .Where(t => t.Id == ticketId)
                .Select(t => t.Employee!.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(ownerUserId)) recipients.Add(ownerUserId);

            if (recipients.Any())
            {
                await _hubContext.Clients.Users(recipients.ToList()).SendAsync(
                    "ReceiveTicketComment",
                    ticketId,
                    authorName,
                    text,
                    createdAt.ToString("g"));
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using Microsoft.AspNetCore.SignalR;
using SmartHR.Hubs;
using System.Security.Claims;

namespace SmartHR.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly SmartHRContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ChatController(SmartHRContext context, UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // GET: Chat
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Get users to chat with (recent active users or everyone)
            var users = await _userManager.Users
                .Where(u => u.Id != currentUserId && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // Fetch unread count map
            var unreadCounts = await _context.Messages
                .Where(m => m.ReceiverId == currentUserId && !m.IsRead)
                .GroupBy(m => m.SenderId)
                .Select(g => new { SenderId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.SenderId, v => v.Count);

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.UnreadCounts = unreadCounts;
            return View(users);
        }

        // GET: Chat/Conversation/userId
        public async Task<IActionResult> Conversation(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(id) || id == currentUserId) return RedirectToAction(nameof(Index));

            var otherUser = await _userManager.FindByIdAsync(id);
            if (otherUser == null) return NotFound();

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == id) ||
                            (m.SenderId == id && m.ReceiverId == currentUserId))
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            // Mark received messages as read
            var unread = messages.Where(m => m.ReceiverId == currentUserId && !m.IsRead).ToList();
            if (unread.Any())
            {
                foreach (var msg in unread)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.OtherUser = otherUser;

            return View(messages);
        }

        // POST: Chat/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string receiverId, string content, IFormFile? attachment, [FromServices] IWebHostEnvironment env)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(content) && !string.IsNullOrEmpty(receiverId))
            {
                var message = new Message
                {
                    SenderId = currentUserId ?? string.Empty,
                    ReceiverId = receiverId,
                    Content = content.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                if (attachment != null && attachment.Length > 0)
                {
                    string uploadsFolder = Path.Combine(env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + attachment.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await attachment.CopyToAsync(fileStream);
                    }
                    message.AttachmentUrl = "/uploads/" + uniqueFileName;
                }

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                var sender = await _userManager.FindByIdAsync(currentUserId);
                await _hubContext.Clients.User(receiverId).SendAsync("ReceiveMessage", sender?.FullName ?? "مستخدم", content, $"/Chat/Conversation/{currentUserId}");
            }

            return RedirectToAction(nameof(Conversation), new { id = receiverId });
        }

        // GET: Chat/Group
        public async Task<IActionResult> Group()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var messages = await _context.GroupMessages
                .Include(m => m.Sender)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.CurrentUserId = currentUserId;
            return View(messages);
        }

        // POST: Chat/SendGroupMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendGroupMessage(string content, IFormFile? attachment, [FromServices] IWebHostEnvironment env)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(content))
            {
                var message = new GroupMessage
                {
                    SenderId = currentUserId ?? string.Empty,
                    Content = content.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                if (attachment != null && attachment.Length > 0)
                {
                    string uploadsFolder = Path.Combine(env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + attachment.FileName;
                    using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                    {
                        await attachment.CopyToAsync(fileStream);
                    }
                    message.AttachmentUrl = "/uploads/" + uniqueFileName;
                }

                _context.GroupMessages.Add(message);
                await _context.SaveChangesAsync();

                var sender = await _userManager.FindByIdAsync(currentUserId);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", sender?.FullName ?? "مستخدم", content, "/Chat/Group");
            }

            return RedirectToAction(nameof(Group));
        }
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SmartHR.Models;
using System.Threading.Tasks;

namespace SmartHR.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationHub(UserManager<ApplicationUser> userManager)
        {
            this._userManager = userManager;
        }

        public async Task SendNotification(string userId, string message)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.ReceivePushNotifications)
            {
                await Clients.User(userId).SendAsync("ReceiveNotification", message);
            }
            if (user != null && user.ReceiveEmailNotifications)
            {
                // Trigger email sending logic here
            }
        }

        public async Task TicketUpdated(int ticketId, string status)
        {
            await Clients.All.SendAsync("TicketStatusChanged", ticketId, status);
        }
    }
}

using SmartHR.Models;

namespace SmartHR.Services
{
    public interface ITicketService
    {
        Task<IEnumerable<Ticket>> GetTicketsAsync(int? employeeId = null, bool isPrivileged = false);
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<(bool Success, string Message, Ticket? Ticket)> CreateTicketAsync(Ticket ticket, IFormFile? attachment, string webRootPath);
        Task<(bool Success, string Message)> UpdateTicketAsync(Ticket ticket);
        Task<(bool Success, string Message, object? CommentData)> AddCommentAsync(int ticketId, string text, int employeeId);
        Task<(bool Success, string Message)> DeleteTicketAsync(int id);
    }
}

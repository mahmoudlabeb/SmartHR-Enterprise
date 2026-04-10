using SmartHR.Models;

namespace SmartHR.Services
{
    public interface IInvoiceService
    {
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<Invoice?> GetInvoiceByIdAsync(int id);
        Task<(bool Success, string Message, Invoice? Invoice)> CreateInvoiceAsync(Invoice invoice);
        Task<(bool Success, string Message, Invoice? Invoice)> UpdateInvoiceAsync(Invoice invoice);
        Task<(bool Success, string Message)> DeleteInvoiceAsync(int id);
        
        Task<IEnumerable<Client>> GetAllClientsAsync();
        decimal CalculateInvoiceTotal(Invoice invoice);
    }
}

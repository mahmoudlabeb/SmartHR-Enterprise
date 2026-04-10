using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using SmartHR.Data;
using SmartHR.Models;

namespace SmartHR.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly SmartHRContext _context;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public InvoiceService(SmartHRContext context, IStringLocalizer<SharedResource> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Client)
                .OrderByDescending(i => i.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var totalProjectRevenue = await _context.Invoices
                .Where(i => i.Date >= startDate && i.Date <= endDate && i.Status == "Paid")
                .SumAsync(i => i.TotalAmount);
            return totalProjectRevenue;
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string Message, Invoice? Invoice)> CreateInvoiceAsync(Invoice invoice)
        {
            if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
            {
                invoice.InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
            }

            invoice.TotalAmount = CalculateInvoiceTotal(invoice);

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return (true, _localizer["CreateSuccess"], invoice);
        }

        public async Task<(bool Success, string Message, Invoice? Invoice)> UpdateInvoiceAsync(Invoice invoice)
        {
            var existing = await _context.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == invoice.Id);
            if (existing == null) return (false, _localizer["NotFound"], null);

            invoice.TotalAmount = CalculateInvoiceTotal(invoice);
            
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
            return (true, _localizer["UpdateSuccess"], invoice);
        }

        public async Task<(bool Success, string Message)> DeleteInvoiceAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return (false, _localizer["NotFound"]);

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return (true, _localizer["DeleteSuccess"]);
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _context.Clients.OrderBy(c => c.CompanyName).ToListAsync();
        }

        public decimal CalculateInvoiceTotal(Invoice invoice)
        {
            if (invoice.Items == null || !invoice.Items.Any()) return invoice.TotalAmount; // Fallback to manual total if no items

            return invoice.Items.Sum(item => item.Quantity * item.UnitPrice);
        }
    }
}

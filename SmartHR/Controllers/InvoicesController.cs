using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHR.Data;
using SmartHR.Models;
using SmartHR.Services;
using System.Threading.Tasks;

namespace SmartHR.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.IT},{AppRoles.Manager}")]
    public class InvoicesController : Controller
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        // List invoices
        public async Task<IActionResult> Index()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            return View(invoices);
        }

        // Invoice Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id.Value);
            if (invoice == null) return NotFound();
            return View(invoice);
        }

        // Create Invoice (GET)
        public async Task<IActionResult> Create()
        {
            ViewData["ClientId"] = new SelectList(await _invoiceService.GetAllClientsAsync(), "Id", "CompanyName");
            return View();
        }

        // Create Invoice (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                var result = await _invoiceService.CreateInvoiceAsync(invoice);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            ViewData["ClientId"] = new SelectList(await _invoiceService.GetAllClientsAsync(), "Id", "CompanyName", invoice.ClientId);
            return View(invoice);
        }

        // Edit Invoice (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id.Value);
            if (invoice == null) return NotFound();

            ViewData["ClientId"] = new SelectList(await _invoiceService.GetAllClientsAsync(), "Id", "CompanyName", invoice.ClientId);
            return View(invoice);
        }

        // Edit Invoice (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Invoice invoice)
        {
            if (id != invoice.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _invoiceService.UpdateInvoiceAsync(invoice);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            ViewData["ClientId"] = new SelectList(await _invoiceService.GetAllClientsAsync(), "Id", "CompanyName", invoice.ClientId);
            return View(invoice);
        }

        // Delete Invoice
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id.Value);
            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // Delete Confirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _invoiceService.DeleteInvoiceAsync(id);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
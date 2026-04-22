using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly AppDbContext _context;
    private readonly CurrencyService _currencyService;

    public ServiceRequestsController(AppDbContext context, CurrencyService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _context.ServiceRequests
            .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
            .OrderByDescending(sr => sr.Id)
            .ToListAsync();

        return View(requests);
    }

    public async Task<IActionResult> Details(int id)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null)
            return NotFound();

        return View(serviceRequest);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.ZarRate = await _currencyService.GetUsdToZarRateAsync();
        ViewBag.Contracts = await BuildActiveContractSelectListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequest serviceRequest)
    {
        var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);

        if (contract == null)
        {
            ModelState.AddModelError("ContractId", "The selected contract does not exist.");
        }
        else if (contract.Status == ContractStatus.Expired)
        {
            ModelState.AddModelError("ContractId",
                "A service request cannot be created against an Expired contract.");
        }
        else if (contract.Status == ContractStatus.OnHold)
        {
            ModelState.AddModelError("ContractId",
                "A service request cannot be created against a contract that is On Hold.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ZarRate = await _currencyService.GetUsdToZarRateAsync();
            ViewBag.Contracts = await BuildActiveContractSelectListAsync();
            return View(serviceRequest);
        }

        serviceRequest.CostZAR = await _currencyService.ConvertUsdToZarAsync(serviceRequest.CostUSD);

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null)
            return NotFound();

        return View(serviceRequest);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(id);

        if (serviceRequest == null)
            return NotFound();

        _context.ServiceRequests.Remove(serviceRequest);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> BuildActiveContractSelectListAsync()
    {
        var activeContracts = await _context.Contracts
            .Include(c => c.Client)
            .Where(c => c.Status == ContractStatus.Active)
            .OrderBy(c => c.Client.Name)
            .ToListAsync();

        var items = activeContracts.Select(c => new
        {
            c.Id,
            Display = $"{c.Client.Name} \u2014 {c.ServiceLevel}"
        });

        return new SelectList(items, "Id", "Display");
    }
}

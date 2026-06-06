using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly GlmsApiClient _api;

    public ServiceRequestsController(GlmsApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        List<ServiceRequest> requests = await _api.GetServiceRequestsAsync();
        return View(requests);
    }

    public async Task<IActionResult> Details(int id)
    {
        ServiceRequest serviceRequest = await _api.GetServiceRequestAsync(id);

        if (serviceRequest == null)
            return NotFound();

        return View(serviceRequest);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.ZarRate = await _api.GetZarRateAsync();
        ViewBag.Contracts = await BuildActiveContractSelectListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequest serviceRequest)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ZarRate = await _api.GetZarRateAsync();
            ViewBag.Contracts = await BuildActiveContractSelectListAsync();
            return View(serviceRequest);
        }

        var (success, error) = await _api.CreateServiceRequestAsync(serviceRequest);

        if (!success)
        {
            ModelState.AddModelError("ContractId", error ?? "Failed to create service request.");
            ViewBag.ZarRate = await _api.GetZarRateAsync();
            ViewBag.Contracts = await BuildActiveContractSelectListAsync();
            return View(serviceRequest);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        ServiceRequest serviceRequest = await _api.GetServiceRequestAsync(id);

        if (serviceRequest == null)
            return NotFound();

        return View(serviceRequest);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _api.DeleteServiceRequestAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> BuildActiveContractSelectListAsync()
    {
        List<Contract> contracts = await _api.GetContractsAsync(ContractStatus.Active, null, null);

        var items = contracts
            .OrderBy(c => c.Client?.Name)
            .Select(c => new
            {
                c.Id,
                Display = $"{c.Client?.Name ?? "Unknown"} — {c.ServiceLevel}"
            });

        return new SelectList(items, "Id", "Display");
    }
}

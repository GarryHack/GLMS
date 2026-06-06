using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers;

public class ContractsController : Controller
{
    private readonly GlmsApiClient _api;

    public ContractsController(GlmsApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(ContractStatus? status, DateTime? fromDate, DateTime? toDate)
    {
        List<Contract> contracts = await _api.GetContractsAsync(status, fromDate, toDate);

        ViewBag.SelectedStatus = status;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.StatusList = Enum.GetValues<ContractStatus>();

        return View(contracts);
    }

    public async Task<IActionResult> Details(int id)
    {
        Contract contract = await _api.GetContractAsync(id);

        if (contract == null)
            return NotFound();

        return View(contract);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Clients = await BuildClientSelectListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Contract contract, IFormFile? agreementFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Clients = await BuildClientSelectListAsync();
            return View(contract);
        }

        var (success, error) = await _api.CreateContractAsync(contract, agreementFile);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create contract.");
            ViewBag.Clients = await BuildClientSelectListAsync();
            return View(contract);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        Contract contract = await _api.GetContractAsync(id);

        if (contract == null)
            return NotFound();

        ViewBag.Clients = await BuildClientSelectListAsync(contract.ClientId);
        return View(contract);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? agreementFile)
    {
        if (id != contract.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Clients = await BuildClientSelectListAsync(contract.ClientId);
            return View(contract);
        }

        var (success, error) = await _api.UpdateContractAsync(id, contract, agreementFile);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update contract.");
            ViewBag.Clients = await BuildClientSelectListAsync(contract.ClientId);
            return View(contract);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        Contract contract = await _api.GetContractAsync(id);

        if (contract == null)
            return NotFound();

        return View(contract);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _api.DeleteContractAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Download(int id)
    {
        var downloadUrl = _api.GetDownloadUrl(id);
        return Redirect(downloadUrl);
    }

    private async Task<SelectList> BuildClientSelectListAsync(int? selectedId = null)
    {
        List<Client> clients = await _api.GetClientsAsync();
        return new SelectList(clients.OrderBy(c => c.Name), "Id", "Name", selectedId);
    }
}

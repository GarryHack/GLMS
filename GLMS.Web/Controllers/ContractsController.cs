using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;

namespace GLMS.Web.Controllers;

public class ContractsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ContractsController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index(ContractStatus? status, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Contracts
            .Include(c => c.Client)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(c => c.StartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(c => c.EndDate <= toDate.Value);

        ViewBag.SelectedStatus = status;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.StatusList = Enum.GetValues<ContractStatus>();

        var contracts = await query.OrderByDescending(c => c.StartDate).ToListAsync();
        return View(contracts);
    }

    public async Task<IActionResult> Details(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
            return NotFound();

        return View(contract);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Clients = new SelectList(
            await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Contract contract, IFormFile? agreementFile)
    {
        if (agreementFile != null && agreementFile.Length > 0)
        {
            if (!IsPdf(agreementFile.FileName))
            {
                ModelState.AddModelError("SignedAgreementPath",
                    "Only .pdf files are allowed for signed agreements.");
            }
            else
            {
                contract.SignedAgreementPath = await SavePdfAsync(agreementFile);
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Clients = new SelectList(
                await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            return View(contract);
        }

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);

        if (contract == null)
            return NotFound();

        ViewBag.Clients = new SelectList(
            await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", contract.ClientId);
        return View(contract);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? agreementFile)
    {
        if (id != contract.Id)
            return BadRequest();

        if (agreementFile != null && agreementFile.Length > 0)
        {
            if (!IsPdf(agreementFile.FileName))
            {
                ModelState.AddModelError("SignedAgreementPath",
                    "Only .pdf files are allowed for signed agreements.");
            }
            else
            {
                contract.SignedAgreementPath = await SavePdfAsync(agreementFile);
            }
        }
        else
        {
            var existing = await _context.Contracts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            contract.SignedAgreementPath = existing?.SignedAgreementPath;
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Clients = new SelectList(
                await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", contract.ClientId);
            return View(contract);
        }

        try
        {
            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Contracts.AnyAsync(c => c.Id == id))
                return NotFound();

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
            return NotFound();

        return View(contract);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);

        if (contract == null)
            return NotFound();

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Download(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);

        if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
            return NotFound();

        var fullPath = Path.Combine(_env.WebRootPath, contract.SignedAgreementPath);

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        var fileName = Path.GetFileName(fullPath);
        return PhysicalFile(fullPath, "application/pdf", fileName);
    }

    private static bool IsPdf(string fileName) =>
        Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    private async Task<string> SavePdfAsync(IFormFile file)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var fullPath = Path.Combine(uploadsPath, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"uploads/{fileName}";
    }
}

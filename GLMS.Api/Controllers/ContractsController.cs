using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;

namespace GLMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ContractsController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ContractStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        IQueryable<Contract> query = _context.Contracts
            .Include(c => c.Client)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(c => c.StartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(c => c.EndDate <= toDate.Value);

        List<Contract> contracts = await query.OrderByDescending(c => c.StartDate).ToListAsync();
        return Ok(contracts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        Contract contract = await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.ServiceRequests)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
            return NotFound();

        return Ok(contract);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] Contract contract, IFormFile? agreementFile)
    {
        if (agreementFile != null && agreementFile.Length > 0)
        {
            if (!IsPdf(agreementFile.FileName))
                return BadRequest("Only .pdf files are allowed for signed agreements.");

            contract.SignedAgreementPath = await SavePdfAsync(agreementFile);
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] Contract contract, IFormFile? agreementFile)
    {
        if (id != contract.Id)
            return BadRequest("ID mismatch.");

        Contract existing = await _context.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (existing == null)
            return NotFound();

        if (agreementFile != null && agreementFile.Length > 0)
        {
            if (!IsPdf(agreementFile.FileName))
                return BadRequest("Only .pdf files are allowed for signed agreements.");

            contract.SignedAgreementPath = await SavePdfAsync(agreementFile);
        }
        else
        {
            contract.SignedAgreementPath = existing.SignedAgreementPath;
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Contracts.Update(contract);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("The record was modified by another user.");
        }

        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        Contract contract = await _context.Contracts.FindAsync(id);

        if (contract == null)
            return NotFound();

        contract.Status = request.Status;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        Contract contract = await _context.Contracts.FindAsync(id);

        if (contract == null)
            return NotFound();

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        Contract contract = await _context.Contracts.FindAsync(id);

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

public record UpdateStatusRequest(ContractStatus Status);

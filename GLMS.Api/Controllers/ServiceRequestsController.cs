using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceRequestsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CurrencyService _currencyService;

    public ServiceRequestsController(AppDbContext context, CurrencyService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<ServiceRequest> requests = await _context.ServiceRequests
            .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
            .OrderByDescending(sr => sr.Id)
            .ToListAsync();

        return Ok(requests);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        ServiceRequest serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null)
            return NotFound();

        return Ok(serviceRequest);
    }

    [HttpGet("rate")]
    public async Task<IActionResult> GetZarRate()
    {
        decimal rate = await _currencyService.GetUsdToZarRateAsync();
        return Ok(new { rate });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ServiceRequest serviceRequest)
    {
        Contract contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);

        if (contract == null)
            return BadRequest("The selected contract does not exist.");

        if (contract.Status == ContractStatus.Expired)
            return BadRequest("A service request cannot be created against an Expired contract.");

        if (contract.Status == ContractStatus.OnHold)
            return BadRequest("A service request cannot be created against a contract that is On Hold.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        serviceRequest.CostZAR = await _currencyService.ConvertUsdToZarAsync(serviceRequest.CostUSD);

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = serviceRequest.Id }, serviceRequest);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        ServiceRequest serviceRequest = await _context.ServiceRequests.FindAsync(id);

        if (serviceRequest == null)
            return NotFound();

        _context.ServiceRequests.Remove(serviceRequest);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

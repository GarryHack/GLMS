using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;

namespace GLMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
     
        List<Client> clients = await _context.Clients
            .Include(c => c.Contracts)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(clients);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
         Client client = await _context.Clients
            .Include(c => c.Contracts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
            return NotFound();

        return Ok(client);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Client client)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Client client)
    {
        if (id != client.Id)
            return BadRequest("ID mismatch.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await _context.Clients.AnyAsync(c => c.Id == id))
            return NotFound();

        _context.Clients.Update(client);

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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        Client client = await _context.Clients.FindAsync(id);

        if (client == null)
            return NotFound();

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

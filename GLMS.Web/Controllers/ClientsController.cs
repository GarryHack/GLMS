using Microsoft.AspNetCore.Mvc;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers;

public class ClientsController : Controller
{
    private readonly GlmsApiClient _api;

    public ClientsController(GlmsApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        List<Client> clients = await _api.GetClientsAsync();
        return View(clients);
    }

    public async Task<IActionResult> Details(int id)
    {
        Client client = await _api.GetClientAsync(id);

        if (client == null)
            return NotFound();

        return View(client);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client)
    {
        if (!ModelState.IsValid)
            return View(client);

        var (success, error) = await _api.CreateClientAsync(client);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create client.");
            return View(client);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        Client client = await _api.GetClientAsync(id);

        if (client == null)
            return NotFound();

        return View(client);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Client client)
    {
        if (id != client.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(client);

        var (success, error) = await _api.UpdateClientAsync(id, client);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update client.");
            return View(client);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        Client client = await _api.GetClientAsync(id);

        if (client == null)
            return NotFound();

        return View(client);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _api.DeleteClientAsync(id);
        return RedirectToAction(nameof(Index));
    }
}

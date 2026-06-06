using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GLMS.Web.Models;

namespace GLMS.Web.Services;

public class GlmsApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public GlmsApiClient(HttpClient http)
    {
        _http = http;
    }



    public async Task<List<Client>> GetClientsAsync()
    {
        List<Client> ? result= await _http.GetFromJsonAsync<List<Client>>("api/clients", JsonOpts);
        return result ?? new List<Client>();
    }

    public async Task<Client?> GetClientAsync(int id)
    {
        try { return await _http.GetFromJsonAsync<Client>($"api/clients/{id}", JsonOpts); }
        catch (HttpRequestException) { return null; }
    }

    public async Task<(bool Success, string? Error)> CreateClientAsync(Client client)
    {
        var response = await _http.PostAsJsonAsync("api/clients", client);
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, body);
    }

    public async Task<(bool Success, string? Error)> UpdateClientAsync(int id, Client client)
    {
        var response = await _http.PutAsJsonAsync($"api/clients/{id}", client);
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, body);
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/clients/{id}");
        return response.IsSuccessStatusCode;
    }


    public async Task<List<Contract>> GetContractsAsync(ContractStatus? status, DateTime? fromDate, DateTime? toDate)
    {
        string url = "api/contracts";
        List<string> qs = new List<string>();
        if (status.HasValue) qs.Add($"status={status.Value}");
        if (fromDate.HasValue) qs.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) qs.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        if (qs.Count > 0) url += "?" + string.Join("&", qs);
        List<Contract>? result = await _http.GetFromJsonAsync<List<Contract>>(url, JsonOpts);
        return result ?? new List<Contract>();
    }

    public async Task<Contract?> GetContractAsync(int id)
    {
        try { return await _http.GetFromJsonAsync<Contract>($"api/contracts/{id}", JsonOpts); }
        catch (HttpRequestException) { return null; }
    }

    public async Task<(bool Success, string? Error)> CreateContractAsync(Contract contract, IFormFile? agreementFile)
    {
        using var form = BuildContractFormData(contract, agreementFile);
        var response = await _http.PostAsync("api/contracts", form);
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, body);
    }

    public async Task<(bool Success, string? Error)> UpdateContractAsync(int id, Contract contract, IFormFile? agreementFile)
    {
        using var form = BuildContractFormData(contract, agreementFile);
        var response = await _http.PutAsync($"api/contracts/{id}", form);
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, body);
    }

    public async Task<bool> UpdateContractStatusAsync(int id, ContractStatus status)
    {
        var response = await _http.PatchAsJsonAsync($"api/contracts/{id}/status", new { Status = status.ToString() });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteContractAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/contracts/{id}");
        return response.IsSuccessStatusCode;
    }

    public string GetDownloadUrl(int id) => $"{_http.BaseAddress}api/contracts/{id}/download";

    public async Task<List<ServiceRequest>> GetServiceRequestsAsync()
    {
        List<ServiceRequest> ?result = await _http.GetFromJsonAsync<List<ServiceRequest>>("api/servicerequests", JsonOpts);
        return result ?? new List<ServiceRequest>();
    }

    public async Task<ServiceRequest?> GetServiceRequestAsync(int id)
    {
        try { return await _http.GetFromJsonAsync<ServiceRequest>($"api/servicerequests/{id}", JsonOpts); }
        catch (HttpRequestException) { return null; }
    }

    public async Task<decimal> GetZarRateAsync()
    {
        try
        {
            ZarRateResponse? result = await _http.GetFromJsonAsync<ZarRateResponse>("api/servicerequests/rate", JsonOpts);
            return result?.Rate ?? 18.50m;
        }
        catch { return 18.50m; }
    }

    public async Task<(bool Success, string? Error)> CreateServiceRequestAsync(ServiceRequest serviceRequest)
    {
        var response = await _http.PostAsJsonAsync("api/servicerequests", serviceRequest);
        if (response.IsSuccessStatusCode) return (true, null);
        var body = await response.Content.ReadAsStringAsync();
        return (false, body.Trim('"'));
    }

    public async Task<bool> DeleteServiceRequestAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/servicerequests/{id}");
        return response.IsSuccessStatusCode;
    }


    private static MultipartFormDataContent BuildContractFormData(Contract contract, IFormFile? file)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(contract.Id.ToString()), "Id" },
            { new StringContent(contract.ClientId.ToString()), "ClientId" },
            { new StringContent(contract.StartDate.ToString("yyyy-MM-dd")), "StartDate" },
            { new StringContent(contract.EndDate.ToString("yyyy-MM-dd")), "EndDate" },
            { new StringContent(contract.Status.ToString()), "Status" },
            { new StringContent(contract.ServiceLevel), "ServiceLevel" }
        };

        if (file != null && file.Length > 0)
        {
            var stream = file.OpenReadStream();
            form.Add(new StreamContent(stream), "agreementFile", file.FileName);
        }

        return form;
    }

    private record ZarRateResponse(decimal Rate);
}

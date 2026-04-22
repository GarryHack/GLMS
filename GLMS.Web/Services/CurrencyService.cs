using System.Text.Json;

namespace GLMS.Web.Services;

public class CurrencyService
{
    private readonly HttpClient _httpClient;
    private const decimal FallbackRate = 18.50m;
    private const string ApiUrl = "https://open.er-api.com/v6/latest/USD";

    public CurrencyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> GetUsdToZarRateAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(ApiUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("rates", out var rates) &&
                rates.TryGetProperty("ZAR", out var zarElement))
            {
                return zarElement.GetDecimal();
            }

            return FallbackRate;
        }
        catch
        {
            return FallbackRate;
        }
    }

    public async Task<decimal> ConvertUsdToZarAsync(decimal amountUsd)
    {
        var rate = await GetUsdToZarRateAsync();
        return Math.Round(amountUsd * rate, 2);
    }
}

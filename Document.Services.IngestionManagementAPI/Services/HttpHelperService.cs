
using Document.Services.IngestionManagementAPI.Services.IServices;
using System.Text;
using System.Text.Json;

namespace Document.Services.IngestionManagementAPI.Services;

public class HttpHelperService : IHttpHelperService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpHelperService> _logger;

    public HttpHelperService(HttpClient httpClient, ILogger<HttpHelperService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TResponse?> SendRequestAsync<TResponse>(
        HttpMethod method,
        string url,
        object? payload = null,
        Dictionary<string, string>? headers = null)
    {
        try
        {
            var request = new HttpRequestMessage(method, url);

            // Add headers if provided
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Add payload if it's a POST/PUT/PATCH and there's a payload
            if (payload != null && (method == HttpMethod.Post || method == HttpMethod.Put || method.Method == "PATCH"))
            {
                var json = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP request failed: {Method} {Url}", method, url);
            return default;
        }
    }
}

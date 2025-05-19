namespace Document.Services.IngestionManagementAPI.Services.IServices;
public interface IHttpHelperService
{
    Task<TResponse?> SendRequestAsync<TResponse>(
        HttpMethod method,
        string url,
        object? payload = null,
        Dictionary<string, string>? headers = null);
}

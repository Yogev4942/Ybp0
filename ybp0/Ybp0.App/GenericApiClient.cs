using System.Net.Http.Json;

namespace Ybp0.App;

public static class GenericApiClient
{
    private static HttpClient? _httpClient;
    private static string _baseUrl = GetDefaultBaseUrl();

    public static string BaseUrl => _baseUrl;

    public static void Configure(string baseUrl)
    {
        _baseUrl = baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : baseUrl + "/";
        _httpClient = null;
    }

    public static async Task<TResponse?> GetAsync<TResponse>(string path)
    {
        HttpResponseMessage response = await Client.GetAsync(NormalizePath(path));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public static async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest data)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(NormalizePath(path), data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public static async Task<TResponse?> PutAsync<TRequest, TResponse>(string path, TRequest data)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync(NormalizePath(path), data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public static async Task DeleteAsync(string path)
    {
        HttpResponseMessage response = await Client.DeleteAsync(NormalizePath(path));
        response.EnsureSuccessStatusCode();
    }

    private static HttpClient Client => _httpClient ??= new HttpClient
    {
        BaseAddress = new Uri(_baseUrl),
        Timeout = TimeSpan.FromSeconds(20)
    };

    private static string NormalizePath(string path)
    {
        return path.TrimStart('/');
    }

    private static string GetDefaultBaseUrl()
    {
#if ANDROID
        return "http://10.0.2.2:59992/api/";
#else
        return "http://127.0.0.1:59992/api/";
#endif
    }
}

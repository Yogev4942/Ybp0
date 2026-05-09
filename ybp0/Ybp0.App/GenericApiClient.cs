using System.Net.Http.Json;
using System.Text.Json;

namespace Ybp0.App;

public static class GenericApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true
    };

    private static HttpClient? _httpClient;
    private static readonly string _baseUrl = GetDefaultBaseUrl();

    public static string BaseUrl => _baseUrl;

    public static void Init()
    {
        if (_httpClient == null)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(20)
            };
        }
    }

    public static async Task<TResponse?> GetAsync<TResponse>(string path)
    {
        Init();
        HttpResponseMessage response = await _httpClient!.GetAsync(NormalizePath(path));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
    }

    public static async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest data)
    {
        Init();
        HttpResponseMessage response = await _httpClient!.PostAsJsonAsync(NormalizePath(path), data, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
    }

    public static async Task<TResponse?> PutAsync<TRequest, TResponse>(string path, TRequest data)
    {
        Init();
        HttpResponseMessage response = await _httpClient!.PutAsJsonAsync(NormalizePath(path), data, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
    }

    public static async Task DeleteAsync(string path)
    {
        Init();
        HttpResponseMessage response = await _httpClient!.DeleteAsync(NormalizePath(path));
        response.EnsureSuccessStatusCode();
    }

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

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ybp0.App;

public static class GenericApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true,
        Converters = { new AspNetDateTimeConverter() }
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

#if ANDROID
            // Android emulator reaches the host PC through 10.0.2.2, but IIS Express
            // is bound to localhost and rejects requests with a 10.0.2.2 Host header.
            _httpClient.DefaultRequestHeaders.Host = "localhost:59992";
#endif
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
        return "http://localhost:59992/api/";
#endif
    }
}

public class AspNetDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly Regex AspNetDateRegex = new(@"^/Date\((-?\d+)([+-]\d{4})?\)/$", RegexOptions.Compiled);

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string value = reader.GetString() ?? string.Empty;
            Match match = AspNetDateRegex.Match(value);

            if (match.Success && long.TryParse(match.Groups[1].Value, out long milliseconds))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).LocalDateTime;
            }

            if (DateTime.TryParse(value, out DateTime dateTime))
            {
                return dateTime;
            }
        }

        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

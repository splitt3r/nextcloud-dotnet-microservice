using System.Text;
using System.Text.Json;

class NextcloudClient : IDisposable
{
    private string _baseUrl;
    private HttpClient _httpClient;

    public NextcloudClient(string baseUrl, IList<KeyValuePair<string, string>> headers)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
        foreach (var header in headers)
        {
            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    public async Task<string> Get(string path)
    {
        return await Request(HttpMethod.Get, path);
    }

    public async Task<string> Delete(string path, object content)
    {
        return await Request(HttpMethod.Delete, path, content);
    }

    public async Task<string> Post(string path, object content)
    {
        return await Request(HttpMethod.Post, path, content);
    }

    public async Task<string> Put(string path, object content)
    {
        return await Request(HttpMethod.Put, path, content);
    }

    private async Task<string> Request(HttpMethod httpMathod, string path, object? content = null)
    {
        HttpRequestMessage req = new(httpMathod, $"{_baseUrl}/{path}")
        {
            Content = content != null ? new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json") : null
        };
        HttpResponseMessage resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
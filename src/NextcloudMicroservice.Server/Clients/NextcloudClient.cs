using System.Text;
using System.Text.Json;

namespace NextcloudMicroservice.Server.Clients;

public class NextcloudClient(HttpClient httpClient) : IDisposable
{
    private HttpClient _httpClient = httpClient;

    public void SetDefaultRequestHeaders(IHeaderDictionary headers)
    {
        // TODO: error handling with TryGetValue
        var headerList = new List<KeyValuePair<string, string>>
        {
            new("OCS-APIRequest", "true"),
            new("AA-VERSION", headers["AA-VERSION"].ToString()),
            new("EX-APP-ID", headers["EX-APP-ID"].ToString()),
            new("EX-APP-VERSION", headers["EX-APP-VERSION"].ToString()),
            new("AUTHORIZATION-APP-API", headers["AUTHORIZATION-APP-API"].ToString())
        };
        foreach (var header in headerList)
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
        HttpRequestMessage req = new(httpMathod, $"{_httpClient.BaseAddress}/{path}")
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

using System.Net;

namespace Wra10Core2023.Util;

public static class HttpUtil
{
    public static async Task<HttpContent> GetAsync(string uri, string token = null,
        double timeoutSec = 10.0)
    {
        var handler = CompressionHandler;
        using var c = new HttpClient(handler);
        c.Timeout = TimeSpan.FromSeconds(timeoutSec);
        if (token != null) c.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        var res = await c.GetAsync(uri);
        res.EnsureSuccessStatusCode();
        return res.Content;
    }

    public static HttpClientHandler CompressionHandler => new()
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    };

    /// <summary>
    /// Use HTTP GET. Note: This throws exception if the response status code is not
    /// 200 (OK)
    /// </summary>
    public static async Task<string> GetStringAsync(string uri, string token = null,
        double timeoutSec = 10.0)
    {
        var content = await GetAsync(uri, token, timeoutSec);
        return await content.ReadAsStringAsync();
    }

}

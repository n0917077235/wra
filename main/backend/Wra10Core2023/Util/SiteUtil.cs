using Microsoft.AspNetCore.Mvc.Routing;
using System.Security.Claims;

namespace Wra10Core2023.Util;

public static class SiteUtil
{
    public static SQLHelper MainDB(this IConfiguration c)
    {
        return new(c.GetConnectionString("Water2022"));
    }

    public static string? GetUserName(this HttpContext ctx)
    {
        return ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static async Task<string> ReadAllTextAsync(this Stream stream)
    {
        using var sr = new StreamReader(stream);
        return await sr.ReadToEndAsync();
    }
}

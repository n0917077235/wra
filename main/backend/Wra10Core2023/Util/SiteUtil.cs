using System.Security.Claims;
using Wra10Core2023.Util.AncadSensor;

namespace Wra10Core2023.Util;

public static class SiteUtil
{
    public static IConfiguration Config;

    public static bool RemoteVideoImageEnabled => bool.Parse(Config["RemoteVideoImage:Enable"]);
    public static string RemoteVideoImageUrl => Config["RemoteVideoImage:Url"];
    public static string AncadSensorKey => Config["AncadSensor:Key"];
    public static int AncadSensorPort => int.Parse(Config["AncadSensor:Port"]);
    public static string CwaToken => Config["CwaToken"];
    public static string DevAccessToken => Config["DevAccessToken"];
    public static SocketDataHandler AncadDataHandler = new();

    public static SQLHelper MainDB()
    {
        return new(Config.GetConnectionString("Water2022"));
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

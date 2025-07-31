using Newtonsoft.Json.Linq;
using System.Data;

namespace Wra10Core2023.Util;

public static class DataRowUtil
{
    public static T Get<T>(this DataRow row, string key) => (T)row[key];
    public static int GetInt(this DataRow row, string key) => row.Get<int>(key);
    public static string GetStr(this DataRow row, string key) => row.Get<string>(key);
    public static JToken GetJson(this DataRow row, string key)
        => JToken.Parse(row.GetStr(key));

    public static DateTime GetDate(this DataRow row, string key) => row.Get<DateTime>(key);
    public static byte[] GetBytes(this DataRow row, string key) => row.Get<byte[]>(key);

    public static T GetMaybe<T>(this DataRow row, string key) where T : class
    {
        if (row.IsNull(key)) return null;
        return (T)row[key];
    }

    public static T? GetNullable<T>(this DataRow row, string key) where T : struct
    {
        if (row.IsNull(key)) return null;
        return (T)row[key];
    }
}

using System.Globalization;

namespace Wra10Core2023.Util;

public static class DateUtil
{
    public static string StandardFormat = "yyyy/MM/dd HH:mm:ss";

    public static string ToStandardString(this DateTime t) => t.ToString(StandardFormat);

    public static bool TryParseFormat(string s, string format, out DateTime d)
    {
        return DateTime.TryParseExact(s, format, null, DateTimeStyles.None, out d);
    }

    public static DateTime ParseFormat(string s, string format)
    {
        if (!TryParseFormat(s, format, out var d)) throw new ArgumentException();
        return d;
    }
}

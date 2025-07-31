namespace Wra10Core2023.Util;

public static class DateUtil
{
    public static string StandardFormat = "yyyy/MM/dd HH:mm:ss";

    public static string ToStandardString(this DateTime t) => t.ToString(StandardFormat);

}

using Microsoft.Data.SqlClient;
using System.Data;

namespace Wra10Core2023.Util.Earthquake;

public static class EarthquakeCwa
{
    public static CwaEarthquakeEntry GetLatest()
    {
        var q = "SELECT TOP(1) OriginTime FROM EQOpenData ORDER BY OriginTime DESC";
        var table = SiteUtil.MainDB().ExecuteQuery(q);
        var t = table.Rows[0].GetDate("OriginTime");
        return GetEntry(t);
    }

    public static CwaEarthquakeEntry FindClosest(DateTime t)
    {
        var all = GetTimes(t.AddMinutes(-10), t.AddMinutes(10));
        var time = all.MinBy(x => Math.Abs((t - x).TotalSeconds));
        return GetEntry(time);
    }

    private static CwaEarthquakeEntry GetEntry(DateTime t)
    {
        var q = @"
            SELECT StationName, Latitude, Longitude, Intensity
            FROM EQOpenData WHERE OriginTime=@t
        ";

        SqlParameter[] p = [new("t", t)];
        var table = SiteUtil.MainDB().ExecuteQuery(q, p);

        var points = table.Rows.Cast<DataRow>()
            .Select(row => new EarthquakePoint
            {
                Lat = (double)row.GetDecimal("Latitude"),
                Lon = (double)row.GetDecimal("Longitude"),

                // Show 1 decimal place (e.g. 5.1, 5.9) only if needed
                Intensity = row.GetDouble("Intensity").ToString("0.#"),

                StationName = row.GetStr("StationName"),
            })
            .ToList();

        return new() { Time = t, Points = points };
    }

    public static List<DateTime> GetTimes(DateTime start, DateTime end)
    {
        var q = @"
            SELECT DISTINCT OriginTime FROM EQOpenData WHERE @start<=OriginTime AND OriginTime<@end
        ";

        SqlParameter[] p = [new("start", start), new("end", end)];
        var table = SiteUtil.MainDB().ExecuteQuery(q, p);

        return table.Rows.Cast<DataRow>()
            .Select(row => row.GetDate("OriginTime"))
            .OrderBy(x => x)
            .ToList();
    }
}

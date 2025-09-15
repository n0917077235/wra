using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Data;

namespace Wra10Core2023.Util.Earthquake;

public static class EarthquakeCwa
{
    public static async Task LoopAsync()
    {
        CreateTableIfNeeded();

        while (true)
        {
            try
            {
                await UpdateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            var fiveMin = 5 * 60_000;
            await Task.Delay(fiveMin);
        }
    }

    private static void CreateTableIfNeeded()
    {
        var q = @"
            IF OBJECT_ID('EarthquakeCwa', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[EarthquakeCwa](
	                [id] [int] IDENTITY(1,1) NOT NULL,
	                [t] [datetime] NOT NULL,
	                [json] [nvarchar](max) NOT NULL
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

                CREATE UNIQUE NONCLUSTERED INDEX [IDX_t] ON [dbo].[EarthquakeCwa]
                (
	                [t] ASC
                )
            END
            ";

        SiteUtil.MainDB().ExecuteNonQuery(q);
    }

    public static CwaEarthquakeEntry GetLatest()
    {
        var q = "SELECT TOP(1) t, json FROM EarthquakeCwa ORDER BY t DESC";
        var table = SiteUtil.MainDB().ExecuteQuery(q);
        var row = table.Rows[0];

        return new CwaEarthquakeEntry
        {
            Time = row.GetDate("t"),
            Points = JToken.Parse(row.GetStr("json")).ToObject<List<EarthquakePoint>>()
        };
    }

    public static List<CwaEarthquakeEntry> Get(DateTime start, DateTime end)
    {
        var q = "SELECT t, json FROM EarthquakeCwa WHERE @start<=t AND t<@end";
        SqlParameter[] p = [new("start", start), new("end", end)];
        var table = SiteUtil.MainDB().ExecuteQuery(q, p);

        return table.Rows.Cast<DataRow>()
            .Select(row => new CwaEarthquakeEntry
            {
                Time = row.GetDate("t"),
                Points = JToken.Parse(row.GetStr("json")).ToObject<List<EarthquakePoint>>()
            })
            .ToList();
    }

    public static CwaEarthquakeEntry FindClosest(DateTime t)
    {
        var all = Get(t.AddMinutes(-10), t.AddMinutes(10));
        return all.MinBy(x => Math.Abs((t - x.Time).TotalSeconds));
    }

    public static async Task UpdateAsync()
    {
        var all = await FetchAsync();
        foreach (var entry in all) Update(entry);
    }

    private static void Update(CwaEarthquakeEntry entry)
    {
        var json = JToken.FromObject(entry.Points).ToString();

        var q = $@"
            UPDATE EarthquakeCwa SET json=@json WHERE t=@t;

            IF @@ROWCOUNT = 0
                INSERT INTO EarthquakeCwa (t, json) VALUES (@t, @json);
            ";

        SqlParameter[] p =
        [
            new("t", entry.Time),
            new("json", json),
        ];

        SiteUtil.MainDB().ExecuteNonQuery(q, p);
    }

    public static async Task<List<CwaEarthquakeEntry>> FetchAsync()
    {
        var apikey = SiteUtil.CwaToken;

        // 顯著有感地震報告
        var url = $"https://opendata.cwa.gov.tw/api/v1/rest/datastore/E-A0015-001?" +
            $"Authorization={apikey}&format=JSON";

        var json = await HttpUtil.GetStringAsync(url);
        var t = JToken.Parse(json);

        var earthquakes = t["records"]["Earthquake"].Select(eq =>
        {
            var info = eq["EarthquakeInfo"];
            var time = DateUtil.ParseFormat(info.GetStr("OriginTime"), "yyyy-MM-dd HH:mm:ss");
            var stations = eq["Intensity"]["ShakingArea"].SelectMany(x => x["EqStation"]);

            var points = stations.Select(x => new EarthquakePoint
            {
                StationName = x.GetStr("StationName"),
                Lat = x.GetDouble("StationLatitude"),
                Lon = x.GetDouble("StationLongitude"),
                Intensity = MapIntensity(x.GetStr("SeismicIntensity")),
            }).ToList();

            return new CwaEarthquakeEntry { Time = time, Points = points };
        }).ToList();

        return earthquakes;
    }

    private static string MapIntensity(string text) => text switch
    {
        "1級" => "1",
        "2級" => "2",
        "3級" => "3",
        "4級" => "4",
        "5弱" => "5.1",
        "5強" => "5.9",
        "6弱" => "6.1",
        "6強" => "6.9",
        "7級" => "7",
        _ => throw new NotSupportedException()
    };
}

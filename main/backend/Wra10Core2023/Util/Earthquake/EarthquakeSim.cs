using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Wra10Core2023.Util.Earthquake;

public static class EarthquakeSim
{
    public static void Insert(DateTime t)
    {
        InsertCwa(t);
        InsertWra10(t);
        Console.WriteLine("earthquake sim inserted");
    }

    public static void Clear(DateTime t)
    {
        ClearCwa(t);
        ClearWra10(t);
        Console.WriteLine("earthquake sim cleared");
    }

    private static void InsertCwa(DateTime t)
    {
        var lines = File.ReadAllLines("Data/eq_sim.csv");
        var items = SimpleCsv.Parse(lines, true, words => words);

        foreach (var words in items)
        {
            var q = @"
            INSERT INTO EQOpenData 
            (EQNo, OriginTime, CountyName, StationId
            , StationName, Latitude, Longitude, Intensity)
            VALUES
            (@EQNo, @OriginTime, @CountyName, @StationId
            , @StationName, @Latitude, @Longitude, @Intensity);
            ";

            SqlParameter[] p =
            [
                new("EQNo", int.Parse(words[0])),
                new("OriginTime", t),
                new("CountyName", words[2]),
                new("StationId", words[3]),
                new("StationName", words[4]),
                new("Latitude", decimal.Parse(words[5])),
                new("Longitude", decimal.Parse(words[6])),
                new("Intensity", double.Parse(words[7])),
            ];

            SiteUtil.MainDB().ExecuteNonQuery(q, p);
        }
    }

    private static void ClearCwa(DateTime t)
    {
        var q = "DELETE FROM EQOpenData WHERE EQNo=99999 AND OriginTime=@t";
        SqlParameter[] p = [new("t", t)];
        SiteUtil.MainDB().ExecuteNonQuery(q, p);
    }

    private static void InsertWra10(DateTime t)
    {
        var lines = File.ReadAllLines("Data/wra10_sim.csv");
        var items = SimpleCsv.Parse(lines, true, words => words);

        foreach (var words in items)
        {
            var q = @"
            INSERT INTO EarthquakeMain 
            (SensorId, RecordTime, Intensity, EventTag, FileName, GroupTime)
            VALUES
            (@SensorId, @RecordTime, @Intensity, @EventTag, @FileName, @GroupTime);
            ";

            SqlParameter[] p =
            [
                new("SensorId", words[0]),
                new("RecordTime", t),
                new("Intensity", double.Parse(words[2])),
                new("EventTag", words[3]),
                new("FileName", words[4]),
                new("GroupTime", t),
            ];

            SiteUtil.MainDB().ExecuteNonQuery(q, p);
        }
    }

    private static void ClearWra10(DateTime t)
    {
        var q = "DELETE FROM EarthquakeMain WHERE FileName='test' AND RecordTime=@t";
        SqlParameter[] p = [new("t", t)];
        SiteUtil.MainDB().ExecuteNonQuery(q, p);
    }
}

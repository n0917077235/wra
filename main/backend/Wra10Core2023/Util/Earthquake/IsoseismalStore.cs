using Microsoft.Data.SqlClient;
using System.Data;

namespace Wra10Core2023.Util.Earthquake;

public static class IsoseismalStore
{
    public static void CreateTableIfNeeded()
    {
        var q = @"
            IF OBJECT_ID('IsoseismalStore', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[IsoseismalStore](
	                [id] [int] IDENTITY(1,1) NOT NULL,
	                [eventTime] [datetime] NOT NULL,
	                [image] [varbinary](max) NOT NULL
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

                CREATE NONCLUSTERED INDEX [IDX_eventTime] ON [dbo].[IsoseismalStore]
                (
	                [eventTime] ASC
                )
            END
            ";

        SiteUtil.MainDB().ExecuteNonQuery(q);
    }

    public record Item(DateTime Time, byte[] Image);

    public static byte[] Get(DateTime t)
    {
        var q = "SELECT image FROM IsoseismalStore WHERE eventTime=@t";
        SqlParameter[] p = [new("t", t)];
        var rows = SiteUtil.MainDB().ExecuteQuery(q, p).Rows;
        return rows[0].GetBytes("image");
    }

    public static List<Item> Get(DateTime start, DateTime end)
    {
        var q = @"
            SELECT eventTime, image FROM IsoseismalStore 
            WHERE @start <= eventTime AND eventTime < @end
            ";

        SqlParameter[] p = [new("start", start), new("end", end)];
        var rows = SiteUtil.MainDB().ExecuteQuery(q, p).Rows;

        return rows.Cast<DataRow>()
            .Select(row => new Item(row.GetDate("eventTime"), row.GetBytes("image")))
            .ToList();
    }

    public static void Insert(DateTime time, byte[] image)
    {
        var q = $@"
            UPDATE IsoseismalStore SET image=@bytes WHERE eventTime=@t;

            IF @@ROWCOUNT = 0
                INSERT INTO IsoseismalStore (eventTime, image) 
                VALUES (@t, @bytes);
            ";

        SqlParameter[] p = [new("bytes", image), new("t", time)];
        SiteUtil.MainDB().ExecuteNonQuery(q, p);
    }

    public static void Delete(DateTime time)
    {
        var q = "DELETE FROM IsoseismalStore WHERE eventTime=@t";
        SqlParameter[] p = [new("t", time)];
        SiteUtil.MainDB().ExecuteNonQuery(q, p);
    }
}

using Microsoft.Data.SqlClient;
using System.Data;

namespace Wra10Core2023.Util;

public static class Drawings
{
    public static void CreateTableIfNeeded()
    {
        var q = @"
            IF OBJECT_ID('MapDrawings', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[MapDrawings](
                    [id] [int] IDENTITY(1,1) NOT NULL,
                    [userId] [nvarchar](50) NOT NULL,
                    [name] [nvarchar](100) NOT NULL,
                    [geojson] [nvarchar](max) NOT NULL
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

                CREATE UNIQUE NONCLUSTERED INDEX [IDX_userId_name] ON [dbo].[MapDrawings]
                (
                    [userId] ASC,
                    [name] ASC
                )
            END
            ";

        SiteUtil.MainDB().ExecuteNonQuery(q);
    }

    public static void Update(string userId, string name, string geojson)
    {
        var q = $@"
            UPDATE MapDrawings SET geojson=@geojson WHERE userId=@uid AND name=@name;

            IF @@ROWCOUNT = 0
                INSERT INTO MapDrawings (userId, name, geojson) 
                VALUES (@uid, @name, @geojson);
            ";

        SqlParameter[] p =
        [
            new("uid", userId),
            new("name", name),
            new("geojson", geojson)
        ];

        SiteUtil.MainDB().ExecuteNonQuery(q, p);
    }

    public static string Get(string userId, string name)
    {
        var q = $@"SELECT geojson FROM MapDrawings WHERE userId=@uid AND name=@name";
        SqlParameter[] p = [new("uid", userId), new("name", name)];
        var table = SiteUtil.MainDB().ExecuteQuery(q, p);
        return table.Rows[0].GetStr("geojson");
    }

    public static IEnumerable<string> GetNames(string userId)
    {
        var q = $@"SELECT name FROM MapDrawings WHERE userId=@uid";
        SqlParameter[] p = [new("uid", userId)];
        var table = SiteUtil.MainDB().ExecuteQuery(q, p);
        return table.Rows.Cast<DataRow>().Select(x => x.GetStr("name"));
    }

    public static void Delete(string userId, string name)
    {
        var q = $@"DELETE FROM MapDrawings WHERE userId=@uid AND name=@name";
        SqlParameter[] p = [new("uid", userId), new("name", name)];
        SiteUtil.MainDB().ExecuteNonQuery(q, p);
    }

}

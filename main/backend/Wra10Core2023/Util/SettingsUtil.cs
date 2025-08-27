using Microsoft.Data.SqlClient;
using System.Data;

namespace Wra10Core2023.Util;

public static class SettingsUtil
{
    public static string? Get(string id)
    {
        var q = "SELECT Value FROM Settings WHERE Id=@id";
        SqlParameter[] p = [new("id", id)];
        var rows = SiteUtil.MainDB().ExecuteQuery(q, p).Rows;
        if (rows.Count == 0) return null;
        return rows[0].GetStr("Value");
    }

    public static void Set(string id, string value)
    {
        var q = $@"
            UPDATE Settings SET Value=@value WHERE Id=@id;

            IF @@ROWCOUNT = 0
                INSERT INTO Settings (Id, Value) 
                VALUES (@id, @value);
            ";

        SqlParameter[] p = [new("value", value), new("id", id)];
        SiteUtil.MainDB().ExecuteNonQuery(q, p);
    }
}

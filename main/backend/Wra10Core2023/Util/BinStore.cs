namespace Wra10Core2023.Util;

public static class BinStore
{
    public static void CreateTableIfNeeded(IConfiguration c)
    {
        var q = @"
            IF OBJECT_ID('BinStore', 'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[BinStore](
	                [Name] [nvarchar](100) NOT NULL,
	                [Value] [varbinary](max) NOT NULL
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
                GO

                CREATE UNIQUE NONCLUSTERED INDEX [IDX_Name] ON [dbo].[BinStore]
                (
	                [Name] ASC
                )
            END
            ";

        c.MainDB().ExecuteQuery(q);
    }

    public static byte[] Get(string key)
    {

    }

    public static void Set(string key, byte[] value)
    {

    }
}

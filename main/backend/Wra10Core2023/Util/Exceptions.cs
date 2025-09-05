namespace Wra10Core2023.Util;

public static class Exceptions
{
    public static void IgnoreException(Action a)
    {
        try
        {
            a();
        }
        catch { }
    }

    public static void PrintIfFailed(Action a)
    {
        try
        {
            a();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

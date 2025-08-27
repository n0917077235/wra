namespace Wra10Core2023.Util;

public class YearMonth
{
    public int Year { get; set; }
    public int Month { get; set; }
    
    // For json deserialization
    public YearMonth() { }

    public YearMonth(int year, int month)
    {
        this.Year = year;
        this.Month = month;
    }

    public YearMonth(DateTime t) : this(t.Year, t.Month) { }

    public YearMonth GetNextMonth()
    {
        return Month == 12 ? new(Year + 1, 1) : new(Year, Month + 1);
    }

    public static YearMonth Min(YearMonth x, YearMonth y)
    {
        return x > y ? y : x;
    }

    public static bool operator ==(YearMonth x, YearMonth y)
    {
        return x.Year == y.Year && x.Month == y.Month;
    }

    public static bool operator >(YearMonth x, YearMonth y)
    {
        return x.Year > y.Year || (x.Year == y.Year && x.Month > y.Month);
    }

    public static bool operator <(YearMonth x, YearMonth y)
    {
        return x.Year < y.Year || (x.Year == y.Year && x.Month < y.Month);
    }

    public static bool operator !=(YearMonth x, YearMonth y) => !(x == y);
    public static bool operator >=(YearMonth x, YearMonth y) => x > y || x == y;
    public static bool operator <=(YearMonth x, YearMonth y) => x < y || x == y;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj != null && obj is YearMonth y && this == y;
    }

    public override int GetHashCode() => (Year * 397) ^ Month;
}

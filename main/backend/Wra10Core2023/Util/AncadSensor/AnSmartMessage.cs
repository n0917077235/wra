namespace Wra10Core2023.Util.AncadSensor;

public class AnSmartMessage
{
    public const int AnalogInputCount = 4;
    public const int DigitalInputCount = 16;

    private static readonly string[] InputColumns =
    [
        .. Enumerable.Range(0, AnalogInputCount).Select(i => $"AIN{i}"),
        .. Enumerable.Range(0, DigitalInputCount).Select(i => $"DIG{i}"),
    ];

    private static readonly string[] Columns;
    public static IReadOnlyList<string> ColumnNames => Columns;

    static AnSmartMessage()
    {
        Columns = ["BAT0", "BAT1", "RSSI", .. InputColumns];
    }

    //public string DebugMessage;
    public DateTime Time;

    /// <summary>
    /// Length is the same as ColumnNames
    /// </summary>
    public float[] Data;

    public AnSmartMessage() { }

    public AnSmartMessage(string[] words, DateTime time)
    {
        Time = time;
        var head = 3;
        var colCount = ColumnNames.Count;

        Data = Enumerable.Range(head, colCount).Select(i =>
        {
            var b = Bits.HexToBytes(words[i]).Reverse().ToArray();
            return BitConverter.ToSingle(b, 0);
        }).ToArray();
    }
}

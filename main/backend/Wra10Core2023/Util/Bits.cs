namespace Wra10Core2023.Util;

public static class Bits
{
    /// <summary>
    /// Convert hex to byte array. 
    /// E.g. 
    /// "011E8081" => [1, 30, 128, 129]
    /// "011e8081" => [1, 30, 128, 129]
    /// </summary>
    public static byte[] HexToBytes(string hex)
    {
        var len = hex.Length / 2;
        var b = new byte[len];
        for (int i = 0; i < len; i++) b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return b;
    }
}

namespace Wra10Core2023.Util;

public static class SimpleCsv
{
    public static List<T> Parse<T>(string filePath, bool skipFirstLine,
        Func<List<string>, T> parseLine)
    {
        return Parse(filePath, skipFirstLine, (w, _) => parseLine(w));
    }

    public static List<T> Parse<T>(string filePath, bool skipFirstLine,
        Func<List<string>, int, T> parseLine)
    {
        return Parse(File.ReadAllLines(filePath), skipFirstLine, parseLine);
    }

    public static List<T> Parse<T>(IEnumerable<string> lines, bool skipFirstLine,
        Func<List<string>, T> parseLine)
    {
        return Parse(lines, skipFirstLine, (w, _) => parseLine(w));
    }

    /// <summary>
    /// parseLine: (words, line index) => List<T>
    /// </summary>
    public static List<T> Parse<T>(IEnumerable<string> lines, bool skipFirstLine,
        Func<List<string>, int, T> parseLine)
    {
        return Parse(lines, skipFirstLine, (words, i, line) => parseLine(words, i));
    }

    /// <summary>
    /// parseLine: (words, line index, line) => List<T>
    /// </summary>
    public static List<T> Parse<T>(IEnumerable<string> lines, bool skipFirstLine,
        Func<List<string>, int, string, T> parseLine)
    {
        return lines
            .Skip(skipFirstLine ? 1 : 0)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select((line, i) =>
            {
                var words = line.Split(',').Select(x => x.Trim()).ToList();
                return parseLine(words, i, line);
            })
            .ToList();
    }
}

using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Wra10Core2023.Util.Earthquake;

public static class IsoseismalUtil
{
    private const string LastRunYearMonthId = "isoseismal.last_run";

    public static async Task LoopAsync()
    {
        IsoseismalStore.CreateTableIfNeeded();

        while (true)
        {
            try
            {
                await GenerateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            var minute = 60_000;
            await Task.Delay(minute);
        }
    }

    private static YearMonth GetLastRun()
    {
        var json = SettingsUtil.Get(LastRunYearMonthId);
        if (json == null) return new YearMonth(2023, 1);
        return JToken.Parse(json).ToObject<YearMonth>();
    }

    private static async Task GenerateAsync()
    {
        var lastRun = GetLastRun();
        var current = new YearMonth(DateTime.Now);
        var t = YearMonth.Min(lastRun, current);

        while (t <= current)
        {
            var year = t.Year;
            var month = t.Month;
            var next = t.GetNextMonth();
            var start = new DateTime(year, month, 1);
            var end = new DateTime(next.Year, next.Month, 1);
            var existing = IsoseismalStore.Get(start, end);
            var exitingsTimes = existing.Select(x => x.Time).ToHashSet();

            var events = GetEQEvents(year, month, 3)
                .Where(x => x.HasIsoseismalMap)
                .Select(x => new { t = DateTime.Parse(x.Time), tStr = x.Time });

            var generate = events.Where(x => !exitingsTimes.Contains(x.t));

            foreach (var x in generate)
            {
                Console.WriteLine($"generating: {x.tStr}");
                var image = await GenerateImageAsync(x.tStr);
                IsoseismalStore.Insert(x.t, image);
                Console.WriteLine($"done: {x.tStr}");
            }

            t = next;
        }

        var json = JToken.FromObject(current).ToString();
        SettingsUtil.Set(LastRunYearMonthId, json);
    }

    public record EqEvent(string Time, bool HasIsoseismalMap);

    public static IEnumerable<EqEvent> GetEQEvents(int year, int month, float intensity)
    {
        SqlParameter[] parameters =
        [
            new("@year", year),
            new("@month", month),
            new("@intensity", intensity),
        ];

        var sqlHeper = SiteUtil.MainDB();
        var dt = sqlHeper.ExecuteStoreProcedureQuery("sp_getEQMapTimeNew", parameters);
        var rows = dt.Rows.Cast<DataRow>();
        return rows.Select(row => new EqEvent((string)row[0], (int)row[1] == 1));
    }

    private static string GetInputFile(string eventTime)
    {
        SqlParameter[] parameters = [new("@eventTime", eventTime)];
        var sqlHelper = SiteUtil.MainDB();
        var dt = sqlHelper.ExecuteStoreProcedureQuery("sp_Isoseismal", parameters);
        var sb = new StringBuilder();
        sb.AppendLine("N,E,震度");

        foreach (var row in dt.Rows.Cast<DataRow>())
        {
            sb.AppendLine($"{row[2]},{row[3]},{row[4]}");
        }

        return sb.ToString();
    }

    public static async Task<byte[]> GenerateImageAsync(string eventTime)
    {
        var input = GetInputFile(eventTime);
        var dir = FindDir();
        var f = Path.Combine(dir, "Input/input.txt");
        await File.WriteAllTextAsync(f, input);
        return await RunGeneratorAsync(dir);
    }

    private static async Task<byte[]> RunGeneratorAsync(string dir)
    {
        ClearDir(Path.Combine(dir, "Output"));
        var process = new Process();
        var startInfo = new ProcessStartInfo();
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "/C python srec_interpolate.py input.txt log_plot";
        startInfo.WorkingDirectory = dir;
        process.StartInfo = startInfo;
        process.Start();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0) throw new Exception($"python exit code={process.ExitCode}");
        var file = Path.Combine(dir, @"Output\input_twd97.png");
        return await File.ReadAllBytesAsync(file);
    }

    private static void ClearDir(string dir)
    {
        foreach (var f in Directory.GetFiles(dir)) File.Delete(f);
    }

    private static string FindDir()
    {
        var target = "srec_proj";
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        while (true)
        {
            var d = Path.Combine(dir, target);
            if (Directory.Exists(d)) return d;
            var parent = new DirectoryInfo(dir).Parent;
            if (parent == null) throw new DirectoryNotFoundException();
            dir = parent.FullName;
        }
    }

}
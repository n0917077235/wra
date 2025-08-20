using System;
using System.Data;
//using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using SqlHelper = Wra10Core2023.Util.SQLHelper;
using Notify.Services;
using Microsoft.Data.SqlClient;

using System.Net.Http;
using System.Globalization;

namespace ScheduledTask.Services
{
    public class ScheduledTaskServiceSystem : BackgroundService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ScheduledTaskServiceSystem> _logger;
        private readonly IConfiguration _configuration;
        private static readonly HttpClient _httpClient = new();
        private CrontabSchedule _schedule;
        private DateTime _nextRun;

        public ScheduledTaskServiceSystem(IWebHostEnvironment env, ILogger<ScheduledTaskServiceSystem> logger, IConfiguration configuration)
        {
            _env = env;
            _logger = logger;
            _configuration = configuration;

            // cron 表達式
            // 在每天的 08:30, 12:30, 16:30 執行
            _schedule = CrontabSchedule.Parse("30 8,12,16 * * *");
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("System定時任務已啟動");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                if (now >= _nextRun)
                {
                    try
                    {
                        Console.WriteLine($"System任務執行時間: {now}");

                        string? conn = _configuration.GetConnectionString("WaterToANCAD");
                        if (conn == null)
                        {
                            Console.WriteLine("無法取得資料庫連線字串");
                            return;
                        }

                        var sqlHelper = new SqlHelper(conn);

                        // 取出全部 Areas 資料
                        string sqlAreas = "SELECT * FROM Areas";
                        DataTable dtAreas = sqlHelper.ExecuteQuery(sqlAreas);
                        var areaDict = dtAreas.AsEnumerable()
                                            .Where(r => r["AreaID"] != DBNull.Value)
                                            .ToDictionary(r => r["AreaID"].ToString() ?? "",
                                                            r => r["AreaName"]?.ToString() ?? "");

                        // 取出全部 Stations 資料
                        string sqlStations = "SELECT * FROM Stations";
                        DataTable dtStations = sqlHelper.ExecuteQuery(sqlStations);
                        var stationDict = dtStations.AsEnumerable()
                                                    .Where(r => r["StationID"] != DBNull.Value)
                                                    .ToDictionary(r => r["StationID"].ToString() ?? "",
                                                                r => new {
                                                                    StationNameA = r["StationNameA"]?.ToString() ?? "",
                                                                    AreaID = r["AreaID"]?.ToString() ?? ""
                                                                });

                        // 取出全部 Sensors 資料
                        string sqlSensors = "SELECT * FROM Sensors";
                        DataTable dtSensors = sqlHelper.ExecuteQuery(sqlSensors);

                        var errorList = new List<string>();

                        // --- Sensors 異常處理 ---
                        foreach (DataRow row in dtSensors.Rows)
                        {
                            string sensorNameA = row["sensorNameA"]?.ToString() ?? "";
                            string areaId = row["AreaID"]?.ToString() ?? "";
                            string areaName = areaDict.ContainsKey(areaId) ? areaDict[areaId] : "";
                            string displayName = $"{areaName} {sensorNameA}";

                            int isDisc = row["isDisc"] != DBNull.Value ? Convert.ToInt32(row["isDisc"]) : 0;
                            int isAlarm = row["isAlarm"] != DBNull.Value ? Convert.ToInt32(row["isAlarm"]) : 0;

                            if (isDisc == 1)
                            {
                                errorList.Add($"{displayName} 斷線");
                            }
                            else if (isAlarm == 1)
                            {
                                errorList.Add($"{displayName} 回傳數值異常");
                            }
                        }

                        // --- Cameras 異常處理 ---
                        string sqlCameras = "SELECT * FROM Cameras";
                        DataTable dtCameras = sqlHelper.ExecuteQuery(sqlCameras);

                        foreach (DataRow row in dtCameras.Rows)
                        {
                            string camName = row["CamName"]?.ToString() ?? "";
                            string stationId = row["StationID"]?.ToString() ?? "";

                            if (!stationDict.ContainsKey(stationId))
                                continue;

                            var stationInfo = stationDict[stationId];
                            string stationNameA = stationInfo.StationNameA;
                            string areaId = stationInfo.AreaID;
                            string areaName = areaDict.ContainsKey(areaId) ? areaDict[areaId] : "";

                            string displayName = $"{areaName} {stationNameA} {camName}";

                            int isAlarm = row["isAlarm"] != DBNull.Value ? Convert.ToInt32(row["isAlarm"]) : 0;

                            if (isAlarm == 1)
                            {
                                errorList.Add($"{displayName} 斷線");
                            }
                        }

                        var channelToken = _configuration["Line:channelAccessToken"];
                        var groupId = _configuration["Line:GroupId"];

                        var message = errorList.Count > 0
                            ? string.Join("\n", errorList)
                            : "目前無異常";

                        var notify = new NotifyService(channelToken, _configuration);

                        // 每段最多 4000 字，避免超過限制
                        const int maxLength = 4000;
                        for (int i = 0; i < message.Length; i += maxLength)
                        {
                            var chunk = message.Substring(i, Math.Min(maxLength, message.Length - i));
                            await notify.PushTextAsync(groupId, chunk);
                        }

                        Console.WriteLine("System定時任務已完成一次");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"System任務執行出錯: {ex.Message}");
                    }



                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }

                await Task.Delay(1000, stoppingToken);
            }

            Console.WriteLine("System定時任務已停止");
        }

        public static async Task<DateTimeOffset?> GetLastModifiedAsync(string imageUrl)
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, imageUrl);
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            if (response.Content.Headers.LastModified.HasValue)
                return response.Content.Headers.LastModified.Value;

            if (response.Headers.TryGetValues("Last-Modified", out var values))
            {
                if (DateTimeOffset.TryParseExact(values.First(),
                        "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    return dt;
                }
            }

            return null;
        }

    }
}
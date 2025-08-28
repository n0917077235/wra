using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

using SqlHelper = Wra10Core2023.Util.SQLHelper;

using System.Net;
using System.Data;

using System.Net.Http;
using System.Globalization;

using Wra10Core2023.Models;
using Wra10Core2023.Services;

using NCrontab;

namespace Wra10Core2023.Services
{
    public class ScheduledTaskServiceCamera : BackgroundService
    {
        private readonly ILogger<ScheduledTaskServiceCamera> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private static readonly HttpClient _httpClient = new();

        private CrontabSchedule _schedule;
        private DateTime _nextRun;

        string? videoImagePath = "";
        bool bNetDrive = false;
        string login = "";
        string password = "";
        string driveLetter = "Q:";

        public ScheduledTaskServiceCamera(IWebHostEnvironment env, ILogger<ScheduledTaskServiceCamera> logger, IConfiguration configuration)
        {
            _env = env;
            _logger = logger;
            _configuration = configuration;

            videoImagePath = _configuration["VideoImage:Path"];
            bool.TryParse(_configuration["VideoImage:NetDrive"], out bNetDrive);
            login = _configuration["VideoImage:Login"];
            password = _configuration["VideoImage:Password"];
            driveLetter = _configuration["VideoImage:DriveLetter"];

            // cron 表達式，每 10 分鐘執行一次
            _schedule = CrontabSchedule.Parse("*/10 * * * *");
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Camera定時任務已啟動");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                if (now >= _nextRun)
                {
                    try
                    {
                        Console.WriteLine($"Camera任務執行時間: {now}");

                        DataTable dt;
                        List<CameraSearch> lstCamera = new List<CameraSearch>();
                        string? conn = _configuration.GetConnectionString("Water2022");
                        SqlHelper sqlHelper;

                        if (conn != null)
                        {
                            sqlHelper = new SqlHelper(conn);
                            List<SqlParameter> lstParams = new List<SqlParameter>
                            {
                                new SqlParameter("@areaId", DBNull.Value),
                                new SqlParameter("@StationId", DBNull.Value),
                                new SqlParameter("@keyword", DBNull.Value)
                            };
                            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetCameraBySearch", lstParams.ToArray());
                            foreach (DataRow row in dt.Rows)
                            {
                                string camName = Convert.ToString(row["StreamMain"]) ?? string.Empty;
                                lstCamera.Add(new CameraSearch
                                {
                                    StreamMain = camName,
                                    CamName = Convert.ToString(row["CamName"]) ?? string.Empty
                                });
                            }

                            Console.WriteLine($"攝影機數量：{lstCamera.Count}");
                            foreach (var cam in lstCamera.Take(5)) //測試所以不檢查全部
                            {
                                string imageUrl = $"https://rivermonitoring.wra10.gov.tw/fx/videoimages/{cam.StreamMain}";
                                DateTimeOffset? lastModified = await GetLastModifiedAsync(imageUrl);

                                if (lastModified.HasValue)
                                {
                                    DateTimeOffset t = lastModified.Value.ToLocalTime();
                                    TimeSpan diff = DateTimeOffset.Now - t;

                                    if (diff.TotalMinutes > 30)
                                    {
                                        // SQL：只有當設備斷線且尚未通報時才更新 isAlarm = 1
                                        string sqlUpdate = @"
                                        UPDATE Cameras
                                        SET isAlarm = 1
                                        WHERE CamName = @CamName
                                        AND (isAlarm IS NULL OR isAlarm <> 1)
                                        ";
                                        SqlParameter[] updateParams = new SqlParameter[]
                                        {
                                            new SqlParameter("@CamName", cam.CamName)
                                        };

                                        // 執行更新
                                        int rowsAffected = sqlHelper.ExecuteNonQuery(sqlUpdate, updateParams);

                                        if (rowsAffected > 0)
                                        {
                                            string lng = string.Empty;
                                            string lat = string.Empty;
                                            string sql = @"SELECT TOP 1 X, Y FROM Cameras WHERE CamName = @CamName";
                                            SqlParameter[] parameters = new SqlParameter[]
                                            {
                                                new SqlParameter("@CamName", cam.CamName)
                                            };

                                            try
                                            {
                                                DataTable area_dt = sqlHelper.ExecuteQuery(sql, parameters);
                                                if (area_dt.Rows.Count > 0)
                                                {
                                                    lng = area_dt.Rows[0]["X"].ToString() ?? string.Empty;
                                                    lat = area_dt.Rows[0]["Y"].ToString() ?? string.Empty;
                                                }
                                                else
                                                {
                                                    Console.WriteLine("⚠️ 找不到符合的資料");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                            }

                                            var channelToken = _configuration["Line:channelAccessToken"];
                                            var groupId = _configuration["Line:groupId"];
                                            var message = $"🔍❌缺測通報\n站點名稱：{cam.CamName}\r\n監視器超過30分鐘沒有新資料";
                                            Console.WriteLine(message);

                                            var notify = new NotifyService(channelToken, _configuration);
                                            await notify.PushMapAlertAsync(groupId, lat, lng, message);
                                        }
                                        else
                                        {
                                            // isAlarm 原本就是 1 → 已經通報，不做任何事
                                            Console.WriteLine($"📌 {cam.CamName} 已通報過，跳過通知");
                                        }
                                    }
                                    else
                                    {
                                        string sqlReset = @"
                                        UPDATE Cameras
                                        SET isAlarm = 0
                                        WHERE CamName = @CamName
                                        AND (isAlarm IS NULL OR isAlarm <> 0)
                                        ";
                                        sqlHelper.ExecuteNonQuery(sqlReset, new SqlParameter[] { new SqlParameter("@CamName", cam.CamName) });
                                    }
                                }

                                await Task.Delay(500);
                            }
                        }

                        Console.WriteLine("Camera定時任務已完成一次");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Camera任務執行出錯: {ex.Message}");
                    }

                    // 計算下一次執行時間
                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }

                // 每秒檢查一次是否到達排程時間
                await Task.Delay(1000, stoppingToken);
            }

            Console.WriteLine("Camera定時任務已停止");
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

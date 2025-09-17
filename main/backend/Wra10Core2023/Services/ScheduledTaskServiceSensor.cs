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
using Wra10Core2023.Services;
using Microsoft.Data.SqlClient;

namespace Wra10Core2023.Services
{
    public class ScheduledTaskServiceSensor : BackgroundService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ScheduledTaskServiceSensor> _logger;
        private readonly IConfiguration _configuration;
        private CrontabSchedule _schedule;
        private DateTime _nextRun;

        public ScheduledTaskServiceSensor(IWebHostEnvironment env, ILogger<ScheduledTaskServiceSensor> logger, IConfiguration configuration)
        {
            _env = env;
            _logger = logger;
            _configuration = configuration;

            // cron 表達式，每 10 分鐘執行一次
            _schedule = CrontabSchedule.Parse("*/10 * * * *");
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Sensor定時任務已啟動");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                if (now >= _nextRun)
                {
                    try
                    {
                        Console.WriteLine($"Sensor任務執行時間: {now}");

                        string? conn = _configuration.GetConnectionString("Water2022");
                        if (conn == null)
                        {
                            Console.WriteLine("無法取得資料庫連線字串");
                            continue;
                        }

                        var sqlHelper = new SqlHelper(conn);

                        // 取出全部 Sensors 資料，並關聯 Areas 表格
                        string sql = @"SELECT s.*, a.AreaName 
                                     FROM Sensors s 
                                     LEFT JOIN Areas a ON s.AreaID = a.AreaID";
                        DataTable dt = sqlHelper.ExecuteQuery(sql);

                        foreach (DataRow row in dt.Rows)
                        {
                            string sensorId = row["sensorId"]?.ToString() ?? "";
                            string sensorNameA = row["sensorNameA"]?.ToString() ?? "";
                            string areaName = row["AreaName"]?.ToString() ?? "";
                            DateTime? lastDataTime = row["LastDataTime"] as DateTime?;
                            double LastValue1 = row["LastValue1"] != DBNull.Value ? Convert.ToDouble(row["LastValue1"]) : double.NaN;
                            int isDisc = row["isDisc"] != DBNull.Value ? Convert.ToInt32(row["isDisc"]) : 0;
                            int isAlarm = row["isAlarm"] != DBNull.Value ? Convert.ToInt32(row["isAlarm"]) : 0;
                            double? lng = row["X"] != DBNull.Value ? Convert.ToDouble(row["X"]) : (double?)null;
                            double? lat = row["Y"] != DBNull.Value ? Convert.ToDouble(row["Y"]) : (double?)null;

                            bool needUpdateDisc = false;
                            bool needUpdateAlarm = false;

                            // 判斷缺測或異常值
                            if (lastDataTime.HasValue && (now - lastDataTime.Value).TotalMinutes > 30)
                            {
                                // 缺測 → 優先處理
                                if (isDisc != 1) // 原本不是缺測 → 更新 & 通報
                                {
                                    isDisc = 1;
                                    needUpdateDisc = true;

                                    // 缺測時異常值狀態清除
                                    if (isAlarm != 0)
                                    {
                                        isAlarm = 0;
                                        needUpdateAlarm = true;
                                    }

                                    string channelToken = _configuration["Line:channelAccessToken"];
                                    string groupId = _configuration["Line:groupId"];
                                    string message = $"🔍❌缺測通報\n站點名稱：{areaName} {sensorNameA}\r\n感測器超過30分鐘沒有新資料";

                                    var notify = new NotifyService(channelToken, _configuration);
                                    if (lat.HasValue && lng.HasValue)
                                        await notify.PushMapAlertAsync(groupId, lat.Value.ToString(), lng.Value.ToString(), message);
                                    else
                                        await notify.PushTextAsync(groupId, message);
                                }
                            }
                            else
                            {
                                // 缺測恢復
                                if (isDisc != 0)
                                {
                                    isDisc = 0;
                                    needUpdateDisc = true;
                                }

                                // 再檢查異常值
                                if (LastValue1 == -998)
                                {
                                    if (isAlarm != 1) // 原本不是異常 → 更新 & 通報
                                    {
                                        isAlarm = 1;
                                        needUpdateAlarm = true;

                                        string channelToken = _configuration["Line:channelAccessToken"];
                                        string groupId = _configuration["Line:groupId"];
                                        string message = $"⚠️異常值通報\n站點名稱：{sensorNameA}\r\n感測器回傳異常值 (-998)";

                                        var notify = new NotifyService(channelToken, _configuration);
                                        if (lat.HasValue && lng.HasValue)
                                            await notify.PushMapAlertAsync(groupId, lat.Value.ToString(), lng.Value.ToString(), message);
                                        else
                                            await notify.PushTextAsync(groupId, message);
                                    }
                                }
                                else
                                {
                                    if (isAlarm != 0) // 從異常恢復
                                    {
                                        isAlarm = 0;
                                        needUpdateAlarm = true;
                                    }
                                }
                            }


                            // 更新資料庫
                            if (needUpdateDisc || needUpdateAlarm)
                            {
                                string updateSql = @"
                                    UPDATE Sensors
                                    SET isDisc = @isDisc, isAlarm = @isAlarm
                                    WHERE SensorID = @SensorID";

                                sqlHelper.ExecuteNonQuery(updateSql, new SqlParameter[]
                                {
                                    new SqlParameter("@isDisc", isDisc),
                                    new SqlParameter("@isAlarm", isAlarm),
                                    new SqlParameter("@SensorID", sensorId)
                                });
                            }

                        }

                        Console.WriteLine("Sensor定時任務已完成一次");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Sensor任務執行出錯: {ex.Message}");
                    }

                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }

                await Task.Delay(1000, stoppingToken);
            }

            Console.WriteLine("Sensor定時任務已停止");
        }

    }
}
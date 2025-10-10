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
                            string sensorType = row["SensorType"]?.ToString() ?? "";
                            DateTime? lastDataTime = row["LastDataTime"] as DateTime?;
                            double LastValue1 = row["LastValue1"] != DBNull.Value ? Convert.ToDouble(row["LastValue1"]) : double.NaN;
                            int isDisc = row["isDisc"] != DBNull.Value ? Convert.ToInt32(row["isDisc"]) : 0;
                            int isAlarm = row["isAlarm"] != DBNull.Value ? Convert.ToInt32(row["isAlarm"]) : 0;
                            int thresholdAlarm = row["ThresholdAlarm"] != DBNull.Value ? Convert.ToInt32(row["ThresholdAlarm"]) : 0;
                            double threshold1 = row["Threshold1"] != DBNull.Value ? Convert.ToDouble(row["Threshold1"]) : double.MaxValue;
                            double threshold2 = row["Threshold2"] != DBNull.Value ? Convert.ToDouble(row["Threshold2"]) : double.MaxValue;
                            double threshold3 = row["Threshold3"] != DBNull.Value ? Convert.ToDouble(row["Threshold3"]) : double.MaxValue;
                            double? lng = row["X"] != DBNull.Value ? Convert.ToDouble(row["X"]) : (double?)null;
                            double? lat = row["Y"] != DBNull.Value ? Convert.ToDouble(row["Y"]) : (double?)null;

                            bool needUpdateDisc = false;
                            bool needUpdateAlarm = false;

                            // 判斷缺測
                            if (lastDataTime.HasValue && (now - lastDataTime.Value).TotalMinutes > 30)
                            {
                                // 缺測狀態處理
                                if (isDisc != 1) // 原本不是缺測 → 更新 & 通報
                                {
                                    isDisc = 1;
                                    needUpdateDisc = true;

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
                            }

                            // 檢查異常值
                            if (LastValue1 <= -888)
                            {
                                if (isAlarm != 1) // 原本不是異常 → 更新 & 通報
                                {
                                    isAlarm = 1;
                                    needUpdateAlarm = true;

                                    // string channelToken = _configuration["Line:channelAccessToken"];
                                    // string groupId = _configuration["Line:groupId"];
                                    // string message = $"⚠️異常值通報\n站點名稱：{areaName} {sensorNameA}\r\n感測器回傳異常值 (-998)";

                                    // var notify = new NotifyService(channelToken, _configuration);
                                    // if (lat.HasValue && lng.HasValue)
                                    //     await notify.PushMapAlertAsync(groupId, lat.Value.ToString(), lng.Value.ToString(), message);
                                    // else
                                    //     await notify.PushTextAsync(groupId, message);
                                }
                            }
                            else
                            {
                                    if (isAlarm != 0) // 從異常恢復
                                    {
                                        isAlarm = 0;
                                        needUpdateAlarm = true;
                                    }

                                    // 共用的警戒值判斷邏輯
                                    async Task CheckThresholdAlarm(string sensorTypeString, string unitString, bool hasLevel3 = true)
                                    {
                                        int newThresholdAlarm = 0;
                                        string alertLevel = "";

                                        // 判斷警戒等級 (一級警戒最嚴重)
                                        // 使用絕對值進行判斷，只在警戒值不為 NULL (不等於 MaxValue) 時才進行判斷
                                        double absValue = Math.Abs(LastValue1);
                                        if (threshold1 != double.MaxValue && absValue >= threshold1)
                                        {
                                            newThresholdAlarm = 1;
                                            alertLevel = "一級";
                                        }
                                        else if (threshold2 != double.MaxValue && absValue >= threshold2)
                                        {
                                            newThresholdAlarm = 2;
                                            alertLevel = "二級";
                                        }
                                        else if (hasLevel3 && threshold3 != double.MaxValue && absValue >= threshold3)
                                        {
                                            newThresholdAlarm = 3;
                                            alertLevel = "三級";
                                        }

                                        // 如果警戒等級改變，更新狀態
                                        if (newThresholdAlarm != thresholdAlarm)
                                        {
                                            // 發送通知的條件：
                                            // 1. 從正常狀態(0)進入任何警戒狀態
                                            // 2. 從較低警戒等級進入較高警戒等級
                                            bool shouldNotify = (thresholdAlarm == 0 && newThresholdAlarm > 0) || // 從正常進入警戒
                                                              (thresholdAlarm > 0 && newThresholdAlarm > 0 && newThresholdAlarm < thresholdAlarm); // 警戒等級提升

                                            if (shouldNotify)
                                            {
                                                string channelToken = _configuration["Line:channelAccessToken"];
                                                string groupId = _configuration["Line:groupId"];
                                                string message = $"⚠️{sensorTypeString}{alertLevel}警戒通報\n" +
                                                               $"站點名稱：{areaName} {sensorNameA}\n" +
                                                               $"目前數值：{LastValue1:F2}{unitString}";

                                                var notify = new NotifyService(channelToken, _configuration);
                                                if (lat.HasValue && lng.HasValue)
                                                    await notify.PushMapAlertAsync(groupId, lat.Value.ToString(), lng.Value.ToString(), message);
                                                else
                                                    await notify.PushTextAsync(groupId, message);
                                            }

                                            // 無論是否發送通知，都要更新狀態
                                            thresholdAlarm = newThresholdAlarm;
                                            needUpdateAlarm = true;
                                        }
                                    }

                                    // 判斷水位警戒
                                    if (sensorType == "WaterLevel")
                                    {
                                        await CheckThresholdAlarm("水位計", "公尺");
                                    }
                                    // 判斷沉陷警戒
                                    else if (sensorType == "Sink")
                                    {
                                        await CheckThresholdAlarm("沉陷計", "毫米");
                                    }
                                    // 判斷裂縫警戒
                                    else if (sensorType == "Crack")
                                    {
                                        // 裂縫計只有一二級警戒
                                        await CheckThresholdAlarm("裂縫計", "毫米", false);
                                    }
                                    // 判斷傾斜計警戒（只判斷一級，且馬上通報）
                                    else if (sensorType == "Slope")
                                    {
                                        int newThresholdAlarm = 0;
                                        string alertLevel = "";
                                        if (threshold1 != double.MaxValue && Math.Abs(LastValue1) >= threshold1)
                                        {
                                            newThresholdAlarm = 1;
                                            alertLevel = "一級";
                                        }
                                        // 只要一級警戒有變化就通報
                                        if (newThresholdAlarm != thresholdAlarm)
                                        {
                                            bool shouldNotify = (thresholdAlarm == 0 && newThresholdAlarm == 1) ||
                                                               (thresholdAlarm > 1 && newThresholdAlarm == 1); // 只要進入一級
                                            if (shouldNotify)
                                            {
                                                string channelToken = _configuration["Line:channelAccessToken"];
                                                string groupId = _configuration["Line:groupId"];
                                                string message = $"⚠️傾斜計{alertLevel}警戒通報\n" +
                                                               $"站點名稱：{areaName} {sensorNameA}\n" +
                                                               $"目前數值：{LastValue1:F2}度";
                                                var notify = new NotifyService(channelToken, _configuration);
                                                if (lat.HasValue && lng.HasValue)
                                                    await notify.PushMapAlertAsync(groupId, lat.Value.ToString(), lng.Value.ToString(), message);
                                                else
                                                    await notify.PushTextAsync(groupId, message);
                                            }
                                            thresholdAlarm = newThresholdAlarm;
                                            needUpdateAlarm = true;
                                        }
                                    }
                            }

                            // 更新資料庫
                            if (needUpdateDisc || needUpdateAlarm)
                            {
                                string updateSql = @"
                                    UPDATE Sensors
                                    SET isDisc = @isDisc, isAlarm = @isAlarm, ThresholdAlarm = @ThresholdAlarm
                                    WHERE SensorID = @SensorID";

                                sqlHelper.ExecuteNonQuery(updateSql, new SqlParameter[]
                                {
                                    new SqlParameter("@isDisc", isDisc),
                                    new SqlParameter("@isAlarm", isAlarm),
                                    new SqlParameter("@ThresholdAlarm", thresholdAlarm),
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
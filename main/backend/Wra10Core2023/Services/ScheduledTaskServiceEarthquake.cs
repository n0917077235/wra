using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Data;
using Microsoft.Data.SqlClient;

using Wra10Core2023.Models;
using EarthquakeInfoApi.Models;

using Notify.Services;
using Earthquake.Services;
using System.Runtime.Versioning;

using SqlHelper = Wra10Core2023.Util.SQLHelper;
using NCrontab;

using NCrontab;

namespace ScheduledTask.Services
{
    [SupportedOSPlatform("windows")]
    public class ScheduledTaskServiceEarthquake : BackgroundService
    {
        private readonly ILogger<ScheduledTaskServiceEarthquake> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly CrontabSchedule _schedule;
        private DateTime _nextRun;

        public ScheduledTaskServiceEarthquake(IWebHostEnvironment env, ILogger<ScheduledTaskServiceEarthquake> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _env = env;

            // cron 表達式，* * * * * 表示每分鐘
            _schedule = CrontabSchedule.Parse("* * * * *"); 
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Earthquake定時任務已啟動");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                if (now >= _nextRun)
                {
                    try
                    {
                        Console.WriteLine($"Earthquake任務執行時間: {now}");
                        var checker = new EarthquakeService(_env, _configuration);
                        var today = DateTime.Now;
                        var list = checker.GetEQEventRangeIntensity(today.Year, today.Month, 3);
                        var latest = "";
                        if (list.Count > 0 && list[0] is object[] firstRow && firstRow.Length > 0)
                        {
                            latest = firstRow[0]?.ToString() ?? "";
                        }

                        var EQMainlist = checker.GetEQEvents("-1", latest);
                        int count = 0;

                        foreach (var item in EQMainlist)
                        {
                            if (float.TryParse(item.intensity, out float value) && value >= 3)
                                count++;
                        }

                        if (count >= 3)
                        {
                            var channelToken = _configuration["Line:channelAccessToken"];
                            var groupId = _configuration["Line:GroupId"];
                            var filename = checker.GenerateCombinedImage(EQMainlist);
                            string picurl = $"https://testdotnet6.softrend.com.tw/output/{filename}";
                            Console.WriteLine($"圖片網址: {picurl}");
                            var message = "⚠️警戒通報\n地震發生!!地震儀總覽如下圖";

                            var notify = new NotifyService(channelToken, _configuration);
                            await notify.PushImageAsync(groupId, message, picurl);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Earthquake任務執行出錯: {ex.Message}");
                    }

                    // 計算下一次執行時間
                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }

                // 每秒檢查一次是否到達排程時間
                await Task.Delay(1000, stoppingToken);
            }

            Console.WriteLine("Earthquake定時任務已停止");
        }
    }
}

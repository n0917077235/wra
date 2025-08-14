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

namespace ScheduledTask.Services
{

    [SupportedOSPlatform("windows")]
    public class ScheduledTaskServiceEarthquake : BackgroundService
    {
        private readonly ILogger<ScheduledTaskServiceEarthquake> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // 每x分鐘執行一次
        private readonly IWebHostEnvironment _env;

        public ScheduledTaskServiceEarthquake(IWebHostEnvironment env,ILogger<ScheduledTaskServiceEarthquake> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _env = env;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("定時任務已啟動");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"任務執行時間: {DateTime.Now}");
                    var checker = new EarthquakeService(_env,_configuration);
                    var list = checker.GetEQEventRangeIntensity(2025, 6, 3);
                    var latest = "";
                    if (list.Count > 0 && list[0] is object[] firstRow && firstRow.Length > 0)
                    {
                        latest = firstRow[0] != null ? firstRow[0].ToString() : "";
                        Console.WriteLine($"最新時間: {latest}");
                    }

                    var EQMainlist = checker.GetEQEvents("-1", latest);
                    int count = 0;

                    foreach (var item in EQMainlist)
                    {
                        if (float.TryParse(item.intensity, out float value))
                        {
                            Console.WriteLine($"感測器：{item.sensorName}, intensity: {value}");
                            if (value >= 3)
                                count++;
                        }
                    }

                    // 如果達到 3 筆以上符合條件，就觸發
                    if (count >= 1)
                    {
                        var channelToken = _configuration["Line:channelAccessToken"];
                        var groupId = _configuration["Line:GroupId"];
                        var filename = checker.GenerateCombinedImage(EQMainlist);
                        string picurl = $"https://testdotnet6.softrend.com.tw/output/{filename}";
                        Console.WriteLine($"圖片網址: {picurl}");
                        var message = "⚠️警戒通報\\n這是發給群組測試通報訊息";

                        var lineService = new NotifyService(channelToken);
                        await lineService.PushMessageWithImageToGroupAsync(groupId, message, picurl);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"任務執行出錯: {ex.Message}");
                }

                // 等待一段時間再執行下一輪
                await Task.Delay(_interval, stoppingToken);
            }

            Console.WriteLine("定時任務已停止");
        }

        
    }
}

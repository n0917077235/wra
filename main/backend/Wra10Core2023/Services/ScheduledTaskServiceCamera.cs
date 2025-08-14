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
using Notify.Services;

namespace ScheduledTask.Services
{
    public class ScheduledTaskServiceCamera : BackgroundService
    {
        private readonly ILogger<ScheduledTaskServiceCamera> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(10); // 每x分鐘執行一次
        private readonly IWebHostEnvironment _env;
        private static readonly HttpClient _httpClient = new();


        string? videoImagePath="";
        bool bNetDrive = false;
        string login = "";
        string password = "";
        string driveLetter = "Q:";

        public ScheduledTaskServiceCamera(IWebHostEnvironment env,ILogger<ScheduledTaskServiceCamera> logger,IConfiguration configuration)
        {
            _env = env;
            _logger = logger;
            _configuration = configuration;

            videoImagePath = _configuration["VideoImage:Path"];
            bool.TryParse(_configuration["VideoImage:NetDrive"], out bNetDrive);
            login = _configuration["VideoImage:Login"];
            password = _configuration["VideoImage:Password"];
            driveLetter = _configuration["VideoImage:DriveLetter"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("定時任務已啟動");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"任務執行時間: {DateTime.Now}");

                    DataTable dt;
                    List<CameraSearch> lstCamera = new List<CameraSearch>();
                    string? conn = _configuration.GetConnectionString("WaterToANCAD");

                    if (conn != null)
                    {
                        // if (bNetDrive)
                        // {
                        //     NetworkDriveAccess networkDriveAccess = new NetworkDriveAccess();
                        //     networkDriveAccess.AccessNetworkDrive(driveLetter, videoImagePath, login, password);
                        // }
                        SqlHelper sqlHelper = new SqlHelper(conn);                                                                                                        
                        List<SqlParameter> lstParams = new List<SqlParameter>();
                        lstParams.Add(new SqlParameter("@areaId", DBNull.Value));
                        lstParams.Add(new SqlParameter("@StationId", DBNull.Value));
                        lstParams.Add(new SqlParameter("@keyword", DBNull.Value));
                        dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetCameraBySearch", lstParams.ToArray());
                        foreach (DataRow row in dt.Rows)
                        {
                            CameraSearch camera;
                            string camName = Convert.ToString(row["StreamMain"]) ?? string.Empty;

                            camera = new CameraSearch
                            {
                                StreamMain = camName,
                                CamName = Convert.ToString(row["CamName"]) ?? string.Empty
                            };
                            lstCamera.Add(camera);
                        }
                    }

                    Console.WriteLine($"攝影機數量：{lstCamera.Count}");
                    foreach (var cam in lstCamera.Take(20))
                    {   
                        string imageUrl = $"https://rivermonitoring.wra10.gov.tw/fx/videoimages/{cam.StreamMain}";
                        //Console.WriteLine($"攝影機名稱：{cam.CamName},imageUrl:{imageUrl}");
                        DateTimeOffset? lastModified = await GetLastModifiedAsync(imageUrl);

                        if (lastModified.HasValue)
                        {
                            DateTimeOffset t = lastModified.Value.ToLocalTime();

                            //Console.WriteLine($"圖片最後修改時間（本機時區）：{t:yyyy'-'MM'-'dd HH:mm:ss}");

                            TimeSpan diff = DateTimeOffset.Now - t;
                            if (diff.TotalMinutes > 30)
                            {
                                var channelToken = _configuration["Line:channelAccessToken"];;
                                var groupId = _configuration["Line:groupId"];
                                var message = $"🔍❌缺測通報\\n站點名稱：{cam.CamName}\r\n監視器超過30分鐘沒有新資料";
                                Console.WriteLine(message); // 超過 30 分鐘

                                var lineService = new NotifyService(channelToken);
                                await lineService.PushMessageToGroupAsync(groupId, message);
                            }
                        }

                        // 等待 500 毫秒，避免過度請求對方伺服器
                        await Task.Delay(500);
                    }
                    Console.WriteLine("定時任務已完成一次");
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

        public static async Task<DateTimeOffset?> GetLastModifiedAsync(string imageUrl)
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, imageUrl);
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;  // 圖片不存在或無法取得

            if (response.Content.Headers.LastModified.HasValue)
                return response.Content.Headers.LastModified.Value;  // 取得時間

            // 某些伺服器可能放在 Raw Headers 中，可以手動讀取
            if (response.Headers.TryGetValues("Last-Modified", out var values))
            {
                if (DateTimeOffset.TryParseExact(values.First(),
                        "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    return dt;
                }
            }

            return null; // 找不到該欄位
        }

    }

    public class NetworkDriveAccess
    {
        public void AccessNetworkDrive(string driveLetter, string networkPath, string username, string password)
        {
            NetworkCredential networkCredential = new NetworkCredential(username, password);

            using (new NetworkConnection(networkPath, networkCredential))
            {
                string fullPath = Path.Combine(driveLetter, "Path", "To", "Your", "File.txt");
                if (File.Exists(fullPath))
                {
                    string fileContent = File.ReadAllText(fullPath);
                    Console.WriteLine($"File Content: {fileContent}");
                }
                else
                {
                    Console.WriteLine("File does not exist.");
                }
            }
        }

        public class NetworkConnection : IDisposable
        {
            private string _networkName;

            public NetworkConnection(string networkPath, NetworkCredential credentials)
            {
                _networkName = networkPath;

                var netResource = new NetResource
                {
                    Scope = ResourceScope.GlobalNetwork,
                    RemoteName = networkPath
                };

                int result = WNetAddConnection2(netResource, credentials.Password, credentials.UserName, 0);

                if (result != 0)
                {
                    throw new System.ComponentModel.Win32Exception(result);
                }
            }

            public void Dispose()
            {
                WNetCancelConnection2(_networkName, 0, true);
            }

            [System.Runtime.InteropServices.DllImport("mpr.dll")]
            private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

            [System.Runtime.InteropServices.DllImport("mpr.dll")]
            private static extern int WNetCancelConnection2(string name, int flags, bool force);
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public class NetResource
        {
            public ResourceScope Scope;
            public ResourceType ResourceType;
            public ResourceDisplaytype DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        public enum ResourceScope
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        }

        public enum ResourceType
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8,
        }

        public enum ResourceDisplaytype
        {
            Generic = 0x0,
            Domain = 0x01,
            Server = 0x02,
            Share = 0x03,
            File = 0x04,
            Group = 0x05,
            Network = 0x06,
            Root = 0x07,
            Shareadmin = 0x08,
            Directory = 0x09,
            Tree = 0x0a,
            Ndscontainer = 0x0b
        }
    }
}

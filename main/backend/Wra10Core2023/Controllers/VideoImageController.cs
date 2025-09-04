using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Wra10Core2023.Models;
using SqlHelper = Wra10Core2023.Util.SQLHelper;
using System;
using System.IO;
using System.Net;


namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors]
    public class VideoImageController : Controller
    {
        private readonly IConfiguration _configuration;
        string? videoImagePath = "";
        string? conn = "";
        private readonly IWebHostEnvironment _hostingEnvironment;
        private string? virtualImagePath = "";
        bool bNetDrive = false;
        string login = "";
        string password = "";
        string driveLetter = "Q:";
        public VideoImageController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            videoImagePath = _configuration["VideoImage:Path"];
            conn = _configuration.GetConnectionString("Water2022");
            virtualImagePath = _configuration["VirtualVideoImage:Path"];
            bool.TryParse(_configuration["VideoImage:NetDrive"], out bNetDrive);
            login = _configuration["VideoImage:Login"];
            password = _configuration["VideoImage:Password"];
            driveLetter = _configuration["VideoImage:DriveLetter"];

        }
        [HttpGet]
        [Authorize]
        [Route("GetCameraAll")]
        public IActionResult GetCameraAll()
        {
            DataTable dt;
            List<Camera> lstCamera = new List<Camera>();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                dt = sqlHelper.ExecuteQuery("sp_GetCameraAll");
                foreach (DataRow row in dt.Rows)
                {
                    decimal x = (decimal)0.0, y = (decimal)0.0;
                    decimal.TryParse(row["X"].ToString(), out x);
                    decimal.TryParse(row["Y"].ToString(), out y);
                    int sx = 0, sy = 0;
                    int.TryParse(row["ScreenX"].ToString(), out sx);
                    int.TryParse(row["ScreenY"].ToString(), out sy);
                    Camera camera = new Camera
                    {
                        CamID = row["CamID"].ToString().Replace("\r", "").Replace("\n", ""),
                        CamName = row["CamName"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaId = row["AreaId"].ToString(),
                        AreaName = row["AreaName"].ToString(),
                        StationID = row["StationID"].ToString(),
                        StationNameA = row["StationNameA"].ToString(),
                        X = x,
                        Y = y,
                        StreamMain = row["StreamMain"].ToString(),
                        ScreenX = sx,
                        ScreenY = sy,
                    };
                    lstCamera.Add(camera);
                }
            }
            return Ok(lstCamera.ToList());
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);


        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("GetCameraByAreaIdSimple")]
        public IActionResult GetCameraByAreaIdSimlpe(string? areaId)
        {
            DataTable dt;
            List<CameraSimple> lstCamera = new List<CameraSimple>();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                SqlHelper sqlHelper = new SqlHelper(conn);

                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@areaId", String.IsNullOrEmpty(areaId) ? DBNull.Value : areaId));
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetCameraByAreaId", lstParams.ToArray());

                foreach (DataRow row in dt.Rows)
                {
                    decimal x = (decimal)0.0, y = (decimal)0.0;
                    decimal.TryParse(row["X"].ToString(), out x);
                    decimal.TryParse(row["Y"].ToString(), out y);
                    var rootUrl = RowToImageUrl(row);

                    CameraSimple camera = new CameraSimple
                    {
                        StationID = row["StationID"].ToString(),
                        StationNameA = row["StationNameA"].ToString(),
                        CamName = row["CamName"].ToString(),
                        X = x,
                        Y = y,
                        StreamMain = rootUrl,

                    };
                    lstCamera.Add(camera);
                }
            }
            return Ok(lstCamera.ToList());
        }

        private string RowToImageUrl(DataRow row)
        {
            return SensorController.GetCctvImageUrlByFile(_configuration,
                Request, row["StreamMain"].ToString());
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("GetCameraBySearch")]
        public IActionResult GetCameraBySearch(string? areaId, string? stationId, bool? isAlarm, string? keyword)
        {
            DataTable dt;
            List<CameraSearch> lstCamera = new List<CameraSearch>();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                if (bNetDrive)
                {
                    NetworkDriveAccess networkDriveAccess = new NetworkDriveAccess();



                    networkDriveAccess.AccessNetworkDrive(driveLetter, videoImagePath, login, password);
                }
                SqlHelper sqlHelper = new SqlHelper(conn);

                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@areaId", String.IsNullOrEmpty(areaId) ? DBNull.Value : areaId));
                lstParams.Add(new SqlParameter("@StationId", String.IsNullOrEmpty(stationId) ? DBNull.Value : stationId));
                lstParams.Add(new SqlParameter("@keyword", String.IsNullOrEmpty(keyword) ? DBNull.Value : keyword));
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetCameraBySearch", lstParams.ToArray());
                foreach (DataRow row in dt.Rows)
                {
                    // Get the root URL
                    string webRootPath = _hostingEnvironment.ContentRootPath;
                    CameraSearch camera = null;
                    string camName = row["StreamMain"].ToString();
                    string imagePath = Path.Combine(webRootPath, videoImagePath) + @"\" + camName;
                    bool bFile = false;
                    //sqlHelper.ExecuteNonQuery("Insert into Logs (message) values ('" + imagePath + "')");
                    string fileTime = new FileInfo(imagePath).LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss)");
                    bFile = System.IO.File.Exists(videoImagePath + @"\" + camName);
                    int nAlarm = -1;
                    var rootUrl = RowToImageUrl(row);
                    FileContentResult fcr = null;
                    int alarmMins = 30;
                    int.TryParse(_configuration["VideoImage:Alarm"], out alarmMins);
                    if (bFile)
                    {
                        //sqlHelper.ExecuteNonQuery("Insert into Logs (message) values ('" + fileTime + "')");

                        if ((DateTime.Now - new FileInfo(imagePath).LastWriteTime).TotalMinutes >= alarmMins)
                            nAlarm = 1;
                        else
                            nAlarm = 0;

                        //byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

                        //string contentType = GetImageContentType(camName);

                        //fcr=File(imageBytes, contentType);
                    }
                    if (isAlarm == null)
                    {
                        camera = new CameraSearch
                        {
                            StationNameA = row["StationNameA"].ToString(),
                            StreamMain = rootUrl.ToString(),
                            isAlarm = nAlarm,
                            CamName = row["CamName"].ToString()
                        };
                        lstCamera.Add(camera);
                    }
                    else if ((bool)isAlarm)
                    {
                        if (nAlarm == 1 || nAlarm == -1)
                        {
                            camera = new CameraSearch
                            {
                                StationNameA = row["StationNameA"].ToString(),
                                StreamMain = rootUrl.ToString(),
                                isAlarm = nAlarm,
                                CamName = row["CamName"].ToString()
                            };
                            lstCamera.Add(camera);
                        }
                    }
                    else
                    {
                        if (nAlarm == 0)
                        {
                            camera = new CameraSearch
                            {
                                StationNameA = row["StationNameA"].ToString(),
                                StreamMain = rootUrl.ToString(),
                                isAlarm = nAlarm,
                                CamName = row["CamName"].ToString()
                            };
                            lstCamera.Add(camera);
                        }
                    }


                }
            }
            return Ok(lstCamera.ToList());
        }

        [HttpPost]
        [Authorize]
        [Route("GetCameraByAreaId")]
        public IActionResult GetCameraByAreaId(string? areaId)
        {
            DataTable dt;
            List<Camera> lstCamera = new List<Camera>();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                SqlHelper sqlHelper = new SqlHelper(conn);

                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@areaId", String.IsNullOrEmpty(areaId) ? DBNull.Value : areaId));
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetCameraByAreaId", lstParams.ToArray());
                foreach (DataRow row in dt.Rows)
                {
                    decimal x = (decimal)0.0, y = (decimal)0.0;
                    decimal.TryParse(row["X"].ToString(), out x);
                    decimal.TryParse(row["Y"].ToString(), out y);
                    int sx = 0, sy = 0;
                    int.TryParse(row["ScreenX"].ToString(), out sx);
                    int.TryParse(row["ScreenY"].ToString(), out sy);
                    var rootUrl = RowToImageUrl(row);
                    
                    Camera camera = new Camera
                    {
                        CamID = row["CamID"].ToString().Replace("\r", "").Replace("\n", ""),
                        CamName = row["CamName"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaId = row["AreaId"].ToString(),
                        AreaName = row["AreaName"].ToString(),
                        StationID = row["StationID"].ToString(),
                        StationNameA = row["StationNameA"].ToString(),
                        X = x,
                        Y = y,
                        StreamMain = rootUrl.ToString(),
                        ScreenX = sx,
                        ScreenY = sy,
                    };
                    lstCamera.Add(camera);
                }
            }
            return Ok(lstCamera.ToList());
        }

        [HttpPost]
        [Authorize]
        [Route("GetCameraByAreaName")]
        public IActionResult GetCameraByAreaName(string? areaName)
        {
            DataTable dt;
            List<Camera> lstCamera = new List<Camera>();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@areaName", String.IsNullOrEmpty(areaName) ? DBNull.Value : areaName));
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetCameraByAreaName", lstParams.ToArray());
                foreach (DataRow row in dt.Rows)
                {
                    decimal x = (decimal)0.0, y = (decimal)0.0;
                    decimal.TryParse(row["X"].ToString(), out x);
                    decimal.TryParse(row["Y"].ToString(), out y);
                    int sx = 0, sy = 0;
                    int.TryParse(row["ScreenX"].ToString(), out sx);
                    int.TryParse(row["ScreenY"].ToString(), out sy);
                    Camera camera = new Camera
                    {
                        CamID = row["CamID"].ToString().Replace("\r", "").Replace("\n", ""),
                        CamName = row["CamName"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaId = row["AreaId"].ToString(),
                        AreaName = row["AreaName"].ToString(),
                        StationID = row["StationID"].ToString(),
                        StationNameA = row["StationNameA"].ToString(),
                        X = x,
                        Y = y,
                        StreamMain = row["StreamMain"].ToString(),
                        ScreenX = sx,
                        ScreenY = sy,
                    };
                    lstCamera.Add(camera);
                }
            }
            return Ok(lstCamera.ToList());
        }

        [HttpPost]
        [Authorize]
        [Route("GetCameraByStationId")]
        public IActionResult GetCameraByStationId(string? stationId)
        {
            DataTable dt;
            List<Camera> lstCamera = new List<Camera>();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@stationId", String.IsNullOrEmpty(stationId) ? DBNull.Value : stationId));
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetCameraByStationId", lstParams.ToArray());
                foreach (DataRow row in dt.Rows)
                {
                    decimal x = (decimal)0.0, y = (decimal)0.0;
                    decimal.TryParse(row["X"].ToString(), out x);
                    decimal.TryParse(row["Y"].ToString(), out y);
                    int sx = 0, sy = 0;
                    int.TryParse(row["ScreenX"].ToString(), out sx);
                    int.TryParse(row["ScreenY"].ToString(), out sy);
                    Camera camera = new Camera
                    {
                        CamID = row["CamID"].ToString().Replace("\r", "").Replace("\n", ""),
                        CamName = row["CamName"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaId = row["AreaId"].ToString(),
                        AreaName = row["AreaName"].ToString(),
                        StationID = row["StationID"].ToString(),
                        StationNameA = row["StationNameA"].ToString(),
                        X = x,
                        Y = y,
                        StreamMain = row["StreamMain"].ToString(),
                        ScreenX = sx,
                        ScreenY = sy,
                    };
                    lstCamera.Add(camera);
                }
            }
            return Ok(lstCamera.ToList());
        }

        [HttpPost]
        [Authorize]
        [Route("GetCameraByStationName")]
        public IActionResult GetCameraByStationName(string? stationName)
        {
            DataTable dt;
            List<Camera> lstCamera = new List<Camera>();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@stationName", String.IsNullOrEmpty(stationName) ? DBNull.Value : stationName));
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetCameraByStationName", lstParams.ToArray());
                foreach (DataRow row in dt.Rows)
                {
                    decimal x = (decimal)0.0, y = (decimal)0.0;
                    decimal.TryParse(row["X"].ToString(), out x);
                    decimal.TryParse(row["Y"].ToString(), out y);
                    int sx = 0, sy = 0;
                    int.TryParse(row["ScreenX"].ToString(), out sx);
                    int.TryParse(row["ScreenY"].ToString(), out sy);
                    Camera camera = new Camera
                    {
                        CamID = row["CamID"].ToString().Replace("\r", "").Replace("\n", ""),
                        CamName = row["CamName"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaId = row["AreaId"].ToString(),
                        AreaName = row["AreaName"].ToString(),
                        StationID = row["StationID"].ToString(),
                        StationNameA = row["StationNameA"].ToString(),
                        X = x,
                        Y = y,
                        StreamMain = row["StreamMain"].ToString(),
                        ScreenX = sx,
                        ScreenY = sy,
                    };
                    lstCamera.Add(camera);
                }
            }
            return Ok(lstCamera.ToList());
        }


        [HttpPost]
        [Authorize]
        [Route("GetVideoImage")]
        public IActionResult GetVideoImage(string CamName)
        {
            string webRootPath = _hostingEnvironment.ContentRootPath;

            //string imagePath = Path.Combine(videoImagePath, CamName);// +@"\snapshot.jpg";
            string imagePath = Path.Combine(webRootPath, videoImagePath) + @"/" + CamName;
            if (bNetDrive)
            {
                NetworkDriveAccess networkDriveAccess = new NetworkDriveAccess();



                networkDriveAccess.AccessNetworkDrive(driveLetter, videoImagePath, login, password);
            }
            if (!System.IO.File.Exists(videoImagePath + @"/" + CamName))
            {
                return NotFound(imagePath);//BadRequest(imagePath);
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

            string contentType = GetImageContentType(CamName);

            return File(imageBytes, contentType);
        }

        private string GetImageContentType(string imageName)
        {
            if (imageName.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            {
                return "image/png";
            }
            else if (imageName.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) || imageName.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase))
            {
                return "image/jpeg";
            }
            else if (imageName.EndsWith(".gif", System.StringComparison.OrdinalIgnoreCase))
            {
                return "image/gif";
            }
            else
            {
                return "application/octet-stream";
            }
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

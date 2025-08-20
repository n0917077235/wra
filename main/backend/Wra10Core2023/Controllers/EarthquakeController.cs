using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting.Internal;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Data;
using System.Diagnostics;
using Wra10Core2023.Models;
using SqlHelper = Wra10Core2023.Util.SQLHelper;
using System.IO;
using Docker.DotNet;
using Docker.DotNet.Models;
using Windows.Media.Protection.PlayReady;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Wra10Core2023.Util;

namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EarthquakeController : Controller
    {
        private readonly ILogger<EarthquakeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        string? videoImagePath = "";
        private string? virtualImagePath = "";

        public EarthquakeController(ILogger<EarthquakeController> logger, IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            videoImagePath = _configuration["VideoImage:Path"];
            virtualImagePath = _configuration["VirtualVideoImage:Path"];
        }

        private bool Connect2Docker(string userId,string eventTime)
        {
            StreamWriter file = new StreamWriter(@"D:\ApiDebug\Tcp_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".txt");
            string serverIp = _configuration["IsoseismalMap:ServerIP"];
            int port = 15001;
            int.TryParse(_configuration["IsoseismalMap:ServerPort"], out port);
            bool result = false;
            TcpClient tcpClient = new TcpClient();
            string json = $"{{ \"userId\":\"{userId}\", \"eventTime\":\"{eventTime}\"}}";
            file.WriteLine(json);

            if (tcpClient.ConnectAsync(serverIp, port).Wait(TimeSpan.FromSeconds(5)))
            {
                NetworkStream netStream = tcpClient.GetStream();
                byte[] sendBuffer = Encoding.UTF8.GetBytes(json);
                netStream.Write(sendBuffer);

                file.WriteLine("sent");

                byte[] receiveBuffer = new byte[1024];
                int bytesReceived = netStream.Read(receiveBuffer);
                string data = Encoding.UTF8.GetString(receiveBuffer.AsSpan(0, bytesReceived));
                file.WriteLine("read:" + data);
                if (data.IndexOf("Processed")!=-1)
                {
                    file.WriteLine("true");
                    result = true;
                }
                netStream.Close();
                tcpClient.Close();
            }
            file.WriteLine(result);
            file.Close();
            return result;
        }

        [Authorize]
        [HttpPost]
        [Route("Isoseismal")]
        public async Task<IActionResult> GetIsoseismalMap([FromBody] IMapModel iMap)
        {
            IsoseismalMapData mapData = new IsoseismalMapData(false, string.Empty, string.Empty, null);
            if (iMap==null)
            {
                return BadRequest(mapData);
            }
            string userId = HttpContext.GetUserName();
            string eventTime = iMap.eventTime;
            StreamWriter file = new StreamWriter(@"D:\ApiDebug\EQ_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".txt");
            IsoseismalMap im = new IsoseismalMap();
            
            
            try
            {
                
                string webRootPath = _hostingEnvironment.ContentRootPath;
                string outputPath = _configuration["IsoseismalMap:outputPath"];
                string outputTextFile = outputPath + "\\" + userId + ".txt";
                string outputJsonFile = outputPath + "\\" + userId + ".geojson";
                string srcImage = Path.Combine(outputPath, userId + "_twd97.png");
                string imagePath = _configuration["IsoseismalMap:ImagePath"];
                string dstImage = Path.Combine(webRootPath, imagePath) + @"\" + userId + "_twd97.png";
                
                if (!String.IsNullOrEmpty(eventTime))
                {
                    try
                    {
                        bool result = Connect2Docker(userId, eventTime);
                        if (!result)
                        {
                            file.WriteLine("Connect to docker service failied");
                            return BadRequest(mapData);
                        }
                    }
                    catch (Exception ex)
                    {
                        file.WriteLine("ex:" + ex.Message);
                        return BadRequest(mapData);
                    }
                }
                file.WriteLine(srcImage);
                file.WriteLine(outputTextFile);
                file.WriteLine(outputJsonFile); 
                file.Flush();
                if (System.IO.File.Exists(srcImage) && System.IO.File.Exists(outputJsonFile) && System.IO.File.Exists(outputTextFile))
                {


                    StreamReader sr1 = new StreamReader(outputJsonFile);
                    string geoJson = sr1.ReadToEnd();
                    sr1.Close();

                    StreamReader sr2 = new StreamReader(outputTextFile);
                    string line = null;
                    List<object> lstInfo = new List<object>();
                    while (!String.IsNullOrEmpty((line = sr2.ReadLine())))
                    {
                        lstInfo.Add(line);
                    }
                    sr2.Close();

                    var imageUrl = new Uri($"{Request.Scheme}://{Request.Host}/" + virtualImagePath + @"/IMap/" + userId + "_twd97.png");
                    file.WriteLine("imgUrl:" + imageUrl);
                    file.Close();
                    mapData.Result = true;
                    mapData.InfoList = lstInfo;
                    mapData.ImageUrl = imageUrl.ToString();
                    mapData.GeoJson = geoJson;
                    return Ok(mapData);
                }
                else
                {
                    return BadRequest(mapData);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(mapData);
            }
            finally
            {
                file.Close();
            }
        }

        public class IMapModel
        {
            public string eventTime { get; set; }
        }

        [Authorize]
        [HttpPost]
        [Route("GetEQEventNew")]
        public IActionResult GetEQEvent(int range, string eventTime, float intensity = -1)
        {
            try
            {
                List<EQMain> lstEvent = new List<EQMain>();
                string? conn = _configuration.GetConnectionString("Water2022");
                List<SqlParameter> lstParam = new List<SqlParameter>();
                lstParam.Add(new SqlParameter("@range", range));
                lstParam.Add(new SqlParameter("@eventtime", eventTime));
                lstParam.Add(new SqlParameter("@intensity", intensity));
                if (conn != null)
                {
                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteStoreProcedureQuery("sp_eqeventNew", lstParam.ToArray());
                    EQMain em = new EQMain();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        em = new EQMain()
                        {
                            sensorId = dt.Rows[i]["sensorId"].ToString(),
                            sensorName = dt.Rows[i]["sensorNameA"].ToString(),
                            recordTime = dt.Rows[i]["recordtime"].ToString(),
                            intensity = dt.Rows[i]["intensity"].ToString(),
                            grade = dt.Rows[i]["grade"].ToString(),
                            pga = dt.Rows[i]["pga"].ToString(),
                            pgv = dt.Rows[i]["pgv"].ToString(),
                            eventGroup = dt.Rows[i]["groupTime"].ToString(),
                            eventTag = dt.Rows[i]["eventTag"].ToString(),
                            areaName = dt.Rows[i]["areaName"].ToString()
                        };
                        lstEvent.Add(em);
                    }
                }
                return Ok(lstEvent.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetEQEventRange")]
        public IActionResult GetEQEventRange(int year, int month)
        {
            try
            {
                List<object> lstEventRange = new List<object>();
                string? conn = _configuration.GetConnectionString("Water2022");
                List<SqlParameter> lstParam = new List<SqlParameter>();
                lstParam.Add(new SqlParameter("@year", year));
                lstParam.Add(new SqlParameter("@month", month));
                if (conn != null)
                {
                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteStoreProcedureQuery("sp_getEQMapTime", lstParam.ToArray());
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        object obj = new object[] { dt.Rows[i][0].ToString(), int.Parse(dt.Rows[i][1].ToString()) };
                        lstEventRange.Add(obj);
                    }
                }
                return Ok(lstEventRange.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetEQEventRangeIntensity")]
        public IActionResult GetEQEventRangeIntensity(int year, int month, float intensity)
        {
            try
            {
                List<object> lstEventRange = new List<object>();
                string? conn = _configuration.GetConnectionString("Water2022");
                List<SqlParameter> lstParam = new List<SqlParameter>();
                lstParam.Add(new SqlParameter("@year", year));
                lstParam.Add(new SqlParameter("@month", month));
                lstParam.Add(new SqlParameter("@intensity", intensity));
                if (conn != null)
                {
                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteStoreProcedureQuery("sp_getEQMapTimeNew", lstParam.ToArray());
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        object obj = new object[] { dt.Rows[i][0].ToString(), int.Parse(dt.Rows[i][1].ToString()) };
                        lstEventRange.Add(obj);
                    }
                }
                return Ok(lstEventRange.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetEQEventRangeOld")]
        public IActionResult GetEQEventRangeWithOld(int year, int month)
        {
            try
            {
                List<string> lstEventRange = new List<string>();
                string? conn = _configuration.GetConnectionString("Water2022");
                List<SqlParameter> lstParam = new List<SqlParameter>();
                lstParam.Add(new SqlParameter("@year", year));
                lstParam.Add(new SqlParameter("@month", month));
                if (conn != null)
                {
                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteStoreProcedureQuery("sp_eqeventrange", lstParam.ToArray());
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        lstEventRange.Add(dt.Rows[i][0].ToString());
                    }
                }
                return Ok(lstEventRange.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetEQEvent")]
        public IActionResult GetEQEvent(int range,string eventTime)
        {
            try
            {
                List<EQMain> lstEvent = new List<EQMain>();
                string? conn = _configuration.GetConnectionString("Water2022");
                List<SqlParameter> lstParam = new List<SqlParameter>();
                lstParam.Add(new SqlParameter("@range", range));
                lstParam.Add(new SqlParameter("@eventtime", eventTime));
                if (conn != null)
                {
                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteStoreProcedureQuery("sp_eqevent", lstParam.ToArray());
                    EQMain em = new EQMain();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        em = new EQMain()
                        {
                            sensorId = dt.Rows[i]["sensorId"].ToString(),
                            sensorName = dt.Rows[i]["sensorNameA"].ToString(),
                            recordTime = dt.Rows[i]["recordtime"].ToString(),
                            intensity = dt.Rows[i]["intensity"].ToString(),
                            grade = dt.Rows[i]["grade"].ToString(),
                            pga= dt.Rows[i]["pga"].ToString(),
                            pgv = dt.Rows[i]["pgv"].ToString(),
                            eventGroup= dt.Rows[i]["groupTime"].ToString(),
                            eventTag = dt.Rows[i]["eventTag"].ToString(),
                            areaName = dt.Rows[i]["areaName"].ToString()
                        };
                        lstEvent.Add(em);
                    }
                }
                return Ok(lstEvent.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetEQEventDetail")]
        public IActionResult GetEQEventDetail(string sensorId, string eventTag)
        {
            try
            {
                EQData eqData = new EQData();
                string? conn = _configuration.GetConnectionString("Water2022");
                List<SqlParameter> lstParam = new List<SqlParameter>();
                lstParam.Add(new SqlParameter("@sensorId", sensorId));
                lstParam.Add(new SqlParameter("@eventtag", eventTag));
                lstParam.Add(new SqlParameter("@cycle", 100));
                if (conn != null)
                {
                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteStoreProcedureQuery("sp_eqchart", lstParam.ToArray());
                    
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            eqData.sensorId = sensorId;
                            eqData.sensorName = dt.Rows[i]["SensorNameA"].ToString();
                            eqData.xLabel = eqData.sensorName + "-X(" + dt.Rows[i]["XAxis"].ToString() + ")";
                            eqData.yLabel = eqData.sensorName + "-Y(" + dt.Rows[i]["YAxis"].ToString() + ")";
                            eqData.zLabel = eqData.sensorName + "-Z(" + dt.Rows[i]["ZAxis"].ToString() + ")";
                        }
                        eqData.lstRecordTime.Add(dt.Rows[i]["RecordTime"].ToString());
                        eqData.lstX.Add(double.Parse(dt.Rows[i]["X"].ToString()));
                        eqData.lstY.Add(double.Parse(dt.Rows[i]["Y"].ToString()));
                        eqData.lstZ.Add(double.Parse(dt.Rows[i]["Z"].ToString()));
                    }
                }
                return Ok(eqData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Data;
using Wra10Core2023.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using SqlHelper = Wra10Core2023.Util.SQLHelper;

namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RadarController : ControllerBase
    {
        private readonly ILogger<RadarController> _logger;
        private readonly IConfiguration _configuration;
        public RadarController(ILogger<RadarController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet]
        [Route("GetRadarHeader")]
        public IActionResult GetRadarHeader()
        {
            try
            {
                DataTable dt;
                List<RadarHeaderName> lstRadarHeaderName = new List<RadarHeaderName>();
                string? conn = _configuration.GetConnectionString("Water2022");
                if (conn != null)
                {
                    SqlHelper sqlHelper = new SqlHelper(conn);

                    dt = sqlHelper.ExecuteQuery(@"select distinct headerName from [buildCheck].[dbo].[RadarMain]");

                    foreach (DataRow row in dt.Rows)
                    {
                        RadarHeaderName rhn = new RadarHeaderName
                        {
                            HeaderName = row["headerName"]?.ToString()?.Replace("\r", "").Replace("\n", "")
                        };
                        lstRadarHeaderName.Add(rhn);
                    }
                }
                return Ok(lstRadarHeaderName.ToList());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost]
        [Route("GetCase")]
        public IActionResult GetCase(string? header)
        {
            try
            {
                List<RadarMain> lstCase = new List<RadarMain>();
                DataTable dt = null;
                string filter = " 1=1 ";
                filter += " and a.HeaderName='" + header + "'";
                string cmd = @"SELECT a.RadarId, a.Title 
                             FROM [buildCheck].[dbo].[radarMain] a
                             WHERE (1=1) ";
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHeper = new SqlHelper(conn);
                List<SqlParameter> lstParameter = new List<SqlParameter>();
                if (!string.IsNullOrEmpty(header))
                {
                    cmd += " AND (a.HeaderName = @HeaderName)";
                    lstParameter.Add(new SqlParameter("@HeaderName", header));
                    dt = sqlHeper.ExecuteQuery(cmd, lstParameter.ToArray());
                }
                else
                {
                    dt = sqlHeper.ExecuteQuery(cmd);
                }
                
                    
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    RadarMain rm = new RadarMain();
                    rm.id = dt.Rows[i][0].ToString();
                    rm.fileName = dt.Rows[i][1].ToString();
                    lstCase.Add(rm);
                }
                return Ok(lstCase.ToList());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetImage")]
        public IActionResult GetImage(string radarId)
        {
            try
            {
                string json = "";
                List<int> lstImages = new List<int>();
                DataTable dt = null;
                List<SqlParameter> lstParameter = new List<SqlParameter>();
                string cmd = @"select sequence from  [buildCheck].[dbo].[RadarImage]
                        where radarid =@radarId order by Sequence";
                lstParameter.Add(new SqlParameter("@radarId", radarId));
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHeper = new SqlHelper(conn);
                dt = sqlHeper.ExecuteQuery(cmd,lstParameter.ToArray());

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    lstImages.Add(int.Parse(dt.Rows[i][0].ToString()));
                }

                //json = JsonConvert.SerializeObject(lstImages);
                return Ok(lstImages.ToList());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetRadarData")]
        public IActionResult GetRadarData(string radarids, string keyword)
        {
            try
            {
                List<Radar> lstRadar = new List<Radar>();
                DataTable dt = null;
                string filter = " 1=1 ";
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHeper = new SqlHelper(conn);
                string cmd = @"select ROW_NUMBER() OVER (ORDER BY a.radarid ASC) as serialNo, a.*,b.sequence as seq,c.HeaderName,c.Title,c.DocType
                          FROM [BuildCheck].[dbo].[RadarObject] a
                          inner join [BuildCheck].[dbo].RadarImage b on a.radarid=b.Radarid
                          inner join [BuildCheck].[dbo].RadarMain c on b.RadarID=c.RadarID
                          where " + filter ;
                List<SqlParameter> lstParameter = new List<SqlParameter>();
                if (!string.IsNullOrEmpty(radarids))
                {
                    cmd += " and a.radarid in (@radarids) ";
                    lstParameter.Add(new SqlParameter("@radarids", radarids));
                }
                if (!string.IsNullOrEmpty(keyword))
                {
                    cmd += " and c.headername like @keyword";
                    lstParameter.Add(new SqlParameter("@keyword", "%" + keyword + "%"));
                }
                cmd += @" order by a.Radarid,b.Sequence";

                if (lstParameter.Count > 0)
                    dt = sqlHeper.ExecuteQuery(cmd, lstParameter.ToArray());
                else
                    dt = sqlHeper.ExecuteQuery(cmd);
                string preRaderId = "";

                var distinctValues1 = dt.AsEnumerable()
                                    .Select(row => new
                                    {

                                        id = row.Field<int>("radarid"),
                                        title = row.Field<string>("title"),
                                        headerName = row.Field<string>("headername"),
                                        //serialno = row.Field<long>("serialno"),


                                    })
                                    .Distinct().AsQueryable();
                Radar rd = null;
                for (int i = 0; i < distinctValues1.Count(); i++)
                {
                    var row = distinctValues1.ElementAt(i);
                    rd = new Radar();
                    rd.radarId = row.id;
                    rd.title = row.title;
                    rd.serialNo = (i + 1).ToString();
                    rd.headerName = row.headerName;
                    lstRadar.Add(rd);
                    var distinctValues2 = dt.AsEnumerable()
                                    .Select(gps => new
                                    {
                                        id = gps.Field<int>("radarid"),
                                        x = gps.Field<decimal>("gpsx"),
                                        y = gps.Field<decimal>("gpsy"),
                                        distance = gps.Field<string>("distance"),

                                    })
                                    .Where(x => x.id == row.id)
                                    .Distinct().AsQueryable();
                    for (int j = 0; j < distinctValues2.Count(); j++)
                    {
                        RadarGps rg = new RadarGps();
                        var rgData = distinctValues2.ElementAt(j);
                        rg.x = (double)rgData.x;
                        rg.y = (double)rgData.y;
                        rg.distance = rgData.distance;
                        rd.lstGps.Add(rg);
                    }

                    var distinctValues3 = dt.AsEnumerable()
                                    .Select(img => new
                                    {
                                        id = img.Field<int>("radarid"),
                                        seq = img.Field<int>("seq"),

                                    })
                                    .Where(x => x.id == row.id)
                                    .Distinct().AsQueryable();
                    for (int j = 0; j < distinctValues3.Count(); j++)
                    {
                        //distinctValues3.ElementAt(j);
                        rd.lstImg.Add(distinctValues3.ElementAt(j).seq.ToString());
                    }
                }
                return Ok(lstRadar.ToList());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}

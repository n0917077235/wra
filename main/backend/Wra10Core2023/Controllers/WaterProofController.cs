using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Data;
using Wra10Core2023.Models;
using static Wra10Core2023.Models.WaterProofTaget;
using SqlHelper = SQLHelper.SQLHelper;

namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WaterProofController : ControllerBase
    {
        private readonly ILogger<WaterProofController> _logger;
        private readonly IConfiguration _configuration;
        public WaterProofController(ILogger<WaterProofController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet]
        [Route("GetWaterProofTarget")]
        public IActionResult GetWaterProofTarget()
        {
            try
            {
                List<WaterProofTaget> lstWaterProofTarget = new List<WaterProofTaget>();
                string? conn = _configuration.GetConnectionString("BuildCheck");
                if (conn != null)
                {
                    string cmd = @"select distinct target,waterproofid from WaterProofMain";
                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteQuery(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        int id = -1;
                        int.TryParse(row["waterproofid"].ToString(), out id);
                        WaterProofTaget wpf = new WaterProofTaget
                        {
                            target = row["target"]?.ToString()?.Replace("\r", "").Replace("\n", ""),
                            waterproofid = id
                        };
                        lstWaterProofTarget.Add(wpf);
                    }
                }
                return Ok(lstWaterProofTarget.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetWaterProofProjectId")]
        public IActionResult GetWaterProofProjectId()
        {
            try
            {
                List<WaterProofProject> lstWaterProofProject = new List<WaterProofProject>();
                string? conn = _configuration.GetConnectionString("BuildCheck");
                if (conn != null)
                {
                    string cmd = @"SELECT [WaterProofId]
      ,[ProjectId]
      ,[Year]
      ,[ProjectName]
  FROM [BuildCheck].[dbo].[WaterProofImageMain] order by [WaterProofId],[ProjectId]";
                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteQuery(cmd);

                    foreach (DataRow row in dt.Rows)
                    {
                        int id = -1;
                        int.TryParse(row["waterproofid"].ToString(), out id);
                        WaterProofProject wpp = new WaterProofProject
                        {
                            projectId = int.Parse(row["ProjectId"]?.ToString()?.Replace("\r", "").Replace("\n", "")),
                            waterProofId = id,
                            year = row["year"]?.ToString()?.Replace("\r", "").Replace("\n", ""),
                            projectName= row["ProjectName"]?.ToString()?.Replace("\r", "").Replace("\n", "")
                        };
                        lstWaterProofProject.Add(wpp);
                    }
                }
                return Ok(lstWaterProofProject.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetWaterProofCase")]
        public IActionResult GetWaterProofCase(string? header)
        {
            try
            {
                List<WaterProofMain> lstCase = new List<WaterProofMain>();
                DataTable dt = null;
                string? conn = _configuration.GetConnectionString("BuildCheck");
                SqlHelper sqlHeper = new SqlHelper(conn);
                string filter = " 1=1 ";
                string cmd = @"select a.ProjectId,a.ProjectName from WaterProofImageMain a 
                           inner join WaterProofMain b on a.WaterProofID=b.WaterProofId
                           where (1=1) ";
                List<SqlParameter> lstParam = new List<SqlParameter>();

                if (header != "-1")
                {
                    cmd += " and a.WaterProofId=@header";
                    lstParam.Add(new SqlParameter("@header", header));
                    dt = sqlHeper.ExecuteQuery(cmd, lstParam.ToArray());
                }
                else
                {
                    dt = sqlHeper.ExecuteQuery(cmd);
                }

                

                
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    WaterProofMain rm = new WaterProofMain();
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
        [Route("GetWaterProofImage")]
        public IActionResult GetWaterProofImage(string waterProofId, string projectId)
        {
            try
            {
                List<int> lstImages = new List<int>();
                DataTable dt = null;

                string cmd = @"select 
                      sequence from  WaterProofImage 
                      where waterProofid =@waterProofId and projectid=@projectId  order by Sequence";
                List<SqlParameter> lstParam = new List<SqlParameter>();
                lstParam.Add(new SqlParameter("@waterProofId", waterProofId));
                lstParam.Add(new SqlParameter("@projectId", projectId));

                string? conn = _configuration.GetConnectionString("BuildCheck");
                SqlHelper sqlHeper = new SqlHelper(conn);
               
                dt = sqlHeper.ExecuteQuery(cmd, lstParam.ToArray());

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    lstImages.Add(int.Parse(dt.Rows[i][0].ToString()));
                }


                return Ok(lstImages.ToList());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetWaterProofGps")]
        public IActionResult GetWaterProofGps(string waterProofId)
        {
            try
            {
                string json = "";
                string cmd = @"select 
  gpsx,gpsy,gpsname from  WaterProofObject 
  where waterProofid =@waterProofId  order by gpsy,gpsx";

                string? conn = _configuration.GetConnectionString("BuildCheck");
                SqlHelper sqlHeper = new SqlHelper(conn);
                List<SqlParameter> lstParam = new List<SqlParameter>();
                lstParam.Add(new SqlParameter("@waterProofId", waterProofId));
                DataTable dt = sqlHeper.ExecuteQuery(cmd, lstParam.ToArray());
                List<WaterProofGps> lstGps = new List<WaterProofGps>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    WaterProofGps wpg = new WaterProofGps();
                    wpg.x = double.Parse(dt.Rows[i]["gpsx"].ToString());
                    wpg.y = double.Parse(dt.Rows[i]["gpsy"].ToString());
                    wpg.gpsName = dt.Rows[i]["gpsname"].ToString();
                    lstGps.Add(wpg);
                }
                return Ok(lstGps.ToList());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetWaterProofData")]
        public IActionResult GetWaterProofData(string? waterProofId, string? projectId, string? keyword)
        {
            try
            {
                List<WaterProof> lstWP = new List<WaterProof>();
                DataTable dt = null;
                string filter = " 1=1 ";
                string cmd = @"select ROW_NUMBER() OVER (ORDER BY b.waterproofid ASC) as serialNo,b.waterproofid, b.imagefilename,b.sequence as seq,c.target,d.year,d.projectname,d.projectid
                              FROM  WaterProofImage b
                              inner join WaterProofMain c on b.WaterProofID=c.WaterProofID
                              inner join WaterProofImageMain d on d.waterproofid=c.waterproofid and d.projectid=b.projectid
                              where (1=1) ";
                List<SqlParameter> lstParam = new List<SqlParameter>();

                if (waterProofId != "-1")
                {
                    cmd += " and b.waterproofid=@waterProofId";
                    lstParam.Add(new SqlParameter("@waterProofId", waterProofId));
                }
                if (projectId != "-1")
                {
                    cmd += " and d.projectid=@projectId";
                    lstParam.Add(new SqlParameter("@projectid", projectId));
                }
                if (!string.IsNullOrEmpty(keyword))
                {
                    cmd += " and d.projectname like @keyword";
                    lstParam.Add(new SqlParameter("@keyword", "%" + keyword + "%"));
                }
                cmd += "  order by b.waterproofid,d.year";

                string? conn = _configuration.GetConnectionString("BuildCheck");
                SqlHelper sqlHeper = new SqlHelper(conn);
                if (lstParam.Count > 0)
                {
                    dt = sqlHeper.ExecuteQuery(cmd,lstParam.ToArray()) ;
                }
                else
                {
                    dt = sqlHeper.ExecuteQuery(cmd);
                }
                //string preRaderId = "";

                var distinctValues1 = dt.AsEnumerable()
                                    .Select(row => new
                                    {

                                        id = row.Field<int>("waterProofid"),
                                        target = row.Field<string>("target"),
                                        year = row.Field<string>("year"),
                                        projectName = row.Field<string>("projectname"),
                                        projectId = row.Field<int>("projectId"),
                                        //serialno = row.Field<long>("serialno"),


                                    })
                                    .Distinct().AsQueryable();
                WaterProof wp = null;
                for (int i = 0; i < distinctValues1.Count(); i++)
                {
                    var row = distinctValues1.ElementAt(i);
                    wp = new WaterProof();
                    wp.waterProofId = row.id;
                    wp.year = row.year;
                    wp.target = row.target;
                    wp.serialNo = (i + 1).ToString();
                    wp.projectName = row.projectName;
                    wp.projectId = row.projectId;
                    lstWP.Add(wp);
                    var distinctValues3 = dt.AsEnumerable()
                                    .Select(img => new
                                    {
                                        id = img.Field<int>("waterproofid"),
                                        seq = img.Field<int>("seq"),

                                    })
                                    .Where(x => x.id == row.id)
                                    .Distinct().AsQueryable();
                    for (int j = 0; j < distinctValues3.Count(); j++)
                    {
                        //distinctValues3.ElementAt(j);
                        wp.lstImg.Add(distinctValues3.ElementAt(j).seq.ToString());
                    }
                }
                return Ok(lstWP.ToList());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize]
        [HttpPost]
        [Route("GetCompareData")]
        public IActionResult GetCompareData(List<CompareParameter> lstCompare)//string[] lstId)
        {
            try
            {
                string json = "";
                var lstId = lstCompare.Select(x => x.id).ToList();
                //var sn = lstCompare.Select(x => x.sn).ToList();
                List<CompareData> lstCompareData = new List<CompareData>();
                //var range = string.Join(",", lstId);

                string? conn = _configuration.GetConnectionString("BuildCheck");
                SqlHelper sqlHeper = new SqlHelper(conn);
                string cmd = @"select sequence,projectname 
  FROM  WaterProofImage a 
  inner join WaterProofImageMain b on a.projectid=b.projectid
  where a.projectid = @projectid order by a.sequence";
                List<SqlParameter> lstParam = new List<SqlParameter>();

                for (int i = 0; i < lstId.Count; i++)
                {
                    lstParam.Clear();
                    lstParam.Add(new SqlParameter("@projectid", lstId[i]));
                    DataTable dt = sqlHeper.ExecuteQuery(cmd,lstParam.ToArray());
                    CompareData cd = new CompareData()
                    {
                        sequence = dt.Rows[i][0].ToString(),
                        projectname = dt.Rows[i][1].ToString(),
                    };
                    lstCompareData.Add(cd);
                    dt.Clear();
                }

                return Ok(lstCompareData.ToList());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);  
            }
        }
    }
}

using Ionic.Zip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Wra10Core2023.Models;
using SqlHelper = Wra10Core2023.Util.SQLHelper;

namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManualCheckController : ControllerBase
    {
        private readonly ILogger<ManualCheckController> _logger;
        private readonly IConfiguration _configuration;
        public ManualCheckController(ILogger<ManualCheckController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet]
        [Route("GetCaseYear")]
        public IActionResult GetCaseYear()
        {
            try
            {
                List<CaseYear> lstCaseYear = new List<CaseYear>();
                string? conn = _configuration.GetConnectionString("BuildCheck");
                if (conn != null)
                {
                    string cmd = @"select distinct year(checkdate)-1911 as Year from CheckMission order by year(checkdate)-1911 desc";
                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteQuery(cmd);
                    CaseYear cy = new CaseYear();
                    cy.year = "全部年度";
                    cy.value = "-1";
                    lstCaseYear.Add(cy);    
                    foreach (DataRow row in dt.Rows)
                    {
                        cy = new CaseYear
                        {
                            year = row["year"]?.ToString()?.Replace("\r", "").Replace("\n", ""),
                            value= row["year"]?.ToString()?.Replace("\r", "").Replace("\n", "")
                        };
                        lstCaseYear.Add(cy);
                    }
                }
                return Ok(lstCaseYear.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //
        [Authorize]
        [HttpGet]
        [Route("GetAllEquement")]
        public IActionResult GetAllEquement()
        {
            try
            {
                List<Equement> lstEquement = new List<Equement>();
                string? conn = _configuration.GetConnectionString("BuildCheck");
                if (conn != null)
                {
                    string cmd = @"select code,name from checktype where keyword is not null and del=0 order by code ";
                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteQuery(cmd);
                    Equement equ = new Equement();
                    equ.name = "全部設施";
                    equ.code = "-1";
                    lstEquement.Add(equ);
                    foreach (DataRow row in dt.Rows)
                    {
                        equ = new Equement
                        {
                            code = row["code"]?.ToString()?.Replace("\r", "").Replace("\n", ""),
                            name = row["name"]?.ToString()?.Replace("\r", "").Replace("\n", ""),
                        };
                        lstEquement.Add(equ);
                    }
                }
                return Ok(lstEquement.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetAllTarget")]
        public IActionResult GetAllTarget()
        {
            try
            {
                List<Target> lstTarget = new List<Target>();
                string? conn = _configuration.GetConnectionString("BuildCheck");
                if (conn != null)
                {
                    string cmd = @"
SELECT 
                          a.[id]
                          ,[Name]
      
                      FROM [BuildCheck].[dbo].[CheckTarget] a
					  inner join (select max(id) as id  from checktarget group by [name]) b
					  on a.id=b.id
                      where del=0 order by name
        ";
                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteQuery(cmd);
                    Target tgt = new Target();
                    tgt.name = "全部檢查";
                    tgt.id = -1;
                    lstTarget.Add(tgt);
                    foreach (DataRow row in dt.Rows)
                    {
                        tgt = new Target
                        {
                            id = int.Parse(row["id"]?.ToString()?.Replace("\r", "").Replace("\n", "")),
                            name = row["name"]?.ToString()?.Replace("\r", "").Replace("\n", ""),
                        };
                        lstTarget.Add(tgt);
                    }
                }
                return Ok(lstTarget.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("GetMapData")]
        public IActionResult GetMapData(CheckParameter param)
        {
            string filter = "";// " 1=1";
            string like = "";
            try
            {
                if (!string.IsNullOrEmpty(param.year) && param.year != "-1")
                {
                    filter += " and Year(a.CheckDate)-1911=" + param.year;
                }
                if (!string.IsNullOrEmpty(param.kind) && param.kind != "-1")
                {
                    filter += " and f.code='" + param.kind + "'";
                }

                if (!string.IsNullOrEmpty(param.station) && param.station != "-1")
                {
                    //filter += " and checktargetid='" + param.station + "'";
                    filter += " and e.name='" + param.targetName + "'";

                }

                if (!string.IsNullOrEmpty(param.state))
                {
                    switch (param.state)
                    {
                        case "4":
                            filter += " and h.name <> '正常'";
                            break;
                        case "-1":
                            break;
                        case "0":
                            filter += " and h.name = '正常'";
                            break;
                        case "1":
                            filter += " and h.name = '立即改善'";
                            break;
                        case "2":
                            filter += " and h.name = '注意改善'";
                            break;
                        case "3":
                            filter += " and h.name = '計畫改善'";
                            break;
                    }
                }
                string cmd = @"Select distinct a.id,a.checkdate
                --,g.Name as Machine
                --,d.name as state
                ,a.location 
                ,a.memo,Improvement
                ,f.Name as stationtype
                ,f.KeyWord
                ,c.Weather
                ,c.regular
                ,e.Name as station
                ,a.longitude,a.latitude
                ,a.Range
                ,a.Situation
                ,a.RepairRecord
                --,h.ID,h.FileID
                --,(h.No+' '+h.Memo) as description
				,h.name
                ,case h.name when '正常' then 0
                  when '計畫改善' then 1
                  when '注意改善' then 2
                  when '立即改善' then 3
                  end  as  [level]
 
                from checkmissionpoint a

                  
                  inner join CheckMission c on c.id=a.CheckMissionID and c.del=0
                  inner join CheckTarget e on e.ID=c.CheckTargetID and e.del=0
				  ---inner join [CheckPoint] m on m.CheckTypeID=a.CheckTypeID and m.CheckTypeID=a.CheckTypeID
                  inner join CheckType f on f.ID=a.CheckTypeID and f.del=0
                  inner join CheckTypeColumn g on g.CheckTypeID=f.id
				  inner join CheckTypeColumnItem h on h.CheckTypeColumnID=g.id 
				  inner join CheckMissionPointColumnValue n on n.CheckMissionPointID=a.ID and (n.CheckTypeColumnID=g.ID or n.CheckTypeColumnItemID=h.id)
                           where    a.del =0 
						   

				";
                cmd += filter;
                MapData md = new MapData();
                string? conn = _configuration.GetConnectionString("BuildCheck");
                SqlHelper sqlHelper = new SqlHelper(conn);
                DataTable dt = sqlHelper.ExecuteQuery(cmd);
                //List<GPS> lstGps = new List<GPS>();
                //List<MissionPointDetail> missionPointDetails = new List<MissionPointDetail>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    GPS gps = new GPS();
                    MissionPointDetail mpd = new MissionPointDetail();
                    DateTime t = DateTime.Parse(dt.Rows[i]["checkdate"].ToString());
                    mpd.checkdate = (t.Year - 1911) + "年";// + t.Month + "月" + t;
                    mpd.id = int.Parse(dt.Rows[i]["id"].ToString());
                    mpd.location = dt.Rows[i]["location"].ToString();
                    mpd.improvement = dt.Rows[i]["improvement"].ToString();
                    mpd.keyword = dt.Rows[i]["keyword"].ToString();
                    mpd.memo = dt.Rows[i]["memo"].ToString();
                    mpd.range = dt.Rows[i]["range"].ToString();
                    mpd.repairrecord = dt.Rows[i]["repairrecord"].ToString();
                    mpd.situation = dt.Rows[i]["situation"].ToString();
                    gps.title = mpd.station = dt.Rows[i]["station"].ToString();
                    mpd.stationtype = dt.Rows[i]["stationtype"].ToString();
                    gps.y = mpd.latitude = double.Parse(dt.Rows[i]["latitude"].ToString());
                    gps.x = mpd.longitude = double.Parse(dt.Rows[i]["longitude"].ToString());
                    gps.level = int.Parse(dt.Rows[i]["level"].ToString());
                    mpd.weather = dt.Rows[i]["weather"].ToString();
                    mpd.regular = dt.Rows[i]["regular"].ToString();
                    md.lstGps.Add(gps);
                    cmd = @"
                    select a.id,d.name as machine, c.name as state,c.memo as itemmemo,h.fileid,(h.no+' '+h.memo) as description from CheckMissionPoint a
	inner join CheckMissionPointColumnValue b on a.ID=b.CheckMissionPointID and CheckTypeColumnItemID is not null
	inner join CheckTypeColumnItem c on c.ID=b.CheckTypeColumnItemID
	inner join CheckTypeColumn d on d.id=c.CheckTypeColumnID
	inner join CheckMissionPointPicture h on h.CheckMissionPointID=a.id
	where a.id=" + mpd.id;

                    DataTable dt2 = sqlHelper.ExecuteQuery(cmd);
                    var distinctValues = dt2.AsEnumerable()
                            .Select(row => new
                            {
                                fileid = row.Field<int>("fileid"),
                                //id = row.Field<int>("id"),
                                description = row.Field<string>("description"),
                            })
                            //.Where(x => x.id.ToString() == mpd.id.ToString())
                            .Distinct().AsQueryable();
                    for (int j = 0; j < distinctValues.Count(); j++)
                    {
                        Picture pic = new Picture();
                        pic.fileId = distinctValues.ElementAt(j).fileid;
                        pic.description = distinctValues.ElementAt(j).description;
                        mpd.lstPic.Add(pic);
                    }

                    var distinctValues2 = dt2.AsEnumerable()
                            .Select(row => new
                            {
                                //id = row.Field<int>("id"),
                                machine = row.Field<string>("machine"),
                                state = row.Field<string>("state"),
                                //twd97x = row.Field<double>("TWD97X"),
                            })
                            //.Where(x => x.id.ToString() == mpd.id.ToString())
                            .Distinct().AsQueryable();
                    for (int j = 0; j < distinctValues2.Count(); j++)
                    {
                        MachineState ms = new MachineState();
                        ms.machine = distinctValues2.ElementAt(j).machine;
                        ms.state = distinctValues2.ElementAt(j).state;
                        mpd.lstMS.Add(ms);
                    }
                    md.lstMissionPointDetail.Add(mpd);
                }
                return Ok(md);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("GetStatistics")]
        public IActionResult GetStatistics(CheckParameter param)
        {
            string filter = " 1=1";
            string field1 = "";
            string field2 = "";
            string group = "";
            string like = "";
            try
            {
                if (param.year != "-1")
                {
                    filter += " and Year(a.CheckDate)-1911=" + param.year;
                    field1 += ",tyear";
                    field2 += ",Year(a.CheckDate)-1911 as tyear";
                    group += ",Year(a.CheckDate) - 1911";
                }
                else
                {
                    field1 += ",'全部年度' as tyear";
                    //group += ",'全部年度'";
                }
                if (param.kind != "-1")
                {
                    filter += " and c.code='" + param.kind + "'";
                    field1 += ",code,typename";
                    field2 += ",c.code,c.name as typename";
                    group += ",c.code,c.name";
                }
                else
                {
                    field1 += ",'all'  as code,'全部設施' as typename";
                    //group += ",'all','全部設施'";
                }
                if (param.station != "-1")
                {
                    //filter += " and checktargetid='" + param.station + "'";
                    filter += " and g.name='" + param.targetName + "'";
                }

                if (!string.IsNullOrEmpty(param.keyword))
                {
                    //like = " Improvement like '%"+param.keyword+"%' or h.name like '%" + param.keyword + "%' or a.memo like '%" + param.keyword + "%' or e.name like '%" + param.keyword + "%' or a.situation likr '%" + param.keyword + "%'";
                    like = " and (g.name like '%" + param.keyword + "%' or a.Improvement like '%" + param.keyword + "%' or f.FileName like '%" + param.keyword + "%' or a.Memo like  '%" + param.keyword + "%')";
                }
                string? conn = _configuration.GetConnectionString("BuildCheck");
                List<MissionStatistics> lstMissionStatistics = new List<MissionStatistics>();
                if (conn != null)
                {
                    string cmd = @"
                   SELECT ROW_NUMBER() OVER (ORDER BY  targetname ASC) as serialNo " +

                field1 + @", targetname, isnull([正常],0) as level0, isnull([立即改善],0) as level1, isnull([注意改善],0) as level2, isnull([計畫改善],0) as level3
                ,isnull([正常],0)+isnull([注意改善],0)+isnull([立即改善],0)+isnull([計畫改善],0) as totalstations,isnull([注意改善],0)+isnull([立即改善],0)+isnull([計畫改善],0) as faults
                FROM (

                SELECT 
                  g.name as targetname, e.name as ename ,count(e.Name) as result " + field2 + @"
                  FROM [BuildCheck].[dbo].[CheckMissionPoint] a
                  inner join CheckMissionPointColumnValue b on a.ID=b.CheckMissionPointID and b.del=0
                  inner join CheckMission f on f.id=a.CheckMissionID and f.del=0
                  inner join CheckType c on c.id=a.CheckTypeID and c.del=0
                 --inner join CheckTypeColumn d on d.id=b.CheckTypeColumnID
                 inner join CheckTarget g on g.ID=f.CheckTargetID and g.del=0 ---and isnull(g.code,'') <> ''
                 inner join CheckTypeColumnItem e on e.id=b.CheckTypeColumnItemID and e.del=0 
                 where " + filter + like + @" 
                 group by e.name,g.name" + group + @"
                 ) as t
                 PIVOT
                 (
   
                   max(REsult)
                   FOR ename IN ([正常], [注意改善], [立即改善],[計畫改善])
                 ) p
             order by targetname
                   ";


                    SqlHelper sqlHeper = new SqlHelper(conn);
                    DataTable dt = sqlHeper.ExecuteQuery(cmd);
                    
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        MissionStatistics mp = new MissionStatistics();
                        mp.serialNo = dt.Rows[i]["SerialNo"].ToString();
                        mp.year = dt.Rows[i]["tyear"].ToString();
                        mp.name = dt.Rows[i]["targetname"].ToString();
                        mp.level0 = dt.Rows[i]["level0"].ToString();
                        mp.level1 = dt.Rows[i]["level1"].ToString();
                        mp.level2 = dt.Rows[i]["level2"].ToString();
                        mp.level3 = dt.Rows[i]["level3"].ToString();
                        mp.kind = dt.Rows[i]["typename"].ToString();
                        mp.totalStations = dt.Rows[i]["totalstations"].ToString();
                        mp.faults = dt.Rows[i]["faults"].ToString();
                        mp.id = dt.Rows[i]["SerialNo"].ToString();
                        mp.targetId = dt.Rows[i]["TargetName"].ToString();
                        param.targetId = "";// dt.Rows[i]["TargetName"].ToString();
                        param.targetName = dt.Rows[i]["TargetName"].ToString();
                        mp.checkParameter = JsonConvert.SerializeObject(param);
                        //mp.gps = dt.Rows[i]["SerialNo"].ToString();
                        //mp.download = dt.Rows[i]["SerialNo"].ToString();
                        //mp.view = dt.Rows[i]["view"].ToString();
                        lstMissionStatistics.Add(mp);
                    }
                }
                return Ok(lstMissionStatistics.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("GetGpsStatistics")]
        public IActionResult GetGpsStatistics(CheckParameter param)
        {
            try
            {
                string filter = "";
                if (!string.IsNullOrEmpty(param.year) && param.year != "-1")
                {
                    filter += " and Year(a.CheckDate)-1911=" + param.year;
                }
                if (!string.IsNullOrEmpty(param.kind) && param.kind != "-1")
                {
                    filter += " and f.code='" + param.kind + "'";
                }

                if (!string.IsNullOrEmpty(param.targetId) && param.targetId != "-1")
                {
                    filter += " and checktargetid='" + param.targetId + "'";

                }

                if (!string.IsNullOrEmpty(param.state))
                {
                    switch (param.state)
                    {
                        case "4":
                            filter += " and state <> '正常'";
                            break;
                        case "-1":
                            break;
                        case "0":
                            filter += " and state = '正常'";
                            break;
                        case "1":
                            filter += " and state = '立即改善'";
                            break;
                        case "2":
                            filter += " and state = '注意改善'";
                            break;
                        case "3":
                            filter += " and state = '計畫改善'";
                            break;
                    }
                }


                string cmd = @"
                Select a.id,a.checkdate
                ,a.location 
                ,f.Name as stationtype
                ,f.KeyWord
                ,c.Weather
                ,case when c.regular= 0 then '不定期'
                          when c.Regular= 1 then '定期'
                     end as regular
                ,e.Name as station
                ,a.longitude,a.latitude
                ,a.Range
                ,a.Situation
                ,a.RepairRecord
                from checkmissionpoint a
                  inner join CheckMission c on c.id=a.CheckMissionID and c.del=0
                  inner join CheckTarget e on e.ID=c.CheckTargetID and e.del=0
                  inner join CheckType f on f.ID=a.CheckTypeID and f.del=0
                           where    a.del =0 
            ";
                cmd += filter;
                string? conn = _configuration.GetConnectionString("BuildCheck");
                SqlHelper sqlHelper = new SqlHelper(conn);

                DataTable dt = sqlHelper.ExecuteQuery(cmd);
                List<GPS> lstGps = new List<GPS>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    GPS gps = new GPS();
                    gps.title = dt.Rows[i]["station"].ToString();
                    gps.y = double.Parse(dt.Rows[i]["latitude"].ToString());
                    gps.x = double.Parse(dt.Rows[i]["longitude"].ToString());
                    lstGps.Add(gps);
                }

                return Ok(lstGps.ToList());
            }
            catch (Exception ex)
            { 
                return BadRequest(ex.Message);  
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetDetailDataStatistics")]
        public IActionResult GetDetailDataStatistics(CheckParameter param)
        {
            try
            {
                string filter = "";
                if (!string.IsNullOrEmpty(param.year) && param.year != "-1")
                {
                    filter += " and Year(a.CheckDate)-1911=" + param.year;
                }
                if (!string.IsNullOrEmpty(param.kind) && param.kind != "-1")
                {
                    filter += " and c.code='" + param.kind + "'";
                }
                /*
                if (!string.IsNullOrEmpty(param.station) && param.station != "-1")
                {
                    filter += " and checktargetid='" + param.station + "'";

                }
                */

                if (!string.IsNullOrEmpty(param.targetName))// && param.targetId != "-1")
                {
                    //filter += " and checktargetid='" + param.targetId + "'";
                    filter += " and e.name='" + param.targetName + "'";

                }
                if (!string.IsNullOrEmpty(param.state))
                {
                    switch (param.state)
                    {
                        case "4":
                            filter += " and state <> '正常'";
                            break;
                        case "-1":
                            break;
                        case "0":
                            filter += " and state = '正常'";
                            break;
                        case "1":
                            filter += " and state = '立即改善'";
                            break;
                        case "2":
                            filter += " and state = '注意改善'";
                            break;
                        case "3":
                            filter += " and state = '計畫改善'";
                            break;
                    }
                }
                string json = "";


                string cmd = @"
                Select a.id,a.checkdate
                --,g.Name as Machine
                --,d.name as state
                ,a.location 
                ,a.memo,Improvement
                ,f.Name as stationtype
                ,f.KeyWord
                ,c.Weather
                ,case when c.regular= 0 then '不定期'
                          when c.Regular= 1 then '定期'
                     end as regular
                ,e.Name as station
                ,a.longitude,a.latitude
                ,a.Range
                ,a.Situation
                ,a.RepairRecord
                --,h.ID,h.FileID
                --,(h.No+' '+h.Memo) as description
                from checkmissionpoint a

                  --inner join checkmissionpointcolumnvalue b on a.ID=b.CheckMissionPointID and CheckTypeColumnItemID is not null
  
                  --inner join CheckTypeColumnItem d on d.ID=b.CheckTypeColumnItemID
                  --inner join CheckTypeColumn g on g.id=d.CheckTypeColumnID
  
                  inner join CheckMission c on c.id=a.CheckMissionID and c.del=0
                  inner join CheckTarget e on e.ID=c.CheckTargetID and e.del=0
                  inner join CheckType f on f.ID=a.CheckTypeID and f.del=0
                  --inner join CheckMissionPointPicture h on h.CheckMissionPointID=a.id
                           where    a.del =0 
            ";
                cmd += filter;
                string? conn = _configuration.GetConnectionString("BuildCheck");
                SqlHelper sqlHelper = new SqlHelper(conn);
                DataTable dt = sqlHelper.ExecuteQuery(cmd);
                //List<GPS> lstGps = new List<GPS>();
                List<MissionPointDetail> lstMissionPointDetails = new List<MissionPointDetail>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    GPS gps = new GPS();
                    MissionPointDetail mpd = new MissionPointDetail();
                    DateTime t = DateTime.Parse(dt.Rows[i]["checkdate"].ToString());
                    mpd.checkdate = (t.Year - 1911) + "年";// + t.Month + "月" + t;
                    mpd.id = int.Parse(dt.Rows[i]["id"].ToString());
                    mpd.location = dt.Rows[i]["location"].ToString();
                    mpd.improvement = dt.Rows[i]["improvement"].ToString();
                    mpd.keyword = dt.Rows[i]["keyword"].ToString();
                    mpd.memo = dt.Rows[i]["memo"].ToString();
                    mpd.range = dt.Rows[i]["range"].ToString();
                    mpd.repairrecord = dt.Rows[i]["repairrecord"].ToString();
                    mpd.situation = dt.Rows[i]["situation"].ToString();
                    gps.title = mpd.station = dt.Rows[i]["station"].ToString();
                    mpd.stationtype = dt.Rows[i]["stationtype"].ToString();
                    gps.y = mpd.latitude = double.Parse(dt.Rows[i]["latitude"].ToString());
                    gps.x = mpd.longitude = double.Parse(dt.Rows[i]["longitude"].ToString());
                    mpd.weather = dt.Rows[i]["weather"].ToString();
                    mpd.regular = dt.Rows[i]["regular"].ToString();
                    //lstGps.Add(gps);
                    cmd = @"
                    select a.id,d.name as machine, c.name as state,c.memo as itemmemo,h.fileid,(h.no+' '+h.memo) as description from CheckMissionPoint a
	inner join CheckMissionPointColumnValue b on a.ID=b.CheckMissionPointID and CheckTypeColumnItemID is not null
	inner join CheckTypeColumnItem c on c.ID=b.CheckTypeColumnItemID
	inner join CheckTypeColumn d on d.id=c.CheckTypeColumnID
	inner join CheckMissionPointPicture h on h.CheckMissionPointID=a.id
	where a.id=" + mpd.id;

                    DataTable dt2 = sqlHelper.ExecuteQuery(cmd);
                    var distinctValues = dt2.AsEnumerable()
                            .Select(row => new
                            {
                                fileid = row.Field<int>("fileid"),
                                //id = row.Field<int>("id"),
                                description = row.Field<string>("description"),
                            })
                            //.Where(x => x.id.ToString() == mpd.id.ToString())
                            .Distinct().AsQueryable();
                    for (int j = 0; j < distinctValues.Count(); j++)
                    {
                        Picture pic = new Picture();
                        pic.fileId = distinctValues.ElementAt(j).fileid;
                        pic.description = distinctValues.ElementAt(j).description;
                        mpd.lstPic.Add(pic);
                    }

                    var distinctValues2 = dt2.AsEnumerable()
                            .Select(row => new
                            {
                                //id = row.Field<int>("id"),
                                machine = row.Field<string>("machine"),
                                state = row.Field<string>("state"),
                                //twd97x = row.Field<double>("TWD97X"),
                            })
                            //.Where(x => x.id.ToString() == mpd.id.ToString())
                            .Distinct().AsQueryable();
                    for (int j = 0; j < distinctValues2.Count(); j++)
                    {
                        MachineState ms = new MachineState();
                        ms.machine = distinctValues2.ElementAt(j).machine;
                        ms.state = distinctValues2.ElementAt(j).state;
                        mpd.lstMS.Add(ms);
                    }
                    lstMissionPointDetails.Add(mpd);
                }

                

                return Ok(lstMissionPointDetails.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("DownloadZip")]
        public async Task<IActionResult> DownloadZip(CheckParameter param)
        {
            try
            {
                string filter = " 1=1";
                string field1 = "";
                string field2 = "";
                string group = "";
                if (param.year != "-1")
                {
                    filter += " and Year(a.CheckDate)-1911=" + param.year;
                }
                else
                {
                    field1 += ",'全部年度' as tyear";
                }
                if (param.kind != "-1")
                {
                    filter += " and e.code='" + param.kind + "'";
                }
                else
                {
                }
                if (param.targetId != null)
                {
                    filter += " and checktargetid='" + param.targetId + "'";

                }
                string zipName = "";
                string cmd = @"
                   

SELECT 
  distinct CheckTargetID, a.id, h.id as fileId, bytes,f.FileName,g.Name as stationname
  FROM [BuildCheck].[dbo].[CheckMissionPoint] a
  
  inner join CheckMissionPointColumnValue b on a.ID=b.CheckMissionPointID and b.del=0
  inner join CheckMission f on f.id=a.CheckMissionID and f.del=0
  inner join CheckType c on c.id=a.CheckTypeID and c.del=0
 inner join CheckTarget g on g.ID=f.CheckTargetID and g.del=0 and isnull(g.code,'') <> ''
 inner join CheckTypeColumnItem e on e.id=b.CheckTypeColumnItemID and e.del=0 
 inner join [file] h on h.id=a.fileid

 where " + filter;


                string? conn = _configuration.GetConnectionString("BuildCheck");
                SqlHelper sqlHeper = new SqlHelper(conn);
                DataTable dt = sqlHeper.ExecuteQuery(cmd);
                using (var zipStream = new MemoryStream())
                {
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                        zip.AlternateEncoding = Encoding.UTF8;
                        //zip.AddDirectoryByName("Files");

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (i == 0)
                                zipName = dt.Rows[i]["stationname"].ToString();
                            zip.AddEntry(dt.Rows[i]["filename"].ToString(), (byte[])dt.Rows[i]["bytes"]);

                        }
                        zip.Save(zipStream);
                        string fileName = String.Format("{0}_{1}.zip", zipName, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                        zipStream.Seek(0, SeekOrigin.Begin);
                        var result = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new ByteArrayContent(zipStream.ToArray())
                        };
                        result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = "archive.zip" // Set the file name for the downloaded archive
                        };
                        result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");


                    }

                    return Ok(File(zipStream.ToArray(), "application/zip", zipName));
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        //[Authorize]
        [HttpPost]
        [Route("GetData")]
        public IActionResult GetData(CheckParameter param)
        {
            try
            {
                string filter = "a.del=0 ";// " 1=1";
                string filter2 = "a.del=0 ";
                string like = "";
                if (!string.IsNullOrEmpty(param.year) && param.year != "-1")
                {
                    filter += " and Year(a.CheckDate)-1911=" + param.year;
                }
                if (!string.IsNullOrEmpty(param.kind) && param.kind != "-1")
                {
                    filter += " and e.code='" + param.kind + "'";
                }

                if (!string.IsNullOrEmpty(param.station) && param.station != "-1")
                {
                    //filter += " and checktargetid='" + param.station + "'";
                    filter += " and d.name='" + param.targetName + "'";
                }

                if (!string.IsNullOrEmpty(param.state))
                {
                    switch (param.state)
                    {
                        case "4":
                            filter2 += " and d.name <> '正常'";
                            break;
                        case "-1":
                            break;
                        case "0":
                            filter2 += " and d.name = '正常'";
                            break;
                        case "1":
                            filter2 += " and d.name = '立即改善'";
                            break;
                        case "2":
                            filter2 += " and d.name = '注意改善'";
                            break;
                        case "3":
                            filter2 += " and d.name = '計畫改善'";
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(param.keyword))
                {
                    //like = " Improvement like '%"+param.keyword+"%' or h.name like '%" + param.keyword + "%' or a.memo like '%" + param.keyword + "%' or e.name like '%" + param.keyword + "%' or a.situation likr '%" + param.keyword + "%'";
                    like = " and (d.name like '%" + param.keyword + "%' or a.Improvement like '%" + param.keyword + "%' or c.FileName like '%" + param.keyword + "%' or a.Memo like  '%" + param.keyword + "%')";
                }

                string json = "";
                string cmd = @"
Select ROW_NUMBER() OVER (ORDER BY a.ID ASC) as serialNo,a.id , CONVERT(VARCHAR(3),CONVERT(VARCHAR(4),a.checkdate,20) - 1911) as year ,d.name,a.location, case when b.state=0 then '正常' 
                 when b.state=1 then '計畫改善'
				 when b.state=2 then '注意改善'
				 when b.state=3 then '立即改善'
             end as [state],a.FileId,
             case when c.regular= 0 then '不定期'
                  when c.Regular= 1 then '定期'
             end as regular
			 ,memo,Improvement,'檢視' as [view],[FileName], cast(latitude as varchar)+','+cast(Longitude as varchar) as gps,c.fileid
				 from checkmissionpoint a

inner join (SELECT a.[ID]
  , max(
  case when d.Name= '正常' then 0
     when d.Name= '計畫改善' then 1 
	 when d.Name= '注意改善' then 2
	 when d.Name= '立即改善' then 3
end) as state

  FROM [BuildCheck].[dbo].[CheckMissionPoint] a 
  inner join checkmissionpointcolumnvalue b on a.ID=b.CheckMissionPointID
  inner join CheckTypeColumnItem d on d.ID=b.CheckTypeColumnItemID

  where " + filter2 + @"  
  
  group by a.id) as b on a.id=b.id
  inner join CheckMission c on c.id=a.CheckMissionID
  inner join CheckTarget d on d.ID=c.CheckTargetID
  inner join checktype e on e.id=a.CheckTypeID

";
                cmd += " where " + filter + like;
                string? conn = _configuration.GetConnectionString("BuildCheck");
                SqlHelper sqlHeper = new SqlHelper(conn);
                DataTable dt = sqlHeper.ExecuteQuery(cmd);
                List<MissionPoint> lstMissionPoint = new List<MissionPoint>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    MissionPoint mp = new MissionPoint();
                    mp.serialNo = dt.Rows[i]["SerialNo"].ToString();
                    mp.year = dt.Rows[i]["year"].ToString();
                    mp.name = dt.Rows[i]["name"].ToString();
                    mp.regular = dt.Rows[i]["regular"].ToString();
                    mp.state = dt.Rows[i]["state"].ToString();
                    mp.location = dt.Rows[i]["location"].ToString();
                    mp.memo = dt.Rows[i]["memo"].ToString();
                    mp.improvement = dt.Rows[i]["improvement"].ToString();
                    mp.id = dt.Rows[i]["id"].ToString();
                    mp.gps = dt.Rows[i]["gps"].ToString();
                    mp.filename = dt.Rows[i]["filename"].ToString();
                    mp.fileId = dt.Rows[i]["fileid"].ToString();
                    mp.view = dt.Rows[i]["view"].ToString();
                    lstMissionPoint.Add(mp);
                }
                return Ok(lstMissionPoint);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private DataTable getDetailDataTable(List<string> lstId)
        {
            try
            {
                var ids = String.Join(",", lstId).Replace("\"", "");

                string cmd = @"Select  a.id,a.checkdate
        ,g.Name as Machine
        ,d.name as state
        ,a.location 
        ,a.memo,Improvement
        ,f.Name as stationtype
        ,f.KeyWord
        ,c.Weather
        ,case when c.regular= 0 then '不定期'
                  when c.Regular= 1 then '定期'
             end as regular
        ,e.Name as station
        ,a.TWD97X,a.TWD97Y
        ,a.Range
        ,a.Situation
        ,a.RepairRecord
        ,h.ID,h.FileID
        ,(h.No+' '+h.Memo) as description
        from checkmissionpoint a

          inner join checkmissionpointcolumnvalue b on a.ID=b.CheckMissionPointID and CheckTypeColumnItemID is not null
  
          inner join CheckTypeColumnItem d on d.ID=b.CheckTypeColumnItemID
          inner join CheckTypeColumn g on g.id=d.CheckTypeColumnID
  
          inner join CheckMission c on c.id=a.CheckMissionID
          inner join CheckTarget e on e.ID=c.CheckTargetID
          inner join CheckType f on f.ID=a.CheckTypeID
          inner join CheckMissionPointPicture h on h.CheckMissionPointID=a.id
  
            where a.id in (" + ids + @") order by a.id";
                string? conn = _configuration.GetConnectionString("BuildCheck");
                SqlHelper sqlHeper = new SqlHelper(conn);
                DataTable dt = sqlHeper.ExecuteQuery(cmd);
                return dt;
            }
            catch(Exception ex) {
                return null;
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetDetailData")]
        public IActionResult GetDetailData(string id)
        {
            string json = "";

            List<string> lstId = new List<string>();
            lstId.Add(id);
            DataTable dt = getDetailDataTable(lstId);
            List<MissionPointDetail> lstDetail = new List<MissionPointDetail>();





            try
            {
                var distinctValues3 = dt.AsEnumerable()
                            .Select(row => new
                            {

                                id = row.Field<int>("id"),
                                checkdate = row.Field<DateTime>("checkdate"),
                                location = row.Field<string>("location"),
                                memo = row.Field<string>("memo"),

                                improvement = row.Field<string>("improvement"),
                                stationtype = row.Field<string>("stationtype"),
                                station = row.Field<string>("station"),

                                keyword = row.Field<string>("keyword"),
                                weather = row.Field<string>("weather"),
                                regular = row.Field<string>("regular"),

                                twd97x = row.Field<double>("TWD97X"),
                                twd97y = row.Field<double>("TWD97Y"),
                                range = row.Field<string>("range"),
                                situation = row.Field<string>("situation"),
                                repairrecord = row.Field<string>("repairrecord"),

                            })
                            .Distinct().AsQueryable();
                for (int i = 0; i < distinctValues3.Count(); i++)
                {
                    MissionPointDetail mpd = new MissionPointDetail();
                    mpd.checkdate = (distinctValues3.ElementAt(i).checkdate.Year - 1911) + "年" + distinctValues3.ElementAt(i).checkdate.Month + "月" + distinctValues3.ElementAt(i).checkdate.Day;
                    mpd.id = distinctValues3.ElementAt(i).id;
                    mpd.location = distinctValues3.ElementAt(i).location;
                    mpd.improvement = distinctValues3.ElementAt(i).improvement;
                    mpd.keyword = distinctValues3.ElementAt(i).keyword;
                    mpd.memo = distinctValues3.ElementAt(i).memo;
                    mpd.range = distinctValues3.ElementAt(i).range;
                    mpd.repairrecord = distinctValues3.ElementAt(i).repairrecord;
                    mpd.situation = distinctValues3.ElementAt(i).situation;
                    mpd.station = distinctValues3.ElementAt(i).station;
                    mpd.stationtype = distinctValues3.ElementAt(i).stationtype;
                    mpd.twd97x = distinctValues3.ElementAt(i).twd97x;
                    mpd.twd97y = distinctValues3.ElementAt(i).twd97y;
                    mpd.weather = distinctValues3.ElementAt(i).weather;
                    mpd.regular = distinctValues3.ElementAt(i).regular;
                    lstDetail.Add(mpd);

                    var distinctValues = dt.AsEnumerable()
                        .Select(row => new {
                            fileid = row.Field<int>("fileid"),
                            id = row.Field<int>("id"),
                            description = row.Field<string>("description"),
                        })
                        .Distinct().AsQueryable();
                    for (int j = 0; j < distinctValues.Count(); j++)
                    {
                        Picture pic = new Picture();
                        pic.fileId = distinctValues.ElementAt(j).fileid;
                        pic.description = distinctValues.ElementAt(j).description;
                        mpd.lstPic.Add(pic);
                    }

                    var distinctValues2 = dt.AsEnumerable()
                        .Select(row => new {
                            machine = row.Field<string>("machine"),
                            state = row.Field<string>("state"),
                            //twd97x = row.Field<double>("TWD97X"),
                        })
                        .Distinct().AsQueryable();

                    for (int j = 0; j < distinctValues2.Count(); j++)
                    {
                        MachineState ms = new MachineState();
                        ms.machine = distinctValues2.ElementAt(j).machine;
                        ms.state = distinctValues2.ElementAt(j).state;
                        mpd.lstMS.Add(ms);
                    }
                }

                return Ok(lstDetail.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        

        [Authorize]
        [HttpPost]
        [Route("GetCompareData")]
        public IActionResult GetCompareData(List<CompareParemeter> lstParam)//string[] lstId)
        {
            try
            {
                var lstId = lstParam.Select(x => x.id).ToList();
                var sn = lstParam.Select(x => x.sn).ToList();
                DataTable dt = getDetailDataTable(lstId);

                List<MissionPointCompare> lstMissionPointCompare = new List<MissionPointCompare>();
                for (int k = 0; k < lstId.Count; k++)
                {
                    MissionPointCompare mpc = new MissionPointCompare();
                    mpc.id = lstId[k];
                    lstMissionPointCompare.Add(mpc);



                    try
                    {
                        var distinctValues3 = dt.AsEnumerable()
                                    .Select(row => new
                                    {

                                        id = row.Field<int>("id"),
                                        checkdate = row.Field<DateTime>("checkdate"),
                                        location = row.Field<string>("location"),
                                        memo = row.Field<string>("memo"),

                                        improvement = row.Field<string>("improvement"),
                                        stationtype = row.Field<string>("stationtype"),
                                        station = row.Field<string>("station"),

                                        keyword = row.Field<string>("keyword"),
                                        weather = row.Field<string>("weather"),
                                        regular = row.Field<string>("regular"),

                                        twd97x = row.Field<double>("TWD97X"),
                                        twd97y = row.Field<double>("TWD97Y"),
                                        range = row.Field<string>("range"),
                                        situation = row.Field<string>("situation"),
                                        repairrecord = row.Field<string>("repairrecord"),

                                    })
                                    .Where(x => x.id.ToString() == mpc.id)
                                    .Distinct().AsQueryable();
                        MissionPointDetail mpd = new MissionPointDetail();
                        mpc.missionPointDetails.Add(mpd);
                        for (int i = 0; i < 1; i++)
                        {

                            mpd.checkdate = (distinctValues3.ElementAt(i).checkdate.Year - 1911) + "年" + distinctValues3.ElementAt(i).checkdate.Month + "月" + distinctValues3.ElementAt(i).checkdate.Day;
                            mpd.id = distinctValues3.ElementAt(i).id;
                            mpd.location = distinctValues3.ElementAt(i).location;
                            mpd.improvement = distinctValues3.ElementAt(i).improvement;
                            mpd.keyword = distinctValues3.ElementAt(i).keyword;
                            mpd.memo = distinctValues3.ElementAt(i).memo;
                            mpd.range = distinctValues3.ElementAt(i).range;
                            mpd.repairrecord = distinctValues3.ElementAt(i).repairrecord;
                            mpd.situation = distinctValues3.ElementAt(i).situation;
                            mpd.station = distinctValues3.ElementAt(i).station;
                            mpd.stationtype = distinctValues3.ElementAt(i).stationtype;
                            mpd.twd97x = distinctValues3.ElementAt(i).twd97x;
                            mpd.twd97y = distinctValues3.ElementAt(i).twd97y;
                            mpd.weather = distinctValues3.ElementAt(i).weather;
                            mpd.regular = distinctValues3.ElementAt(i).regular;
                            mpc.missionPointDetails.Add(mpd);

                            var distinctValues = dt.AsEnumerable()
                                .Select(row => new
                                {
                                    fileid = row.Field<int>("fileid"),
                                    id = row.Field<int>("id"),
                                    description = row.Field<string>("description"),
                                })
                                .Where(x => x.id.ToString() == mpc.id)
                                .Distinct().AsQueryable();
                            for (int j = 0; j < distinctValues.Count(); j++)
                            {
                                Picture pic = new Picture();
                                pic.fileId = distinctValues.ElementAt(j).fileid;
                                pic.description = distinctValues.ElementAt(j).description;
                                mpd.lstPic.Add(pic);
                            }

                            var distinctValues2 = dt.AsEnumerable()
                                .Select(row => new
                                {
                                    id = row.Field<int>("id"),
                                    machine = row.Field<string>("machine"),
                                    state = row.Field<string>("state"),
                                    //twd97x = row.Field<double>("TWD97X"),
                                })
                            .Where(x => x.id.ToString() == mpc.id)
                                .Distinct().AsQueryable();

                            for (int j = 0; j < distinctValues2.Count(); j++)
                            {
                                MachineState ms = new MachineState();
                                ms.machine = distinctValues2.ElementAt(j).machine;
                                ms.state = distinctValues2.ElementAt(j).state;
                                mpd.lstMS.Add(ms);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                       
                    }
                }
                return Ok(lstMissionPointCompare.ToList());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

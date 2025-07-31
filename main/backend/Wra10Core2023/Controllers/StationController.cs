using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Newtonsoft.Json;
using System.Data;
using Wra10Core2023.Models;
using SqlHelper = Wra10Core2023.Util.SQLHelper;

namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationController : ControllerBase
    {
        private readonly ILogger<StationController> _logger;
        private readonly IConfiguration _configuration;
        public StationController(ILogger<StationController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost]
        [Route("GetWaterStationById")]
        public IActionResult GetWaterStationById(string? stationId)
        {
            DataTable dt;
            List<WaterStation> lstStation = new List<WaterStation>();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("stationId", String.IsNullOrEmpty(stationId) ? DBNull.Value : stationId));
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetStationById", lstParams.ToArray());
                foreach (DataRow row in dt.Rows)
                {
                    decimal x = 0;
                    decimal y = 0;
                    decimal.TryParse(row["X"].ToString(), out x);
                    decimal.TryParse(row["Y"].ToString(), out y);
                    WaterStation ws = new WaterStation
                    {
                        StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                        StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                        X = x,
                        Y = y
                    };
                    lstStation.Add(ws);
                }
            }
            return Ok(lstStation.ToList());
        }

        [Authorize]
        [HttpPost]
        [Route("GetWaterStationByName")]
        public IActionResult GetWaterStationByName(string? stationName)
        {
            DataTable dt;
            List<WaterStation> lstStation = new List<WaterStation>();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("stationName", String.IsNullOrEmpty(stationName) ? DBNull.Value : stationName));
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetStationByName", lstParams.ToArray());
                foreach (DataRow row in dt.Rows)
                {
                    decimal x = 0;
                    decimal y = 0;
                    decimal.TryParse(row["X"].ToString(), out x);
                    decimal.TryParse(row["Y"].ToString(), out y);
                    WaterStation ws = new WaterStation
                    {
                        StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                        StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                        X = x,
                        Y = y
                    };
                    lstStation.Add(ws);
                }
            }
            return Ok(lstStation.ToList());
        }

        [Authorize]
        [HttpPost]
        [Route("GetWaterStationByAreaId")]
        public IActionResult GetWaterStationByAreaId(string? areaId)
        {
            DataTable dt;
            List<WaterStation> lstStation = new List<WaterStation>();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("areaId", String.IsNullOrEmpty(areaId) ? DBNull.Value : areaId));
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetStationByAreaId", lstParams.ToArray());
                foreach (DataRow row in dt.Rows)
                {
                    decimal x = 0;
                    decimal y = 0;
                    decimal.TryParse(row["X"].ToString(), out x);
                    decimal.TryParse(row["Y"].ToString(), out y);
                    WaterStation ws = new WaterStation
                    {
                        StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                        StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                        X = x,
                        Y = y
                    };
                    lstStation.Add(ws);
                }
            }
            return Ok(lstStation.ToList());
        }

        [Authorize]
        [HttpPost]
        [Route("GetWaterStationByAreaName")]
        public IActionResult GetWaterStationByAreaName(string? areaName)
        {
            DataTable dt;
            List<WaterStation> lstStation = new List<WaterStation>();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("areaName", String.IsNullOrEmpty(areaName) ? DBNull.Value : areaName));
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetStationByAreaName", lstParams.ToArray());
                foreach (DataRow row in dt.Rows)
                {
                    decimal x = 0;
                    decimal y = 0;
                    decimal.TryParse(row["X"].ToString(), out x);
                    decimal.TryParse(row["Y"].ToString(), out y);
                    WaterStation ws = new WaterStation
                    {
                        StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                        StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                        AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                        X = x,
                        Y = y
                    };
                    lstStation.Add(ws);
                }
            }
            return Ok(lstStation.ToList());
        }

        /*
        [Authorize]
        [HttpGet]
        [Route("GetStationAllData")]
        public IActionResult GetStationAllData()
        {
            List<SensorMoreData> lstStations = new List<SensorMoreData>();
            List<SensorMoreData> lstWaters = new List<SensorMoreData>();
            List<SensorMoreData> lstGates = new List<SensorMoreData>();
            List<SensorMoreData> lstOthers = new List<SensorMoreData>();
            string jsonStation = "";

            SqlHelper sqlhelper = new SqlHelper(_configuration.GetConnectionString("Water2022"));
            try
            {
                DataTable dt = sqlhelper.ExecuteQuery("Select * from Stations order by StationId");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SensorMoreData sensor = new SensorMoreData();
                    sensor.errorMessage = "";
                    sensor.sensorType = dt.Rows[i]["sensorType"].ToString();
                    sensor.sensorTypeName = dt.Rows[i]["sensorTypeName"].ToString();
                    sensor.areaID = dt.Rows[i]["areaId"].ToString();
                    sensor.areaName = dt.Rows[i]["areaName"].ToString();
                    sensor.stationNameA = dt.Rows[i]["stationnameA"].ToString();
                    sensor.sensorNameA = dt.Rows[i]["sensornameA"].ToString();
                    sensor.sensorId = dt.Rows[i]["sensorId"].ToString();
                    sensor.lastDataTime = dt.Rows[i]["lastdatatime"].ToString();
                    sensor.lastDataTimePrev = dt.Rows[i]["lastdatatimePrev"].ToString();
                    sensor.value1 = decimal.Parse(dt.Rows[i]["value1"].ToString());
                    sensor.value2 = decimal.Parse(dt.Rows[i]["value2"].ToString());
                    sensor.diff1 = decimal.Parse(dt.Rows[i]["diff1"].ToString());
                    sensor.diff2 = decimal.Parse(dt.Rows[i]["diff2"].ToString());
                    sensor.initValue = decimal.Parse(dt.Rows[i]["InitValue"].ToString());
                    sensor.initValue2 = decimal.Parse(dt.Rows[i]["InitValue2"].ToString());
                    sensor.offset = decimal.Parse(dt.Rows[i]["offset"].ToString());
                    sensor.offset2 = decimal.Parse(dt.Rows[i]["offset2"].ToString());
                    sensor.status1 = int.Parse(dt.Rows[i]["status1"].ToString());
                    sensor.status2 = int.Parse(dt.Rows[i]["status2"].ToString());
                    sensor.unit = dt.Rows[i]["unit"].ToString();
                    sensor.remark = dt.Rows[i]["remark"].ToString();
                    sensor.alarm = int.Parse(dt.Rows[i]["alarm"].ToString());
                    int eqGrade = 0;
                    int.TryParse(dt.Rows[i]["eqgrade"].ToString(), out eqGrade);
                    sensor.eqGrade = eqGrade;
                    if (sensor.sensorType.ToLower() == "waterlevel")
                        lstWaters.Add(sensor);
                    else if (sensor.sensorType.ToLower() == "gate")
                        lstGates.Add(sensor);
                    else
                        lstOthers.Add(sensor);
                }
                //jsonStation = JsonConvert.SerializeObject(lstStations);
                //Context.Response.Write(jsonStation);
            }
            catch (Exception ex)
            {
                BadRequest(ex.Message);
                //Context.Response.Write(jsonStation); 
            }
            for (int i = 0; i < lstWaters.Count; i++)
            {
                lstStations.Add(lstWaters[i]);
            }
            for (int i = 0; i < lstGates.Count; i++)
            {
                lstStations.Add(lstGates[i]);
            }
            for (int i = 0; i < lstOthers.Count; i++)
            {
                lstStations.Add(lstOthers[i]);
            }
            try
            {
                string sql = @"
                select[CamID]
      ,[CamName]
      ,[StationID]
      ,[X]
      ,[Y]
      ,[StreamMain] from cameras order by stationId
                ";
                DataTable dt = sqlhelper.ExecuteQuery(sql);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SensorMoreData sensor = new SensorMoreData();
                    sensor.errorMessage = "";
                    sensor.sensorType = "CCTV";
                    sensor.sensorTypeName = "CCTV";
                    sensor.areaID = "";
                    sensor.areaName = "";
                    sensor.stationNameA = "";
                    sensor.sensorNameA = dt.Rows[i]["camName"].ToString(); ;
                    sensor.stream = _configuration["VideoImage:Path"];
                    sensor.sensorId = dt.Rows[i]["camId"].ToString();
                    sensor.lastDataTime = "";
                    sensor.lastDataTimePrev = "";
                    sensor.value1 = 0;
                    sensor.value2 = 0;
                    sensor.diff1 = 0;
                    sensor.diff2 = 0;
                    sensor.initValue = 0;
                    sensor.initValue2 = 0;
                    sensor.offset = 0;
                    sensor.offset2 = 0;
                    sensor.status1 = 0;
                    sensor.status2 = 0;
                    sensor.unit = "";
                    sensor.remark = "";
                    lstStations.Add(sensor);

                }
                //Context.Response.Write(jsonStation);
            }
            catch (Exception ex)
            {
                BadRequest(ex.Message);
            }
            return Ok(lstStations.ToList());
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetStationDataByStationName")]
        public IActionResult GetStationDataByStationName(string? stationName)
        {
            List<SensorMoreData> lstStations = new List<SensorMoreData>();
            List<SensorMoreData> lstWaters = new List<SensorMoreData>();
            List<SensorMoreData> lstGates = new List<SensorMoreData>();
            List<SensorMoreData> lstOthers = new List<SensorMoreData>();
            string jsonStation = "";

            SqlHelper sqlhelper = new SqlHelper(_configuration.GetConnectionString("Water2022"));
            List<SqlParameter> lstParameter = new List<SqlParameter>();
            lstParameter.Add(new SqlParameter("stationname", stationName));
            try
            {
                DataTable dt = sqlhelper.ExecuteStoreProcedureQuery("sp_AllSensorsByName", lstParameter.ToArray());
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SensorMoreData sensor = new SensorMoreData();
                    sensor.errorMessage = "";
                    sensor.sensorType = dt.Rows[i]["sensorType"].ToString();
                    sensor.sensorTypeName = dt.Rows[i]["sensorTypeName"].ToString();
                    sensor.areaID = dt.Rows[i]["areaId"].ToString();
                    sensor.areaName = dt.Rows[i]["areaName"].ToString();
                    sensor.stationNameA = dt.Rows[i]["stationnameA"].ToString();
                    sensor.sensorNameA = dt.Rows[i]["sensornameA"].ToString();
                    sensor.sensorId = dt.Rows[i]["sensorId"].ToString();
                    sensor.lastDataTime = dt.Rows[i]["lastdatatime"].ToString();
                    sensor.lastDataTimePrev = dt.Rows[i]["lastdatatimePrev"].ToString();
                    sensor.value1 = decimal.Parse(dt.Rows[i]["value1"].ToString());
                    sensor.value2 = decimal.Parse(dt.Rows[i]["value2"].ToString());
                    sensor.diff1 = decimal.Parse(dt.Rows[i]["diff1"].ToString());
                    sensor.diff2 = decimal.Parse(dt.Rows[i]["diff2"].ToString());
                    sensor.initValue = decimal.Parse(dt.Rows[i]["InitValue"].ToString());
                    sensor.initValue2 = decimal.Parse(dt.Rows[i]["InitValue2"].ToString());
                    sensor.offset = decimal.Parse(dt.Rows[i]["offset"].ToString());
                    sensor.offset2 = decimal.Parse(dt.Rows[i]["offset2"].ToString());
                    sensor.status1 = int.Parse(dt.Rows[i]["status1"].ToString());
                    sensor.status2 = int.Parse(dt.Rows[i]["status2"].ToString());
                    sensor.unit = dt.Rows[i]["unit"].ToString();
                    sensor.remark = dt.Rows[i]["remark"].ToString();
                    sensor.alarm = int.Parse(dt.Rows[i]["alarm"].ToString());
                    int eqGrade = 0;
                    int.TryParse(dt.Rows[i]["eqgrade"].ToString(), out eqGrade);
                    sensor.eqGrade = eqGrade;
                    if (sensor.sensorType.ToLower() == "waterlevel")
                        lstWaters.Add(sensor);
                    else if (sensor.sensorType.ToLower() == "gate")
                        lstGates.Add(sensor);
                    else
                        lstOthers.Add(sensor);
                }
                //jsonStation = JsonConvert.SerializeObject(lstStations);
                //Context.Response.Write(jsonStation);
            }
            catch (Exception ex)
            {
                BadRequest(ex.Message);
                //Context.Response.Write(jsonStation); 
            }
            for (int i = 0; i < lstWaters.Count; i++)
            {
                lstStations.Add(lstWaters[i]);
            }
            for (int i = 0; i < lstGates.Count; i++)
            {
                lstStations.Add(lstGates[i]);
            }
            for (int i = 0; i < lstOthers.Count; i++)
            {
                lstStations.Add(lstOthers[i]);
            }
            try
            {
                string sql = @"
                select[CamID]
      ,[CamName]
      ,a.[StationID]
      ,a.[X]
      ,a.[Y]
      ,[StreamMain],[StationNameA] a
      inner join Stations b on a.StationId=b.stationId from cameras where b.stationName = @stdName
                ";
                lstParameter.Clear();
                lstParameter.Add(new SqlParameter("@stdName", stationName));
                DataTable dt = sqlhelper.ExecuteQuery(sql, lstParameter.ToArray());
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SensorMoreData sensor = new SensorMoreData();
                    sensor.errorMessage = "";
                    sensor.sensorType = "CCTV";
                    sensor.sensorTypeName = "CCTV";
                    sensor.areaID = "";
                    sensor.areaName = "";
                    sensor.stationNameA = dt.Rows[i]["stationNameA"].ToString(); ;
                    sensor.sensorNameA = dt.Rows[i]["camName"].ToString(); ;
                    sensor.stream = _configuration["VideoImage:Path"];
                    sensor.sensorId = dt.Rows[i]["camId"].ToString();
                    sensor.lastDataTime = "";
                    sensor.lastDataTimePrev = "";
                    sensor.value1 = 0;
                    sensor.value2 = 0;
                    sensor.diff1 = 0;
                    sensor.diff2 = 0;
                    sensor.initValue = 0;
                    sensor.initValue2 = 0;
                    sensor.offset = 0;
                    sensor.offset2 = 0;
                    sensor.status1 = 0;
                    sensor.status2 = 0;
                    sensor.unit = "";
                    sensor.remark = "";
                    lstStations.Add(sensor);

                }
                //Context.Response.Write(jsonStation);
            }
            catch (Exception ex)
            {
                BadRequest(ex.Message);
            }
            return Ok(lstStations.ToList());
        }
        */



        [Authorize]
        [HttpPost]
        [Route("GetStationAlertData")]
        public IActionResult GetStationAlertData(int userType) //0:all alarm 1:user alram
        {
            Dictionary<string, AlarmStation> dicAlarms = new Dictionary<string, AlarmStation>();
            string filter = "";

            if (userType == 1)
            {
                filter = " and alarmtype=@userType ";
            }
            //Sensors sensors = new Sensors();
            string json = "";
            SqlHelper sqlhelper = new SqlHelper(_configuration.GetConnectionString("Water2022"));
            string sql = @"
SELECT 
      a.[SensorId]
      ,[RecordTime]
      ,[Event]
      ,[EventDescription]
      ,[Value1]
      ,[Value2]
      ,[RecoverTime]
      ,isnull([AlarmType],1) as alarmtype
      ,[AlarmLevel]
      ,[UpdateTime]
      ,[Sent]
      ,[DataFrom],isnull(b.SensorNameA,'') as sensorNameA 
      ,[AlarmRange],d.stationId,d.stationNameA
	  ,isnull(e.CamName,'') as camName,isnull(e.camid,'') as camid,
	  case when isnull(a.stationCategory,'') = '' then 4
	  else a.stationCategory 
      end as stationCategory,
      isnull(b.sensorid,'') as sensorId2
  FROM [Water2022].[dbo].[Events] a
  left join sensors b on a.SensorId=b.SensorID
  left join Cameras e on a.SensorId=e.CamID
  left join sensortypes c on b.SensorType=c.SensorType
  left join Stations d on (b.stationid=d.StationID or e.StationID=d.StationID)
  where ((RecoverTime is null and (b.SensorType !='earthquake' or b.SensorType is null)) or
  (b.sensortype in ('earthquake') and datediff(minute,recordtime,getdate()) < @EqTimeBefore)) 
  and (isnull(e.camid,'') <> '' or isnull(b.SensorID,'') <> '' )
" + filter;
            

            List<SqlParameter> lstParameter = new List<SqlParameter>();
            lstParameter.Add(new SqlParameter("EqTimeBefore", _configuration["EqTimeBefore:DaysBefore"]));
            if (filter.Length > 0)
            {
                sql += filter;
                lstParameter.Add(new SqlParameter("userType", 1));
            }
            /* keep this maybe use max(recordtime)
             SELECT 
                  a.[SensorId]
                  ,[RecordTime]
                  ,[Event]
                  ,[EventDescription]
                  ,[Value1]
                  ,[Value2]
                  ,[RecoverTime]
                  ,isnull([AlarmType],1) as alarmtype
                  ,[AlarmLevel]
                  ,[UpdateTime]
                  ,[Sent]
                  ,[DataFrom],isnull(b.SensorNameA,'') as sensorNameA 
                  ,[AlarmRange],d.stationId,d.stationNameA
                  ,isnull(e.CamName,'') as camName,isnull(e.camid,'') as camid,
                  case when isnull(a.stationCategory,'') = '' then 4
                  else a.stationCategory 
                  end as stationCategory,
                  isnull(b.sensorid,'') as sensorId2
              FROM [Water2022].[dbo].[Events] a
              inner join (
              select SensorId,max(Recordtime) as rt from [events]
                group by sensorid
              ) r on a.RecordTime=r.rt and a.SensorId=r.SensorId
              left join sensors b on a.SensorId=b.SensorID
              left join Cameras e on a.SensorId=e.CamID
              left join sensortypes c on b.SensorType=c.SensorType
              left join Stations d on (b.stationid=d.StationID or e.StationID=d.StationID)
              where ((RecoverTime is null and (b.SensorType !='earthquake' or b.SensorType is null)) or
              (b.sensortype in ('earthquake') and datediff(minute,recordtime,getdate()) < 1440*10)) 
              and (isnull(e.camid,'') <> '' or isnull(b.SensorID,'') <> '' )
             */
            try
            {
                DataTable dt = sqlhelper.ExecuteQuery(sql,lstParameter.ToArray());
                //alarmType  1:警戒 2:傳輸異常 3:監視器異常
                //stationCategory 1: 淡水 2:員山子 3:堤防
                
                AlarmStation alarmStation = null;
                AlarmData alarmData = null;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string sensorId2 = dt.Rows[i]["sensorId2"].ToString();
                    string camid = dt.Rows[i]["camid"].ToString();

                    //if (sensorId2 == "" && camid == "")
                    //    continue;
                    //int counts = int.Parse(dt.Rows[i]["counts"].ToString());
                    int alarmType = int.Parse(dt.Rows[i]["AlarmType"].ToString());
                    int stationCategory = int.Parse(dt.Rows[i]["stationCategory"].ToString());
                    string sensorId = dt.Rows[i]["sensorId"].ToString();
                    //string camId = dt.Rows[i]["camId"].ToString();
                    string camName = dt.Rows[i]["camName"].ToString();
                    string sensorName = dt.Rows[i]["sensorNameA"].ToString();
                    string stationId = dt.Rows[i]["stationId"].ToString();
                    string stationName = dt.Rows[i]["stationNameA"].ToString();
                    string alarmName = dt.Rows[i]["event"].ToString();

                    alarmData = new AlarmData();
                    alarmData.sensorId = sensorId;
                    alarmData.sensorName = sensorName != "" ? sensorName : camName;
                    alarmData.alarmName = alarmName;
                    alarmData.alarmType = alarmType;
                    //alarmData.sensorType = dt.Rows[i]["sensorType"].ToString();
                    if (!dicAlarms.ContainsKey(stationId))
                    {
                        alarmStation = new AlarmStation();
                        dicAlarms.Add(stationId, alarmStation);
                        alarmStation.lstSensors.Add(alarmData);
                        alarmStation.stationId = stationId;
                        alarmStation.stationName = stationName;
                        alarmStation.stationCategory = stationCategory;

                    }
                    else
                    {
                        alarmStation = dicAlarms[stationId];

                        alarmStation.lstSensors.Add(alarmData);

                    }

                }





                //json = JsonConvert.SerializeObject(dicAlarms.Values);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            //Context.Response.Write(json);
            return Ok(dicAlarms.Values.ToList());
        }
    }
}

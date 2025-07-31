using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Encodings;
using System.Web;
using Windows.Devices.Sensors;
using Wra10Core2023.Models;
using Wra10Core2023.Util;
using static System.Collections.Specialized.BitVector32;
using SqlHelper = Wra10Core2023.Util.SQLHelper;

namespace Wra10Core2023.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorController : ControllerBase
{
    private readonly ILogger<SensorController> _logger;
    private readonly IConfiguration _configuration;
    public SensorController(ILogger<SensorController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Authorize]
    [HttpGet]
    [Route("GetWaterSensorType")]
    public IActionResult GetWaterSensorType()
    {
        DataTable dt;
        List<WaterSensorType> lstSensorType = new List<WaterSensorType>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);

            dt = sqlHelper.ExecuteQuery(@"select [SensorType]
      ,[SensorTypeName]
      ,[SensorTypeSimple]
      ,[Unit] from Sensortypes");


            foreach (DataRow row in dt.Rows)
            {
                WaterSensorType wst = new WaterSensorType
                {
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeSimple = row["SensorTypeSimple"].ToString(),
                    //                  Bit = UInt32.Parse(row["Bit"].ToString()),
                    Unit = row["Unit"].ToString()
                };
                lstSensorType.Add(wst);
            }

            return Ok(lstSensorType.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorTypeById")]
    public IActionResult GetWaterSensorTypeById(string? sensorType)
    {
        DataTable dt;
        List<WaterSensorType> lstSensorType = new List<WaterSensorType>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("SensorType", String.IsNullOrEmpty(sensorType) ? DBNull.Value : sensorType));
            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetSensorTypeById", lstParams.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                WaterSensorType wst = new WaterSensorType
                {
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeSimple = row["SensorTypeSimple"].ToString(),
                    //                  Bit = UInt32.Parse(row["Bit"].ToString()),
                    Unit = row["Unit"].ToString()
                };
                lstSensorType.Add(wst);
            }

            return Ok(lstSensorType.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorTypeByName")]
    public IActionResult GetWaterSensorTypeByName(string? sensorTypeName)
    {
        DataTable dt;
        List<WaterSensorType> lstSensorType = new List<WaterSensorType>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("SensorTypeName", String.IsNullOrEmpty(sensorTypeName) ? DBNull.Value : sensorTypeName));
            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetSensorTypeByName", lstParams.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                WaterSensorType wst = new WaterSensorType
                {
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeSimple = row["SensorTypeSimple"].ToString(),
                    //                  Bit = UInt32.Parse(row["Bit"].ToString()),
                    Unit = row["Unit"].ToString()
                };
                lstSensorType.Add(wst);
            }

            return Ok(lstSensorType.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorTypeBySimple")]
    public IActionResult GetWaterSensorTypeBySimple(string? sensorTypeSimple)
    {
        DataTable dt;
        List<WaterSensorType> lstSensorType = new List<WaterSensorType>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("SensorTypeSimple", String.IsNullOrEmpty(sensorTypeSimple) ? DBNull.Value : sensorTypeSimple));
            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetSensorTypeBySimple", lstParams.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                WaterSensorType wst = new WaterSensorType
                {
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeSimple = row["SensorTypeSimple"].ToString(),
                    //                  Bit = UInt32.Parse(row["Bit"].ToString()),
                    Unit = row["Unit"].ToString()
                };
                lstSensorType.Add(wst);
            }

            return Ok(lstSensorType.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet]
    [Route("GetWaterSensorArea")]
    public IActionResult GetWaterSensorArea()
    {
        DataTable dt;
        List<WaterSensorArea> lstSensorArea = new List<WaterSensorArea>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);

            //dt = sqlHelper.ExecuteQuery(@"select [AreaId],[AreaName] from Areas order by areaId");
            dt = sqlHelper.ExecuteQuery(@"select distinct a.[AreaId],[AreaName] from Areas a 
                                            	inner join Stations  b on a.areaid=b.areaid
	                                            inner join sensors c on c.StationId=b.StationID
                                                order by a.areaId");


            foreach (DataRow row in dt.Rows)
            {
                WaterSensorArea wsa = new WaterSensorArea
                {
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                };
                lstSensorArea.Add(wsa);
            }

            return Ok(lstSensorArea.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet]
    [Route("GetWaterSensorAreaCCTV")]
    public IActionResult GetWaterSensorAreaCCTV()
    {
        DataTable dt;
        List<WaterSensorArea> lstSensorArea = new List<WaterSensorArea>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);

            dt = sqlHelper.ExecuteQuery(@"SELECT distinct a.AreaId,AreaName from areas a 
	inner join Stations b on a.AreaID=b.AreaId
	inner join cameras c  on b.stationid=c.StationID 
    order by a.areaId");


            foreach (DataRow row in dt.Rows)
            {
                WaterSensorArea wsa = new WaterSensorArea
                {
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                };
                lstSensorArea.Add(wsa);
            }

            return Ok(lstSensorArea.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorAreaById")]
    public IActionResult GetWaterSensorAreaById(string? areaId)
    {
        DataTable dt;
        List<WaterSensorArea> lstSensorArea = new List<WaterSensorArea>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("AreaId", String.IsNullOrEmpty(areaId) ? DBNull.Value : areaId));
            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetAreaById", lstParams.ToArray());


            foreach (DataRow row in dt.Rows)
            {
                WaterSensorArea wsa = new WaterSensorArea
                {
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                };
                lstSensorArea.Add(wsa);
            }

            return Ok(lstSensorArea.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorAreaByName")]
    public IActionResult GetWaterSensorAreaByName(string? areaName)
    {
        DataTable dt;
        List<WaterSensorArea> lstSensorArea = new List<WaterSensorArea>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("AreaName", String.IsNullOrEmpty(areaName) ? DBNull.Value : areaName));
            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetAreaByName", lstParams.ToArray());


            foreach (DataRow row in dt.Rows)
            {
                WaterSensorArea wsa = new WaterSensorArea
                {
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                };
                lstSensorArea.Add(wsa);
            }

            return Ok(lstSensorArea.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorBasicDataByAreaId")]
    public IActionResult GetWaterSensorBasicDataByAreaId(string? areaId, bool? isAlarm)
    {
        //List<WaterSensor> lstSensor = new List<WaterSensor>();
        DataTable dt;
        List<WaterSensor> lstSensor = new List<WaterSensor>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("AreaId", String.IsNullOrEmpty(areaId) ? DBNull.Value : areaId));
            lstParams.Add(new SqlParameter("isAlarm", isAlarm == null ? DBNull.Value : isAlarm));

            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetSensorBasicDataByAreaId", lstParams.ToArray());


            foreach (DataRow row in dt.Rows)
            {
                WaterSensor ws = new WaterSensor
                {
                    SensorId = row["SensorId"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorNameA = row["SensorNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    isAlarm = row["isAlarm"] == DBNull.Value ? null : bool.Parse(row["isAlarm"].ToString()),

                };
                lstSensor.Add(ws);
            }

            return Ok(lstSensor.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterEmbankAlarm")]
    public IActionResult GetWaterEmabnkAlarm()
    {
        //List<WaterSensor> lstSensor = new List<WaterSensor>();
        DataTable dt;
        List<EmbakWaterAlarm> lstEmbankAlarm = new List<EmbakWaterAlarm>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);

            dt = sqlHelper.ExecuteQuery(@"Select SensorId,SensorNameA,lastvalue1,lastdatatime,a.StationId,b.StationNameA,HiLimit01 from sensors a 
                                              inner join Stations b on a.stationid=b.stationId and a.importflag like 'EM%' and sensortype='WaterLevel' and lastvalue1 >= hilimit01 ");


            foreach (DataRow row in dt.Rows)
            {
                EmbakWaterAlarm ea = new EmbakWaterAlarm
                {
                    SensorId = row["SensorId"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorNameA = row["SensorNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    LastDataTime = row["lastDataTime"].ToString().Replace("\r", "").Replace("\n", ""),
                    LastValue1 = row["lastValue1"].ToString().Replace("\r", "").Replace("\n", ""),
                    HiLimit01 = row["Hilimit01"].ToString().Replace("\r", "").Replace("\n", ""),
                };
                lstEmbankAlarm.Add(ea);
            }

            return Ok(lstEmbankAlarm.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorBasicDataByAreaName")]
    public IActionResult GetWaterSensorBasicDataByAreaName(string? areaName, bool? isAlarm)
    {
        //List<WaterSensor> lstSensor = new List<WaterSensor>();
        DataTable dt;
        List<WaterSensor> lstSensor = new List<WaterSensor>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("AreaName", String.IsNullOrEmpty(areaName) ? DBNull.Value : areaName));
            lstParams.Add(new SqlParameter("isAlarm", isAlarm == null ? DBNull.Value : isAlarm));

            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetSensorBasicDataByAreaName", lstParams.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                WaterSensor ws = new WaterSensor
                {
                    SensorId = row["SensorId"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorNameA = row["SensorNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    isAlarm = row["isAlarm"] == DBNull.Value ? null : bool.Parse(row["isAlarm"].ToString()),
                };

                lstSensor.Add(ws);
            }

            return Ok(lstSensor.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorBasicDataBySensorId")]
    public IActionResult GetWaterSensorBasicDataBySensorId(string? sensorId, bool? isAlarm)
    {
        DataTable dt;
        List<WaterSensor> lstSensor = new List<WaterSensor>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("sensorId", String.IsNullOrEmpty(sensorId) ? DBNull.Value : sensorId));
            lstParams.Add(new SqlParameter("isAlarm", isAlarm == null ? DBNull.Value : isAlarm));
            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetSensorBasicDataBySensorId", lstParams.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                WaterSensor ws = new WaterSensor
                {
                    SensorId = row["SensorId"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorNameA = row["SensorNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    isAlarm = row["isAlarm"] == DBNull.Value ? null : bool.Parse(row["isAlarm"].ToString()),

                };
                lstSensor.Add(ws);
            }

            return Ok(lstSensor.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorBasicDataBySensorName")]
    public IActionResult GetWaterSensorBasicDataBySensorName(string? sensorName, bool? isAlarm)
    {
        //List<WaterSensor> lstSensor = new List<WaterSensor>();
        DataTable dt;
        List<WaterSensor> lstSensor = new List<WaterSensor>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("SensorName", String.IsNullOrEmpty(sensorName) ? DBNull.Value : sensorName));
            lstParams.Add(new SqlParameter("isAlarm", isAlarm == null ? DBNull.Value : isAlarm));

            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetSensorBasicDataBySensorName", lstParams.ToArray());


            foreach (DataRow row in dt.Rows)
            {
                WaterSensor ws = new WaterSensor
                {
                    SensorId = row["SensorId"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorNameA = row["SensorNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    isAlarm = row["isAlarm"] == DBNull.Value ? null : bool.Parse(row["isAlarm"].ToString()),
                    AlarmLevel = int.Parse(row["AlarmLevel"].ToString().Replace("\r", "").Replace("\n", "")),
                    lastValue1 = row["lastValue1"] == DBNull.Value ? null : decimal.Parse(row["lastValue1"].ToString().Replace("\r", "").Replace("\n", "")),
                    lastValue2 = row["lastValue2"] == DBNull.Value ? null : decimal.Parse(row["lastValue2"].ToString().Replace("\r", "").Replace("\n", "")),
                    lastDataTime = row["lastDataTime"] == DBNull.Value ? null : DateTime.Parse(row["lastDataTime"].ToString().Replace("\r", "").Replace("\n", "")),
                    AlarmName = row["AlarmName"].ToString().Replace("\r", "").Replace("\n", ""),
                };
                lstSensor.Add(ws);
            }

            return Ok(lstSensor.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorBasicDataBySensorType")]
    public IActionResult GetWaterSensorBasicDataBySensorType(string? sensorType, bool? isAlarm)
    {
        DataTable dt;
        List<WaterSensor> lstSensor = new List<WaterSensor>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("SensorType", String.IsNullOrEmpty(sensorType) ? DBNull.Value : sensorType));
            lstParams.Add(new SqlParameter("isAlarm", isAlarm == null ? DBNull.Value : isAlarm));
            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetSensorBasicDataBySensorType", lstParams.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                WaterSensor ws = new WaterSensor
                {
                    SensorId = row["SensorId"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorNameA = row["SensorNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    isAlarm = row["isAlarm"] == DBNull.Value ? null : bool.Parse(row["isAlarm"].ToString()),

                };

                lstSensor.Add(ws);
            }

            return Ok(lstSensor.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorBasicDataBySensorTypeName")]
    public IActionResult GetWaterSensorBasicDataBySensorTypeName(string? sensorTypeName, bool? isAlarm)
    {
        //List<WaterSensor> lstSensor = new List<WaterSensor>();
        DataTable dt;
        List<WaterSensor> lstSensor = new List<WaterSensor>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("SensorTypeName", String.IsNullOrEmpty(sensorTypeName) ? DBNull.Value : sensorTypeName));
            lstParams.Add(new SqlParameter("isAlarm", isAlarm == null ? DBNull.Value : isAlarm));

            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetSensorBasicDataBySensorTypeName", lstParams.ToArray());


            foreach (DataRow row in dt.Rows)
            {
                WaterSensor ws = new WaterSensor
                {
                    SensorId = row["SensorId"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorNameA = row["SensorNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    isAlarm = row["isAlarm"] == DBNull.Value ? null : bool.Parse(row["isAlarm"].ToString()),

                };
                lstSensor.Add(ws);
            }

            return Ok(lstSensor.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorBasicDataByStationId")]
    public IActionResult GetWaterSensorBasicDataByStationId(string? stationId, bool? isAlarm)
    {
        //List<WaterSensor> lstSensor = new List<WaterSensor>();
        DataTable dt;
        List<WaterSensor> lstSensor = new List<WaterSensor>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("StationId", String.IsNullOrEmpty(stationId) ? DBNull.Value : stationId));
            lstParams.Add(new SqlParameter("isAlarm", isAlarm == null ? DBNull.Value : isAlarm));

            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetSensorBasicDataByStationId", lstParams.ToArray());


            foreach (DataRow row in dt.Rows)
            {
                WaterSensor ws = new WaterSensor
                {
                    SensorId = row["SensorId"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorNameA = row["SensorNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    isAlarm = row["isAlarm"] == DBNull.Value ? null : bool.Parse(row["isAlarm"].ToString()),

                };
                lstSensor.Add(ws);
            }

            return Ok(lstSensor.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorBasicDataByStationName")]
    public IActionResult GetWaterSensorBasicDataByStationName(string? stationName, bool? isAlarm)
    {
        //List<WaterSensor> lstSensor = new List<WaterSensor>();
        DataTable dt;
        List<WaterSensor> lstSensor = new List<WaterSensor>();
        string? conn = _configuration.GetConnectionString("Water2022");
        try
        {
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParams = new List<SqlParameter>();
            lstParams.Add(new SqlParameter("StationName", String.IsNullOrEmpty(stationName) ? DBNull.Value : stationName));
            lstParams.Add(new SqlParameter("isAlarm", isAlarm == null ? DBNull.Value : isAlarm));

            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetSensorBasicDataByStationName", lstParams.ToArray());


            foreach (DataRow row in dt.Rows)
            {
                WaterSensor ws = new WaterSensor
                {
                    SensorId = row["SensorId"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorNameA = row["SensorNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaId = row["AreaId"].ToString().Replace("\r", "").Replace("\n", ""),
                    AreaName = row["AreaName"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationId = row["StationId"].ToString().Replace("\r", "").Replace("\n", ""),
                    StationNameA = row["StationNameA"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorType = row["SensorType"].ToString().Replace("\r", "").Replace("\n", ""),
                    SensorTypeName = row["SensorTypeName"].ToString().Replace("\r", "").Replace("\n", ""),
                    isAlarm = row["isAlarm"] == DBNull.Value ? null : bool.Parse(row["isAlarm"].ToString()),

                };
                lstSensor.Add(ws);
            }
            return Ok(lstSensor.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Route("GetSensorByStationId")]
    public IActionResult GetSensorByStationId(string? stationId)
    {
        List<StationSensorData> lstSensors = new List<StationSensorData>();
        try
        {

            SqlHelper sqlhelper = new SqlHelper(_configuration.GetConnectionString("Water2022"));
            List<SqlParameter> lstParameter = new List<SqlParameter>();
            lstParameter.Add(new SqlParameter("stationid", stationId));
            DataTable dt = sqlhelper.ExecuteStoreProcedureQuery("sp_SensorsByStation", lstParameter.ToArray());
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                StationSensorData sensor = new StationSensorData();
                sensor.stationId = stationId;
                sensor.stationNameA = dt.Rows[i]["stationnameA"].ToString();
                sensor.sensorNameA = dt.Rows[i]["sensornameA"].ToString();
                sensor.sensorId = dt.Rows[i]["sensorId"].ToString();
                sensor.lastDataTime = DateTime.Parse(dt.Rows[i]["lastdatatime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                sensor.value1 = decimal.Parse(dt.Rows[i]["value1"].ToString());
                sensor.value2 = decimal.Parse(dt.Rows[i]["value2"].ToString());
                lstSensors.Add(sensor);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);

        }
        return Ok(lstSensors);
    }

    [HttpGet]
    [Route("GetSensorByStationName")]
    public IActionResult GetSensorByStationName(string? stationName)
    {
        List<StationSensorData2> lstSensors = new List<StationSensorData2>();
        try
        {

            SqlHelper sqlhelper = new SqlHelper(_configuration.GetConnectionString("Water2022"));
            List<SqlParameter> lstParameter = new List<SqlParameter>();
            if (String.IsNullOrEmpty(stationName))
            {
                lstParameter.Add(new SqlParameter("stationName", DBNull.Value));
            }
            else
            {

                lstParameter.Add(new SqlParameter("stationName", stationName));
            }
            DataTable dt = sqlhelper.ExecuteStoreProcedureQuery("sp_SensorsByStationName", lstParameter.ToArray());
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                StationSensorData2 sensor = new StationSensorData2();
                sensor.areaName = dt.Rows[i]["areaName"].ToString();
                sensor.stationNameA = dt.Rows[i]["stationnameA"].ToString();
                sensor.sensorNameA = dt.Rows[i]["sensornameA"].ToString();
                sensor.sensorTypeName = dt.Rows[i]["sensorTypeName"].ToString();
                sensor.lastDataTime = DateTime.Parse(dt.Rows[i]["lastdatatime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                sensor.value1 = decimal.Parse(dt.Rows[i]["value1"].ToString());
                sensor.value2 = decimal.Parse(dt.Rows[i]["value2"].ToString());
                sensor.alarmName = dt.Rows[i]["alarmName"].ToString();
                lstSensors.Add(sensor);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);

        }
        return Ok(lstSensors);
    }

    [Authorize]
    [HttpPost]
    [Route("GetSensorMoreDataByStationId")]
    public IActionResult GetSensorMoreDataByStationId(string? stationId)
    {
        List<SensorMoreData> lstStations = new List<SensorMoreData>();
        List<SensorMoreData> lstWaters = new List<SensorMoreData>();
        List<SensorMoreData> lstGates = new List<SensorMoreData>();
        List<SensorMoreData> lstOthers = new List<SensorMoreData>();

        SqlHelper sqlhelper = new SqlHelper(_configuration.GetConnectionString("Water2022"));
        List<SqlParameter> lstParameter = new List<SqlParameter>();
        lstParameter.Add(new SqlParameter("stationid", stationId));
        try
        {
            DataTable dt = sqlhelper.ExecuteStoreProcedureQuery("sp_AllSensorsByStation", lstParameter.ToArray());
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
            return BadRequest(ex.Message);
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
                select [CamID]
      ,[CamName]
      ,a.[StationID]
      ,a.[X]
      ,a.[Y]
      ,[StreamMain],[StationNameA] from cameras a 
      inner join stations b on a.stationid=b.stationId  where a.stationid = @stdid
                ";
            lstParameter.Clear();
            lstParameter.Add(new SqlParameter("@stdid", stationId));
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

                sensor.sensorId = dt.Rows[i]["camId"].ToString();
                sensor.stream = new Uri($"{Request.Scheme}://{Request.Host}/" + _configuration["VirtualVideoImage:path"] + @"/" + sensor.sensorId + ".jpg").ToString();
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
            return BadRequest(ex.Message);
        }
        return Ok(lstStations.ToList());
    }

    [Authorize]
    [HttpPost]
    [Route("GetSensorMoreDataByStationName")]
    public IActionResult GetSensorMoreDataByStationName(string? stationName)
    {
        var lstStations = new List<SensorMoreData>();
        var lstWaters = new List<SensorMoreData>();
        var lstGates = new List<SensorMoreData>();
        var lstOthers = new List<SensorMoreData>();

        SqlHelper sqlhelper = new SqlHelper(_configuration.GetConnectionString("Water2022"));
        List<SqlParameter> lstParameter = new List<SqlParameter>();
        lstParameter.Add(new SqlParameter("stationName", stationName));

        try
        {
            DataTable dt = sqlhelper.ExecuteStoreProcedureQuery("sp_AllSensorsByStationName", lstParameter.ToArray());

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i];
                SensorMoreData sensor = new SensorMoreData();
                sensor.errorMessage = "";
                sensor.sensorType = row["sensorType"].ToString();
                sensor.sensorTypeName = row["sensorTypeName"].ToString();
                sensor.areaID = row["areaId"].ToString();
                sensor.areaName = row["areaName"].ToString();
                sensor.stationNameA = row["stationnameA"].ToString();
                sensor.sensorNameA = row["sensornameA"].ToString();
                sensor.sensorId = row["sensorId"].ToString();
                sensor.lastDataTime = row.GetDate("lastdatatime").ToStandardString();
                sensor.lastDataTimePrev = row.GetDate("lastdatatimePrev").ToStandardString();
                sensor.value1 = decimal.Parse(row["value1"].ToString());
                sensor.value2 = decimal.Parse(row["value2"].ToString());
                sensor.diff1 = decimal.Parse(row["diff1"].ToString());
                sensor.diff2 = decimal.Parse(row["diff2"].ToString());
                sensor.initValue = decimal.Parse(row["InitValue"].ToString());
                sensor.initValue2 = decimal.Parse(row["InitValue2"].ToString());
                sensor.offset = decimal.Parse(row["offset"].ToString());
                sensor.offset2 = decimal.Parse(row["offset2"].ToString());
                sensor.status1 = int.Parse(row["status1"].ToString());
                sensor.status2 = int.Parse(row["status2"].ToString());
                sensor.unit = row["unit"].ToString();
                sensor.remark = row["remark"].ToString();
                sensor.alarm = int.Parse(row["alarm"].ToString());
                int eqGrade = 0;
                int.TryParse(row["eqgrade"].ToString(), out eqGrade);
                sensor.eqGrade = eqGrade;
                if (sensor.sensorType.ToLower() == "waterlevel")
                    lstWaters.Add(sensor);
                else if (sensor.sensorType.ToLower() == "gate")
                    lstGates.Add(sensor);
                else
                    lstOthers.Add(sensor);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
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
      ,[StreamMain],[StationNameA] from cameras a
      inner join Stations b on a.StationId=b.StationId where b.stationNameA = @stdname
                ";
            lstParameter.Clear();
            lstParameter.Add(new SqlParameter("@stdname", stationName));
            DataTable dt = sqlhelper.ExecuteQuery(sql, lstParameter.ToArray());
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                SensorMoreData sensor = new SensorMoreData();
                sensor.errorMessage = "";
                sensor.sensorType = "CCTV";
                sensor.sensorTypeName = "CCTV";
                sensor.areaID = "";
                sensor.areaName = "";
                sensor.stationNameA = dt.Rows[i]["stationnameA"].ToString(); ;
                sensor.sensorNameA = dt.Rows[i]["camName"].ToString(); ;
                sensor.sensorId = dt.Rows[i]["camId"].ToString();
                sensor.stream = new Uri($"{Request.Scheme}://{Request.Host}/" + _configuration["VirtualVideoImage:Path"] + @"/" + sensor.sensorId + ".jpg").ToString();
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
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok(lstStations.ToList());
    }

    [Authorize]
    [HttpPost]
    [Route("GetSensorGeneralQueryData")]
    public string GetSensorGeneralQueryData(int userGroupId, string parameters)
    {
        var lstDatas = new List<WaterSensorQuery>();
        string areas = "";
        string sensorTypes = "";
        if (parameters == "##")
        {
            areas = "";
            sensorTypes = "";
        }
        else
        {
            string[] datas1 = parameters.Split(',');

            for (int i = 0; i < datas1.Length; i++)
            {
                string[] datas2 = datas1[i].Split(';');
                if (datas2[0] == "0")
                {
                    areas += datas2[1] + ",";
                }
                else if (datas2[0] == "1")
                {
                    sensorTypes += datas2[1] + ",";
                }
            }

            if (areas.Length > 2)
            {
                areas = areas.Substring(0, areas.Length - 1);
            }

            if (sensorTypes.Length > 2)
            {
                sensorTypes = sensorTypes.Substring(0, sensorTypes.Length - 1);
            }
        }

        lstDatas.Clear();
        DataTable dt = null;
        string? conn = _configuration.GetConnectionString("Water2022");
        SqlHelper sqlHelper = new SqlHelper(conn); ;
        var lstParam = new List<SqlParameter>();
        lstParam.Add(new SqlParameter("areaid", areas == "" ? DBNull.Value : areas));
        lstParam.Add(new SqlParameter("sensortype", sensorTypes == "" ? DBNull.Value : sensorTypes));
        int userType = userGroupId;
        lstParam.Add(new SqlParameter("userType", userType));
        dt = sqlHelper.ExecuteStoreProcedureQuery("sp_SensorQuery2022", lstParam.ToArray());

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            WaterSensorQuery data = new WaterSensorQuery();
            data.userType = userType;
            data.serialNo = (i + 1).ToString();
            //data.fIndex = int.Parse(dt.Rows[i]["f_index"].ToString());
            data.sensorId = dt.Rows[i]["sensorid"].ToString();
            data.areaName = dt.Rows[i]["areaname"].ToString();
            data.stationName = dt.Rows[i]["stationnameA"].ToString();
            data.sensorTypeName = dt.Rows[i]["sensortypename"].ToString();
            data.sensorType = dt.Rows[i]["sensortype"].ToString();
            data.sensorName = dt.Rows[i]["sensorNameA"].ToString();
            data.lastDataTime = dt.Rows[i]["lastdatatime"].ToString();
            data.status = dt.Rows[i]["status"].ToString();
            data.x = dt.Rows[i]["x"].ToString();
            data.y = dt.Rows[i]["y"].ToString();
            data.unit = dt.Rows[i]["unit"].ToString();
            data.eqGrade = dt.Rows[i]["eqgrade"].ToString();
            double value1 = 0;
            double.TryParse(dt.Rows[i]["lastValue1"].ToString(), out value1);
            double value2 = 0;
            double.TryParse(dt.Rows[i]["lastValue2"].ToString(), out value2);
            double value1Prev = 0;
            double.TryParse(dt.Rows[i]["lastValue1Prev"].ToString(), out value1Prev);
            double value2Prev = 0;
            double.TryParse(dt.Rows[i]["lastValue2Prev"].ToString(), out value2Prev);
            double differ1 = value1 - value1Prev;
            string direction1 = "", direction2 = "";
            if (differ1 > 0)
            {
                direction1 = "images/up128.png";
                data.differ1 = 1;
            }
            else if (differ1 == 0)
            {
                direction1 = "images/remove128.png";
                data.differ1 = 0;
            }
            else
            {
                direction1 = "images/down128.png";
                data.differ1 = -1;
            }

            data.value = value1.ToString("0.00") + " " + data.unit;
            data.direction = direction1;

            if (data.sensorType.ToLower() == "slope")
            {

                double differ2 = value2 - value2Prev;
                if (differ2 > 0)
                {
                    direction2 = "images/up128.png";
                    data.differ2 = 1;
                }
                else if (differ2 == 0)
                {
                    direction2 = "images/remove128.png";
                    data.differ2 = 0;

                }
                else
                {
                    direction2 = "images/down128.png";
                    data.differ2 = -1;
                }
                data.value += " , " + value2.ToString("0.00") + " " + data.unit;
                data.direction += " , " + direction2;
            }

            if (data.sensorType.ToLower() == "earthquake")
            {
                data.value = data.eqGrade;
            }

            data.more = "詳細資料..";
            lstDatas.Add(data);
        }

        return JsonConvert.SerializeObject(lstDatas);
    }

    private class AlarmLines
    {
        public string ColumnName, Label;
        public Func<SensorChartParameter, string> GetBackgroundColor, GetBorderColor;
    }

    private static AlarmLines[] AlarmLinesDef =
    [
        new()
        {
            ColumnName = "HiLimit01",
            Label = "三級警戒(高)",
            GetBackgroundColor = p => p.backgroundColorLevel3,
            GetBorderColor = p => p.borderColorLevel3,
        },
        new()
        {
            ColumnName = "HiLimit02",
            Label = "二級警戒(高)",
            GetBackgroundColor = p => p.backgroundColorLevel2,
            GetBorderColor = p => p.borderColorLevel2,
        },
        new()
        {
            ColumnName = "HiLimit03",
            Label = "一級警戒(高)",
            GetBackgroundColor = p => p.backgroundColorLevel1,
            GetBorderColor = p => p.borderColorLevel1,
        },
        new()
        {
            ColumnName = "LoLimit01",
            Label = "三級警戒(低)",
            GetBackgroundColor = p => p.backgroundColorLevel3,
            GetBorderColor = p => p.borderColorLevel3,
        },
        new()
        {
            ColumnName = "LoLimit02",
            Label = "二級警戒(低)",
            GetBackgroundColor = p => p.backgroundColorLevel2,
            GetBorderColor = p => p.borderColorLevel2,
        },
        new()
        {
            ColumnName = "LoLimit03",
            Label = "一級警戒(低)",
            GetBackgroundColor = p => p.backgroundColorLevel1,
            GetBorderColor = p => p.borderColorLevel1,
        },
    ];

    [Authorize]
    [HttpPost]
    [Route("GetSensorChartData")]
    public async Task<string> GetSensorChartData(SensorChartParameter param)
    {
        string sensorId = param.sensorId;
        string begin = param.begin;
        string end = param.end;
        int duration = param.duration;
        if (duration < 10) duration = 10;
        var st = new SensorQueryStation();
        st.chart.lstData.Add(new ListData());
        string? conn = _configuration.GetConnectionString("Water2022");
        var sqlHelper = new SqlHelper(conn);
        var lstParam = new List<SqlParameter>();
        lstParam.Add(new SqlParameter("@sensorid", sensorId));
        lstParam.Add(new SqlParameter("@date1", begin));
        lstParam.Add(new SqlParameter("@date2", end));
        lstParam.Add(new SqlParameter("@duration", duration));

        var dt = sqlHelper.ExecuteStoreProcedureQuery("sp_SensorChart", lstParam.ToArray());
        bool hasSecondValue = false;
        bool hasThirdValue = false;

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            var row = dt.Rows[i];
            var name = row.GetStr("SensorNameA");
            var t = row.GetDate("RecordTime");
            var sensorType = row.GetStr("SensorType").ToLowerInvariant();

            if (i == 0)
            {
                var tFirst = t;
                var tEnd = dt.Rows[dt.Rows.Count - 1].GetDate("RecordTime");
                st.sensorId = row["SensorId"].ToString();
                st.sensorName = name;
                st.unit = row["unit"].ToString();
                st.sensorTypeName = row["SensorTypeName"].ToString();
                st.chart.sensorId = row["SensorId"].ToString();
                st.chart.main = row["areaName"].ToString() + " " + row["stationNameA"].ToString();
                st.chart.xLabel = "時間";

                st.chart.yLabel = sensorType == "earthquake"
                    ? st.sensorTypeName + st.unit
                    : st.sensorTypeName + "(" + st.unit + ")";

                st.chart.lstData[0].label = name;
                st.chart.lstData[0].fill = true;
                st.chart.chartTitle = name;

                if (row["Value3"] != DBNull.Value)
                {
                    hasSecondValue = true;
                    hasThirdValue = true;
                    st.chart.lstData[0].label = name + "-X(" + row["XAxis"].ToString() + ")";
                    st.chart.lstData[0].fill = true;
                    st.chart.lstData[0].backgroundColor = param.backgroundColorValue1;
                    st.chart.lstData[0].borderColor = param.borderColorValue1;

                    st.chart.lstData.Add(new ListData());
                    st.chart.lstData[1].label = name + "-Y(" + row["YAxis"].ToString() + ")";
                    st.chart.lstData[1].fill = true;
                    st.chart.lstData[1].backgroundColor = param.backgroundColorValue2;
                    st.chart.lstData[1].borderColor = param.borderColorValue2;

                    st.chart.lstData.Add(new ListData());
                    st.chart.lstData[2].label = name + "-Z(" + row["ZAxis"].ToString() + ")"; ;
                    st.chart.lstData[2].fill = true;
                    st.chart.lstData[2].backgroundColor = param.backgroundColorValue3;
                    st.chart.lstData[2].borderColor = param.borderColorValue3;
                }
                else if (row["Value2"] != DBNull.Value)
                {
                    hasSecondValue = true;
                    st.chart.lstData[0].label = name + "-X(" + row["XAxis"].ToString() + ")"; ;
                    st.chart.lstData[0].fill = true;
                    st.chart.lstData[0].backgroundColor = param.backgroundColorValue1;
                    st.chart.lstData[0].borderColor = param.borderColorValue1;

                    st.chart.lstData.Add(new ListData());
                    st.chart.lstData[1].label = name + "-Y(" + row["YAxis"].ToString() + ")"; ;
                    st.chart.lstData[1].fill = true;
                    st.chart.lstData[1].backgroundColor = param.backgroundColorValue2;
                    st.chart.lstData[1].borderColor = param.borderColorValue2;
                }
                else
                {
                    var addAxisLabel = sensorType == "earthquake" || sensorType == "slope";
                    st.chart.lstData[0].label = addAxisLabel ? $"{name}-X" : name;
                    st.chart.lstData[0].fill = true;
                    st.chart.lstData[0].backgroundColor = param.backgroundColorValue1;
                    st.chart.lstData[0].borderColor = param.borderColorValue1;
                }

                AddAlarmLines(param, st, row, tFirst, tEnd);

                if (sensorType == "crack")
                {
                    var y = row.GetDouble("InitValue");

                    var line = new ListData()
                    {
                        label = "初始值",
                        data = [new(tFirst, y), new(tEnd, y)],
                        fill = true,
                        backgroundColor = param.backgroundColorLevel1,
                        borderColor = param.borderColorLevel1,
                    };

                    st.chart.lstData.Add(line);
                }
            }

            var wd = new Data(t, double.Parse(row["value1"].ToString()));
            st.chart.lstData[0].data.Add(wd);
            st.chart.lstData[0].fill = true;

            if (hasSecondValue)
            {
                wd = new Data();
                wd.y = double.Parse(row["value2"].ToString());
                wd.x = t;
                st.chart.lstData[1].data.Add(wd);
                st.chart.lstData[1].fill = true;
            }

            if (hasThirdValue)
            {
                wd = new Data();
                wd.y = double.Parse(row["value3"].ToString());
                wd.x = t;
                st.chart.lstData[2].data.Add(wd);
                st.chart.lstData[2].fill = true;
            }
        }

        return JsonConvert.SerializeObject(st.chart);
    }

    private static void AddAlarmLines(SensorChartParameter param, SensorQueryStation st, 
        DataRow row, DateTime tFirst, DateTime tEnd)
    {
        foreach (var def in AlarmLinesDef)
        {
            if (row.IsNull(def.ColumnName)) continue;
            var y = (double)row.GetDecimal(def.ColumnName);

            var line = new ListData()
            {
                label = def.Label,
                data = [new(tFirst, y), new(tEnd, y)],
                fill = true,
                backgroundColor = def.GetBackgroundColor(param),
                borderColor = def.GetBorderColor(param),
            };

            st.chart.lstData.Add(line);
        }
    }

    [Authorize]
    [HttpGet]
    [Route("ResetSensorAlarm")]
    public IActionResult ResetSensorAlarm(string sensorId)
    {
        try
        {
            string? conn = _configuration.GetConnectionString("Water2022");
            SqlHelper sqlHelper = new SqlHelper(conn);
            sqlHelper.ExecuteNonQuery("update events set recovertime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where sensorid='" + sensorId + "' and recovertime is null");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok();
    }

    [Authorize]
    [HttpPost]
    [Route("GetWaterSensorDataByFuzzy")]
    public IActionResult GetWaterSensorDataByFuzzy(string? areaId, string? areaName, string? stationId,
        string? stationName, string? timeBegin, string? timeEnd, bool? isAlarm)
    {
        try
        {
            List<WaterSensorDataQuery> lstDatas = new List<WaterSensorDataQuery>();
            DataTable dt = null;
            string? conn = _configuration.GetConnectionString("Water2022");
            SqlHelper sqlHelper = new SqlHelper(conn); ;
            List<SqlParameter> lstParam = new List<SqlParameter>();


            lstParam.Add(new SqlParameter("@AreaId", String.IsNullOrEmpty(areaId) ? DBNull.Value : areaId));
            lstParam.Add(new SqlParameter("@AreaName", String.IsNullOrEmpty(areaName) ? DBNull.Value : areaName));
            lstParam.Add(new SqlParameter("@StationId", String.IsNullOrEmpty(stationId) ? DBNull.Value : stationId));
            lstParam.Add(new SqlParameter("@StationName", String.IsNullOrEmpty(stationName) ? DBNull.Value : stationName));
            lstParam.Add(new SqlParameter("@begin", String.IsNullOrEmpty(timeBegin) ? DBNull.Value : timeBegin));
            lstParam.Add(new SqlParameter("@end", String.IsNullOrEmpty(timeEnd) ? DBNull.Value : timeEnd));
            lstParam.Add(new SqlParameter("@isAlarm", isAlarm == null ? DBNull.Value : isAlarm));
            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_SensorQueryFuzzy2", lstParam.ToArray());
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                float x = 0, y = 0;
                WaterSensorDataQuery data = new WaterSensorDataQuery();
                data.SensorId = dt.Rows[i]["sensorid"].ToString();
                data.AreaId = dt.Rows[i]["areaid"].ToString();
                data.AreaName = dt.Rows[i]["areaname"].ToString();
                data.StationId = dt.Rows[i]["stationId"].ToString();
                data.StationName = dt.Rows[i]["stationnameA"].ToString();
                data.SensorTypeName = dt.Rows[i]["sensortypename"].ToString();
                data.SensorType = dt.Rows[i]["sensortype"].ToString();
                data.SensorName = dt.Rows[i]["sensorNameA"].ToString();
                data.RecordTime = dt.Rows[i]["RecordTime"].ToString();
                float.TryParse(dt.Rows[i]["x"].ToString(), out x);
                data.X = x;
                float.TryParse(dt.Rows[i]["y"].ToString(), out y);
                data.Y = y;
                data.Unit = dt.Rows[i]["unit"].ToString();
                decimal value1 = 0;
                decimal.TryParse(dt.Rows[i]["Value1"].ToString(), out value1);
                data.Value1 = value1;
                decimal value2 = 0;
                decimal.TryParse(dt.Rows[i]["Value2"].ToString(), out value2);
                data.Value2 = value2;
                data.AlarmLevel = dt.Rows[i]["AlarmLevel"].ToString();

                float initValue = (float)0.0;
                float.TryParse(dt.Rows[i]["InitValue"].ToString(), out initValue);
                float initValue2 = (float)0.0;
                float.TryParse(dt.Rows[i]["InitValue2"].ToString(), out initValue2);
                float offset = (float)0.0;
                float.TryParse(dt.Rows[i]["offset"].ToString(), out offset);
                float offset2 = (float)0.0;
                float.TryParse(dt.Rows[i]["offset2"].ToString(), out offset2);

                decimal Sensitivity1 = (decimal)0.0;
                decimal.TryParse(dt.Rows[i]["Sensitivity1"].ToString(), out Sensitivity1);
                decimal Sensitivity2 = (decimal)0.0;
                decimal.TryParse(dt.Rows[i]["Sensitivity2"].ToString(), out Sensitivity2);

                decimal ExtraOffset1 = (decimal)0.0;
                decimal.TryParse(dt.Rows[i]["ExtraOffset1"].ToString(), out ExtraOffset1);
                decimal ExtraOffset2 = (decimal)0.0;
                decimal.TryParse(dt.Rows[i]["ExtraOffset2"].ToString(), out ExtraOffset2);

                decimal RawValue11 = (decimal)0.0;
                decimal.TryParse(dt.Rows[i]["RawValue11"].ToString(), out RawValue11);
                decimal RawValue12 = (decimal)0.0;
                decimal.TryParse(dt.Rows[i]["RawValue12"].ToString(), out RawValue12);
                lstDatas.Add(data);
            }
            return Ok(lstDatas.ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

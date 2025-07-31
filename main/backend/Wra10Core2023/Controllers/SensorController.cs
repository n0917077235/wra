using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using SqlHelper = SQLHelper.SQLHelper;
//using Dapper;
//using Azure.Core;
using Microsoft.AspNetCore.Hosting.Server;
using Newtonsoft.Json.Linq;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Wra10Core2023.Models;
using Newtonsoft.Json;
using static System.Collections.Specialized.BitVector32;
using System;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Text;
using Windows.Devices.Sensors;
using System.IO;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Web;
using Org.BouncyCastle.Utilities;
using System.Text.Encodings;
using Wra10Core2023.Util;


namespace Wra10Core2023.Controllers
{

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
            DataTable dt ;
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
            catch(Exception ex)
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
            catch(Exception ex)
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
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        
        [Authorize]
        [HttpPost]
        [Route("GetWaterSensorBasicDataBySensorId")]
        public IActionResult GetWaterSensorBasicDataBySensorId(string? sensorId, bool? isAlarm)
        {
            //List<WaterSensor> lstSensor = new List<WaterSensor>();
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
            catch(Exception ex)
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
            catch(Exception ex)
            {
                return BadRequest(ex.Message);  
            }

        }

        [Authorize]
        [HttpPost]
        [Route("GetWaterSensorBasicDataBySensorType")]
        public IActionResult GetWaterSensorBasicDataBySensorType(string? sensorType, bool? isAlarm)
        {
            //List<WaterSensor> lstSensor = new List<WaterSensor>();
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
            catch(Exception ex)
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
            catch(Exception ex)
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
            catch(Exception ex)
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
            catch(Exception ex)
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
            catch(Exception ex)
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
                    sensor.alarmName= dt.Rows[i]["alarmName"].ToString();
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
            string jsonStation = "";

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
                    sensor.stream= new Uri($"{Request.Scheme}://{Request.Host}/" + _configuration["VirtualVideoImage:Path"] + @"/" + sensor.sensorId+".jpg").ToString();
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
            List<WaterSensorQuery> lstDatas = new List<WaterSensorQuery>();
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
                    //areas = "'" + areas + "'";
                }
                if (sensorTypes.Length > 2)
                {
                    sensorTypes = sensorTypes.Substring(0, sensorTypes.Length - 1);
                    //sensorTypes = "'" + sensorTypes + "'";
                }
            }
            lstDatas.Clear();
            DataTable dt = null;
            string? conn = _configuration.GetConnectionString("Water2022");
            SqlHelper sqlHelper = new SqlHelper(conn); ;
            List<SqlParameter> lstParam = new List<SqlParameter>();

            //if (stations[0] == "" || stations[1] == "1")

            lstParam.Add(new SqlParameter("areaid", areas==""?DBNull.Value:areas));
            lstParam.Add(new SqlParameter("sensortype", sensorTypes=="" ? DBNull.Value:sensorTypes));
            int userType = userGroupId;
            lstParam.Add(new SqlParameter("userType", userType));
            dt = sqlHelper.ExecuteStoreProcedureQuery("sp_SensorQuery2022", lstParam.ToArray());
            string preStation = "";
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

        [Authorize]
        [HttpPost]
        [Route("GetSensorChartData")]
        public async Task<string> GetSensorChartData(SensorChartParameter param)
        {
            //string id = param.id;
            string sensorId = param.sensorId;
            //string sensorType = param.sensorType;
            string begin = param.begin;
            string end = param.end;
            int duration = param.duration;
            if (duration < 10)
                duration = 10;
            //JsonParams jp=JsonConvert.DeserializeObject<JsonParams>(jsonParams);
            //List<ListData> lstData = new List<ListData>();
            string[] arrColor = new string[] { "#231F20", "#FFC200", "#F44937", "#16F27E", "#FC9775", "#5A69A6", "#231F20", "#FFC200", "#F44937", "#16F27E", "#FC9775", "#5A69A6", "#231F20", "#FFC200", "#F44937", "#16F27E", "#FC9775", "#5A69A6", "#231F20", "#FFC200", "#F44937", "#16F27E", "#FC9775", "#5A69A6" };
            SensorQueryStation st = new SensorQueryStation();
            st.chart.lstData.Add(new ListData());
            string? conn = _configuration.GetConnectionString("Water2022");
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParam = new List<SqlParameter>();
            //string[] stations = stationId.Split(',');
            lstParam.Add(new SqlParameter("@sensorid", sensorId));
            lstParam.Add(new SqlParameter("@date1", begin));
            lstParam.Add(new SqlParameter("@date2", end));
            lstParam.Add(new SqlParameter("@duration", duration));

            DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_SensorChart", lstParam.ToArray());
            bool bValue2 = false;
            bool bValue3 = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i == 0)
                {
                    DateTime tFirst = DateTime.Parse(dt.Rows[i]["RecordTime"].ToString());
                    DateTime tEnd = DateTime.Parse(dt.Rows[dt.Rows.Count - 1]["RecordTime"].ToString());
                    st.sensorId = dt.Rows[i]["SensorId"].ToString();
                    st.sensorName = dt.Rows[i]["SensorNameA"].ToString();
                    st.unit = dt.Rows[i]["unit"].ToString();
                    st.sensorTypeName = dt.Rows[i]["SensorTypeName"].ToString();
                    string sensorType= dt.Rows[i]["SensorType"].ToString();
                    st.chart.sensorId = dt.Rows[i]["SensorId"].ToString();
                    st.chart.main = dt.Rows[i]["areaName"].ToString() + " " + dt.Rows[i]["stationNameA"].ToString();
                    st.chart.xLabel = "時間";
                    if (sensorType.ToLower()=="earthquake")
                        st.chart.yLabel = st.sensorTypeName + st.unit ;
                    else
                        st.chart.yLabel = st.sensorTypeName + "(" + st.unit + ")";

                    st.chart.lstData[0].label = dt.Rows[i]["SensorNameA"].ToString();
                    st.chart.lstData[0].fill = true;
                    st.chart.chartTitle = dt.Rows[i]["SensorNameA"].ToString();// + " " + dt.Rows[i]["StationNameA"].ToString();
                    
                    if (dt.Rows[i]["Value3"] != DBNull.Value)
                    {
                        bValue2 = true;
                        bValue3 = true;
                        st.chart.lstData[0].label = dt.Rows[i]["SensorNameA"].ToString() + "-X(" + dt.Rows[i]["XAxis"].ToString() + ")";
                        st.chart.lstData[0].fill = true;
                        st.chart.lstData[0].backgroundColor = param.backgroundColorValue1;
                        st.chart.lstData[0].borderColor = param.borderColorValue1;

                        st.chart.lstData.Add(new ListData());
                        st.chart.lstData[1].label = dt.Rows[i]["SensorNameA"].ToString() + "-Y(" + dt.Rows[i]["YAxis"].ToString() + ")";
                        st.chart.lstData[1].fill = true;
                        st.chart.lstData[1].backgroundColor = param.backgroundColorValue2;
                        st.chart.lstData[1].borderColor = param.borderColorValue2;

                        st.chart.lstData.Add(new ListData());
                        st.chart.lstData[2].label = dt.Rows[i]["SensorNameA"].ToString() + "-Z(" + dt.Rows[i]["ZAxis"].ToString() + ")"; ;
                        st.chart.lstData[2].fill = true;
                        st.chart.lstData[2].backgroundColor = param.backgroundColorValue3;
                        st.chart.lstData[2].borderColor = param.borderColorValue3;
                        //st.chart.lstData.Add(new ListData());
                    }
                    else if (dt.Rows[i]["Value2"] != DBNull.Value)
                    {
                        bValue2 = true;
                        st.chart.lstData[0].label = dt.Rows[i]["SensorNameA"].ToString() + "-X(" + dt.Rows[i]["XAxis"].ToString() + ")"; ;
                        st.chart.lstData[0].fill = true;
                        st.chart.lstData[0].backgroundColor = param.backgroundColorValue1;
                        st.chart.lstData[0].borderColor = param.borderColorValue1;

                        st.chart.lstData.Add(new ListData());
                        st.chart.lstData[1].label = dt.Rows[i]["SensorNameA"].ToString() + "-Y(" + dt.Rows[i]["YAxis"].ToString() + ")"; ;
                        st.chart.lstData[1].fill = true;
                        st.chart.lstData[1].backgroundColor = param.backgroundColorValue2;
                        st.chart.lstData[1].borderColor = param.borderColorValue2;
                        //st.chart.lstData.Add(new ListData());
                    }
                    else
                    {
                        if (sensorType.ToLower() == "earthquake" || sensorType.ToLower() == "slope")
                            st.chart.lstData[0].label = dt.Rows[i]["SensorNameA"].ToString() + "-X";
                        else
                            st.chart.lstData[0].label = dt.Rows[i]["SensorNameA"].ToString();
                        st.chart.lstData[0].fill = true;
                        st.chart.lstData[0].backgroundColor = param.backgroundColorValue1;
                        st.chart.lstData[0].borderColor = param.borderColorValue1;

                    }
                    if (dt.Rows[i]["HiLimit01"]!=DBNull.Value)
                    {
                        st.chart.lstData.Add(new ListData());
                        Data wdAlarm = new Data();
                        wdAlarm.x = tFirst;
                        wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit01"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].label = "三級警戒(高)";
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                        st.chart.lstData[st.chart.lstData.Count - 1].fill=true;
                        st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel3;
                        st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel3;
                        wdAlarm = new Data();
                        wdAlarm.x = tEnd;
                        wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit01"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                    }
                    if (dt.Rows[i]["HiLimit02"] != DBNull.Value)
                    {
                        st.chart.lstData.Add(new ListData());
                        Data wdAlarm = new Data();
                        wdAlarm.x = tFirst;
                        wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit02"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].label = "二級警戒(高)";
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                        st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                        st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel2;
                        st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel2;
                        wdAlarm = new Data();
                        wdAlarm.x = tEnd;
                        wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit02"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                    }
                    if (dt.Rows[i]["HiLimit03"] != DBNull.Value)
                    {
                        st.chart.lstData.Add(new ListData());
                        Data wdAlarm = new Data();
                        wdAlarm.x = tFirst;
                        wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit03"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].label = "一級警戒(高)";
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                        st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                        st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel1;
                        st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel1;
                        wdAlarm = new Data();
                        wdAlarm.x = tEnd;
                        wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit03"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                    }
                    if (dt.Rows[i]["LoLimit01"] != DBNull.Value)
                    {
                        st.chart.lstData.Add(new ListData());
                        Data wdAlarm = new Data();
                        wdAlarm.x = tFirst;
                        wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit01"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].label = "三級警戒(低)";
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                        st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                        st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel3;
                        st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel3;
                        wdAlarm = new Data();
                        wdAlarm.x = tEnd;
                        wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit01"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                    }
                    if (dt.Rows[i]["LoLimit02"] != DBNull.Value)
                    {
                        st.chart.lstData.Add(new ListData());
                        Data wdAlarm = new Data();
                        wdAlarm.x = tFirst;
                        wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit02"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].label = "二級警戒(低)";
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                        st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                        st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel2;
                        st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel2;
                        wdAlarm = new Data();
                        wdAlarm.x = tEnd;
                        wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit02"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                    }
                    if (dt.Rows[i]["LoLimit03"] != DBNull.Value)
                    {
                        st.chart.lstData.Add(new ListData());
                        Data wdAlarm = new Data();
                        wdAlarm.x = tFirst;
                        wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit03"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].label = "一級警戒(低)";
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                        st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                        st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel1;
                        st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel1;
                        wdAlarm = new Data();
                        wdAlarm.x = tEnd;
                        wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit03"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                    }
                    if (dt.Rows[i]["SensorType"].ToString().ToLower() == "crack")
                    {
                        st.chart.lstData.Add(new ListData());
                        Data wdInitValue = new Data();
                        wdInitValue.x = tFirst;
                        wdInitValue.y = double.Parse(dt.Rows[i]["InitValue"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].label = "初始值";
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdInitValue);
                        st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                        st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel1;
                        st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel1;
                        wdInitValue = new Data();
                        wdInitValue.x = tEnd;
                        wdInitValue.y = double.Parse(dt.Rows[i]["InitValue"].ToString());
                        st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdInitValue);
                    }
                }
                Data wd = new Data();
                wd.y = double.Parse(dt.Rows[i]["value1"].ToString());
                wd.x = DateTime.Parse(dt.Rows[i]["RecordTime"].ToString());
                st.chart.lstData[0].data.Add(wd);
                st.chart.lstData[0].fill = true;
                if (bValue2)
                {
                    wd = new Data();
                    wd.y = double.Parse(dt.Rows[i]["value2"].ToString());
                    wd.x = DateTime.Parse(dt.Rows[i]["RecordTime"].ToString());
                    st.chart.lstData[1].data.Add(wd);
                    //st.chart.lstData[1].label = "一級警戒(低)";
                    st.chart.lstData[1].fill = true;
                }
                if (bValue3)
                {
                    wd = new Data();
                    wd.y = double.Parse(dt.Rows[i]["value3"].ToString());
                    wd.x = DateTime.Parse(dt.Rows[i]["RecordTime"].ToString());
                    st.chart.lstData[2].data.Add(wd);
                    //st.chart.lstData[1].label = "一級警戒(低)";
                    st.chart.lstData[2].fill = true;
                }
            }

            return JsonConvert.SerializeObject(st.chart);
        }

        [Authorize]
        [HttpPost]
        [Route("GetSensorChartDataToCSV")]
        public async Task<IActionResult> GetSensorChartDataToCsv(SensorChartParameter param)
        {
            //string id = param.id;
            string sensorId = param.sensorId;
            //string sensorType = param.sensorType;
            string begin = param.begin;
            string end = param.end;
            int duration = param.duration;
            if (duration < 10)
                duration = 10;
            //JsonParams jp=JsonConvert.DeserializeObject<JsonParams>(jsonParams);
            //List<ListData> lstData = new List<ListData>();
            SensorQueryStation st = new SensorQueryStation();
            st.chart.lstData.Add(new ListData());
            string? conn = _configuration.GetConnectionString("Water2022");
            SqlHelper sqlHelper = new SqlHelper(conn);
            List<SqlParameter> lstParam = new List<SqlParameter>();
            //string[] stations = stationId.Split(',');
            lstParam.Add(new SqlParameter("@sensorid", sensorId));
            lstParam.Add(new SqlParameter("@date1", begin));
            lstParam.Add(new SqlParameter("@date2", end));
            lstParam.Add(new SqlParameter("@duration", duration));

            DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_SensorChart", lstParam.ToArray());
            bool bValue2 = false;
            bool bValue3 = false;

            using var memoryStream = new MemoryStream();


            string sensorId0 = "";
            string sensorName = "";
            string range = "";
            string sensorType = "";
            var csvBuilder = new StringBuilder();
            try
            {
                System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // 註冊Big5等編碼
                Encoding big5Enc = Encoding.GetEncoding("big5");

                using (var writer = new StreamWriter(memoryStream, big5Enc))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            sensorId0 = dt.Rows[i]["SensorId"].ToString();
                            sensorName = dt.Rows[i]["SensorNameA"].ToString();
                            DateTime tFirst = DateTime.Parse(dt.Rows[i]["RecordTime"].ToString());
                            DateTime tEnd = DateTime.Parse(dt.Rows[dt.Rows.Count - 1]["RecordTime"].ToString());

                            range = "(" + tFirst.ToString("yyyy/MM/dd HH:mm:ss") + "-" + tEnd.ToString("yyyy/MM/dd HH:mm:ss") + ")";


                            string unit = dt.Rows[i]["unit"].ToString();
                            sensorType = dt.Rows[i]["SensorType"].ToString();

                            if (sensorType.ToLower() == "earthquake")
                            {
                                csvBuilder.AppendLine($"時間,X({unit}),Y({unit}),Z({unit})");
                                //writer.WriteLine($"時間,X({unit}),Y({unit}),Z({unit})");
                            }
                            else if (sensorType.ToLower() == "slope")
                            {
                                csvBuilder.AppendLine($"時間,X({unit}),Y({unit})");
                                //writer.WriteLine($"時間,X({unit}),Y({unit})");
                            }
                            else if (sensorType.ToLower() == "crack")
                            {
                                csvBuilder.AppendLine($"時間,X({unit}),初始值({unit})");
                                //writer.WriteLine($"時間,X({unit}),初始值({unit})");
                            }
                            else
                            {
                                csvBuilder.AppendLine($"時間,X({unit})");
                                //writer.WriteLine($"時間,X({unit})");
                            }
                            //st.chart.chartTitle = dt.Rows[i]["SensorNameA"].ToString();// + " " + dt.Rows[i]["StationNameA"].ToString();

                            if (dt.Rows[i]["Value3"] != DBNull.Value)
                            {
                                bValue2 = true;
                                bValue3 = true;
                                /*
                                st.chart.lstData[0].label = dt.Rows[i]["SensorNameA"].ToString() + "-X(" + dt.Rows[i]["XAxis"].ToString() + ")";
                                st.chart.lstData[0].fill = true;
                                st.chart.lstData[0].backgroundColor = param.backgroundColorValue1;
                                st.chart.lstData[0].borderColor = param.borderColorValue1;

                                st.chart.lstData.Add(new ListData());
                                st.chart.lstData[1].label = dt.Rows[i]["SensorNameA"].ToString() + "-Y(" + dt.Rows[i]["YAxis"].ToString() + ")";
                                st.chart.lstData[1].fill = true;
                                st.chart.lstData[1].backgroundColor = param.backgroundColorValue2;
                                st.chart.lstData[1].borderColor = param.borderColorValue2;

                                st.chart.lstData.Add(new ListData());
                                st.chart.lstData[2].label = dt.Rows[i]["SensorNameA"].ToString() + "-Z(" + dt.Rows[i]["ZAxis"].ToString() + ")"; ;
                                st.chart.lstData[2].fill = true;
                                st.chart.lstData[2].backgroundColor = param.backgroundColorValue3;
                                st.chart.lstData[2].borderColor = param.borderColorValue3;
                                */
                                //st.chart.lstData.Add(new ListData());
                            }
                            else if (dt.Rows[i]["Value2"] != DBNull.Value)
                            {
                                bValue2 = true;
                                /*
                                st.chart.lstData[0].label = dt.Rows[i]["SensorNameA"].ToString() + "-X(" + dt.Rows[i]["XAxis"].ToString() + ")"; ;
                                st.chart.lstData[0].fill = true;
                                st.chart.lstData[0].backgroundColor = param.backgroundColorValue1;
                                st.chart.lstData[0].borderColor = param.borderColorValue1;

                                st.chart.lstData.Add(new ListData());
                                st.chart.lstData[1].label = dt.Rows[i]["SensorNameA"].ToString() + "-Y(" + dt.Rows[i]["YAxis"].ToString() + ")"; ;
                                st.chart.lstData[1].fill = true;
                                st.chart.lstData[1].backgroundColor = param.backgroundColorValue2;
                                st.chart.lstData[1].borderColor = param.borderColorValue2;
                                //st.chart.lstData.Add(new ListData());
                                */
                            }
                            else
                            {
                                /*
                                if (sensorType.ToLower() == "earthquake" || sensorType.ToLower() == "slope")
                                    st.chart.lstData[0].label = dt.Rows[i]["SensorNameA"].ToString() + "-X";
                                else
                                    st.chart.lstData[0].label = dt.Rows[i]["SensorNameA"].ToString();
                                st.chart.lstData[0].fill = true;
                                st.chart.lstData[0].backgroundColor = param.backgroundColorValue1;
                                st.chart.lstData[0].borderColor = param.borderColorValue1;
                                */

                            }
                            if (dt.Rows[i]["HiLimit01"] != DBNull.Value)
                            {
                                /*
                                st.chart.lstData.Add(new ListData());
                                Data wdAlarm = new Data();
                                wdAlarm.x = tFirst;
                                wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit01"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].label = "三級警戒(高)";
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                                st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel3;
                                st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel3;
                                wdAlarm = new Data();
                                wdAlarm.x = tEnd;
                                wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit01"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                */
                            }
                            if (dt.Rows[i]["HiLimit02"] != DBNull.Value)
                            {
                                /*
                                st.chart.lstData.Add(new ListData());
                                Data wdAlarm = new Data();
                                wdAlarm.x = tFirst;
                                wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit02"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].label = "二級警戒(高)";
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                                st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel2;
                                st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel2;
                                wdAlarm = new Data();
                                wdAlarm.x = tEnd;
                                wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit02"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                */
                            }
                            if (dt.Rows[i]["HiLimit03"] != DBNull.Value)
                            {
                                /*
                                st.chart.lstData.Add(new ListData());
                                Data wdAlarm = new Data();
                                wdAlarm.x = tFirst;
                                wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit03"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].label = "一級警戒(高)";
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                                st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel1;
                                st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel1;
                                wdAlarm = new Data();
                                wdAlarm.x = tEnd;
                                wdAlarm.y = double.Parse(dt.Rows[i]["HiLimit03"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                */
                            }
                            if (dt.Rows[i]["LoLimit01"] != DBNull.Value)
                            {
                                /*
                                st.chart.lstData.Add(new ListData());
                                Data wdAlarm = new Data();
                                wdAlarm.x = tFirst;
                                wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit01"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].label = "三級警戒(低)";
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                                st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel3;
                                st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel3;
                                wdAlarm = new Data();
                                wdAlarm.x = tEnd;
                                wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit01"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                */
                            }
                            if (dt.Rows[i]["LoLimit02"] != DBNull.Value)
                            {
                                /*
                                st.chart.lstData.Add(new ListData());
                                Data wdAlarm = new Data();
                                wdAlarm.x = tFirst;
                                wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit02"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].label = "二級警戒(低)";
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                                st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel2;
                                st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel2;
                                wdAlarm = new Data();
                                wdAlarm.x = tEnd;
                                wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit02"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                */
                            }
                            if (dt.Rows[i]["LoLimit03"] != DBNull.Value)
                            {
                                /*
                                st.chart.lstData.Add(new ListData());
                                Data wdAlarm = new Data();
                                wdAlarm.x = tFirst;
                                wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit03"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].label = "一級警戒(低)";
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                                st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel1;
                                st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel1;
                                wdAlarm = new Data();
                                wdAlarm.x = tEnd;
                                wdAlarm.y = double.Parse(dt.Rows[i]["LoLimit03"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdAlarm);
                                */
                            }
                            if (dt.Rows[i]["SensorType"].ToString().ToLower() == "crack")
                            {
                                /*
                                st.chart.lstData.Add(new ListData());
                                Data wdInitValue = new Data();
                                wdInitValue.x = tFirst;
                                wdInitValue.y = double.Parse(dt.Rows[i]["InitValue"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].label = "初始值";
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdInitValue);
                                st.chart.lstData[st.chart.lstData.Count - 1].fill = true;
                                st.chart.lstData[st.chart.lstData.Count - 1].backgroundColor = param.backgroundColorLevel1;
                                st.chart.lstData[st.chart.lstData.Count - 1].borderColor = param.borderColorLevel1;
                                wdInitValue = new Data();
                                wdInitValue.x = tEnd;
                                wdInitValue.y = double.Parse(dt.Rows[i]["InitValue"].ToString());
                                st.chart.lstData[st.chart.lstData.Count - 1].data.Add(wdInitValue);
                                */
                            }
                        }
                        string sb = "";

                        sb += DateTime.Parse(dt.Rows[i]["RecordTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss") + ",";
                        sb += dt.Rows[i]["value1"].ToString() + ",";
                        if (bValue2)
                        {
                            sb += dt.Rows[i]["value2"].ToString() + ",";

                        }
                        if (bValue3)
                        {
                            sb += dt.Rows[i]["value3"].ToString() + ",";
                        }
                        if (sensorType.ToLower() == "crack")
                            sb += dt.Rows[i]["InitValue"].ToString();
                        else
                        {
                            if (sb.Length > 1)
                                sb = sb.Substring(0, sb.Length - 1);
                        }
                        csvBuilder.AppendLine(sb);

                        //writer.WriteLine(sb);

                    }

                    memoryStream.Position = 0;
                    var big5Encoding = Encoding.GetEncoding("big5");
                    var bytes = big5Encoding.GetBytes(csvBuilder.ToString());
                    //var bytes = Encoding.Default.GetBytes(csvBuilder.ToString());
                    //writer.Flush();
                    //memoryStream.Flush();
                    //memoryStream.Position = 0;
                    //byte[] bytes2 = memoryStream.ToArray();
                    string fileName = sensorName + range + "-" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
                    string encodedFilename = HttpUtility.UrlPathEncode(fileName);
                    Response.Headers.Add("Content-Disposition", $"attachment; filename*=UTF-8''{encodedFilename}");
                    Response.ContentType = "application/octet-stream;";
                    return File(bytes, Response.ContentType);
                }
                /*
                var result = new FileContentResult(bytes, "text/csv; charset=Big5")
                {
                    FileDownloadName = fileName,
                   
                };
                return result;
                */
                //return File(memoryStream, "text/csv; charset=Big5", "clients.csv");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /*
        [HttpGet("export")]
        public async Task<IActionResult> ExportToCsv()
        {
            try
            {
                System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // 註冊Big5等編碼
                Encoding big5Enc = Encoding.GetEncoding("big5");

                using var ms = new MemoryStream();
                using var sr = new StreamWriter(ms, big5Enc);
                List<string> dataList = new List<string>();
                dataList.Add($"1,林育才,ahtsair@a.a");
                dataList.Add($"2,林育麟,ling@a.a");
                foreach (var item in dataList)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(item);
                    sr.Write(sb.ToString());
                }
                var config = new CsvConfiguration(CultureInfo.CurrentCulture)
                {
                    // 採用標準的 RFC 4180 解析與寫入 CSV 資料
                    Mode = CsvMode.RFC4180,
                    // 用來讓 CSV 欄位標頭不區分大小寫
                    PrepareHeaderForMatch = args => args.Header.ToLower()
                };


                // 寫入尾行
                sr.Flush();
                ms.Flush();
                ms.Position = 0;
                byte[] bytes2 = ms.ToArray();
                string fileName = "輸出-" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
                string encodedFilename = HttpUtility.UrlPathEncode(fileName);
                Response.Headers.Add("Content-Disposition",$"attachment; filename*=UTF-8''{encodedFilename}");

                Response.ContentType = "application/octet-stream;";
                //return new FileContentResult(bytes, Response.ContentType);
                return File(bytes2, Response.ContentType);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("匯出失敗！" + ex.Message, ex);
            }
        }


        static byte[] ConvertToBigEndian(string utf8String)
        {
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(utf8String);

            byte[] bigEndianBytes = new byte[utf8Bytes.Length];

            for (int i = 0; i < utf8Bytes.Length; i++)
            {
                bigEndianBytes[i] = utf8Bytes[i];
            }

            return bigEndianBytes;
        }
        */
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

        /*
        //[Authorize]
        [HttpPost]
        [Route("GetWaterSensorById")]
        public IActionResult GetSensorById(List<string>? lstSensorParam)
        {
            //List<WaterSensor> lstSensor = new List<WaterSensor>();
            string? conn = _configuration.GetConnectionString("Water2022");
            try
            {
                using (IDbConnection dbConnection = new SqlConnection(conn))
                {
                    dbConnection.Open();
                    if (lstSensorParam.Count > 0)
                    {
                        string query = "SELECT [SensorId],[SensorNameA],[SensorNameB] FROM Sensors WHERE SensorId IN @Values order by sensorId";
                        var result = dbConnection.Query(query, new { values = lstSensorParam });
                        
                        return Ok(result);
                    }
                    else
                    {
                        string query = "SELECT [SensorId],[SensorNameA],[SensorNameB] FROM Sensors order by SensorId";
                        var result = dbConnection.Query(query);
                        
                        return Ok(result);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "處理資料時發生錯誤(500)");
            }
            
        }

        //[Authorize]
        [HttpPost]
        [Route("GetWaterSensorByName")]
        public IActionResult GetSensorByName(List<string>? lstSensorParam)
        {
            //List<WaterSensor> lstSensor = new List<WaterSensor>();
            string? conn = _configuration.GetConnectionString("Water2022");
            try
            {
                using (IDbConnection dbConnection = new SqlConnection(conn))
                {
                    dbConnection.Open();
                    if (lstSensorParam.Count > 0)
                    {
                        string query = "SELECT [SensorId],[SensorNameA],[SensorNameB] FROM Sensors WHERE SensorNameA like @Values order by sensorNameA";
                        var result = dbConnection.Query(query, new { values = lstSensorParam });
                        
                        return Ok(result);
                    }
                    else
                    {
                        string query = "SELECT [SensorId],[SensorNameA],[SensorNameB] FROM Sensors order by SensorNameA";
                        var result = dbConnection.Query(query);
                        
                        return Ok(result);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "處理資料時發生錯誤(500)");
            }

        }
        */
        /*
        [Authorize]
        [HttpPost]
        [Route("GetWaterSensorByFuzzy")]
        public IActionResult GetSensorByFuzzy(string? sensorId,string? sensorName)
        {
            //List<WaterSensor> lstSensor = new List<WaterSensor>();
            string? conn = _configuration.GetConnectionString("Water2022");
            try
            {
                using (IDbConnection dbConnection = new SqlConnection(conn))
                {
                    dbConnection.Open();
                    string query = "SELECT [SensorId],[SensorNameA],[SensorNameB] FROM Sensors WHERE (sensorid like @value1 or SensorNameA like @value2)  order by sensorId";
                    if (!sensorId.IsNullOrEmpty())
                        sensorId = "%" + sensorId + "%";
                    if (!sensorName.IsNullOrEmpty())
                        sensorName = "%" + sensorName + "%";
                    var result = dbConnection.Query(query, new { value1 = sensorId, value2=sensorName });
                    
                    return Ok(result);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "處理資料時發生錯誤(500)");
            }

        }
        */

        [Authorize]
        [HttpPost]
        [Route("GetWaterSensorDataByFuzzy")]
        public IActionResult GetWaterSensorDataByFuzzy(string? areaId, string? areaName, string? stationId, string? stationName, string? timeBegin, string? timeEnd, bool? isAlarm)
        {
            try
            {
                List<WaterSensorDataQuery> lstDatas = new List<WaterSensorDataQuery>();
                DataTable dt = null;
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHelper = new SqlHelper(conn); ;
                List<SqlParameter> lstParam = new List<SqlParameter>();


                lstParam.Add(new SqlParameter("@AreaId", String.IsNullOrEmpty(areaId) ? DBNull.Value:areaId));
                lstParam.Add(new SqlParameter("@AreaName", String.IsNullOrEmpty(areaName) ? DBNull.Value :areaName));
                lstParam.Add(new SqlParameter("@StationId", String.IsNullOrEmpty(stationId) ? DBNull.Value :stationId));
                lstParam.Add(new SqlParameter("@StationName", String.IsNullOrEmpty(stationName) ? DBNull.Value :stationName));
                lstParam.Add(new SqlParameter("@begin", String.IsNullOrEmpty(timeBegin) ? DBNull.Value :timeBegin));
                lstParam.Add(new SqlParameter("@end", String.IsNullOrEmpty(timeEnd) ? DBNull.Value : timeEnd));
                lstParam.Add(new SqlParameter("@isAlarm", isAlarm==null ? DBNull.Value : isAlarm));
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
            catch(Exception ex)
            {
                return BadRequest(ex.Message);  
            }

        }


    }

}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using SqlHelper = Wra10Core2023.Util.SQLHelper;
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
using Windows.Devices.Sensors;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;
using static Wra10Core2023.Controllers.SensorManageController;
using System.Reflection;

namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorManageController : ControllerBase
    {
        public delegate bool TryParseHandler<T>(string value, out T result);

        private readonly ILogger<SensorController> _logger;
        private readonly IConfiguration _configuration;
        public SensorManageController(ILogger<SensorController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost]
        [Route("DeleteSensor")]
        public IActionResult DeleteSensor(string sensorId, string sensorName)
        {
            string? conn = _configuration.GetConnectionString("Water2022");
            try
            {
                SqlHelper sqlHelper = new SqlHelper(conn);

                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("SensorId", sensorId));
                lstParams.Add(new SqlParameter("SensorName", sensorName));
                sqlHelper.ExecuteStoreProcedureQuery("sp_DeleteSensor", lstParams.ToArray());
                

                return Ok("刪除成功");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("DeleteStation")]
        public IActionResult DeleteStation(string stationId, string stationName)
        {
            string? conn = _configuration.GetConnectionString("Water2022");
            try
            {
                SqlHelper sqlHelper = new SqlHelper(conn);

                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("StationId", stationId));
                lstParams.Add(new SqlParameter("StationName", stationName));
                DataTable dt=sqlHelper.ExecuteStoreProcedureQuery("sp_DeleteStation", lstParams.ToArray());
                if (dt.Rows.Count > 0 && int.Parse(dt.Rows[0][0].ToString()) > 0)
                {

                    return Ok("刪除成功");
                }
                else
                {
                    return Ok("刪除失敗");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpPost]
        [Route("AddStation")]
        public IActionResult AddStation(string areaId, string stationName, double X, double Y)
        {
            string? conn = _configuration.GetConnectionString("Water2022");
            try
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("AreaId", areaId));
                DataTable dt = sqlHelper.ExecuteQuery("Select areaId from areas where areaid=@areaId", lstParams.ToArray());
                if (dt.Rows.Count==0)
                {
                    return BadRequest("沒有此 areaID:"+areaId);
                }


                lstParams.Clear();
                dt.Clear();
                lstParams.Add(new SqlParameter("StationName", stationName));
                dt = sqlHelper.ExecuteQuery("Select stationNameA from stations where stationNamea=@stationName", lstParams.ToArray());
                if (dt.Rows.Count > 0)
                {
                    return BadRequest("已有相同站台名稱:"+stationName);
                }

                dt.Clear();
                lstParams.Clear();
                dt = sqlHelper.ExecuteQuery("Select stationId from stations order by stationId desc");
                if (dt.Rows.Count > 0) {
                    string stationId = dt.Rows[0][0].ToString();
                    string stationDigit = RemoveNonDigits(stationId);
                    int newDigit = 1;
                    int.TryParse(stationDigit, out newDigit);
                    string newStationId=stationId.Replace(stationDigit,"")+(newDigit+1).ToString().PadLeft(4, '0');  

                    

                    lstParams.Add(new SqlParameter("AreaId", areaId));
                    lstParams.Add(new SqlParameter("StationId", newStationId));
                    lstParams.Add(new SqlParameter("StationName", stationName));
                    lstParams.Add(new SqlParameter("X", X));
                    lstParams.Add(new SqlParameter("Y", Y));
                    dt.Clear();
                    dt= sqlHelper.ExecuteStoreProcedureQuery("sp_AddStation", lstParams.ToArray());
                    List<AddEditStationReturn> lstStation = new List<AddEditStationReturn>();
                    AddEditStationReturn asr = new AddEditStationReturn()
                    {
                        stationId = dt.Rows[0]["stationId"].ToString(),
                        stationName = dt.Rows[0]["stationNameA"].ToString(),
                        X =double.Parse(dt.Rows[0]["X"].ToString()),
                        Y = double.Parse(dt.Rows[0]["Y"].ToString()),
                        areaId = dt.Rows[0]["areaId"].ToString(),
                        //areaName = dt.Rows[0]["areaName"].ToString(),

                    };
                    lstStation.Add(asr);
                    MergedData<AddEditStationReturn> mergedData = new MergedData<AddEditStationReturn>
                    {
                        StringInfo = "新增成功",
                        DataList = lstStation
                    };
                    return Ok(mergedData);

                }
                return BadRequest("新增失敗:無法取的新的站台編碼");
            }
            catch (Exception ex)
            {
                return BadRequest("新增失敗:"+ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("EditStation")]
        public IActionResult EditStation(string stationId,string areaId, string stationName, double X, double Y)
        {
            string? conn = _configuration.GetConnectionString("Water2022");
            try
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("AreaId", areaId));
                DataTable dt = sqlHelper.ExecuteQuery("Select areaId from areas where areaid=@areaId",lstParams.ToArray());
                if (dt.Rows.Count == 0)
                {
                    return BadRequest("沒有此areaID:"+areaId);
                }


                lstParams.Clear();
                dt.Clear();
                lstParams.Add(new SqlParameter("@StationId", stationId));
                lstParams.Add(new SqlParameter("StationName", stationName));
                dt = sqlHelper.ExecuteQuery("Select stationNameA from stations where stationId <> @stationid and stationNamea=@stationName", lstParams.ToArray());
                if (dt.Rows.Count > 0)
                {
                    return BadRequest("已有相同站台名稱,非本站 stationName:"+stationName);
                }

                dt.Clear();
                lstParams.Clear();


                lstParams.Add(new SqlParameter("AreaId", areaId));
                lstParams.Add(new SqlParameter("StationId", stationId));
                lstParams.Add(new SqlParameter("StationName", stationName));
                lstParams.Add(new SqlParameter("X", X));
                lstParams.Add(new SqlParameter("Y", Y));
                dt.Clear();
                dt=sqlHelper.ExecuteStoreProcedureQuery("sp_EditStation", lstParams.ToArray());

                List<AddEditStationReturn> lstStation = new List<AddEditStationReturn>();
                AddEditStationReturn asr = new AddEditStationReturn()
                {
                    stationId = dt.Rows[0]["stationId"].ToString(),
                    stationName = dt.Rows[0]["stationNameA"].ToString(),
                    X = double.Parse(dt.Rows[0]["X"].ToString()),
                    Y = double.Parse(dt.Rows[0]["Y"].ToString()),
                    areaId = dt.Rows[0]["areaId"].ToString(),
                    //areaName = dt.Rows[0]["areaName"].ToString(),

                };
                lstStation.Add(asr);
                MergedData<AddEditStationReturn> mergedData = new MergedData<AddEditStationReturn>
                {
                    StringInfo = "修改成功",
                    DataList = lstStation
                };
                return Ok(mergedData);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetStationForEdit")]
        public IActionResult GetStationForEdit(string stationId)
        {
            string areaId;
            string stationName;
            double X;
            double Y;
            string? conn = _configuration.GetConnectionString("Water2022");
            try
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@StationId", stationId));
                
                DataTable dt = sqlHelper.ExecuteQuery(@"
                Select stationId,stationNameA,X,Y,areaId from stations where stationId=@stationId 
                "
                , lstParams.ToArray());

                List<AddEditStationReturn> lstStation = new List<AddEditStationReturn>();
                AddEditStationReturn asr = new AddEditStationReturn()
                {
                    stationId = dt.Rows[0]["stationId"].ToString(),
                    stationName = dt.Rows[0]["stationNameA"].ToString(),
                    X = double.Parse(dt.Rows[0]["X"].ToString()),
                    Y = double.Parse(dt.Rows[0]["Y"].ToString()),
                    areaId = dt.Rows[0]["areaId"].ToString(),
                    //areaName = dt.Rows[0]["areaName"].ToString(),

                };
                lstStation.Add(asr);
                MergedData<AddEditStationReturn> mergedData = new MergedData<AddEditStationReturn>
                {
                    StringInfo = "讀取成功",
                    DataList = lstStation
                };
                return Ok(mergedData);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [Authorize]
        [HttpPost]
        [Route("AddSensor")]
        public IActionResult AddSensor
            (string sensorName, string sensorType, string stationId, 
            float? initValue, float? initValue2, 
            decimal? hiLimit01, decimal? hiLimit02, decimal? hiLimit03, 
            decimal? loLimit01, decimal? loLimit02, decimal? loLimit03,
            decimal? altitudeLow,
    decimal? altitudeHigh,
    float? offset,
    float? offset2,
    bool? iot,
    string? iotGuid1,
    string? iotGuid2,
    string? remark,
    string? comment,
    decimal? X,
    decimal? Y
    )
        {
            string? conn = _configuration.GetConnectionString("Water2022");
            List<AddEditSensorReturn> lstSensor = new List<AddEditSensorReturn>();
            try
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("StationId", stationId));
                DataTable dt = sqlHelper.ExecuteQuery(@"Select b.AreaId,StationId from stations a 
                    inner join areas b on a.areaid=b.areaid where stationid=@stationId",lstParams.ToArray());
                if (dt.Rows.Count == 0)
                {
                    return BadRequest("新增失敗;沒有此 stationID:" + stationId);
                }
                string areaId = dt.Rows[0]["AreaId"].ToString();

                lstParams.Clear();
                dt.Clear();
                lstParams.Add(new SqlParameter("sensorName", sensorName));
                dt = sqlHelper.ExecuteQuery("Select sensorNameA from sensors where sensorNameA=@sensorName", lstParams.ToArray());
                if (dt.Rows.Count > 0)
                {
                    return BadRequest("新增失敗;已有相同感測器名稱:" + dt.Rows[0][0].ToString());
                }

                dt.Clear();
                lstParams.Clear();
                lstParams.Add(new SqlParameter("sensorType", sensorType));

                dt = sqlHelper.ExecuteQuery("Select sensortype from sensortypes where sensortype=@sensortype", lstParams.ToArray());
                if (dt.Rows.Count <= 0)
                {
                    return BadRequest("新增失敗;查無此感測器類型:" + dt.Rows[0][0].ToString());
                }
                string dataField = "value1";
                if (sensorType.ToUpper()=="SLOPE")
                {
                    dataField += ";value2";
                }
                dt.Clear();
                lstParams.Clear();
                lstParams.Add(new SqlParameter("sensorType", sensorType));
                dt = sqlHelper.ExecuteQuery("Select sensorId from sensors where sensortype=@sensortype order by sensorid desc", lstParams.ToArray());

                if (dt.Rows.Count > 0)
                {
                    string sensorId = dt.Rows[0][0].ToString();
                    string sensorDigit = RemoveNonDigits(sensorId);
                    int newDigit = 1;
                    int.TryParse(sensorDigit, out newDigit);
                    string newSensorId = sensorId.Replace(sensorDigit, "") + (newDigit+1).ToString().PadLeft(4, '0');
                    lstParams.Clear();
                    lstParams.Add(new SqlParameter("@sensorId", newSensorId));
                    lstParams.Add(new SqlParameter("@sensorName", sensorName));
                    lstParams.Add(new SqlParameter("@sensorType", sensorType));
                    lstParams.Add(new SqlParameter("@areaId", areaId));
                    lstParams.Add(new SqlParameter("@initValue", (object)initValue ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@initValue2", (object)initValue2 ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@stationId", stationId));
                    lstParams.Add(new SqlParameter("@HiLimit01", (object)hiLimit01 ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@HiLimit02", (object)hiLimit02 ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@HiLimit03", (object)hiLimit03 ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@LoLimit01", (object)loLimit01 ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@LoLimit02", (object)loLimit02 ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@LoLimit03", (object)loLimit03 ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@AltitudeLow", (object)altitudeLow ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@AltitudeHigh", (object)altitudeHigh ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@Offset", (object)offset ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@Offset2", (object)offset2 ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@Iot", (object)iot ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@IotGuid1", (object)iotGuid1 ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@IotGuid2", (object)iotGuid2 ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@Remark", (object)remark ?? DBNull.Value)); ;
                    lstParams.Add(new SqlParameter("@comment", (object)comment ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@dataField", dataField));
                    lstParams.Add(new SqlParameter("@X", (object)X ?? DBNull.Value));
                    lstParams.Add(new SqlParameter("@Y", (object)Y ?? DBNull.Value));
                    //                    lstParams.Add(new SqlParameter("@X", x));
                    //                    lstParams.Add(new SqlParameter("@Y", y));

                    dt.Clear();
                    dt = sqlHelper.ExecuteStoreProcedureQuery("sp_AddSensor", lstParams.ToArray());

                    //decimal x=0, y=0;

                    TryParseString(dt.Rows[0]["initValue"].ToString(), out initValue);
                    TryParseString(dt.Rows[0]["initValue2"].ToString(), out initValue2);
                    TryParseString(dt.Rows[0]["hiLimit01"].ToString(), out hiLimit01);
                    TryParseString(dt.Rows[0]["hiLimit02"].ToString(), out hiLimit02);
                    TryParseString(dt.Rows[0]["hiLimit03"].ToString(), out hiLimit03);
                    TryParseString(dt.Rows[0]["loLimit01"].ToString(), out loLimit01);
                    TryParseString(dt.Rows[0]["loLimit02"].ToString(), out loLimit02);
                    TryParseString(dt.Rows[0]["loLimit03"].ToString(), out loLimit03);
                    TryParseString(dt.Rows[0]["altitudeLow"].ToString(), out altitudeLow);
                    TryParseString(dt.Rows[0]["altitudeHigh"].ToString(), out altitudeHigh);
                    TryParseString(dt.Rows[0]["offset"].ToString(), out offset);
                    TryParseString(dt.Rows[0]["offset2"].ToString(), out offset2);
                    TryParseString(dt.Rows[0]["iot"].ToString(), out iot);
                    iotGuid1 = dt.Rows[0]["iotGuid1"]?.ToString();// == DBNull.Value ? DBNull.Value.ToString() : dt.Rows[0]["iotGuid1"].ToString();
                    iotGuid2 = dt.Rows[0]["iotGuid2"]?.ToString();
                    remark = dt.Rows[0]["remark"]?.ToString();
                    comment = dt.Rows[0]["comment"]?.ToString();
                    TryParseString(dt.Rows[0]["x"].ToString(), out X);
                    TryParseString(dt.Rows[0]["y"].ToString(), out Y);

                    AddEditSensorReturn esr = new AddEditSensorReturn()
                    {
                        sensorId = dt.Rows[0]["sensorId"].ToString(),
                        sensorName = dt.Rows[0]["sensorNameA"].ToString(),
                        sensorType = dt.Rows[0]["sensorType"].ToString(),
                        initValue = initValue,
                        initValue2 = initValue2,
                        stationId = dt.Rows[0]["stationId"].ToString(),
                        hiLimit01 = hiLimit01,
                        hiLimit02 = hiLimit02,
                        hiLimit03 = hiLimit03,
                        loLimit01 = loLimit01,
                        loLimit02 = loLimit02,
                        loLimit03 = loLimit03,
                        altitudeLow = altitudeLow,
                        altitudeHigh = altitudeHigh,
                        offset = offset,
                        offset2 = offset2,
                        iot = iot,
                        iotGuid1 = iotGuid1,
                        iotGuid2 = iotGuid2,
                        remark = remark,
                        comment = comment,
                        x = X,
                        y = Y

                    };
                    lstSensor.Add(esr);
                }
                MergedData<AddEditSensorReturn> mergedData = new MergedData<AddEditSensorReturn>
                {
                    StringInfo = "新增成功",
                    DataList = lstSensor
                };
                return Ok(lstSensor);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("EditSensor")]
        public IActionResult EditSensor(string sensorId, string stationId,
            string sensorName, string sensorType,
            float? initValue, float? initValue2,
            decimal? hiLimit01, decimal? hiLimit02, decimal? hiLimit03,
            decimal? loLimit01, decimal? loLimit02, decimal? loLimit03,
            decimal? altitudeLow,
    decimal? altitudeHigh,
    float? offset,
    float? offset2,
    bool? iot,
    string? iotGuid1 ,
    string? iotGuid2 ,
    string? remark,
    string? comment,
    decimal? X,
    decimal? Y
    )
        {
            string? conn = _configuration.GetConnectionString("Water2022");
            List<AddEditSensorReturn> lstSensor = new List<AddEditSensorReturn>();
            try
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("StationId", stationId));
                DataTable dt = sqlHelper.ExecuteQuery(@"Select b.AreaId,StationId from stations a 
                    inner join areas b on a.areaid=b.areaid where stationid=@stationId", lstParams.ToArray());
                if (dt.Rows.Count == 0)
                {
                    return BadRequest("修改失敗;沒有此 stationID:" + stationId);
                }
                string areaId = dt.Rows[0]["AreaId"].ToString();

                lstParams.Clear();
                dt.Clear();
                lstParams.Add(new SqlParameter("sensorId", sensorId));
                lstParams.Add(new SqlParameter("sensorName", sensorName));
                dt = sqlHelper.ExecuteQuery("Select sensorNameA from sensors where sensorid <> @sensorid and sensorNameA=@sensorName", lstParams.ToArray());
                if (dt.Rows.Count > 0)
                {
                    return BadRequest("修改失敗;已有相同感測器名稱在不同的感測器編號:" + dt.Rows[0][0].ToString());
                }

                dt.Clear();
                lstParams.Clear();
                lstParams.Add(new SqlParameter("sensorType", sensorType));

                dt = sqlHelper.ExecuteQuery("Select sensortype from sensortypes where sensortype=@sensortype", lstParams.ToArray());
                if (dt.Rows.Count <= 0)
                {
                    return BadRequest("修改失敗;查無此感測器類型:" + dt.Rows[0][0].ToString());
                }
                string dataField = "value1";
                if (sensorType.ToUpper() == "SLOPE")
                {
                    dataField += ";value2";
                }
                lstParams.Clear();
                lstParams.Add(new SqlParameter("@sensorId", sensorId));
                lstParams.Add(new SqlParameter("@sensorName", sensorName));
                lstParams.Add(new SqlParameter("@sensorType", sensorType));
                lstParams.Add(new SqlParameter("@areaId", areaId));
                lstParams.Add(new SqlParameter("@initValue", (object)initValue ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@initValue2", (object)initValue2 ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@stationId", stationId ));
                lstParams.Add(new SqlParameter("@HiLimit01", (object)hiLimit01 ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@HiLimit02", (object)hiLimit02 ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@HiLimit03", (object)hiLimit03 ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@LoLimit01", (object)loLimit01 ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@LoLimit02", (object)loLimit02 ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@LoLimit03", (object)loLimit03 ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@AltitudeLow", (object)altitudeLow ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@AltitudeHigh", (object)altitudeHigh ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@Offset", (object)offset ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@Offset2", (object)offset2 ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@Iot", (object)iot ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@IotGuid1", (object)iotGuid1 ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@IotGuid2", (object)iotGuid2 ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@Remark", (object)remark ?? DBNull.Value)); ;
                lstParams.Add(new SqlParameter("@comment", (object)comment ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@dataField", dataField));
                lstParams.Add(new SqlParameter("@X", (object)X ?? DBNull.Value));
                lstParams.Add(new SqlParameter("@Y", (object)Y ?? DBNull.Value));
                //                    lstParams.Add(new SqlParameter("@X", x));
                //                    lstParams.Add(new SqlParameter("@Y", y));

                dt.Clear();
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_EditSensor", lstParams.ToArray());

                //decimal x = 0, y = 0;

                TryParseString(dt.Rows[0]["initValue"].ToString(), out initValue);
                TryParseString(dt.Rows[0]["initValue2"].ToString(), out initValue2);
                TryParseString(dt.Rows[0]["hiLimit01"].ToString(), out hiLimit01);
                TryParseString(dt.Rows[0]["hiLimit02"].ToString(), out hiLimit02);
                TryParseString(dt.Rows[0]["hiLimit03"].ToString(), out hiLimit03);
                TryParseString(dt.Rows[0]["loLimit01"].ToString(), out loLimit01);
                TryParseString(dt.Rows[0]["loLimit02"].ToString(), out loLimit02);
                TryParseString(dt.Rows[0]["loLimit03"].ToString(), out loLimit03);
                TryParseString(dt.Rows[0]["altitudeLow"].ToString(), out altitudeLow);
                TryParseString(dt.Rows[0]["altitudeHigh"].ToString(), out altitudeHigh);
                TryParseString(dt.Rows[0]["offset"].ToString(), out offset);
                TryParseString(dt.Rows[0]["offset2"].ToString(), out offset2);
                TryParseString(dt.Rows[0]["iot"].ToString(), out iot);
                iotGuid1 = dt.Rows[0]["iotGuid1"]?.ToString();// == DBNull.Value ? DBNull.Value.ToString() : dt.Rows[0]["iotGuid1"].ToString();
                iotGuid2 = dt.Rows[0]["iotGuid2"]?.ToString();
                remark = dt.Rows[0]["remark"]?.ToString();
                comment = dt.Rows[0]["comment"]?.ToString();
                TryParseString(dt.Rows[0]["x"].ToString(), out X);
                TryParseString(dt.Rows[0]["y"].ToString(), out Y);

                AddEditSensorReturn esr = new AddEditSensorReturn()
                {
                    sensorId = dt.Rows[0]["sensorId"].ToString(),
                    sensorName = dt.Rows[0]["sensorNameA"].ToString(),
                    sensorType = dt.Rows[0]["sensorType"].ToString(),
                    initValue = initValue,
                    initValue2 = initValue2,
                    stationId = dt.Rows[0]["stationId"].ToString(),
                    hiLimit01 = hiLimit01,
                    hiLimit02 = hiLimit02,
                    hiLimit03 = hiLimit03,
                    loLimit01 = loLimit01,
                    loLimit02 = loLimit02,
                    loLimit03 = loLimit03,
                    altitudeLow = altitudeLow,
                    altitudeHigh = altitudeHigh,
                    offset = offset,
                    offset2 = offset2,
                    iot = iot,
                    iotGuid1 = iotGuid1,
                    iotGuid2 = iotGuid2,
                    remark = remark,
                    comment = comment,
                    x = X,
                    y = Y

                };
                lstSensor.Add(esr);
                MergedData<AddEditSensorReturn> mergedData = new MergedData<AddEditSensorReturn>
                {
                    StringInfo = "修改成功",
                    DataList = lstSensor
                };
                return Ok(lstSensor);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetSensorForEdit")]
        public IActionResult GetSensorForEdit(string sensorId)
        {
            string stationId;
            string sensorName; string sensorType;
            float? initValue; float? initValue2;
            decimal? hiLimit01; decimal? hiLimit02; decimal? hiLimit03;
            decimal? loLimit01; decimal? loLimit02; decimal? loLimit03;
            decimal? altitudeLow;
    decimal? altitudeHigh;
    float? offset;
    float? offset2;
    bool iot;
    string iotGuid1;
    string iotGuid2;
    string remark;
    string comment;
    decimal? X;
            decimal? Y;
            string? conn = _configuration.GetConnectionString("Water2022");
            List<AddEditSensorReturn> lstSensor = new List<AddEditSensorReturn>();
            try
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("sensorId", sensorId));
                DataTable dt = sqlHelper.ExecuteQuery(@"
                select 
	sensorId,
	sensorNameA,
	sensorType,
	areaId,
	initValue,
	initValue2,
	stationId ,
	HiLimit01 ,
	HiLimit02 ,
	HiLimit03 ,
	LoLimit01 ,
	LoLimit02 ,
	LoLimit03,
	AltitudeLow,
    AltitudeHigh,
	Offset,
    Offset2,
	Iot,
    IotGuid1,
    IotGuid2,
	Remark,
	comment,
	X,
	Y 
	from sensors where sensorid=@sensorId
                ", lstParams.ToArray());
                if (dt.Rows.Count == 0)
                {
                    return BadRequest("無此 sensorID:" + sensorId);
                }
                
                
               
                TryParseString(dt.Rows[0]["initValue"].ToString(), out initValue);
                TryParseString(dt.Rows[0]["initValue2"].ToString(), out initValue2);
                TryParseString(dt.Rows[0]["hiLimit01"].ToString(), out hiLimit01);
                TryParseString(dt.Rows[0]["hiLimit02"].ToString(), out hiLimit02);
                TryParseString(dt.Rows[0]["hiLimit03"].ToString(), out hiLimit03);
                TryParseString(dt.Rows[0]["loLimit01"].ToString(), out loLimit01);
                TryParseString(dt.Rows[0]["loLimit02"].ToString(), out loLimit02);
                TryParseString(dt.Rows[0]["loLimit03"].ToString(), out loLimit03);
                TryParseString(dt.Rows[0]["altitudeLow"].ToString(), out altitudeLow);
                TryParseString(dt.Rows[0]["altitudeHigh"].ToString(), out altitudeHigh);
                TryParseString(dt.Rows[0]["offset"].ToString(), out offset);
                TryParseString(dt.Rows[0]["offset2"].ToString(), out offset2);
                iot = dt.Rows[0]["iot"] == DBNull.Value ? false : bool.Parse(dt.Rows[0]["iot"].ToString());
                iotGuid1 = dt.Rows[0]["iotGuid1"]?.ToString();// == DBNull.Value ? DBNull.Value.ToString() : dt.Rows[0]["iotGuid1"].ToString();
                iotGuid2 = dt.Rows[0]["iotGuid2"]?.ToString();
                remark = dt.Rows[0]["remark"]?.ToString();
                comment = dt.Rows[0]["comment"]?.ToString();
                TryParseString(dt.Rows[0]["x"].ToString(), out X);
                TryParseString(dt.Rows[0]["y"].ToString(), out Y);

                AddEditSensorReturn esr = new AddEditSensorReturn()
                {
                    sensorId = dt.Rows[0]["sensorId"].ToString(),
                    sensorName = dt.Rows[0]["sensorNameA"].ToString(),
                    sensorType = dt.Rows[0]["sensorType"].ToString(),
                    initValue = initValue,
                    initValue2 = initValue2,
                    stationId = dt.Rows[0]["stationId"].ToString(),
                    hiLimit01 = hiLimit01,
                    hiLimit02 = hiLimit02,
                    hiLimit03 = hiLimit03,
                    loLimit01 = loLimit01,
                    loLimit02 = loLimit02,
                    loLimit03 = loLimit03,
                    altitudeLow = altitudeLow,
                    altitudeHigh = altitudeHigh,
                    offset = offset,
                    offset2 = offset2,
                    iot = iot,
                    iotGuid1 = iotGuid1,
                    iotGuid2 = iotGuid2,
                    remark = remark,
                    comment = comment,
                    x = X,
                    y = Y

                };
                lstSensor.Add(esr);
                MergedData<AddEditSensorReturn> mergedData = new MergedData<AddEditSensorReturn>
                {
                    StringInfo = "讀取成功",
                    DataList = lstSensor
                };
                return Ok(lstSensor);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        bool TryParseString(string input, out bool? result)
        {
            if (String.IsNullOrEmpty(input))
            {
                result = null;
                return false;
            }
            if (bool.TryParse(input, out bool parsedResult))
            {
                result = parsedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
        bool TryParseString(string input, out int? result)
        {
            if (String.IsNullOrEmpty(input))
            {
                result = null;
                return false;
            }
            if (int.TryParse(input, out int parsedResult))
            {
                result = parsedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        bool TryParseString(string input, out float? result)
        {
            if (String.IsNullOrEmpty(input))
            {
                result = null;
                return false;
            }
            if (float.TryParse(input, out float parsedResult))
            {
                result = parsedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        bool TryParseString(string input, out decimal? result)
        {
            if (String.IsNullOrEmpty(input))
            {
                result = null;
                return false;
            }
            if (decimal.TryParse(input, out decimal parsedResult))
            {
                result = parsedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        bool TryParseString(string input, out double? result)
        {
            if (String.IsNullOrEmpty(input))
            {
                result = null;
                return false;
            }
            if (double.TryParse(input, out double parsedResult))
            {
                result = parsedResult;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        string RemoveNonDigits(string input)
        {
            string pattern = "[^0-9]";
            string result = Regex.Replace(input, pattern, "");

            return result;
        }
    }

    public class MergedData<T>
    {
        public string StringInfo { get; set; }
        public List<T> DataList { get; set; }
    }
}

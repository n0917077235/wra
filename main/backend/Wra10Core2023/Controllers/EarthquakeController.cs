using GeoJSON.Net.Geometry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Data;
using Wra10Core2023.Models;
using Wra10Core2023.Util;
using Wra10Core2023.Util.Earthquake;
using SqlHelper = Wra10Core2023.Util.SQLHelper;

namespace Wra10Core2023.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EarthquakeController : Controller
{
    private readonly ILogger<EarthquakeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public EarthquakeController(ILogger<EarthquakeController> logger, IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment)
    {
        _logger = logger;
        _configuration = configuration;
        _hostingEnvironment = hostingEnvironment;
    }

    // Allow outside connections becuase the images may be shared by url.
    [HttpGet]
    [Route("Isoseismal")]
    public IActionResult GetIsoseismalMap(string eventTime)
    {
        var t = DateTime.Parse(eventTime);
        var bytes = IsoseismalStore.Get(t);
        return File(bytes, "image/png");
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
    [HttpGet]
    [Route("GetEQEventRangeIntensity")]
    public IActionResult GetEQEventRangeIntensity(int year, int month, float intensity)
    {
        var events = IsoseismalUtil.GetEQEvents(year, month, intensity);
        return Ok(events);
    }

    [Authorize]
    [HttpGet]
    [Route("GetCwaEventLatest")]
    public string GetCwaEventLatest()
    {
        var item = EarthquakeCwa.GetLatest();
        return GeoJsonController.ToFeatureCollectionJson(item.GetFeatures());
    }

    [Authorize]
    [HttpGet]
    [Route("GetCwaEventTimes")]
    public string GetCwaEventTimes()
    {
        // get events in 1 year
        var now = DateTime.Now;
        var start = now.AddYears(-1);
        var times = EarthquakeCwa.GetTimes(start, now);
        return JToken.FromObject(times).ToString();
    }

    [Authorize]
    [HttpGet]
    [Route("GetCwaEvent")]
    public string GetCwaEvent(string time)
    {
        // get event closest to the specified time
        var t = DateUtil.ParseFormat(time, "yyyy-MM-dd HH:mm:ss");
        var item = EarthquakeCwa.FindClosest(t);
        return GeoJsonController.ToFeatureCollectionJson(item.GetFeatures());
    }

    [Authorize]
    [HttpPost]
    [Route("GetEQEvent")]
    public IActionResult GetEQEvent(int range, string eventTime)
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

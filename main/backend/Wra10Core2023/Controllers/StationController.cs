using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Newtonsoft.Json;
using System.Data;
using Wra10Core2023.Models;
using SqlHelper = Wra10Core2023.Util.SQLHelper;

namespace Wra10Core2023.Controllers;

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
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Wra10Core2023.Models;
using Wra10Core2023.Util;
using SqlHelper = Wra10Core2023.Util.SQLHelper;

namespace Wra10Core2023.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EarthquakeController : Controller
{
    private readonly ILogger<EarthquakeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _hostingEnvironment;
    string? videoImagePath = "";
    private string? virtualImagePath = "";

    public EarthquakeController(ILogger<EarthquakeController> logger, IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment)
    {
        _logger = logger;
        _configuration = configuration;
        _hostingEnvironment = hostingEnvironment;
        videoImagePath = _configuration["VideoImage:Path"];
        virtualImagePath = _configuration["VirtualVideoImage:Path"];
    }

    private string GetInputFile(string eventTime)
    {
        SqlParameter[] parameters = [new("@eventTime", eventTime)];
        var sqlHelper = SiteUtil.MainDB(_configuration);
        var dt = sqlHelper.ExecuteStoreProcedureQuery("sp_Isoseismal", parameters);
        var sb = new StringBuilder();
        sb.AppendLine("N,E,震度");

        foreach (var row in dt.Rows.Cast<DataRow>())
        {
            sb.AppendLine($"{row[2]},{row[3]},{row[4]}");
        }

        return sb.ToString();
    }

    private async Task<byte[]> GenerateImageAsync(string eventTime)
    {
        var input = GetInputFile(eventTime);
        var dir = FindDir();
        var f = Path.Combine(dir, "Input/input.txt");
        await System.IO.File.WriteAllTextAsync(f, input);
        return await RunGeneratorAsync(dir);
    }

    private static async Task<byte[]> RunGeneratorAsync(string dir)
    {
        ClearDir(Path.Combine(dir, "Output"));
        var process = new Process();
        var startInfo = new ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "/C python -m venv .venv && .venv\\Scripts\\activate && " +
            "python srec_interpolate.py input.txt log_plot";
        startInfo.WorkingDirectory = dir;
        process.StartInfo = startInfo;
        process.Start();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0) throw new Exception($"python exit code={process.ExitCode}");
        var file = Path.Combine(dir, @"Output\input_twd97.png");
        return await System.IO.File.ReadAllBytesAsync(file);
    }

    private static void ClearDir(string dir)
    {
        foreach (var f in Directory.GetFiles(dir)) System.IO.File.Delete(f);
    }

    private static string FindDir()
    {
        var target = "srec_proj";
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        while (true)
        {
            var d = Path.Combine(dir, target);
            if (Directory.Exists(d)) return d;
            var parent = new DirectoryInfo(dir).Parent;
            if (parent == null) throw new DirectoryNotFoundException();
            dir = parent.FullName;
        }
    }

    [Authorize]
    [HttpGet]
    [Route("Isoseismal")]
    public async Task<IActionResult> GetIsoseismalMap(string eventTime)
    {
        var bytes = await GenerateImageAsync(eventTime);
        return File(bytes, "image/png");
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
    [HttpGet]
    [Route("GetEQEventRangeIntensity")]
    public IActionResult GetEQEventRangeIntensity(int year, int month, float intensity)
    {
        var events = GetEQEvents(year, month, intensity);
        return Ok(events);
    }

    private record EqEvent(string Time, bool HasIsoseismalMap);

    private IEnumerable<EqEvent> GetEQEvents(int year, int month, float intensity)
    {
        SqlParameter[] parameters =
        [
            new("@year", year),
            new("@month", month),
            new("@intensity", intensity),
        ];

        var sqlHeper = SiteUtil.MainDB(_configuration);
        var dt = sqlHeper.ExecuteStoreProcedureQuery("sp_getEQMapTimeNew", parameters);
        var rows = dt.Rows.Cast<DataRow>();
        return rows.Select(row => new EqEvent((string)row[0], (int)row[1] == 1));
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

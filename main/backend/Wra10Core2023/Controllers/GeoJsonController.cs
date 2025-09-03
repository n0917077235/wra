using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using Wra10Core2023.Models;
using Wra10Core2023.Util;
using Point = GeoJSON.Net.Geometry.Point;
using SqlHelper = Wra10Core2023.Util.SQLHelper;

namespace Wra10Core2023.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeoJsonController : ControllerBase
{
    private readonly ILogger<GeoJsonController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public GeoJsonController(ILogger<GeoJsonController> logger,
        IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
    {
        _logger = logger;
        _configuration = configuration;
        _hostingEnvironment = hostingEnvironment;
    }

    [Authorize]
    [HttpGet]
    [Route("GetGeoJsonFileName")]
    public IActionResult GetGeoJsonFileName()
    {
        var fileName = new List<string>
        {
            "GPS01.json",
            "GPS02.json",
            "GPS03.json",
            "GPS04.json",
            "GPS05.json",
            "GPS08.json",
            "map.json",
            "river.json",
            "三重.json",
            "二重疏洪道左岸堤防.json",
            "基隆.json",
            "已施作透地雷達_109年.json",
            "新店.json",
            "板橋.json",
            "汐止.json",
            "河川排水水道.json",
            "a河川區域線.geojson",
            "b用地範圍線.geojson",
            "c治理計畫線.geojson"
        };

        return Ok(fileName);
    }

    [Authorize]
    [HttpPost]
    [Route("GetGeoJsonDataByFileName")]
    public string GetGeoJsonDataByFileName(string fileName)
    {
        string geoJsonFilePath = _hostingEnvironment.ContentRootPath + _configuration["GeoJsonPath:Path"];
        geoJsonFilePath = Path.Combine(geoJsonFilePath, fileName);
        if (!System.IO.File.Exists(geoJsonFilePath))
        {
            return ""; // Return a 404 Not Found if the file doesn't exist
        }

        var geoJsonData = readJsonFile(geoJsonFilePath).Result;
        return geoJsonData;
    }

    private async Task<string> readJsonFile(string filePath)
    {
        string geoJsonData = await System.IO.File.ReadAllTextAsync(filePath);
        return geoJsonData;
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("GetEmpty")]
    public string GetEmpty()
    {
        return ToFeatureCollectionJson([]);
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("GetDrawing")]
    public string GetDrawing(string name)
    {
        Drawings.CreateTableIfNeeded();
        var userId = HttpContext.GetUserName();
        return Drawings.Get(userId, name);
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("ListDrawings")]
    public string ListDrawings()
    {
        Drawings.CreateTableIfNeeded();
        var userId = HttpContext.GetUserName();
        var all = Drawings.GetNames(userId);
        return JToken.FromObject(all).ToString();
    }

    [HttpPut]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("SetDrawing")]
    public async Task SetDrawing()
    {
        var json = await Request.Body.ReadAllTextAsync();
        var t = JToken.Parse(json);
        var userId = HttpContext.GetUserName();
        var name = t.GetStr("name");
        var geojson = t.GetStr("geojson");
        Drawings.Update(userId, name, geojson);
    }

    [HttpDelete]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("DeleteDrawing")]
    public void DeleteDrawing(string name)
    {
        var userId = HttpContext.GetUserName();
        Drawings.Delete(userId, name);
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("GetTaipeiGateGps")]
    public string GetTaipeiGateGps()
    {
        var sqlHelper = SiteUtil.MainDB();
        var query = @"
            Select a.AreaID, a.SensorType, b.AreaName, SensorNameA, a.x, a.y
            , LastDataTime ,CAST(LastValue1 AS INT) AS LastValue1
            from sensors a
            inner join Areas b on a.AreaID=b.areaid
            where SensorType='Gate' and a.areaid in ('A12','A13')
            order by areaid, sensorid
            ";

        var dt = sqlHelper.ExecuteQuery(query);

        var pointList = dt.Rows.Cast<DataRow>().Select((row, i) =>
        {
            var x = (double)(decimal)row["x"];
            var y = (double)(decimal)row["y"];
            var geometry = new Point(new Position(y, x));

            var properties = new Dictionary<string, object>
            {
                { "id", i.ToString() },
                { "name", row["sensorNameA"].ToString() },
                { "lastDataTime", row.GetDate("lastDataTime").ToStandardString() },
                { "lastValue1", row["lastValue1"].ToString() },
                { "sensorType", row["SensorType"]},
                { "areaID", row["AreaID"] },
            };

            return new Feature(geometry, properties);
        });

        return ToFeatureCollectionJson(pointList);
    }

    [Authorize]
    [HttpGet]
    [Route("GetSensorGps")]
    public string GetSensorGps(string sensorType)
    {
        var sqlHelper = SiteUtil.MainDB();
        var type = sensorType.ToLowerInvariant();
        if (type == "planninglevel") return PlanningLevel.GetGeoJson();
        var dt = GetSensorGpsTable(sqlHelper, type);

        var pointList = dt.Rows.Cast<DataRow>().Select(row =>
        {
            var x = (double)(decimal)row["x"];
            var y = (double)(decimal)row["y"];
            var geometry = new Point(new Position(y, x));

            var properties = new Dictionary<string, object>
            {
                { "id", row["sensorid"] },
                { "name", row["sensorNameA"] },
                { "lastDataTime", row.GetDate("lastDataTime").ToStandardString() },
                { "lastValue1", row["lastValue1"] },
                { "lastValue2", row["lastValue2"] },
                { "sensorType", row["SensorType"]},
                { "areaID", row["AreaID"] }
            };

            return new Feature(geometry, properties);
        });

        return ToFeatureCollectionJson(pointList);
    }

    private static DataTable GetSensorGpsTable(SqlHelper sqlHelper, string sensorType)
    {
        var sType = sensorType == "waterlevel2" ? "waterlevel" : sensorType;

        var common = @"
                Select a.sensorid, sensorNameA, a.SensorType, a.AreaID, b.x, b.y, lastvalue1
                , lastvalue2, lastdatatime 
                from sensors a
                inner join Stations b 
                on a.stationid=b.stationId
                where sensortype=@sType and isnull(b.x,0)!=0 and isnull(b.y,0)!=0 
                and disablestate <> 1
                ";

        var extra = sensorType switch
        {
            "gate" => "and a.areaid not in ('A12','A13')",
            "waterlevel" => "and a.importflag not like 'em%'",
            "waterlevel2" => "and a.importflag like 'em%' ",
            _ => ""
        };

        var query = $"{common} {extra}";
        var dt = sqlHelper.ExecuteQuery(query, new[] { new SqlParameter("sType", sType) });
        return dt;
    }

    public static string ToFeatureCollectionJson(IEnumerable<Feature> pointList)
    {
        var o = new
        {
            type = "FeatureCollection",
            features = pointList
        };

        return JToken.FromObject(o).ToString();
    }

    [Authorize]
    [HttpGet]
    [Route("GetTansuiGps")]
    public string GetTansuiGps()
    {
        var sqlHelper = SiteUtil.MainDB();
        string cmd = @"Select stationid,stationnamea,x,y from Stations where isnull(x,0)!=0 and isnull(y,0)!=0 and areaid<>'A08' and isnull( importflag,'') <> 'EMBank' and isnull(importflag,'') <> 'EM2022'";
        DataTable dt = sqlHelper.ExecuteQuery(cmd);
        var pointList = new List<Feature>();

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            var geometry = new Point(new Position(double.Parse(dt.Rows[i]["y"].ToString()), double.Parse(dt.Rows[i]["x"].ToString())));
            var properties = new Dictionary<string, object>
            {
                { "id", dt.Rows[i]["stationid"].ToString() },
                { "name", dt.Rows[i]["stationnameA"].ToString() },

            };
            var feature = new Feature(geometry, properties);
            pointList.Add(feature);
        }

        string json = ToFeatureCollectionJson(pointList);
        return json;
    }

    [Authorize]
    [HttpGet]
    [Route("GetAdslGps")]
    public string GetAdslGps()
    {
        string gpsId = "GPS06";
        var sqlHelper = SiteUtil.MainDB();
        string cmd = @"Select sequence,objectid, GpsID as stationid,layername as stationname,lng as x,lat as y from gpslayersub where gpsid='" + gpsId + "' and isnull(lat,1)!=0 and isnull(lng,0)!=0 and isnull(mark,1)=1 order by layername,sequence";
        DataTable dt = sqlHelper.ExecuteQuery(cmd);
        var pointList = new List<Feature>();

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            var geometry = new Point(new Position(double.Parse(dt.Rows[i]["y"].ToString()), double.Parse(dt.Rows[i]["x"].ToString())));
            var properties = new Dictionary<string, object>
            {
                { "id", dt.Rows[i]["stationname"].ToString() },
                { "name", dt.Rows[i]["stationname"].ToString() },

            };
            var feature = new Feature(geometry, properties);
            pointList.Add(feature);
        }


        string json = JsonConvert.SerializeObject(pointList);
        json = @"{
  ""type"": ""FeatureCollection"",
  ""features"":" + json + "}";


        return json;
    }

    [Authorize]
    [HttpGet]
    [Route("Get4GGps")]
    public string Get4GGps()
    {
        string gpsId = "GPS07";
        var sqlHelper = SiteUtil.MainDB();
        string cmd = @"Select sequence,objectid, GpsID as stationid,layername as stationname,lng as x,lat as y from gpslayersub where gpsid='" + gpsId + "' and isnull(lat,1)!=0 and isnull(lng,0)!=0 and isnull(mark,1)=1 order by layername,sequence";
        DataTable dt = sqlHelper.ExecuteQuery(cmd);
        var pointList = new List<Feature>();
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            var geometry = new Point(new Position(double.Parse(dt.Rows[i]["y"].ToString()), double.Parse(dt.Rows[i]["x"].ToString())));
            var properties = new Dictionary<string, object>
            {
                { "id", dt.Rows[i]["stationname"].ToString() },
                { "name", dt.Rows[i]["stationname"].ToString() },

            };

            var feature = new Feature(geometry, properties);
            pointList.Add(feature);
        }

        string json = JsonConvert.SerializeObject(pointList);
        json = @"{
  ""type"": ""FeatureCollection"",
  ""features"":" + json + "}";


        return json;
    }

    [Authorize]
    [HttpPost]
    [Route("GetCCTVGpsByArea")]
    public string GetCCTVGpsByArea(string? areaId)
    {
        var sqlHelper = SiteUtil.MainDB();
        string cmd = @"select camid,camname,a.x,a.y,streamMain from Cameras a 
       inner join stations b on a.StationID=b.StationID 
                           where isnull(a.X,0)!=0 and isnull(a.y,0)!=0 and areaId=@areaId order by channel";// and station!=10096";
        List<SqlParameter> lstParam = new List<SqlParameter>();
        lstParam.Add(new SqlParameter("@AreaId", areaId));
        DataTable dt = sqlHelper.ExecuteQuery(cmd, lstParam.ToArray());
        var pointList = new List<Feature>();
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            var geometry = new Point(new Position(double.Parse(dt.Rows[i]["y"].ToString()), double.Parse(dt.Rows[i]["x"].ToString())));
            var properties = new Dictionary<string, object>
            {
                { "id", dt.Rows[i]["camid"].ToString() },
                { "name", dt.Rows[i]["camname"].ToString()+";"+_configuration["VirtualVideoImage:Path"]+dt.Rows[i]["streammain"].ToString() },

            };

            var feature = new Feature(geometry, properties);
            pointList.Add(feature);
            //collection.Features.Add(feature);
        }
        string json = JsonConvert.SerializeObject(pointList);
        json = @"{
  ""type"": ""FeatureCollection"",
  ""features"":" + json + "}";

        return json;
    }

    [Authorize]
    [HttpGet]
    [Route("GetBankGps")]
    public string GetBankGps()
    {
        var sqlHelper = SiteUtil.MainDB();
        string cmd = @"Select stationid,stationnamea,x,y from Stations where isnull(x,0)!=0 and isnull(y,0)!=0 and importflag like 'EM%'";
        DataTable dt = sqlHelper.ExecuteQuery(cmd);
        var pointList = new List<Feature>();

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            var geometry = new Point(new Position(double.Parse(dt.Rows[i]["y"].ToString()), double.Parse(dt.Rows[i]["x"].ToString())));
            var properties = new Dictionary<string, object>
            {
                { "id", dt.Rows[i]["stationid"].ToString() },
                { "name", dt.Rows[i]["stationnameA"].ToString() },

            };

            var feature = new Feature(geometry, properties);
            pointList.Add(feature);
        }


        string json = JsonConvert.SerializeObject(pointList);
        json = @"{
  ""type"": ""FeatureCollection"",
  ""features"":" + json + "}";

        return json;
    }

    [Authorize]
    [HttpGet]
    [Route("GetStationGps")]
    public string GetStationsGps()
    {
        var sqlHelper = SiteUtil.MainDB();
        string cmd = @"Select stationid,stationnamea,x,y from Stations where isnull(x,0)!=0 and isnull(y,0)!=0";
        DataTable dt = sqlHelper.ExecuteQuery(cmd);
        var pointList = new List<Feature>();

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            var geometry = new Point(new Position(double.Parse(dt.Rows[i]["y"].ToString()), double.Parse(dt.Rows[i]["x"].ToString())));
            var properties = new Dictionary<string, object>
            {
                { "id", dt.Rows[i]["stationid"].ToString() },
                { "name", dt.Rows[i]["stationnameA"].ToString() },

            };
            var feature = new Feature(geometry, properties);
            pointList.Add(feature);
        }

        string json = JsonConvert.SerializeObject(pointList);
        json = @"{
  ""type"": ""FeatureCollection"",
  ""features"":" + json + "}";


        return json;
    }

    [Authorize]
    [HttpGet]
    [Route("GetDamPointGps")]
    public string GetDamPointGps()
    {
        string? gpsId = "GPS03";
        var sqlHelper = SiteUtil.MainDB();
        string cmd = @"Select sequence,objectid, GpsID as stationid,layername as stationname,lng as x,lat as y from gpslayersub where gpsid='" + gpsId + "' and isnull(lat,1)!=0 and isnull(lng,0)!=0 and isnull(mark,1)=1 order by layername,sequence";
        DataTable dt = sqlHelper.ExecuteQuery(cmd);
        var pointList = new List<Feature>();

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            var geometry = new Point(new Position(double.Parse(dt.Rows[i]["y"].ToString()), double.Parse(dt.Rows[i]["x"].ToString())));
            var properties = new Dictionary<string, object>
            {
                { "id", dt.Rows[i]["stationname"].ToString() },
                { "name", dt.Rows[i]["stationname"].ToString() },

            };
            var feature = new Feature(geometry, properties);
            pointList.Add(feature);
        }

        string json = JsonConvert.SerializeObject(pointList);
        json = @"{
  ""type"": ""FeatureCollection"",
  ""features"":" + json + "}";


        return json;
    }

    [Authorize]
    [HttpGet]
    [Route("GetYansantziGps")]
    public string GetYansantziGps()
    {
        var sqlHelper = SiteUtil.MainDB();
        string cmd = @"Select stationid,stationnameA,x,y from Stations where isnull(x,0)!=0 and isnull(y,0)!=0 and stationid='ST0077'";
        DataTable dt = sqlHelper.ExecuteQuery(cmd);
        var pointList = new List<Feature>();
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            var geometry = new Point(new Position(double.Parse(dt.Rows[i]["y"].ToString()), double.Parse(dt.Rows[i]["x"].ToString())));
            var properties = new Dictionary<string, object>
            {
                { "id", dt.Rows[i]["stationid"].ToString() },
                { "name", dt.Rows[i]["stationnameA"].ToString() },

            };

            var feature = new Feature(geometry, properties);
            pointList.Add(feature);
        }

        string json = JsonConvert.SerializeObject(pointList);
        json = @"{
  ""type"": ""FeatureCollection"",
  ""features"":" + json + "}";

        return json;
    }

    [Authorize]
    [HttpGet]
    [Route("GetCCTVGps")]
    public string GetCCTVGps()
    {
        var sqlHelper = SiteUtil.MainDB();
        string cmd = @"select camid,camname,a.x,a.y,streamMain from Cameras a 
       inner join stations b on a.StationID=b.StationID 
                           where isnull(a.X,0)!=0 and isnull(a.y,0)!=0 ";// and station!=10096";
        DataTable dt = sqlHelper.ExecuteQuery(cmd);
        var pointList = new List<Feature>();
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            var geometry = new Point(new Position(double.Parse(dt.Rows[i]["y"].ToString()), double.Parse(dt.Rows[i]["x"].ToString())));
            var properties = new Dictionary<string, object>
            {
                { "id", dt.Rows[i]["camid"].ToString() },
                { "name", dt.Rows[i]["camname"].ToString()+";"+new Uri($"{Request.Scheme}://{Request.Host}/" + _configuration["VirtualVideoImage:Path"] + @"/" + dt.Rows[i]["camid"].ToString()+".jpg").ToString() },
            };

            var feature = new Feature(geometry, properties);
            pointList.Add(feature);
        }

        string json = JsonConvert.SerializeObject(pointList);
        json = @"{
  ""type"": ""FeatureCollection"",
  ""features"":" + json + "}";

        return json;
    }

    [Authorize]
    [HttpGet]
    [Route("GetChainGps")]
    public IActionResult GetChainGPS()
    {
        var files = new List<ChainGPS>();
        string chainFiles = _configuration["ChainGPS:Files"];
        foreach (var file in Directory.EnumerateFiles(Path.Combine(_hostingEnvironment.ContentRootPath, "GeoJson"), "*.json"))
        {
            string fileName = "";
            int lastIndex = file.LastIndexOf('\\');
            if (lastIndex != -1)
            {
                fileName = file.Substring(lastIndex + 1);
            }
            if (fileName.Length > 0 && chainFiles.IndexOf(fileName) != -1)
            {
                using (var streamReader = new StreamReader(file))
                {
                    string json = streamReader.ReadToEnd();
                    var data = JsonConvert.DeserializeObject<ChainGPS>(json);
                    files.Add(data);
                }
            }

        }

        var combinedData = files;
        return Ok(combinedData);
    }

    public class Feature : Feature<IGeometryObject>
    {
        [JsonConstructor]
        public Feature(IGeometryObject geometry, IDictionary<string, object> properties = null, string id = null)
            : base(geometry, properties, id)
        {
        }

        public Feature(IGeometryObject geometry, object properties, string id = null)
            : base(geometry, properties, id)
        {
        }
    }
}

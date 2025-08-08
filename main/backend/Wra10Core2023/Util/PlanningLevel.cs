using GeoJSON.Net.Geometry;
using System.Xml.Linq;
using Wra10Core2023.Controllers;
using static Wra10Core2023.Controllers.GeoJsonController;

namespace Wra10Core2023.Util;

public static class PlanningLevel
{
    private class Item
    {
        public string StationId, WLStatus, WarnStatus;
        public DateTime T;
        public double Value;
    }

    private static IEnumerable<Item> GetLatest()
    {
        var url = @"https://antiflood.wra10.gov.tw/WS2012/Infos.asmx/GetWstNewestWL";
        var root = XDocument.Load(url).Root;
        var timeFormat = "yyyy-MM-ddTHH:mm:ss";

        return root.Elements().Select(x =>
        {
            var d = GetDict(x);

            return new Item
            {
                StationId = d["StaNo"],
                T = DateUtil.ParseFormat(d["DataTime"], timeFormat),
                Value = double.Parse(d["WL"]),
                WLStatus = d["WLStatus"],
                WarnStatus = d["WarnStatus"],
            };
        });
    }

    private static Dictionary<string, string> GetDict(XElement x)
    {
        return x.Elements().ToDictionary(x => x.Name.LocalName, x => x.Value);
    }

    public static string GetGeoJson()
    {
        var map = GetLatest().ToDictionary(x => x.StationId, x => x);
        var file = @"Data\規劃科水位計.csv";

        var lines = System.IO.File.ReadAllLines(file)
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line));

        var pointList = lines.Select(line =>
        {
            var words = line.Split(',');
            var x = double.Parse(words[7]);
            var y = double.Parse(words[6]);
            var geometry = new Point(new Position(y, x));
            var id = words[0].Trim();
            var item = map.TryGetValue(id, out var v) ? v : null;

            var properties = new Dictionary<string, object>
            {
                { "id", id },
                { "name", words[1].Trim() },
                { "lastDataTime", item.T.ToStandardString() },
                { "lastValue1", item.Value },
                { "sensorType", "PlanningLevel"},
                { "areaID", "A01" }
            };

            return new Feature(geometry, properties);
        });

        return GeoJsonController.ToFeatureCollectionJson(pointList);
    }
}

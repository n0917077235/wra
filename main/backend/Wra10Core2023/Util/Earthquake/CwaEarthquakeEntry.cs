using GeoJSON.Net.Geometry;
using Wra10Core2023.Controllers;

namespace Wra10Core2023.Util.Earthquake;

public class CwaEarthquakeEntry
{
    public DateTime Time;
    public List<EarthquakePoint> Points;

    public IEnumerable<GeoJsonController.Feature> GetFeatures()
    {
        return Points.Select(x =>
        {
            var geometry = new Point(new Position(x.Lat, x.Lon));

            var properties = new Dictionary<string, object>
            {
                { nameof(x.Intensity), x.Intensity },
                { nameof(x.StationName), x.StationName },
            };

            return new GeoJsonController.Feature(geometry, properties);
        });
    }
}

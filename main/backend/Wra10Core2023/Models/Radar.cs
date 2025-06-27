namespace Wra10Core2023.Models
{
    public class RadarHeaderName
    {
        public string? HeaderName { get; set; }
    }

    public class RadarMain
    {
        public string? id { get; set; }
        public string? fileName { get; set; }
    }


    public class Radar
    {
        public int radarId { get; set; }
        public string? serialNo { get; set; }
        public string? headerName { get; set; }
        public string? title { get; set; }
        public List<RadarGps> lstGps { get; set; } = new List<RadarGps>();
        public List<string> lstImg { get; set; } = new List<string>();
    }

    public class RadarGps
    {
        public double x { get; set; }
        public double y { get; set; }

        public string? distance { get; set; }
    }
}

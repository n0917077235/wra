namespace EarthquakeInfoApi.Models
{
        public class EarthquakeInfo
    {
        public string? sensorId { get; set; }
        public string? sensorName { get; set; }
        public string? recordTime { get; set; }
        public string? intensity { get; set; }
        public string? grade { get; set; }
        public string? pga { get; set; }
        public string? pgv { get; set; }
        public string? eventGroup { get; set; }
        public string? eventTag { get; set; }
        public string? areaName { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace Wra10Core2023.Models
{
    public class Camera
    {
        public string? CamID { get; set; }
        public string? CamName { get; set; }
        public string? AreaId { get; set; }
        public string? AreaName { get; set; }
        public string? StationID { get; set; }
        public string? StationNameA { get; set; }
        public decimal? X { get; set; }
        public decimal? Y { get; set; }
        public string? StreamMain { get; set; }
        public int? ScreenX { get; set; }
        public int? ScreenY { get; set; }
    }

    public class CameraSimple
    {
        public string? StationID { get; set; }
        public string? StationNameA { get; set; }
        public string? CamName { get; set; }
        public decimal? X { get; set; }
        public decimal? Y { get; set; }
        public string? StreamMain { get; set; }
    }

    public class CameraSearch
    {
        public string? StationNameA { get; set; }
        public string? CamName { get; set; }
        public string? StreamMain { get; set; }
        public int? isAlarm { get; set; }
    }
}

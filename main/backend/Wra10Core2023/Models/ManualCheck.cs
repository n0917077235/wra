namespace Wra10Core2023.Models
{
    public class CaseYear
    {
        public string? year { get; set; }
        public string? value { get; set; }
    }

    public class Equement
    {
        public string? code { get; set; }
        public string? name { get; set; }
    }

    public class Target
    {
        public int? id { get; set; }
        public string? name { get; set; }
    }

    public class MapData
    {
        public List<MissionPointDetail> lstMissionPointDetail { get; set; } = new List<MissionPointDetail>();
        public List<GPS> lstGps { get; set; }=new List<GPS>();
    }

    public class CheckParameter
    {
        public string year { get; set; }
        public string kind { get; set; }
        public string cycle { get; set; }
        public string state { get; set; }
        public string station { get; set; }
        public string keyword { get; set; }
        public string targetId { get; set; }
        public string targetName { get; set; }
    }

    public class MissionPointCompare
    {
        public string id { get; set; }
        public List<MissionPointDetail> missionPointDetails { get; set; } = new List<MissionPointDetail>();
    }


    public class MissionPoint
    {
        public string serialNo { get; set; }
        public string year { get; set; }
        public string name { get; set; }

        public string location { get; set; }
        public string state { get; set; }
        public string regular { get; set; }
        public string memo { get; set; }
        public string view { get; set; }
        public string improvement { get; set; }
        public string filename { get; set; }
        public string gps { get; set; }
        public string id { get; set; }
        public string fileId { get; set; }
    }

    public class MissionStatistics
    {
        public string serialNo { get; set; }
        public string year { get; set; }
        public string name { get; set; }

        public string kind { get; set; }
        public string level0 { get; set; }
        public string level1 { get; set; }
        public string level2 { get; set; }
        public string level3 { get; set; }
        public string totalStations { get; set; }
        public string faults { get; set; }
        public string download { get; set; }
        public string gps { get; set; }
        public string id { get; set; }
        public string targetId { get; set; }
        public string checkParameter { get; set; }
    }

    public class MissionPointDetail
    {
        public int id { get; set; }
        public string checkdate { get; set; }
        public string location { get; set; }
        public string memo { get; set; }
        public string regular { get; set; }
        public string improvement { get; set; }
        public string stationtype { get; set; }
        public string station { get; set; }
        public string keyword { get; set; }
        public string weather { get; set; }
        public double twd97x { get; set; }
        public double twd97y { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string range { get; set; }
        public string situation { get; set; }
        public string repairrecord { get; set; }
        public List<MachineState> lstMS { get; set; } = new List<MachineState>();
        public List<Picture> lstPic { get; set; } = new List<Picture>();
    }

    public class Picture
    {
        public int fileId { get; set; }
        public string description { get; set; }
    }
    public class MachineState
    {
        public string machine { get; set; }
        public string state { get; set; }
    }

    public class GPS
    {
        public double x { get; set; }
        public double y { get; set; }
        public string title { get; set; }
        public int level { get; set; }
    }

    public class CompareParemeter
    {
        public string id { get; set; }
        public string sn { get; set; }
    }
}

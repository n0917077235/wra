namespace Wra10Core2023.Models
{
    public class WaterStation
    {
        public string? StationId { get; set; }
        public string? StationNameA { get; set; }
        public string? AreaId { get; set; }
        public string? AreaName { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
}
    public class AlarmData
    {
        public string? sensorId { get; set; }
        public string? sensorName { get; set; }
        public int alarmType { get; set; }
        public string? alarmName { get; set; }
        public string? sensorType { get; set; }
    }

    public class AlarmStation
    {
        public string? stationId { get; set; }
        public string? stationName { get; set; }
        public int stationCategory { get; set; }
        public List<AlarmData> lstSensors { get; set; } = new List<AlarmData>();
    }

    [Serializable]
    public class Chart
    {
        public string? main { get; set; } = "";
        public string? sensorId { get; set; } = "";
        public string? chartTitle { get; set; } = "";
        public string? xLabel { get; set; } = "";
        public string? yLabel { get; set; } = "";
        public List<ListData> lstData { get; set; } = new List<ListData>();
    }

    [Serializable]
    public class ListData
    {
        public string? label { get; set; } = "";
        public string? backgroundColor { get; set; }
        public string? borderColor { get; set; }
        public int borderWidth { get; set; } = 0;
        public bool fill { get; set; } = false;
        public List<Data> data { get; set; } = new List<Data>();
    }

    public class Data
    {
        public DateTime x { get; set; }
        public double y { get; set; }

        public Data() { }

        public Data(DateTime x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class SensorQueryStation
    {
        public string? sensorId { get; set; } = "";
        public string? xLabel { get; set; } = "";
        public string? yLabel { get; set; } = "";
        public string? sensorName { get; set; } = "";
        public string? sensorTypeName { get; set; } = "";
        public string? main { get; set; } = "";
        public string? unit { get; set; } = "";
        public Chart chart { get; set; } = new Chart();
        public Dictionary<string, SensorQuerySubStation> dicSub { get; set; } = new Dictionary<string, SensorQuerySubStation>();
    }

    public class SensorQuerySubStation
    {
        public string? name { get; set; }
        public string? header { get; set; }
        public int nIndex { get; set; }
    }
    public class AddEditStationReturn
    {
        public string? stationId { get; set; }
        public string? stationName { get; set;}
        public double X { get; set; }
        public double Y { get; set; }
        public string? areaId{ get; set; }
    }    
}

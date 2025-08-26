namespace Wra10Core2023.Models
{
    public class EQMain
    {
        public string sensorId { get; set; }
        public string sensorName { get; set; } = string.Empty;
        public string recordTime { get; set; }
        public string intensity { get; set; }
        public string grade { get; set; }   
        public string pga { get; set; }
        public string pgv { get; set; }
        public string eventGroup { get; set; }
        public string eventTag { get; set; }
        public string areaName { get; set; }
    }

    public class EQData
    {
        public string? sensorId { get; set; }
        public string? sensorName { get; set; }
        public string? xLabel {  get; set; }
        public string? yLabel { get; set; }
        public string? zLabel { get; set; }
        public List<string> lstRecordTime { get; set; }=new List<string>();
        public List<Double> lstX { get; set; } = new List<Double>();
        public List<Double> lstY { get; set; } = new List<Double>();
        public List<Double> lstZ { get; set; } = new List<Double>();
        
    }

    public class EQDetail
    {
        public string x { get; set; }
        public double y { get; set; }

    }

    public class EventTime
    {
        public string EventTime1 { get; set; }
        public int  DataType { get; set; }

    }

}

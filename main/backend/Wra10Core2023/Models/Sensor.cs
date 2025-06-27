namespace Wra10Core2023.Models
{
    public class WaterSensorType
    {
        public string? SensorType { get; set; }
        public string? SensorTypeName { get; set; }
        public string? SensorTypeSimple { get; set; }
        public string? Unit { get; set; }
    }

    public class WaterSensorArea
    {
        public string? AreaId { get; set; }
        public string? AreaName { get; set; }
    }

    public class WaterSensor
    {
        public string? SensorId { get; set; }
        public string? SensorNameA { get; set; }
        public string? AreaId { get; set; }
        public string? AreaName { get; set; }
        public string? StationId { get; set; }
        public string? StationNameA { get; set; }
        public string? SensorType { get; set; }
        public string? SensorTypeName { get; set; }
        public bool? isAlarm { get; set; } 
        public DateTime? lastDataTime { get; set; }
        public decimal? lastValue1 { get; set; }
        public decimal? lastValue2 { get; set; }
        public int ? AlarmLevel { get; set; }
        public string? AlarmName { get; set; }  
    }

    public class WaterSensorQuery
    {
        public WaterSensorQuery()
        {
            eventTag = "";
        }
        public string? areaName { get; set; }
        public string? stationName { get; set; }
        public string? sensorId { get; set; }
        public string? sensorType { get; set; }
        public string? sensorTypeName { get; set; }
        public string? sensorName { get; set; }
        public string? sensorStatus { get; set; }
        public string? lastDataTime { get; set; }
        public string? gps { get; set; }
        public string? gpsLink { get; set; }
        public string? value { get; set; }
        public int? differ1 { get; set; }
        public int? differ2 { get; set; }
        public string? direction { get; set; }
        public string? more { get; set; }
        public string? serialNo { get; set; }
        public string? unit { get; set; }
        public string? x { get; set; }
        public string? y { get; set; }
        public int fIndex { get; set; }
        public string? eventTag { get; set; }
        public string? eqGrade { get; set; }
        public string? status { get; set; }
        public int userType { get; set; }
    }

    public class SensorChartParameter
    {
        public string? sensorId { get; set; }
        public string? begin { get; set; }
        public string? end { get; set; }
        public int duration { get; set; }
        public string? backgroundColorValue1 { get; set; } = "#000000"; //20240227
        public string? borderColorValue1 { get; set; } = "#000000";//20240227
        public string? backgroundColorValue2 { get; set; } = "#000000"; //20240227
        public string? borderColorValue2 { get; set; } = "#000000";//20240227
        public string? backgroundColorValue3 { get; set; } = "#000000"; //20240227
        public string? borderColorValue3 { get; set; } = "#000000";//20240227
        public string? backgroundColorLevel1 { get; set; } = "#FF0000"; //20240227
        public string? borderColorLevel1 { get; set; } = "#FF0000"; //20240227
        public string? backgroundColorLevel2 { get; set; } = "#FFA500"; //20240227
        public string? borderColorLevel2 { get; set; } = "#FFA500"; //20240227
        public string? backgroundColorLevel3 { get; set; } = "#FFFF00"; //20240227
        public string? borderColorLevel3 { get; set; } = "#FFFF00"; //20240227
    }

    public class EmbakWaterAlarm
    {
        public string ?SensorId { get; set; }
        public string? SensorNameA { get; set; }
        public string? StationId { get; set; }
        public string? StationNameA { get; set; }
        public string? LastValue1 { get; set; }
        public string? LastDataTime { get; set; }
        public string? HiLimit01 { get; set; }
    }

    public class SensorMoreData
    {
        public string? errorMessage { get; set; }
        public string? areaID { get; set; }
        public string? areaName { get; set; }
        public string? stationNameA { get; set; }
        public string? sensorType { get; set; }
        public string? sensorTypeName { get; set; }
        public string? sensorId { get; set; }
        public string? sensorNameA { get; set; }
        public string? lastDataTime { get; set; }
        public string? lastDataTimePrev { get; set; }
        public decimal value1 { get; set; }
        public decimal value2 { get; set; }
        public decimal diff1 { get; set; }
        public decimal diff2 { get; set; }
        public decimal initValue { get; set; } = (decimal)0.0;
        public decimal initValue2 { get; set; } = (decimal)0.0;
        public decimal offset { get; set; } = (decimal)0.0;
        public decimal offset2 { get; set; } = (decimal)0.0;
        public int status1 { get; set; }
        public int status2 { get; set; }
        public string? unit { get; set; }
        public string? remark { get; set; }
        public string? camId { get; set; }
        public string? camName { get; set; }

        public string? optionName { get; set; }
        public string? optionValue { get; set; }
        public string? stream { get; set; }
        public int eqGrade { get; set; }
        public int alarm { get; set; }
    }

    public class StationSensorData
    {
        public string? stationId { get; set; }
        public string? stationNameA { get; set; }
        public string? sensorId { get; set; }
        public string? sensorNameA { get; set; }
        public string? lastDataTime { get; set; }
        public decimal value1 { get; set; }
        public decimal value2 { get; set; }
        
    }

    public class StationSensorData2
    {
        public string? areaName { get; set; }
        public string? stationNameA { get; set; }
        public string? sensorNameA { get; set; }
        public string? lastDataTime { get; set; }
        public decimal value1 { get; set; }
        public decimal value2 { get; set; }
        public string? alarmName { get; set; }
        public string? sensorTypeName { get; set; }
    }

    public class WaterSensorDataQuery
    {
        public string? Unit { get; set; }
        public string? SensorId { get; set; }
        public string? SensorName { get; set; }
        public string? AreaId { get; set; }
        public string? SensorType { get; set; }
        public string? SensorTypeName { get; set; }
        public string? AreaName { get; set; }
        public string? StationId { get; set; }
        public string? StationName { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public string? RecordTime { get; set; }
        public decimal? Value1 { get; set; }
        public decimal? Value2 { get; set; }
        public string? AlarmLevel { get; set; }
        public float? InitValue { get; set; }
        public float? offset { get; set; }
        public float? InitValue2 { get; set; }
        public float? Offset2 { get; set; }
        public decimal? Sensitivity1 { get; set; }
        public decimal? Sensitivity2 { get; set; }
        public decimal? ExtraOffset1 { get; set; }
        public decimal? ExtraOffset2 { get; set; }
        public decimal? RawValue11 { get; set; }
        public decimal? RawValue12 { get; set; }
    }


    public class AddEditSensorReturn
    {
        public string? sensorId { get; set; }
        public string? sensorName { get; set; }
        public string? sensorType { get; set; }
        public string? stationId { get; set; }
        public float? initValue { get; set; }
        public float? initValue2 { get; set; }
        public decimal? hiLimit01 { get; set; }
        public decimal? hiLimit02 { get; set; }
        public decimal? hiLimit03 { get; set; }
        public decimal? loLimit01 { get; set; }
        public decimal? loLimit02 { get; set; }
        public decimal? loLimit03 { get; set; }
        public decimal? altitudeLow { get; set; }
        public decimal? altitudeHigh { get; set; }
        public float? offset { get; set; }
        public float? offset2 { get; set; }
        public bool? iot { get; set; }
        public string iotGuid1 { get; set; }
        public string iotGuid2 { get; set; }
        public string remark { get; set; }
        public string comment { get; set; }
        public decimal? x { get; set; }
        public decimal? y { get; set; }
    }
}
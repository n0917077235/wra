using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;
using SqlHelper = Wra10Core2023.Util.SQLHelper;
using System.Data;

namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceRepairHistoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SqlHelper sqlHelper;
        private readonly string? conn;

        public class DeviceRepairSaveDto
        {
            public int year { get; set; }
            public List<DeviceStationInfo> devices { get; set; }
        }

        public class DeviceStationInfo
        {
            public string Name { get; set; }
            public string Unit { get; set; }
            public Dictionary<string, int> StationCounts { get; set; } // key: 站點名稱, value: 數量
        }

        public DeviceRepairHistoryController(IConfiguration configuration)
        {
            _configuration = configuration;
            conn = _configuration.GetConnectionString("Water2022");
            sqlHelper = new SqlHelper(conn);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("未選擇檔案或檔案為空");
            }

            var fileExtension = Path.GetExtension(file.FileName);
            if (fileExtension != ".xlsx" && fileExtension != ".xls")
            {
                return BadRequest("只接受 Excel 檔案");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            ExcelPackage.License.SetNonCommercialPersonal("My Name");
            int year = 0;
            List<DeviceStationInfo> devices = new List<DeviceStationInfo>();
            List<string> stationNames = new List<string>(); // <-- 宣告在外層

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                // 找到「項」和「次」行
                int itemRow = 0, stationRowStart = 0, stationRowEnd = 0;
                for (int r = 1; r <= rowCount; r++)
                {
                    var cell = worksheet.Cells[r, 1].Text.Trim();
                    if (cell.Contains("項")) { itemRow = r; stationRowStart = r; }
                    if (cell.Contains("次")) { stationRowEnd = r; break; }
                }

                // 組合站點名稱（從單位右邊開始，每一欄合併多行）
                // ...解析 stationNames...
                stationNames.Clear();
                for (int col = 4; col <= colCount; col++)
                {
                    var nameParts = new List<string>();
                    for (int row = stationRowStart; row <= stationRowEnd; row++)
                    {
                        var part = worksheet.Cells[row, col].Text.Trim();
                        if (!string.IsNullOrEmpty(part)) nameParts.Add(part);
                    }
                    var fullName = string.Join("", nameParts);
                    // 去除前面的數字（序號），只保留站名
                    fullName = Regex.Replace(fullName, @"^\d+", "");
                    stationNames.Add(fullName);
                }

                // 解析設備資料
                for (int row = stationRowEnd + 1; row <= rowCount; row++)
                {
                    var aCell = worksheet.Cells[row, 1].Text.Trim();
                    var name = worksheet.Cells[row, 2].Text.Trim();
                    var unit = worksheet.Cells[row, 3].Text.Trim();
                    if (string.IsNullOrEmpty(aCell) || string.IsNullOrEmpty(name)) continue;
                    if (!int.TryParse(aCell, out _)) continue; // A欄不是數字跳過

                    var stationCounts = new Dictionary<string, int>();
                    for (int i = 0; i < stationNames.Count; i++)
                    {
                        int col = 4 + i;
                        var stationName = stationNames[i];
                        if (stationName == "合計") continue; // 忽略合計
                        int count = int.TryParse(worksheet.Cells[row, col].Text.Trim(), out var c) ? c : 0;
                        stationCounts[stationName] = count;
                    }

                    devices.Add(new DeviceStationInfo
                    {
                        Name = name,
                        Unit = unit,
                        StationCounts = stationCounts
                    });
                }

                string yearText = worksheet.Cells[2, 1].Text.Trim();
                var match = Regex.Match(yearText, @"(\d{3})年度");
                if (match.Success)
                    year = int.Parse(match.Groups[1].Value);
            }

            // 以設備為主的格式寫入 txt
            var txtFileName = Path.GetFileNameWithoutExtension(file.FileName) + "_devices.txt";
            var txtFilePath = Path.Combine(uploadsFolder, txtFileName);
            using (var writer = new StreamWriter(txtFilePath, false))
            {
                writer.WriteLine($"year: {year}");
                writer.WriteLine("設備:");
                foreach (var device in devices)
                {
                    writer.WriteLine($"{device.Name} ({device.Unit})");
                    foreach (var kv in device.StationCounts)
                    {
                        if (kv.Value != 0 || kv.Key == "合計")
                            writer.WriteLine($"  {kv.Key}: {kv.Value}");
                    }
                }
            }

            // 不要刪掉註解
            return Ok(new { year, devices });
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveDeviceRepairHistory([FromBody] DeviceRepairSaveDto data)
        {
            foreach (var device in data.devices)
            {
                // 1. 取得或新增設備
                DataTable deviceDt;
                int deviceId;
                {
                    string selectDeviceSql = "SELECT DeviceId FROM RepairDevice WHERE Name = @Name AND Unit = @Unit";
                    SqlParameter[] selectDeviceParams = {
                        new SqlParameter("@Name", device.Name),
                        new SqlParameter("@Unit", device.Unit)
                    };
                    deviceDt = sqlHelper.ExecuteQuery(selectDeviceSql, selectDeviceParams);
                }
                if (deviceDt.Rows.Count > 0)
                {
                    deviceId = Convert.ToInt32(deviceDt.Rows[0]["DeviceId"]);
                }
                else
                {
                    string insertDeviceSql = "INSERT INTO RepairDevice (Name, Unit) VALUES (@Name, @Unit); SELECT SCOPE_IDENTITY();";
                    SqlParameter[] insertDeviceParams = {
                        new SqlParameter("@Name", device.Name),
                        new SqlParameter("@Unit", device.Unit)
                    };
                    object result = sqlHelper.ExecuteScalerObject(insertDeviceSql, insertDeviceParams);
                    deviceId = Convert.ToInt32(result);
                }

                // 2. 逐站點存數量
                foreach (var kv in device.StationCounts)
                {
                    string stationName = kv.Key;
                    int count = kv.Value;

                    DataTable stationDt;
                    int stationId;
                    {
                        string selectStationSql = "SELECT StationId FROM RepairStation WHERE Name = @Name";
                        SqlParameter[] selectStationParams = {
                            new SqlParameter("@Name", stationName)
                        };
                        stationDt = sqlHelper.ExecuteQuery(selectStationSql, selectStationParams);
                    }
                    if (stationDt.Rows.Count > 0)
                    {
                        stationId = Convert.ToInt32(stationDt.Rows[0]["StationId"]);
                    }
                    else
                    {
                        string insertStationSql = "INSERT INTO RepairStation (Name) VALUES (@Name); SELECT SCOPE_IDENTITY();";
                        SqlParameter[] insertStationParams = {
                            new SqlParameter("@Name", stationName)
                        };
                        object result = sqlHelper.ExecuteScalerObject(insertStationSql, insertStationParams);
                        stationId = Convert.ToInt32(result);
                    }

                    // 先刪除同一年、同設備、同站點的資料
                    string deleteSql = @"
            DELETE FROM RepairDeviceCount
            WHERE Year = @Year AND DeviceId = @DeviceId AND StationId = @StationId";
                    SqlParameter[] deleteParams = {
                        new SqlParameter("@Year", data.year),
                        new SqlParameter("@DeviceId", deviceId),
                        new SqlParameter("@StationId", stationId)
                    };
                    sqlHelper.ExecuteNonQuery(deleteSql, deleteParams);

                    // 再新增新資料
                    string insertCountSql = @"
            INSERT INTO RepairDeviceCount (Year, DeviceId, StationId, Count)
            VALUES (@Year, @DeviceId, @StationId, @Count)";
                    SqlParameter[] countParams = {
                        new SqlParameter("@Year", data.year),
                        new SqlParameter("@DeviceId", deviceId),
                        new SqlParameter("@StationId", stationId),
                        new SqlParameter("@Count", count)
                    };
                    sqlHelper.ExecuteNonQuery(insertCountSql, countParams);
                }
            }

            return Ok(new { success = true });
        }

        public class DeviceRepairHistoryQueryDto
        {
            public List<int> years { get; set; }
            public List<string> devices { get; set; }
            public List<string> stations { get; set; }
        }

        [HttpPost("query")]
        public IActionResult QueryDeviceRepairHistory([FromBody] DeviceRepairHistoryQueryDto query)
        {
            // 檢查參數
            var years = query.years != null && query.years.Count > 0 ? query.years : null;
            var devices = query.devices != null && query.devices.Count > 0 ? query.devices : null;
            var stations = query.stations != null && query.stations.Count > 0 ? query.stations : null;

            // 動態組合 SQL
            string sql = @"
        SELECT rdc.Year, rd.Name AS Device, rs.Name AS Station, rdc.Count, rd.Unit
        FROM RepairDeviceCount rdc
        INNER JOIN RepairDevice rd ON rdc.DeviceId = rd.DeviceId
        INNER JOIN RepairStation rs ON rdc.StationId = rs.StationId
        WHERE 1=1
    ";
            var parameters = new List<SqlParameter>();

            if (years != null)
            {
                sql += $" AND rdc.Year IN ({string.Join(",", years.Select((y, i) => $"@Year{i}"))})";
                for (int i = 0; i < years.Count; i++)
                    parameters.Add(new SqlParameter($"@Year{i}", years[i]));
            }
            if (devices != null)
            {
                sql += $" AND rd.Name IN ({string.Join(",", devices.Select((d, i) => $"@Device{i}"))})";
                for (int i = 0; i < devices.Count; i++)
                    parameters.Add(new SqlParameter($"@Device{i}", devices[i]));
            }
            if (stations != null)
            {
                sql += $" AND rs.Name IN ({string.Join(",", stations.Select((s, i) => $"@Station{i}"))})";
                for (int i = 0; i < stations.Count; i++)
                    parameters.Add(new SqlParameter($"@Station{i}", stations[i]));
            }

            var dt = sqlHelper.ExecuteQuery(sql, parameters.ToArray());

            // 回傳資料
            var result = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                result.Add(new
                {
                    year = Convert.ToInt32(row["Year"]),
                    device = row["Device"].ToString(),
                    unit = row["Unit"].ToString(),
                    station = row["Station"].ToString(),
                    count = Convert.ToInt32(row["Count"])
                });
            }

            return Ok(result);
        }

        [HttpGet("options")]
        public IActionResult GetOptions()
        {
            // 年分
            string yearSql = "SELECT DISTINCT Year FROM RepairDeviceCount ORDER BY Year";
            DataTable yearDt = sqlHelper.ExecuteQuery(yearSql);
            var years = new List<int>();
            foreach (DataRow row in yearDt.Rows)
            {
                years.Add(Convert.ToInt32(row["Year"]));
            }

            // 設備
            string deviceSql = "SELECT DISTINCT Name FROM RepairDevice ORDER BY Name";
            DataTable deviceDt = sqlHelper.ExecuteQuery(deviceSql);
            var devices = new List<string>();
            foreach (DataRow row in deviceDt.Rows)
            {
                devices.Add(row["Name"].ToString());
            }

            // 站點
            string stationSql = "SELECT DISTINCT Name FROM RepairStation ORDER BY Name";
            DataTable stationDt = sqlHelper.ExecuteQuery(stationSql);
            var stations = new List<string>();
            foreach (DataRow row in stationDt.Rows)
            {
                stations.Add(row["Name"].ToString());
            }

            return Ok(new
            {
                years,
                devices,
                stations
            });
        }

        [HttpPost("deleteYear")]
        public IActionResult DeleteYear([FromBody] int year)
        {
            // 刪除該年分所有 RepairDeviceCount 資料
            string sql = "DELETE FROM RepairDeviceCount WHERE Year = @Year";
            SqlParameter[] parameters = {
                new SqlParameter("@Year", year)
            };
            int affectedRows = sqlHelper.ExecuteNonQuery(sql, parameters);

            return Ok(new { success = true, deleted = affectedRows });
        }
    }
}
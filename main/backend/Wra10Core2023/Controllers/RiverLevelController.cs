using Wra10Core2023.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SqlHelper = Wra10Core2023.Util.SQLHelper;
using System.Data;

namespace Wra10Core2023.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RiverLevelController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SqlHelper sqlHelper;
        private readonly string? conn;

        public RiverLevelController(IConfiguration configuration)
        {
            _configuration = configuration;
            conn = _configuration.GetConnectionString("Water2022");
            sqlHelper = new SqlHelper(conn);
        }
        [HttpGet]
        [Route("arealist")]
        public JsonResult GetAreaList()
        {
            try
            {
                string query = "SELECT * FROM Areas";
                DataTable table = sqlHelper.ExecuteQuery(query);
                
                var result = table.AsEnumerable()
                    .Select(row => table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, 
                                    col => row[col] == DBNull.Value ? null : row[col]))
                    .ToList();

                return new JsonResult(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("sensors")]
        public JsonResult GetSensorsByArea([FromBody] AreaRequest request)
        {
            try
            {
                string query = "SELECT * FROM Sensors WHERE AreaID = @AreaID AND SensorType = @SensorType";
                SqlParameter[] parameters = {
                    new SqlParameter("@AreaID", request.AreaID),
                    new SqlParameter("@SensorType", "WaterLevel")
                };
                DataTable table = sqlHelper.ExecuteQuery(query, parameters);
                
                var result = table.AsEnumerable()
                    .Select(row => table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, 
                                    col => row[col] == DBNull.Value ? null : row[col]))
                    .ToList();

                return new JsonResult(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }

    public class AreaRequest
    {
        public string AreaID { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using SqlHelper = SQLHelper.SQLHelper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System;
using Microsoft.AspNetCore.Cors;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Wra10Core2023.Models;

namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors]
    public class TaipeiGateController : ControllerBase
    {
        private readonly ILogger<SensorController> _logger;
        private readonly IConfiguration _configuration;

        public TaipeiGateController(ILogger<SensorController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /*
        [HttpGet(Name = "GetTaipeiGate")]
        public IEnumerable<TaipeiGate>? Get()
        {
            return null;
        }
        */

        private DataTable getGateData()
        {
            DataTable dt = new DataTable();
            string? conn = _configuration.GetConnectionString("Water2022");
            if (conn != null)
            {
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParam = new List<SqlParameter>();
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_TaipeiGate", lstParam.ToArray());
            }
            return dt;
            
            
        }
        private byte[] ConvertToCsv(List<TaipeiGate> TaipeiGates)
        {
            StringBuilder csvBuilder = new StringBuilder();

            csvBuilder.AppendLine("ProductID,ProductName,Category,Price");

            foreach (var taipeiGate in TaipeiGates)
            {
                csvBuilder.AppendLine($"{taipeiGate.sensorName},{taipeiGate.lastTime},{taipeiGate.gateValue}");
            }

            byte[] csvData = Encoding.UTF8.GetBytes(csvBuilder.ToString());

            return csvData;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Route("GetTaipeiGate")]
        //[AllowAnonymous]
        public IActionResult Get()
        {
            DataTable dt=getGateData();

            List<TaipeiGate> gates = new List<TaipeiGate>();

            foreach (DataRow row in dt.Rows)
            {
                TaipeiGate gate = new TaipeiGate
                {
                    areaName = row["areaName"].ToString().Replace("\r", "").Replace("\n", ""),
                    sensorName = row["sensorNameA"].ToString().Replace("\r","").Replace("\n",""),
                    lastTime = row["lastDataTime"].ToString(),
                    gateValue = row["lastValue1"].ToString()

                };

                gates.Add(gate);
            }
            return Ok(gates.ToList());
        }


        /*

        [HttpGet]
        [Route("GetStudents")]
        public IActionResult Gets()
        {
            if (_oStudents.Count == 0)
            {
                return NotFound("No list found.");
            }

            return Ok(_oStudents);
        }

        List<Student> _oStudents = new List<Student>()
        {
            new Student(){Id=1,Name="Sayed",Roll=1001},
            new Student(){Id=2,Name="Sakib",Roll=1002},
            new Student(){Id=3,Name="Reaz",Roll=1003},
            new Student(){Id=4,Name="Elias",Roll=1004},
            new Student(){Id=5,Name="Maruf",Roll=1005},
        };
        */
    }

    public class Student
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public int Roll { get; set; } = 0;
    }
}

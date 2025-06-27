using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Wra10Core2023.Controllers
{
    public class IsoseismalMapController : Controller
    {
        private readonly ILogger<SensorController> _logger;
        private readonly IConfiguration _configuration;
        public IsoseismalMapController(ILogger<SensorController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("IsosesimalMap")]
        public async Task<IActionResult> RunPythonAsync()//string userId, string eventTime, DateTime dateEndm)
        {
            string? conn = _configuration.GetConnectionString("Water2022");
            //string[] events = eventTime.Split(' ');
            string? exe = _configuration["IsoseismalMap:exePath"];
            int nIndex=exe.LastIndexOf('\\');
            string workDir = "";
            if (nIndex != -1)
            {
                workDir=exe.Substring(0,nIndex+1);
            }
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exe, 
                    //Arguments = request.Arguments, 
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workDir,
                }
            };

            // Start the process and wait for it to finish

            process.Start();
            await process.WaitForExitAsync();

            string geoJsonData = readJsonFile(_configuration["IsoseismalMap:outputPath"]+@"\\Gps01.json").Result;
            //// Read the output from the process
            var output = process.StandardOutput.ReadToEnd();
            var errorOutput = process.StandardError.ReadToEnd();

            return Ok(new { JsonData = geoJsonData, Output = output, ErrorOutput = errorOutput });
        }

        private async Task<string> readJsonFile(string filePath)
        {
            string geoJsonData = await System.IO.File.ReadAllTextAsync(filePath);
            return geoJsonData;
        }
    }
}

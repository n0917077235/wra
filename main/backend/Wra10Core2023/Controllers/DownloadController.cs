using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.VisualBasic.Logging;
using System.IO;
using Wra10Core2023.Models;
using SqlHelper = SQLHelper.SQLHelper;

namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DownloadController : ControllerBase
    {
        
        private readonly string filesDirectory = "Files"; // Path to the directory containing files
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private string documentPath = "";
        private string login, password;
        private string iconPath = "";
        private string fileVirtualPath = "";
        public DownloadController(IConfiguration configuration,IWebHostEnvironment hostingEnvironment) 
        {
            _configuration = configuration;
            _hostingEnvironment=hostingEnvironment;
            documentPath= _configuration["DocumentDownload:Path"];
            login = _configuration["DocumentDownload:Login"];
            password = _configuration["DocumentDownload:Password"];
            iconPath= _configuration["DocumentDownload:IconPath"];
            fileVirtualPath=_configuration["DocumentDownload:FileVirtualPath"];
        }
        /*
        [HttpGet("{fileName}")]
        public IActionResult Get(string fileName)
        {
            var filePath = Path.Combine(filesDirectory, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(fileName), fileName);
        }
        */
        

        private string GetContentType(string fileName)
        {
            // Determine the MIME type based on the file extension.
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            switch (fileExtension)
            {
                case ".txt":
                    return "text/plain";
                case ".pdf":
                    return "application/pdf";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                // Add more file types as needed.
                default:
                    return "application/octet-stream";
            }
        }
        
        [HttpGet]
        [Authorize]
        [Route("GetFileTree")]
        public ActionResult<IEnumerable<FileNode>> GetFileTree()
        {
            if (documentPath.IndexOf(@"\\")!=-1) 
            {
                NetworkDriveAccess networkDriveAccess = new NetworkDriveAccess();
                networkDriveAccess.AccessNetworkDrive("", documentPath, login, password);
            }
            var fileTree = GetFilesInDirectory(documentPath);
            return Ok(fileTree);
        }

        private List<FileNode> GetFilesInDirectory(string filePath)
        {
            //MapVirtualPathToPhysical(string virt);
            var fileNodes = new List<FileNode>();
            //string filePath = _hostingEnvironment.ContentRootPath;
            string? conn = _configuration.GetConnectionString("Water2022");
            SqlHelper sqlHelper = new SqlHelper(conn);
            //string physicalPath = Path.Combine(filePath, virtualPath);
            sqlHelper.ExecuteNonQuery("insert into logs (message) values ('" + filePath + "')");
            if (Directory.Exists(filePath))
            {
                sqlHelper.ExecuteNonQuery("insert into logs (message) values ('exist')");
                var directories = Directory.GetDirectories(filePath);
                foreach (var directory in directories)
                {
                    var directoryInfo = new DirectoryInfo(directory);
                    sqlHelper.ExecuteNonQuery("insert into logs (message) values ('"+directoryInfo.FullName+"')");
                    var iconUrl = new Uri($"{Request.Scheme}://{Request.Host}/" + iconPath + @"/folder.png"); ;
                    
                    var node = new FileNode
                    {
                        Name = directoryInfo.Name,
                        IsFolder = true,
                        IconPath= iconUrl.ToString(),
                        IconType = "folder",
                        SubFolder = GetFilesInDirectory(Path.Combine(filePath, directoryInfo.Name))
                    };
                    fileNodes.Add(node);
                }

                var files = Directory.GetFiles(filePath);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    string [] folder = fileInfo.DirectoryName.Split('\\');
                    string folderName = folder[folder.Length-1] ;
                    var fileUrl = new Uri($"{Request.Scheme}://{Request.Host}/" + fileVirtualPath+@"/"+ folderName + @"/" +fileInfo.Name);
                    var iconUrl = new Uri($"{Request.Scheme}://{Request.Host}/" +iconPath + @"/" + fileInfo.Extension.Replace(".","") + ".png");
                    var node = new FileNode
                    {
                        Name = fileInfo.Name,
                        IsFolder = false,
                        IconPath=iconUrl.ToString(),
                        IconType=fileInfo.Extension.Replace(".",""),
                        DownloadLink=fileUrl.ToString()
                    };
                    fileNodes.Add(node);
                }
            }

            return fileNodes;
        }

        /*
        public string MapPath(string path)
        {

            // Validate path
            if (String.IsNullOrEmpty(path) || !path.StartsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException($"The '{path}' should be root relative, and start with a '/'.");
            }

            // Translate path to UNC format
            path = path.Replace("/", @"\", StringComparison.Ordinal);

            // Isolate first folder (or file)
            var firstFolder = path.IndexOf(@"\", 1);
            if (firstFolder < 0)
            {
                firstFolder = path.Length;
            }

            // Parse root directory from remainder of path
            var rootDirectory = path.Substring(1, firstFolder - 1);
            var relativePath = path.Substring(firstFolder);

            // Return virtual directory
            if (_virtualDirectories.ContainsKey(rootDirectory))
            {
                return _virtualDirectories[rootDirectory] + relativePath;
            }

            // Return non-virtual directory
            return _webRootPath + @"\" + rootDirectory + relativePath;

        }
        */
    }


}

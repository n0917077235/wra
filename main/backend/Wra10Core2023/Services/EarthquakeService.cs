using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Data;
using Microsoft.Data.SqlClient;

using SqlHelper = Wra10Core2023.Util.SQLHelper;

using Wra10Core2023.Models;
using System.Runtime.Versioning;

namespace Wra10Core2023.Services
{
    [SupportedOSPlatform("windows")]
    public class EarthquakeService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public EarthquakeService(IWebHostEnvironment env,IConfiguration configuration)
        {
            _configuration = configuration;
            _env = env;
        }

        public List<object> GetEQEventRangeIntensity(int year, int month, float intensity)
        {
            List<object> lstEventRange = new List<object>();

            try
            {
                string? conn = _configuration.GetConnectionString("Water2022");
                if (conn == null)
                {
                    throw new Exception("無法取得資料庫連線字串");
                }

                List<SqlParameter> lstParam = new List<SqlParameter>()
                {
                    new SqlParameter("@year", year),
                    new SqlParameter("@month", month),
                    new SqlParameter("@intensity", intensity)
                };

                SqlHelper sqlHeper = new SqlHelper(conn);
                DataTable dt = sqlHeper.ExecuteStoreProcedureQuery("sp_getEQMapTimeNew", lstParam.ToArray());

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string value1 = Convert.ToString(dt.Rows[i][0]) ?? string.Empty;
                    int value2 = Convert.ToInt32(dt.Rows[i][1] is DBNull ? 0 : dt.Rows[i][1]);

                    object obj = new object[] { value1, value2 };

                    lstEventRange.Add(obj);
                }
                return lstEventRange;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public List<EQMain> GetEQEvents(string range, string eventTime)
        {
            List<EQMain> lstEvent = new List<EQMain>();

            try
            {
                string? conn = _configuration.GetConnectionString("Water2022");
                if (conn == null)
                {
                    Console.WriteLine("無法取得資料庫連線字串");
                    throw new Exception("無法取得資料庫連線字串");
                }

                List<SqlParameter> lstParam = new List<SqlParameter>()
                {
                    new SqlParameter("@range", range),
                    new SqlParameter("@eventtime", eventTime)
                };

                SqlHelper sqlHeper = new SqlHelper(conn);
                DataTable dt = sqlHeper.ExecuteStoreProcedureQuery("sp_eqevent", lstParam.ToArray());

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    EQMain em = new EQMain()
                    {
                        sensorId    = Convert.ToString(dt.Rows[i]["sensorId"]) ?? string.Empty,
                        sensorName  = Convert.ToString(dt.Rows[i]["sensorNameA"]) ?? string.Empty,
                        recordTime  = Convert.ToString(dt.Rows[i]["recordtime"]) ?? string.Empty,
                        intensity   = Convert.ToString(dt.Rows[i]["intensity"]) ?? string.Empty,
                        grade       = Convert.ToString(dt.Rows[i]["grade"]) ?? string.Empty,
                        pga         = Convert.ToString(dt.Rows[i]["pga"]) ?? string.Empty,
                        pgv         = Convert.ToString(dt.Rows[i]["pgv"]) ?? string.Empty,
                        eventGroup  = Convert.ToString(dt.Rows[i]["groupTime"]) ?? string.Empty,
                        eventTag    = Convert.ToString(dt.Rows[i]["eventTag"]) ?? string.Empty,
                        areaName    = Convert.ToString(dt.Rows[i]["areaName"]) ?? string.Empty
                    };
                    lstEvent.Add(em);
                }

                return lstEvent;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public string? GenerateCombinedImage(List<EQMain> requests)
        {
            var basePath = Path.Combine(_env.WebRootPath, "images");
            var outputPath = Path.Combine(_env.WebRootPath, "output");
            Directory.CreateDirectory(outputPath);

            List<Bitmap> bitmaps = new();

            foreach (var request in requests)
            {
                string intensity = string.IsNullOrEmpty(request.intensity) ? "0" : request.intensity;
                var inputFile = Path.Combine(basePath, $"{intensity}.png");
                string gradeText = string.IsNullOrEmpty(request.intensity) ? "未達3級" : request.grade;

                if (!File.Exists(inputFile))
                    continue;

                int width = 800, height = 400;
                Bitmap bitmap = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);
                    var titleFont = new Font("Arial", 28, FontStyle.Bold);
                    var infoFont = new Font("Arial", 18, FontStyle.Regular);
                    Brush textBrush = Brushes.Black;

                    var title = request.sensorName ?? "(未提供站名)";
                    var titleSize = g.MeasureString(title, titleFont);
                    var titleX = (width - titleSize.Width) / 2;
                    g.DrawString(title, titleFont, textBrush, new PointF(titleX, 20));

                    float infoStartY = 80, lineHeight = 30, infoX = 30;
                    string[] lines = new string[]
                    {
                        $"所屬流域: {request.areaName}",
                        $"紀錄時間: {request.recordTime}",
                        $"PGA: {request.pga}gal",
                        $"PGV: {request.pgv}mm/s",
                        $"震度: {gradeText}"
                    };

                    for (int i = 0; i < lines.Length; i++)
                        g.DrawString(lines[i], infoFont, textBrush, new PointF(infoX, infoStartY + i * lineHeight));

                    using var smallImage = new Bitmap(inputFile);
                    int embeddedWidth = 500;
                    int embeddedHeight = embeddedWidth * 425 / 776;
                    var destRect = new Rectangle(width - embeddedWidth - 10, height - embeddedHeight - 10, embeddedWidth, embeddedHeight);
                    g.DrawImage(smallImage, destRect);
                }

                bitmaps.Add(bitmap);
            }

            if (bitmaps.Count == 0)
                return null;

            int imageWidth = bitmaps[0].Width, imageHeight = bitmaps[0].Height;
            int total = bitmaps.Count;
            int columns = (int)Math.Ceiling(Math.Sqrt(total));
            int rows = (int)Math.Ceiling((double)total / columns);
            int spacing = 10, radius = 20;

            using Bitmap combined = new Bitmap(columns * imageWidth + (columns + 1) * spacing,
                                            rows * imageHeight + (rows + 1) * spacing);
            using (Graphics g = Graphics.FromImage(combined))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.FromArgb(220, 220, 220));

                for (int i = 0; i < bitmaps.Count; i++)
                {
                    int row = i / columns;
                    int col = i % columns;
                    int x = spacing + col * (imageWidth + spacing);
                    int y = spacing + row * (imageHeight + spacing);

                    using GraphicsPath path = CreateRoundedRectanglePath(new Rectangle(x, y, imageWidth, imageHeight), radius);
                    g.SetClip(path);
                    g.DrawImage(bitmaps[i], new Rectangle(x, y, imageWidth, imageHeight));
                    g.ResetClip();

                    bitmaps[i].Dispose();
                }
            }

            string filename = $"combined_{DateTime.Now:yyyyMMddHHmmss}.png";
            string fullPath = Path.Combine(outputPath, filename);
            combined.Save(fullPath, ImageFormat.Png);

            return filename;
        }

        private GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

    }
}
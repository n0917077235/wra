using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using SqlHelper=SQLHelper.SQLHelper;
using System.Data;
using System.Text.RegularExpressions;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Drawing;
using A = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using System.Net;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Bibliography;
using DOS=DocumentFormat.OpenXml.Spreadsheet;
using System.Text;
using System.Windows.Forms;
using System.IO.Compression;
//using Spire.Doc;
using System.Collections.Generic;
using System.IO;
using SQLHelper;
//using Spire.Doc.Formatting;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Drawing;
using static PdfSharp.Pdf.PdfDictionary;
using System.Reflection;
using DocumentFormat.OpenXml.Presentation;
using DocXToPdfConverter;
using Novacode;
//using Windows.Data.Pdf;
//using Windows.Data.Pdf;

namespace Wra10MaintainWebApi.Controllers
{
    public class DocumentController : ControllerBase
    {
        private readonly string _connection;
        private readonly string _connection2;
        private readonly IConfiguration _configuration;
        public DocumentController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connection = configuration.GetConnectionString("DefaultConnection");
            _connection2 = configuration.GetConnectionString("Water2022Connection");
        }


        //[HttpGet("generate")]

        private string getStationName(string stationId)
        {
            string stationName = "";
            SqlHelper sqlHelper = new SqlHelper(_connection);

            string sql0 = @"
                    SELECT
                          StationNameA
                      FROM [Water2022].[dbo].[Stations]
                      
                      where StationId='" + stationId + "'";

            DataTable dt = sqlHelper.ExecuteQuery(sql0);
            if (dt.Rows.Count > 0)
            {
                stationName = dt.Rows[0][0].ToString();
            }
            return stationName;
        }

        private string GetLastData(string stationId, string tableType, string location, string date)
        {
            //var sql9 = @"select max(recordtime) as rt from formdata where tableType=@tableType and stationId=@stationId and location=@location and recordtime < @recordtime";
            StreamWriter file = new StreamWriter(@"D:\ApiDebug\SubDoc_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".txt");
            SqlHelper sqlHelper = new SqlHelper(_connection);
            List<SqlParameter> lstP1 = new List<SqlParameter>();
            file.WriteLine(tableType + ',' + location + ',' + date + ',' + stationId);
            lstP1.Add(new SqlParameter("@tabletype", tableType));
            lstP1.Add(new SqlParameter("@location", location));
            lstP1.Add(new SqlParameter("@recordtime", date));
            lstP1.Add(new SqlParameter("@stationId", stationId));
            DataTable dtP1 = sqlHelper.ExecuteStoreProcedureQuery("sp_getLastMaintainTime", lstP1.ToArray());
            //DataTable dtP1 = sqlHelper.ExecuteQuery(sql9, lstP1.ToArray());
            var lastDate = "";
            if (dtP1.Rows.Count > 0 && dtP1.Rows[0][0] != DBNull.Value)
            {
                lastDate = dtP1.Rows[0][0].ToString();
            }
            file.WriteLine(lastDate);
            file.Close();
            return lastDate;
        }
        private byte[] getSignature(string userId)
        {
            byte[] image = null;
            var sql = @"select signature from [Water2022].[dbo].[users] where userid='" + userId + "'";

            SqlHelper sqlHelper = new SqlHelper(_connection);
            DataTable dtP1 = sqlHelper.ExecuteQuery(sql);
            image = (byte[])dtP1.Rows[0][0];
            return image;
        }
        private byte[] GetManager(string managerId)
        {
            var sql9 = @"select signature  from [Water2022].[dbo].[users] where userid='" + managerId + "'";

            SqlHelper sqlHelper = new SqlHelper(_connection);
            DataTable dtP1 = sqlHelper.ExecuteQuery(sql9);
            byte[] signature = null;
            if (dtP1.Rows.Count > 0 && dtP1.Rows[0][0] != DBNull.Value)
            {
                signature = (byte[])dtP1.Rows[0][0];
            }

            return signature;
        }

        private string getChineseDate(string date)
        {
            string chineseDate = "";
            string[] dates = date.Split('-');
            if (dates.Length >= 3)
            {
                chineseDate = int.Parse(dates[0]) - 1911 + "年" + dates[1] + "月" + dates[2] + "日";
            }
            return chineseDate;
        }

        private void GetMaintainer(ref Dictionary<string, byte[]> dic, string maintainId)
        {
            var quotedValues = maintainId
            .Split(',')
            .Select(value => $"'{value.Trim()}'") // Enclose each value in single quotes
            .ToArray();
            var queryId = string.Join(",", quotedValues);
            var sql9 = @"select userid, signature from [Water2022].[dbo].[users] where userid in (" + queryId + ")";

            SqlHelper sqlHelper = new SqlHelper(_connection);
            DataTable dtP1 = sqlHelper.ExecuteQuery(sql9);
            //List<byte[]> lstMaintain = new List<byte[]>();
            for (int i = 0; i < dtP1.Rows.Count; i++)
            {
                byte[] signature = null;
                if (dtP1.Rows[i][1] != DBNull.Value)
                    signature = (byte[])dtP1.Rows[i][1];
                string id = dtP1.Rows[i][0].ToString().ToLower();
                dic.Add(id, signature);
            }


        }
        [HttpGet("document/maintaindelete")]
        public IActionResult GetMaintainDelete(string delInfo)
        {
            try
            {
                string[] datas = delInfo.Split(',');
                datas[2] = datas[2].Replace("(", "").Replace(")", "").Trim();
                string[] tables = datas[2].Split('+');
                string result = "";
                for (int i = 0; i < tables.Length; i++)
                {
                    SqlHelper sqlHelper = new SqlHelper(_connection);
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    parameters.Add(new SqlParameter("@tableType", tables[i]));
                    parameters.Add(new SqlParameter("@location", datas[1]));
                    parameters.Add(new SqlParameter("@stationId", datas[0]));
                    parameters.Add(new SqlParameter("@recordtime", datas[3]));
                    DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_deleteMaintain", parameters.ToArray());
                    result += dt.Rows[0][0].ToString() + "," + dt.Rows[0][1].ToString() + "\r\n";
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpGet("document/maintainquery")]
        public IActionResult GetMaintainQuery(string yearmonth)
        {

            try
            {
                StreamWriter file = new StreamWriter(@"D:\ApiDebug\QueryDoc_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".txt");
                file.WriteLine(yearmonth);
                file.Close();
                SqlHelper sqlHelper = new SqlHelper(_connection);
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@yearmonth", yearmonth));
                DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_maintainquery", parameters.ToArray());
                List<MaintainQuery> lstMaintainQuery = new List<MaintainQuery>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    MaintainQuery sq = new MaintainQuery()
                    {
                        Row = dt.Rows[i]["row"].ToString(),
                        Recordtime = dt.Rows[i]["recordtime"].ToString().Substring(0, 10),
                        Station = dt.Rows[i]["station"].ToString(),
                        Location = dt.Rows[i]["location"].ToString(),
                        Tabletype = dt.Rows[i]["tabletype"].ToString(),
                        SimpleFileNo = dt.Rows[i]["fileno"].ToString(),
                        FileNo = dt.Rows[i]["recordtime"].ToString().Substring(2, 2) + "0" + dt.Rows[i]["recordtime"].ToString().Substring(5, 2),
                        FileUrl = dt.Rows[i]["fileurl"].ToString(),
                        IsFileAvailable = dt.Rows[i]["isFileAvailable"].ToString() == "0" ? false : true,
                        IsMultiTable = dt.Rows[i]["isMultiTable"].ToString() == "0" ? false : true,
                    };

                    lstMaintainQuery.Add(sq);
                }
                return Ok(lstMaintainQuery);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        private Drawing CreateDrawingElement(MainDocumentPart mainPart, byte[] imageBytes, long width, long height, long xOffset, long yOffset)
        {
            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);
            using (var stream = new MemoryStream(imageBytes))
            {
                imagePart.FeedData(stream);
            }

            string imagePartId = mainPart.GetIdOfPart(imagePart);

            return new Drawing(
                new Inline(
                    new Extent() { Cx = width * 9525, Cy = (long)(height * 9525) },
                    new EffectExtent()
                    {
                        LeftEdge = 0L,
                        TopEdge = 0L,
                        RightEdge = 0L,
                        BottomEdge = 0L
                    },
                    new DocProperties()
                    {
                        Id = (UInt32Value)1U,
                        Name = "Picture 1"
                    },
                    new DocumentFormat.OpenXml.Drawing.NonVisualGraphicFrameDrawingProperties(
                        new GraphicFrameLocks() { NoChangeAspect = true }),
                    new Graphic(
                        new GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties()
                                    {
                                        Id = (UInt32Value)0U,
                                        Name = "New Image"
                                    },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip(
                                        new A.BlipExtensionList(
                                            new A.BlipExtension()
                                            {
                                                Uri =
                                                "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                            })
                                    )
                                    {
                                        Embed = imagePartId,
                                        CompressionState =
                                        A.BlipCompressionValues.Print
                                    },
                                    new A.Stretch(
                                        new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset() { X = xOffset, Y = yOffset },
                                        new A.Extents() { Cx = width * 9525, Cy = height * 9525 }),
                                    new A.PresetGeometry(
                                        new A.AdjustValueList()
                                    )
                                    { Preset = A.ShapeTypeValues.Rectangle }))
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                )
                {
                    DistanceFromTop = (UInt32Value)0U,
                    DistanceFromBottom = (UInt32Value)0U,
                    DistanceFromLeft = (UInt32Value)0U,
                    DistanceFromRight = (UInt32Value)0U,
                    EditId = "50D07946"
                });
        }


        private DocumentFormat.OpenXml.Wordprocessing.Paragraph CreateRightAlignedParagraph(string text, int rightMarginTwips=300)
        {
            // Create a new paragraph
            DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();

            DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties paragraphProperties = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();
            Justification justification = new Justification() { Val = JustificationValues.Right };
            paragraphProperties.Append(justification);
            Indentation indentation = new Indentation() { Right = rightMarginTwips.ToString() };
            paragraphProperties.Append(indentation);
            DocumentFormat.OpenXml.Wordprocessing.Run run = new DocumentFormat.OpenXml.Wordprocessing.Run();
            DocumentFormat.OpenXml.Wordprocessing.RunProperties runProperties = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();

            string fontName = "標楷體";
            runProperties.Append(new RunFonts() { Ascii = fontName, HighAnsi = fontName, EastAsia = fontName });
            runProperties.Append(new DocumentFormat.OpenXml.Wordprocessing.FontSize() { Val = "20" }); // Font size 12pt (12 * 2 = 24)

            run.Append(runProperties);

            DocumentFormat.OpenXml.Wordprocessing.Text runText = new DocumentFormat.OpenXml.Wordprocessing.Text(text);
            run.Append(runText);

            paragraph.Append(paragraphProperties);
            paragraph.Append(run);

            return paragraph;
        }

        private List<PhotoInfo> GetImagesFromDatabase(string yearMonth, string doc)
        {
            var lstPhoto = new List<PhotoInfo>();
            SqlHelper sqlHelper = new SqlHelper(_connection);
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@yearmonth", yearMonth));
            parameters.Add(new SqlParameter("@doc", doc));
            DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_photoReport", parameters.ToArray());

            for(int i=0;i<dt.Rows.Count;i++)
            {
                PhotoInfo pi = new PhotoInfo()
                {
                    location = dt.Rows[i]["location"].ToString(),
                    image = (byte[])dt.Rows[i]["image"]
                };
                lstPhoto.Add(pi);
            }

            return lstPhoto;
        }

        
        /*
        // Example method to get photo data from the database
        private async Task<PhotoData> GetPhotoDataAsync()
        {
            var photos = await _context.Photos.ToListAsync();

            return new PhotoData
            {
                Title = "My Photos",
                Subtitle = "A collection of my favorite pictures",
                Photos = photos.Select(p => (p.ImageData, p.Description)).ToList()
            };
        }
        */
        [HttpGet("document/DownloadPhoto2")]
        public IActionResult DownloadPhoto2(string yearMonth, string doc)
        {
            string[] date = yearMonth.Replace("-", "/").Split('/');
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int.TryParse(date[0], out year);
            int.TryParse(date[1], out month);
            year -= 1911;
            if (month == 12)
                year++;

            var images = GetImagesFromDatabase(yearMonth,doc);

            try
            {
                // Create a DOCX memory stream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Create the document with required parts
                    using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
                    {
                        // Add a main document part
                        MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();

                        // Create the document structure
                        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(
                            new DocumentFormat.OpenXml.Wordprocessing.Body());

                        // Get the body
                        DocumentFormat.OpenXml.Wordprocessing.Body body = mainPart.Document.Body;

                        // Add required document settings
                        var settings = mainPart.DocumentSettingsPart;
                        if (settings == null)
                        {
                            settings = mainPart.AddNewPart<DocumentSettingsPart>();
                            settings.Settings = new DocumentFormat.OpenXml.Wordprocessing.Settings(
                                new UpdateFieldsOnOpen() { Val = true }
                            );
                        }

                        // Add styles
                        var stylePart = mainPart.StyleDefinitionsPart;
                        if (stylePart == null)
                        {
                            stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
                            stylePart.Styles = new DocumentFormat.OpenXml.Wordprocessing.Styles();
                        }

                        // Your existing document content generation code here
                        //DocumentFormat.OpenXml.Wordprocessing.Paragraph firstParagraph = CreateRightAlignedParagraph("經濟部水利署第十河川分署");
                        //DocumentFormat.OpenXml.Wordprocessing.Paragraph secondParagraph = CreateRightAlignedParagraph($"{year}年度淡水河流域監測系統維護工作");

                        //body.Append(firstParagraph);
                        //body.Append(secondParagraph);

                        int nPages = images.Count / 6 + (images.Count % 6 == 0 ? 0 : 1);
                        int nImage = 0;
                        int indexTitle = 0;
                        int indexImage = 0;
                        for (int i = 0; i < nPages; i++) // Step through two images at a time
                        {
                            // Create a new section for each page
                            //SectionProperties sectionProps = new SectionProperties();
                            UInt32 width = 11906;
                            UInt32 height = 16838;

                            //PageSize pageSize = new PageSize() { Width = width, Height = height }; // A4 size in twips
                            //sectionProps.Append(pageSize);
                            DocumentFormat.OpenXml.Wordprocessing.Paragraph firstParagraph = CreateRightAlignedParagraph("經濟部水利署第十河川分署");


                            DocumentFormat.OpenXml.Wordprocessing.Paragraph secondParagraph = CreateRightAlignedParagraph($"{year}年度淡水河流域監測系統維護工作");

                            DocumentFormat.OpenXml.Wordprocessing.Paragraph breakParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                            DocumentFormat.OpenXml.Wordprocessing.Break lineBreak = new DocumentFormat.OpenXml.Wordprocessing.Break();
                            DocumentFormat.OpenXml.Wordprocessing.Run breakRun = new DocumentFormat.OpenXml.Wordprocessing.Run();
                            breakRun.AddAnnotation(lineBreak);

                            body.Append(firstParagraph);
                            body.Append(secondParagraph);
                            body.Append(breakParagraph);

                            SectionProperties sectionProps = SetDocumentMargins(mainPart);
                            body.Append(sectionProps);
                            DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();

                            DocumentFormat.OpenXml.Wordprocessing.TableProperties tableProperties = new DocumentFormat.OpenXml.Wordprocessing.TableProperties();

                            TableJustification tableJustification = new TableJustification() { Val = TableRowAlignmentValues.Center };
                            tableProperties.Append(tableJustification);


                            table.AppendChild(tableProperties);

                            UInt32 workWidth = (UInt32)(width - 30 * mm);

                            DocumentFormat.OpenXml.Wordprocessing.TableProperties tableProps = new DocumentFormat.OpenXml.Wordprocessing.TableProperties(
                                new TableWidth() { Width = workWidth.ToString(), Type = TableWidthUnitValues.Dxa }, // Full width minus margins
                                new TableBorders(
                                    new DocumentFormat.OpenXml.Wordprocessing.TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                    new DocumentFormat.OpenXml.Wordprocessing.BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                    new DocumentFormat.OpenXml.Wordprocessing.LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                    new DocumentFormat.OpenXml.Wordprocessing.RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                    new DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                    new DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                                )
                            );


                            table.AppendChild(tableProps);
                            UInt32 titleRowHeight = (UInt32)(10 * mm);
                            UInt32 imageHeight = (UInt32)((double)workWidth / 2 / 4 * 3 + 0.5 * mm);
                            UInt32 marginTopBottom = (UInt32)(height - 17 * mm - imageHeight * 3 - 3 * titleRowHeight) / 2;
                            UInt32 imageRowHeight = imageHeight;// (UInt32)((height- 20*mm - 17 * mm) - 3 * titleRowHeight) / 3;
                            for (int row = 0; row < 3; row++)
                            {
                                // title (height = 1.5 cm)
                                DocumentFormat.OpenXml.Wordprocessing.TableRow titleRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                                DocumentFormat.OpenXml.Wordprocessing.TableCell titleCell1 = CreateTitleCell(images[indexTitle++].location);
                                DocumentFormat.OpenXml.Wordprocessing.TableCell titleCell2 = indexTitle + 1 < images.Count ? CreateTitleCell(images[indexTitle++].location) : CreateEmptyCell();

                                titleRow.Append(titleCell1, titleCell2);

                                // (1.5 cm = 850 twips)


                                TableRowProperties titleRowProps = new TableRowProperties(new TableRowHeight() { Val = (UInt32)(titleRowHeight * 0.95) });
                                titleRow.AppendChild(titleRowProps);
                                table.Append(titleRow);

                                // image row (height = 5.5 cm)
                                DocumentFormat.OpenXml.Wordprocessing.TableRow imageRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                                long cellWidth = workWidth * 635 / 2;
                                long cellHeight = (long)((double)imageRowHeight * 635 * 1.2);
                                DocumentFormat.OpenXml.Wordprocessing.TableCell imageCell1 = CreateImageCell(mainPart, images[indexImage++].image, cellWidth, cellHeight);
                                DocumentFormat.OpenXml.Wordprocessing.TableCell imageCell2 = indexImage + 1 < images.Count ? CreateImageCell(mainPart, images[indexImage++].image, cellWidth, cellHeight) : CreateEmptyCell();

                                imageRow.Append(imageCell1, imageCell2);

                                // Set row height (5.5 cm = 3118 twips)
                                TableRowProperties imageRowProps = new TableRowProperties(new TableRowHeight() { Val = (UInt32)(imageRowHeight * 0.95) });
                                imageRow.AppendChild(imageRowProps);
                                table.Append(imageRow);

                                DocumentFormat.OpenXml.Wordprocessing.TableCellProperties titleCellProps1 = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(
                                new TableCellWidth() { Width = ((UInt32)(workWidth / 2)).ToString(), Type = TableWidthUnitValues.Dxa });

                                TableCellVerticalAlignment verticalAlignment1 = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
                                titleCellProps1.Append(verticalAlignment1);

                                titleCell1.Append(titleCellProps1);


                                DocumentFormat.OpenXml.Wordprocessing.TableCellProperties titleCellProps2 = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(
                                new TableCellWidth() { Width = ((UInt32)(workWidth / 2)).ToString(), Type = TableWidthUnitValues.Dxa });
                                TableCellVerticalAlignment verticalAlignment2 = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
                                titleCellProps2.Append(verticalAlignment2);

                                titleCell2.Append(titleCellProps2);


                                DocumentFormat.OpenXml.Wordprocessing.Paragraph imageParagraph1 = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                                DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties paragraphProperties1 = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();

                                // Set indentation to zero
                                paragraphProperties1.Append(new Indentation() { Left = "0", Right = "0" });

                                // Set spacing to zero
                                paragraphProperties1.Append(new SpacingBetweenLines() { Before = "0", After = "0" });

                                imageParagraph1.Append(paragraphProperties1);

                                imageCell1.Append(imageParagraph1);

                                DocumentFormat.OpenXml.Wordprocessing.Paragraph imageParagraph2 = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                                DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties paragraphProperties2 = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();

                                // Set indentation to zero
                                paragraphProperties2.Append(new Indentation() { Left = "0", Right = "0" });

                                // Set spacing to zero
                                paragraphProperties2.Append(new SpacingBetweenLines() { Before = "0", After = "0" });

                                imageParagraph2.Append(paragraphProperties2);

                                imageCell2.Append(imageParagraph2);

                                DocumentFormat.OpenXml.Wordprocessing.TableCellProperties cellProperties1 = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties();
                                TableCellMargin cellMargin1 = new TableCellMargin()
                                {
                                    TopMargin = new TopMargin() { Width = "250", Type = TableWidthUnitValues.Dxa },
                                    BottomMargin = new BottomMargin() { Width = "0", Type = TableWidthUnitValues.Dxa },
                                    LeftMargin = new LeftMargin() { Width = "0", Type = TableWidthUnitValues.Dxa },
                                    RightMargin = new RightMargin() { Width = "0", Type = TableWidthUnitValues.Dxa }
                                };
                                cellProperties1.Append(cellMargin1);
                                imageCell1.Append(cellProperties1);

                                DocumentFormat.OpenXml.Wordprocessing.TableCellProperties cellProperties2 = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties();
                                TableCellMargin cellMargin2 = new TableCellMargin()
                                {
                                    TopMargin = new TopMargin() { Width = "250", Type = TableWidthUnitValues.Dxa },
                                    BottomMargin = new BottomMargin() { Width = "0", Type = TableWidthUnitValues.Dxa },
                                    LeftMargin = new LeftMargin() { Width = "0", Type = TableWidthUnitValues.Dxa },
                                    RightMargin = new RightMargin() { Width = "0", Type = TableWidthUnitValues.Dxa }
                                };
                                cellProperties2.Append(cellMargin2);
                                imageCell2.Append(cellProperties2);

                                nImage += 2;
                                if (nImage >= images.Count)
                                    break;
                            }

                            body.Append(table);
                            if (i < nPages - 1)
                            {
                                DocumentFormat.OpenXml.Wordprocessing.Paragraph pageBreakParagraph =
                                    new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                                        new DocumentFormat.OpenXml.Wordprocessing.Run(
                                            new DocumentFormat.OpenXml.Wordprocessing.Break()
                                            {
                                                Type = BreakValues.Page
                                            }
                                        )
                                    );
                                body.Append(pageBreakParagraph);
                            }
                        }

                        // Ensure proper saving of the document
                        mainPart.Document.Save();

                        // Save all parts
                        wordDocument.Save();
                        //wordDocument.Close();
                    }

                    // Reset stream position
                    memoryStream.Position = 0;

                    // Generate filename
                    string fileName = $"Photo_{yearMonth}_{Guid.NewGuid():N}.docx";

                    // Return as downloadable file with correct MIME type
                    return File(
                        memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        fileName);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Document generation failed: {ex.Message}");
            }
        }

        // Helper method to set document margins
        private SectionProperties SetDocumentMargins2(MainDocumentPart mainPart)
        {
            SectionProperties sectionProps = new SectionProperties();

            // Set page size to A4
            PageSize pageSize = new PageSize()
            {
                Width = 11906U,  // A4 width in twips
                Height = 16838U  // A4 height in twips
            };

            // Set page margins
            PageMargin pageMargin = new PageMargin()
            {
                Top = 1440,    // 1 inch = 1440 twips
                Right = 1440,
                Bottom = 1440,
                Left = 1440,
                Header = 720,
                Footer = 720
            };

            sectionProps.Append(pageSize);
            sectionProps.Append(pageMargin);

            return sectionProps;
        }

        // Helper method to create right-aligned paragraph
        private DocumentFormat.OpenXml.Wordprocessing.Paragraph CreateRightAlignedParagraph(string text)
        {
            return new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties(
                    new Justification() { Val = JustificationValues.Right }
                ),
                new DocumentFormat.OpenXml.Wordprocessing.Run(
                    new DocumentFormat.OpenXml.Wordprocessing.Text(text)
                )
            );
        }

        [HttpPost("generate")]
        public IActionResult GenerateWordDocument()
        {
            // Path to save the document
            var filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "GeneratedDocument.docx");

            using (var document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());
                UInt32 width = 11906;
UInt32 height = 16838;
                int year = 2024;
DocumentFormat.OpenXml.Wordprocessing.Paragraph firstParagraph = CreateRightAlignedParagraph("經濟部水利署第十河川分署");


DocumentFormat.OpenXml.Wordprocessing.Paragraph secondParagraph = CreateRightAlignedParagraph($"{year}年度淡水河流域監測系統維護工作");

DocumentFormat.OpenXml.Wordprocessing.Paragraph breakParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
DocumentFormat.OpenXml.Wordprocessing.Break lineBreak = new DocumentFormat.OpenXml.Wordprocessing.Break();
DocumentFormat.OpenXml.Wordprocessing.Run breakRun = new DocumentFormat.OpenXml.Wordprocessing.Run();
breakRun.AddAnnotation(lineBreak);

body.Append(firstParagraph);
body.Append(secondParagraph);
body.Append(breakParagraph);

SectionProperties sectionProps = SetDocumentMargins(mainPart);
body.Append(sectionProps);
                var table = new DocumentFormat.OpenXml.Wordprocessing.Table();
                DocumentFormat.OpenXml.Wordprocessing.TableProperties tableProperties = new DocumentFormat.OpenXml.Wordprocessing.TableProperties();

TableJustification tableJustification = new TableJustification() { Val = TableRowAlignmentValues.Center };
tableProperties.Append(tableJustification);


table.AppendChild(tableProperties);

                UInt32 workWidth = (UInt32)(width - 30 * mm);

DocumentFormat.OpenXml.Wordprocessing.TableProperties tableProps = new DocumentFormat.OpenXml.Wordprocessing.TableProperties(
    new TableWidth() { Width = workWidth.ToString(), Type = TableWidthUnitValues.Dxa }, // Full width minus margins
    new TableBorders(
        new DocumentFormat.OpenXml.Wordprocessing.TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
        new DocumentFormat.OpenXml.Wordprocessing.BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
        new DocumentFormat.OpenXml.Wordprocessing.LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
        new DocumentFormat.OpenXml.Wordprocessing.RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
        new DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
        new DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
    )
);


table.AppendChild(tableProps);
UInt32 titleRowHeight = (UInt32)(10 * mm);
UInt32 imageHeight = (UInt32)((double)workWidth / 2 / 4 * 3 + 0.5 * mm);
UInt32 marginTopBottom = (UInt32)(height - 17 * mm - imageHeight * 3 - 3 * titleRowHeight) / 2;
UInt32 imageRowHeight = imageHeight;// (UInt32)((height- 20*mm - 17 * mm) - 3 * titleRowHeight) / 3;
                    

                // Adding 3 rows and 2 columns
                for (int i = 0; i < 3; i++)
                {
                    var tableRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                    for (int j = 0; j < 2; j++)
                    {
                        var tableCell = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
                        var paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();

                        // Add image to paragraph
                        AddImage(mainPart, paragraph, $"image_{i * 2 + j + 1}.jpg");

                        tableCell.Append(paragraph);
                        tableRow.Append(tableCell);
                    }
                    table.Append(tableRow);
                }

                body.Append(table);
                mainPart.Document.Save();
            }

            var stream = new FileStream(filePath, FileMode.Open);
            return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                FileDownloadName = "GeneratedDocument.docx"
            };
        }

        private void AddImage(MainDocumentPart mainPart, DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph, string imagePath)
        {
            var imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);
            using (var stream = new FileStream(imagePath, FileMode.Open))
            {
                imagePart.FeedData(stream);
            }

            var imageId = mainPart.GetIdOfPart(imagePart);
            var drawing = new Drawing(
                new Inline(
                    new Extent { Cx = 990000L, Cy = 792000L },
                    new EffectExtent
                    {
                        LeftEdge = 0L,
                        TopEdge = 0L,
                        RightEdge = 0L,
                        BottomEdge = 0L
                    },
                    new DocProperties { Id = (UInt32Value)1U, Name = "Picture" },
                    new DocumentFormat.OpenXml.Drawing.NonVisualGraphicFrameDrawingProperties(new GraphicFrameLocks { NoChangeAspect = true }),
                    new A.Graphic(
                        new A.GraphicData(
                            new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                                new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                                    new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties { Id = (UInt32Value)0U, Name = "New Bitmap Image.jpg" },
                                    new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()),
                                new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                                    new DocumentFormat.OpenXml.Drawing.Blip { Embed = imageId },
                                    new DocumentFormat.OpenXml.Drawing.Stretch(new DocumentFormat.OpenXml.Drawing.FillRectangle())),
                                new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                                    new DocumentFormat.OpenXml.Drawing.Transform2D(
                                        new DocumentFormat.OpenXml.Drawing.Offset { X = 0L, Y = 0L },
                                        new DocumentFormat.OpenXml.Drawing.Extents { Cx = 990000L, Cy = 792000L }),
                                    new DocumentFormat.OpenXml.Drawing.PresetGeometry(new DocumentFormat.OpenXml.Drawing.AdjustValueList()) { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle })))
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    )
                )
            );
              

            paragraph.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run(drawing));
        }


        [HttpGet("document/DownloadPhotoWord")]
        public IActionResult DownloadPhotoWord(string yearMonth, string doc)
        {
            string[] date = yearMonth.Replace("-", "/").Split('/');
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int.TryParse(date[0], out year);
            int.TryParse(date[1], out month);
            year -= 1911;
            if (month == 12)
                year++;

            var images = GetImagesFromDatabase(yearMonth, doc); 
            string guid = Guid.NewGuid().ToString();

            string docxPath = @"D:\temp\" + guid + @".docx";

            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(docxPath, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();



                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                DocumentFormat.OpenXml.Wordprocessing.Body body = new DocumentFormat.OpenXml.Wordprocessing.Body();
                /*
                var settings = mainPart.DocumentSettingsPart;
                if (settings == null)
                {
                    settings = mainPart.AddNewPart<DocumentSettingsPart>();
                    settings.Settings = new DocumentFormat.OpenXml.Wordprocessing.Settings(
                        new UpdateFieldsOnOpen() { Val = true }
                    );
                }

                // Add document-level styles (ONCE, before the page loop)
                var stylePart = mainPart.StyleDefinitionsPart;
                if (stylePart == null)
                {
                    stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
                    stylePart.Styles = new DocumentFormat.OpenXml.Wordprocessing.Styles();
                }
                */
                var sectionProperties = new SectionProperties();
                var pageMargin = new PageMargin
                {
                    Top = (Int32)(1440),    // 1 inch (1440 twips)
                    Right = 1440,  // 1 inch (1440 twips)
                    Bottom = 1440, // 1 inch (1440 twips)
                    Left = 1440,   // 1 inch (1440 twips)
                    Header = 1440,  // 0.5 inch (720 twips)
                    Footer = 720,  // 0.5 inch (720 twips)
                    Gutter = 0
                };
                sectionProperties.Append(pageMargin);
                //body.Append(sectionProperties);
                //body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());

                // Create multiple pages with A4 page size
                int nPages = images.Count / 6 + (images.Count % 6 == 0 ? 0 : 1);
                int nImage = 0;
                int indexTitle = 0;
                int indexImage = 0;
                //nPages = 1;
                for (int i = 0; i < nPages; i++) 
                {
                    body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                    body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());

                    //SectionProperties sectionProps = new SectionProperties();
                    UInt32 width = 11906;
                    UInt32 height = 16838;

                    //PageSize pageSize = new PageSize() { Width = width, Height = height }; // A4 size in twips
                    //sectionProps.Append(pageSize);
                    
                    DocumentFormat.OpenXml.Wordprocessing.Paragraph firstParagraph = CreateRightAlignedParagraph("經濟部水利署第十河川分署");


                    DocumentFormat.OpenXml.Wordprocessing.Paragraph secondParagraph = CreateRightAlignedParagraph($"{year}年度淡水河流域監測系統維護工作");

                    DocumentFormat.OpenXml.Wordprocessing.Paragraph breakParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                    DocumentFormat.OpenXml.Wordprocessing.Break lineBreak = new DocumentFormat.OpenXml.Wordprocessing.Break();
                    DocumentFormat.OpenXml.Wordprocessing.Run breakRun = new DocumentFormat.OpenXml.Wordprocessing.Run();
                    breakRun.AddAnnotation(lineBreak);

                    body.Append(firstParagraph);
                    body.Append(secondParagraph);
                    body.Append(breakParagraph);

                    SectionProperties sectionProps = SetDocumentMargins(mainPart);
                    body.Append(sectionProps);
                    DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();

                    DocumentFormat.OpenXml.Wordprocessing.TableProperties tableProperties = new DocumentFormat.OpenXml.Wordprocessing.TableProperties();

                    TableJustification tableJustification = new TableJustification() { Val = TableRowAlignmentValues.Center };
                    tableProperties.Append(tableJustification);


                    table.AppendChild(tableProperties);

                    UInt32 workWidth = (UInt32)(width - 30 * mm);

                    DocumentFormat.OpenXml.Wordprocessing.TableProperties tableProps = new DocumentFormat.OpenXml.Wordprocessing.TableProperties(
                        new TableWidth() { Width = workWidth.ToString(), Type = TableWidthUnitValues.Dxa }, // Full width minus margins
                        new TableBorders(
                            new DocumentFormat.OpenXml.Wordprocessing.TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new DocumentFormat.OpenXml.Wordprocessing.BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new DocumentFormat.OpenXml.Wordprocessing.LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new DocumentFormat.OpenXml.Wordprocessing.RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                        )
                    );


                    table.AppendChild(tableProps);
                    UInt32 titleRowHeight = (UInt32)(10 * mm);
                    UInt32 imageHeight = (UInt32)(914400*6/2.54);//(UInt32)((double)workWidth / 2 / 4 * 3);// + 0.5 * mm);
                    UInt32 marginTopBottom = (UInt32)(height - 17 * mm - imageHeight * 3 - 3 * titleRowHeight) / 2;
                    UInt32 imageRowHeight = (UInt32)(6 * 1440 / 2.54);// imageHeight;// (UInt32)((height- 20*mm - 17 * mm) - 3 * titleRowHeight) / 3;
                    for (int row = 0; row < 3; row++)
                    {
                        // title (height = 1.5 cm)
                        DocumentFormat.OpenXml.Wordprocessing.TableRow titleRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                        for (int col = 0; col<2;col++)
                        {
                            DocumentFormat.OpenXml.Wordprocessing.TableCell titleCell;
                            if (indexTitle < images.Count)
                            {
                                titleCell=CreateTitleCell(images[indexTitle].location);
                            }
                            else
                            {
                                titleCell = CreateEmptyCell();
                            }
                            indexTitle++;
                            titleRow.Append(titleCell);
                            DocumentFormat.OpenXml.Wordprocessing.TableCellProperties titleCellProps = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(
                            new TableCellWidth() { Width = ((UInt32)(workWidth / 2)).ToString(), Type = TableWidthUnitValues.Dxa });

                            TableCellVerticalAlignment verticalAlignment = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
                            titleCellProps.Append(verticalAlignment);
                            titleCell.Append(titleCellProps);
                        }
                        TableRowProperties titleRowProps = new TableRowProperties(new TableRowHeight() { Val = (UInt32)(titleRowHeight * 0.95) });
                        titleRow.AppendChild(titleRowProps);

                        table.Append(titleRow);

                        // image row (height = 5.5 cm)
                        DocumentFormat.OpenXml.Wordprocessing.TableRow imageRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                        long cellWidth = workWidth * 635 / 2;
                        long cellHeight = imageHeight;// (long)((double)imageRowHeight * 635 * 1.0);
                        //imageRowHeight = (uint)cellHeight;
                        for (int col = 0; col < 2; col++)
                        {
                            DocumentFormat.OpenXml.Wordprocessing.TableCell imageCell;
                            if (indexImage < images.Count)
                            {
                                imageCell = CreateImageCell(mainPart, images[indexImage++].image, cellWidth, cellHeight);
                            }
                            else
                            {
                                imageCell = CreateEmptyCell();
                            }
                            imageRow.Append(imageCell);
                            DocumentFormat.OpenXml.Wordprocessing.Paragraph imageParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                            DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties paragraphProperties = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();

                            paragraphProperties.Append(new Indentation() { Left = "0", Right = "0" });

                            paragraphProperties.Append(new SpacingBetweenLines() { Before = "0", After = "0" });

                            imageParagraph.Append(paragraphProperties);

                            imageCell.Append(imageParagraph);

                            DocumentFormat.OpenXml.Wordprocessing.TableCellProperties cellProperties = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties();
                            TableCellMargin cellMargin = new TableCellMargin()
                            {
                                TopMargin = new TopMargin() { Width = "250", Type = TableWidthUnitValues.Dxa },
                                BottomMargin = new BottomMargin() { Width = "0", Type = TableWidthUnitValues.Dxa },
                                LeftMargin = new LeftMargin() { Width = "0", Type = TableWidthUnitValues.Dxa },
                                RightMargin = new RightMargin() { Width = "0", Type = TableWidthUnitValues.Dxa }
                            };
                            cellProperties.Append(cellMargin);
                            imageCell.Append(cellProperties);
                        }
                        // Set row height (5.5 cm = 3118 twips)
                        TableRowProperties imageRowProps = new TableRowProperties(new TableRowHeight() { Val = (UInt32)(imageRowHeight) });
                        imageRow.AppendChild(imageRowProps);
                        table.Append(imageRow);
                        if (indexImage >= images.Count)
                            break;
                        /*
                        nImage += 2;
                        if (nImage >= images.Count)
                            break;
                        */
                    }

                    body.Append(table);
                    if (i < nPages - 1)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.Paragraph pageBreakParagraph =
                            new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                                new DocumentFormat.OpenXml.Wordprocessing.Run(
                                    new DocumentFormat.OpenXml.Wordprocessing.Break()
                                    {
                                        Type = BreakValues.Page
                                    }
                                )
                            );
                        body.Append(pageBreakParagraph);
                    }
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
                
            }

            var stream = new FileStream(docxPath, FileMode.Open); 
            return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document") { FileDownloadName = guid + @".docx" };



        }


        [HttpGet("document/DownloadPhoto")]
        public IActionResult DownloadPhoto(string yearMonth,string doc)
        {
            string [] date = yearMonth.Replace("-", "/").Split('/');
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int.TryParse(date[0], out year);
            int.TryParse(date[1], out month);
            year -= 1911;
            if (month == 12)
                year++;

            var images = GetImagesFromDatabase(yearMonth, doc); // Implement this function to retrieve your images

            //var images = GetImagesFromFile();
            // Create a DOCX memory stream
            using (var memoryStream = new MemoryStream())
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();



                    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    DocumentFormat.OpenXml.Wordprocessing.Body body = new DocumentFormat.OpenXml.Wordprocessing.Body();

                    var settings = mainPart.DocumentSettingsPart;
                    if (settings == null)
                    {
                        settings = mainPart.AddNewPart<DocumentSettingsPart>();
                        settings.Settings = new DocumentFormat.OpenXml.Wordprocessing.Settings(
                            new UpdateFieldsOnOpen() { Val = true }
                        );
                    }

                    // Add document-level styles (ONCE, before the page loop)
                    var stylePart = mainPart.StyleDefinitionsPart;
                    if (stylePart == null)
                    {
                        stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
                        stylePart.Styles = new DocumentFormat.OpenXml.Wordprocessing.Styles();
                    }

                    // Create multiple pages with A4 page size
                    int nPages = images.Count / 6 + (images.Count % 6 == 0 ? 0 : 1);
                    int nImage = 0;
                    int indexTitle = 0;
                    int indexImage = 0;
                    for (int i = 0; i < nPages; i++) // Step through two images at a time
                    {
                        // Create a new section for each page
                        //SectionProperties sectionProps = new SectionProperties();
                        UInt32 width = 11906;
                        UInt32 height = 16838;

                        //PageSize pageSize = new PageSize() { Width = width, Height = height }; // A4 size in twips
                        //sectionProps.Append(pageSize);
                        DocumentFormat.OpenXml.Wordprocessing.Paragraph firstParagraph = CreateRightAlignedParagraph("經濟部水利署第十河川分署");

                       
                        DocumentFormat.OpenXml.Wordprocessing.Paragraph secondParagraph = CreateRightAlignedParagraph($"{year}年度淡水河流域監測系統維護工作");

                        DocumentFormat.OpenXml.Wordprocessing.Paragraph breakParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                        DocumentFormat.OpenXml.Wordprocessing.Break lineBreak = new DocumentFormat.OpenXml.Wordprocessing.Break();
                        DocumentFormat.OpenXml.Wordprocessing.Run breakRun = new DocumentFormat.OpenXml.Wordprocessing.Run();
                        breakRun.AddAnnotation(lineBreak);

                        body.Append(firstParagraph);
                        body.Append(secondParagraph);
                        body.Append(breakParagraph);

                        SectionProperties sectionProps = SetDocumentMargins(mainPart);
                        body.Append(sectionProps);
                        DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();

                        DocumentFormat.OpenXml.Wordprocessing.TableProperties tableProperties = new DocumentFormat.OpenXml.Wordprocessing.TableProperties();

                        TableJustification tableJustification = new TableJustification() { Val = TableRowAlignmentValues.Center };
                        tableProperties.Append(tableJustification);
                        
                        
                        table.AppendChild(tableProperties);

                        UInt32 workWidth = (UInt32)(width - 30 * mm);

                        DocumentFormat.OpenXml.Wordprocessing.TableProperties tableProps = new DocumentFormat.OpenXml.Wordprocessing.TableProperties(
                            new TableWidth() { Width = workWidth.ToString(), Type = TableWidthUnitValues.Dxa }, // Full width minus margins
                            new TableBorders(
                                new DocumentFormat.OpenXml.Wordprocessing.TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new DocumentFormat.OpenXml.Wordprocessing.BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new DocumentFormat.OpenXml.Wordprocessing.LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new DocumentFormat.OpenXml.Wordprocessing.RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                new DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                            )
                        );


                        table.AppendChild(tableProps);
                        UInt32 titleRowHeight = (UInt32)(10 * mm);
                        UInt32 imageHeight=(UInt32)((double)workWidth / 2 / 4 * 3+ 0.5*mm);
                        UInt32 marginTopBottom=(UInt32)(height - 17 * mm -imageHeight* 3 - 3 * titleRowHeight) / 2;
                        UInt32 imageRowHeight = imageHeight;// (UInt32)((height- 20*mm - 17 * mm) - 3 * titleRowHeight) / 3;
                        for (int row = 0; row < 3; row++)
                        {
                            // title (height = 1.5 cm)
                            DocumentFormat.OpenXml.Wordprocessing.TableRow titleRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                            DocumentFormat.OpenXml.Wordprocessing.TableCell titleCell1 = CreateTitleCell(images[indexTitle++].location);
                            DocumentFormat.OpenXml.Wordprocessing.TableCell titleCell2 = indexTitle + 1 < images.Count ? CreateTitleCell(images[indexTitle++].location) : CreateEmptyCell();

                            titleRow.Append(titleCell1, titleCell2);

                            // (1.5 cm = 850 twips)


                            TableRowProperties titleRowProps = new TableRowProperties(new TableRowHeight() { Val = (UInt32)(titleRowHeight * 0.95) });
                            titleRow.AppendChild(titleRowProps);
                            table.Append(titleRow);

                            // image row (height = 5.5 cm)
                            DocumentFormat.OpenXml.Wordprocessing.TableRow imageRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                            long cellWidth = workWidth * 635 / 2;
                            long cellHeight = (long)((double)imageRowHeight * 635 * 1.2);
                            DocumentFormat.OpenXml.Wordprocessing.TableCell imageCell1 = CreateImageCell(mainPart, images[indexImage++].image, cellWidth, cellHeight);
                            DocumentFormat.OpenXml.Wordprocessing.TableCell imageCell2 = indexImage + 1 < images.Count ? CreateImageCell(mainPart, images[indexImage++].image, cellWidth, cellHeight) : CreateEmptyCell();

                            imageRow.Append(imageCell1, imageCell2);

                            // Set row height (5.5 cm = 3118 twips)
                            TableRowProperties imageRowProps = new TableRowProperties(new TableRowHeight() { Val = (UInt32)(imageRowHeight * 0.95) });
                            imageRow.AppendChild(imageRowProps);
                            table.Append(imageRow);

                            DocumentFormat.OpenXml.Wordprocessing.TableCellProperties titleCellProps1 = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(
                            new TableCellWidth() { Width = ((UInt32)(workWidth / 2)).ToString(), Type = TableWidthUnitValues.Dxa });

                            TableCellVerticalAlignment verticalAlignment1 = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
                            titleCellProps1.Append(verticalAlignment1);

                            titleCell1.Append(titleCellProps1);


                            DocumentFormat.OpenXml.Wordprocessing.TableCellProperties titleCellProps2 = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(
                            new TableCellWidth() { Width = ((UInt32)(workWidth / 2)).ToString(), Type = TableWidthUnitValues.Dxa });
                            TableCellVerticalAlignment verticalAlignment2 = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
                            titleCellProps2.Append(verticalAlignment2);

                            titleCell2.Append(titleCellProps2);


                            DocumentFormat.OpenXml.Wordprocessing.Paragraph imageParagraph1 = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                            DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties paragraphProperties1 = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();

                            // Set indentation to zero
                            paragraphProperties1.Append(new Indentation() { Left = "0", Right = "0" });

                            // Set spacing to zero
                            paragraphProperties1.Append(new SpacingBetweenLines() { Before = "0", After = "0" });

                            imageParagraph1.Append(paragraphProperties1);

                            imageCell1.Append(imageParagraph1);

                            DocumentFormat.OpenXml.Wordprocessing.Paragraph imageParagraph2 = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                            DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties paragraphProperties2 = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();

                            // Set indentation to zero
                            paragraphProperties2.Append(new Indentation() { Left = "0", Right = "0" });

                            // Set spacing to zero
                            paragraphProperties2.Append(new SpacingBetweenLines() { Before = "0", After = "0" });

                            imageParagraph2.Append(paragraphProperties2);

                            imageCell2.Append(imageParagraph2);

                            DocumentFormat.OpenXml.Wordprocessing.TableCellProperties cellProperties1 = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties();
                            TableCellMargin cellMargin1 = new TableCellMargin()
                            {
                                TopMargin = new TopMargin() { Width = "250", Type = TableWidthUnitValues.Dxa },
                                BottomMargin = new BottomMargin() { Width = "0", Type = TableWidthUnitValues.Dxa },
                                LeftMargin = new LeftMargin() { Width = "0", Type = TableWidthUnitValues.Dxa },
                                RightMargin = new RightMargin() { Width = "0", Type = TableWidthUnitValues.Dxa }
                            };
                            cellProperties1.Append(cellMargin1);
                            imageCell1.Append(cellProperties1);

                            DocumentFormat.OpenXml.Wordprocessing.TableCellProperties cellProperties2 = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties();
                            TableCellMargin cellMargin2 = new TableCellMargin()
                            {
                                TopMargin = new TopMargin() { Width = "250", Type = TableWidthUnitValues.Dxa },
                                BottomMargin = new BottomMargin() { Width = "0", Type = TableWidthUnitValues.Dxa },
                                LeftMargin = new LeftMargin() { Width = "0", Type = TableWidthUnitValues.Dxa },
                                RightMargin = new RightMargin() { Width = "0", Type = TableWidthUnitValues.Dxa }
                            };
                            cellProperties2.Append(cellMargin2);
                            imageCell2.Append(cellProperties2);

                            nImage += 2;
                            if (nImage >= images.Count)
                                break;
                        }

                        body.Append(table);
                        if (i < nPages - 1) 
                        {
                            DocumentFormat.OpenXml.Wordprocessing.Paragraph pageBreakParagraph = 
                                new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                                    new DocumentFormat.OpenXml.Wordprocessing.Run(
                                        new DocumentFormat.OpenXml.Wordprocessing.Break() 
                                        { 
                                            Type = BreakValues.Page 
                                        }
                                    )
                                );
                            body.Append(pageBreakParagraph);
                        }
                    }

                    mainPart.Document.Append(body);
                    //mainPart.Document.Save();
                    wordDocument.Save();
                }
                string guid = Guid.NewGuid().ToString();
                string executableLocation = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string docXPath = @"D:\temp\" + guid + @".docx";
                memoryStream.Position = 0;
                SaveDocxToFile(memoryStream.ToArray(), docXPath);
                string pdfPath = @"D:\temp\" + guid + @".pdf";
                var test = new ReportGenerator(_configuration["AppSettings:soffice"]);
                test.Convert(docXPath, pdfPath);
                Thread.Sleep(2000);
                byte [] pdfData = System.IO.File.ReadAllBytes(pdfPath);
                if (pdfData != null)
                {
                    var contentType = "application/pdf";



                    Response.Headers.Add("Content-Disposition", $"attachment; filename*=UTF-8''{guid + ".pdf"}");
                    return File(pdfData, contentType);
                }
                else

                {
                    return BadRequest("PDF Failed");
                }
                /*
                string fileName = $"Photo_{yearMonth}_{Guid.NewGuid():N}.docx";
                memoryStream.Position = 0;
                // Return the document as a downloadable file
                return File(
                    memoryStream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    fileName
                );
                */
                /*
                try
                {
                    //using (var stream = new MemoryStream())
                    {
                        //stream.Position = 0;
                        memoryStream.Position = 0;
                        string guid = Guid.NewGuid().ToString();
                        
                        string docxPath = @"D:\temp\" + guid + @".docx";
                        try
                        {
                            
                            memoryStream.Position = 0;
                            
                            using (MemoryStream docxStream = new MemoryStream(memoryStream.ToArray()))
                            {
                                using (FileStream fileStream = new FileStream(docxPath, FileMode.Create, FileAccess.Write))
                                {
                                    docxStream.WriteTo(fileStream);
                                }
                            }

                            Console.WriteLine("Document saved successfully.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }
                        
                        return File(memoryStream.ToArray(), "applicationd/docx", docxPath);
                    }
                
                    
                        string pdfPath = @"D:\temp\" + guid + @".pdf";
                        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(memoryStream, false))
                        {
                            PdfDocument pdf = new PdfDocument();
                            PdfPage pdfPage = pdf.AddPage();
                            XGraphics gfx = XGraphics.FromPdfPage(pdfPage);

                            // Read text from Word document
                            var text = ReadTextFromWordDocument(wordDoc);
                            gfx.DrawString(text, new XFont("Verdana", 20, XFontStyleEx.Regular), XBrushes.Black,
                                new XRect(0, 0, pdfPage.Width, pdfPage.Height),
                                XStringFormats.TopLeft);

                            // Save the PDF
                            using (var pdfStream = new MemoryStream())
                            {
                                pdf.Save(pdfStream);
                                pdfStream.Position = 0;
                                return File(pdfStream.ToArray(), "application/pdf", pdfPath);
                            }
                        }
                    }
                    
                }
                catch(Exception ex)
                {
                    return BadRequest("PDF Failed ex:" + ex.Message);
                }
            */
                /*
                try
                {
                    string guid = Guid.NewGuid().ToString();
                    string docXPath = @"D:\temp\" + guid + @".docx";
                    memoryStream.Position = 0;
                    SaveDocxToFile(memoryStream.ToArray(), docXPath);
                    string pdfPath = @"D:\temp\" + guid + @".pdf";
                    Spire.Doc.Document document = new Spire.Doc.Document();
                    document.LoadFromFile(docXPath);

                    document.SaveToFile(pdfPath, FileFormat.PDF);
                    byte[] pdfData = System.IO.File.ReadAllBytes(pdfPath);
                    System.IO.File.Delete(docXPath);
                    System.IO.File.Delete(pdfPath);
                    if (pdfData != null)
                    {
                        var contentType = "application/pdf";

                        Response.Headers.Add("Content-Disposition", $"attachment; filename*=UTF-8''{guid + ".pdf"}");
                        return File(pdfData, contentType);
                    }
                    else
                    {
                        return BadRequest("PDF [] null");
                    }

                }
                catch (Exception ex)
                {
                    return BadRequest("PDF Failed ex:" + ex.Message);
                }
                */
                // Return the document as a downloadable file
                //return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "document.docx");
            }
            return Ok();
        }

        private string ReadTextFromWordDocument(WordprocessingDocument wordDoc)
        {
            string text = "";
            var docText = wordDoc.MainDocumentPart.Document.InnerText;
            return docText;
        }

        private DocumentFormat.OpenXml.Wordprocessing.TableCell CreateTitleCell(string text)
        {
            DocumentFormat.OpenXml.Wordprocessing.TableCell cell = new DocumentFormat.OpenXml.Wordprocessing.TableCell();

            DocumentFormat.OpenXml.Wordprocessing.Paragraph titleParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties paraProps = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();

            Justification justification = new Justification() { Val = JustificationValues.Center };
            paraProps.Append(justification);

            titleParagraph.Append(paraProps);

            DocumentFormat.OpenXml.Wordprocessing.Text runText = new DocumentFormat.OpenXml.Wordprocessing.Text(text);
            // Create a run with the title text
            DocumentFormat.OpenXml.Wordprocessing.Run titleRun = new DocumentFormat.OpenXml.Wordprocessing.Run();

            DocumentFormat.OpenXml.Wordprocessing.RunProperties runProperties = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();

            // Set the font
            string fontName = "標楷體";
            int fontSize = 12;
            RunFonts runFonts = new RunFonts() { Ascii = fontName, HighAnsi = fontName, EastAsia = fontName };
            runProperties.Append(runFonts);

            // Set the font size (font size in OpenXML is in half-point units, so fontSize * 2)
            DocumentFormat.OpenXml.Wordprocessing.FontSize size = new DocumentFormat.OpenXml.Wordprocessing.FontSize() { Val = (fontSize * 2).ToString() };  // Convert to half-point size
            runProperties.Append(size);

            // Optional: Set bold or other properties if needed
            // runProperties.Append(new Bold());

            // Apply run properties to the run
            titleRun.Append(runProperties);
            titleRun.Append(runText);


            // Append the run to the paragraph
            titleParagraph.Append(titleRun);

            cell.Append(titleParagraph);

            // Set table cell properties (optional, e.g., width)
            DocumentFormat.OpenXml.Wordprocessing.TableCellProperties cellProps = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties();
            cell.Append(cellProps);

            return cell;
        }

        private DocumentFormat.OpenXml.Wordprocessing.TableCell CreateEmptyCell()
        {
            // Create an empty cell with no content
            DocumentFormat.OpenXml.Wordprocessing.TableCell cell = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
            cell.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(""))));

            return cell;
        }
        private DocumentFormat.OpenXml.Wordprocessing.TableCell CreateImageCell(MainDocumentPart mainPart, byte[] imageBytes, long cellWidth, long cellHeight)
        {
            // Add the image part
            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);
            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                imagePart.FeedData(stream);
            }

            string imagePartId = mainPart.GetIdOfPart(imagePart);

            // Create the image element
            Drawing imageDrawing = CreateImageElement(imagePartId, cellWidth, cellHeight); // Adjust width/height as needed


            DocumentFormat.OpenXml.Wordprocessing.TableCell cell = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
            cell.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(imageDrawing)));


            DocumentFormat.OpenXml.Wordprocessing.TableCellProperties cellProps = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(
                new TableCellWidth() { Width = cellWidth.ToString(), Type = TableWidthUnitValues.Dxa } // Width in Twips
            );
            cell.AppendChild(cellProps);


            //cell.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(imageDrawing)));

            return cell;
        }

        private Drawing CreateImageElement(string imagePartId, long width, long height)
        {
            float desiredWidth = width; // Width in Points
            float desiredHeight = height;// (desiredWidth * 9) / 16;

            return new Drawing(
                new Inline(
                    new Extent() { Cx = (long)desiredWidth, Cy = (long)desiredHeight },
                    new EffectExtent()
                    {
                        LeftEdge = 0L,
                        TopEdge = 0L,
                        RightEdge = 0L,
                        BottomEdge = 0L
                    },
                    new DocProperties() { Id = (UInt32Value)1U, Name = "Picture" },
                    //new A.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks() { NoChangeAspect = true }),
                    /*
                    new A.Graphic(
                        new A.GraphicData(

                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "Image" },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip() { Embed = imagePartId, CompressionState = A.BlipCompressionValues.Print },
                                    new A.Stretch(new A.FillRectangle())),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset() { X = 0L, Y = 0L },
                                        new A.Extents() { Cx = (long)desiredWidth, Cy = (long)desiredHeight }),
                                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle })
                            )

                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    ),
                    */
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "Image" },
                                    new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip() { Embed = imagePartId },//, CompressionState = A.BlipCompressionValues.Print },
                                    new A.Stretch(new A.FillRectangle())
                                ),
                                new PIC.ShapeProperties(new A.Transform2D(
                                    new A.Offset() { X = 0L, Y = 0L },
                                    new A.Extents() { Cx = (long)(desiredWidth), Cy = (long)(desiredHeight) }),
                                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle })
                                
                            )
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    )
                    
                )
            );
        }

        /*
        private List<byte[]> GetImagesFromFile()
        {
            var images = new List<byte[]>();

            var imagePaths = new List<string>
            {
                "D:\\Images3\\1.jpg",
                "D:\\Images3\\2.jpg",
                "D:\\Images3\\3.jpg",
                "D:\\Images3\\4.jpg",
                "D:\\Images3\\5.jpg",
                "D:\\Images3\\6.jpg",
                "D:\\Images3\\7.jpg",
                "D:\\Images3\\8.jpg",
                "D:\\Images3\\1.jpg",
                "D:\\Images3\\2.jpg",
                "D:\\Images3\\3.jpg",
                "D:\\Images3\\4.jpg",
                "D:\\Images3\\5.jpg",
                "D:\\Images3\\6.jpg",
                "D:\\Images3\\7.jpg",
                //"D:\\Images3\\8.jpg",
            };

            for (int i = 0; i < imagePaths.Count; i++)
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(imagePaths[i]);
                byte[] arr;
                using (MemoryStream ms = new MemoryStream())
                {
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    img.Save("d:\\images3\\test" + i + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    arr = ms.ToArray();
                    images.Add(arr);
                }


            }
            return images;
        }
        */

        static double mm = 56.695238095;
        private SectionProperties SetDocumentMargins(MainDocumentPart mainPart)
        {
            SectionProperties sectionProps = new SectionProperties();

            PageSize pageSize = new PageSize()
            {
                Width = 11906, // A4 width in twips 210 11906/210
                Height = 16838, // A4 height in twips 297  16838/297
                Orient = PageOrientationValues.Portrait
            };

            // Define the margins (in twips)
            PageMargin pageMargin = new PageMargin()
            {
                Top = (int)(mm * 10),    // 1 inch (1440 twips)
                Bottom = (int)(mm * 7), // 1 inch (1440 twips)
                Left = (UInt32)(mm * 10),   // 1 inch (1440 twips)
                Right = (UInt32)(mm * 10),  // 1 inch (1440 twips)
                //Header = 720,  // Optional: Distance from the top to the header (0.5 inch)
                //Footer = 720,  // Optional: Distance from the bottom to the footer (0.5 inch)
                Gutter = 0     // No gutter
            };

            // Add page size and margins to section properties
            sectionProps.Append(pageSize);
            sectionProps.Append(pageMargin);

            return sectionProps;
            // Apply the section properties to the document body
            //mainPart.Document.Body.Append(sectionProps);
        }
        /*
        public async Task<IActionResult> GetPhotos(string yearMonth)
        {
            int numberOfRecords = 10; // Assume this value comes from a database

            // Specify paths to your image files
            var imagePaths = new List<string>
        {
            "D:\\Images3\\1.jpg",
            "D:\\Images3\\2.jpg",
            "D:\\Images3\\3.jpg",
            "D:\\Images3\\4.jpg",
            "D:\\Images3\\5.jpg",
            "D:\\Images3\\6.jpg",
            "D:\\Images3\\7.jpg",
            "D:\\Images3\\8.jpg",
            "D:\\Images3\\1.jpg",
            "D:\\Images3\\2.jpg"
        };

            // Create a new DOCX file
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create("d:\\temp\\TableDocument.docx", DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                // Add a main document part
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                new DocumentFormat.OpenXml.Wordprocessing.Document(new DocumentFormat.OpenXml.Wordprocessing.Body()).Save(mainPart);

                for (int i = 0; i < numberOfRecords; i += 3)
                {
                    DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();

                    DocumentFormat.OpenXml.Wordprocessing.TableProperties tblProperties = new DocumentFormat.OpenXml.Wordprocessing.TableProperties(new TableBorders(
                        new DocumentFormat.OpenXml.Wordprocessing.TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new DocumentFormat.OpenXml.Wordprocessing.BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new DocumentFormat.OpenXml.Wordprocessing.LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new DocumentFormat.OpenXml.Wordprocessing.RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                    ));

                    table.AppendChild(tblProperties);

                    // Add rows for titles and photos
                    for (int row = 0; row < 6; row++)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                        if (row % 2 == 0) // Title row
                        {
                            tr.Append(new TableRowHeight() { Val = 540000 }); // Set title height to 1.5 cm
                        }
                        else // Photo row
                        {
                            tr.Append(new TableRowHeight() { Val = 1980000 }); // Set photo height to 5.5 cm
                        }

                        for (int col = 0; col < 2; col++)
                        {
                            DocumentFormat.OpenXml.Wordprocessing.TableCell tc = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
                            if (row % 2 == 0) // Title rows
                            {
                                // Set title cell text
                                tc.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Title {i + row / 2 + 1}"))));
                            }
                            else // Photo rows
                            {
                                if (i + row / 2 < imagePaths.Count)
                                {
                                    string imagePath = imagePaths[i + row / 2];
                                    string imagePartId = AddImageToMainPart(mainPart, imagePath);
                                    AddImageToCell(tc, mainPart, imagePartId);
                                }
                                else
                                    tc.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Title {i + row / 2 + 1}"))));
                            }
                            tr.Append(tc);
                        }
                        table.Append(tr);
                    }

                    // Add the table to the document
                    DocumentFormat.OpenXml.Wordprocessing.Body body = mainPart.Document.Body;
                    body.Append(table);

                    // Add a page break after each table, except for the last one
                    if (i + 3 < numberOfRecords)
                    {
                        body.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Break() { Type = BreakValues.Page })));
                    }
                }

                // Save changes to the main document part
                mainPart.Document.Save();
                
            }
            return Ok("OK");
        }

        private static string AddImageToMainPart(MainDocumentPart mainPart, string imagePath)
        {
            using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg); // Adjust image type as necessary
                imagePart.FeedData(stream);
                return mainPart.GetIdOfPart(imagePart);
            }
        }

        private static void AddImageToCell(DocumentFormat.OpenXml.Wordprocessing.TableCell cell, MainDocumentPart mainPart, string imagePartId)
        {
            // Create a drawing element (image)
            var drawing = new Drawing(
            new Inline(
                new Extent() { Cx = 990000L, Cy = 1980000L }, // Set image size (width and height in EMU)
                new NonVisualGraphicFrameProperties(new NonVisualDrawingProperties()
                {
                    Id = (UInt32Value)1U,
                    Name = "Picture"
                }),

                new Graphic(new GraphicData(
                    new Stretch(
                        new FillRectangle()),
                    new BlipFill(new Blip() { Embed = imagePartId })
                    {
                        //Stretch = new Stretch(new FillRectangle())
                    })
                { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                )),
                new EffectExtent() { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 }
            );


            cell.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(drawing)));
        }
        */



        [HttpGet("document/Download")]
        public async Task<IActionResult> GetDocument(string stationId, string location, string tableType, string recordtime)
        {
            //StreamWriter file0 = new StreamWriter(@"D:\ApiDebug\QueryDoc_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".txt");
            // file0.WriteLine("2024/08");
            //file0.Close();

            StreamWriter file = new StreamWriter(@"D:\ApiDebug\Doc_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".txt");
            file.WriteLine("Begin");
            file.WriteLine(tableType);
            file.Flush();

            string[] tableTypes = tableType.Replace("(", "").Replace(")", "").Trim().Split(' ');
            file.WriteLine("len:" + tableTypes.Length);
            file.Flush();
            try
            {

                file.WriteLine(location);

                //location = WebUtility.UrlDecode(location);
                file.WriteLine(location);
                string stationName = getStationName(stationId);
                SqlHelper sqlHelper = new SqlHelper(_connection);

                string lastDate = GetLastData(stationId, tableTypes[0], location, recordtime);
                if (lastDate.Length > 10)
                    lastDate = lastDate.Substring(0, 10);
                lastDate = getChineseDate(lastDate);
                file.WriteLine("lastDate:" + lastDate);
                string sql0 = @"
                    SELECT
                          [FieldValue],[FieldName]
                      FROM [FormData] a
                      
                      where tabletype=@tableType and recordtime=@recordtime and StationId=@stationId and location=@location and (FieldName='simplefileno' or FieldName='fileno')
                ";
                /*2024-09-14 begin
                string sql1 = @"
                    SELECT
      a.[FieldName]
      ,a.[FieldValue]
  FROM [FormData] a
  inner join [dbo].[FormData] b
  on a.Location=b.Location and a.StationId=b.StationId and a.TableType=b.TableType and a.RecordTime=b.RecordTime

   where  b.recordtime=@recordtime and b.FieldValue=@simpleFileNo and a.fieldvalue <> '一'
order by fieldname
                    ";
                */

                string sql2 = @"Select [DocXContent] from WordDocXBinary where fileNo=@simpleFileNo";
                file.WriteLine(tableTypes[0]);
                file.WriteLine(stationId);
                file.WriteLine(location);
                file.WriteLine(recordtime);

                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@tabletype", tableTypes[0]));
                parameters.Add(new SqlParameter("@stationId", stationId));
                parameters.Add(new SqlParameter("@location", location));
                parameters.Add(new SqlParameter("@recordtime", recordtime));
                DataTable dt = sqlHelper.ExecuteQuery(sql0, parameters.ToArray());
                if (dt.Rows.Count <= 0)
                {
                    file.WriteLine("No FileNo Found");
                    file.Flush();
                    return BadRequest("No FileNo Found");
                }

                string simpleFileNo = "";// dt.Rows[0]["simplefileNo"].ToString();
                string fileNo = "";// dt.Rows[0]["fileNo"].ToString();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i][1].ToString().ToLower() == "simplefileno")
                    {
                        simpleFileNo = dt.Rows[i]["fieldValue"].ToString();
                    }
                    else if (dt.Rows[i][1].ToString().ToLower() == "fileno")
                    {
                        fileNo = dt.Rows[i]["fieldValue"].ToString();
                    }
                }
                file.WriteLine("fileNo:" + fileNo);
                file.WriteLine("simpleFileNo:" + simpleFileNo);
                file.Flush();
                dt.Clear();
                parameters.Clear();
                parameters.Add(new SqlParameter("@recordtime", recordtime));
                parameters.Add(new SqlParameter("@simplefileno", simpleFileNo));
                dt = sqlHelper.ExecuteStoreProcedureQuery("sp_getDocXFormData", parameters.ToArray());
                Dictionary<string, string> dicFormValue = new Dictionary<string, string>();
                Dictionary<string, byte[]> dicSignature = new Dictionary<string, byte[]>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (!dicFormValue.ContainsKey(@"{{" + dt.Rows[i]["fieldName"].ToString() + @"}}"))
                        dicFormValue.Add(@"{{" + dt.Rows[i]["fieldName"].ToString() + @"}}", dt.Rows[i]["fieldValue"].ToString());
                    else if (dt.Rows[i]["fieldName"].ToString().ToLower() == "memos")
                        dicFormValue[@"{{" + dt.Rows[i]["fieldName"].ToString() + @"}}"] += "; " + dt.Rows[i]["fieldValue"].ToString();
                }
                if (!dicFormValue.ContainsKey(@"{{Station}}"))
                {
                    dicFormValue.Add(@"{{Station}}", stationName);
                }
                if (!dicFormValue.ContainsKey(@"{{LastDate}}"))
                {
                    file.WriteLine("add:" + lastDate);
                    dicFormValue.Add(@"{{LastDate}}", lastDate);
                }
                else
                {
                    file.WriteLine("update:" + lastDate);
                    dicFormValue[@"{{LastDate}}"] = lastDate;
                }

                if (dicFormValue.ContainsKey(@"{{manager}}"))
                {
                    //string manager = GetManager(dicFormValue[@"{{manager}}"]);
                    //dicFormValue[@"{{manager}}"]= manager;
                    dicSignature[dicFormValue[@"{{manager}}"].ToLower()] = GetManager(dicFormValue[@"{{manager}}"].ToLower());
                }

                if (dicFormValue.ContainsKey(@"{{maintainer}}"))
                {
                    GetMaintainer(ref dicSignature, dicFormValue[@"{{maintainer}}"].ToLower());

                }

                if (dicFormValue.ContainsKey(@"{{CurrDate}}"))
                {
                    string currDate = dicFormValue[@"{{CurrDate}}"];
                    string chineseDate = getChineseDate(currDate);
                    dicFormValue[@"{{CurrDate}}"] = chineseDate;
                }



                dt.Clear();
                parameters.Clear();
                parameters.Add(new SqlParameter("@simplefileno", simpleFileNo));
                dt = sqlHelper.ExecuteQuery(sql2, parameters.ToArray());
                byte[] docxData = null;
                if (dt.Rows.Count > 0)
                {
                    docxData = (byte[])dt.Rows[0]["DocxContent"];
                }
                if (docxData != null)
                {
                    string[] dtData = recordtime.Split('-');
                    string year = (int.Parse(dtData[0]) - 1911).ToString();
                    string month = dtData[1];

                    using (MemoryStream originalMemoryStream = new MemoryStream(docxData))
                    {
                        using (MemoryStream editableMemoryStream = new MemoryStream())
                        {
                            await originalMemoryStream.CopyToAsync(editableMemoryStream);
                            editableMemoryStream.Seek(0, SeekOrigin.Begin);
                            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(editableMemoryStream, true))
                            {
                                var body = wordDoc.MainDocumentPart.Document.Body;
                                foreach (var paragraph in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                                {
                                    //ParagraphProperties paragraphProperties = paragraph.ParagraphProperties;
                                    foreach (var kvp in dicFormValue)
                                    {

                                        if (paragraph.InnerText.IndexOf(kvp.Key) != -1)
                                        {
                                            if (kvp.Key == @"{{CurrDate}}" || kvp.Key == @"{{LastDate}}")
                                            {
                                                paragraph.InnerXml = paragraph.InnerXml.Replace("維護日期：", "維護日期:");
                                                //paragraph.InnerXml = paragraph.InnerXml.Replace(kvp.Key, $"\u00A0{kvp.Key}");
                                                NoBreakHyphen noBreakHyphen = new NoBreakHyphen();
                                                paragraph.Append(noBreakHyphen);
                                                /*
                                                string xml= paragraph.InnerXml;
                                                int nIndex1 = xml.LastIndexOf("<w:ind w:left=\"");
                                                if (nIndex1 != -1)
                                                {
                                                    xml = xml.Substring(nIndex1 + "<w:ind w:left=\"".Length);
                                                    int nIndex2 = xml.IndexOf("\"");
                                                    if (nIndex2!=-1)
                                                    {
                                                        string strlength=xml.Substring(0,nIndex2).Trim();
                                                        int length = 0;
                                                        int.TryParse(strlength, out length);
                                                        if (length > 0)
                                                        {
                                                            string chnagedXml=paragraph.InnerXml.Replace("<w:ind w:left=\"" + length + "\"", "<w:ind w:left=\"" + (int)(length*0.9) + "\"");
                                                            paragraph.InnerXml = chnagedXml;
                                                        }
                                                    }
                                                    
                                                }
                                                */
                                            }
                                            var originalProperties = paragraph.ParagraphProperties?.CloneNode(true);
                                            if (kvp.Key == @"{{manager}}")
                                            {
                                                var run = paragraph.Descendants<DocumentFormat.OpenXml.Wordprocessing.Run>().FirstOrDefault(r => r.InnerText.Contains(@"{{manager}}"));
                                                if (run != null)
                                                {
                                                    var textElement = run.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>()
                                                            .FirstOrDefault(t => t.Text.Contains(@"{{manager}}"));

                                                    if (textElement != null)
                                                    {
                                                        long yOffset = 0;
                                                        long.TryParse(_configuration["AppSettings:yOffset"], out yOffset);
                                                        if (dicSignature[dicFormValue[@"{{manager}}"].ToLower()] != null)
                                                        {
                                                            string[] parts = textElement.Text.Split(new string[] { @"{{manager}}" }, StringSplitOptions.None);

                                                            textElement.Text = parts[0];


                                                            var element = CreateDrawingElement(wordDoc.MainDocumentPart, dicSignature[dicFormValue[@"{{manager}}"].ToLower()], 180, 30, 0, yOffset);
                                                            run.InsertAfter(element, textElement);




                                                            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                                                            {
                                                                DocumentFormat.OpenXml.Wordprocessing.Text afterText = new DocumentFormat.OpenXml.Wordprocessing.Text(parts[1].Trim());
                                                                run.InsertAfter(afterText, element);
                                                            }
                                                        }
                                                    }

                                                }
                                            }
                                            else if (kvp.Key == @"{{maintainer}}")
                                            {
                                                var run = paragraph.Descendants<DocumentFormat.OpenXml.Wordprocessing.Run>().FirstOrDefault(r => r.InnerText.Contains(@"{{maintainer}}"));
                                                if (run != null)
                                                {
                                                    string[] users = dicFormValue[@"{{maintainer}}"].Split(',');
                                                    int nUsers = 0;
                                                    for (int i = 0; i < users.Length; i++)
                                                    {
                                                        if (dicSignature[users[i].ToLower()] != null)
                                                        {
                                                            nUsers++;
                                                        }
                                                    }
                                                    if (nUsers > 0)
                                                    {
                                                        var textElement = run.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>()
                                                                .FirstOrDefault(t => t.Text.Contains(@"{{maintainer}}"));

                                                        if (textElement != null)
                                                        {
                                                            string[] parts = textElement.Text.Split(new string[] { @"{{maintainer}}" }, StringSplitOptions.None);
                                                            textElement.Text = parts[0];

                                                            int k = 0;
                                                            long yOffset = 0;
                                                            long.TryParse(_configuration["AppSettings:yOffset"], out yOffset);
                                                            for (int i = 0; i < users.Length; i++)
                                                            {
                                                                if (k == 0)
                                                                {
                                                                    if (dicSignature[users[i].ToLower()] != null)
                                                                    {
                                                                        var element1 = CreateDrawingElement(wordDoc.MainDocumentPart, dicSignature[users[i].ToLower()], 120, 30, 0, yOffset);

                                                                        run.InsertAfter(element1, textElement);
                                                                        k++;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (dicSignature[users[i].ToLower()] != null)
                                                                    {
                                                                        var element2 = CreateDrawingElement(wordDoc.MainDocumentPart, dicSignature[users[i].ToLower()], 120, 30, i * 100000, yOffset);
                                                                        run.Append(element2);
                                                                    }
                                                                }

                                                            }
                                                            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                                                            {
                                                                DocumentFormat.OpenXml.Wordprocessing.Text afterText = new DocumentFormat.OpenXml.Wordprocessing.Text(parts[1]);
                                                                run.Append(afterText);
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                            else if (kvp.Key == @"{{memos}}")
                                            {
                                                string innerXml = paragraph.InnerXml;

                                                string[] memos = kvp.Value.Split(';');
                                                ///paragraph.RemoveAllChildren<DocumentFormat.OpenXml.Wordprocessing.Run>();
                                                ///DocumentFormat.OpenXml.Wordprocessing.Run run = new DocumentFormat.OpenXml.Wordprocessing.Run();//
                                                ///
                                                //RemoveBreakElements(paragraph);
                                                var run = paragraph.Descendants<DocumentFormat.OpenXml.Wordprocessing.Run>().FirstOrDefault(r => r.InnerText.Contains(@"{{memos}}"));

                                                if (run != null)
                                                {
                                                    //innerXml = paragraph.InnerXml.Replace(kvp.Key, "");
                                                    //paragraph.InnerXml = innerXml;
                                                    //for (int i=0;i<memos.Length;i++)
                                                    {
                                                        run.Append(new DocumentFormat.OpenXml.Wordprocessing.Break());
                                                        run.Append(new DocumentFormat.OpenXml.Wordprocessing.Text(kvp.Value));
                                                        //run.Append(new DocumentFormat.OpenXml.Wordprocessing.Text(memos[i]));
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                string innerXml = paragraph.InnerXml;


                                                string value = kvp.Value;

                                                file.WriteLine(kvp.Key + ":" + value);
                                                innerXml = innerXml.Replace(kvp.Key, value);
                                                innerXml = innerXml.Replace("：", ":");
                                                paragraph.InnerXml = innerXml;
                                            }
                                            if (originalProperties != null)
                                            {
                                                paragraph.ParagraphProperties = (DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties)originalProperties;
                                            }
                                            //dicFormValue.Remove(kvp.Key);
                                            //break;
                                        }

                                    }


                                }

                                foreach (var paragraph in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                                {
                                    int nIndex1;
                                    int nIndex2;

                                    while (true)
                                    {
                                        if ((nIndex1 = paragraph.InnerText.IndexOf(@"{{")) != -1)
                                        {
                                            if ((nIndex2 = paragraph.InnerText.IndexOf(@"}}")) != -1)
                                            {
                                                string text1 = paragraph.InnerText.Substring(nIndex1, nIndex2 - nIndex1 + 2);

                                                paragraph.InnerXml = paragraph.InnerXml.Replace(text1, "");
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                }

                                DocumentFormat.OpenXml.Wordprocessing.Paragraph lastParagraph = body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().LastOrDefault();

                                if (lastParagraph != null && string.IsNullOrWhiteSpace(lastParagraph.InnerText))
                                {
                                    lastParagraph.Remove();
                                }


                                wordDoc.MainDocumentPart.Document.Save();
                            }
                            file.Flush();
                            editableMemoryStream.Position = 0;
                            string guid = Guid.NewGuid().ToString();
                            string docXPath = @"D:\temp\" + guid + @".docx";
                            file.WriteLine("docx:" + docXPath);
                            string pdfPath = @"D:\temp\" + guid + @".pdf";
                            file.WriteLine("pdf:" + pdfPath);
                            SaveDocxToFile(editableMemoryStream.ToArray(), docXPath);
                            var test = new ReportGenerator(_configuration["AppSettings:soffice"]);
                            test.Convert(docXPath, pdfPath);
                            var fileName = $"{stationName}";
                            byte[] pdfData = System.IO.File.ReadAllBytes(pdfPath);
                            if (pdfData != null)
                            {
                                var contentType = "application/pdf";



                                Response.Headers.Add("Content-Disposition", $"attachment; filename*=UTF-8''{fileNo + "_" + Uri.EscapeDataString(fileName) + ".pdf"}");
                                return File(pdfData, contentType);
                            }
                            else

                            {
                                return BadRequest("PDF Failed");
                            }
                            /*
                            try
                            {
                                //using (var stream = new MemoryStream())
                                {
                                    //stream.Position = 0;
                                    editableMemoryStream.Position = 0;
                                    string guid = Guid.NewGuid().ToString();
                                    string pdfPath = @"D:\temp\" + guid + @".pdf";

                                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(editableMemoryStream, false))
                                    {
                                        PdfDocument pdf = new PdfDocument();
                                        PdfPage pdfPage = pdf.AddPage();
                                        XGraphics gfx = XGraphics.FromPdfPage(pdfPage);

                                        // Read text from Word document
                                        var text = ReadTextFromWordDocument(wordDoc);
                                        gfx.DrawString(text, new XFont("Verdana", 20, XFontStyleEx.Regular), XBrushes.Black,
                                            new XRect(0, 0, pdfPage.Width, pdfPage.Height),
                                            XStringFormats.TopLeft);

                                        // Save the PDF
                                        using (var pdfStream = new MemoryStream())
                                        {
                                            pdf.Save(pdfStream);
                                            pdfStream.Position = 0;
                                            return File(pdfStream.ToArray(), "application/pdf", pdfPath);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                return BadRequest("PDF Failed ex:" + ex.Message);
                            }
                            */
                            /*
                            editableMemoryStream.Position = 0;
                            string guid = Guid.NewGuid().ToString();
                            string docXPath = @"D:\temp\" + guid + @".docx";
                            file.WriteLine("docx:" + docXPath);
                            string pdfPath = @"D:\temp\" + guid + @".pdf";
                            file.WriteLine("pdf:" + pdfPath);
                            byte[] pdfData = null;
                            try
                            {
                                SaveDocxToFile(editableMemoryStream.ToArray(), docXPath);
                                Spire.Doc.Document document = new Spire.Doc.Document();
                                document.LoadFromFile(docXPath);

                                document.SaveToFile(pdfPath, FileFormat.PDF);
                                pdfData = System.IO.File.ReadAllBytes(pdfPath);
                                System.IO.File.Delete(docXPath);
                                System.IO.File.Delete(pdfPath);
                            }
                            catch (Exception ex)
                            {
                                file.WriteLine("ex:" + ex.Message);
                                file.Flush();
                            }
                            //byte[] pdfData = ConvertHtmlToPdf(htmlContent);
                            //var fileName = $"{fileNo}_{stationName}.pdf";
                            var fileName = $"{stationName}";
                            file.WriteLine(fileName);
                            file.Close();
                            //return File(pdfData, "application/pdf", fileName);
                            if (pdfData != null)
                            {
                                var contentType = "application/pdf";



                                Response.Headers.Add("Content-Disposition", $"attachment; filename*=UTF-8''{fileNo + "_" + Uri.EscapeDataString(fileName) + ".pdf"}");
                                return File(pdfData, contentType);
                            }
                            else

                            {
                                return BadRequest("PDF Failed");
                            }
                            */
                            //return result;
                            /*
                            using (var docxStream = new MemoryStream(editableMemoryStream.ToArray()))
                            {
                                using (var pdfStream = new MemoryStream())
                                {
                                    using (var wordDocument = WordprocessingDocument.Open(docxStream, false))
                                    {
                                       
                                        var writerProperties = new WriterProperties();
                                        // Here you can set various writer properties as needed
                                        using (var writer = new PdfWriter(pdfStream, writerProperties))
                                        {
                                            using (var pdf = new PdfDocument(writer))
                                            {
                                                iText.Layout.Document pdfDoc = new iText.Layout.Document(pdf);
                                                ConverterProperties converterProperties = new ConverterProperties();
                                                HtmlConverter.ConvertToPdf(docxStream, pdfDoc, converterProperties);

                                                
                                                //pdfDoc.Add(new iText.Layout.Element.Paragraph(text));
                                                //pdfDoc.Close();


                                                //pdfStream.Position = 0;
                                                var fileName = $"{fileNo}_{stationName}.pdf";
                                                file.WriteLine(fileName);
                                                file.Close();
                                                //return File(editableMemoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
                                                return File(pdfStream.ToArray(), "application/pdf", fileName);
                                            }

                                        }
                                    }

                                }




                            }
                            */
                        }
                    }
                }
                else
                {
                    file.WriteLine("No DocX Found");
                    file.Close();
                    return BadRequest("No DocX Found");
                }
            }
            catch (Exception ex)
            {
                file.WriteLine(ex.Message);
                file.Close();
                return BadRequest(ex.Message);
            }
        }

        static void RemoveBreakElements(DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph)
        {
            var breaks = paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>()
                                    .SelectMany(run => run.Elements<DocumentFormat.OpenXml.Wordprocessing.Break>().ToList())
                                    .ToList();

            foreach (var breakElement in breaks)
            {
                breakElement.Remove();
            }
        }


        public void SaveDocxToFile(byte[] docxData, string filePath)
        {
            using (MemoryStream docxStream = new MemoryStream(docxData))
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    docxStream.WriteTo(fileStream);
                }
            }
        }

        public MemoryStream SaveDocxToNewMemoryStream(byte[] docxData)
        {
            MemoryStream newStream = new MemoryStream();
            using (MemoryStream docxStream = new MemoryStream(docxData))
            {
                docxStream.CopyTo(newStream);
            }
            newStream.Position = 0; // Reset position to the start
            return newStream;
        }

        /*
        private void word2PDF(object Source, object Target)
        {
            Microsoft.Office.Interop.Word.ApplicationClass MSdoc=null;

            object Unknown = Type.Missing;

            if (MSdoc == null) MSdoc = new Microsoft.Office.Interop.Word.ApplicationClass();

            try
            {
                MSdoc.Visible = false;
                MSdoc.Documents.Open(ref Source, ref Unknown,
                     ref Unknown, ref Unknown, ref Unknown,
                     ref Unknown, ref Unknown, ref Unknown,
                     ref Unknown, ref Unknown, ref Unknown,
                     ref Unknown, ref Unknown, ref Unknown, ref Unknown, ref Unknown);
                MSdoc.Application.Visible = false;
                MSdoc.WindowState = Microsoft.Office.Interop.Word.WdWindowState.wdWindowStateMinimize;

                object format = Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatPDF;

                MSdoc.ActiveDocument.SaveAs(ref Target, ref format,
                        ref Unknown, ref Unknown, ref Unknown,
                        ref Unknown, ref Unknown, ref Unknown,
                        ref Unknown, ref Unknown, ref Unknown,
                        ref Unknown, ref Unknown, ref Unknown,
                       ref Unknown, ref Unknown);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                if (MSdoc != null)
                {
                    MSdoc.Documents.Close(ref Unknown, ref Unknown, ref Unknown);
                    //WordDoc.Application.Quit(ref Unknown, ref Unknown, ref Unknown);
                }
                // for closing the application
                MSdoc.Quit(ref Unknown, ref Unknown, ref Unknown);
            }
        }
        */




        private void SetParagraphJustification(DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph, JustificationValues justification)
        {
            DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties pPr = paragraph.ParagraphProperties ?? new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();

            var alignment = new Justification() { Val = justification };
            pPr.Justification = alignment;

            paragraph.ParagraphProperties = pPr;
        }
    }

    public class MaintainQuery
    {
        public string? Row { get; set; }
        public string? SimpleFileNo { get; set; }
        public string? Recordtime { get; set; }
        public string? Station { get; set; }
        public string? Location { get; set; }
        public string? Tabletype { get; set; }
        public string? FileUrl { get; set; }
        public string? FileNo { get; set; }
        public bool? IsFileAvailable { get; set; } = false;
        public bool? IsMultiTable { get; set; } = false;


    }

    public class PhotoInfo
    {
        public string location { get; set; }
        public string sensorType { get; set; }
        public byte[] image { get; set; }
    }
}

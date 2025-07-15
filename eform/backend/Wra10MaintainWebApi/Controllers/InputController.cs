using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using SqlHelper = SQLHelper.SQLHelper;
using System.Data;
using System.Reflection.PortableExecutable;
using Newtonsoft.Json;
using System.ComponentModel;
//using DocumentFormat.OpenXml.Bibliography;
//using DocumentFormat.OpenXml.EMMA;
using Microsoft.VisualBasic;
using Windows.UI;
using System.Reflection.Metadata;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Bibliography;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using DocumentFormat.OpenXml.Spreadsheet;
using Windows.ApplicationModel.Appointments.DataProvider;
using System.Windows.Forms;
using System.IO;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http.Formatting;
using DocumentFormat.OpenXml.Office2010.Excel;
using SQLHelper;
using Microsoft.AspNetCore.Mvc.Routing;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Drawing;

namespace Wra10MaintainWebApi.Controllers
{
    [ApiController]
    [Route("api/forms")]
    public class InputController : ControllerBase
    {

        private readonly string _connection;
        //private readonly string _connection2;
        private readonly string _connectionWater2022;
        private readonly IConfiguration _configuration;
        private bool imageAppendMode = false;
        public InputController(IConfiguration configuration)//SqlConnection connection, SqlConnection connectionWater2022)
        {
            _configuration = configuration;
            _connection = _configuration.GetConnectionString("DefaultConnection");
            _connectionWater2022 = _configuration.GetConnectionString("Water2022Connection");
            imageAppendMode=_configuration.GetValue<bool>("AppSettings:ImageAppendMode");
        }

        //`${apiUrl}/api/forms/getForm?tabletype=${this.category.value}&stationod=${this.stationName.value}&location=${this.maintainLocation}&date=${this.formDate}`
        [HttpGet("Dummy")]
        public ActionResult Dummy()
        {
            return Ok();
        }
            

        [HttpGet("{formId2}")]
        public IActionResult GetFormDefinition2(int formId)
        {
            try
            {
                SqlHelper sqlHelper = new SqlHelper(_connection);

                var sql = $"SELECT Name, Definition FROM Forms WHERE Id = @FormId";
                List<SqlParameter> parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter("@FormId", formId));

                DataTable dt = sqlHelper.ExecuteQuery(sql, parameters.ToArray());
                if (dt.Rows.Count > 9)
                {
                    var formName = dt.Rows[0][0].ToString();
                    var formDefinition = dt.Rows[0][1].ToString();

                    return Ok(new { Name = formName, Definition = formDefinition });
                }
                else
                {
                    return NotFound();
                }
            
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
            
        }
        /*
        [HttpGet("getFormField")]
        public IActionResult GetFormDefinition(string category, string date, string station)
        {
            try
            {
                _connection.Open();
                var sql = $"SELECT Name, Definition FROM Forms WHERE Id = @FormId";
                var sql2 = $"SELECT ItemName,FieldName FROM ExtraItems WHERE formId = @FormId and  stationId=@stationId order by seq";
                using (var command = new SqlCommand(sql, _connection))
                {
                    command.Parameters.AddWithValue("@FormId", category);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Form form = new Form();
                            var formName = reader.GetString(0);
                            var formDefinition = reader.GetString(1);
                            reader.Close(); 
                            using (var command2 = new SqlCommand(sql2, _connection))
                            {
                                command2.Parameters.AddWithValue("@FormId", category);
                                command2.Parameters.AddWithValue("@StationId", station);
                                using (var reader2 = command2.ExecuteReader())
                                {
                                    while(reader2.Read())
                                    {
                                        var itemName = reader2.GetString(0);
                                        var fieldName="";
                                        if (!reader2.IsDBNull(reader2.GetOrdinal("fieldName")))
                                        {
                                            fieldName = reader2.GetString(1);
                                        }
                                        form.lstExtraItem.Add(new ExtraItem(itemName, fieldName));
                                    }
                                }
                            }
                            form.Name = formName;
                            form.Definition= formDefinition;

                            return Ok(form);

                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }
        }
        */

        public class UploadModels
        {
            public string? category { get; set; }
            public string? formId { get; set; }
            public string? stationId { get; set; }
            public string? date { get; set; }
            public string? guid { get; set; }
            public string? maintainlocation { get; set; }
            public IFormFileCollection? files { get; set; }
            public bool EmbeddedDate { get; set; }
        }

        

        private void UpdateOrInsertFormImage(string category, string stationId, string location, string date, string guid)
        {
            SqlHelper sqlHelper = new SqlHelper(_connection);
            List<SqlParameter> parameters1 = new List<SqlParameter>();
            parameters1.Add(new SqlParameter("@tabletype", category));
            parameters1.Add(new SqlParameter("@stationId", stationId));
            parameters1.Add(new SqlParameter("@recordtime", date));
            parameters1.Add(new SqlParameter("@fieldName", "image"));
            parameters1.Add(new SqlParameter("@fieldValue", guid));
            parameters1.Add(new SqlParameter("@location", location));
            string sql8 = @"update formdata set fieldValue=@fieldValue where tabletype=@tabletype and recordtime=@recordtime and stationId=@stationid and fieldname=@fieldName and Location=@location";
            int nUpdate = sqlHelper.ExecuteNonQuery(sql8, parameters1.ToArray());
            if (nUpdate == 0)
            {
                sql8 = @"insert into formdata (tabletype,stationId,location,recordtime,fieldName,fieldValue)
                                     values (@tabletype,@stationId,@location,@recordtime,@fieldName,@fieldValue)";
                sqlHelper.ExecuteNonQuery(sql8, parameters1.ToArray());
            }
        }

        [HttpPost("files")]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> UploadFiles([FromForm] UploadModels uploadModels)
        {
            try
            {
                SqlHelper sqlHelper = new SqlHelper(_connection);
                List<SqlParameter> parameters = new List<SqlParameter>();
                string guid = "";
                bool bNewGuid = false;
                if (!String.IsNullOrEmpty(uploadModels.guid) && uploadModels.guid != "null")
                {
                    guid = uploadModels.guid;
                }
                else
                {
                    guid = Guid.NewGuid().ToString();
                    bNewGuid = true;
                }
                string date = uploadModels.date;
                foreach (var file in uploadModels.files)
                {
                    if (file.Length > 0)
                    {
                        // Ensure the uploads folder exists
                        var uploadPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }
                        parameters.Clear();
                        // Generate a unique file name
                        var fileName = System.IO.Path.GetRandomFileName() + System.IO.Path.GetExtension(file.FileName);
                        var filePath = System.IO.Path.Combine(uploadPath, fileName);

                        // Save the file to the server

                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            
                            byte [] imageData = await ImageProcessor.ResizeAndAddText(memoryStream.ToArray(), uploadModels.date, uploadModels.EmbeddedDate);
                            //var imageData = memoryStream.ToArray();


                            var insert = "INSERT INTO ImageData (guid, recordtime, image) VALUES (@guid, @recordtime, @image)";
                            parameters.Add(new SqlParameter("@guid", guid));
                            parameters.Add(new SqlParameter("@recordtime", date));
                            parameters.Add(new SqlParameter("@image", imageData));

                            sqlHelper.ExecuteNonQuery(insert, parameters.ToArray());

                        }

                    }
                }
                DataTable dt = sqlHelper.ExecuteQuery("select sequence from formData where fieldName='image' and fieldValue='" + guid + "'");
                if (dt.Rows.Count == 0)
                {
                    UpdateOrInsertFormImage(uploadModels.category, uploadModels.stationId, uploadModels.maintainlocation, uploadModels.date, guid);
                }

                List <ImageData> lstImageData = new List<ImageData>();
                var sql5 = @"select sequence,image from imagedata 
                             where guid=@guId and recordtime=@recordtime";

                List<SqlParameter> parameters2 = new List<SqlParameter>();
                parameters2.Add(new SqlParameter("@guid", guid));
                parameters2.Add(new SqlParameter("@recordtime", date));
                DataTable dt2 = sqlHelper.ExecuteQuery(sql5, parameters2.ToArray());
                for (int j = 0; j < dt2.Rows.Count; j++)
                {
                    string? imageId = dt2.Rows[j]["sequence"].ToString();
                    ImageData id = new ImageData();
                    id.Id = imageId;
                    id.Guid= guid;
                    id.Image = (byte[])dt2.Rows[j]["image"];
                    id.Date = date;
                    lstImageData.Add(id);

                }
                return Ok(lstImageData);
            }         
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class MyFormData
        {
            public DateTime Date { get; set; }
            public List<IFormFile>? Files { get; set; }
            public List<ImageData3>? PreSavedImages { get; set; }
            public bool EmbeddedData { get; set; } = false;
        }

        private byte[] Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            string base64StringWithoutPrefix = base64String.Replace("data:image/jpeg;base64,","").Replace("data:image/png;base64,","");
            byte[] imageBytes = Convert.FromBase64String(base64StringWithoutPrefix);
            // Convert byte[] to Image
            return imageBytes;
        }

        private async Task<byte[]> ConvertIFormFileToByteArray(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
        //string date, List<IFormFile> files, [FromForm] List<ImageData> preSavedImages
        [HttpPost("insertimages")]
        ///public async Task<IActionResult> InsertImages(string date, List<IFormFile> files, [FromForm] List<ImageData> preSavedImages)
        public async Task<IActionResult> InsertImages([FromForm] MyFormData formData)
        {

            try
            {
                string date = formData.Date.ToString("yyyy-MM-dd");
                List<IFormFile> files = formData.Files;
                List<ImageData3> preSavedImages = formData.PreSavedImages;

                var insert = "INSERT INTO ImageData (guid, recordtime, image) VALUES (@guid, @recordtime, @image)";

                SqlHelper sqlHelper = new SqlHelper(_connection);
                sqlHelper.ExecuteNonQuery("delete from imagedata where recordtime='" + date + "'");
                //sqlHelper.ExecuteNonQuery("delete from formData where fieldName='image' and recordtime='" + date + "'");
                string guid = Guid.NewGuid().ToString();

                
                List<SqlParameter> parameters = new List<SqlParameter>();
                /*
                parameters.Add(new SqlParameter("@fieldValue", guid));
                parameters.Add(new SqlParameter("@recordtime", date));
                sqlHelper.ExecuteQuery("Insert into FormData (recordtime,fieldname,fieldValue) values (@recordtime,'image',@fieldValue)",parameters.ToArray());
                */

                if (files != null)
                {
                    int k = 0;
                    foreach (var file in files)
                    {
                        try
                        {
                            parameters.Clear();
                            using var memoryStream = new MemoryStream();
                            await file.CopyToAsync(memoryStream);

                            byte[] imageData = null;
                            
                            await ImageProcessor.ResizeAndAddText(memoryStream.ToArray(), DateTime.Now.ToString("yyyy-MM-dd"), formData.EmbeddedData);

                            parameters.Add(new SqlParameter("@guid", guid));
                            parameters.Add(new SqlParameter("@recordtime", date));
                            parameters.Add(new SqlParameter("@image", imageData));

                            sqlHelper.ExecuteNonQuery(insert, parameters.ToArray());
                        }
                        catch (Exception ex)
                        {

                        }
                        k++;
                    }
                }
                if (preSavedImages != null)
                {
                    int k = 0;
                    foreach (var image in preSavedImages)
                    {
                        try
                        {
                            byte[] byteImage = await ConvertIFormFileToByteArray(image.Image);
                            parameters.Clear();
                            //byte[] byteImage = Base64ToImage(image.Image);
                            //var imageData = await ImageProcessor.ResizeAndAddText(byteImage, DateTime.Now.ToString("yyyy-MM-dd"));

                            byte[] imageData = null;
                            if (preSavedImages[k].Tag?.ToLower() != "save")
                            {
                                imageData = await ImageProcessor.ResizeAndAddText(byteImage, DateTime.Now.ToString("yyyy-MM-dd"),formData.EmbeddedData);
                            }
                            else
                            {
                                imageData = byteImage;
                            }
                            parameters.Add(new SqlParameter("@guid", guid));
                            parameters.Add(new SqlParameter("@recordtime", date));
                            parameters.Add(new SqlParameter("@image", imageData));

                            sqlHelper.ExecuteNonQuery(insert, parameters.ToArray());
                        }
                        catch (Exception ex)
                        {

                        }
                        k++;
                    }
                }

                List<ImageData> lstImageData = new List<ImageData>();
                List<SqlParameter> parameters2 = new List<SqlParameter>();
                parameters2.Add(new SqlParameter("@guid", guid));
                var sql5 = @"select sequence,image,recordtime from imagedata 
                             where guid=@guId";
                DataTable dt2 = sqlHelper.ExecuteQuery(sql5, parameters2.ToArray());
                for (int j = 0; j < dt2.Rows.Count; j++)
                {
                    string? imageId = dt2.Rows[j]["sequence"].ToString();
                    ImageData id = new ImageData();
                    id.Id = imageId;
                    id.Guid = guid;
                    id.Image = (byte[])dt2.Rows[j]["image"];
                    id.Date = dt2.Rows[j]["recordtime"].ToString();
                    lstImageData.Add(id);

                }

                return Ok(lstImageData);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

            //return Ok("");
        }

        [HttpGet("CheckDataExist")]
        public IActionResult CheckDataExist(string tableType, string stationId, string location, string date)
        {
            try
            {
                bool bExist = false;
                string getLastDate = @"select convert(varchar,recordtime,120) as recordtime  from formdata where recordtime=@date and tabletype=@tabletype and stationid=@stationid and location=@location order by recordtime desc";
                SqlHelper sqlHelper = new SqlHelper(_connection);
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@tabletype", tableType));
                parameters.Add(new SqlParameter("@stationId", stationId));
                parameters.Add(new SqlParameter("@date", date));
                parameters.Add(new SqlParameter("@location", location));

                DataTable dt = sqlHelper.ExecuteQuery(getLastDate, parameters.ToArray());
                if (dt.Rows.Count > 0)
                    bExist = true;
                return Ok(bExist);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("savedata")]
        public IActionResult PostFormData([FromBody] object formData)
        {
            if (!Directory.Exists(@"D:\ApiDebug"))
            {
                Directory.CreateDirectory(@"D:\ApiDebug");
            }
            StreamWriter file = new StreamWriter(@"D:\ApiDebug\"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+".txt");

            try
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                file.WriteLine("date:" + date);
                
                string? jsonString = formData.ToString();
                file.WriteLine("jsonString:" + jsonString);
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                DataModel? dataModel = JsonConvert.DeserializeObject<DataModel>(jsonString, settings);
                SqlHelper sqlHelper = new SqlHelper(_connection);
                Dictionary<string, string>? dicData = JsonConvert.DeserializeObject<Dictionary<string, string>>(dataModel.FormData);
                if (dicData.ContainsKey("CurrDate"))
                {
                    dicData["CurrDate"] = dataModel.Date.ToString("yyyy-MM-dd");
                }
                else
                {
                    dicData.Add("CurrDate",dataModel.Date.ToString("yyyy-MM-dd"));
                }

                List<SqlParameter> parameters = new List<SqlParameter>();

                string getLastDate = @"select distinct top (1) convert(varchar,recordtime,120) as recordtime  from formdata where tabletype=@tabletype and location=@location and stationId=@stationid and recordtime < @date order by recordtime desc";
                parameters.Add(new SqlParameter("@date", dataModel.Date));
                parameters.Add(new SqlParameter("@tabletype", dataModel.Category));
                parameters.Add(new SqlParameter("@location", dataModel.MaintainLocation));
                parameters.Add(new SqlParameter("@stationid", dataModel.Station));
                DataTable dt = sqlHelper.ExecuteQuery(getLastDate, parameters.ToArray());
                if (dt.Rows.Count > 0)
                {
                    if (dicData.ContainsKey("LastDate"))
                    {
                        dicData["LastDate"] = dt.Rows[0][0].ToString();
                    }
                }
                else
                {
                    if (dicData.ContainsKey("LastDate"))
                    {
                        dicData["LastDate"] = null;
                    }
                    else
                    {
                        dicData.Add("LastDate", null);
                    }
                }

                string getGuid = @"select fieldValue from formdata where tabletype=@tabletype and location=@location and recordtime=@recordtime and stationId=@stationid and fieldname='image'";
                

                //string update = @"update formdata set @fieldName=@fieldvalue where formid=@formid and recordtime=@recordtime and stationId=@stationid and fieldName=@fieldName";
                //string insert = @"insert into formdata (formid,recordtime,stationid,fieldname,fieldvalue) values (@formid,@recordtime,@stationid,@fieldname,@fieldvalue)";
                parameters.Clear();
                parameters.Add(new SqlParameter("@tabletype", dataModel.Category));
                parameters.Add(new SqlParameter("@stationId", dataModel.Station));
                parameters.Add(new SqlParameter("@recordtime", dataModel.Date));
                parameters.Add(new SqlParameter("@location", dataModel.MaintainLocation));
                sqlHelper.ExecuteStoreProcedureQuery("sp_deleteFormData", parameters.ToArray());
                for (int i = 0; i < dicData?.Count; i++)
                {
                    parameters.Clear();
                    parameters.Add(new SqlParameter("@tabletype", dataModel.Category));
                    parameters.Add(new SqlParameter("@stationId", dataModel.Station));
                    parameters.Add(new SqlParameter("@recordtime", dataModel.Date));
                    parameters.Add(new SqlParameter("@location", dataModel.MaintainLocation));
                    string fieldName = dicData.ElementAt(i).Key;
                    string fieldValue = dicData.ElementAt(i).Value;
                    /*
                    if (i == 0 && dataModel.guid != "")
                    {
                        DataTable dt3 = sqlHelper.ExecuteQuery(getGuid, parameters.ToArray());
                        if (dt3.Rows.Count > 0)
                        {
                            if (!String.IsNullOrEmpty(dt3.Rows[0][0].ToString()))
                            {
                                string? oriGuid = dt3.Rows[0][0].ToString();

                                sqlHelper.ExecuteNonQuery("update imageData set guid='" + oriGuid + "' where recordtime='"+ dataModel.Date + "' and guid='" + dataModel.guid + "'");
                                dataModel.guid = oriGuid;
                            }
                        }
                    }
                    */
                    
                    parameters.Add(new SqlParameter("@fieldName", fieldName));
                    SqlParameter parameter = new SqlParameter("@fieldValue", SqlDbType.NVarChar, -1);
                    if (fieldValue==null)
                        parameter.Value = DBNull.Value;
                    else
                        parameter.Value = fieldValue;
                    parameters.Add(parameter);
                    dt.Clear();
                    if (fieldName != "formDate")
                    {
                        
                        //dt = sqlHelper.ExecuteStoreProcedureQuery("sp_updateFormData", parameters.ToArray());
                        //int nUpdate = 0;
                        //int.TryParse(dt.Rows[0][0].ToString(), out nUpdate);
                        //if (dt.Rows.Count > 0 &&  nUpdate <= 0)
                        {
                            //sqlHelper.ExecuteNonQuery(insert, parameters.ToArray());
                            sqlHelper.ExecuteStoreProcedureQuery("sp_insertFormData", parameters.ToArray());
                        }
                    }
                }
                if (dataModel.maintainer!=null && dataModel.maintainer.Count > 0)
                {
                    try
                    {
                        List<string> data = new List<string>();
                        for (int i = 0; i < dataModel.maintainer.Count; i++)
                        {
                            data.Add(dataModel.maintainer[i].Id);
                        }
                        string joined = string.Join(",", data);
                        List<SqlParameter> parameters1 = new List<SqlParameter>();
                        parameters1.Add(new SqlParameter("@tabletype", dataModel.Category));
                        parameters1.Add(new SqlParameter("@stationId", dataModel.Station));
                        parameters1.Add(new SqlParameter("@recordtime", dataModel.Date));
                        parameters1.Add(new SqlParameter("@fieldName", "maintainer"));
                        parameters1.Add(new SqlParameter("@fieldValue", joined));
                        parameters1.Add(new SqlParameter("@location", dataModel.MaintainLocation));
                        string sql8 = @"update formdata set fieldValue=@fieldValue where tabletype=@tabletype and recordtime=@recordtime and stationId=@stationid and fieldname=@fieldName and Location=@location";
                        int nUpdate = sqlHelper.ExecuteNonQuery(sql8, parameters1.ToArray());
                        if (nUpdate == 0)
                        {
                            sql8 = @"insert into formdata (tabletype,stationId,location,recordtime,fieldName,fieldValue)
                                     values (@tabletype,@stationId,@location,@recordtime,@fieldName,@fieldValue)";
                            sqlHelper.ExecuteNonQuery(sql8, parameters1.ToArray());
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                }
                if (dataModel.manager!=null && dataModel.manager.Count > 0)
                {
                    try
                    {
                        
                        string? managerId = dataModel.manager[0].Id;

                        List<SqlParameter> parameters1 = new List<SqlParameter>();
                        parameters1.Add(new SqlParameter("@tabletype", dataModel.Category));
                        parameters1.Add(new SqlParameter("@stationId", dataModel.Station));
                        parameters1.Add(new SqlParameter("@recordtime", dataModel.Date));
                        parameters1.Add(new SqlParameter("@fieldName", "manager"));
                        parameters1.Add(new SqlParameter("@fieldValue", managerId));
                        parameters1.Add(new SqlParameter("@location", dataModel.MaintainLocation));
                        string sql9 = @"update formdata set fieldValue=@fieldValue where TableType=@TableType and recordtime=@recordtime and stationId=@stationid and fieldname=@fieldName and location=@location";
                        int nUpdate = sqlHelper.ExecuteNonQuery(sql9, parameters1.ToArray());
                        if (nUpdate == 0)
                        {
                            sql9 = @"insert into formdata (tabletype,stationId,location,recordtime,fieldName,fieldValue)
                                     values (@tabletype,@stationId,@location,@recordtime,@fieldName,@fieldValue)";
                            sqlHelper.ExecuteNonQuery(sql9, parameters1.ToArray());
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                file.WriteLine("guid:" + dataModel.guid);
                if (dataModel.guid != "" && dataModel.guid != null)
                {
                    try
                    {
                        string imageData = "Select convert(varchar,recordtime,120) as recordtime from formdata where fieldvalue='" + dataModel.guid + "'";
                        dt.Clear();
                        dt = sqlHelper.ExecuteQuery(imageData);
                        string preImageDate = "";
                        if (dt.Rows.Count > 0)
                        {
                            preImageDate = dt.Rows[0][0].ToString();
                        }
                        file.WriteLine("preImageDate:" + preImageDate);
                        if (preImageDate != "" && DateTime.Parse(preImageDate) != dataModel.Date)
                        {
                            
                            string newGuid = Guid.NewGuid().ToString();
                            file.WriteLine("newGuid:" + newGuid);
                            string sqlDuplicate1 = @"insert into formdata (tabletype,recordtime,stationid,fieldname,fieldvalue,location) 
SELECT tabletype,@newrecordtime,stationid,fieldname,@guid,location
FROM FormData
WHERE fieldValue=@fieldValue and recordTime=@RecordTime";

                            file.WriteLine("newRecordTime:" + dataModel.Date.ToString("yyyy-MM-dd HH:mm:ss"));

                            
                            try
                            {
                                parameters.Clear();
                                parameters.Add(new SqlParameter("@recordtime", dataModel.Date.ToString("yyyy-MM-dd HH:mm:ss")));
                                parameters.Add(new SqlParameter("@location", dataModel.MaintainLocation));
                                parameters.Add(new SqlParameter("@tabletype", dataModel.Category));
                                parameters.Add(new SqlParameter("@stationId", dataModel.Station));
                                string sqlCheckExist = "Select fieldValue from formData where fieldname='image' and tabletype=@tabletype and stationId=@stationId and location=@location and recordtime=@recordtime";

                                DataTable dtCheck = sqlHelper.ExecuteQuery(sqlCheckExist, parameters.ToArray());
                                if (dtCheck.Rows.Count > 0)
                                {
                                    string delGuid = dtCheck.Rows[0][0].ToString();
                                    file.WriteLine("delGuid:" +delGuid);
                                    if (imageAppendMode)
                                    {
                                        sqlHelper.ExecuteNonQuery("update formdata set fieldValue='" + newGuid + "' where fieldValue='" + delGuid + "'");
                                        sqlHelper.ExecuteNonQuery("update imageData set guid='" + newGuid + "' where guid='" + delGuid + "'");

                                    }
                                    else
                                    {

                                        sqlHelper.ExecuteNonQuery("delete formdata where fieldValue='" + delGuid + "'");
                                        sqlHelper.ExecuteNonQuery("delete imageData where guid='" + delGuid + "'");
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                file.WriteLine("check exist image ex:" + ex.Message);
                            }
                            parameters.Clear();
                            parameters.Add(new SqlParameter("@newrecordtime", dataModel.Date.ToString("yyyy-MM-dd HH:mm:ss")));

                            parameters.Add(new SqlParameter("@fieldValue", dataModel.guid));
                            parameters.Add(new SqlParameter("@recordtime", preImageDate));
                            parameters.Add(new SqlParameter("@guid", newGuid));
                            int nUpdate = 0;
                            if (!imageAppendMode)
                            {
                                nUpdate = sqlHelper.ExecuteNonQuery(sqlDuplicate1, parameters.ToArray());
                            }
                            string sqlDuplicate2 = @"INSERT INTO [ImageData]
                                       ([RecordTime]
                                       ,[Guid]
                                       ,[Image])
                                 select @newrecordtime,@newguid,image from imagedata
                                       where RecordTime=@recordtime and guid=@guid
                                       ";
                            parameters.Clear();
                            parameters.Add(new SqlParameter("@newrecordtime", dataModel.Date.ToString("yyyy-MM-dd HH:mm:ss")));
                            parameters.Add(new SqlParameter("@newguid", newGuid));
                            parameters.Add(new SqlParameter("@guid", dataModel.guid));
                            parameters.Add(new SqlParameter("@recordtime", preImageDate));
                            nUpdate = sqlHelper.ExecuteNonQuery(sqlDuplicate2, parameters.ToArray());
                            file.WriteLine("nUpdate:" + nUpdate);
                        }
                    }
                    catch(Exception ex)
                    {
                        file.WriteLine("638 ex:" + ex.Message);
                    }
                }
                //List<SaveDataFormat> lstData= JsonConvert.DeserializeObject<List<SaveDataFormat>>(ld);

                return Ok("Form data received successfully.");
            }
            catch (Exception ex)
            {
                file.WriteLine("647 ex:" + ex.Message);
                return BadRequest(ex.Message);
            }
            finally
            {
                file.Close();
            }
        }

        [HttpGet("schedulequery")]
        public IActionResult GetSheduleQuery(string year)
        {
            try
            {
                SqlHelper sqlHelper = new SqlHelper(_connection);
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@year", year));
                DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_schedulequery", parameters.ToArray());
                List<ScheduleQuery> lstScheduleQuery = new List<ScheduleQuery>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ScheduleQuery sq = new ScheduleQuery()
                    {
                        Row = dt.Rows[i]["row"].ToString(),
                        YearMonth = dt.Rows[i]["yearmonth"].ToString(),
                        item1= dt.Rows[i]["item1"].ToString(),
                        item2 = dt.Rows[i]["item2"].ToString(),
                        item3 = dt.Rows[i]["item3"].ToString(),
                        item4 = dt.Rows[i]["item4"].ToString(),
                        item5 = dt.Rows[i]["item5"].ToString(),
                        item6 = dt.Rows[i]["item6"].ToString(),
                        item7 = dt.Rows[i]["item7"].ToString(),
                        item8 = dt.Rows[i]["item8"].ToString(),
                    };
                    
                    lstScheduleQuery.Add(sq) ;
                }
                return Ok(lstScheduleQuery);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getscheduledata")]
        public IActionResult GetSheduleData(string startYearMonth, string maintainType )
        {
            try
            {

                SqlHelper sqlHelper = new SqlHelper(_connection);
                string sqlSelect = "Select yearmonth  from MaintainSchedule where startyearmonth=@startyearmonth and Maintaintype=@maintaintype";
                //string sqlDelete = "delete from MaintainSchedule where startyearmonth=@startyearmonth and Maintaintype=@maintaintype";
                //string sqlInsert = "insert into MaintainSchedule (startYearMonth,yearmonth,maintaintype) values (@startyearmonth, @yearmonth, @maintaintype)";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Clear();
                parameters.Add(new SqlParameter("@startyearmonth", startYearMonth));
                parameters.Add(new SqlParameter("@maintaintype", maintainType));
                DataTable dt=sqlHelper.ExecuteQuery(sqlSelect, parameters.ToArray());
                List<String> lstSelected = new List<String>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    lstSelected.Add(dt.Rows[i][0].ToString());
                }

                return Ok(lstSelected) ;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("deletescheduledata")]
        public IActionResult DeleteSheduleData(string startYearMonth, string maintainType)
        {
            try
            {

                SqlHelper sqlHelper = new SqlHelper(_connection);
                //string sqlSelect = "Select yearmonth  from MaintainSchedule where startyearmonth=@startyearmonth and Maintaintype=@maintaintype";
                string sqlDelete = "delete from MaintainSchedule where startyearmonth=@startyearmonth and Maintaintype=@maintaintype";
                //string sqlInsert = "insert into MaintainSchedule (startYearMonth,yearmonth,maintaintype) values (@startyearmonth, @yearmonth, @maintaintype)";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Clear();
                parameters.Add(new SqlParameter("@startyearmonth", startYearMonth));
                parameters.Add(new SqlParameter("@maintaintype", maintainType));
                int nDelete=sqlHelper.ExecuteNonQuery(sqlDelete, parameters.ToArray());
                
                return Ok(nDelete);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("savescheduledata")]
        public IActionResult SaveSheduleData([FromBody] object scheduleFormData)
        {
            try
            {
                
                string? jsonString = scheduleFormData.ToString();
                ScheduleForm? scheduleFrom = JsonConvert.DeserializeObject<ScheduleForm>(jsonString);
                ScheduleFormData? scheduleFromdata = JsonConvert.DeserializeObject<ScheduleFormData>(scheduleFrom.scheduleFormData);
                SqlHelper sqlHelper = new SqlHelper(_connection);
                string sqlDelete="delete from MaintainSchedule where startyearmonth=@startyearmonth and Maintaintype=@maintaintype";
                string sqlInsert = "insert into MaintainSchedule (startYearMonth,yearmonth,maintaintype) values (@startyearmonth, @yearmonth, @maintaintype)";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Clear();
                parameters.Add(new SqlParameter("@startyearmonth", scheduleFromdata.startYearMonth));
                parameters.Add(new SqlParameter("@maintaintype", scheduleFromdata.maintainType));
                sqlHelper.ExecuteNonQuery(sqlDelete, parameters.ToArray());
                for (int i = 0; i < scheduleFromdata.selectedDates.Count; i++)
                {
                    parameters.Clear();
                    parameters.Add(new SqlParameter("@startyearmonth", scheduleFromdata.startYearMonth));
                    parameters.Add(new SqlParameter("@maintaintype", scheduleFromdata.maintainType));
                    parameters.Add(new SqlParameter("@yearmonth", scheduleFromdata.selectedDates[i]));
                    sqlHelper.ExecuteNonQuery(sqlInsert, parameters.ToArray());
                }

                return Ok("MaintainSchedule data saved.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("getLocationAndType")]
        public IActionResult GetLocationAndType(string stationId)
        {
            try
            {
                SqlHelper sqlHelper = new SqlHelper(_connection);
                List<SqlParameter> lstParameter = new List<SqlParameter>();
                lstParameter.Add(new SqlParameter("@stationId", stationId));
                DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_getLocationAndTableType", lstParameter.ToArray());

                LocationTabletype lt = new LocationTabletype();
                string prevType = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string tableType = dt.Rows[i]["tabletype"].ToString();
                    if (tableType!=prevType)
                    {
                        lt.lstTypes.Add(new ComboData(dt.Rows[i]["tableName"].ToString(), tableType));
                        prevType = tableType;
                    }
                    if (lt.lstLocations.Find(a=>a.value== dt.Rows[i]["location"].ToString())==null)
                        lt.lstLocations.Add(new ComboData(dt.Rows[i]["location"].ToString(), dt.Rows[i]["location"].ToString()));

                }

                return Ok(lt);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private void removeDBImage(string imageId)
        {

        }

        [HttpPost("removeImage")]
        public IActionResult RemoveImage(string imageId)
        {
            try
            {
                SqlHelper sqlHelper = new SqlHelper(_connection);
                DataTable dt=sqlHelper.ExecuteQuery("select guid from imageData where sequence=" + imageId);
                string guid = "";
                if (dt.Rows.Count > 0)
                {
                    guid = dt.Rows[0][0].ToString();
                }
                sqlHelper.ExecuteNonQuery("delete from imageData where sequence=" + imageId);
                if (!String.IsNullOrEmpty(guid))
                {
                    dt.Clear();
                    dt = sqlHelper.ExecuteQuery("select sequence from imageData where guid='" + guid+"'");
                    if (dt.Rows.Count==0)
                    {
                        sqlHelper.ExecuteNonQuery("delete from FormData where fieldName='image' and fieldValue='" +guid+"'" );
                    }

                }
                return Ok(imageId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("maintainquery")]
        public IActionResult getMaintainHistoryQuery(string startDate, string endDate)
        {
            try
            {
                HistoryData hd = new HistoryData();
                SqlHelper sqlHelper = new SqlHelper(_connection);

                //var sql = $"SELECT Name, Definition FROM Forms WHERE Id = @FormId";
                List<SqlParameter> parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter("@startDate",startDate));
                parameters.Add(new SqlParameter("@endDate", endDate));
                DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_getMaintainHistory", parameters.ToArray());
                string preKeys = "";
                HistoryHeader hh=null;
                int id = 0;
                for (int i=0; i< dt.Rows.Count;i++)
                {
                    string keys=dt.Rows[i]["keys"].ToString();
                    string recordTime = dt.Rows[i]["RecordTime"].ToString().Substring(0,10);
                    string stationId = dt.Rows[i]["stationId"].ToString();
                    string stationName = dt.Rows[i]["stationNameA"].ToString();
                    string tableType = dt.Rows[i]["tableType"].ToString();
                    string tableTypeName = dt.Rows[i]["tableName"].ToString();
                    if (keys != preKeys)
                    {
                        hh = new HistoryHeader();
                        
                        hh.id = id++;
                        hh.recordTime = recordTime;
                        hh.stationId = stationId;
                        hh.stationName = stationName;
                        hd.lstHistoryHeader.Add(hh);
                    }
                    HistoryDetail historyDetail = new HistoryDetail();
                    historyDetail.id = id++;
                    historyDetail.tableType = tableType;
                    historyDetail.tableTypeName=tableTypeName;
                    hh.lstHistoryDetail.Add(historyDetail);
                }
                return Ok(hd);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private IActionResult getFormFields20240802(string tableType, string stationId, string location, string date)
        {
            try
            {
                SqlHelper sqlHelper = new SqlHelper(_connection);

                //var sql = $"SELECT Name, Definition FROM Forms WHERE Id = @FormId";
                List<SqlParameter> parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter("@StationID", stationId));
                parameters.Add(new SqlParameter("@tableType", tableType));
                parameters.Add(new SqlParameter("@location", location));

                DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_getItemForTable", parameters.ToArray());
                if (dt.Rows.Count > 0)
                {
                    var formName = dt.Rows[0][0].ToString();
                    var formDefinition = dt.Rows[0][1].ToString();

                    var sql3 = $"Select fieldName,fieldValue from formdata where tabletype=@tableType and stationId=@stationId and recordtime=@recordtime and location=@location";
                    var sql4 = @"select fieldName,fieldValue,recordtime from formdata a
                             inner join 
                             (Select max(recordtime) as rt from formdata where tableType=@tableType and stationId=@stationId and location=@location) as b on a.recordtime=b.rt
                             where location=@location";
                    var sql5 = @"select sequence,image from imagedata where guid=@guId and recordtime=@recordTime";

                    //var sql6 = @"select managerid,managername from manager";
                    var sql6 = @"select userid as managerid,username as managername from [Water2022].[dbo].[users] where specialuser=9";

                    FormDefNew form = new FormDefNew();
                    form.Name = formName;
                    form.Definition = formDefinition;
                    
                    dt.Clear();
                    parameters.Add(new SqlParameter("@recordtime", date));
                    dt = sqlHelper.ExecuteQuery(sql3, parameters.ToArray());
                    //form.Date = DateTime.Parse(date);
                    string fieldName = "";
                    if (dt.Rows.Count > 0)
                    {
                        
                        form.Date = DateTime.Parse(date);
                        form.PreDate = form.Date;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            fieldName = dt.Rows[i][0].ToString();
                            var fieldValue = "";
                            if (fieldName?.ToLower()=="currdate")
                            {
                                fieldValue = DateTime.Now.ToString("yyyy-MM-dd");
                                form.lstFormData.Add(new FormData(fieldName, fieldValue));
                            }
                            else if (fieldName?.ToLower() != "image")
                            {


                                if (dt.Rows[i][1] != DBNull.Value)
                                {
                                    fieldValue = dt.Rows[i][1].ToString();
                                }
                                form.lstFormData.Add(new FormData(fieldName, fieldValue));
                            }
                            else
                            {
                                List<SqlParameter> parameters2 = new List<SqlParameter>();
                                parameters2.Add(new SqlParameter("@guid", dt.Rows[i][1].ToString()));
                                parameters2.Add(new SqlParameter("@recordtime", date));
                                DataTable dt2 = sqlHelper.ExecuteQuery(sql5, parameters2.ToArray());
                                for (int j = 0; j < dt2.Rows.Count; j++)
                                {
                                    string? imageId = dt2.Rows[j]["sequence"].ToString();
                                    ImageData id = new ImageData();
                                    id.Id = imageId;
                                    id.Guid= dt.Rows[i][1].ToString();
                                    id.Image = (byte[])dt2.Rows[j]["image"];
                                    id.Date = date;
                                    form.lstImageData.Add(id);

                                }
                            }
                        }

                        
                    }
                    else
                    {
                        dt.Clear();
                        parameters.RemoveAt(parameters.Count - 1);
                        dt = sqlHelper.ExecuteQuery(sql4, parameters.ToArray());
                        if (dt.Rows.Count > 0)
                        {
                            //string preDate=dt.Rows[0]["recordTime"].ToString().Substring(0,10);
                            form.PreDate = DateTime.Parse(dt.Rows[0]["recordTime"].ToString());
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                //if (i == 0)
                                //{
                                //    form.LastDate =  DateTime.Parse(dt.Rows[i][2].ToString());
                                      form.Date= DateTime.Parse(dt.Rows[i][2].ToString());
                                //}
                                fieldName = dt.Rows[i][0].ToString();
                                var fieldValue = "";
                                if (fieldName?.ToLower() != "image")
                                {
                                    if (fieldName.ToLower() == "currdate")
                                    {
                                        fieldValue = DateTime.Now.ToString("yyyy-MM-dd");
                                    }
                                    else
                                    {
                                        if (dt.Rows[i][1] != DBNull.Value)
                                        {
                                            fieldValue = dt.Rows[i][1].ToString();
                                        }
                                    }
                                    form.lstFormData.Add(new FormData(fieldName, fieldValue));
                                }
                                else
                                {
                                    List<SqlParameter> parameters2 = new List<SqlParameter>();
                                    parameters2.Add(new SqlParameter("@guid", dt.Rows[i][1].ToString()));
                                    parameters2.Add(new SqlParameter("@recordTime", form.PreDate));
                                    DataTable dt2 = sqlHelper.ExecuteQuery(sql5, parameters2.ToArray());
                                    for (int j = 0; j < dt2.Rows.Count; j++)
                                    {
                                        string? imageId = dt2.Rows[j]["sequence"].ToString();
                                        ImageData id = new ImageData();
                                        id.Id = imageId;
                                        id.Image = (byte[])dt2.Rows[j]["image"];
                                        id.Date = form.PreDate?.ToString("yyyy-MM-dd");
                                        form.lstImageData.Add(id);

                                    }
                                }
                            }
                            
                        }
                        else
                        {
                            form.Date = DateTime.Now;
                            
                        }
                    }
                    dt.Clear();
                    dt = sqlHelper.ExecuteQuery(sql6);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Manager manager = new Manager()
                        {
                            Id = dt.Rows[i]["ManagerId"].ToString(),
                            Name = dt.Rows[i]["ManagerName"].ToString(),
                        };
                        form.lstManager.Add(manager);
                    }
                    dt.Clear();
                    SqlHelper sqlHelper2 = new SqlHelper(_connectionWater2022);
                    string sql8 = "Select userid,username from users where specialuser=3";
                    dt = sqlHelper2.ExecuteQuery(sql8);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Maintainer mr = new Maintainer()
                        {
                            Id = dt.Rows[i]["userId"].ToString(),
                            Name = dt.Rows[i]["userName"].ToString(),
                        };
                        form.lstMaintainer.Add(mr);
                    }


                    parameters.Clear();
                    parameters.Add(new SqlParameter("@tabletype", tableType));
                    parameters.Add(new SqlParameter("@year", form.Date.Year));
                    parameters.Add(new SqlParameter("@yearmonth", form.Date.ToString("yyyy/MM")));//date.Substring(0, 7).Replace("-", "/"))) ; ;
                    dt.Clear();
                    dt = sqlHelper.ExecuteStoreProcedureQuery("sp_getMaintainTypeCount",parameters.ToArray());
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        fieldName = dt.Rows[i][0].ToString();
                        string fieldValue= dt.Rows[i][1].ToString();
                        if (form.lstFormData.Find(x => x.fieldName == fieldName) != null)
                            form.lstFormData.Find(x => x.fieldName == fieldName).fieldValue = fieldValue;
                        else
                            form.lstFormData.Add(new FormData(fieldName, fieldValue));
                    }

                    parameters.Clear();
                    parameters.Add(new SqlParameter("@tableType", tableType));
                    parameters.Add(new SqlParameter("@location", location));
                    parameters.Add(new SqlParameter("@stationId", stationId));
                    parameters.Add(new SqlParameter("@recordtime", date));
                    dt.Clear();
                    dt = sqlHelper.ExecuteStoreProcedureQuery("sp_getLastMaintainTime", parameters.ToArray());
                    if (dt.Rows.Count > 0)
                    {
                        
                        string fieldValue = dt.Rows[0][0].ToString();
                        if (form.lstFormData.Find(x => x.fieldName == "LastDate") != null)
                            form.lstFormData.Find(x => x.fieldName == "LastDate").fieldValue = fieldValue;
                        else
                            form.lstFormData.Add(new FormData("LastDate",fieldValue));

                    }

                    return Ok(form);

                    
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }

        private void LoadTableDef(string stationId,string tableType,string location,ref string formName, ref string formDefinition)
        {
            using (SqlConnection conn = new SqlConnection(_connection))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_getItemForTable", conn);
                cmd.Parameters.Add(new SqlParameter("@StationID", stationId));
                cmd.Parameters.Add(new SqlParameter("@tableType", tableType));
                cmd.Parameters.Add(new SqlParameter("@location", location));
                cmd.CommandType = CommandType.StoredProcedure;

                
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        formName=rdr[0].ToString();
                        formDefinition = rdr["jsonresult"].ToString();
                    }
                }
            }
        }

        private string GetLastData(string stationId,string tableType,string location, string date)
        {
            var sql9 = @"select max(recordtime) as rt from formdata where tableType=@tableType and stationId=@stationId and location=@location and recordtime < @recordtime";

            SqlHelper sqlHelper = new SqlHelper(_connection);
            List<SqlParameter> lstP1 = new List<SqlParameter>();
            lstP1.Add(new SqlParameter("@tabletype", tableType));
            lstP1.Add(new SqlParameter("@location", location));
            lstP1.Add(new SqlParameter("@recordtime", date));
            lstP1.Add(new SqlParameter("@stationId", stationId));
            DataTable dtP1 = sqlHelper.ExecuteQuery(sql9, lstP1.ToArray());
            var lastDate = "";
            if (dtP1.Rows.Count > 0 && dtP1.Rows[0][0]!=DBNull.Value)
            {
                lastDate = dtP1.Rows[0][0].ToString();
            }
            
            return lastDate;
        }

        [HttpGet("getform")]
        public IActionResult getFormFields(string tableType, string stationId, string location, string date)
        {
            try
            {
                SqlHelper sqlHelper = new SqlHelper(_connection);

                //var sql = $"SELECT Name, Definition FROM Forms WHERE Id = @FormId";
                List<SqlParameter> parameters = new List<SqlParameter>();
                //DataTable dt = null;
                string formName = "";
                string formDefinition = "";
                string fileNo = "";
                //LoadTableDef(stationId, tableType, location, ref formName, ref formDefinition);
                
                
                parameters.Add(new SqlParameter("@StationID", stationId));
                parameters.Add(new SqlParameter("@tableType", tableType));
                parameters.Add(new SqlParameter("@location", location));
                
                DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_getItemForTable", parameters.ToArray());
                if (dt.Rows.Count > 0)
                {
                    formName = dt.Rows[0][0].ToString();
                    formDefinition = dt.Rows[0][1].ToString();
                    fileNo = dt.Rows[0][2].ToString();
                }
                if (!String.IsNullOrEmpty(formDefinition))
                {
                    

                    var sql3 = $"Select fieldName,fieldValue from formdata where tabletype=@tableType and stationId=@stationId and recordtime=@recordtime and location=@location";
                    var sql4 = @"select fieldName,fieldValue,recordtime from formdata a
                             inner join 
                             (Select max(recordtime) as rt from formdata where tableType=@tableType and stationId=@stationId and location=@location) as b on a.recordtime=b.rt
                             where location=@location and tabletype=@tabletype";
                    var sql5 = @"select sequence,image from imagedata where guid=@guId and recordtime=@recordTime";

                    //var sql6 = @"select managerid,managername from manager";
                    var sql6 = @"select userid as managerid,username as managername from [Water2022].[dbo].[users]  where specialuser=9";

                    var sql7 = @"Select a.fieldLabel,b.fieldName, a.initValue from sensorinitvalue a
                                 inner join checkitem b on a.tabletype=b.tabletype and a.id=b.id
                                 where a.stationid='" + stationId + "' and a.tabletype='" + tableType + "' and a.[location]='" + location + "'";
                    
                    FormDefNew form = new FormDefNew();
                    form.Name = formName;
                    form.Definition = formDefinition;

                    dt?.Clear();
                    parameters.Add(new SqlParameter("@recordtime", date));
                    dt = sqlHelper.ExecuteQuery(sql3, parameters.ToArray());
                    //form.Date = DateTime.Parse(date);
                    string fieldName = "";
                    if (dt.Rows.Count > 0)
                    {

                        form.Date = DateTime.Parse(date);
                        form.PreDate = form.Date;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            fieldName = dt.Rows[i][0].ToString();
                            var fieldValue = "";
                            if (fieldName?.ToLower() == "currdate")
                            {
                                fieldValue = DateTime.Now.ToString("yyyy-MM-dd");
                                form.lstFormData.Add(new FormData(fieldName, fieldValue));
                                string lastDate = GetLastData(stationId, tableType, location, date);

                                if (form.lstFormData.Find(x => x.fieldName.ToLower() == "lastdate") != null)
                                    form.lstFormData.Find(x => x.fieldName.ToLower() == "lastdate").fieldValue = lastDate;
                                else
                                    form.lstFormData.Add(new FormData("LastDate", lastDate));

                            }
                            else if (fieldName?.ToLower() == "lastdate")
                            {
                                //fieldValue = DateTime.Now.ToString("yyyy-MM-dd");
                                //form.lstFormData.Add(new FormData(fieldName, fieldValue));
                            }
                            else if (fieldName?.ToLower() != "image")
                            {


                                if (dt.Rows[i][1] != DBNull.Value)
                                {
                                    fieldValue = dt.Rows[i][1].ToString();
                                }
                                form.lstFormData.Add(new FormData(fieldName, fieldValue));
                            }
                            else
                            {
                                form.Guid = dt.Rows[i][1].ToString();
                                List<SqlParameter> parameters2 = new List<SqlParameter>();
                                parameters2.Add(new SqlParameter("@guid", dt.Rows[i][1].ToString()));
                                parameters2.Add(new SqlParameter("@recordtime", date));
                                DataTable dt2 = sqlHelper.ExecuteQuery(sql5, parameters2.ToArray());
                                for (int j = 0; j < dt2.Rows.Count; j++)
                                {
                                    string? imageId = dt2.Rows[j]["sequence"].ToString();
                                    ImageData id = new ImageData();
                                    id.Id = imageId;
                                    id.Guid = dt.Rows[i][1].ToString();
                                    id.Image = (byte[])dt2.Rows[j]["image"];
                                    id.Date = date;
                                    form.lstImageData.Add(id);

                                }
                            }
                        }


                    }
                    else
                    {
                        dt?.Clear();
                        parameters.RemoveAt(parameters.Count - 1);
                        dt = sqlHelper.ExecuteQuery(sql4, parameters.ToArray());
                        if (dt.Rows.Count > 0)
                        {
                            //string preDate=dt.Rows[0]["recordTime"].ToString().Substring(0,10);
                            form.PreDate = DateTime.Parse(dt.Rows[0]["recordTime"].ToString());
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                //if (i == 0)
                                //{
                                //    form.LastDate =  DateTime.Parse(dt.Rows[i][2].ToString());
                                form.Date = DateTime.Parse(dt.Rows[i][2].ToString());
                                //}
                                fieldName = dt.Rows[i][0].ToString();
                                var fieldValue = "";
                                if (fieldName?.ToLower() != "image")
                                {
                                    if (fieldName.ToLower() == "currdate")
                                    {
                                        fieldValue = DateTime.Now.ToString("yyyy-MM-dd");
                                        string lastDate = GetLastData(stationId, tableType, location, date);

                                        if (form.lstFormData.Find(x => x.fieldName.ToLower() == "lastdate") != null)
                                            form.lstFormData.Find(x => x.fieldName.ToLower() == "lastdate").fieldValue = lastDate;
                                        else
                                            form.lstFormData.Add(new FormData("LastDate", lastDate));

                                        //currDate = fieldValue;
                                    }
                                    else if (fieldName?.ToLower() == "lastdate")
                                    {
                                        //fieldValue = DateTime.Now.ToString("yyyy-MM-dd");
                                        //form.lstFormData.Add(new FormData(fieldName, fieldValue));
                                    }
                                    else
                                    {
                                        if (dt.Rows[i][1] != DBNull.Value)
                                        {
                                            fieldValue = dt.Rows[i][1].ToString();
                                        }
                                    }
                                    form.lstFormData.Add(new FormData(fieldName, fieldValue));
                                }
                                else
                                {
                                    List<SqlParameter> parameters2 = new List<SqlParameter>();
                                    parameters2.Add(new SqlParameter("@guid", dt.Rows[i][1].ToString()));
                                    parameters2.Add(new SqlParameter("@recordTime", form.PreDate));
                                    DataTable dt2 = sqlHelper.ExecuteQuery(sql5, parameters2.ToArray());
                                    for (int j = 0; j < dt2.Rows.Count; j++)
                                    {
                                        string? imageId = dt2.Rows[j]["sequence"].ToString();
                                        ImageData id = new ImageData();
                                        id.Id = imageId;
                                        id.Image = (byte[])dt2.Rows[j]["image"];
                                        id.Date = form.PreDate?.ToString("yyyy-MM-dd");
                                        id.Guid = dt.Rows[i][1].ToString();
                                        form.lstImageData.Add(id);

                                    }
                                }
                            }

                        }
                        else
                        {
                            form.Date = DateTime.Now;

                        }
                    }
                    
                    dt?.Clear();
                    dt = sqlHelper.ExecuteQuery(sql6);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Manager manager = new Manager()
                        {
                            Id = dt.Rows[i]["ManagerId"].ToString(),
                            Name = dt.Rows[i]["ManagerName"].ToString(),
                        };
                        form.lstManager.Add(manager);
                    }
                    dt?.Clear();
                    SqlHelper sqlHelper2 = new SqlHelper(_connectionWater2022);
                    string sql8 = "Select userid,username from users where specialuser=3";
                    dt = sqlHelper2.ExecuteQuery(sql8);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Maintainer mr = new Maintainer()
                        {
                            Id = dt.Rows[i]["userId"].ToString(),
                            Name = dt.Rows[i]["userName"].ToString(),
                        };
                        form.lstMaintainer.Add(mr);
                    }


                    parameters.Clear();
                    parameters.Add(new SqlParameter("@tabletype", tableType));
                    parameters.Add(new SqlParameter("@year", form.Date.Year));
                    parameters.Add(new SqlParameter("@yearmonth", form.Date.ToString("yyyy/MM")));//date.Substring(0, 7).Replace("-", "/"))) ; ;
                    dt?.Clear();
                    dt = sqlHelper.ExecuteStoreProcedureQuery("sp_getMaintainTypeCount", parameters.ToArray());
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        fieldName = dt.Rows[i][0].ToString();
                        string fieldValue = dt.Rows[i][1].ToString();
                        if (form.lstFormData.Find(x => x.fieldName == fieldName) != null)
                            form.lstFormData.Find(x => x.fieldName == fieldName).fieldValue = fieldValue;
                        else
                            form.lstFormData.Add(new FormData(fieldName, fieldValue));
                    }

                    parameters.Clear();
                    parameters.Add(new SqlParameter("@tableType", tableType));
                    parameters.Add(new SqlParameter("@location", location));
                    parameters.Add(new SqlParameter("@stationId", stationId));
                    parameters.Add(new SqlParameter("@recordtime", date));
                    dt.Clear();
                    dt = sqlHelper.ExecuteStoreProcedureQuery("sp_getLastMaintainTime", parameters.ToArray());
                    if (dt.Rows.Count > 0)
                    {

                        string fieldValue = dt.Rows[0][0].ToString();
                        if (form.lstFormData.Find(x => x.fieldName == "LastDate") != null)
                            form.lstFormData.Find(x => x.fieldName == "LastDate").fieldValue = fieldValue;
                        else
                            form.lstFormData.Add(new FormData("LastDate", fieldValue));

                    }

                    dt.Clear();
                    dt = sqlHelper.ExecuteQuery(sql7);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string[] fieldValues = dt.Rows[i]["initValue"].ToString().Split(',');
                        string[] fieldNames = dt.Rows[i]["fieldName"].ToString().Split(',');
                        for (int j = 0; j < fieldValues.Length; j++)
                        {
                            var list=form.lstFormData.Find(x => x.fieldName == fieldNames[j]);
                            if (list != null)
                            {
                                list.fieldValue= fieldValues[j];
                                list.isInitValue=true;
                            }
                            else
                            {
                                form.lstFormData.Add(new FormData(fieldNames[j], fieldValues[j], true));
                            }
                        }

                        
                    }
                    var fileNo1 = form.lstFormData.Find(x => x.fieldName.ToLower() == "fileno");
                    string newFileNo = String.Format("{0}0{1}-{2}", DateTime.Now.Year%100, DateTime.Now.Month.ToString("00"), fileNo);
                    var simpleFileNo1 = form.lstFormData.Find(x => x.fieldName.ToLower() == "simplefileno");
                    /*
                    if (fileNo1 != null)
                        fileNo1.fieldValue = newFileNo;
                    else
                        form.lstFormData.Add(new FormData("FileNo", newFileNo));
                    */
                    if (fileNo1 == null)
                        form.lstFormData.Add(new FormData("FileNo", newFileNo));
                    if (simpleFileNo1 == null)
                        form.lstFormData.Add(new FormData("SimpleFileNo", fileNo));
                    return Ok(form);


                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }

        /*
        [HttpGet("getFormField")]
        public IActionResult GetFormDefinition(string category, string date, string station)
        {
            try
            {
                SqlHelper sqlHelper = new SqlHelper(_connection);

                var sql = $"SELECT Name, Definition FROM Forms WHERE Id = @FormId";
                var sql2 = $"SELECT ItemName,FieldName FROM ExtraItems WHERE formId = @FormId and  stationId=@stationId order by seq";
                var sql3 = $"Select fieldName,fieldValue from formdata where FormId=@FormId and stationId=@stationId and recordtime=@recordtime";
                var sql4 = @"select fieldName,fieldValue,recordtime from formdata a
                             inner join 
                             (Select max(recordtime) as rt from formdata where FormId=@FormId and stationId=@stationId) as b on a.recordtime=b.rt";
                var sql5 = @"select sequence,image from imagedata 
                             where guid=@guId";
                var sql6 = @"select managerid,managername from manager";

                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@formId", category));

                DataTable dt = sqlHelper.ExecuteQuery(sql, parameters.ToArray());
                if (dt.Rows.Count > 0)
                {
                    FormDef form = new FormDef();
                    var formName = dt.Rows[0][0].ToString();
                    var formDefinition = dt.Rows[0][1].ToString();
                    form.Name = formName;
                    form.Definition = formDefinition;
                    parameters.Add(new SqlParameter("@stationId", station));
                    dt.Clear();
                    dt = sqlHelper.ExecuteQuery(sql2, parameters.ToArray());
                    string? fieldName = "";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var itemName = dt.Rows[i][0].ToString();


                        if (dt.Rows[i][1] != DBNull.Value)
                        {
                            fieldName = dt.Rows[i][1].ToString();
                            form.lstExtraItem.Add(new ExtraItem(itemName, fieldName));
                        }


                    }
                    try
                    {
                        dt.Clear();
                        parameters.Add(new SqlParameter("@recordtime", date));
                        dt = sqlHelper.ExecuteQuery(sql3, parameters.ToArray());
                        form.Date = DateTime.Parse(date);
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                fieldName = dt.Rows[i][0].ToString();
                                var fieldValue = "";
                                if (fieldName?.ToLower() != "image")
                                {


                                    if (dt.Rows[i][1] != DBNull.Value)
                                    {
                                        fieldValue = dt.Rows[i][1].ToString();
                                    }
                                    form.lstFormData.Add(new FormData(fieldName, fieldValue));
                                }
                                else
                                {
                                    List<SqlParameter> parameters2 = new List<SqlParameter>();
                                    parameters2.Add(new SqlParameter("@guid", dt.Rows[i][1].ToString()));
                                    DataTable dt2 = sqlHelper.ExecuteQuery(sql5, parameters2.ToArray());
                                    for (int j = 0; j < dt2.Rows.Count; j++)
                                    {
                                        string? imageId = dt2.Rows[j]["sequence"].ToString();
                                        ImageData id = new ImageData();
                                        id.Id = imageId;
                                        id.Image = (byte[])dt2.Rows[j]["image"];
                                        form.lstImageData.Add(id);

                                    }
                                }
                            }
                        }
                        else
                        {
                            dt.Clear();
                            parameters.RemoveAt(parameters.Count - 1);
                            dt = sqlHelper.ExecuteQuery(sql4, parameters.ToArray());
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (i == 0)
                                {
                                    form.Date = DateTime.Parse(dt.Rows[i][2].ToString());
                                }
                                fieldName = dt.Rows[i][0].ToString();
                                var fieldValue = "";
                                if (fieldName?.ToLower() != "image")
                                {


                                    if (dt.Rows[i][1] != DBNull.Value)
                                    {
                                        fieldValue = dt.Rows[i][1].ToString();
                                    }
                                    form.lstFormData.Add(new FormData(fieldName, fieldValue));
                                }
                                else
                                {
                                    List<SqlParameter> parameters2 = new List<SqlParameter>();
                                    parameters2.Add(new SqlParameter("@guid", dt.Rows[i][1].ToString()));
                                    DataTable dt2 = sqlHelper.ExecuteQuery(sql5, parameters2.ToArray());
                                    for (int j = 0; j < dt2.Rows.Count; j++)
                                    {
                                        string? imageId = dt2.Rows[j]["sequence"].ToString();
                                        ImageData id = new ImageData();
                                        id.Id = imageId;
                                        id.Image = (byte[])dt2.Rows[j]["image"];
                                        form.lstImageData.Add(id);

                                    }
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                    dt.Clear();
                    dt = sqlHelper.ExecuteQuery(sql6);
                    for(int i=0; i<dt.Rows.Count;i++)
                    {
                        Manager manager = new Manager()
                        {
                            Id = dt.Rows[i]["ManagerId"].ToString(),
                            Name = dt.Rows[i]["ManagerName"].ToString(),
                        };
                        form.manager = manager;
                        break;
                    }
                    dt.Clear();
                    SqlHelper sqlHelper2 = new SqlHelper(_connectionWater2022);
                    string sql7 = "Select userid,username from users where specialuser=3";
                    dt = sqlHelper2.ExecuteQuery(sql7);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Maintainer  mr= new Maintainer()
                        {
                            Id = dt.Rows[i]["userId"].ToString(),
                            Name = dt.Rows[i]["userName"].ToString(),
                        };
                        form.lstMaintainer.Add(mr);
                    }

                    return Ok(form);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
            
        }
        */
        [HttpGet("GetStation")]
        public IActionResult GetStation()
        {
            try
            {
                SqlHelper sqlHelper = new SqlHelper(_connection);
                List<SqlParameter> lstParament = new List<SqlParameter>();
                DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_getStations", lstParament.ToArray());
                Station st = new Station();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    st.lstStations.Add(new ComboData(dt.Rows[i]["stationnameA"].ToString(), dt.Rows[i]["stationid"].ToString()));
                }

                
                return Ok(st);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
            finally
            {

            }
        }

        [HttpGet("GetStationCategory")]
        public IActionResult GetStationCategory()
        {
            try
            {
                SqlHelper sqlHelper = new SqlHelper(_connection);

                DataTable dt = sqlHelper.ExecuteQuery("Select id ,name from stations order by name");
                StationCategory sc = new StationCategory();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sc.lstStations.Add(new ComboData(dt.Rows[i]["name"].ToString(), dt.Rows[i]["id"].ToString()));
                }

                dt.Clear();
                dt = sqlHelper.ExecuteQuery(@"select [CategoryId],'('+CategoryId+') '+[CategoryName] as CategoryName
                                              FROM FormCategory order by categoryId");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sc.lstCategories.Add(new ComboData(dt.Rows[i]["categoryname"].ToString(), dt.Rows[i]["categoryid"].ToString()));
                }
                return Ok(sc);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
            finally
            {

            }
        }

        async Task<List<ExtraItemEdit>> getItems(string station, string category)
        {
            List<ExtraItemEdit> items = new List<ExtraItemEdit>();
            SqlHelper sqlHelper = new SqlHelper(_connection);
            string sql = @"Select sequence, stationname, itemname,seq,fieldName from extraitems where stationid=@stationId and formId=@formId order by seq";
            
            List<SqlParameter> parameters = new List<SqlParameter>();   
            parameters.Add(new SqlParameter("@stationId", station));
            parameters.Add(new SqlParameter("@formId", category));
            DataTable dt = sqlHelper.ExecuteQuery(sql, parameters.ToArray());
            for(int i=0;i<dt.Rows.Count;i++)
            {
                items.Add(new ExtraItemEdit
                {
                    sequence = dt.Rows[i]["sequence"].ToString(),
                    ItemName = dt.Rows[i]["itemName"].ToString(),
                    FieldName = dt.Rows[i]["fieldName"].ToString(),
                    Seq = dt.Rows[i]["seq"].ToString(),
                }); ;
            }

            
            
            return items;
        }

        [HttpGet("getItems")]
        public async Task<ActionResult<IEnumerable<ExtraItemEdit>>> GetItems(string station, string category)
        {

            return await getItems(station, category);
        }


        [HttpGet("item/{id}")]
        public async Task<ActionResult<ExtraItemEdit>> GetItem(int id)
        {
            //var item = await _context.Items.FindAsync(id);
            var item = new ExtraItemEdit
            {
                ItemName = "4",
                FieldName = "Cam4",
                Seq = "4",
            };
            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        // POST: api/Items
        [HttpPost("item/add/{station}/{formId}")]
        public async Task<ActionResult<ExtraItemEdit>> AddExtraItem(string station,string formId,ExtraItemEdit item)
        {
            try
            {

                SqlHelper sqlHelper = new SqlHelper(_connection);
                string sql = @"insert into extraitems  (stationId, formId, itemName,fieldName,seq) values (@stationId,@formId,@itemName,@fieldName,@seq);
                              select * from extraitems where sequence=SCOPE_IDENTITY()";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@stationId", station));
                parameters.Add(new SqlParameter("@formId", formId));
                parameters.Add(new SqlParameter("@itemName", item.ItemName));
                parameters.Add(new SqlParameter("@fieldName", item.FieldName));
                parameters.Add(new SqlParameter("@seq", item.Seq));

                DataTable dt=sqlHelper.ExecuteQuery(sql, parameters.ToArray());
                if (dt != null)
                {
                    var newItem = new ExtraItemEdit
                    {
                        ItemName = dt.Rows[0]["itemName"].ToString(),
                        FieldName = dt.Rows[0]["fieldName"].ToString(),
                        Seq = dt.Rows[0]["seq"].ToString(),
                        sequence = dt.Rows[0]["Sequence"].ToString(),
                    };
                    return Ok(newItem);
                }
                else
                    return BadRequest("-1");
            }
            catch (Exception ex)
            {
                return BadRequest("-1");
            }
        }

        // PUT: api/Items/5
        [HttpPost("item/edit/{sequence}")]
        public async Task<IActionResult> EditExtraItem(string sequence, ExtraItemEdit item)
        {
            try
            {

                SqlHelper sqlHelper = new SqlHelper(_connection);
                string sql = @"Update extraitems set itemName=@itemName, fieldName=@fieldName, seq=@seq where sequence=@sequence";
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@sequence", sequence));
                parameters.Add(new SqlParameter("@itemName", item.ItemName));
                parameters.Add(new SqlParameter("@fieldName", item.FieldName));
                parameters.Add(new SqlParameter("@seq", item.Seq));

                int nUpdate = sqlHelper.ExecuteNonQuery(sql, parameters.ToArray());
                if (nUpdate > 0)
                    return Ok("OK");
                else
                    return Ok("Nothing delete");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        // DELETE: api/Items/5
        [HttpDelete("item/delete")]
        //public async Task<IActionResult> DeleteItem(string station, string category, string itemname,string seq)
        public async Task<IActionResult> DeleteItem(string sequence)
        {
            
            try
            {
                SqlHelper sqlHelper = new SqlHelper(_connection);
                //string sql = @"delete from extraitems where stationid=@stationId and formId=@formId and itemName=@itemName and seq=@seq";
                string sql = @"delete from extraitems where sequence=@sequence";

                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@sequence", sequence));
                /*
                parameters.Add(new SqlParameter("@stationId", station));
                parameters.Add(new SqlParameter("@formId", category));
                parameters.Add(new SqlParameter("@itemName", itemname));
                parameters.Add(new SqlParameter("@seq", seq));
                */
                int nDelete=sqlHelper.ExecuteNonQuery(sql, parameters.ToArray());
                if (nDelete > 0)
                    return Ok("OK");
                else
                    return Ok("Nothing delete");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool ItemExists(int id)
        {
            return true;// _context.Items.Any(e => e.Id == id);
        }

        [HttpGet("WordForm")]
        public async Task<ActionResult<WordForm>> WordForm()
        {
            var wordFormData = RetrieveDocxFromDb("2.10-1 淡水河流域監測系統維護工作維護紀錄表.docx", "3", _connection);
            if (wordFormData == null)
            {
                return NotFound();
            }

            var wordFormJson = System.Text.Encoding.UTF8.GetString(wordFormData);
            //var wordForm = JsonConvert.DeserializeObject<WordForm>(wordFormJson);

            return Ok(wordFormJson);
        }

        private byte[] RetrieveDocxFromDb(string fileName, string documentId, string connectionString)
        {
            byte[] docxData = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT fileName, DocXContent FROM WordDocXBinary WHERE sequence = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", documentId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            docxData = (byte[])reader["DocxContent"];
                            fileName = (string)reader["fileName"];
                        }
                    }
                }
            }

            return docxData;
        }
    }
    // Models/Form.cs

    public class ExtraItemEdit
    {
        public string? sequence { get; set; }
        public string? ItemName { get; set; }
        public string? FieldName { get; set; }
        public string? Seq { get; set; }
    }


    
    public class WordForm
    {
        public string Word { get; set; }
        public string Definition { get; set; }
    }

    public class FormDefNew
    {
        public DateTime? PreDate { get; set; }
        public int TableType { get; set; }
        public string? Name { get; set; }
        public string? Definition { get; set; }
        public string? Guid { get; set; } = null;
        public DateTime Date { get; set; }
        public DateTime LastDate { get; set; }
        public List<FormData> lstFormData { get; set; } = new List<FormData>();

        public List<ImageData> lstImageData { get; set; } = new List<ImageData>();
        //public List<Manager> lstManager { get; set; } = new List<Manager>() { };
        public List<Manager> lstManager { get; set; }=new List<Manager>();
        public List<Maintainer> lstMaintainer { get; set; } = new List<Maintainer>() { };
    }


    public class FormDef
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Definition { get; set; }
        public DateTime Date { get; set; }
        public List<ExtraItem> lstExtraItem { get; set; } = new List<ExtraItem>();
        public List<FormData> lstFormData { get; set; } = new List<FormData>();

        public List<ImageData> lstImageData { get; set; } = new List<ImageData>();
        public Manager manager { get; set; }
        public List<Maintainer> lstMaintainer { get; set; } = new List<Maintainer>() { };
    }

    public class Manager
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    public class Maintainer
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
    public class ImageData
    {
        public string? Id { get; set; }
        public string? Guid { get; set; }
        public byte[] Image { get; set; }
        public string? Tag { get; set; }
        public string? Date { get; set; }
    }
    public class ImageData2
    {
        public string? Id { get; set; }
        public string? Guid { get; set; }
        public string? Image { get; set; }
        public string? Tag { get; set; }
    }

    public class ImageData3
    {
        public string? Id { get; set; }
        public string? Guid { get; set; }
        public IFormFile? Image { get; set; }
        public string? Tag { get; set; }
    }
    public class ListData
    {
        List<SaveDataFormat> lstSaveFormats { get; set; } = new List<SaveDataFormat>();
    }
    public class SaveDataFormat
    {
        public string? fieldName { get; set; }
        public string? fieldValue { get; set; }
    }

    public class ExtraItem
    {
        public ExtraItem(string? _itemName, string? _fieldName)
        {
            itemName = _itemName;
            fieldName = _fieldName;
        }
        public string? itemName { get; set; }
        public string? fieldName { get; set; }
    }

    public class FormDataModel
    {
        //public DateTime FormDate { get; set; }
        public Dictionary<string, string> datas { get; set; } = new Dictionary<string, string>();
        //public string datas { get; set; }
    }

    public class DataModel
    {
        //public FormDataModel FormData { get; set; }
        public string? FormData { get; set; }
        public string? Category { get; set; }
        public DateTime Date { get; set; }
        public string? Station { get; set; }
        public string? MaintainLocation { get; set; }
        public string? guid { get; set; } = "";
        public DateTime ImageDate { get; set; }
        public List<Manager> manager { get; set; }=new List<Manager>() { };
        public List<Maintainer> maintainer { get; set; }=new List<Maintainer>() { };   
    }

    public class ScheduleForm
    {
        public string scheduleFormData { get; set; }
    }

    public class ScheduleFormData
    {
        public List<string> selectedDates { get; set; } = new List<string>();
        public string? yearMonth { get; set; }
        public string? maintainType { get; set; }
        public string? startYearMonth { get; set; }
        
    }

    public class FormData
    {
        public FormData(string _fieldName, string _fieldValue, bool _isInitValue=false)
        {
            fieldName = _fieldName;
            fieldValue = _fieldValue;
            isInitValue=_isInitValue;
        }
        public string? fieldName { get; set; }
        public string? fieldValue { get; set; }
        public bool isInitValue {get;set;}=false;
    }
    public class StationLocation
    {
        public ComboData station { get; set; }  
        public List<string> lstLocation { get; set; } =new List<string>() { };
    }
    public class ComboData
    {
        public ComboData(string _text, string _value)
        {
            text = _text;
            value = _value;
        }
        public string? text { get; set; }
        public string? value { get; set; }
    }

    public class LocationTabletype
    {
        public List<ComboData> lstLocations { get; set; } = new List<ComboData>();
        public List<ComboData> lstTypes { get; set; } = new List<ComboData>();

    }
    public class StationCategory
    {
        public List<ComboData> lstStations { get; set; } = new List<ComboData>();
        public List<ComboData> lstCategories { get; set; } = new List<ComboData>();

    }

    public class Station
    {
        public List<ComboData> lstStations { get; set; } = new List<ComboData>();

    }

    public class Category
    {
        public List<ComboData> lstCategories { get; set; } = new List<ComboData>();

    }

    public static class ImageProcessor
    {
        public static async Task<byte[]> ResizeImage(byte[] imageData)
        {
            using var memoryStream = new MemoryStream(imageData);
            using var bitmap = Bitmap.FromStream(memoryStream);
            var resizedBitmap = new Bitmap(1280, 720);
            using var graphics = Graphics.FromImage(resizedBitmap);
            graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);

            using var newMemoryStream = new MemoryStream();
            resizedBitmap.Save(newMemoryStream, ImageFormat.Jpeg);
            return newMemoryStream.ToArray();
        }

        public static async Task<byte[]> ResizeAndAddText(byte[] imageData, string text, bool bEmbeddedDate)
        {
            using var memoryStream = new MemoryStream(imageData);
            using var bitmap = Bitmap.FromStream(memoryStream);
            using var resizedBitmap = new Bitmap(1280, 720);
            using var graphics = Graphics.FromImage(resizedBitmap);
            graphics.DrawImage(bitmap, 0, 0, 1280, 720);
            if (bEmbeddedDate)
            {
                using var font = new System.Drawing.Font("Arial", 24);
                using var brush = new SolidBrush(System.Drawing.Color.Red);

                var textSize = graphics.MeasureString(text, font);
                int x = resizedBitmap.Width - (int)textSize.Width - 20; // right side
                int y = resizedBitmap.Height - (int)textSize.Height - 20; // bottom side
                graphics.DrawString(text, font, brush, x, y);
            }
            using var newMemoryStream = new MemoryStream();
            resizedBitmap.Save(newMemoryStream, ImageFormat.Jpeg);
            return newMemoryStream.ToArray();
        }
    }

    public class HistoryHeader
    {
        public int id { get; set; }
        public string? recordTime { get; set; }
        public string? stationId { get; set; }
        public string? stationName { get; set; }

        public List<HistoryDetail> lstHistoryDetail { get; set; }= new List<HistoryDetail>();
    }

    public class HistoryDetail
    {
        public int id { get; set; }
        public string? tableType{ get; set; }
        public string? tableTypeName { get; set; }
    }
    public class HistoryData
    {
        public List<HistoryHeader> lstHistoryHeader { get; set; }= new List<HistoryHeader>();

    }

    public class ScheduleQuery
    {
        public string? Row { get; set; }
        public string? YearMonth { get; set; }
        public string? item1 { get; set; }
        public string? item2 { get; set; }
        public string? item3 { get; set; }
        public string? item4 { get; set; }
        public string? item5 { get; set; }
        public string? item6 { get; set; }
        public string? item7 { get; set; }
        public string? item8 { get; set; }

    }
}


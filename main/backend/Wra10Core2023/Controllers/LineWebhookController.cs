using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Notify.Services;

using Microsoft.Data.SqlClient;
using SqlHelper = Wra10Core2023.Util.SQLHelper;
using System.Data;
using System.Text;

using System.IO;


namespace LineWebhookApi.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class LineWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LineWebhookController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            Console.WriteLine("收到 LINE Webhook：\n" + body);

            try
            {
                var json = JsonDocument.Parse(body);
                var events = json.RootElement.GetProperty("events");

                foreach (var ev in events.EnumerateArray())
                {
                    var type = ev.GetProperty("type").GetString();
                    var replyToken = ev.GetProperty("replyToken").GetString();
                    var source = ev.GetProperty("source");
                    var userId = source.GetProperty("userId").GetString();
                    string user_status = "";
                    string user_area = "";
                    string user_type = "";
                    string user_sensorId = "";
                    string user_sensorName = "";
                    DateTime? user_startDate = null;
                    DateTime? user_endDate = null;
                    
                    // 回覆對話
                    var channelToken = _configuration["Line:channelAccessToken"];
                    var notifyService = new NotifyService(channelToken);

                    string? conn = _configuration.GetConnectionString("WaterToANCAD");
                    if (conn != null) //查詢user當前狀態
                    {
                        SqlHelper sqlHelper = new SqlHelper(conn);

                        string selectSql = "SELECT * FROM LineTemp WHERE UserId = @UserId";

                        SqlParameter[] parameters = new SqlParameter[]
                        {
                            new SqlParameter("@UserId", userId)
                        };

                        try
                        {
                            DataTable dt = sqlHelper.ExecuteQuery(selectSql, parameters);

                            if (dt.Rows.Count > 0)
                            {
                                user_status = dt.Rows[0]["Status"].ToString() ?? "";
                                user_area = dt.Rows[0]["Area"].ToString() ?? "";
                                user_type = dt.Rows[0]["Type"].ToString() ?? "";
                                user_sensorId = dt.Rows[0]["SensorId"].ToString() ?? "";
                                user_sensorName = dt.Rows[0]["SensorName"].ToString() ?? "";
                                if (!Convert.IsDBNull(dt.Rows[0]["StartDate"]))
                                {
                                    user_startDate = Convert.ToDateTime(dt.Rows[0]["StartDate"]);
                                }

                                if (!Convert.IsDBNull(dt.Rows[0]["EndDate"]))
                                {
                                    user_endDate = Convert.ToDateTime(dt.Rows[0]["EndDate"]);
                                }
                                
                                Console.WriteLine($"✅ 查詢結果 Status：{user_status}, Area：{user_area}, Type：{user_type}, SensorId：{user_sensorId}, SensorName：{user_sensorName}, StartDate：{user_startDate}, EndDate：{user_endDate}");
                            }
                            else
                            {
                                Console.WriteLine("⚠️ 找不到該 UserId 的資料");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                        }


                        if (type == "postback")
                        {
                            var postback = ev.GetProperty("postback");
                            var postbackParams = postback.GetProperty("params");
                            var input_date = postbackParams.GetProperty("date").GetString();
                            var action = postback.GetProperty("data").GetString();

                            if(action == "action=startDate"){
                                string sql = @"
                                IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                                BEGIN
                                    UPDATE LineTemp
                                    SET StartDate = @StartDate
                                    WHERE UserId = @UserId;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO LineTemp (UserId, StartDate)
                                    VALUES (@UserId, @StartDate);
                                END
                                ";

                                SqlParameter[] startdate_parameters = new SqlParameter[]
                                {
                                    new SqlParameter("@UserId", userId),
                                    new SqlParameter("@StartDate", input_date)
                                };

                                try
                                {
                                    int rows = sqlHelper.ExecuteNonQuery(sql, startdate_parameters);
                                    if (rows > 0)
                                    {
                                        Console.WriteLine("✅ 資料更新成功");
                                    }
                                    else
                                    {
                                        Console.WriteLine("⚠️ 無資料被更新");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                }
                                string endDateText = user_endDate.HasValue ? user_endDate.Value.ToString("yyyy-MM-dd") : "";
                                await notifyService.ReplyDatePickerAsync(replyToken!,start:input_date,end: endDateText);
                                return Ok();
                            }

                            if(action == "action=endDate"){
                                string sql = @"
                                IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                                BEGIN
                                    UPDATE LineTemp
                                    SET EndDate = @EndDate
                                    WHERE UserId = @UserId;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO LineTemp (UserId, EndDate)
                                    VALUES (@UserId, @EndDate);
                                END
                                ";

                                SqlParameter[] inputdate_parameters = new SqlParameter[]
                                {
                                    new SqlParameter("@UserId", userId),
                                    new SqlParameter("@EndDate", input_date)
                                };

                                try
                                {
                                    int rows = sqlHelper.ExecuteNonQuery(sql, inputdate_parameters);
                                    if (rows > 0)
                                    {
                                        Console.WriteLine("✅ 資料更新成功");
                                    }
                                    else
                                    {
                                        Console.WriteLine("⚠️ 無資料被更新");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                }

                                string startDateText = user_startDate.HasValue ? user_startDate.Value.ToString("yyyy-MM-dd") : "";
                                await notifyService.ReplyDatePickerAsync(replyToken! ,start: startDateText ,end:input_date);
                                return Ok();
                            }

                        }


                        if (type == "message")
                        {
                            var message = ev.GetProperty("message");
                            var messageType = message.GetProperty("type").GetString();

                            if (messageType == "text")
                            {
                                var userText = message.GetProperty("text").GetString();

                                if (userText == "開始查詢") //更新user當前狀態至"start"
                                {
                                    string sql = @"
                                                IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                                                BEGIN
                                                    UPDATE LineTemp
                                                    SET Status = @Status,
                                                        Area = @Area,
                                                        Type = @Type,
                                                        SensorID = @SensorID,
                                                        SensorName = @SensorName,
                                                        StartDate = @StartDate,
                                                        EndDate = @EndDate
                                                    WHERE UserId = @UserId;
                                                END
                                                ELSE
                                                BEGIN
                                                    INSERT INTO LineTemp (UserId, Status, Area, Type, SensorID, SensorName, StartDate, EndDate)
                                                    VALUES (@UserId, @Status, @Area, @Type, @SensorID, @SensorName, @StartDate, @EndDate);
                                                END
                                                ";


                                    SqlParameter[] a_parameters = new SqlParameter[]
                                    {
                                        new SqlParameter("@UserId", userId),
                                        new SqlParameter("@Status", "start"),
                                        new SqlParameter("@Area", ""),          // 預設空字串
                                        new SqlParameter("@Type", ""),          // 預設空字串
                                        new SqlParameter("@SensorID", ""),      // 預設空字串
                                        new SqlParameter("@SensorName", ""),    // 預設空字串
                                        new SqlParameter("@StartDate", DBNull.Value), // 預設 NULL
                                        new SqlParameter("@EndDate", DBNull.Value)    // 預設 NULL
                                    };


                                    try
                                    {
                                        int rows = sqlHelper.ExecuteNonQuery(sql, a_parameters);
                                        if (rows > 0)
                                        {
                                            Console.WriteLine("✅ 資料更新成功");
                                        }
                                        else
                                        {
                                            Console.WriteLine("⚠️ 無資料被更新");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                    }

                                    await notifyService.ReplyMenuAsync(replyToken!);

                                    return Ok();
                                }

                                if (userText == "選擇類型" && user_status == "start") //更新user當前狀態至"type"
                                {   
                                    string sql = @"
                                    IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                                    BEGIN
                                        UPDATE LineTemp
                                        SET Status = @Status
                                        WHERE UserId = @UserId;
                                    END
                                    ELSE
                                    BEGIN
                                        INSERT INTO LineTemp (UserId, Status)
                                        VALUES (@UserId, @Status);
                                    END
                                    ";

                                    SqlParameter[] type_parameters = new SqlParameter[]
                                    {
                                        new SqlParameter("@UserId", userId),
                                        new SqlParameter("@Status", "type")
                                    };

                                    try
                                    {
                                        int rows = sqlHelper.ExecuteNonQuery(sql, type_parameters);
                                        if (rows > 0)
                                        {
                                            Console.WriteLine("✅ 資料更新成功");
                                        }
                                        else
                                        {
                                            Console.WriteLine("⚠️ 無資料被更新");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                    }

                                    DataTable result_dt = new DataTable();
                                    string st_selectSql = "SELECT * FROM SensorTypes";

                                    try
                                    {
                                        result_dt = sqlHelper.ExecuteQuery(st_selectSql); // 無參數查詢
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                        await notifyService.ReplyMessageAsync(replyToken!, $"查詢失敗：{ex.Message}");
                                    }

                                    StringBuilder sb = new StringBuilder();

                                    sb.Append($@"
                                    {{
                                    ""replyToken"": ""{replyToken}"",
                                    ""messages"": [
                                        {{
                                        ""type"": ""flex"",
                                        ""altText"": ""查詢資料選單"",
                                        ""contents"": {{
                                            ""type"": ""carousel"",
                                            ""contents"": [
                                    ");

                                    int btnsPerBubble = 5;
                                    int totalBtns = result_dt.Rows.Count;

                                    int index = 0; // 用來追蹤 DataTable 的資料
                                    while (index < totalBtns)
                                    {
                                        sb.Append($@"
                                            {{
                                                ""type"": ""bubble"",
                                                ""body"": {{
                                                ""type"": ""box"",
                                                ""layout"": ""vertical"",
                                                ""contents"": [
                                                    {{
                                                    ""type"": ""text"",
                                                    ""text"": ""請選擇感測器類型"",
                                                    ""weight"": ""bold"",
                                                    ""size"": ""lg"",
                                                    ""margin"": ""md""
                                                    }},
                                        ");

                                        int currentCount = 0;
                                        while (index < totalBtns && currentCount < btnsPerBubble)
                                        {
                                            string label = result_dt.Rows[index]["SensorTypeName"].ToString() ?? "";

                                            index++; // 無論如何 index 先往後移，避免死迴圈

                                            //if (label == "影像") continue; // 如果是影像，跳過，不計數

                                            sb.Append($@"
                                                    {{
                                                    ""type"": ""button"",
                                                    ""action"": {{
                                                        ""type"": ""message"",
                                                        ""label"": ""{label}"",
                                                        ""text"": ""{label}""
                                                    }},
                                                    ""style"": ""primary"",
                                                    ""margin"": ""sm""
                                                    }},
                                            ");

                                            currentCount++;
                                        }

                                        // 移除最後一個逗號
                                        int lastCommaIndex = sb.ToString().LastIndexOf(',');
                                        if (lastCommaIndex >= 0)
                                        {
                                            sb.Remove(lastCommaIndex, 1);
                                        }

                                        sb.Append($@"
                                                ]
                                                }}
                                            }},
                                        ");
                                    }

                                    // 移除最後一個逗號（如果存在）
                                    int lastCommaIndex2 = sb.ToString().LastIndexOf(',');
                                    if (lastCommaIndex2 >= 0)
                                    {
                                        sb.Remove(lastCommaIndex2, 1);
                                    }

                                    sb.Append($@"
                                            ]
                                        }}
                                        }}
                                    ]
                                    }}");

                                    string customjson = sb.ToString();
                                    
                                    await notifyService.ReplyCustomJsonAsync(replyToken!,customjson);
                                    return Ok();
                                }

                                if (user_status == "type") //更新user當前狀態至"start"
                                {
                                    await notifyService.ReplyMenuAsync(replyToken!,area: user_area,type: userText);

                                    string sql = @"
                                    IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                                    BEGIN
                                        UPDATE LineTemp
                                        SET Status = @Status,
                                            Type = @Type
                                        WHERE UserId = @UserId;
                                    END
                                    ELSE
                                    BEGIN
                                        INSERT INTO LineTemp (UserId, Status, Type)
                                        VALUES (@UserId, @Status ,@Type);
                                    END
                                    ";

                                    SqlParameter[] inputtype_parameters = new SqlParameter[]
                                    {
                                        new SqlParameter("@UserId", userId),
                                        new SqlParameter("@Status", "start"),
                                        new SqlParameter("@Type", userText)
                                    };

                                    try
                                    {
                                        int rows = sqlHelper.ExecuteNonQuery(sql, inputtype_parameters);
                                        if (rows > 0)
                                        {
                                            Console.WriteLine("✅ 資料更新成功");
                                        }
                                        else
                                        {
                                            Console.WriteLine("⚠️ 無資料被更新");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                    }

                                    return Ok();
                                }

                                if (userText == "選擇組別" && user_status == "start") //更新user當前狀態至"area"
                                {   
                                    string sql = @"
                                    IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                                    BEGIN
                                        UPDATE LineTemp
                                        SET Status = @Status
                                        WHERE UserId = @UserId;
                                    END
                                    ELSE
                                    BEGIN
                                        INSERT INTO LineTemp (UserId, Status)
                                        VALUES (@UserId, @Status);
                                    END
                                    ";

                                    SqlParameter[] area_parameters = new SqlParameter[]
                                    {
                                        new SqlParameter("@UserId", userId),
                                        new SqlParameter("@Status", "area")
                                    };

                                    try
                                    {
                                        int rows = sqlHelper.ExecuteNonQuery(sql, area_parameters);
                                        if (rows > 0)
                                        {
                                            Console.WriteLine("✅ 資料更新成功");
                                        }
                                        else
                                        {
                                            Console.WriteLine("⚠️ 無資料被更新");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                    }

                                    DataTable result_dt = new DataTable();
                                    string ar_selectSql = "SELECT * FROM Areas";

                                    try
                                    {
                                        result_dt = sqlHelper.ExecuteQuery(ar_selectSql); // 無參數查詢
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                        await notifyService.ReplyMessageAsync(replyToken!, $"查詢失敗：{ex.Message}");
                                    }

                                    StringBuilder sb = new StringBuilder();

                                    sb.Append($@"
                                    {{
                                    ""replyToken"": ""{replyToken}"",
                                    ""messages"": [
                                        {{
                                        ""type"": ""flex"",
                                        ""altText"": ""查詢資料選單"",
                                        ""contents"": {{
                                            ""type"": ""carousel"",
                                            ""contents"": [
                                    ");

                                    int btnsPerBubble = 5;
                                    int totalBtns = result_dt.Rows.Count;

                                    int index = 0; // 用來追蹤 DataTable 的資料
                                    while (index < totalBtns)
                                    {
                                        sb.Append($@"
                                            {{
                                                ""type"": ""bubble"",
                                                ""body"": {{
                                                ""type"": ""box"",
                                                ""layout"": ""vertical"",
                                                ""contents"": [
                                                    {{
                                                    ""type"": ""text"",
                                                    ""text"": ""請選擇感測器類組別"",
                                                    ""weight"": ""bold"",
                                                    ""size"": ""lg"",
                                                    ""margin"": ""md""
                                                    }},
                                        ");

                                        int currentCount = 0;
                                        while (index < totalBtns && currentCount < btnsPerBubble)
                                        {
                                            string label = result_dt.Rows[index]["AreaName"].ToString() ?? "";

                                            index++; // 無論如何 index 先往後移，避免死迴圈

                                            sb.Append($@"
                                                    {{
                                                    ""type"": ""button"",
                                                    ""action"": {{
                                                        ""type"": ""message"",
                                                        ""label"": ""{label}"",
                                                        ""text"": ""{label}""
                                                    }},
                                                    ""style"": ""primary"",
                                                    ""margin"": ""sm""
                                                    }},
                                            ");

                                            currentCount++;
                                        }

                                        // 移除最後一個逗號
                                        int lastCommaIndex = sb.ToString().LastIndexOf(',');
                                        if (lastCommaIndex >= 0)
                                        {
                                            sb.Remove(lastCommaIndex, 1);
                                        }

                                        sb.Append($@"
                                                ]
                                                }}
                                            }},
                                        ");
                                    }

                                    // 移除最後一個逗號（如果存在）
                                    int lastCommaIndex2 = sb.ToString().LastIndexOf(',');
                                    if (lastCommaIndex2 >= 0)
                                    {
                                        sb.Remove(lastCommaIndex2, 1);
                                    }

                                    sb.Append($@"
                                            ]
                                        }}
                                        }}
                                    ]
                                    }}");

                                    string customjson = sb.ToString();
                                    
                                    await notifyService.ReplyCustomJsonAsync(replyToken!,customjson);
                                    return Ok();
                                }

                                if (user_status == "area") //更新user當前狀態至"start"
                                {   
                                    await notifyService.ReplyMenuAsync(replyToken!, area: userText, type: user_type);

                                    string sql = @"
                                    IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                                    BEGIN
                                        UPDATE LineTemp
                                        SET Status = @Status,
                                            Area = @Area
                                        WHERE UserId = @UserId;
                                    END
                                    ELSE
                                    BEGIN
                                        INSERT INTO LineTemp (UserId, Status, Area)
                                        VALUES (@UserId, @Status, @Area);
                                    END
                                    ";

                                    SqlParameter[] b_parameters = new SqlParameter[]
                                    {
                                        new SqlParameter("@UserId", userId),
                                        new SqlParameter("@Status", "start"),
                                        new SqlParameter("@Area", userText)
                                    };

                                    try
                                    {
                                        int rows = sqlHelper.ExecuteNonQuery(sql, b_parameters);
                                        if (rows > 0)
                                        {
                                            Console.WriteLine("✅ 資料更新成功");
                                        }
                                        else
                                        {
                                            Console.WriteLine("⚠️ 無資料被更新");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                    }

                                    return Ok();
                                }

                                if (userText!.StartsWith("確認") && (user_status == "start")) //更新user當前狀態至"sensor"
                                {
                                    Console.WriteLine($"userText: {userText}");
                                    // 去掉開頭的 "確認 " （包含空格）
                                    string content = userText.Substring(3).Trim();

                                    // content 應該是 "Area_a/type_b"
                                    var parts = content.Split('/');

                                    string area = parts.Length > 0 ? parts[0] : "";
                                    string s_type = parts.Length > 1 ? parts[1] : "";

                                    Console.WriteLine($"Area: {area}");
                                    Console.WriteLine($"Type: {s_type}");

                                    string sensorSql = "SELECT SensorType FROM SensorTypes WHERE SensorTypeName = @Type";
                                    string areaSql = "SELECT AreaID FROM Areas WHERE AreaName = @Area";

                                    SqlParameter[] typeParams = new SqlParameter[]
                                    {
                                        new SqlParameter("@Type", s_type)
                                    };

                                    SqlParameter[] areaParams = new SqlParameter[]
                                    {
                                        new SqlParameter("@Area", area)
                                    };

                                    string sensorTypeValue = "";
                                    string areaIdValue = "";

                                    try
                                    {
                                        // 查 SensorType
                                        DataTable sensorDt = sqlHelper.ExecuteQuery(sensorSql, typeParams);
                                        if (sensorDt.Rows.Count > 0)
                                        {
                                            sensorTypeValue = sensorDt.Rows[0]["SensorType"].ToString() ?? "";
                                        }

                                        // 查 AreaID
                                        DataTable areaDt = sqlHelper.ExecuteQuery(areaSql, areaParams);
                                        if (areaDt.Rows.Count > 0)
                                        {
                                            areaIdValue = areaDt.Rows[0]["AreaID"].ToString() ?? "";
                                        }

                                        Console.WriteLine($"SensorType: {sensorTypeValue}, AreaID: {areaIdValue}");
                                        string status = "sensor";
                                        if(s_type == "影像"){
                                            status = "camera";
                                            Console.WriteLine($"status to camera");
                                            List<SqlParameter> lstParams = new List<SqlParameter>();
                                            lstParams.Add(new SqlParameter("@areaId", String.IsNullOrEmpty(areaIdValue) ? DBNull.Value : areaIdValue));
                                            lstParams.Add(new SqlParameter("@StationId", DBNull.Value));
                                            lstParams.Add(new SqlParameter("@keyword", DBNull.Value));
                                            DataTable dt = sqlHelper.ExecuteStoreProcedureQuery("sp_GetCameraBySearch", lstParams.ToArray());
                                            if (dt.Rows.Count > 0)
                                            {
                                                StringBuilder sb = new StringBuilder();

                                                sb.Append($@"
                                                {{
                                                ""replyToken"": ""{replyToken}"",
                                                ""messages"": [
                                                    {{
                                                    ""type"": ""flex"",
                                                    ""altText"": ""查詢資料選單"",
                                                    ""contents"": {{
                                                        ""type"": ""carousel"",
                                                        ""contents"": [
                                                ");

                                                int btnsPerBubble = 5;
                                                int totalBtns = dt.Rows.Count;

                                                int index = 0; // 用來追蹤 DataTable 的資料
                                                while (index < totalBtns)
                                                {
                                                    sb.Append($@"
                                                        {{
                                                            ""type"": ""bubble"",
                                                            ""body"": {{
                                                            ""type"": ""box"",
                                                            ""layout"": ""vertical"",
                                                            ""contents"": [
                                                                {{
                                                                ""type"": ""text"",
                                                                ""text"": ""請選擇要查詢的監視器"",
                                                                ""weight"": ""bold"",
                                                                ""size"": ""lg"",
                                                                ""margin"": ""md""
                                                                }},
                                                    ");

                                                    int currentCount = 0;
                                                    while (index < totalBtns && currentCount < btnsPerBubble)
                                                    {
                                                        string label = dt.Rows[index]["CamName"].ToString() ?? "";

                                                        index++; // 無論如何 index 先往後移，避免死迴圈

                                                        sb.Append($@"
                                                                {{
                                                                ""type"": ""button"",
                                                                ""action"": {{
                                                                    ""type"": ""message"",
                                                                    ""label"": ""{label}"",
                                                                    ""text"": ""{label}""
                                                                }},
                                                                ""style"": ""primary"",
                                                                ""margin"": ""sm""
                                                                }},
                                                        ");

                                                        currentCount++;
                                                    }

                                                    // 移除最後一個逗號
                                                    int lastCommaIndex = sb.ToString().LastIndexOf(',');
                                                    if (lastCommaIndex >= 0)
                                                    {
                                                        sb.Remove(lastCommaIndex, 1);
                                                    }

                                                    sb.Append($@"
                                                            ]
                                                            }}
                                                        }},
                                                    ");
                                                }

                                                // 移除最後一個逗號（如果存在）
                                                int lastCommaIndex2 = sb.ToString().LastIndexOf(',');
                                                if (lastCommaIndex2 >= 0)
                                                {
                                                    sb.Remove(lastCommaIndex2, 1);
                                                }

                                                sb.Append($@"
                                                        ]
                                                    }}
                                                    }}
                                                ]
                                                }}");

                                                string customjson = sb.ToString();
                                                
                                                await notifyService.ReplyCustomJsonAsync(replyToken!,customjson);
                                            }
                                            else
                                            {
                                                await notifyService.ReplyMessageAsync(replyToken!, "⚠️ 找不到符合的感測器");
                                                Console.WriteLine("⚠️ 找不到符合的感測器");
                                            }
                                        }else{
                                            string nv_sql = @"
                                            SELECT * 
                                            FROM Sensors
                                            WHERE SensorType = @SensorType AND AreaID = @AreaID";

                                            SqlParameter[] areaid_parameters = new SqlParameter[]
                                            {
                                                new SqlParameter("@SensorType", sensorTypeValue),
                                                new SqlParameter("@AreaID", areaIdValue)
                                            };

                                            DataTable sensorData = sqlHelper.ExecuteQuery(nv_sql, areaid_parameters);
                                            if (sensorData.Rows.Count > 0)
                                            {
                                                StringBuilder sb = new StringBuilder();

                                                sb.Append($@"
                                                {{
                                                ""replyToken"": ""{replyToken}"",
                                                ""messages"": [
                                                    {{
                                                    ""type"": ""flex"",
                                                    ""altText"": ""查詢資料選單"",
                                                    ""contents"": {{
                                                        ""type"": ""carousel"",
                                                        ""contents"": [
                                                ");

                                                int btnsPerBubble = 5;
                                                int totalBtns = sensorData.Rows.Count;

                                                int index = 0; // 用來追蹤 DataTable 的資料
                                                while (index < totalBtns)
                                                {
                                                    sb.Append($@"
                                                        {{
                                                            ""type"": ""bubble"",
                                                            ""body"": {{
                                                            ""type"": ""box"",
                                                            ""layout"": ""vertical"",
                                                            ""contents"": [
                                                                {{
                                                                ""type"": ""text"",
                                                                ""text"": ""請選擇要查詢的感測器"",
                                                                ""weight"": ""bold"",
                                                                ""size"": ""lg"",
                                                                ""margin"": ""md""
                                                                }},
                                                    ");

                                                    int currentCount = 0;
                                                    while (index < totalBtns && currentCount < btnsPerBubble)
                                                    {
                                                        string label = sensorData.Rows[index]["SensorNameA"].ToString() ?? "";

                                                        index++; // 無論如何 index 先往後移，避免死迴圈

                                                        sb.Append($@"
                                                                {{
                                                                ""type"": ""button"",
                                                                ""action"": {{
                                                                    ""type"": ""message"",
                                                                    ""label"": ""{label}"",
                                                                    ""text"": ""{label}""
                                                                }},
                                                                ""style"": ""primary"",
                                                                ""margin"": ""sm""
                                                                }},
                                                        ");

                                                        currentCount++;
                                                    }

                                                    // 移除最後一個逗號
                                                    int lastCommaIndex = sb.ToString().LastIndexOf(',');
                                                    if (lastCommaIndex >= 0)
                                                    {
                                                        sb.Remove(lastCommaIndex, 1);
                                                    }

                                                    sb.Append($@"
                                                            ]
                                                            }}
                                                        }},
                                                    ");
                                                }

                                                // 移除最後一個逗號（如果存在）
                                                int lastCommaIndex2 = sb.ToString().LastIndexOf(',');
                                                if (lastCommaIndex2 >= 0)
                                                {
                                                    sb.Remove(lastCommaIndex2, 1);
                                                }

                                                sb.Append($@"
                                                        ]
                                                    }}
                                                    }}
                                                ]
                                                }}");

                                                string customjson = sb.ToString();
                                                
                                                await notifyService.ReplyCustomJsonAsync(replyToken!,customjson);
                                            }
                                            else
                                            {
                                                await notifyService.ReplyMessageAsync(replyToken!, "⚠️ 找不到符合的感測器");
                                                Console.WriteLine("⚠️ 找不到符合的感測器");
                                            }
                                        }

                                        string sql = @"
                                        IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                                        BEGIN
                                            UPDATE LineTemp
                                            SET Status = @Status
                                            WHERE UserId = @UserId;
                                        END
                                        ELSE
                                        BEGIN
                                            INSERT INTO LineTemp (UserId, Status)
                                            VALUES (@UserId, @Status);
                                        END
                                        ";

                                        SqlParameter[] s_parameters = new SqlParameter[]
                                        {
                                            new SqlParameter("@UserId", userId),
                                            new SqlParameter("@Status", status)
                                        };

                                        try
                                        {
                                            int rows = sqlHelper.ExecuteNonQuery(sql, s_parameters);
                                            if (rows > 0)
                                            {
                                                Console.WriteLine("✅ 資料更新成功");
                                            }
                                            else
                                            {
                                                Console.WriteLine("⚠️ 無資料被更新");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        await notifyService.ReplyMessageAsync(replyToken!, $"發生錯誤： + {ex.Message}");
                                        Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                    }

                                    return Ok();
                                }

                                if (user_status == "camera") //更新user當前狀態保持"camera"
                                {   
                                    DataTable result_dt = new DataTable();
                                    string camId = "";

                                    string sn_selectSql = "SELECT TOP 1 CamID FROM Cameras WHERE CamName = @CamName";

                                    SqlParameter[] water_parameters = new SqlParameter[]
                                    {
                                        new SqlParameter("@CamName", userText)
                                    };

                                    try
                                    {
                                        result_dt = sqlHelper.ExecuteQuery(sn_selectSql, water_parameters); // 有參數查詢

                                        if (result_dt.Rows.Count > 0)
                                        {
                                            camId = result_dt.Rows[0]["CamID"].ToString() ?? "";
                                            Console.WriteLine($"✅ 查到 camID：{camId}");
                                            if (conn != null)
                                            {
                                                string sql = @"
                                                IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                                                BEGIN
                                                    UPDATE LineTemp
                                                    SET Status = @Status,
                                                        SensorID = @SensorID,
                                                        SensorName = @SensorName
                                                    WHERE UserId = @UserId;
                                                END
                                                ELSE
                                                BEGIN
                                                    INSERT INTO LineTemp (UserId, Status, SensorID, SensorName)
                                                    VALUES (@UserId, @Status, @SensorID, @SensorName);
                                                END
                                                ";

                                                SqlParameter[] sn_parameters = new SqlParameter[]
                                                {
                                                    new SqlParameter("@UserId", userId),
                                                    new SqlParameter("@Status", "camera"),
                                                    new SqlParameter("@SensorID", camId),
                                                    new SqlParameter("@SensorName", userText)
                                                };

                                                try
                                                {
                                                    int rows = sqlHelper.ExecuteNonQuery(sql, sn_parameters);
                                                    if (rows > 0)
                                                    {
                                                        Console.WriteLine("✅ 資料更新成功");
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("⚠️ 無資料被更新");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("⚠️ 找不到符合條件的感測器");
                                            await notifyService.ReplyMessageAsync(replyToken!, "找不到該感測器名稱");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                        await notifyService.ReplyMessageAsync(replyToken!, $"查詢失敗：{ex.Message}");
                                    }

                                    string messageText = $"監視器：{userText} 最新畫面";
                                    string ImageUrl = $"https://rivermonitoring.wra10.gov.tw/fx/videoimages/{camId}.jpg"; //抓十河網頁的
                                    Console.WriteLine(ImageUrl);
                                    await notifyService.ReplyMessageWithImageAsync(replyToken!, messageText, ImageUrl);
                                    return Ok();
                                }

                                if (user_status == "sensor") //更新user當前狀態保持"datepicker"
                                {   
                                    DataTable result_dt = new DataTable();
                                    string sna_selectSql = "SELECT TOP 1 SensorID FROM Sensors WHERE SensorNameA = @SensorNameA";

                                    SqlParameter[] water_parameters = new SqlParameter[]
                                    {
                                        new SqlParameter("@SensorNameA", userText)
                                    };

                                    try
                                    {
                                        result_dt = sqlHelper.ExecuteQuery(sna_selectSql, water_parameters); // 有參數查詢

                                        if (result_dt.Rows.Count > 0)
                                        {
                                            string sensorId = result_dt.Rows[0]["SensorID"].ToString() ?? "";
                                            Console.WriteLine($"✅ 查到 SensorID：{sensorId}");

                                            string sql = @"
                                            IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                                            BEGIN
                                                UPDATE LineTemp
                                                SET Status = @Status,
                                                    SensorID = @SensorID,
                                                    SensorName = @SensorName
                                                WHERE UserId = @UserId;
                                            END
                                            ELSE
                                            BEGIN
                                                INSERT INTO LineTemp (UserId, Status, SensorID, SensorName)
                                                VALUES (@UserId, @Status, @SensorID, @SensorName);
                                            END
                                            ";

                                            SqlParameter[] isn_parameters = new SqlParameter[]
                                            {
                                                new SqlParameter("@UserId", userId),
                                                new SqlParameter("@Status", "datepicker"),
                                                new SqlParameter("@SensorID", sensorId),
                                                new SqlParameter("@SensorName", userText)
                                            };

                                            try
                                            {
                                                int rows = sqlHelper.ExecuteNonQuery(sql, isn_parameters);
                                                if (rows > 0)
                                                {
                                                    Console.WriteLine("✅ 資料更新成功");
                                                }
                                                else
                                                {
                                                    Console.WriteLine("⚠️ 無資料被更新");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("⚠️ 找不到符合條件的感測器");
                                            await notifyService.ReplyMessageAsync(replyToken!, "找不到該感測器名稱");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                        await notifyService.ReplyMessageAsync(replyToken!, $"查詢失敗：{ex.Message}");
                                    }
                                    await notifyService.ReplyDatePickerAsync(replyToken!);
                                    return Ok();
                                }

                                if(user_status == "datepicker" && userText == "送出查詢"){
                                    Console.WriteLine($"✅ 查詢目標 Status：{user_status}, Area：{user_area}, Type：{user_type}, SensorId：{user_sensorId}, SensorName：{user_sensorName}, StartDate：{user_startDate}, EndDate：{user_endDate}");
                                    DataTable result_dt = new DataTable();

                                    string d_selectSql = @"
                                        SELECT *
                                        FROM SensorData
                                        WHERE SensorId = @SensorId
                                        AND RecordTime BETWEEN @StartDate AND @EndDate
                                        ORDER BY RecordTime ASC";

                                    try
                                    {
                                        // 這些值已經從 LineTemp 取得
                                        string sensorId = user_sensorId;
                                        DateTime startDate = user_startDate ?? DateTime.Now.AddDays(-7); // 預設過去7天
                                        DateTime endDate = user_endDate ?? DateTime.Now;
                                        endDate = endDate.AddDays(1);

                                        // 傳入參數
                                        SqlParameter[] d_parameters = new SqlParameter[]
                                        {
                                            new SqlParameter("@SensorId", sensorId),
                                            new SqlParameter("@StartDate", startDate),
                                            new SqlParameter("@EndDate", endDate)
                                        };

                                        result_dt = sqlHelper.ExecuteQuery(d_selectSql, d_parameters);

                                        if (result_dt.Rows.Count == 0)
                                        {
                                            await notifyService.ReplyMessageAsync(replyToken!, "查無資料");
                                            return Ok();
                                        }

                                        string customjson = $@"
                                        {{
                                            ""replyToken"": ""{replyToken}"",
                                            ""messages"": [
                                                {{
                                                    ""type"": ""text"",
                                                    ""text"": ""測試""
                                                }}
                                            ]
                                        }}";

                                        await notifyService.ReplyCustomJsonAsync(replyToken!, customjson);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("❌ 發生錯誤：" + ex.Message);
                                        await notifyService.ReplyMessageAsync(replyToken!, $"查詢失敗：{ex.Message}");
                                    }
                                    return Ok();
                                }

                                await notifyService.ReplyMessageAsync(replyToken!, $"不明指令：「{userText}」");
                            }
                        }
                    }else{
                        await notifyService.ReplyMessageAsync(replyToken!, $"資料庫連接失敗");
                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Webhook 錯誤：" + ex.Message);
            }

            return Ok();
        }

    }
}

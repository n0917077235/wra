using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Wra10Core2023.Services;

using Microsoft.Data.SqlClient;
using SqlHelper = Wra10Core2023.Util.SQLHelper;
using System.Data;
using System.Text;

using System.IO;
using Newtonsoft.Json;
using Wra10Core2023.Models;
using System.Drawing;
// using System.Windows.Forms.DataVisualization.Charting; Chart命名衝突

using Wra10Core2023.Util;


namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class LineWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SqlHelper sqlHelper;
        private readonly string? conn;
        private readonly IWebHostEnvironment _env;

        public LineWebhookController(IWebHostEnvironment env, IConfiguration configuration)
        {
            _configuration = configuration;
            conn = _configuration.GetConnectionString("Water2022");
            sqlHelper = new SqlHelper(conn);
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            try
            {
                var json = JsonDocument.Parse(body);
                var events = json.RootElement.GetProperty("events");

                foreach (var ev in events.EnumerateArray())
                {
                    var type = ev.GetProperty("type").GetString();
                    var replyToken = ev.GetProperty("replyToken").GetString();
                    var source = ev.GetProperty("source");
                    
                    // 檢查訊息來源類型
                    var sourceType = source.GetProperty("type").GetString();
                    if (sourceType == "group" || sourceType == "room")
                    {
                        // 如果是來自群組或聊天室的訊息，直接忽略不處理
                        //Console.WriteLine("Group ID：" + source.GetProperty("groupId").GetString());
                        return Ok();
                    }
                    
                    var userId = source.GetProperty("userId").GetString();

                    // 取得使用者狀態
                    var userInfo = GetUserInfo(userId);

                    var channelToken = _configuration["Line:channelAccessToken"];
                    var notify = new NotifyService(channelToken, _configuration);

                    if (conn == null)
                    {
                        await notify.ReplyTextAsync(replyToken!, $"資料庫連接失敗，請稍後再試或聯絡管理員。");
                        return Ok();
                    }

                    // 新增：流程中斷自動恢復引導
                    // 若收到「恢復查詢」或「回到上一步」等指令，嘗試恢復狀態
                    if (type == "message")
                    {
                        var message = ev.GetProperty("message");
                        var messageType = message.GetProperty("type").GetString();

                        if (messageType == "text")
                        {
                            var userText = message.GetProperty("text").GetString();

                            if (userText == "恢復查詢")
                            {
                                // 檢查目前狀態，依狀態回覆相應選單或提示
                                switch (userInfo.Status)
                                {
                                    case "start":
                                        await notify.ReplyMenuAsync(replyToken!);
                                        break;
                                    case "type":
                                        await ReplySensorTypeMenuAsync(notify, replyToken!);
                                        break;
                                    case "area":
                                        await ReplyAreaMenuAsync(notify, replyToken!);
                                        break;
                                    case "sensor":
                                        await notify.ReplyTextAsync(replyToken!, "請選擇感測器名稱。");
                                        break;
                                    case "camera":
                                        await notify.ReplyTextAsync(replyToken!, "請選擇監視器名稱。");
                                        break;
                                    case "datepicker":
                                        await notify.ReplyDatePickerAsync(replyToken!, start: userInfo.StartDate?.ToString("yyyy-MM-dd"), end: userInfo.EndDate?.ToString("yyyy-MM-dd"));
                                        break;
                                    default:
                                        await notify.ReplyTextAsync(replyToken!, "目前查詢流程已中斷，請輸入「開始查詢」重新開始。");
                                        break;
                                }
                                return Ok();
                            }

                            if (userText == "回到上一步")
                            {
                                // 根據目前狀態回到上一個狀態
                                string prevStatus = GetPreviousStatus(userInfo.Status);
                                await SetUserStatusAsync(userId, prevStatus);
                                switch (prevStatus)
                                {
                                    case "start":
                                        await notify.ReplyMenuAsync(replyToken!);
                                        break;
                                    case "type":
                                        await ReplySensorTypeMenuAsync(notify, replyToken!);
                                        break;
                                    case "area":
                                        await ReplyAreaMenuAsync(notify, replyToken!);
                                        break;
                                    default:
                                        await notify.ReplyTextAsync(replyToken!, "已回到主選單，請繼續操作。");
                                        break;
                                }
                                return Ok();
                            }
                        }
                    }

                    // 處理 postback
                    if (type == "postback")
                    {
                        try
                        {
                            var postback = ev.GetProperty("postback");
                            var postbackParams = postback.GetProperty("params");
                            var input_date = postbackParams.GetProperty("date").GetString();
                            var action = postback.GetProperty("data").GetString();

                            if (action == "action=startDate" || action == "action=endDate")
                            {
                                await UpdateDateAsync(userId, action, input_date);
                                // 重新取得最新的 userInfo
                                var updatedUserInfo = GetUserInfo(userId);
                                var startDateText = updatedUserInfo.StartDate?.ToString("yyyy-MM-dd") ?? "";
                                var endDateText = updatedUserInfo.EndDate?.ToString("yyyy-MM-dd") ?? "";
                                await notify.ReplyDatePickerAsync(replyToken!, start: startDateText, end: endDateText);
                                return Ok();
                            }
                            else
                            {
                                await notify.ReplyTextAsync(replyToken!, $"無法識別的日期選擇操作，請重新選擇日期。");
                                return Ok();
                            }
                        }
                        catch (Exception ex)
                        {
                            await notify.ReplyTextAsync(replyToken!, $"日期選擇發生錯誤，請重新操作或聯絡管理員。");
                            continue;
                        }
                    }

                    // 處理 message
                    if (type == "message")
                    {
                        var message = ev.GetProperty("message");
                        var messageType = message.GetProperty("type").GetString();

                        if (messageType == "text")
                        {
                            var userText = message.GetProperty("text").GetString();

                            // 只要收到「開始查詢」就重設狀態並清除查詢參數
                            if (userText == "開始查詢")
                            {
                                await ResetUserQueryAsync(userId);
                                await SetUserStatusAsync(userId, "start");
                                await notify.ReplyMenuAsync(replyToken!);
                                return Ok();
                            }

                            // 狀態機邏輯
                            switch (userInfo.Status)
                            {
                                case "start":
                                    if (userText == "選擇類型")
                                    {
                                        await SetUserStatusAsync(userId, "type");
                                        await ReplySensorTypeMenuAsync(notify, replyToken!);
                                        return Ok();
                                    }
                                    if (userText == "選擇河系")
                                    {
                                        await SetUserStatusAsync(userId, "area");
                                        await ReplyAreaMenuAsync(notify, replyToken!);
                                        return Ok();
                                    }
                                    if (userText!.StartsWith("確認"))
                                    {
                                        await HandleConfirmAsync(userId, userText, notify, replyToken!);
                                        return Ok();
                                    }
                                    break;

                                case "type":
                                    if (string.IsNullOrWhiteSpace(userText))
                                    {
                                        await notify.ReplyTextAsync(replyToken!, "請選擇感測器類型。");
                                        return Ok();
                                    }
                                    await HandleTypeSelectedAsync(userId, userText, notify, replyToken!, userInfo.Area);
                                    return Ok();

                                case "area":
                                    if (string.IsNullOrWhiteSpace(userText))
                                    {
                                        await notify.ReplyTextAsync(replyToken!, "請選擇感測器河系。");
                                        return Ok();
                                    }
                                    await HandleAreaSelectedAsync(userId, userText, notify, replyToken!, userInfo.Type);
                                    return Ok();

                                case "sensor":
                                    if (string.IsNullOrWhiteSpace(userText))
                                    {
                                        await notify.ReplyTextAsync(replyToken!, "請選擇感測器名稱。");
                                        return Ok();
                                    }
                                    await HandleSensorSelectedAsync(userId, userText, notify, replyToken!);
                                    return Ok();

                                case "camera":
                                    if (string.IsNullOrWhiteSpace(userText))
                                    {
                                        await notify.ReplyTextAsync(replyToken!, "請選擇監視器名稱。");
                                        return Ok();
                                    }
                                    await HandleCameraSelectedAsync(userId, userText, notify, replyToken!);
                                    return Ok();

                                case "datepicker":
                                    if (userText == "送出查詢")
                                    {
                                        // 檢查日期是否已選
                                        if (userInfo.StartDate == null || userInfo.EndDate == null)
                                        {
                                            await notify.ReplyTextAsync(replyToken!, "請先選擇完整的查詢日期範圍。");
                                            await notify.ReplyDatePickerAsync(replyToken!, start: userInfo.StartDate?.ToString("yyyy-MM-dd"), end: userInfo.EndDate?.ToString("yyyy-MM-dd"));
                                            return Ok();
                                        }
                                        await HandleQueryAsync(userInfo, notify, replyToken!);
                                        return Ok();
                                    }
                                    else
                                    {
                                        await notify.ReplyTextAsync(replyToken!, "請使用下方按鈕選擇日期並送出查詢。");
                                        return Ok();
                                    }
                            }

                            // 未知狀態或指令
                            await notify.ReplyTextAsync(replyToken!, $"不明指令：「{userText}」，請依照選單操作或輸入「開始查詢」重新開始。");
                        }
                        else
                        {
                            await notify.ReplyTextAsync(replyToken!, "目前僅支援文字訊息查詢，請依照選單操作。");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Webhook 錯誤：" + ex.Message);
            }

            return Ok();
        }

        // --- 新增輔助方法區 ---
        private (string Status, string Area, string Type, string SensorId, string SensorName, string Unit, DateTime? StartDate, DateTime? EndDate) GetUserInfo(string userId)
        {
            string status = "", area = "", type = "", sensorId = "", sensorName = "", unit = "";
            DateTime? startDate = null, endDate = null;

            string selectSql = "SELECT * FROM LineTemp WHERE UserId = @UserId";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };
            try
            {
                DataTable dt = sqlHelper.ExecuteQuery(selectSql, parameters);
                if (dt.Rows.Count > 0)
                {
                    status = dt.Rows[0]["Status"].ToString() ?? "";
                    area = dt.Rows[0]["Area"].ToString() ?? "";
                    type = dt.Rows[0]["Type"].ToString() ?? "";
                    sensorId = dt.Rows[0]["SensorId"].ToString() ?? "";
                    sensorName = dt.Rows[0]["SensorName"].ToString() ?? "";
                    unit = dt.Rows[0]["unit"].ToString() ?? "";
                    if (!Convert.IsDBNull(dt.Rows[0]["StartDate"])) startDate = Convert.ToDateTime(dt.Rows[0]["StartDate"]);
                    if (!Convert.IsDBNull(dt.Rows[0]["EndDate"])) endDate = Convert.ToDateTime(dt.Rows[0]["EndDate"]);
                }
            }
            catch { }
            return (status, area, type, sensorId, sensorName, unit, startDate, endDate);
        }

        private async Task SetUserStatusAsync(string userId, string status)
        {
            string sql = @"
                IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                BEGIN
                    UPDATE LineTemp SET Status = @Status WHERE UserId = @UserId;
                END
                ELSE
                BEGIN
                    INSERT INTO LineTemp (UserId, Status) VALUES (@UserId, @Status);
                END";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId), new SqlParameter("@Status", status) };
            sqlHelper.ExecuteNonQuery(sql, parameters);
        }

        private async Task UpdateDateAsync(string userId, string action, string date)
        {
            string field = action == "action=startDate" ? "StartDate" : "EndDate";
            string sql = $@"
                IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                BEGIN
                    UPDATE LineTemp SET {field} = @{field} WHERE UserId = @UserId;
                END
                ELSE
                BEGIN
                    INSERT INTO LineTemp (UserId, {field}) VALUES (@UserId, @{field});
                END";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId), new SqlParameter($"@{field}", date) };
            sqlHelper.ExecuteNonQuery(sql, parameters);
        }

        private async Task ReplySensorTypeMenuAsync(NotifyService notify, string replyToken)
        {
            DataTable dt = sqlHelper.ExecuteQuery("SELECT * FROM SensorTypes");
            int btnsPerBubble = 5;
            var carouselContents = new List<object>();
            for (int i = 0; i < dt.Rows.Count; i += btnsPerBubble)
            {
                var buttons = new List<object>
                {
                    new { type = "text", text = "請選擇感測器類型", weight = "bold", size = "lg", margin = "md" }
                };
                for (int j = i; j < Math.Min(i + btnsPerBubble, dt.Rows.Count); j++)
                {
                    string label = dt.Rows[j]["SensorTypeName"].ToString() ?? "";
                    buttons.Add(new
                    {
                        type = "button",
                        action = new { type = "message", label = label, text = label },
                        style = "primary",
                        margin = "sm"
                    });
                }
                carouselContents.Add(new
                {
                    type = "bubble",
                    body = new { type = "box", layout = "vertical", contents = buttons }
                });
            }
            var flexMessage = new
            {
                replyToken,
                messages = new[] {
                    new {
                        type = "flex",
                        altText = "查詢資料選單",
                        contents = new { type = "carousel", contents = carouselContents }
                    }
                }
            };
            string customJson = JsonConvert.SerializeObject(flexMessage, Formatting.None);
            await notify.ReplyCustomJsonAsync(replyToken, customJson);
        }

        private async Task ReplyAreaMenuAsync(NotifyService notify, string replyToken)
        {
            DataTable dt = sqlHelper.ExecuteQuery("SELECT * FROM Areas");
            int btnsPerBubble = 5;
            var bubbles = new List<object>();
            for (int i = 0; i < dt.Rows.Count; i += btnsPerBubble)
            {
                var buttons = new List<object>
                {
                    new { type = "text", text = "請選擇感測器類河系", weight = "bold", size = "lg", margin = "md" }
                };
                for (int j = i; j < Math.Min(i + btnsPerBubble, dt.Rows.Count); j++)
                {
                    string label = dt.Rows[j]["AreaName"]?.ToString() ?? "";
                    buttons.Add(new
                    {
                        type = "button",
                        action = new { type = "message", label = label, text = label },
                        style = "primary",
                        margin = "sm"
                    });
                }
                bubbles.Add(new
                {
                    type = "bubble",
                    body = new { type = "box", layout = "vertical", contents = buttons }
                });
            }
            var flexMessage = new
            {
                replyToken,
                messages = new[] {
                    new {
                        type = "flex",
                        altText = "查詢資料選單",
                        contents = new { type = "carousel", contents = bubbles }
                    }
                }
            };
            string customJson = JsonConvert.SerializeObject(flexMessage, Formatting.None);
            await notify.ReplyCustomJsonAsync(replyToken, customJson);
        }

        private async Task HandleTypeSelectedAsync(string userId, string type, NotifyService notify, string replyToken, string area)
        {
            await notify.ReplyMenuAsync(replyToken, area: area, type: type);
            string sql = @"
                DECLARE @Unit NVARCHAR(50);
                SELECT TOP 1 @Unit = Unit FROM SensorTypes WHERE SensorTypeName = @SensorTypeName;
                IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                BEGIN
                    UPDATE LineTemp SET Status = @Status, Type = @Type, Unit = @Unit WHERE UserId = @UserId;
                END
                ELSE
                BEGIN
                    INSERT INTO LineTemp (UserId, Status, Type, Unit) VALUES (@UserId, @Status, @Type, @Unit);
                END";
            SqlParameter[] parameters = {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@Status", "start"),
                new SqlParameter("@Type", type),
                new SqlParameter("@SensorTypeName", type)
            };
            sqlHelper.ExecuteNonQuery(sql, parameters);
        }

        private async Task HandleAreaSelectedAsync(string userId, string area, NotifyService notify, string replyToken, string type)
        {
            await notify.ReplyMenuAsync(replyToken, area: area, type: type);
            string sql = @"
                IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                BEGIN
                    UPDATE LineTemp SET Status = @Status, Area = @Area WHERE UserId = @UserId;
                END
                ELSE
                BEGIN
                    INSERT INTO LineTemp (UserId, Status, Area) VALUES (@UserId, @Status, @Area);
                END";
            SqlParameter[] parameters = {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@Status", "start"),
                new SqlParameter("@Area", area)
            };
            sqlHelper.ExecuteNonQuery(sql, parameters);
        }

        private async Task HandleConfirmAsync(string userId, string userText, NotifyService notify, string replyToken)
        {
            string content = userText.Substring(3).Trim();
            var parts = content.Split('/');
            string area = parts.Length > 0 ? parts[0] : "";
            string s_type = parts.Length > 1 ? parts[1] : "";

            string sensorSql = "SELECT SensorType FROM SensorTypes WHERE SensorTypeName = @Type";
            string areaSql = "SELECT AreaID FROM Areas WHERE AreaName = @Area";
            SqlParameter[] typeParams = { new SqlParameter("@Type", s_type) };
            SqlParameter[] areaParams = { new SqlParameter("@Area", area) };

            string sensorTypeValue = "";
            string areaIdValue = "";

            var sensorDt = sqlHelper.ExecuteQuery(sensorSql, typeParams);
            if (sensorDt.Rows.Count > 0)
                sensorTypeValue = sensorDt.Rows[0]["SensorType"]?.ToString() ?? "";

            var areaDt = sqlHelper.ExecuteQuery(areaSql, areaParams);
            if (areaDt.Rows.Count > 0)
                areaIdValue = areaDt.Rows[0]["AreaID"]?.ToString() ?? "";

            string status = s_type == "影像" ? "camera" : "sensor";
            DataTable targetData;
            if (s_type == "影像")
            {
                var lstParams = new List<SqlParameter>
                {
                    new SqlParameter("@areaId", string.IsNullOrEmpty(areaIdValue) ? DBNull.Value : areaIdValue),
                    new SqlParameter("@StationId", DBNull.Value),
                    new SqlParameter("@keyword", DBNull.Value)
                };
                targetData = sqlHelper.ExecuteStoreProcedureQuery("sp_GetCameraBySearch", lstParams.ToArray());
            }
            else
            {
                string nv_sql = @"
                    SELECT * FROM Sensors WHERE SensorType = @SensorType AND AreaID = @AreaID";
                SqlParameter[] areaid_parameters =
                {
                    new SqlParameter("@SensorType", sensorTypeValue),
                    new SqlParameter("@AreaID", areaIdValue)
                };
                targetData = sqlHelper.ExecuteQuery(nv_sql, areaid_parameters);
            }

            if (targetData.Rows.Count > 0)
            {
                var bubbles = new List<object>();
                int btnsPerBubble = 5;
                int totalBtns = targetData.Rows.Count;
                int index = 0;
                while (index < totalBtns)
                {
                    var buttons = new List<object>
                    {
                        new
                        {
                            type = "text",
                            text = s_type == "影像" ? "請選擇要查詢的監視器" : "請選擇要查詢的感測器",
                            weight = "bold",
                            size = "lg",
                            margin = "md"
                        }
                    };
                    int currentCount = 0;
                    while (index < totalBtns && currentCount < btnsPerBubble)
                    {
                        string label = s_type == "影像"
                            ? targetData.Rows[index]["CamName"]?.ToString() ?? ""
                            : targetData.Rows[index]["SensorNameA"]?.ToString() ?? "";
                        buttons.Add(new
                        {
                            type = "button",
                            action = new { type = "message", label = label, text = label },
                            style = "primary",
                            margin = "sm"
                        });
                        index++;
                        currentCount++;
                    }
                    bubbles.Add(new
                    {
                        type = "bubble",
                        body = new { type = "box", layout = "vertical", contents = buttons }
                    });
                }
                var flexMessage = new
                {
                    replyToken,
                    messages = new[] {
                        new {
                            type = "flex",
                            altText = "查詢資料選單",
                            contents = new { type = "carousel", contents = bubbles }
                        }
                    }
                };
                string customjson = JsonConvert.SerializeObject(flexMessage, Formatting.None);
                await notify.ReplyCustomJsonAsync(replyToken, customjson);
            }
            else
            {
                await notify.ReplyTextAsync(replyToken, "⚠️ 找不到符合的感測器");
            }

            // 更新狀態
            await SetUserStatusAsync(userId, status);
        }

        private async Task HandleSensorSelectedAsync(string userId, string sensorName, NotifyService notify, string replyToken)
        {
            string sna_selectSql = "SELECT TOP 1 SensorID FROM Sensors WHERE SensorNameA = @SensorNameA";
            var water_parameters = new[] { new SqlParameter("@SensorNameA", sensorName) };
            DataTable result_dt = sqlHelper.ExecuteQuery(sna_selectSql, water_parameters);
            if (result_dt.Rows.Count > 0)
            {
                string sensorId = result_dt.Rows[0]["SensorID"].ToString() ?? "";
                string sql = @"
                    IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                    BEGIN
                        UPDATE LineTemp SET Status = @Status, SensorID = @SensorID, SensorName = @SensorName WHERE UserId = @UserId;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO LineTemp (UserId, Status, SensorID, SensorName) VALUES (@UserId, @Status, @SensorID, @SensorName);
                    END";
                var isn_parameters = new[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@Status", "datepicker"),
                    new SqlParameter("@SensorID", sensorId),
                    new SqlParameter("@SensorName", sensorName)
                };
                sqlHelper.ExecuteNonQuery(sql, isn_parameters);
                await notify.ReplyDatePickerAsync(replyToken);
            }
            else
            {
                await notify.ReplyTextAsync(replyToken, "找不到該感測器名稱");
            }
        }

        private async Task HandleCameraSelectedAsync(string userId, string camName, NotifyService notify, string replyToken)
        {
            string sn_selectSql = "SELECT TOP 1 CamID FROM Cameras WHERE CamName = @CamName";
            SqlParameter[] water_parameters = { new SqlParameter("@CamName", camName) };
            DataTable result_dt = sqlHelper.ExecuteQuery(sn_selectSql, water_parameters);
            if (result_dt.Rows.Count > 0)
            {
                string camId = result_dt.Rows[0]["CamID"].ToString() ?? "";
                string sql = @"
                    IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                    BEGIN
                        UPDATE LineTemp SET Status = @Status, SensorID = @SensorID, SensorName = @SensorName WHERE UserId = @UserId;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO LineTemp (UserId, Status, SensorID, SensorName) VALUES (@UserId, @Status, @SensorID, @SensorName);
                    END";
                SqlParameter[] sn_parameters = {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@Status", "camera"),
                    new SqlParameter("@SensorID", camId),
                    new SqlParameter("@SensorName", camName)
                };
                sqlHelper.ExecuteNonQuery(sql, sn_parameters);
                string messageText = $"監視器：{camName} 最新畫面";
                string apiurl = SiteUtil.RemoteVideoImageUrl;
                string ImageUrl = $"{apiurl}/videoimages/{camId}.jpg";
                await notify.ReplyImageAsync(replyToken, messageText, ImageUrl);
            }
            else
            {
                await notify.ReplyTextAsync(replyToken, "找不到該感測器名稱");
            }
        }

        private async Task HandleQueryAsync(
            (string Status, string Area, string Type, string SensorId, string SensorName, string Unit, DateTime? StartDate, DateTime? EndDate) userInfo,
            NotifyService notify, string replyToken)
        {
            // 查詢參數完整性檢查
            var missingParams = new List<string>();
            if (string.IsNullOrWhiteSpace(userInfo.Area)) missingParams.Add("感測器河系");
            if (string.IsNullOrWhiteSpace(userInfo.Type)) missingParams.Add("感測器類型");
            if (string.IsNullOrWhiteSpace(userInfo.SensorId)) missingParams.Add("感測器ID");
            if (string.IsNullOrWhiteSpace(userInfo.SensorName)) missingParams.Add("感測器名稱");
            if (string.IsNullOrWhiteSpace(userInfo.Unit)) missingParams.Add("單位");
            if (userInfo.StartDate == null) missingParams.Add("開始日期");
            if (userInfo.EndDate == null) missingParams.Add("結束日期");

            if (missingParams.Count > 0)
            {
                string msg = $"⚠️ 查詢參數不完整，缺少：{string.Join("、", missingParams)}。\n請依照選單操作或輸入「開始查詢」重新開始。";
                await notify.ReplyTextAsync(replyToken, msg);
                return;
            }

            string d_selectSql = @"
                SELECT * FROM SensorData
                WHERE SensorId = @SensorId
                AND RecordTime BETWEEN @StartDate AND @EndDate
                ORDER BY RecordTime ASC";
            string sensorId = userInfo.SensorId;
            DateTime startDate = userInfo.StartDate ?? DateTime.Now.AddDays(-7);
            DateTime endDate = (userInfo.EndDate ?? DateTime.Now).AddDays(1);
            SqlParameter[] d_parameters = {
                new SqlParameter("@SensorId", sensorId),
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };
            DataTable result_dt = sqlHelper.ExecuteQuery(d_selectSql, d_parameters);
            if (result_dt.Rows.Count == 0)
            {
                await notify.ReplyTextAsync(replyToken, "查無資料");
                return;
            }
            string filename = GenerateChartAndSave(result_dt, userInfo.Unit, startDate, endDate, userInfo.Area, userInfo.SensorName, userInfo.Type);
            string lineurl = _configuration["Line:lineUrl"];
            string imageUrl = $"{lineurl}/output/{filename}";
            string chartTitle = $"{userInfo.Area} {userInfo.SensorName}\n{startDate:yyyy-MM-dd} 至 {endDate:yyyy-MM-dd}\n感測器資料圖表";
            await notify.ReplyImageAsync(replyToken, chartTitle, imageUrl, imageUrl);
        }

        string GenerateChartAndSave(System.Data.DataTable dt, string unit, DateTime startDate, DateTime endDate,
                                    string area, string sensorName, string sensorType)
        {
            using (var chart = new System.Windows.Forms.DataVisualization.Charting.Chart())
            {
                chart.Width = 1920;
                chart.Height = 1080;
                chart.BackColor = System.Drawing.Color.White;

                var chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
                chartArea.BackColor = System.Drawing.Color.White;

                // X軸設定
                chartArea.AxisX.Title = "時間";
                chartArea.AxisX.TitleFont = new System.Drawing.Font("微軟正黑體", 16, System.Drawing.FontStyle.Bold);
                chartArea.AxisX.LabelStyle.Font = new System.Drawing.Font("微軟正黑體", 12);
                chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
                chartArea.AxisX.Minimum = startDate.ToOADate();
                chartArea.AxisX.Maximum = endDate.ToOADate();

                double totalDays = (endDate - startDate).TotalDays;
                if (totalDays <= 1)
                {
                    chartArea.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Hours;
                    chartArea.AxisX.Interval = 1;
                    chartArea.AxisX.LabelStyle.Format = "HH:mm";
                }
                else if (totalDays <= 7)
                {
                    chartArea.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Hours;
                    chartArea.AxisX.Interval = 6;
                    chartArea.AxisX.LabelStyle.Format = "MM-dd HH:mm";
                }
                else if (totalDays <= 31)
                {
                    chartArea.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Days;
                    chartArea.AxisX.Interval = 1;
                    chartArea.AxisX.LabelStyle.Format = "MM-dd";
                }
                else if (totalDays <= 92)
                {
                    chartArea.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Days;
                    chartArea.AxisX.Interval = 7;
                    chartArea.AxisX.LabelStyle.Format = "MM-dd";
                }
                else
                {
                    chartArea.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Months;
                    chartArea.AxisX.Interval = 1;
                    chartArea.AxisX.LabelStyle.Format = "yyyy-MM";
                }
                chartArea.AxisX.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.FixedCount;

                // Y軸設定
                chartArea.AxisY.Title = $"{area} {sensorName} {sensorType}數值 ({unit})";
                chartArea.AxisY.TitleFont = new System.Drawing.Font("微軟正黑體", 16, System.Drawing.FontStyle.Bold);
                chartArea.AxisY.LabelStyle.Font = new System.Drawing.Font("微軟正黑體", 12);
                chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
                chartArea.AxisY.LabelStyle.Format = "0.##";

                chart.ChartAreas.Add(chartArea);

                if (sensorType == "水位")
                {
                    // // Area Series（半透明淡藜色）可以保留註解
                    // double yMin = dt.AsEnumerable()
                    //                 .Where(r => Convert.ToDouble(r["Value1"]) != -998)
                    //                 .Select(r => Convert.ToDouble(r["Value1"]))
                    //                 .DefaultIfEmpty(0)
                    //                 .Min();
                    //
                    // var areaSeries = new System.Windows.Forms.DataVisualization.Charting.Series
                    // {
                    //     ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area,
                    //     Color = System.Drawing.Color.FromArgb(128, System.Drawing.Color.LightBlue),
                    //     BorderWidth = 0,
                    //     XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime
                    // };

                    var lineSeries = new System.Windows.Forms.DataVisualization.Charting.Series
                    {
                        ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                        Color = System.Drawing.Color.Blue,
                        BorderWidth = 3,
                        MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.None,
                        XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime
                    };

                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        double value = Convert.ToDouble(row["Value1"]);
                        if (value == -998) continue;
                        DateTime recordTime = Convert.ToDateTime(row["RecordTime"]);
                        lineSeries.Points.AddXY(recordTime, value);
                        // areaSeries.Points.AddXY(recordTime, value);
                    }

                    chart.Series.Add(lineSeries);
                    // chart.Series.Add(areaSeries);
                }
                else if (sensorType == "傾斜")
                {
                    // Value1線（藍色） - 平行牆面
                    var value1Series = new System.Windows.Forms.DataVisualization.Charting.Series
                    {
                        Name = "平行牆面",
                        ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                        Color = System.Drawing.Color.Blue,
                        BorderWidth = 3,
                        MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.None,
                        XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime
                    };

                    // Value2線（橘色） - 垂直牆面
                    var value2Series = new System.Windows.Forms.DataVisualization.Charting.Series
                    {
                        Name = "垂直牆面",
                        ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                        Color = System.Drawing.Color.Orange,
                        BorderWidth = 3,
                        MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.None,
                        XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime
                    };

                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        DateTime recordTime = Convert.ToDateTime(row["RecordTime"]);

                        double value1 = Convert.ToDouble(row["Value1"]);
                        if (value1 != -998)
                            value1Series.Points.AddXY(recordTime, value1);

                        double value2 = Convert.ToDouble(row["Value2"]);
                        if (value2 != -998)
                            value2Series.Points.AddXY(recordTime, value2);
                    }

                    chart.Series.Add(value1Series);
                    chart.Series.Add(value2Series);

                    // 啟用圖例（只建立一次）
                    if (chart.Legends.Count == 0)
                    {
                        var legend = new System.Windows.Forms.DataVisualization.Charting.Legend();
                        legend.Name = "Legend1";
                        chart.Legends.Add(legend);
                    }
                }
                else
                {
                    // 一般感測器只畫Value1線（藍色）
                    var series = new System.Windows.Forms.DataVisualization.Charting.Series
                    {
                        BorderWidth = 3,
                        Color = System.Drawing.Color.Blue,
                        XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime,
                        ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                        MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.None
                    };

                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        double value = Convert.ToDouble(row["Value1"]);
                        if (value == -998) continue;
                        DateTime recordTime = Convert.ToDateTime(row["RecordTime"]);
                        series.Points.AddXY(recordTime, value);
                    }

                    chart.Series.Add(series);
                }

                var outputPath = System.IO.Path.Combine(_env.WebRootPath, "output");
                if (!System.IO.Directory.Exists(outputPath)) System.IO.Directory.CreateDirectory(outputPath);

                string filename = $"chart_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string fullPath = System.IO.Path.Combine(outputPath, filename);

                chart.SaveImage(fullPath, System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Png);

                return filename;
            }
        }

        // 新增方法：重設使用者查詢參數
        private async Task ResetUserQueryAsync(string userId)
        {
            string sql = @"
                IF EXISTS (SELECT 1 FROM LineTemp WHERE UserId = @UserId)
                BEGIN
                    UPDATE LineTemp SET
                        Status = NULL,
                        Area = NULL,
                        Type = NULL,
                        SensorId = NULL,
                        SensorName = NULL,
                        Unit = NULL,
                        StartDate = NULL,
                        EndDate = NULL
                    WHERE UserId = @UserId;
                END";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };
            sqlHelper.ExecuteNonQuery(sql, parameters);
        }

        // 新增：取得上一個狀態的輔助方法
        private string GetPreviousStatus(string currentStatus)
        {
            switch (currentStatus)
            {
                case "datepicker": return "sensor";
                case "sensor": return "start";
                case "camera": return "start";
                case "type": return "start";
                case "area": return "start";
                default: return "start";
            }
        }

    }
}

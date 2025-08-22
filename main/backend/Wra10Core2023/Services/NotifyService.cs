using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Wra10Core2023.Models;

namespace Notify.Services
{
    public class NotifyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _channelAccessToken;
        private readonly IConfiguration _configuration;

        public NotifyService(string channelToken, IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _channelAccessToken = channelToken;
            _configuration = configuration;
        }

        private string GetGoogleApiKey() => _configuration["Line:GoogleAPIKey"];

        private async Task SendLineRequestAsync(string url, object payload)
        {
            var json = JsonConvert.SerializeObject(payload, Formatting.Indented);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                Console.WriteLine("✅ 成功");
            else
                Console.WriteLine($"❌ 失敗: {response.StatusCode}\n{content}");
        }

        private string GetPreviewUrl(string? preview, string original) => string.IsNullOrEmpty(preview) ? original : preview;

        // 推送文字訊息
        public Task PushTextAsync(string groupId, string messageText)
        {
            var payload = new LineRequest
            {
                to = groupId,
                messages = new List<LineMessage>
                {
                    new LineMessage { type = "text", text = messageText }
                }
            };
            return SendLineRequestAsync("https://api.line.me/v2/bot/message/push", payload);
        }

        // 推送文字 + 圖片
        public Task PushImageAsync(string groupId, string messageText, string imageUrl, string? previewImageUrl = null)
        {
            var payload = new LineRequest
            {
                to = groupId,
                messages = new List<LineMessage>
                {
                    new LineMessage { type = "text", text = messageText },
                    new LineMessage 
                    { 
                        type = "image", 
                        originalContentUrl = imageUrl, 
                        previewImageUrl = GetPreviewUrl(previewImageUrl, imageUrl)
                    }
                }
            };
            return SendLineRequestAsync("https://api.line.me/v2/bot/message/push", payload);
        }

        //推送文字 + map
        public Task PushMapAlertAsync(string groupId, string lat, string lng, string messageText)
        {
            string apiKey = GetGoogleApiKey();
            string googleMapLink = $"https://www.google.com/maps/search/?api=1&query={lat},{lng}";
            string mapUrl = $"https://maps.googleapis.com/maps/api/staticmap?" +
                            $"center={lat},{lng}" +
                            $"&zoom=16" +
                            $"&size=600x400" +
                            $"&markers=color:red%7Clabel:%7C{lat},{lng}" +
                            $"&key={apiKey}";

            // Flex Bubble body
            var body = new FlexBody
            {
                contents = new List<object>
                {
                    new { type = "text", text = messageText, wrap = true },
                    new { type = "image", url = mapUrl, size = "full", aspectMode = "cover" }
                }
            };

            // Footer 按鈕
            body.contents.Add(new
            {
                type = "box",
                layout = "vertical",
                contents = new List<object>
                {
                    new
                    {
                        type = "button",
                        action = new
                        {
                            type = "uri",
                            label = "打開地圖",
                            uri = googleMapLink
                        },
                        style = "primary"
                    }
                }
            });

            var bubble = new FlexBubble { body = body };
            var flexMessage = new FlexMessage { altText = "通報", contents = bubble };

            var payload = new LineRequest
            {
                to = groupId,
                messages = new List<LineMessage>
                {
                    new LineMessage
                    {
                        type = "flex",
                        altText = flexMessage.altText,
                        contents = flexMessage.contents
                    }
                }
            };

            return SendLineRequestAsync("https://api.line.me/v2/bot/message/push", payload);
        }


        // 回覆文字訊息
        public Task ReplyTextAsync(string replyToken, string messageText)
        {
            var payload = new LineRequest
            {
                replyToken = replyToken,
                messages = new List<LineMessage>
                {
                    new LineMessage { type = "text", text = messageText }
                }
            };
            return SendLineRequestAsync("https://api.line.me/v2/bot/message/reply", payload);
        }

        // 回覆文字 + 圖片
        public Task ReplyImageAsync(string replyToken, string messageText, string imageUrl, string? previewImageUrl = null)
        {
            var payload = new LineRequest
            {
                replyToken = replyToken,
                messages = new List<LineMessage>
                {
                    new LineMessage { type = "text", text = messageText },
                    new LineMessage 
                    { 
                        type = "image", 
                        originalContentUrl = imageUrl, 
                        previewImageUrl = GetPreviewUrl(previewImageUrl, imageUrl)
                    }
                }
            };
            return SendLineRequestAsync("https://api.line.me/v2/bot/message/reply", payload);
        }

        // 回覆按鈕選單
        public Task ReplyMenuAsync(string replyToken, string? area = null, string? type = null)
        {
            var actions = new List<LineAction>
            {
                new LineAction 
                { 
                    type = "message", 
                    label = string.IsNullOrWhiteSpace(area) ? "選擇組別" : area, 
                    text = "選擇組別" 
                },
                new LineAction 
                { 
                    type = "message", 
                    label = string.IsNullOrWhiteSpace(type) ? "選擇類型" : type, 
                    text = "選擇類型" 
                }

            };
            if (!string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(type))
            {
                actions.Add(new LineAction
                {
                    type = "message",
                    label = $"送出查詢: {area}/{type}",
                    text = $"確認 {area}/{type}"
                });
            }

            var payload = new LineRequest
            {
                replyToken = replyToken,
                messages = new List<LineMessage>
                {
                    new LineMessage
                    {
                        type = "template",
                        altText = "篩選感測器",
                        template = new ButtonsTemplate
                        {
                            title = "篩選感測器",
                            text = "請選擇組別與感測器類型",
                            actions = actions
                        }
                    }
                }
            };
            return SendLineRequestAsync("https://api.line.me/v2/bot/message/reply", payload);
        }

        public Task ReplyDatePickerAsync(string replyToken, string? start = null, string? end = null)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string twoYearsAgo = DateTime.Now.AddYears(-2).ToString("yyyy-MM-dd");

            var contents = new List<object>();

            // 開始日期按鈕
            contents.Add(new
            {
                type = "button",
                action = new DateTimePickerAction
                {
                    type = "datetimepicker",
                    label = string.IsNullOrEmpty(start) ? "選擇開始日期" : $"開始日期：{start}",
                    data = "action=startDate",
                    mode = "date",
                    initial = string.IsNullOrEmpty(start) ? today : start,
                    min = twoYearsAgo,
                    max = today
                },
                style = "primary",
                margin = "sm"
            });

            // 結束日期按鈕
            contents.Add(new
            {
                type = "button",
                action = new DateTimePickerAction
                {
                    type = "datetimepicker",
                    label = string.IsNullOrEmpty(end) ? "選擇結束日期" : $"結束日期：{end}",
                    data = "action=endDate",
                    mode = "date",
                    initial = string.IsNullOrEmpty(end) ? today : end,
                    min = twoYearsAgo,
                    max = today
                },
                style = "secondary",
                margin = "sm"
            });

            // 如果 start & end 都有值，加確認按鈕
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                contents.Add(new
                {
                    type = "button",
                    action = new MessageAction
                    {
                        label = "送出查詢",
                        text = "送出查詢"
                    },
                    style = "primary",
                    margin = "md"
                });
            }

            var flexBody = new FlexBody { contents = contents };
            var bubble = new FlexBubble { body = flexBody };

            var message = new LineMessage
            {
                type = "flex",
                altText = "請選擇日期",
                contents = bubble
            };

            var payload = new LineRequest
            {
                replyToken = replyToken,
                messages = new List<LineMessage> { message }
            };

            return SendLineRequestAsync("https://api.line.me/v2/bot/message/reply", payload);
        }

        // 自訂 Flex / DatePicker 等訊息
        public Task ReplyCustomAsync(string replyToken, List<LineMessage> messages)
        {
            var payload = new LineRequest
            {
                replyToken = replyToken,
                messages = messages
            };
            return SendLineRequestAsync("https://api.line.me/v2/bot/message/reply", payload);
        }

        public async Task ReplyCustomJsonAsync(string replyToken,string customjson)
        {
            var requestUrl = "https://api.line.me/v2/bot/message/reply";

            var json = customjson;
            // Console.WriteLine(json);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                Console.WriteLine("✅ 選單回覆成功");
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ 回覆失敗: {response.StatusCode}\n{errorMsg}");
            }
        }

    }
}

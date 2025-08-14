using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Notify.Services
{
    public class NotifyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _channelAccessToken;

        public NotifyService(string channelAccessToken)
        {
            _httpClient = new HttpClient();
            _channelAccessToken = channelAccessToken;
        }

        public async Task PushMessageToGroupAsync(string groupId, string messageText)
        {
            var requestUrl = "https://api.line.me/v2/bot/message/push";

            var json = $@"
            {{
                ""to"": ""{groupId}"",
                ""messages"": [
                    {{
                        ""type"": ""text"",
                        ""text"": ""{messageText}""
                    }}
                ]
            }}";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ 推送成功");
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ 推送失敗: {response.StatusCode} \n{errorMsg}");
            }
        }

        public async Task PushMessageWithImageToGroupAsync(string groupId, string messageText, string imageUrl, string? previewImageUrl = null)
        {
            var requestUrl = "https://api.line.me/v2/bot/message/push";
            var finalPreviewUrl = String.IsNullOrEmpty(previewImageUrl) ? imageUrl : previewImageUrl;
            // 如果沒有指定 previewImageUrl，則用 imageUrl 當預覽圖
            if (string.IsNullOrEmpty(previewImageUrl))
            {
                previewImageUrl = imageUrl;
            }

            var json = $@"
            {{
                ""to"": ""{groupId}"",
                ""messages"": [
                    {{
                        ""type"": ""text"",
                        ""text"": ""{messageText}""
                    }},
                    {{
                        ""type"": ""image"",
                        ""originalContentUrl"": ""{imageUrl}"",
                        ""previewImageUrl"": ""{finalPreviewUrl}""
                    }}
                ]
            }}";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ 推送成功");
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ 推送失敗: {response.StatusCode} \n{errorMsg}");
            }
        }

        public async Task ReplyMessageAsync(string replyToken, string messageText)
        {
            var requestUrl = "https://api.line.me/v2/bot/message/reply";

            var json = $@"
            {{
                ""replyToken"": ""{replyToken}"",
                ""messages"": [
                    {{
                        ""type"": ""text"",
                        ""text"": ""{messageText}""
                    }}
                ]
            }}";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ 回應訊息成功");
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ 回應訊息失敗: {response.StatusCode} \n{errorMsg}");
            }
        }

        public async Task ReplyMessageWithImageAsync(string replyToken, string messageText, string originalImageUrl, string? previewImageUrl = null)
        {
            var requestUrl = "https://api.line.me/v2/bot/message/reply";
            var finalPreviewUrl = String.IsNullOrEmpty(previewImageUrl) ? originalImageUrl : previewImageUrl;
            var json = $@"
            {{
                ""replyToken"": ""{replyToken}"",
                ""messages"": [
                    {{
                        ""type"": ""text"",
                        ""text"": ""{messageText}""
                    }},
                    {{
                        ""type"": ""image"",
                        ""originalContentUrl"": ""{originalImageUrl}"",
                        ""previewImageUrl"": ""{finalPreviewUrl}""
                    }}
                ]
            }}";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ 回應訊息成功");
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ 回應訊息失敗: {response.StatusCode} \n{errorMsg}");
            }
        }

        public async Task ReplyMenuAsync(string replyToken,string? area = null ,string? type = null)
        {
            var requestUrl = "https://api.line.me/v2/bot/message/reply";
            
            var actionsBuilder = new StringBuilder();

            actionsBuilder.AppendLine($@"
                {{
                    ""type"": ""message"",
                    ""label"": ""{(string.IsNullOrEmpty(area) ? "選擇組別" : area)}"",
                    ""text"": ""選擇組別""
                }},
                {{
                    ""type"": ""message"",
                    ""label"": ""{(string.IsNullOrEmpty(type) ? "選擇類型" : type)}"",
                    ""text"": ""選擇類型""
                }}");

            if (!string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(type))
            {
                actionsBuilder.AppendLine($@",
                {{
                    ""type"": ""message"",
                    ""label"": ""送出查詢: {area} / {type}"",
                    ""text"": ""確認 {area}/{type}""
                }}");
            }

            // 建立完整 JSON
            var json = $@"
            {{
                ""replyToken"": ""{replyToken}"",
                ""messages"": [
                    {{
                        ""type"": ""template"",
                        ""altText"": ""篩選感測器"",
                        ""template"": {{
                            ""type"": ""buttons"",
                            ""title"": ""篩選感測器"",
                            ""text"": ""請選擇組別與感測器類型"",
                            ""actions"": [
                                {actionsBuilder.ToString()}
                            ]
                        }}
                    }}
                ]
            }}";


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

        public async Task ReplyDatePickerAsync(string replyToken, string? start = null, string? end = null)
        {
            var requestUrl = "https://api.line.me/v2/bot/message/reply";

            // 計算日期範圍
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string twoYearsAgo = DateTime.Now.AddYears(-2).ToString("yyyy-MM-dd");

            // **日期選擇按鈕 Builder**
            var contentBuilder = new StringBuilder();

            // 標題文字
            contentBuilder.AppendLine($@"
                {{
                    ""type"": ""text"",
                    ""text"": ""請選擇日期範圍"",
                    ""weight"": ""bold"",
                    ""size"": ""lg"",
                    ""margin"": ""md""
                }},");

            // 開始日期按鈕
            contentBuilder.AppendLine($@"
                {{
                    ""type"": ""button"",
                    ""action"": {{
                        ""type"": ""datetimepicker"",
                        ""label"": ""{(string.IsNullOrEmpty(start) ? "選擇開始日期" : $"開始日期：{start}")}"",
                        ""data"": ""action=startDate"",
                        ""mode"": ""date"",
                        ""initial"": ""{(string.IsNullOrEmpty(start) ? today : start)}"",
                        ""max"": ""{today}"",
                        ""min"": ""{twoYearsAgo}""
                    }},
                    ""style"": ""primary"",
                    ""margin"": ""sm""
                }},");

            // 結束日期按鈕
            contentBuilder.AppendLine($@"
                {{
                    ""type"": ""button"",
                    ""action"": {{
                        ""type"": ""datetimepicker"",
                        ""label"": ""{(string.IsNullOrEmpty(end) ? "選擇結束日期" : $"結束日期：{end}")}"",
                        ""data"": ""action=endDate"",
                        ""mode"": ""date"",
                        ""initial"": ""{(string.IsNullOrEmpty(end) ? today : end)}"",
                        ""max"": ""{today}"",
                        ""min"": ""{twoYearsAgo}""
                    }},
                    ""style"": ""secondary"",
                    ""margin"": ""sm""
                }}");

            // **如果 start & end 都有值，加確認按鈕**
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                contentBuilder.AppendLine($@",
                {{
                    ""type"": ""button"",
                    ""action"": {{
                        ""type"": ""message"",
                        ""label"": ""送出查詢"",
                        ""text"": ""送出查詢""
                    }},
                    ""style"": ""primary"",
                    ""margin"": ""md""
                }}");
            }

            // **完整 JSON**
            var json = $@"
            {{
                ""replyToken"": ""{replyToken}"",
                ""messages"": [
                    {{
                        ""type"": ""flex"",
                        ""altText"": ""請選擇日期"",
                        ""contents"": {{
                            ""type"": ""bubble"",
                            ""body"": {{
                                ""type"": ""box"",
                                ""layout"": ""vertical"",
                                ""contents"": [
                                    {contentBuilder.ToString()}
                                ]
                            }}
                        }}
                    }}
                ]
            }}";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                Console.WriteLine("✅ 日期選擇回覆成功");
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ 回覆失敗: {response.StatusCode}\n{errorMsg}");
            }
        }

    }
}

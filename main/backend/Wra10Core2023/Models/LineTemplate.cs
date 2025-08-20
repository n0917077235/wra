namespace Wra10Core2023.Models
{
    public class LineMessage
    {
        public string type { get; set; } = "text";
        public string text { get; set; }
        public string originalContentUrl { get; set; } // image type
        public string previewImageUrl { get; set; }    // image type
        public object template { get; set; }           // template type
        public object contents { get; set; }           // flex type
        public string altText { get; set; }            // flex type
    }

    public class LineRequest
    {
        public string to { get; set; }             // push 專用
        public string replyToken { get; set; }     // reply 專用
        public List<LineMessage> messages { get; set; } = new();
    }

    public class LineAction
    {
        public string type { get; set; }   // message, postback, datetimepicker
        public string label { get; set; }
        public string text { get; set; }          // message type
        public string data { get; set; }          // postback / datetimepicker
        public string mode { get; set; }          // datetimepicker
        public string initial { get; set; }       // datetimepicker
        public string max { get; set; }           // datetimepicker
        public string min { get; set; }           // datetimepicker
    }

    public class DateTimePickerAction
    {
        public string type { get; set; } = "datetimepicker";
        public string label { get; set; } = null!;
        public string data { get; set; } = null!;
        public string mode { get; set; } = "date";
        public string? initial { get; set; }
        public string? min { get; set; }
        public string? max { get; set; }
    }

    public class MessageAction
    {
        public string type { get; set; } = "message";
        public string label { get; set; } = null!;
        public string text { get; set; } = null!;
    }


    public class ButtonsTemplate
    {
        public string type { get; set; } = "buttons";
        public string title { get; set; }
        public string text { get; set; }
        public List<LineAction> actions { get; set; } = new();
    }

    public class FlexBubble
    {
        public string type { get; set; } = "bubble";
        public FlexBody body { get; set; }
    }

    public class FlexBody
    {
        public string type { get; set; } = "box";
        public string layout { get; set; } = "vertical";
        public List<object> contents { get; set; } = new();
    }

    public class FlexMessage
    {
        public string type { get; set; } = "flex";
        public string altText { get; set; } = "查詢資料選單";
        public object contents { get; set; } // carousel
    }

    public class Carousel
    {
        public string type { get; set; } = "carousel";
        public List<object> contents { get; set; } = new();
    }

}
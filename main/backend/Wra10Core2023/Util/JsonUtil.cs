using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Wra10Core2023.Util;

public static class JsonUtil
{
    public static bool GetBool(this JToken o, string key) => (bool)o[key];
    public static int GetInt(this JToken o, string key) => (int)o[key];
    public static double GetDouble(this JToken o, string key) => (double)o[key];
    public static string GetStr(this JToken o, string key) => (string)o[key];
}

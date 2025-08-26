using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Data;
using Microsoft.Data.SqlClient;

using SqlHelper = Wra10Core2023.Util.SQLHelper;

using Wra10Core2023.Models;
using EarthquakeInfoApi.Models;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Wra10Core2023.Util;

public static class IsoseismalUtil
{
    public static async Task Loop()
    {
        while (true)
        {
            await GenerateAsync();
            var minute = 60_000;
            await Task.Delay(minute);
        }
    }

    private static async Task GenerateAsync()
    {

    }
}
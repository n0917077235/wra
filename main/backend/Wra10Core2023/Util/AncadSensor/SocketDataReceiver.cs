using System.Security.Cryptography;
using System.Text;

namespace Wra10Core2023.Util.AncadSensor;

/*
  SecretKey is in appsettings.json

  Protocol for AnSmartWayPlus

    ticks = unsigned long
    \r\n = CRLF 換行碼

    1. 參數同步封包

    1.1 設備發送參數同步請求  
    SRC
    $SYNC=id,ticks
      id = 介面卡號
      ticks = MCU ticks
    SHA
    SHA256(SRC, SecretKey)
    OUT
    SRC,SHA\r\n
    EX:
    $SYNC=36126878,0000575E,7FFE1DB542DA1B87A933E4ED50592C7747B14A795673851984CB25BCFC6DE163

    1.2 伺服器回傳參數到設備
    SRC
    $SYNC=id,cmd,DONE
      id = 介面卡號
      cmd = 網頁上提供的參數更新字串
    SHA256(SRC, SecretKey)
    OUT
    SRC,SHA\r\n
    EX:
    $SYNC=36126878,22/07/22-12:00:00,MODE=[8],RATE=[5,5],DONE,ECE830127B690CDC70F143933A5B1CF949623364170056BFD9A926F3B9F65774

    1.3 設備發送參數同步完成
    SRC
    $UPDATE=id,ticks
      id = 介面卡號
      ticks = MCU ticks
    SHA256(SRC, SecretKey)
    OUT
    SRC,SHA\r\n
    EX:
    $UPDATE=36126878,000057C4,B5F947FB5EE5716C7DC96E2C328A0DCA251A60DAFA9808FD4AAB1EDB66228984



    2. 上傳資料封包格式

    2.1 設備發送資料到伺服器
    SRC
    $ANAN=id,ticks,yy/MM/dd-HH:mm:ss,data...............
      id = 介面卡號
      ticks = MCU ticks
      yy/MM/dd-HH:mm:ss = 時間戳
      data = 資料 IEEE-754 floating point
    SHA
    SHA256(SRC, SecretKey)
    OUT
    SRC,SHA\r\n
    EX:
    $ANAN=1057198,0000C1D2,22/07/06-10:45:00,413E8889,00000000,C2680000,41409BA6,00000000,00000000,00000000,C61C3C00,C61C3C00,C61C3C00,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,00000000,D5729D2394200DC885E1EBF1C44F9A72D0A1BFFE524F2E91281122C619F250CC

    2.2 伺服器發送接收完成到設備
    SRC
    $ANAN=id,DONE
      id = 介面卡號
    SHA256(SRC, SecretKey)
    OUT
    SRC,SHA\r\n
    EX:
    $ANAN=36126878,DONE,C9172E574B89C69A751A34086C06F293883478A0B0EA9C3086782915C70A29BE



    3. 資料列 data......

    為了固定設備韌體版本，設備目前支援4組類比資料和8組數位資料，目前基本欄位如下

    BAT0,BAT1,RSSI,AIN0,AIN1,AIN2,AIN3,DIG0,DIG1,DIG2,...,DIG15,[USER...]
    所有欄位以 IEEE-754 floating point 表示

    平台依據設備類別解讀對應欄位資料

    EX:
    流量站
    DIG0=正流量
    DIG1=負流量
    DIG0=累積流量

    水位站
    AIN0~AIN3=水位計0~3

    開度計站
    AIN0~AIN3=開度計0~3

    流量 + 水位 站
    AIN0~AIN3=水位計0~3
    DIG0=正流量
    DIG1=負流量
    DIG0=累積流量

    [USER....]
    預留DEBUG使用，無須解讀，保留LOG即可

 */
public class SocketDataReceiver
{
    private enum MessageType
    {
        AnAn,
        Sync
    }

    private const string AnAnPrefix = "$ANAN";
    private const string SyncPrefix = "$SYNC";
    private const string UpdatedPrefix = "$UPDATE";
    public const string TimeFormat = "yy/MM/dd-HH:mm:ss";
    private readonly Stream stream;
    private DateTime Time;
    private string InterfaceName;
    private MessageType Type;

    /// <summary>
    /// This should never throw an exception
    /// </summary>
    public Func<InterfaceUpdateInfo, Task> OnDataUpdateAsync = _ => Task.CompletedTask;

    public SocketDataReceiver(Stream stream)
    {
        this.stream = stream;
    }

    public async Task StartAsync()
    {
        while (true)
        {
            await AuthenticateAsync();

            if (Type == MessageType.Sync)
            {
                var time = DateTime.Now.ToString(TimeFormat) + ",";
                SendCommand(SyncPrefix, $"{time},DONE");
                ReceiveUpdatedMessage();
            }

            Thread.Sleep(20);
        }
    }

    private string ReadLine()
    {
        var buf = new byte[1000];
        var offset = 0;

        while (true)
        {
            var count = stream.Read(buf, offset, buf.Length - offset);
            offset += count;
            if (offset >= 2 && buf[offset - 2] == '\r' && buf[offset - 1] == '\n') break;

            // End of stream reached
            if (count == 0) throw new EOLException("End of stream. No new line detected");

            if (offset >= buf.Length)
            {
                // Can be an attack causing memory leak
                throw new OverflowException("overflow: line is too long");
            }

            Thread.Sleep(20);
        }

        var s = Encoding.ASCII.GetString(buf, 0, offset - 2);
        return s;
    }

    private void CheckHash(string[] words)
    {
        var hash = words[words.Length - 1];
        var expected = ComputeHash(string.Join(",", words.Take(words.Length - 1)));
        if (hash != expected) throw new Exception("Hash does not match");
    }

    private async Task AuthenticateAsync()
    {
        var s = ReadLine();
        var words = s.Split([',']);
        var pre = words[0];
        InterfaceName = pre.Substring(pre.IndexOf('=') + 1);
        CheckHash(words);
        Console.WriteLine($"[ANCAD SENSOR] receiving from socket: {s}");

        if (pre.StartsWith(SyncPrefix))
        {
            Type = MessageType.Sync;
            return;
        }

        Time = DateUtil.ParseFormat(words[2], TimeFormat);

        if (pre.StartsWith(AnAnPrefix))
        {
            Type = MessageType.AnAn;
            var msg = new AnSmartMessage(words, Time);
            await OnDataUpdateAsync(new(InterfaceName, msg));
            SendCommand(AnAnPrefix, "DONE");
        }
        else
        {
            throw new Exception($"Unrecognized prefix. Data: {pre}");
        }
    }

    private void SendCommand(string prefix, string cmd)
    {
        var m = $"{prefix}={InterfaceName},{cmd}";
        var all = $"{m},{ComputeHash(m)}\r\n";
        var b = Encoding.UTF8.GetBytes(all);
        stream.Write(b, 0, b.Length);
        stream.Flush();
    }

    private void ReceiveUpdatedMessage()
    {
        var s = ReadLine();
        var words = s.Split([',']);
        var pre = words[0];
        if (!pre.StartsWith(UpdatedPrefix)) throw new Exception();
        CheckHash(words);
    }

    public static string ComputeHash(string message)
    {
        var b = Convert.FromBase64String(SiteUtil.AncadSensorKey);
        using var hmacSHA256 = new HMACSHA256(b);
        var hash = hmacSHA256.ComputeHash(Encoding.UTF8.GetBytes(message));
        return BitConverter.ToString(hash).Replace("-", "");
    }
}


using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using NodaTime;

namespace MemoryUsage.Common;

public static class Utils
{
    public static string NormalizePath(string path)
    {
        return Path.GetFullPath(new Uri(path).LocalPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }

    public static string GetRelativeTime(DateTime ago)
    {
        LocalDateTime start = LocalDateTime.FromDateTime(ago);
        LocalDateTime end = LocalDateTime.FromDateTime(DateTime.Now);
        Period period = Period.Between(start, end);
        if (period.Years > 0)
        {
            return $"{period.Years} 年前";
        }

        if (period.Months > 0)
        {
            return $"{period.Months} 个月前";
        }

        if (period.Days > 0)
        {
            return $"{period.Days} 天前";
        }

        if (period.Hours > 0)
        {
            return $"{period.Hours} 小时前";
        }

        if (period.Minutes > 0)
        {
            return $"{period.Minutes} 分钟前";
        }

        return $"{period.Seconds} 秒前";
    }

    public static bool IsSQLiteDatabase(string filename)
    {
        bool result = false;

        if (File.Exists(filename))
        {
            using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] header = new byte[16];

            for (int i = 0; i < 16; i++)
            {
                header[i] = (byte)stream.ReadByte();
            }

            result = Encoding.UTF8.GetString(header).Contains("SQLite format 3");

            stream.Close();
        }

        return result;
    }

    public static TimeSpan ParseSpan(string spanStr)
    {
        var ts = TimeSpan.FromDays(365 * 100);
        var regex = @"\d*\.?\d*";
        var ms = Regex.Matches(spanStr, regex, RegexOptions.None);
        var value = ms[0].Value;
        if (string.IsNullOrEmpty(value))
        {
            return ts;
        }

        string unit = spanStr.Replace(value, "").Trim();
        if (string.IsNullOrEmpty(unit))
        {
            return ts;
        }

        var r = double.TryParse(value, out double time);
        if (r)
        {
            switch (unit)
            {
                case "m":
                case "min":
                case "mins":
                case "分钟":
                    ts = TimeSpan.FromMinutes(time);
                    break;

                case "h":
                case "hour":
                case "hours":
                case "小时":
                    ts = TimeSpan.FromHours(time);
                    break;

                case "d":
                case "day":
                case "days":
                case "天":
                    ts = TimeSpan.FromDays(time);
                    break;

                case "M":
                case "Mon":
                case "Month":
                case "Months":
                case "个月":
                    ts = TimeSpan.FromDays(time * 31);
                    break;

                case "y":
                case "year":
                case "years":
                case "年":
                    ts = TimeSpan.FromDays(time * 365);
                    break;

                default:
                    return ts;
            }
        }

        return ts;
    }

    public static List<string> GetLocalIps()
    {
        var ips = new List<string>(10);
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            var addr = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
            if (addr != null)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ips.Add(ip.Address.ToString());
                        }
                    }
                }
            }
        }

        return ips;
    }

    public static string GetMd5Hash(string file)
    {
        string hash;
        try
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToUpper();
                }
            }
        }
        catch (IOException)
        {
            // 数据库模板文件占用中，每次都返回不一样的md5，即使相同重新发一次也无妨
            hash = file + "_" + DateTime.Now.Millisecond;
        }

        return hash;
    }
}
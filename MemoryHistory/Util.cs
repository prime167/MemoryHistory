using System;
using System.Linq;
using System.Management;

namespace MemoryHistory;

public class Util
{
    public static string GetMemoryUsage()
    {
        var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

        var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
        {
            FreePhysicalMemory = double.Parse(mo["FreePhysicalMemory"].ToString()),
            TotalVisibleMemorySize = double.Parse(mo["TotalVisibleMemorySize"].ToString())
        }).FirstOrDefault();

        if (memoryValues != null)
        {
            var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
            percent = Math.Round(percent, 2);
            return percent.ToString("00.00") +"%";
        }

        return "0%";
    }
}
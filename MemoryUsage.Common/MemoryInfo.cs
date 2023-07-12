namespace MemoryUsage.Common;

public class MemoryInfo
{
    public double FreePhysicalMemory { get; set; } = 1;

    public double TotalVisibleMemorySize { get; set; } = 2;

    public double FreeSpaceInPagingFiles { get; set; } = 3;

    public double TotalVirtualMemorySize { get; set; } = 4;

    public double FreeVirtualMemory { get; set; } = 5;
}
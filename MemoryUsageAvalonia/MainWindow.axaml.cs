using System;
using System.Threading;
using Avalonia.Controls;
using ScottPlot.Plottable;
using ScottPlot;
using Avalonia.Threading;
using System.Diagnostics;
using System.Management;
using System.Linq;
using Avalonia;
using MemoryUsage.Common;

namespace MemoryUsageAvalonia;

public partial class MainWindow : Window
{
    private Timer _timer;

    /// <summary>
    /// 显示的时间范围
    /// </summary>
    private const int MaxPeriod = 60 * 30; // s

    private double _pageFileSize;
    private static readonly object Locker = new();
    private const int DataCount = 10; // 移动平均最近点数;

    private ExponentialMovingAverageIndicator _ema;

    private DataStreamer _streamerUsed;
    private DataStreamer _streamerCommit;
    private double _commitPctTitle;

    public MemoryViewModel Vm = new();
    private Plot _pltUsed;
    private Plot _pltCommit;

    public MainWindow()
    {
        InitializeComponent();
        Vm.MinUsedPct = 100;
        Vm.MaxUsedPct = 0;
        Vm.MinCommitPct = 100;
        Vm.MaxCommitPct = 0;
        DataContext = Vm;
    }

    private void Main_OnOpened(object sender, EventArgs e)
    {
        double height = Screens.Primary.Bounds.Height;
        double width = Screens.Primary.Bounds.Width;
        double h1 = Screens.Primary.WorkingArea.Height;
        var main = this.GetControl<Window>("Main");
        main.Position = new PixelPoint((int)((width - main.Width) / 2), (int)((h1 - main.Height) - 32));

        _pageFileSize = Math.Round(GetPageFileSize() / 1024.0, 2);
        _pltUsed = ApUsed.Plot;
        _pltCommit = ApCommit.Plot;

        _ema = new ExponentialMovingAverageIndicator(DataCount);
        ResetCharts();

        _timer = new Timer(GetMemory, null, 0, 1000);
    }

    private void ResetCharts()
    {
        _pltUsed.Clear();
        _pltCommit.Clear();

        ApUsed.Configuration.DoubleClickBenchmark = false;
        ApCommit.Configuration.DoubleClickBenchmark = false;

        _pltUsed.XAxis.MinimumTickSpacing(1);
        _pltUsed.YAxis.MinimumTickSpacing(5);
        _pltUsed.Grid();
        _pltUsed.XLabel("时间 (s)");
        _pltUsed.Title("内存使用 %");

        _pltCommit.XAxis.MinimumTickSpacing(1);
        _pltCommit.YAxis.MinimumTickSpacing(5);
        _pltCommit.XLabel("时间 (s)");
        _pltCommit.Title("虚拟内存 %");

        _streamerUsed = _pltUsed.AddDataStreamer(MaxPeriod);
        _streamerUsed.ViewScrollLeft();

        _streamerCommit = _pltCommit.AddDataStreamer(MaxPeriod);
        _streamerCommit.ViewScrollLeft();

        //_pltUsed.SetAxisLimits(0, MaxPeriod, 0, 100, 1);
        //_pltUsed.YAxis2.SetZoomOutLimit(100);
        //_pltUsed.YAxis2.SetZoomInLimit(0);
        _pltUsed.YAxis2.SetBoundary(0, 100);
        _pltUsed.YAxis2.SetInnerBoundary(0, 100);
        ApUsed.Configuration.Pan = false;
        ApUsed.Configuration.Zoom = false;
        ApUsed.Configuration.ScrollWheelZoom = false;
        ApUsed.Configuration.MiddleClickDragZoom = false;

        //_pltCommit.SetAxisLimits(0, MaxPeriod, 0, 100, 1);
        //_pltCommit.YAxis2.SetZoomOutLimit(100);
        //_pltCommit.YAxis2.SetZoomInLimit(0);
        _pltCommit.YAxis2.SetBoundary(0, 100);
        _pltCommit.YAxis2.SetInnerBoundary(0, 100);
        ApCommit.Configuration.Pan = false;
        ApCommit.Configuration.Zoom = false;
        ApCommit.Configuration.ScrollWheelZoom = false;
        ApCommit.Configuration.MiddleClickDragZoom = false;

        // 右侧显示Y轴
        _streamerUsed.YAxisIndex = _pltUsed.RightAxis.AxisIndex;
        _pltUsed.RightAxis.Ticks(true);
        _pltUsed.LeftAxis.Ticks(false);
        _pltUsed.RightAxis.Label("使用中 (%)");
        _pltUsed.YLabel("使用中 (%)");

        // 右侧显示Y轴
        _streamerCommit.YAxisIndex = _pltCommit.RightAxis.AxisIndex;
        _pltCommit.RightAxis.Ticks(true);
        _pltCommit.LeftAxis.Ticks(false);
        _pltCommit.RightAxis.Label("已提交 (%)");

        ApUsed.Refresh();
        ApCommit.Refresh();
    }

    public void GetMemory(object state)
    {
        double p1;
        double p2;
        double virtualUsed;
        MemoryInfo mi;
        _pageFileSize = Math.Round(GetPageFileSize() / 1024.0, 2);

        lock (Locker)
        {
            mi = GetSystemMemoryUsagePercentage();
            p1 = ((mi.TotalVisibleMemorySize - mi.FreePhysicalMemory) / mi.TotalVisibleMemorySize) * 100;
            p1 = Math.Round(p1, 1);

            virtualUsed = mi.TotalVirtualMemorySize - mi.FreeVirtualMemory;
            p2 = (virtualUsed / mi.TotalVirtualMemorySize) * 100;
            p2 = Math.Round(p2, 1);

            _ema.AddDataPoint(p2);
        }

        Vm.CurrentUsedPct = p1;

        if (p1 < Vm.MinUsedPct)
        {
            Vm.MinUsedPct = p1;
            Vm.MinUsedPctTime = DateTime.Now;
        }

        if (p1 > Vm.MaxUsedPct)
        {
            Vm.MaxUsedPct = p1;
            Vm.MaxUsedPctTime = DateTime.Now;
        }

        if (p2 < Vm.MinCommitPct)
        {
            Vm.MinCommitPct = p2;
            Vm.MinCommitPctTime = DateTime.Now;
        }

        if (p2 > Vm.MaxCommitPct)
        {
            Vm.MaxCommitPct = p2;
            Vm.MaxCommitPctTime = DateTime.Now;
        }

        if (Math.Abs(p2 - Vm.CurrentCommitPct) > 0.001)
        {
            Vm.CurrentCommitPct = p2;
        }

        Vm.MaxUsedPctTimeStr = Utils.GetRelativeTime(Vm.MaxUsedPctTime);
        Vm.MinUsedPctTimeStr = Utils.GetRelativeTime(Vm.MinUsedPctTime);
        Vm.MaxCommitPctTimeStr = Utils.GetRelativeTime(Vm.MaxCommitPctTime);
        Vm.MinCommitPctTimeStr = Utils.GetRelativeTime(Vm.MinCommitPctTime);
        var pageFileUsed = Math.Round(_pageFileSize - mi.FreeSpaceInPagingFiles, 2).ToString("0.00");
        var totalPageFile = _pageFileSize.ToString("0.00");
        Vm.CommitDetailStr =
            $"{Math.Round(virtualUsed, 2):0.00} GB / {mi.TotalVirtualMemorySize:0.00} GB, 空闲：{mi.FreeVirtualMemory:0.00} GB";
        Vm.PageFileDetailStr = $"页面文件: {pageFileUsed} GB / {totalPageFile} GB";
        try
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Math.Abs(_commitPctTitle - p2) >= 1)
                {
                    _commitPctTitle = p2;
                    Title = Math.Round(_commitPctTitle, 0) + "%";
                }

                _streamerUsed.Add(p1);
                if (_streamerUsed.Count != _streamerUsed.CountTotalOnLastRender)
                {
                    ApUsed.Refresh();
                }

                double vv = _ema.Average;
                _streamerCommit.Add(vv);
                if (_streamerCommit.Count != _streamerCommit.CountTotalOnLastRender)
                {
                    ApCommit.Refresh();
                }
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }

    public static MemoryInfo GetSystemMemoryUsagePercentage()
    {
        const double toGb = 1024.0 * 1024.0;
        var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
        MemoryInfo mi = wmiObject.Get().Cast<ManagementObject>().Select(mo => new MemoryInfo
        {
            FreePhysicalMemory = Math.Round(double.Parse(mo["FreePhysicalMemory"].ToString()) / toGb, 2),
            TotalVisibleMemorySize = Math.Round(double.Parse(mo["TotalVisibleMemorySize"].ToString()) / toGb, 2),
            FreeSpaceInPagingFiles = Math.Round(double.Parse(mo["FreeSpaceInPagingFiles"].ToString()) / toGb, 2),
            TotalVirtualMemorySize = Math.Round(double.Parse(mo["TotalVirtualMemorySize"].ToString()) / toGb, 2),
            FreeVirtualMemory = Math.Round(double.Parse(mo["FreeVirtualMemory"].ToString()) / toGb, 2),
        }).FirstOrDefault();

        return mi;
    }

    public static uint GetPageFileSize()
    {
        uint total = 0;
        using (var query = new ManagementObjectSearcher("SELECT AllocatedBaseSize FROM Win32_PageFileUsage"))
        {
            foreach (ManagementBaseObject obj in query.Get())
            {
                uint used = (uint)obj.GetPropertyValue("AllocatedBaseSize");
                total += used;
            }
        }

        return total;
    }
}
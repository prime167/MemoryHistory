using System;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Windows;
using ScottPlot;
using ScottPlot.Plottable;

namespace MemoryUsage;

public partial class MainWindow : Window
{
    private Timer _timer;

    /// <summary>
    /// 显示的时间范围
    /// </summary>
    private const int MaxPeriod = 60 * 10;

    private double _pageFileSize;
    private static readonly object Locker = new();
    private const int DataCount = 10;// 移动平均最近点数;

    public double[] Percentages = new double[MaxPeriod];
    public double[] Commits = new double[MaxPeriod];
    public double[] CommitsAvg = new double[MaxPeriod];
    public double[] CommitsEma = new double[MaxPeriod];

    private ExponentialMovingAverageIndicator _ema;

    private SignalPlot _plotUsed;
    private SignalPlot _plotCurrentCommit;
    private SignalPlot _plotEma;// ema
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
        double height = SystemParameters.FullPrimaryScreenHeight;
        double width = SystemParameters.FullPrimaryScreenWidth;
        double h1 = SystemParameters.WorkArea.Height;

        Top = (h1 - Height) + 8;
        Left = (width - Width) / 2;
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        _pageFileSize = Math.Round(GetPageFileSize() / 1024.0, 2);
        _pltUsed = WpUsed.Plot;
        _pltCommit = WpCommit.Plot;

        _ema = new ExponentialMovingAverageIndicator(DataCount);
        ResetCharts();

        _plotUsed = _pltUsed.AddSignal(Percentages, color: Color.Green, label: "used");
        _plotCurrentCommit = _pltCommit.AddSignal(Commits, color: Color.Silver, label: "now"); ;
        _plotEma = _pltCommit.AddSignal(CommitsEma, color: Color.Red, label: "ema");
        _pltCommit.Legend(location: Alignment.UpperLeft);

        // 右侧显示Y轴
        _plotUsed.YAxisIndex = _pltUsed.RightAxis.AxisIndex;
        _pltUsed.RightAxis.Ticks(true);
        _pltUsed.LeftAxis.Ticks(false);
        _pltUsed.RightAxis.Label("使用中 (%)");
        //_pltUsed.YLabel("使用中 (%)");

        // 右侧显示Y轴
        _plotCurrentCommit.YAxisIndex = _pltCommit.RightAxis.AxisIndex;
        _plotEma.YAxisIndex = _pltCommit.RightAxis.AxisIndex;
        _pltCommit.RightAxis.Ticks(true);
        _pltCommit.LeftAxis.Ticks(false);
        _pltCommit.RightAxis.Label("已提交 (%)");
        //_pltCommit.YLabel("已提交 (%)");

        _plotUsed.MarkerSize = 1;
        _plotCurrentCommit.MarkerSize = 1;
        _plotEma.MarkerSize = 1;

        // x轴坐标逆序 600-0
        static string CustomTickFormatter(double position)
        {
            return $"{600 - position}";
        }

        _pltUsed.XAxis.TickLabelFormat(CustomTickFormatter);
        _pltCommit.XAxis.TickLabelFormat(CustomTickFormatter);

        _timer = new Timer(GetMemory, null, 0, 1000);
    }

    private void ResetCharts()
    {
        _pltUsed.Clear();
        _pltCommit.Clear();

        WpUsed.Configuration.DoubleClickBenchmark = false;
        WpCommit.Configuration.DoubleClickBenchmark = false;

        WpUsed.MouseDoubleClick += WpUsed_MouseDoubleClick;
        WpCommit.MouseDoubleClick += WpCommit_MouseDoubleClick;

        _pltUsed.XAxis.MinimumTickSpacing(1);
        _pltUsed.YAxis.MinimumTickSpacing(5);
        _pltUsed.YAxis.SetBoundary(0, 100);
        _pltUsed.Grid();
        _pltUsed.XLabel("时间 (s)");
        _pltUsed.Title("内存使用 %");

        _pltCommit.XAxis.MinimumTickSpacing(1);
        _pltCommit.YAxis.MinimumTickSpacing(5);
        _pltCommit.YAxis.SetBoundary(0, 100);
        _pltCommit.XLabel("时间 (s)");
        _pltCommit.Title("虚拟内存 %");

        WpUsed.Refresh();
        WpCommit.Refresh();
    }

    /// <summary>
    /// 数组元素左移 [1,2,3] => [2,3,null] 然后加入新元素
    /// </summary>
    /// <param name="array"></param>
    /// <param name="last"></param>
    private static void UpdateArray(double[] array, double last)
    {
        int len = array.Length;
        Array.Copy(array, 1, array, 0, len - 1);
        array[^1] = last;
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

            UpdateArray(Percentages, p1);
            UpdateArray(Commits, p2);

            _ema.AddDataPoint(p2);
            double vv = _ema.Average;
            UpdateArray(CommitsEma, vv);
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
        Vm.CommitDetailStr = $"{Math.Round(virtualUsed, 2):0.00} GB / {mi.TotalVirtualMemorySize:0.00} GB, 空闲：{mi.FreeVirtualMemory:0.00} GB";
        Vm.PageFileDetailStr = $"页面文件: {pageFileUsed} GB / {totalPageFile} GB";
        try
        {
            Dispatcher.Invoke(() =>
            {
                if (Math.Abs(_commitPctTitle - p2) >= 1)
                {
                    _commitPctTitle = p2;
                    Title = Math.Round(_commitPctTitle, 0) + "%";
                }

                WpUsed.Render();
                WpCommit.Render();
                _pltUsed.AxisAuto();
                _pltCommit.AxisAuto();
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }

    private void WpUsed_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        WpUsed.Plot.AxisAuto();
    }

    private void WpCommit_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        WpCommit.Plot.AxisAuto();
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
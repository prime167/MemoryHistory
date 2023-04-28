using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Windows;
using ScottPlot;
using ScottPlot.MinMaxSearchStrategies;
using ScottPlot.Plottable;

namespace MemoryUsage;

public partial class MainWindow : Window
{
    private Timer _timer;

    /// <summary>
    /// 显示的时间范围
    /// </summary>
    private const int MaxPeriod = 60 * 10;

    private double _pageFileSize = 0.0;
    private static object _locker = new object();
    private const int k = 30;// 移动平均最近点数;
    public List<double> Times = new();

    public double[] Percentages = new double[MaxPeriod];
    public double[] Commits = new double[MaxPeriod];
    public double[] CommitsAvg = new double[MaxPeriod];
    public double[] CommitsEma = new double[MaxPeriod];

    private ExponentialMovingAverageIndicator _ema;

    private SignalPlot _plotUsed;
    private SignalPlot _plotCurrentCommit;
    private SignalPlot _plotAvg;
    private SignalPlot _plotEma;// ema
    private double _ppTitle = 0.0;

    public MemoryViewModel Vm = new();
    private Plot _plt1;
    private Plot _plt2;

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
        _plt1 = WpUsed.Plot;
        _plt2 = WpCommit.Plot;

        _ema = new ExponentialMovingAverageIndicator(k);
        ResetCharts();

        _plotUsed = _plt1.AddSignal(Percentages, 1, color: Color.Green, label: "used");
        _plotCurrentCommit = _plt2.AddSignal(Commits, 1, color: Color.Green, label: "now"); ;
        _plotAvg = _plt2.AddSignal(CommitsAvg, 1, color: Color.Blue, label: "avg");
        _plotEma = _plt2.AddSignal(CommitsEma, 1, color: Color.Red, label: "ema");
        _plt2.Legend(location: Alignment.UpperLeft);

        _plotUsed.MarkerSize = 1;
        _plotCurrentCommit.MarkerSize = 1;
        _plotAvg.MarkerSize = 1;
        _plotEma.MarkerSize = 1;

        // 反转 x轴坐标 600-0
        static string customTickFormatter(double position)
        {
            return $"{600 - position}";
        }

        _plt1.XAxis.TickLabelFormat(customTickFormatter);
        _plt2.XAxis.TickLabelFormat(customTickFormatter);

        _timer = new Timer(GetMemory, null, 0, 1000);
    }

    private void ResetCharts()
    {
        _plt1.Clear();
        _plt2.Clear();

        WpUsed.Configuration.DoubleClickBenchmark = false;
        WpCommit.Configuration.DoubleClickBenchmark = false;

        WpUsed.MouseDoubleClick += WpUsed_MouseDoubleClick;
        WpCommit.MouseDoubleClick += WpCommit_MouseDoubleClick;

        _plt1.XAxis.MinimumTickSpacing(1);
        _plt1.YAxis.SetBoundary(0, 100);
        _plt1.Grid();
        _plt1.YLabel("使用中 (%)");
        _plt1.XLabel("时间 (s)");
        _plt1.Title("内存使用 %");

        _plt2.XAxis.MinimumTickSpacing(1);
        _plt2.YAxis.SetBoundary(0, 100);
        _plt2.YLabel("已提交 (%)");
        _plt2.XLabel("时间 (s)");
        _plt2.Title("虚拟内存 %");

        WpUsed.Refresh();
        WpCommit.Refresh();
    }

    private static void UpdateArray(double[] array, double last)
    {
        var len = array.Length;
        Array.Copy(array, 1, array, 0, len - 1);
        array[^1] = last;
    }

    public void GetMemory(object state)
    {
        double p1 = 0.0;
        double p2 = 0.0;
        double virtualUsed = 0.0;
        MemoryInfo mi;
        lock (_locker)
        {
            mi = GetSystemMemoryUsagePercentage();
            p1 = ((mi.TotalVisibleMemorySize - mi.FreePhysicalMemory) / mi.TotalVisibleMemorySize) * 100;
            p1 = Math.Round(p1, 2);

            virtualUsed = mi.TotalVirtualMemorySize - mi.FreeVirtualMemory;
            p2 = (virtualUsed / mi.TotalVirtualMemorySize) * 100;
            p2 = Math.Round(p2, 2);

            UpdateArray(Percentages, p1);
            UpdateArray(Commits, p2);
            UpdateArray(CommitsAvg, Math.Round(Commits.Average(), 2));

            _ema.AddDataPoint(p2);
            var vv = _ema.Average;
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
                if (Math.Abs(_ppTitle - p2) >= 0.5)
                {
                    _ppTitle = p2;
                    Title = Math.Round(_ppTitle, 1) + "%";
                }

                WpUsed.Render();
                WpCommit.Render();
                _plt1.AxisAuto();
                _plt2.AxisAuto();
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
        var toGb = 1024.0 * 1024.0;
        var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
        var mi = wmiObject.Get().Cast<ManagementObject>().Select(mo => new MemoryInfo
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

public class MemoryInfo
{
    public double FreePhysicalMemory { get; set; } = 1;

    public double TotalVisibleMemorySize { get; set; } = 2;

    public double FreeSpaceInPagingFiles { get; set; } = 3;

    public double TotalVirtualMemorySize { get; set; } = 4;

    public double FreeVirtualMemory { get; set; } = 5;
}
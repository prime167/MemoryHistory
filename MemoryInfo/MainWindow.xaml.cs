using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Windows;
using ScottPlot;
using ScottPlot.Plottable;

namespace MemoryInfo;

// ReSharper disable once UnusedMember.Global
public partial class MainWindow : Window
{
    // ReSharper disable once NotAccessedField.Local
    private Timer _updateMemoryTimer;

    /// <summary>
    /// 显示的时间范围
    /// </summary>
    private const int MaxPeriod = 60 * 10;//60 * 30;

    private static object _locker = new object();
    private const int k = 30;// sample Count;
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

    /// <summary>
    /// X轴最大值
    /// </summary>
    private const int MaxSeconds = 60 * 60 * 24 * 365;

    public MemoryViewModel Vm = new();
    private Plot _plt1;
    private Plot _plt2;

    public MainWindow()
    {
        InitializeComponent();
        Vm.MinPercentage = 100;
        Vm.MaxPercentage = 0;
        Vm.MinCommit = 100;
        Vm.MaxCommit = 0;
        DataContext = Vm;
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
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
        _updateMemoryTimer = new Timer(GetMemory, null, 0, 1000);

        static string customTickFormatter(double position)
        {
            return $"{600 - position}";
        }

        _plt1.XAxis.TickLabelFormat(customTickFormatter);
        _plt2.XAxis.TickLabelFormat(customTickFormatter);
    }

    private void ResetCharts()
    {
        _plt1.Clear();

        WpUsed.Configuration.DoubleClickBenchmark = false;
        WpUsed.MouseDoubleClick += WpUsed_MouseDoubleClick;

        WpUsed.Plot.XAxis.SetBoundary(0, MaxSeconds);
        _plt1.XAxis.MinimumTickSpacing(1);
        _plt1.YAxis.SetBoundary(0, 100);
        _plt1.Grid();
        _plt1.YLabel("使用中 (%)");
        _plt1.XLabel("时间 (s)");
        _plt1.Title("内存使用 %");
        WpUsed.Refresh();

        _plt2.Clear();
        WpCommit.Configuration.DoubleClickBenchmark = false;
        WpCommit.MouseDoubleClick += WpCommit_MouseDoubleClick;
        _plt2.XAxis.SetBoundary(0, MaxSeconds);
        _plt2.XAxis.MinimumTickSpacing(1);
        _plt2.YAxis.SetBoundary(0, 100);
        _plt2.YLabel("已提交 (%)");
        _plt2.XLabel("时间 (s)");
        _plt2.Title("虚拟内存 %");
        WpCommit.Refresh();
    }

    private void UpdateArray(double[] array, double last)
    {
        var len = array.Length;
        Array.Copy(array, 1, array, 0, len - 1);
        array[^1] = last;
    }

    public void GetMemory(object state)
    {
        double p1 = 0.0;
        double p2 = 0.0;
        lock (_locker)
        {
            (p1, p2) = GetSystemMemoryUsagePercentage();
            p1 = Math.Round(p1, 2);
            p2 = Math.Round(p2, 2);

            UpdateArray(Percentages, p1);
            UpdateArray(Commits, p2);
            UpdateArray(CommitsAvg, Math.Round(Commits.Average(), 2));

            _ema.AddDataPoint(p2);
            var vv = _ema.Average;
            UpdateArray(CommitsEma, vv);
        }

        Vm.CurrentPercentage = p1;

        if (Math.Abs(p2 - Vm.CurrentCommit) > 0.001)
        {
            Vm.CurrentCommit = p2;
        }

        if (p1 < Vm.MinPercentage)
        {
            Vm.MinPercentage = p1;
            Vm.MinPercentageTime = DateTime.Now;
        }

        if (p1 > Vm.MaxPercentage)
        {
            Vm.MaxPercentage = p1;
            Vm.MaxPercentageTime = DateTime.Now;
        }

        if (p2 < Vm.MinCommit)
        {
            Vm.MinCommit = p2;
            Vm.MinCommitTime = DateTime.Now;
        }

        if (p2 > Vm.MaxCommit)
        {
            Vm.MaxCommit = p2;
            Vm.MaxCommitTime = DateTime.Now;
        }

        Vm.MaxPercentageTimeStr = Utils.GetRelativeTime(Vm.MaxPercentageTime);
        Vm.MinPercentageTimeStr = Utils.GetRelativeTime(Vm.MinPercentageTime);
        Vm.MaxCommitTimeStr = Utils.GetRelativeTime(Vm.MaxCommitTime);
        Vm.MinCommitTimeStr = Utils.GetRelativeTime(Vm.MinCommitTime);

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

    public (double percentageUsed, double percentageCommitted) GetSystemMemoryUsagePercentage()
    {
        var toGb = 1024.0 * 1024.0;
        var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
        var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
        {
            FreePhysicalMemory = Math.Round(double.Parse(mo["FreePhysicalMemory"].ToString()) / toGb, 2),
            TotalVisibleMemorySize = Math.Round(double.Parse(mo["TotalVisibleMemorySize"].ToString()) / toGb, 2),
            FreeSpaceInPagingFiles = Math.Round(double.Parse(mo["FreeSpaceInPagingFiles"].ToString()) / toGb, 2),
            TotalVirtualMemorySize = Math.Round(double.Parse(mo["TotalVirtualMemorySize"].ToString()) / toGb, 2),
            FreeVirtualMemory = Math.Round(double.Parse(mo["FreeVirtualMemory"].ToString()) / toGb, 2),
        }).FirstOrDefault();

        if (memoryValues != null)
        {
            var percentageUsed = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
            var tc = memoryValues.TotalVirtualMemorySize - memoryValues.FreeVirtualMemory;
            //Console.WriteLine($"Total committed: {Math.Round(tc, 1)} GB");
            var percentageCommitted = (tc / memoryValues.TotalVirtualMemorySize) * 100;
            return (percentageUsed, percentageCommitted);
        }

        return (0, 0);
    }
}
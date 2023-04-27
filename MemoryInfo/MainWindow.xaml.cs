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
    private const int k = 30;// sample Count;
    public List<double> Times = new();
    public List<double> Percentages = new();
    public List<double> Commits = new();
    public List<double> CommitsAvg = new();
    public List<double> CommitsSma= new();
    private SimpleMovingAverage _sma;

    private ScatterPlotList<double> _plot1;
    private ScatterPlotList<double> _plot2;
    private ScatterPlotList<double> _plot3;
    private ScatterPlotList<double> _plot4;
    private int _count;
    private double _ppTitle = 0.0;

    /// <summary>
    /// X轴最大值
    /// </summary>
    private const int MaxSeconds = 60 * 60 * 24 * 365;

    /// <summary>
    /// 显示的时间范围
    /// </summary>
    private const int MaxPeriod = 60 * 30;

    public MemoryViewModel Vm = new();

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
        _sma = new SimpleMovingAverage(k);
        ResetCharts();
        _plot1 = WpMemory.Plot.AddScatterList();

        var plt = WpCommit.Plot;
        _plot2 = plt.AddScatterList(label:"now",color:Color.Green);
        _plot3 = plt.AddScatterList(label:"avg",color:Color.Blue);
        _plot4 = plt.AddScatterList(label:"sma", color:Color.Orange);
        plt.Legend(location:Alignment.UpperLeft);

        _plot1.MarkerSize = 1;
        _plot2.MarkerSize = 1;
        _plot3.MarkerSize = 1;
        _plot4.MarkerSize = 1;
        _updateMemoryTimer = new Timer(GetMemory, null, 0, 1000);
    }

    private void ResetCharts()
    {
        WpMemory.Plot.Clear();

        WpMemory.Configuration.DoubleClickBenchmark = false;
        WpMemory.Plot.XAxis.SetBoundary(0, MaxSeconds);
        WpMemory.Plot.XAxis.MinimumTickSpacing(1);
        WpMemory.Plot.YAxis.SetBoundary(0, 100);
        WpMemory.Plot.Grid();
        WpMemory.MouseDoubleClick += WpMemory_MouseDoubleClick;
        WpMemory.Plot.YLabel("使用中 (%)");
        WpMemory.Plot.XLabel("时间 (s)");
        WpMemory.Plot.Title("内存使用 %");
        WpMemory.Refresh();

        WpCommit.Plot.Clear();
        WpCommit.Configuration.DoubleClickBenchmark = false;
        WpCommit.Plot.XAxis.SetBoundary(0, MaxSeconds);
        WpCommit.Plot.XAxis.MinimumTickSpacing(1);
        WpCommit.Plot.YAxis.SetBoundary(0, 100);
        WpCommit.MouseDoubleClick += WpPageFile_MouseDoubleClick;
        WpCommit.Plot.YLabel("已提交 (%)");
        WpCommit.Plot.XLabel("时间 (s)");
        WpCommit.Plot.Title("虚拟内存 %");
        WpCommit.Refresh();
    }

    public void GetMemory(object state)
    {
        (double p1, double p2) = GetSystemMemoryUsagePercentage();
        p1 = Math.Round(p1, 2);
        p2 = Math.Round(p2, 2);

        Times.Add(_count);
        Percentages.Add(p1);
        Commits.Add(p2);
        CommitsAvg.Add(Math.Round(Commits.Average(),3));
        var v = _sma.Update(p2);
        CommitsSma.Add(v);

        if (Times.Count > MaxPeriod)
        {
            Times.RemoveAt(0);
            Percentages.RemoveAt(0);
            Commits.RemoveAt(0);
            CommitsAvg.RemoveAt(0);
            CommitsSma.RemoveAt(0);
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

                _plot1.Clear();
                _plot1.AddRange(Times.ToArray(), Percentages.ToArray());
                WpMemory.Refresh();
                WpMemory.Plot.AxisAuto();

                _plot2.Clear();
                _plot2.AddRange(Times.ToArray(), Commits.ToArray());

                _plot3.Clear();
                _plot3.AddRange(Times.ToArray(), CommitsAvg.ToArray());

                _plot4.Clear();
                _plot4.AddRange(Times.ToArray(), CommitsSma.ToArray());

                WpCommit.Refresh();
                WpCommit.Plot.AxisAuto();

                _count++;
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }

    private void WpMemory_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        WpMemory.Plot.AxisAuto();
    }

    private void WpPageFile_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
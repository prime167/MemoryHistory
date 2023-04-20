using System.Management;
using System.Windows;
using ScottPlot.Plottable;
using System.IO;

namespace MemoryInfo;

public partial class MainWindow : Window
{
    private Timer updateMemoryTimer;
    public readonly double[] Mem = new double[600];
    public readonly double[] FileSize = new double[600];
    private SignalPlot _plot1;
    private SignalPlot _plot2;

    private double _currentPercentage = 0; //%
    private double _minPercentage = 100; //%
    private double _maxPercentage = 0; //%

    private double _currentSize = 0; //GB
    private double _minSize = 100; //GB
    private double _maxSize = 0; //GB
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        ResetCharts();
        _plot1 = WpMemory.Plot.AddSignal(Mem.ToArray());
        _plot2 = WpMemory.Plot.AddSignal(FileSize.ToArray());

        updateMemoryTimer = new Timer(GetMemory, null, 0, 1000);
    }

    private void ResetCharts()
    {
        WpMemory.Plot.Clear();

        WpMemory.Configuration.DoubleClickBenchmark = false;
        WpMemory.Plot.SetAxisLimits(0, 600, 0, 100);
        //WpMemory.Plot.SetOuterViewLimits(0, 10000, -0.1, 300);
        WpMemory.MouseDoubleClick += WpMemory_MouseDoubleClick;
        WpMemory.Plot.YLabel("Memory");
        WpMemory.Plot.XLabel("Time");
        WpMemory.Plot.Title("Memory usage");
        WpMemory.Refresh();
        
        WpPageFile.Plot.Clear();

        WpPageFile.Configuration.DoubleClickBenchmark = false;
        WpPageFile.Plot.SetAxisLimits(0, 600, 0, 8);
        //WpPageFile.Plot.SetOuterViewLimits(0, 10000, -0.1, 300);
        WpPageFile.MouseDoubleClick += WpPageFile_MouseDoubleClick;
        WpPageFile.Plot.YLabel("pagefile size");
        WpPageFile.Plot.XLabel("Time");
        WpPageFile.Plot.Title("pagefile size");
        WpPageFile.Refresh();
    }

    public void GetMemory(object? state)
    {
        var p = GetMemoryUsage();
        p = Math.Round(p, 2);

        var file = @"c:\pagefile.sys";
        var fi = new FileInfo(file);
        var size = fi.Length / 1024.0 / 1024.0 / 1024.0;
        size = Math.Round(size, 3);

        if (p < _minPercentage)
        {
            _minPercentage = p;
        }

        if (p > _maxPercentage)
        {
            _maxPercentage = p;
        }

        if (size < _minSize)
        {
            _minSize = size;
        }

        if (size > _maxSize)
        {
            _maxSize = size;
        }

        // 左移曲线
        Array.Copy(Mem, 1, Mem, 0, Mem.Length - 1);
        Mem[^1] = p;

        Array.Copy(FileSize, 1, FileSize, 0, FileSize.Length - 1);
        FileSize[^1] = size;

        try
        {
            this.Dispatcher.Invoke(() =>
            {
                ResetCharts();
                _plot1 = WpMemory.Plot.AddSignal(Mem.ToArray());
                WpMemory.Refresh();

                _plot2 = WpPageFile.Plot.AddSignal(FileSize.ToArray());
                WpPageFile.Refresh();

                LblCurrentPercentage.Content = $"{p} %";
                LblMinPercentage.Content = $"{_minPercentage} %";
                LblMaxPercentage.Content = $"{_maxPercentage} %";

                LblCurrentSize.Content = $"{size} GB";
                LblMinSize.Content = $"{_minSize} GB";
                LblMaxSize.Content = $"{_maxSize} GB";
            });
        }
        catch (Exception e)
        {
        }
    }

    private void WpMemory_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        WpMemory.Plot.AxisAuto();
    }

    private void WpPageFile_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        WpPageFile.Plot.AxisAuto();
    }

    private double GetMemoryUsage()
    {
        var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

        var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
        {
            FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
            TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
        }).FirstOrDefault();

        if (memoryValues != null)
        {
            var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
            return percent;
        }

        return 0;
    }
}
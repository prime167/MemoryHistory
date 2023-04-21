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

    public MemoryViewModel Vm = new MemoryViewModel();

    public MainWindow()
    {
        InitializeComponent();
        Vm.MinPercentage = 100;
        Vm.MaxPercentage = 0;
        Vm.MinSize = 100;
        Vm.MaxSize = 0;
        DataContext = Vm;
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

        Vm.CurrentPercentage = p;
        Vm.CurrentSize = size;
        if (p < Vm.MinPercentage)
        {
            Vm.MinPercentage = p;
            Vm.MinPercentageTime = DateTime.Now.ToLongTimeString();
        }

        if (p > Vm.MaxPercentage)
        {
            Vm.MaxPercentage = p;
            Vm.MaxPercentageTime = DateTime.Now.ToLongTimeString();
        }

        if (size < Vm.MinSize)
        {
            Vm.MinSize = size;
            Vm.MinSizeTime = DateTime.Now.ToLongTimeString();
        }

        if (size > Vm.MaxSize)
        {
            Vm.MaxSize = size;
            Vm.MaxSizeTime = DateTime.Now.ToLongTimeString();
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
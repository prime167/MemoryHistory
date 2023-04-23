﻿using System.Diagnostics;
using System.Management;
using System.Windows;
using ScottPlot.Plottable;
using System.IO;

namespace MemoryInfo;

// ReSharper disable once UnusedMember.Global
public partial class MainWindow : Window
{
    // ReSharper disable once NotAccessedField.Local
    private Timer _updateMemoryTimer;
    public List<double> Times = new();
    public List<double> Percentages = new();
    public List<double> Sizes = new();

    private ScatterPlotList<double> _plot1;
    private ScatterPlotList<double> _plot2;
    private int _count;
    private const int MaxSeconds = 60 * 15;

    public MemoryViewModel Vm = new();

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
        _plot1 = WpMemory.Plot.AddScatterList();
        _plot2 = WpPageFile.Plot.AddScatterList();

        _plot1.MarkerSize = 1;
        _plot2.MarkerSize = 1;
        _updateMemoryTimer = new Timer(GetMemory, null, 0, 1000);
    }

    private void ResetCharts()
    {
        WpMemory.Plot.Clear();

        WpMemory.Configuration.DoubleClickBenchmark = false;
        WpMemory.Plot.XAxis.SetBoundary(0, 10000);
        WpMemory.Plot.YAxis.SetBoundary(0, 100);
        WpMemory.MouseDoubleClick += WpMemory_MouseDoubleClick;
        WpMemory.Plot.YLabel("Memory");
        WpMemory.Plot.XLabel("Time");
        WpMemory.Plot.Title("Memory usage");
        WpMemory.Refresh();

        WpPageFile.Plot.Clear();

        WpPageFile.Configuration.DoubleClickBenchmark = false;
        WpPageFile.Plot.XAxis.SetBoundary(0, 10000);
        WpPageFile.Plot.YAxis.SetBoundary(0, 100);
        WpPageFile.MouseDoubleClick += WpPageFile_MouseDoubleClick;
        WpPageFile.Plot.YLabel("已提交");
        WpPageFile.Plot.XLabel("Time");
        WpPageFile.Plot.Title("虚拟内存");
        WpPageFile.Refresh();
    }

    public void GetMemory(object state)
    {
        (double p1,double p2) = GetSystemMemoryUsagePercentage();
        p1 = Math.Round(p1, 2);

        //const string file = @"c:\pagefile.sys";
        //var fi = new FileInfo(file);
        //double pp = fi.Length / 1024.0 / 1024.0 / 1024.0;
        //size = Math.Round(size, 3);
        var pp = Math.Round(p2, 2);

        Times.Add(_count);
        Percentages.Add(p1);
        Sizes.Add(pp);

        if (Times.Count > MaxSeconds)
        {
            Times.RemoveAt(0);
            Percentages.RemoveAt(0);
            Sizes.RemoveAt(0);
        }

        Vm.CurrentPercentage = p1;

        if (Math.Abs(pp - Vm.CurrentSize) > 0.1)
        {
            Vm.CurrentSize = pp;
            Vm.CurrentSizeTime = DateTime.Now;
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

        if (pp < Vm.MinSize)
        {
            Vm.MinSize = pp;
            Vm.MinSizeTime = DateTime.Now;
        }

        if (pp > Vm.MaxSize)
        {
            Vm.MaxSize = pp;
            Vm.MaxSizeTime = DateTime.Now;
        }

        Vm.MaxPercentageTimeStr = Utils.GetRelativeTime(Vm.MaxPercentageTime);
        Vm.MinPercentageTimeStr = Utils.GetRelativeTime(Vm.MinPercentageTime);
        Vm.CurrentSizeTimeStr = Utils.GetRelativeTime(Vm.CurrentSizeTime);
        Vm.MaxSizeTimeStr = Utils.GetRelativeTime(Vm.MaxSizeTime);
        Vm.MinSizeTimeStr = Utils.GetRelativeTime(Vm.MinSizeTime);

        try
        {
            Dispatcher.Invoke(() =>
            {
                _plot1.Clear();
                _plot1.AddRange(Times.ToArray(), Percentages.ToArray());
                WpMemory.Refresh();
                WpMemory.Plot.AxisAuto();

                _plot2.Clear();
                _plot2.AddRange(Times.ToArray(), Sizes.ToArray());
                WpPageFile.Refresh();
                WpPageFile.Plot.AxisAuto();
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
        WpPageFile.Plot.AxisAuto();
    }

    public (double percentageUsed, double percentageCommitted) GetSystemMemoryUsagePercentage()
    {
        var toGb = 1024.0 * 1024.0;
        var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
        var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
        {
            FreePhysicalMemory = Math.Round(double.Parse(mo["FreePhysicalMemory"].ToString()) / toGb,2),
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
using System;
using System.Diagnostics;
using System.Threading;
using ScottPlot;
using ScottPlot.Plottable;

namespace MemHistoryWpf;

public class ProcessPlotInfo
{
    public string Name { get; set; }

    public int Time { get; set; }

    public readonly double[] Mem;
    public readonly double[] ProcessCount;

    public SignalPlot? SignalPlotMem;
    public SignalPlot? SignalPlotPc;

    public WpfPlot FormPlot;

    private double memoryUsed;

    private Timer? updateMemoryTimer;
    private Timer? refreshTimer;

    public bool PlotProcessCount { get; set; }

    public ProcessPlotInfo(string processName, int time, WpfPlot formPlot, bool plotProcessCount = false)
    {
        Name = processName;
        Time = time;
        Mem = new double[60 * time];
        ProcessCount = new double[60 * time];
        FormPlot = formPlot;
        PlotProcessCount = plotProcessCount;
    }

    public void GetMemory(object? state)
    {
        var ps = Process.GetProcessesByName(Name);
        var total = 0.0;
        foreach (var p in ps)
        {
            if (!p.HasExited)
            {
                try
                {
                    if (Name == "msedge" && !p.MainModule.FileName.Contains("Dev"))
                    {
                        continue;
                    }
                }
                catch (Exception)
                {
                    continue;
                }

                p.Refresh();
                double s = 0.0;
                try
                {
                    s = p.WorkingSet64 / 1024.0 / 1024.0;
                    total += s;
                }
                catch (Exception)
                {

                }
            }
        }

        memoryUsed = Math.Round(total, 2);

        // 左移曲线
        Array.Copy(Mem, 1, Mem, 0, Mem.Length - 1);
        Mem[^1] = memoryUsed;
        if (PlotProcessCount)
        {
            Array.Copy(ProcessCount, 1, ProcessCount, 0, ProcessCount.Length - 1);
            ProcessCount[^1] = ps.Length;
        }

        try
        {
            FormPlot.Dispatcher.Invoke(() =>
            {
                FormPlot.Plot.AxisAuto();
                FormPlot.Refresh();
            });
        }
        catch (Exception e)
        {
        }
    }

    public void StartPlot()
    {
        updateMemoryTimer = new Timer(GetMemory, null, 0, 1000);
        //refreshTimer = new Timer(RefreshPlot, null, 0, 1000);
    }

    private void RefreshPlot(object? state)
    {
        try
        {
            FormPlot.Dispatcher.Invoke(() =>
            {
                FormPlot.Plot.AxisAuto();
                FormPlot.Refresh();
            });
        }
        catch (Exception e)
        {
        }
    }
}
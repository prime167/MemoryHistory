using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using MemoryHistory.Models;
using Prism.Mvvm;
using NLog;
using System.Runtime.ConstrainedExecution;

namespace MemoryHistory.ViewModels;

public class MainWindowViewModel : BindableBase
{
    public int Keep { get; set; } = 60 * 30;
    public int SampleCount { get; set; } = 6;

    private readonly DateTime _startTime;

    private string _title = "Memory Monitor";

    private Logger _logger = LogManager.GetCurrentClassLogger();

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _time;
    public string Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }

    private string _systemMemUsage;
    public string SystemMemUsage
    {
        get => _systemMemUsage;
        set => SetProperty(ref _systemMemUsage, value);
    }

    private ObservableCollection<ProcessMem> _processMems;
    public ObservableCollection<ProcessMem> ProcessMems
    {
        get => _processMems;
        set => SetProperty(ref _processMems, value);
    }

    public MainWindowViewModel()
    {
        ProcessMems = new ObservableCollection<ProcessMem>();

        DispatcherTimer timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += CallBack;
        timer.Start();
        _startTime = DateTime.Now;
        Dictionary<string, string> rd = new Dictionary<string, string>
        {
            { "Vivaldi", "Vivaldi" },
            { "msedge", "Edge" },
            { "Chrome", "Chrome" },
            { "MemoryHistory", "Self" },
        };

        foreach (var process in rd)
        {
            var ps = Process.GetProcessesByName(process.Key);
            if (ps.Any())
            {
                var pm = new ProcessMem
                {
                    MemCurve = new ObservableCollection<Point>(),
                    ProcessName = process.Key,
                    ProcessDisplayName = process.Value,
                    MovingAverage = new SimpleMovingAverage(5)
                };
                ProcessMems.Add(pm);
            }
        }
    }

    private void CallBack(object sender, EventArgs e)
    {
        var ts = DateTime.Now - _startTime;
        int seconds = (int)ts.TotalSeconds;

        Time = ts.ToString(@"hh\:mm\:ss");
        var memUsage = Util.GetMemoryUsage();
        SystemMemUsage = memUsage;
        try
        {
            foreach (ProcessMem pm in ProcessMems)
            {
                var ps = Process.GetProcessesByName(pm.ProcessName);
                var total = 0.0;
                foreach (var p in ps)
                {
                    if (!p.HasExited)
                    {
                        p.Refresh();
                        var s = Math.Round(p.PrivateMemorySize64 / 1024.0 / 1024.0, 2);
                        total += s;
                    }
                }

                total = Math.Round(total, 2);

                var r = pm.MovingAverage.Update(total);
                pm.MemCurve.Add(new Point(seconds, r));

                if (pm.MemCurve.Count >= Keep)
                {
                    pm.MemCurve = new ObservableCollection<Point>(pm.MemCurve.TakeLast(Keep));
                }

                pm.BottomStr = pm.ProcessDisplayName + " " + total.ToString("#.00").PadLeft(12) + " MB";
            }

        }
        catch (Exception exception)
        {
            _logger.Error(exception,exception.Message);
        }
    }
}
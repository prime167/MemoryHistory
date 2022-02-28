using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MemoryHistory.Models;
using Prism.Mvvm;
using NLog;
using Prism.Commands;
using Tomlet;

namespace MemoryHistory.ViewModels;

public class MainWindowViewModel : BindableBase
{
    public int Keep { get; set; } = 60 * 30;

    public int SampleCount { get; set; } = 11;

    private DateTime _startTime;

    private string _title = "Memory Monitor";

    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

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

    private DelegateCommand _loadedCmd;
    private List<ProcessInfo1> _psInfo1s;

    public DelegateCommand LoadedCmd => _loadedCmd ?? (_loadedCmd = new DelegateCommand(_loaded));

    public MainWindowViewModel()
    {

    }

    private void _loaded()
    {
        var str = File.ReadAllText("configs.ini");
        var config = TomletMain.To<Configs>(str);
        var ps = config.Processes;

        ProcessMems = new ObservableCollection<ProcessMem>();

        DispatcherTimer timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += CallBack;
        timer.Start();
        _startTime = DateTime.Now;
        List<ProcessInfo1> rd = new List<ProcessInfo1>();

        foreach (var process in ps)
        {
            var pi = new ProcessInfo1
                {
                    ProcessDisplayName = process,
                    ProcessName = process,
                    DecimalCount = 1
                };

            rd.Add(pi);
        }

        foreach (var process in rd)
        {
            //var ps = Process.GetProcessesByName(process.Key);
            //if (ps.Any())
            {
                var pm = new ProcessMem
                {
                    MemCurve = new ObservableCollection<Point>(),
                    ProcessName = process.ProcessName,
                    ProcessDisplayName = process.ProcessDisplayName,
                    MovingAverage = new SimpleMovingAverage(SampleCount),
                    DecimalCount = process.DecimalCount,
                };

                ProcessMems.Add(pm);
            }
        }
    }

    private void CallBack(object sender, EventArgs e)
    {
        var ts = DateTime.Now - _startTime;
        int seconds = (int)ts.TotalSeconds;

        if (ts.TotalDays > 1)
        {
            Time = $"{Math.Floor(ts.TotalDays)}d {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
        }
        else
        {
            Time = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
        }

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

                total = Math.Round(total, pm.DecimalCount);
                //if (total == 0)
                //{
                //    pm.Visibility = Visibility.Collapsed;
                //}
                //else
                //{
                //    pm.Visibility = Visibility.Visible;
                //}

                var r = pm.MovingAverage.Update(total);
                pm.MemCurve.Add(new Point(seconds, r));

                if (pm.MemCurve.Count >= Keep)
                {
                    pm.MemCurve = new ObservableCollection<Point>(pm.MemCurve.TakeLast(Keep));
                }

                pm.BottomStr = pm.ProcessDisplayName + " " + total.ToString().PadLeft(12) + " MB";
            }
        }
        catch (Exception exception)
        {
            _logger.Error(exception, exception.Message);
        }
    }
}
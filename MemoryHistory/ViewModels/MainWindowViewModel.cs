using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using MemoryHistory.Models;
using Prism.Mvvm;

namespace MemoryHistory.ViewModels;

public class MainWindowViewModel : BindableBase
{
    public int Keep { get; set; } = 60 * 30;
    private readonly DateTime _startTime;

    private string _title = "Memory Monitor";
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
            { "MemoryHistory", "Self" }
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
                    ProcessDisplayName = process.Value
                };
                ProcessMems.Add(pm);
            }
        }
    }

    private void CallBack(object sender, EventArgs e)
    {
        var ts = DateTime.Now - _startTime;
        var seconds = ts.TotalSeconds;

        Time = ts.ToString(@"hh\:mm\:ss");
        var memUsage = Util.GetMemoryUsage();
        SystemMemUsage = memUsage;
        foreach (ProcessMem pm in ProcessMems)
        {
            var ps = Process.GetProcessesByName(pm.ProcessName);
            var total = 0.0;
            foreach (var v in ps)
            {
                var s = Math.Round(v.WorkingSet64 / 1024.0 / 1024.0, 2);
                total += s;
            }

            total = Math.Round(total, 2);
            pm.MemCurve.Add(new Point(seconds, total));

            if (pm.MemCurve.Count >= Keep)
            {
                pm.MemCurve = new ObservableCollection<Point>(pm.MemCurve.TakeLast(Keep));
            }

            pm.BottomStr = pm.ProcessDisplayName + " " + total.ToString("#.00").PadLeft(12) + " MB";
        }
    }
}
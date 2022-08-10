using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using NLog;
using ScottPlot;
using ScottPlot.Plottable;

namespace MemoryHistoryWpf
{
    public partial class MainWindow : Window
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly List<ProcessPlotInfo> _processPlotInfos = new List<ProcessPlotInfo>(10);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var me = Assembly.GetEntryAssembly().GetName().Name;
            var ps1 = new ProcessPlotInfo("msedge", 120, WpfPlot1);
            var ps2 = new ProcessPlotInfo("msedge", 12, WpfPlot2);

            var ps3 = new ProcessPlotInfo(me, 120, WpfPlot3);
            var ps4 = new ProcessPlotInfo(me, 12, WpfPlot4);

            var ps5 = new ProcessPlotInfo("vivaldi", 120, WpfPlot5);
            var ps6 = new ProcessPlotInfo("vivaldi", 12, WpfPlot6);

            _processPlotInfos.Add(ps1);
            _processPlotInfos.Add(ps2);
            _processPlotInfos.Add(ps3);
            _processPlotInfos.Add(ps4);
            _processPlotInfos.Add(ps5);
            _processPlotInfos.Add(ps6);

            foreach (ProcessPlotInfo pi in _processPlotInfos)
            {
                pi.FormPlot.Configuration.DoubleClickBenchmark = false;
                pi.FormPlot.Plot.Title($"{pi.Name}");
                pi.FormPlot.Plot.YLabel("Memory (MB)");
                pi.FormPlot.Plot.XLabel("Time (min)");

                var sig = pi.FormPlot.Plot.AddSignal(pi.LiveData);
                sig.FillBelow();
                pi.SignalPlot = sig;
                sig.MarkerSize = 0;
                sig.MarkerShape = MarkerShape.filledCircle;

                double[] xPositions = Enumerable.Range(0, 11).Select(e => e * 60.0 * pi.Time / 10).ToArray();
                string[] xLabels = xPositions.Select(x => x / 60 + " m").Reverse().ToArray();
                pi.FormPlot.Plot.XAxis.ManualTickPositions(xPositions, xLabels);

                pi.StartPlot();
            }
        }
    }

    public class ProcessPlotInfo
    {
        public string Name { get; set; }

        public int Time { get; set; }

        public readonly double[] LiveData;

        public SignalPlot SignalPlot;

        public WpfPlot FormPlot;

        private double memoryUsed;

        private Timer updateMemoryTimer;
        private Timer refreshTimer;

        public ProcessPlotInfo(string processName, int time, WpfPlot formPlot)
        {
            Name = processName;
            Time = time;
            LiveData = new double[60 * time];
            FormPlot = formPlot;
        }

        public void GetMemory(object? state)
        {
            var ps = Process.GetProcessesByName(Name);
            var total = 0.0;
            foreach (var p in ps)
            {
                if (!p.HasExited)
                {
                    p.Refresh();
                    var s = p.WorkingSet64 / 1024.0 / 1024.0;
                    total += s;
                }
            }

            memoryUsed = Math.Round(total, 2);

            // "scroll" the whole chart to the left
            Array.Copy(LiveData, 1, LiveData, 0, LiveData.Length - 1);
            LiveData[LiveData.Length - 1] = memoryUsed;

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

}

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
using ScottPlot.Renderable;

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

                // Create another axis to the left and give it an index of 2
                pi.FormPlot.Plot.YAxis2.Ticks(true);
                pi.FormPlot.Plot.YAxis2.Label("Porcess Count");
                pi.FormPlot.Plot.YAxis2.Grid(false);
                pi.FormPlot.Plot.YAxis2.MinorGrid(false);
                var sigMem = pi.FormPlot.Plot.AddSignal(pi.Mem);
                sigMem.YAxisIndex = 0;
                sigMem.XAxisIndex = 0;

                var sigPc = pi.FormPlot.Plot.AddSignal(pi.ProcessCount);
                sigPc.YAxisIndex = 1;
                sigPc.XAxisIndex = 0;

                sigMem.FillBelow();
                pi.SignalPlotMem = sigMem;
                pi.SignalPlotPc = sigPc;
                sigMem.MarkerSize = 0;
                sigMem.MarkerShape = MarkerShape.filledCircle;

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

        public readonly double[] Mem;
        public readonly double[] ProcessCount;

        public SignalPlot SignalPlotMem;
        public SignalPlot SignalPlotPc;

        public WpfPlot FormPlot;

        private double memoryUsed;

        private Timer updateMemoryTimer;
        private Timer refreshTimer;

        public ProcessPlotInfo(string processName, int time, WpfPlot formPlot)
        {
            Name = processName;
            Time = time;
            Mem = new double[60 * time];
            ProcessCount = new double[60 * time];
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
                    if (Name == "msedge" && !p.MainModule.FileName.Contains("Dev"))
                    {
                        continue;
                    }

                    p.Refresh();
                    var s = p.WorkingSet64 / 1024.0 / 1024.0;
                    total += s;
                }
            }

            memoryUsed = Math.Round(total, 2);

            // "scroll" the whole chart to the left
            Array.Copy(Mem, 1, Mem, 0, Mem.Length - 1);
            Array.Copy(ProcessCount, 1, ProcessCount, 0, ProcessCount.Length - 1);
            Mem[Mem.Length - 1] = memoryUsed;
            ProcessCount[ProcessCount.Length - 1] = ps.Count();

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

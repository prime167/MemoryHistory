using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using NLog;
using ScottPlot;

namespace MemHistoryWpf;

public partial class MainWindow : Window
{
    private Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly List<ProcessPlotInfo> _processPlotInfos = new(10);

    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var me = Assembly.GetEntryAssembly()?.GetName().Name;
        var ps1 = new ProcessPlotInfo(me, 600, WpfPlot1);
        var ps2 = new ProcessPlotInfo(me, 60, WpfPlot2);

        var ps3 = new ProcessPlotInfo("firefox", 600, WpfPlot3);
        var ps4 = new ProcessPlotInfo("firefox", 60, WpfPlot4, true);

        var ps5 = new ProcessPlotInfo("vivaldi", 600, WpfPlot5);
        var ps6 = new ProcessPlotInfo("vivaldi", 60, WpfPlot6);

        var ps7 = new ProcessPlotInfo("chrome", 600, WpfPlot7);
        var ps8 = new ProcessPlotInfo("chrome", 60, WpfPlot8);

        _processPlotInfos.Add(ps1);
        _processPlotInfos.Add(ps2);
        _processPlotInfos.Add(ps3);
        _processPlotInfos.Add(ps4);
        _processPlotInfos.Add(ps5);
        _processPlotInfos.Add(ps6);
        _processPlotInfos.Add(ps7);
        _processPlotInfos.Add(ps8);

        foreach (ProcessPlotInfo pi in _processPlotInfos)
        {
            pi.FormPlot.Configuration.DoubleClickBenchmark = false;
            pi.FormPlot.Plot.Title($"{pi.Name}");
            pi.FormPlot.Plot.YLabel("Memory (MB)");
            pi.FormPlot.Plot.XLabel("Time (min)");
            var sigMem = pi.FormPlot.Plot.AddSignal(pi.Mem);
            sigMem.YAxisIndex = 0;
            sigMem.XAxisIndex = 0;

            if (pi.PlotProcessCount)
            {
                // 右侧Y轴，进程数量
                pi.FormPlot.Plot.YAxis2.Ticks(true);
                pi.FormPlot.Plot.YAxis2.Label("Porcess Count");
                pi.FormPlot.Plot.YAxis2.Grid(false);
                pi.FormPlot.Plot.YAxis2.MinimumTickSpacing(1); // 确保不出现小数

                var sigPc = pi.FormPlot.Plot.AddSignal(pi.ProcessCount);
                sigPc.YAxisIndex = 1;
                sigPc.XAxisIndex = 0;
                pi.SignalPlotPc = sigPc;
            }

            sigMem.FillBelow();
            pi.SignalPlotMem = sigMem;
            sigMem.MarkerSize = 0;
            sigMem.MarkerShape = MarkerShape.filledCircle;

            double[] xPositions;
            string[] xLabels;

            if (pi.Time <= 60)
            {
                xPositions = Enumerable.Range(0, 11).Select(e => e * 60.0 * pi.Time / 10).ToArray();
                xLabels = xPositions.Select(x => x / 60 + " m").Reverse().ToArray(); // s=> min
            }
            else
            {
                xPositions = Enumerable.Range(0, 11).Select(e => e * 60.0 * pi.Time / 10).ToArray();
                xLabels = xPositions.Select(x => x / 3600 + " h").Reverse().ToArray();// s => hour
            }

            pi.FormPlot.Plot.XAxis.ManualTickPositions(xPositions, xLabels);
            pi.StartPlot();
        }
    }
}
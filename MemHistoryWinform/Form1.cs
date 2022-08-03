using System.Diagnostics;
using NLog;
using ScottPlot;
using Timer = System.Threading.Timer;
using ScottPlot.Plottable;

namespace MemHistoryWinform
{
    public partial class Form1 : Form
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly List<ProcessPlotInfo> _processPlotInfos = new List<ProcessPlotInfo>(10);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var ps1 = new ProcessPlotInfo("msedge", 120, formsPlot1);
            var ps2 = new ProcessPlotInfo("msedge", 12, formsPlot2);

            var ps3 = new ProcessPlotInfo("MemHistoryWinform", 120, formsPlot3);
            var ps4 = new ProcessPlotInfo("MemHistoryWinform", 12, formsPlot4);

            var ps5 = new ProcessPlotInfo("vivaldi", 120, formsPlot5);
            var ps6 = new ProcessPlotInfo("vivaldi", 12, formsPlot6);

            _processPlotInfos.Add(ps1);
            _processPlotInfos.Add(ps2);
            _processPlotInfos.Add(ps3);
            _processPlotInfos.Add(ps4);
            _processPlotInfos.Add(ps5);
            _processPlotInfos.Add(ps6);

            foreach (ProcessPlotInfo pi in _processPlotInfos)
            {
                pi.FormPlot.Configuration.DoubleClickBenchmark = false;
                //formsPlot1.Plot.SetAxisLimits(0, null, 0, null);
                //formsPlot1.Plot.SetOuterViewLimits(0, 60, 0, 9000);
                pi.FormPlot.MouseDoubleClick += formsPlot1_MouseDoubleClick;
                pi.FormPlot.Plot.Title($"{pi.Name} Memroy Chart");
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

        private void formsPlot1_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            foreach (var pi in _processPlotInfos)
            {
                pi.FormPlot.Plot.AxisAuto();
            }
        }
    }

    public class ProcessPlotInfo
    {
        public string Name { get; set; }

        public int Time { get; set; }

        public readonly double[] LiveData;

        public SignalPlot SignalPlot;

        public FormsPlot FormPlot;

        private double memoryUsed;

        private Timer updateMemoryTimer;
        private Timer refreshTimer;

        public ProcessPlotInfo(string processName, int time, FormsPlot formPlot)
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
                FormPlot.Invoke(() =>
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
                FormPlot.Invoke(() =>
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
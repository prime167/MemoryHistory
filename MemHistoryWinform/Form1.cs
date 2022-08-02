using System.Diagnostics;
using NLog;
using ScottPlot;
using Timer = System.Threading.Timer;
using ScottPlot.Plottable;

namespace MemHistoryWinform
{
    public partial class Form1 : Form
    {
        private Timer timer;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private List<string> _processList = new List<string>(5) { "msedge" };
        private ScatterPlotList<double> _plotTotal;
        private ScatterPlotList<double> _plotEdge;
        private DateTime _startTime;
        readonly double[] liveData = new double[600];

        public Form1()
        {
            InitializeComponent();
        }

        private void Callback(object obj)
        {
            //var ts = DateTime.Now - _startTime;
            //int seconds = (int)ts.TotalSeconds;
            //var memUsage = Util.GetMemoryUsage();
            //SystemMemUsage = memUsage;
            try
            {
                foreach (var pn in _processList)
                {
                    var ps = Process.GetProcessesByName(pn);
                    var total = 0.0;
                    foreach (var p in ps)
                    {
                        if (!p.HasExited)
                        {
                            p.Refresh();
                            var s = Math.Round(p.WorkingSet64 / 1024.0 / 1024.0, 2);
                            total += s;
                        }
                    }

                    total = Math.Round(total, 2);

                    // "scroll" the whole chart to the left
                    Array.Copy(liveData, 1, liveData, 0, liveData.Length - 1);
                    liveData[liveData.Length - 1] = total;
                    Invoke(() => 
                    {
                        formsPlot1.Plot.AxisAuto();
                        formsPlot1.Refresh();
                    });
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            formsPlot1.Configuration.DoubleClickBenchmark = false;
            //formsPlot1.Plot.SetAxisLimits(0, null, 0, null);
            //formsPlot1.Plot.SetOuterViewLimits(0, 60, 0, 9000);
            formsPlot1.MouseDoubleClick += formsPlot1_MouseDoubleClick;
            formsPlot1.Plot.Title("Edge Memroy Chart");
            formsPlot1.Plot.YLabel("Memory (MB)");
            formsPlot1.Plot.XLabel("Time (s)");

            var sig = formsPlot1.Plot.AddSignal(liveData);
            sig.FillBelow();

            // ·´×ªºá×ø±ê
            // 1) manually define X axis tick positions and labels
            //double[] xPositions = { 0, 100,200,300, 400,500,600 };
            //string[] xLabels = { "600 s","500 s","400 s", "300 s", "200 s", "100 s", "0 s" };
            //formsPlot1.Plot.XAxis.ManualTickPositions(xPositions, xLabels);
            
            // 2) auto
            sig.OffsetX = -600;
            formsPlot1.Plot.XAxis.TickLabelNotation(invertSign: true);
            sig.MarkerSize = 0;
            sig.MarkerShape = MarkerShape.filledCircle;

            timer = new Timer(Callback, null, 0, 1000);
            _startTime = DateTime.Now;
        }

        private void formsPlot1_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            formsPlot1.Plot.AxisAuto();
        }
    }
}
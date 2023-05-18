using System.Threading;
using Avalonia.Controls;
using ScottPlot.Plottable;
using ScottPlot;

namespace MemoryUsageAvalonia
{
    public partial class MainWindow : Window
    {
        private Timer _timer;

        /// <summary>
        /// 显示的时间范围
        /// </summary>
        private const int MaxPeriod = 60 * 60; // s

        private double _pageFileSize;
        private static readonly object Locker = new();
        private const int DataCount = 10;// 移动平均最近点数;

        public double[] Percentages = new double[MaxPeriod];
        public double[] Commits = new double[MaxPeriod];
        public double[] CommitsEma = new double[MaxPeriod];

        private ExponentialMovingAverageIndicator _ema;

        private SignalPlot _plotUsed;
        private SignalPlot _plotCurrentCommit;
        private SignalPlot _plotEma;// ema
        private double _commitPctTitle;

        public MemoryViewModel Vm = new();
        private Plot _pltUsed;
        private Plot _pltCommit;
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
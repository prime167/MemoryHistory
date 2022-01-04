using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows;
using Prism.Mvvm;
namespace MemoryHistory.Models;

public class ProcessMem:BindableBase
{
    private ObservableCollection<Point> _memCurve;
    public ObservableCollection<Point> MemCurve
    {
        get => _memCurve;
        set => SetProperty(ref _memCurve, value);
    }

    private string _bottomStr;
    public string BottomStr
    {
        get => _bottomStr;
        set => SetProperty(ref _bottomStr, value);
    }

    private string _processName;
    public string ProcessName
    {
        get => _processName;
        set => SetProperty(ref _processName, value);
    }

    private string _processDisplayName;
    public string ProcessDisplayName
    {
        get => _processDisplayName;
        set => SetProperty(ref _processDisplayName, value);
    }

    private Visibility _visibility;
    public Visibility Visibility
    {
        get => _visibility;
        set => SetProperty(ref _visibility, value);
    }

    public IMovingAverage MovingAverage;

    public int DecimalCount { get; set; } = 2;
}
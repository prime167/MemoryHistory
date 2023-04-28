using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoryUsage;

public partial class MemoryViewModel:ObservableObject
{
    [ObservableProperty] 
    private double _currentPercentage;

    [ObservableProperty]
    private double _minPercentage;

    [ObservableProperty]
    private double _maxPercentage;

    [ObservableProperty]
    private DateTime _minPercentageTime;

    [ObservableProperty]
    private DateTime _maxPercentageTime;

    [ObservableProperty]
    private string _minPercentageTimeStr;

    [ObservableProperty]
    private string _maxPercentageTimeStr;

    [ObservableProperty]
    private double _currentCommit;

    [ObservableProperty]
    private double _minCommit;

    [ObservableProperty]
    private double _maxCommit;

    [ObservableProperty]
    private DateTime _minCommitTime;

    [ObservableProperty]
    private DateTime _maxCommitTime;

    [ObservableProperty]
    private string _minCommitTimeStr;

    [ObservableProperty]
    private string _maxCommitTimeStr;
}
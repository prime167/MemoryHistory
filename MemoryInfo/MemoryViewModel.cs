using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoryInfo;

public partial class MemoryViewModel:ObservableObject
{
    [ObservableProperty] 
    private double? _currentPercentage;

    [ObservableProperty]
    private double? _minPercentage;

    [ObservableProperty]
    private double? _maxPercentage;

    [ObservableProperty]
    private double _currentSize;

    [ObservableProperty]
    private double? _minSize;

    [ObservableProperty]
    private double? _maxSize;

    [ObservableProperty]
    private DateTime _minPercentageTime;

    [ObservableProperty]
    private DateTime _maxPercentageTime;

    [ObservableProperty]
    private DateTime _currentSizeTime;

    [ObservableProperty]
    private DateTime _minSizeTime;

    [ObservableProperty]
    private DateTime _maxSizeTime;

    [ObservableProperty]
    private string? _currentSizeTimeStr;

    [ObservableProperty]
    private string? _minPercentageTimeStr;

    [ObservableProperty]
    private string? _maxPercentageTimeStr;

    [ObservableProperty]
    private string? _minSizeTimeStr;

    [ObservableProperty]
    private string? _maxSizeTimeStr;
}
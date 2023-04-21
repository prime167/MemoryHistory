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
    private string? _minPercentageTime;

    [ObservableProperty]
    private string? _maxPercentageTime;

    [ObservableProperty]
    private double? _currentSize;

    [ObservableProperty]
    private double? _minSize;

    [ObservableProperty]
    private double? _maxSize;

    [ObservableProperty]
    private string? _minSizeTime;

    [ObservableProperty]
    private string? _maxSizeTime;
}
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MemoryUsageAvalonia;

public partial class MemoryViewModel : ObservableObject
{
    [ObservableProperty]
    private double _currentUsedPct;

    [ObservableProperty]
    private double _minUsedPct;

    [ObservableProperty]
    private double _maxUsedPct;

    [ObservableProperty]
    private DateTime _minUsedPctTime;

    [ObservableProperty]
    private DateTime _maxUsedPctTime;

    [ObservableProperty]
    private string _minUsedPctTimeStr;

    [ObservableProperty]
    private string _maxUsedPctTimeStr;

    [ObservableProperty]
    private double _currentCommitPct;

    [ObservableProperty]
    private double _minCommitPct;

    [ObservableProperty]
    private double _maxCommitPct;

    [ObservableProperty]
    private DateTime _minCommitPctTime;

    [ObservableProperty]
    private DateTime _maxCommitPctTime;

    [ObservableProperty]
    private string _minCommitPctTimeStr;

    [ObservableProperty]
    private string _maxCommitPctTimeStr;

    [ObservableProperty]
    private string _commitDetailStr;

    [ObservableProperty]
    private string _pageFileDetailStr;
}
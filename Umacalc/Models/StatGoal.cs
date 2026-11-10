using Umacalc.ViewModels;

namespace Umacalc.Models;

public class StatGoal : ViewModelBase
{
    private bool _isSelected;
    private int _weight = 1;
    private int _targetStat = 1200;
    private int _startingStat = 100;
    private int _assignedTurns;
    private int _usedTurns;

    public StatType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string ImagePath => $"/Assets/{Name.ToLowerInvariant()}.png";
    public string ColorHex { get; set; } = "#3B82F6";
    public string LightColorHex { get; set; } = "#DBEAFE";
    public string SecondaryGain { get; set; } = string.Empty;
    public string SecondaryGainShort { get; set; } = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                NotifyProgressChanged();
            }
        }
    }

    public int Weight
    {
        get => _weight;
        set => SetProperty(ref _weight, Math.Clamp(value, 1, 10));
    }

    public int TargetStat
    {
        get => _targetStat;
        set => SetProperty(ref _targetStat, Math.Max(100, value));
    }

    public int StartingStat
    {
        get => _startingStat;
        set => SetProperty(ref _startingStat, Math.Clamp(value, 0, 1000));
    }

    public int AssignedTurns
    {
        get => _assignedTurns;
        set
        {
            if (SetProperty(ref _assignedTurns, Math.Max(0, value)))
            {
                NotifyProgressChanged();
            }
        }
    }

    public int UsedTurns
    {
        get => _usedTurns;
        set
        {
            if (SetProperty(ref _usedTurns, Math.Max(0, value)))
            {
                NotifyProgressChanged();
            }
        }
    }

    public int RemainingTurns => Math.Max(0, AssignedTurns - UsedTurns);

    public int ExcessTurns => Math.Max(0, UsedTurns - AssignedTurns);

    public bool IsCompleted => AssignedTurns > 0 && UsedTurns >= AssignedTurns;

    public double ProgressPercentage
    {
        get
        {
            if (AssignedTurns <= 0) return UsedTurns > 0 ? 100.0 : 0.0;
            return Math.Min(100.0, ((double)UsedTurns / AssignedTurns) * 100.0);
        }
    }

    public string ButtonTurnsDisplay
    {
        get
        {
            if (AssignedTurns <= 0)
            {
                return UsedTurns > 0 ? $"+{UsedTurns} Extra" : "0 Left";
            }

            if (UsedTurns < AssignedTurns)
            {
                return $"{RemainingTurns} Left";
            }
            else if (UsedTurns == AssignedTurns)
            {
                return "GOAL MET!";
            }
            else
            {
                return $"+{ExcessTurns} Bonus";
            }
        }
    }

    public string CompactTurnsDisplay
    {
        get
        {
            if (AssignedTurns <= 0)
            {
                return UsedTurns > 0 ? $"+{UsedTurns}" : "0";
            }

            if (UsedTurns < AssignedTurns)
            {
                return $"{RemainingTurns} Left";
            }
            else if (UsedTurns == AssignedTurns)
            {
                return "MAX";
            }
            else
            {
                return $"+{ExcessTurns}";
            }
        }
    }

    public string StatusDescription
    {
        get
        {
            if (!IsSelected)
            {
                return UsedTurns > 0 ? $"{UsedTurns} trained (Bonus)" : "Not Targeted";
            }

            if (AssignedTurns <= 0)
            {
                return UsedTurns > 0 ? $"{UsedTurns} trained (Bonus)" : "0 Turns Assigned";
            }

            if (UsedTurns < AssignedTurns)
            {
                return $"{RemainingTurns} left ({UsedTurns}/{AssignedTurns})";
            }
            else if (UsedTurns == AssignedTurns)
            {
                return $"Target Met ({UsedTurns}/{AssignedTurns})";
            }
            else
            {
                return $"+{ExcessTurns} Bonus ({UsedTurns}/{AssignedTurns})";
            }
        }
    }

    private void NotifyProgressChanged()
    {
        OnPropertyChanged(nameof(RemainingTurns));
        OnPropertyChanged(nameof(ExcessTurns));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(ProgressPercentage));
        OnPropertyChanged(nameof(ButtonTurnsDisplay));
        OnPropertyChanged(nameof(CompactTurnsDisplay));
        OnPropertyChanged(nameof(StatusDescription));
    }

    public void Reset()
    {
        UsedTurns = 0;
    }
}

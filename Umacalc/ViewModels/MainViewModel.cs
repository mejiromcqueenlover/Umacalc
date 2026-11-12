using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Umacalc.Models;

namespace Umacalc.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ScenarioData _selectedScenario;
    private int _trainableTurns = 60;
    private bool _isAlwaysOnTop = false;
    private bool _isCompactMode = false;
    private BuildPreset? _activePreset;

    private PlayerInventory? _playerInventory;
    private bool _isPlayerInventoryLoaded = false;
    private bool _showOnlyOwnedCards = true;
    private string _inventoryStatusText = "No umadump file loaded (showing meta defaults)";
    private string _inventoryLoadedFileName = string.Empty;

    private TrainedCharaInventory? _trainedCharaInventory;
    private bool _isTrainedCharaLoaded = false;
    private string _charaStatusText = "No trained_chara file loaded (showing general factor guide)";
    private string _charaLoadedFileName = string.Empty;

    public ObservableCollection<ScenarioData> Scenarios { get; }
    public ObservableCollection<StatGoal> StatGoals { get; }
    public ObservableCollection<TurnRecord> TurnHistory { get; }
    public ObservableCollection<BuildPreset> Presets { get; }

    public BuildPreset? ActivePreset
    {
        get => _activePreset;
        set => SetProperty(ref _activePreset, value);
    }

    public bool IsPlayerInventoryLoaded
    {
        get => _isPlayerInventoryLoaded;
        set => SetProperty(ref _isPlayerInventoryLoaded, value);
    }

    public bool ShowOnlyOwnedCards
    {
        get => _showOnlyOwnedCards;
        set
        {
            if (SetProperty(ref _showOnlyOwnedCards, value))
            {
                RefreshPresetCards();
            }
        }
    }

    public string InventoryStatusText
    {
        get => _inventoryStatusText;
        set => SetProperty(ref _inventoryStatusText, value);
    }

    public string InventoryLoadedFileName
    {
        get => _inventoryLoadedFileName;
        set => SetProperty(ref _inventoryLoadedFileName, value);
    }

    public bool IsTrainedCharaLoaded
    {
        get => _isTrainedCharaLoaded;
        set => SetProperty(ref _isTrainedCharaLoaded, value);
    }

    public string CharaStatusText
    {
        get => _charaStatusText;
        set => SetProperty(ref _charaStatusText, value);
    }

    public string CharaLoadedFileName
    {
        get => _charaLoadedFileName;
        set => SetProperty(ref _charaLoadedFileName, value);
    }

    private CareerPointPrediction? _careerPrediction;
    public CareerPointPrediction? CareerPrediction
    {
        get => _careerPrediction;
        set => SetProperty(ref _careerPrediction, value);
    }

    public ICommand TrainStatCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand ResetRunCommand { get; }
    public ICommand RecalculateCommand { get; }
    public ICommand SelectPresetCommand { get; }
    public ICommand AdjustAssignedTurnsCommand { get; }
    public ICommand AdjustTrainableTurnsCommand { get; }
    public ICommand AddQuickTurnsCommand { get; }
    public ICommand ToggleAlwaysOnTopCommand { get; }

    public ICommand LoadUmadumpFileCommand { get; }
    public ICommand ToggleOwnedOnlyCommand { get; }
    public ICommand ClearPlayerInventoryCommand { get; }

    public ICommand LoadTrainedCharaFileCommand { get; }
    public ICommand ClearTrainedCharaCommand { get; }

    public MainViewModel()
    {
        Scenarios = new ObservableCollection<ScenarioData>(ScenarioData.GetGlobalScenarios());
        _selectedScenario = Scenarios.First();
        _trainableTurns = _selectedScenario.DefaultTrainableTurns;

        StatGoals = new ObservableCollection<StatGoal>
        {
            new StatGoal
            {
                Type = StatType.Speed,
                Name = "Speed",
                ShortName = "SPD",
                ColorHex = "#2563EB",       // Vibrant Blue
                LightColorHex = "#DBEAFE",
                SecondaryGain = "+Power",
                SecondaryGainShort = "+POW",
                IsSelected = true,
                Weight = 1,
                TargetStat = 1200
            },
            new StatGoal
            {
                Type = StatType.Stamina,
                Name = "Stamina",
                ShortName = "STA",
                ColorHex = "#EA580C",       // Vibrant Orange
                LightColorHex = "#FFEDD5",
                SecondaryGain = "+Guts",
                SecondaryGainShort = "+GUT",
                IsSelected = false,
                Weight = 1,
                TargetStat = 800
            },
            new StatGoal
            {
                Type = StatType.Power,
                Name = "Power",
                ShortName = "POW",
                ColorHex = "#D97706",       // Amber/Gold
                LightColorHex = "#FEF3C7",
                SecondaryGain = "+Stamina",
                SecondaryGainShort = "+STA",
                IsSelected = true,
                Weight = 1,
                TargetStat = 1200
            },
            new StatGoal
            {
                Type = StatType.Guts,
                Name = "Guts",
                ShortName = "GUT",
                ColorHex = "#DB2777",       // Hot Pink/Magenta
                LightColorHex = "#FCE7F3",
                SecondaryGain = "+Speed & Power",
                SecondaryGainShort = "+SPD/POW",
                IsSelected = false,
                Weight = 1,
                TargetStat = 600
            },
            new StatGoal
            {
                Type = StatType.Wisdom,
                Name = "Wisdom",
                ShortName = "WIS",
                ColorHex = "#059669",       // Emerald Green
                LightColorHex = "#D1FAE5",
                SecondaryGain = "+Speed & Energy",
                SecondaryGainShort = "+SPD/ENG",
                IsSelected = true,
                Weight = 1,
                TargetStat = 1000
            }
        };

        foreach (var goal in StatGoals)
        {
            goal.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(StatGoal.IsSelected) ||
                    e.PropertyName == nameof(StatGoal.Weight))
                {
                    DistributeTurns();
                    NotifyOverviewProperties();
                }
            };
        }

        TurnHistory = new ObservableCollection<TurnRecord>();
        Presets = new ObservableCollection<BuildPreset>(BuildPreset.GetDefaultPresets());
        foreach (var p in Presets)
        {
            p.DefaultTrainingBonusCards = new List<SupportCardTier>(p.TrainingBonusCards);
            p.DefaultInspirationBonusCards = new List<SupportCardTier>(p.InspirationBonusCards);
        }
        _activePreset = Presets.FirstOrDefault(p => p.Id == "mile") ?? Presets.FirstOrDefault();

        TrainStatCommand = new RelayCommand(param =>
        {
            if (param is StatGoal goal)
            {
                TrainStat(goal);
            }
        });

        UndoCommand = new RelayCommand(_ => UndoLastAction(), _ => TurnHistory.Count > 0);
        ResetRunCommand = new RelayCommand(_ => ResetRun());
        RecalculateCommand = new RelayCommand(_ => DistributeTurns());
        SelectPresetCommand = new RelayCommand(param =>
        {
            if (param is BuildPreset preset)
            {
                ApplyPresetObject(preset);
            }
            else if (param is string str)
            {
                ApplyPreset(str);
            }
        });

        AdjustAssignedTurnsCommand = new RelayCommand(param =>
        {
            if (param is string pStr && pStr.Contains(':'))
            {
                var parts = pStr.Split(':');
                var statName = parts[0];
                var delta = int.Parse(parts[1]);
                var goal = StatGoals.FirstOrDefault(g => g.Name.Equals(statName, StringComparison.OrdinalIgnoreCase));
                if (goal != null)
                {
                    goal.AssignedTurns = Math.Max(0, goal.AssignedTurns + delta);
                    NotifyOverviewProperties();
                }
            }
        });

        AdjustTrainableTurnsCommand = new RelayCommand(param =>
        {
            if (param is string pStr && int.TryParse(pStr, out int delta))
            {
                TrainableTurns = Math.Max(1, TrainableTurns + delta);
            }
        });

        AddQuickTurnsCommand = new RelayCommand(param =>
        {
            int delta = 5;
            if (param is string pStr && int.TryParse(pStr, out int d))
            {
                delta = d;
            }
            TrainableTurns += delta;
        });

        ToggleAlwaysOnTopCommand = new RelayCommand(_ => IsAlwaysOnTop = !IsAlwaysOnTop);
        LoadUmadumpFileCommand = new RelayCommand(_ => LoadUmadumpFile());
        ToggleOwnedOnlyCommand = new RelayCommand(_ => ShowOnlyOwnedCards = !ShowOnlyOwnedCards);
        ClearPlayerInventoryCommand = new RelayCommand(_ => ClearPlayerInventory());

        LoadTrainedCharaFileCommand = new RelayCommand(_ => LoadTrainedCharaFile());
        ClearTrainedCharaCommand = new RelayCommand(_ => ClearTrainedChara());

        DistributeTurns();
        RefreshPresetCards();
    }

    public ScenarioData SelectedScenario
    {
        get => _selectedScenario;
        set
        {
            if (SetProperty(ref _selectedScenario, value))
            {
                TrainableTurns = value.DefaultTrainableTurns;
                foreach (var goal in StatGoals)
                {
                    goal.TargetStat = Math.Min(goal.TargetStat, value.StatCap);
                }
                DistributeTurns();
                OnPropertyChanged(nameof(ScenarioTip));
                OnPropertyChanged(nameof(ScenarioDescription));
            }
        }
    }

    public string ScenarioTip => SelectedScenario.Tips;
    public string ScenarioDescription => SelectedScenario.Description;

    public int TrainableTurns
    {
        get => _trainableTurns;
        set
        {
            if (SetProperty(ref _trainableTurns, Math.Max(1, value)))
            {
                DistributeTurns();
                NotifyOverviewProperties();
            }
        }
    }

    public int TotalTurnsTrained => StatGoals.Sum(g => g.UsedTurns);
    public int TrainableTurnsRemaining => Math.Max(0, TrainableTurns - TotalTurnsTrained);
    public int ExcessTurnsTrained => Math.Max(0, TotalTurnsTrained - TrainableTurns);

    public double TrainableProgressPercentage
    {
        get
        {
            if (TrainableTurns <= 0) return 0;
            return Math.Min(100.0, ((double)TotalTurnsTrained / TrainableTurns) * 100.0);
        }
    }

    public bool IsAlwaysOnTop
    {
        get => _isAlwaysOnTop;
        set
        {
            if (SetProperty(ref _isAlwaysOnTop, value))
            {
                OnPropertyChanged(nameof(PinButtonText));
            }
        }
    }

    public string PinButtonText => IsAlwaysOnTop ? "Pinned" : "Pin Window";

    public bool IsCompactMode
    {
        get => _isCompactMode;
        set => SetProperty(ref _isCompactMode, value);
    }

    public string RecommendedNextStat
    {
        get
        {
            var selected = StatGoals.Where(g => g.IsSelected && g.RemainingTurns > 0).ToList();
            if (!selected.Any())
            {
                var anySelected = StatGoals.Where(g => g.IsSelected).ToList();
                if (anySelected.Any())
                {
                    return "All targets achieved! Bonus turns can be trained freely.";
                }
                return "Select target stats to allocate your training budget.";
            }
            var priority = selected.OrderByDescending(g => g.RemainingTurns).First();
            return $"Focus: {priority.Name} ({priority.RemainingTurns} turns left)";
        }
    }

    public string StatusSummary
    {
        get
        {
            if (ExcessTurnsTrained > 0)
            {
                return $"{TotalTurnsTrained} / {TrainableTurns} Trained • +{ExcessTurnsTrained} Bonus Turns";
            }
            return $"{TotalTurnsTrained} / {TrainableTurns} Trained • {TrainableTurnsRemaining} turns left";
        }
    }

    public void DistributeTurns()
    {
        var selectedGoals = StatGoals.Where(g => g.IsSelected).ToList();
        if (!selectedGoals.Any())
        {
            foreach (var g in StatGoals) g.AssignedTurns = 0;
            NotifyOverviewProperties();
            return;
        }

        int totalWeight = selectedGoals.Sum(g => g.Weight);
        if (totalWeight <= 0) totalWeight = 1;

        int turnsToAssign = TrainableTurns;

        foreach (var g in StatGoals)
        {
            if (!g.IsSelected)
            {
                g.AssignedTurns = 0;
            }
        }

        int assignedSum = 0;
        for (int i = 0; i < selectedGoals.Count; i++)
        {
            var goal = selectedGoals[i];
            if (i == selectedGoals.Count - 1)
            {
                goal.AssignedTurns = Math.Max(0, turnsToAssign - assignedSum);
            }
            else
            {
                int share = (int)Math.Round((double)turnsToAssign * goal.Weight / totalWeight);
                goal.AssignedTurns = share;
                assignedSum += share;
            }
        }

        NotifyOverviewProperties();
    }

    private void TrainStat(StatGoal goal)
    {
        // If the player trains past the initial planned budget, smoothly expand the budget
        // so they never get stuck with 0 turns!
        if (TrainableTurnsRemaining <= 0)
        {
            _trainableTurns++;
            if (goal.IsSelected)
            {
                goal.AssignedTurns++;
            }
            OnPropertyChanged(nameof(TrainableTurns));
        }

        goal.UsedTurns++;

        var record = new TurnRecord
        {
            TurnIndex = TotalTurnsTrained,
            Stat = goal.Type,
            StatName = goal.Name,
            ColorHex = goal.ColorHex,
            Timestamp = DateTime.Now,
            Note = $"{goal.StatusDescription}"
        };

        TurnHistory.Insert(0, record);
        NotifyOverviewProperties();
    }

    private void UndoLastAction()
    {
        if (TurnHistory.Count == 0) return;

        var last = TurnHistory[0];
        TurnHistory.RemoveAt(0);

        var goal = StatGoals.FirstOrDefault(g => g.Type == last.Stat);
        if (goal != null && goal.UsedTurns > 0)
        {
            goal.UsedTurns--;
        }

        NotifyOverviewProperties();
    }

    private void ResetRun()
    {
        TurnHistory.Clear();

        foreach (var goal in StatGoals)
        {
            goal.Reset();
        }

        NotifyOverviewProperties();
    }

    public void ApplyPresetObject(BuildPreset preset)
    {
        ActivePreset = preset;

        if (_playerInventory != null && ShowOnlyOwnedCards)
        {
            preset.ApplyPlayerInventory(_playerInventory);
        }
        else
        {
            preset.RevertToDefaultCards();
        }

        if (_trainedCharaInventory != null && _trainedCharaInventory.IsLoaded)
        {
            preset.ApplyTrainedCharas(_trainedCharaInventory);
        }
        else
        {
            preset.RevertToDefaultLegacies();
        }

        foreach (var g in StatGoals)
        {
            if (preset.Weights.TryGetValue(g.Type, out int weight))
            {
                g.IsSelected = true;
                g.Weight = weight;
            }
            else
            {
                g.IsSelected = false;
                g.Weight = 1;
            }
        }

        DistributeTurns();

        CareerPrediction = CareerPointPrediction.Calculate(
            preset,
            SelectedScenario,
            preset.TrainingBonusCards.Concat(preset.InspirationBonusCards),
            preset.RecommendedLegacies);
        OnPropertyChanged(nameof(CareerPrediction));
    }

    public void LoadUmadumpFile(string? path = null)
    {
        string? selectedFile = path;
        if (string.IsNullOrEmpty(selectedFile))
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select support_card_data.json from umadump",
                Filter = "Umadump JSON (*support_card_data*.json;*.json)|*support_card_data*.json;*.json|All Files (*.*)|*.*",
                FileName = "support_card_data.json"
            };

            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (Directory.Exists(downloads))
            {
                dialog.InitialDirectory = downloads;
            }

            if (dialog.ShowDialog() == true)
            {
                selectedFile = dialog.FileName;
            }
        }

        if (!string.IsNullOrEmpty(selectedFile) && File.Exists(selectedFile))
        {
            var inv = PlayerInventory.LoadFromFile(selectedFile);
            if (inv != null && inv.TotalCards > 0)
            {
                _playerInventory = inv;
                IsPlayerInventoryLoaded = true;
                ShowOnlyOwnedCards = true;
                InventoryLoadedFileName = inv.FileName;
                InventoryStatusText = $"{inv.FileName}: {inv.SummaryText}";
                RefreshPresetCards();
            }
        }
    }

    public void ClearPlayerInventory()
    {
        _playerInventory = null;
        IsPlayerInventoryLoaded = false;
        InventoryLoadedFileName = string.Empty;
        InventoryStatusText = "No umadump file loaded (showing meta defaults)";
        RefreshPresetCards();
    }

    public void RefreshPresetCards()
    {
        foreach (var p in Presets)
        {
            if (_playerInventory != null && ShowOnlyOwnedCards)
            {
                p.ApplyPlayerInventory(_playerInventory);
            }
            else
            {
                p.RevertToDefaultCards();
            }
        }

        if (ActivePreset != null)
        {
            if (_playerInventory != null && ShowOnlyOwnedCards)
            {
                ActivePreset.ApplyPlayerInventory(_playerInventory);
            }
            else
            {
                ActivePreset.RevertToDefaultCards();
            }

            if (_trainedCharaInventory != null && _trainedCharaInventory.IsLoaded)
            {
                ActivePreset.ApplyTrainedCharas(_trainedCharaInventory);
            }
            else
            {
                ActivePreset.RevertToDefaultLegacies();
            }

            // Recalculate Career Prediction
            CareerPrediction = CareerPointPrediction.Calculate(
                ActivePreset,
                SelectedScenario,
                ActivePreset.TrainingBonusCards.Concat(ActivePreset.InspirationBonusCards),
                ActivePreset.RecommendedLegacies);

            OnPropertyChanged(nameof(ActivePreset));
            OnPropertyChanged(nameof(CareerPrediction));
        }
    }

    public void LoadTrainedCharaFile(string? path = null)
    {
        string? selectedFile = path;
        if (string.IsNullOrEmpty(selectedFile))
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select trained_chara_data.json from umadump",
                Filter = "Umadump Trained Chara JSON (*trained_chara*.json;*.json)|*trained_chara*.json;*.json|All Files (*.*)|*.*",
                FileName = "trained_chara_data.json"
            };

            string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (Directory.Exists(downloads))
            {
                dialog.InitialDirectory = downloads;
            }

            if (dialog.ShowDialog() == true)
            {
                selectedFile = dialog.FileName;
            }
        }

        if (!string.IsNullOrEmpty(selectedFile) && File.Exists(selectedFile))
        {
            var inv = TrainedCharaInventory.Instance;
            if (inv.LoadFromFile(selectedFile))
            {
                _trainedCharaInventory = inv;
                IsTrainedCharaLoaded = true;
                CharaLoadedFileName = inv.LoadedFileName ?? "trained_chara_data.json";
                CharaStatusText = $"{CharaLoadedFileName}: {inv.Characters.Count} Trained Uma Legacies Loaded";
                RefreshPresetCards();
            }
        }
    }

    public void ClearTrainedChara()
    {
        if (_trainedCharaInventory != null)
        {
            _trainedCharaInventory.Clear();
            _trainedCharaInventory = null;
        }
        IsTrainedCharaLoaded = false;
        CharaLoadedFileName = string.Empty;
        CharaStatusText = "No trained_chara file loaded (showing general factor guide)";
        RefreshPresetCards();
    }

    private void ApplyPreset(string? presetId)
    {
        if (string.IsNullOrEmpty(presetId)) return;

        var found = Presets.FirstOrDefault(p => p.Id.Equals(presetId, StringComparison.OrdinalIgnoreCase));
        if (found != null)
        {
            ApplyPresetObject(found);
            return;
        }

        // Fallback for legacy short codes
        switch (presetId.ToLowerInvariant())
        {
            case "spw":
                ApplyPreset("mile");
                break;
            case "ssp":
                ApplyPreset("long");
                break;
            case "sw":
                ApplyPreset("sw");
                break;
            case "sp":
                ApplyPreset("sp");
                break;
            case "guts_focus":
                ApplyPreset("guts_meta");
                break;
            case "all":
                ApplyPreset("factor_farm");
                break;
        }
    }

    private void SetStat(StatType type, bool isSelected, int weight = 1)
    {
        var goal = StatGoals.FirstOrDefault(g => g.Type == type);
        if (goal != null)
        {
            goal.IsSelected = isSelected;
            goal.Weight = weight;
        }
    }

    private void NotifyOverviewProperties()
    {
        OnPropertyChanged(nameof(TotalTurnsTrained));
        OnPropertyChanged(nameof(TrainableTurnsRemaining));
        OnPropertyChanged(nameof(ExcessTurnsTrained));
        OnPropertyChanged(nameof(TrainableProgressPercentage));
        OnPropertyChanged(nameof(RecommendedNextStat));
        OnPropertyChanged(nameof(StatusSummary));
    }
}

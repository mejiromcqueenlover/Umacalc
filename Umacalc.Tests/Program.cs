using System.Diagnostics;
using System.IO;
using Umacalc.Models;
using Umacalc.ViewModels;

namespace Umacalc.Tests;

class Program
{
    static int _passed = 0;
    static int _failed = 0;

    [STAThread]
    static void Main()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("      UMACALC TRAINABLE TURN TESTS     ");
        Console.WriteLine("========================================\n");

        string assetsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Umacalc\Assets"));
        Console.WriteLine($"--> Generating stock icon assets into: {assetsDir}");
        AssetGenerator.GenerateAll(assetsDir);
        Console.WriteLine("[PASS] Stock icon assets generated successfully.\n");

        TestScenariosData();
        TestTurnDistribution();
        TestPresets();
        TestStatClickTraining();
        TestUndo();
        TestReset();
        TestBudgetAdjustment();
        TestOverTrainingAndQuickAdd();

        Console.WriteLine("\n========================================");
        Console.WriteLine($"RESULTS: {_passed} Passed, {_failed} Failed");
        Console.WriteLine("========================================");

        if (_failed > 0)
        {
            Environment.Exit(1);
        }
    }

    static void Assert(bool condition, string testName)
    {
        if (condition)
        {
            _passed++;
            Console.WriteLine($"[PASS] {testName}");
        }
        else
        {
            _failed++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[FAIL] {testName}");
            Console.ResetColor();
        }
    }

    static void TestScenariosData()
    {
        Console.WriteLine("--> Testing Scenarios Data (Trainable Turns Only)");
        var scenarios = ScenarioData.GetGlobalScenarios();

        Assert(scenarios.Count >= 7, "Supports all major Global scenarios + Custom");
        Assert(scenarios.Any(s => s.Id == "ura" && s.DefaultTrainableTurns == 60), "URA Finals has 60 trainable turns");
        Assert(scenarios.Any(s => s.Id == "aoharu" && s.DefaultTrainableTurns == 56), "Aoharu Cup has 56 trainable turns");
        Assert(scenarios.Any(s => s.Id == "mant" && s.DefaultTrainableTurns == 50), "MANT / Climax has 50 trainable turns");
        Assert(scenarios.Any(s => s.Id == "grandlive" && s.DefaultTrainableTurns == 64), "Our Grand Live has 64 trainable turns");
        Assert(scenarios.Any(s => s.Id == "grandmasters" && s.DefaultTrainableTurns == 63), "Grand Masters has 63 trainable turns");
        Assert(scenarios.Any(s => s.Id == "larc" && s.DefaultTrainableTurns == 60), "Project L'Arc has 60 trainable turns");
        Assert(scenarios.Any(s => s.Id == "uaf" && s.DefaultTrainableTurns == 62), "U.A.F. Ready GO! has 62 trainable turns");
    }

    static void TestTurnDistribution()
    {
        Console.WriteLine("\n--> Testing Turn Budget Distribution");
        var vm = new MainViewModel();

        int sumAssigned = vm.StatGoals.Sum(g => g.AssignedTurns);
        Assert(sumAssigned == vm.TrainableTurns, $"Sum of assigned turns ({sumAssigned}) matches TrainableTurns ({vm.TrainableTurns})");

        var unselected = vm.StatGoals.Where(g => !g.IsSelected).ToList();
        Assert(unselected.All(g => g.AssignedTurns == 0), "Unselected stats receive 0 assigned turns");

        var selected = vm.StatGoals.Where(g => g.IsSelected).ToList();
        Assert(selected.All(g => g.AssignedTurns > 0), "Selected stats receive positive assigned turns");
    }

    static void TestPresets()
    {
        Console.WriteLine("\n--> Testing Build Presets");
        var vm = new MainViewModel();

        var presets = BuildPreset.GetDefaultPresets();
        Assert(presets.Count >= 15, $"Comprehensive preset library has {presets.Count} presets (>=15)");

        // Test Long distance preset
        vm.SelectPresetCommand.Execute("long");
        var spd = vm.StatGoals.First(g => g.Type == StatType.Speed);
        var sta = vm.StatGoals.First(g => g.Type == StatType.Stamina);
        var pow = vm.StatGoals.First(g => g.Type == StatType.Power);
        var gut = vm.StatGoals.First(g => g.Type == StatType.Guts);
        var wis = vm.StatGoals.First(g => g.Type == StatType.Wisdom);

        Assert(spd.IsSelected && sta.IsSelected && pow.IsSelected, "Long preset checks Speed, Stamina, Power");
        Assert(sta.Weight == 3, "Long distance prioritizes Stamina weight to 3x");
        Assert(!gut.IsSelected && !wis.IsSelected, "Long preset leaves Guts and Wisdom unchecked");
        Assert(vm.StatGoals.Sum(g => g.AssignedTurns) == vm.TrainableTurns, "Assigned turns sum equals TrainableTurns");

        // Test Chaser (Oikomi) preset
        vm.SelectPresetCommand.Execute("chaser");
        Assert(pow.IsSelected && pow.Weight == 3, "Chaser preset prioritizes Power weight to 3x");
        Assert(spd.IsSelected && sta.IsSelected, "Chaser preset checks Speed and Stamina");

        // Test Factor Farm preset
        vm.SelectPresetCommand.Execute("factor_farm");
        Assert(vm.StatGoals.All(g => g.IsSelected), "Factor Farm checks all 5 stats for 3-star Blue Factors");

        // Test Support Card Speciality Recommendations & Bonus Tiers
        foreach (var p in presets)
        {
            Assert(!string.IsNullOrEmpty(p.DeckRecommendation), $"Preset {p.Name} has DeckRecommendation");
            Assert(!string.IsNullOrEmpty(p.KeyCardsSuggestion), $"Preset {p.Name} has KeyCardsSuggestion");
            Assert(!string.IsNullOrEmpty(p.InheritanceTip), $"Preset {p.Name} has InheritanceTip");
            int totalDeckCards = p.DeckComposition.Sum(s => s.Count);
            Assert(totalDeckCards == 6, $"Preset {p.Name} deck composition totals 6 cards ({totalDeckCards}/6)");

            // Validate Training Bonus and Inspiration Bonus support card lists
            Assert(p.TrainingBonusCards.Count >= 3, $"Preset {p.Name} has {p.TrainingBonusCards.Count} Training Bonus cards (>=3)");
            Assert(p.InspirationBonusCards.Count >= 2, $"Preset {p.Name} has {p.InspirationBonusCards.Count} Inspiration Bonus cards (>=2)");
            Assert(p.TrainingBonusCards.All(c => !string.IsNullOrEmpty(c.CardName) && !string.IsNullOrEmpty(c.BonusStat) && !string.IsNullOrEmpty(c.SkillOrReason)), $"Preset {p.Name} Training Bonus cards have valid details");
            Assert(p.InspirationBonusCards.All(c => !string.IsNullOrEmpty(c.CardName) && !string.IsNullOrEmpty(c.BonusStat) && !string.IsNullOrEmpty(c.SkillOrReason)), $"Preset {p.Name} Inspiration Bonus cards have valid details");
        }

        // Test ActivePreset updates on preset execution
        vm.SelectPresetCommand.Execute("sprint");
        Assert(vm.ActivePreset != null && vm.ActivePreset.Id == "sprint", "ActivePreset updated to Sprint");
        Assert(vm.ActivePreset!.DeckRecommendation.Contains("4 Speed"), "ActivePreset contains 4 Speed recommendation");
        Assert(vm.ActivePreset.TrainingBonusCards.Any(c => c.CardName.Contains("Biko Pegasus")), "Sprint preset includes Biko Pegasus (+20% Training Effect)");
        Assert(vm.ActivePreset.InspirationBonusCards.Any(c => c.CardName.Contains("Taiki Shuttle")), "Sprint preset includes Taiki Shuttle (Hint Lv 3)");
    }

    static void TestStatClickTraining()
    {
        Console.WriteLine("\n--> Testing Interactive Stat Training Clicks");
        var vm = new MainViewModel();
        vm.ResetRunCommand.Execute(null);

        var spd = vm.StatGoals.First(g => g.Type == StatType.Speed);
        int initialRemaining = spd.RemainingTurns;
        int initialTotalRemaining = vm.TrainableTurnsRemaining;

        vm.TrainStatCommand.Execute(spd);

        Assert(spd.UsedTurns == 1, "Speed used turns is 1");
        Assert(spd.RemainingTurns == initialRemaining - 1, $"Speed remaining turns decremented from {initialRemaining} to {initialRemaining - 1}");
        Assert(vm.TotalTurnsTrained == 1, "Total turns trained is 1");
        Assert(vm.TrainableTurnsRemaining == initialTotalRemaining - 1, "Trainable turns remaining decremented by 1");
        Assert(vm.TurnHistory.Count == 1, "Turn history recorded the event");
    }

    static void TestUndo()
    {
        Console.WriteLine("\n--> Testing Undo Functionality");
        var vm = new MainViewModel();
        vm.ResetRunCommand.Execute(null);

        var pow = vm.StatGoals.First(g => g.Type == StatType.Power);
        vm.TrainStatCommand.Execute(pow);
        Assert(pow.UsedTurns == 1, "Power trained once");

        vm.UndoCommand.Execute(null);
        Assert(pow.UsedTurns == 0, "Power trained reverted to 0");
        Assert(vm.TotalTurnsTrained == 0, "Total turns trained reverted to 0");
        Assert(vm.TurnHistory.Count == 0, "History cleared by undo");
    }

    static void TestReset()
    {
        Console.WriteLine("\n--> Testing Reset Functionality");
        var vm = new MainViewModel();
        var spd = vm.StatGoals.First(g => g.Type == StatType.Speed);
        vm.TrainStatCommand.Execute(spd);
        vm.TrainStatCommand.Execute(spd);

        Assert(vm.TotalTurnsTrained == 2, "2 turns trained before reset");
        vm.ResetRunCommand.Execute(null);
        Assert(vm.TotalTurnsTrained == 0, "0 turns trained after reset");
        Assert(spd.UsedTurns == 0, "Speed used turns is 0 after reset");
        Assert(vm.TurnHistory.Count == 0, "Turn history is empty after reset");
    }

    static void TestBudgetAdjustment()
    {
        Console.WriteLine("\n--> Testing Budget Adjustments");
        var vm = new MainViewModel();
        int initialBudget = vm.TrainableTurns;

        vm.AdjustTrainableTurnsCommand.Execute("1");
        Assert(vm.TrainableTurns == initialBudget + 1, "Budget increased by 1");

        vm.AdjustTrainableTurnsCommand.Execute("-1");
        Assert(vm.TrainableTurns == initialBudget, "Budget restored by -1");
    }

    static void TestOverTrainingAndQuickAdd()
    {
        Console.WriteLine("\n--> Testing Over-Training & Quick Add");
        var vm = new MainViewModel();
        vm.TrainableTurns = 1; // set small budget
        vm.SelectPresetCommand.Execute("sw"); // Speed & Wisdom

        var spd = vm.StatGoals.First(g => g.Type == StatType.Speed);
        // Train speed twice (exceeding initial 1 turn)
        vm.TrainStatCommand.Execute(spd);
        Assert(spd.ButtonTurnsDisplay == "GOAL MET!" || spd.RemainingTurns == 0, "First train meets goal");

        // Train speed again (over-training)
        vm.TrainStatCommand.Execute(spd);
        Assert(spd.UsedTurns == 2, "Allows training past budget without getting stuck");
        Assert(vm.TrainableTurns >= 2, "Auto-expanded budget to accommodate training");

        // Quick Add +5
        int beforeQuick = vm.TrainableTurns;
        vm.AddQuickTurnsCommand.Execute("5");
        Assert(vm.TrainableTurns == beforeQuick + 5, "Quick add +5 turns expanded budget immediately");
    }
}

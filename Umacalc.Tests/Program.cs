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
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "dump_master_schema")
        {
            DumpMasterDb();
            return;
        }

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
        TestUmadumpSupportCards();

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

    static void TestUmadumpSupportCards()
    {
        Console.WriteLine("\n--> Testing Umadump Support Card Integration");

        // 1. Catalog Test
        var catalog = SupportCardCatalog.Instance;
        Assert(catalog.AllCards.Count >= 200, $"SupportCardCatalog loaded {catalog.AllCards.Count} cards (>= 200)");
        
        var kitasan = catalog.GetCard(30028);
        Assert(kitasan != null, "Catalog found Kitasan Black SSR (ID 30028)");
        Assert(kitasan?.Speciality == "SPD", "Kitasan Black speciality is SPD");
        Assert(kitasan?.GetTrainingBonus(0) == 10, "Kitasan Black 0LB has +10% Training Bonus");
        Assert(kitasan?.GetTrainingBonus(4) == 15, "Kitasan Black 4LB has +15% Training Bonus");

        var biko = catalog.GetCard(30020);
        Assert(biko != null, "Catalog found Biko Pegasus SSR (ID 30020)");
        Assert(biko?.GetTrainingBonus(4) == 20, "Biko Pegasus 4LB has +20% Training Bonus");

        // 2. Player Inventory Load Test
        string dumpPath = @"C:\Users\mejiromcqueenlover\Downloads\support_card_data.json";
        if (File.Exists(dumpPath))
        {
            var inv = PlayerInventory.LoadFromFile(dumpPath);
            Assert(inv != null, "PlayerInventory.LoadFromFile parsed support_card_data.json successfully");
            Assert(inv?.TotalCards == 166, $"Player inventory has 166 cards (actual: {inv?.TotalCards})");
            Assert(inv?.SsrCount == 37, $"Player inventory has 37 SSR cards (actual: {inv?.SsrCount})");
            Assert(inv?.SrCount == 45, $"Player inventory has 45 SR cards (actual: {inv?.SrCount})");
            Assert(inv?.RCount == 84, $"Player inventory has 84 R cards (actual: {inv?.RCount})");

            // 3. Preset Evaluation Test
            var presets = BuildPreset.GetDefaultPresets();
            var milePreset = presets.First(p => p.Id == "mile");
            
            var ownedTraining = inv!.EvaluateBestTrainingCards(milePreset, 4);
            Assert(ownedTraining.Count == 4, $"Evaluated {ownedTraining.Count} training cards for Mile from player inventory");
            foreach (var card in ownedTraining)
            {
                Assert(card.IsPlayerOwned, $"Card '{card.CardName}' is marked as player owned");
                Assert(!string.IsNullOrEmpty(card.OwnershipBadge), $"Card '{card.CardName}' has ownership badge: {card.OwnershipBadge}");
                Assert(card.BonusStat.Contains("% Training Effect"), $"Card '{card.CardName}' has training bonus stat: {card.BonusStat}");
                Assert(card.SkillOrReason.Contains("mile", StringComparison.OrdinalIgnoreCase) || card.SkillOrReason.Contains("speed", StringComparison.OrdinalIgnoreCase) || card.SkillOrReason.Contains("power", StringComparison.OrdinalIgnoreCase), $"Mile card '{card.CardName}' has mile-tailored reason: {card.SkillOrReason}");
            }

            var ownedInspiration = inv.EvaluateBestInspirationCards(milePreset, 3);
            Assert(ownedInspiration.Count == 3, $"Evaluated {ownedInspiration.Count} inspiration cards for Mile from player inventory");
            foreach (var card in ownedInspiration)
            {
                Assert(card.IsPlayerOwned, $"Card '{card.CardName}' is marked as player owned");
                Assert(!string.IsNullOrEmpty(card.OwnershipBadge), $"Card '{card.CardName}' has ownership badge: {card.OwnershipBadge}");
                Assert(card.BonusStat.Contains("Hint Lv"), $"Card '{card.CardName}' has hint bonus stat: {card.BonusStat}");
            }

            // Test Long preset tailored cards
            var longPreset = presets.First(p => p.Id == "long");
            var longTraining = inv.EvaluateBestTrainingCards(longPreset, 4);
            Assert(longTraining.Any(c => c.Speciality == "STA"), "Long distance preset recommends Stamina (STA) cards from player inventory");
            Assert(longTraining.Any(c => c.Speciality == "SPD"), "Long distance preset recommends Speed (SPD) cards from player inventory");
            Assert(longTraining.All(c => c.Speciality is "STA" or "SPD" or "PAL" or "POW"), "Long distance cards only use long-compatible specialities");
            Assert(longTraining.First(c => c.Speciality == "STA").SkillOrReason.Contains("Long distance", StringComparison.OrdinalIgnoreCase) || longTraining.First(c => c.Speciality == "STA").SkillOrReason.Contains("marathon", StringComparison.OrdinalIgnoreCase), "Long training card has long-distance tailored reason");

            // Test Sprint preset tailored cards
            var sprintPreset = presets.First(p => p.Id == "sprint");
            var sprintTraining = inv.EvaluateBestTrainingCards(sprintPreset, 4);
            Assert(sprintTraining.All(c => c.Speciality != "STA"), "Sprint preset strictly excludes Stamina cards");
            Assert(sprintTraining.Any(c => c.Speciality == "SPD"), "Sprint preset prioritizes Speed cards");
            Assert(sprintTraining.First(c => c.Speciality == "SPD").SkillOrReason.Contains("sprint", StringComparison.OrdinalIgnoreCase), "Sprint training card has sprint-tailored reason");

            // Test Front-Runner preset tailored cards
            var frontRunnerPreset = presets.First(p => p.Id is "runner" or "front_runner");
            var frontTraining = inv.EvaluateBestTrainingCards(frontRunnerPreset, 4);
            Assert(frontTraining.Any(c => c.Speciality is "SPD" or "WIS"), "Front runner recommends Speed & Wisdom");
            Assert(frontTraining.Any(c => c.SkillOrReason.Contains("Front-Runner", StringComparison.OrdinalIgnoreCase) || c.SkillOrReason.Contains("rail", StringComparison.OrdinalIgnoreCase)), "Front runner training cards have front-runner tailored reasons");

            // Test Chaser preset tailored cards
            var chaserPreset = presets.First(p => p.Id is "chaser");
            var chaserTraining = inv.EvaluateBestTrainingCards(chaserPreset, 4);
            Assert(chaserTraining.Any(c => c.Speciality == "POW"), "Chaser recommends Power cards");
            Assert(chaserTraining.Any(c => c.SkillOrReason.Contains("Chaser", StringComparison.OrdinalIgnoreCase) || c.SkillOrReason.Contains("overtake", StringComparison.OrdinalIgnoreCase) || c.SkillOrReason.Contains("spurt", StringComparison.OrdinalIgnoreCase)), "Chaser training cards have chaser-tailored reasons");

            // 4. MainViewModel Integration Test
            var vm = new MainViewModel();
            Assert(!vm.IsPlayerInventoryLoaded, "VM starts with IsPlayerInventoryLoaded = false");
            
            vm.LoadUmadumpFile(dumpPath);
            Assert(vm.IsPlayerInventoryLoaded, "VM has IsPlayerInventoryLoaded = true after loading");
            Assert(vm.InventoryLoadedFileName == "support_card_data.json", $"VM loaded file name is {vm.InventoryLoadedFileName}");
            Assert(vm.ActivePreset?.TrainingBonusCards.First().IsPlayerOwned == true, "ActivePreset training cards are now from player inventory");
            Assert(!string.IsNullOrEmpty(vm.ActivePreset?.TrainingBonusCards.First().OwnershipBadge), "ActivePreset card shows player LB badge");

            // Toggle back to meta
            vm.ToggleOwnedOnlyCommand.Execute(null);
            Assert(!vm.ShowOnlyOwnedCards, "ShowOnlyOwnedCards toggled to false");
            Assert(vm.ActivePreset?.TrainingBonusCards.First().IsPlayerOwned == false, "ActivePreset reverted to meta default cards");

            // Toggle back to owned
            vm.ToggleOwnedOnlyCommand.Execute(null);
            Assert(vm.ShowOnlyOwnedCards, "ShowOnlyOwnedCards toggled back to true");
            Assert(vm.ActivePreset?.TrainingBonusCards.First().IsPlayerOwned == true, "ActivePreset restored player cards");

            // Clear inventory
            vm.ClearPlayerInventoryCommand.Execute(null);
            Assert(!vm.IsPlayerInventoryLoaded, "IsPlayerInventoryLoaded cleared");
            Assert(vm.ActivePreset?.TrainingBonusCards.First().IsPlayerOwned == false, "ActivePreset reverted to meta after clear");
        }
    }

    static void DumpMasterDb()
    {
        string dbPath = @"C:\Program Files (x86)\Steam\steamapps\common\UmamusumePrettyDerby\UmamusumePrettyDerby_Data\Persistent\master\master.mdb";
        if (!File.Exists(dbPath))
        {
            Console.WriteLine($"DB not found: {dbPath}");
            return;
        }

        using var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
        conn.Open();

        // 1. Load Unique Effects
        var uniqueEffects = new Dictionary<int, (int type0, int val0, int type1, int val1, int unlockLv)>();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT id, type_0, value_0, type_1, value_1, lv FROM support_card_unique_effect";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                int id = r.GetInt32(0);
                int t0 = r.IsDBNull(1) ? 0 : r.GetInt32(1);
                int v0 = r.IsDBNull(2) ? 0 : r.GetInt32(2);
                int t1 = r.IsDBNull(3) ? 0 : r.GetInt32(3);
                int v1 = r.IsDBNull(4) ? 0 : r.GetInt32(4);
                int lv = r.IsDBNull(5) ? 0 : r.GetInt32(5);
                uniqueEffects[id] = (t0, v0, t1, v1, lv);
            }
        }

        // 2. Load Effect Tables (key: (table_id, type) -> array of values for init, limit_lv5..50)
        // Levels: 0=init, 1=lv5, 2=lv10, 3=lv15, 4=lv20, 5=lv25, 6=lv30, 7=lv35, 8=lv40, 9=lv45, 10=lv50
        var effectTables = new Dictionary<(int tableId, int type), int[]>();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT id, type, init, limit_lv5, limit_lv10, limit_lv15, limit_lv20, limit_lv25, limit_lv30, limit_lv35, limit_lv40, limit_lv45, limit_lv50 FROM support_card_effect_table";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                int tableId = r.GetInt32(0);
                int type = r.GetInt32(1);
                int[] vals = new int[11];
                int lastVal = 0;
                for (int i = 0; i < 11; i++)
                {
                    int v = r.GetInt32(i + 2);
                    if (v != -1) lastVal = v;
                    vals[i] = lastVal;
                }
                effectTables[(tableId, type)] = vals;
            }
        }

        // Helper to get effect value at specific level
        int GetEffectVal(int tableId, int effectType, int level)
        {
            if (!effectTables.TryGetValue((tableId, effectType), out var vals)) return 0;
            // Map level to index (0=init, 1=lv5, 2=lv10, 3=lv15, 4=lv20, 5=lv25, 6=lv30, 7=lv35, 8=lv40, 9=lv45, 10=lv50)
            int idx = Math.Clamp(level / 5, 0, 10);
            return vals[idx];
        }

        // 3. Query all support cards
        var cards = new List<object>();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT c.id, c.chara_id, c.rarity, c.command_id, c.effect_table_id, c.unique_effect_id,
                       tCard.text as card_title, tChara.text as chara_name
                FROM support_card_data c
                LEFT JOIN text_data tCard ON tCard.category = 75 AND tCard.[index] = c.id
                LEFT JOIN text_data tChara ON tChara.category = 6 AND tChara.[index] = c.chara_id
                ORDER BY c.id ASC";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                int id = r.GetInt32(0);
                int charaId = r.GetInt32(1);
                int rarity = r.GetInt32(2);
                int cmdId = r.GetInt32(3);
                int tableId = r.GetInt32(4);
                int uniqueId = r.IsDBNull(5) ? 0 : r.GetInt32(5);
                string title = r.IsDBNull(6) ? "" : r.GetString(6);
                string chara = r.IsDBNull(7) ? "" : r.GetString(7);

                string rarityStr = rarity switch { 3 => "SSR", 2 => "SR", _ => "R" };
                string spec = cmdId switch
                {
                    101 => "SPD",
                    102 => "STA",
                    103 => "POW",
                    105 => "GUT",
                    106 => "WIS",
                    _ => "PAL"
                };
                string colorHex = spec switch
                {
                    "SPD" => "#2563EB",
                    "STA" => "#EA580C",
                    "POW" => "#D97706",
                    "GUT" => "#DB2777",
                    "WIS" => "#059669",
                    _ => "#F59E0B"
                };

                // Max levels for each LB [0LB, 1LB, 2LB, 3LB, 4LB]
                int[] maxLevels = rarity switch
                {
                    3 => new[] { 30, 35, 40, 45, 50 },
                    2 => new[] { 25, 30, 35, 40, 45 },
                    _ => new[] { 20, 25, 30, 35, 40 }
                };

                int[] tbVals = new int[5];
                int[] hintLvVals = new int[5];
                int[] hintRateVals = new int[5];
                int[] specRateVals = new int[5];

                for (int lb = 0; lb < 5; lb++)
                {
                    int lvl = maxLevels[lb];
                    int tb = GetEffectVal(tableId, 8, lvl); // type 8 = Training Effect
                    int hintLv = GetEffectVal(tableId, 17, lvl); // type 17 = Hint Lv Up
                    int hintRate = GetEffectVal(tableId, 18, lvl); // type 18 = Hint Rate Up
                    int specRate = GetEffectVal(tableId, 19, lvl); // type 19 = Specialty Priority

                    if (uniqueEffects.TryGetValue(uniqueId, out var u) && lvl >= u.unlockLv)
                    {
                        if (u.type0 == 8) tb += u.val0;
                        if (u.type1 == 8) tb += u.val1;

                        if (u.type0 == 17) hintLv += u.val0;
                        if (u.type1 == 17) hintLv += u.val1;

                        if (u.type0 == 18) hintRate += u.val0;
                        if (u.type1 == 18) hintRate += u.val1;

                        if (u.type0 == 19) specRate += u.val0;
                        if (u.type1 == 19) specRate += u.val1;
                    }

                    tbVals[lb] = tb;
                    hintLvVals[lb] = hintLv > 0 ? hintLv + 1 : 1; // 1-based hint level
                    hintRateVals[lb] = hintRate;
                    specRateVals[lb] = specRate;
                }

                cards.Add(new
                {
                    id,
                    charaId,
                    charaName = chara,
                    cardTitle = title,
                    displayName = $"{chara} ({rarityStr})",
                    rarity = rarityStr,
                    speciality = spec,
                    colorHex,
                    trainingBonus = tbVals,
                    hintLevel = hintLvVals,
                    hintRate = hintRateVals,
                    specialtyRate = specRateVals
                });
            }
        }

        string jsonOutput = System.Text.Json.JsonSerializer.Serialize(cards, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        
        // Write to Umacalc Assets
        string destPath1 = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Umacalc\Assets\support_card_catalog.json"));
        File.WriteAllText(destPath1, jsonOutput);
        Console.WriteLine($"[PASS] Wrote {cards.Count} support cards to: {destPath1}");

        // Write to Mobile Assets
        string destPath2 = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Umacalc-Mobile\support_card_catalog.json"));
        File.WriteAllText(destPath2, jsonOutput);
        Console.WriteLine($"[PASS] Wrote {cards.Count} support cards to: {destPath2}");

        string destPath2Js = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Umacalc-Mobile\cards_catalog.js"));
        File.WriteAllText(destPath2Js, "window.SUPPORT_CARD_CATALOG = " + jsonOutput + ";");
        Console.WriteLine($"[PASS] Wrote {cards.Count} support cards to: {destPath2Js}");

        // Also write to Android assets
        string destPath3 = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Umacalc-Android\app\src\main\assets\support_card_catalog.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(destPath3)!);
        File.WriteAllText(destPath3, jsonOutput);
        Console.WriteLine($"[PASS] Wrote {cards.Count} support cards to: {destPath3}");

        string destPath3Js = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Umacalc-Android\app\src\main\assets\cards_catalog.js"));
        File.WriteAllText(destPath3Js, "window.SUPPORT_CARD_CATALOG = " + jsonOutput + ";");
        Console.WriteLine($"[PASS] Wrote {cards.Count} support cards to: {destPath3Js}");
    }
}

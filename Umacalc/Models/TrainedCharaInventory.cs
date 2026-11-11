using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Umacalc.Models;

public class LegacyFactorEntry
{
    public int FactorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Stars { get; set; }
    public string Category { get; set; } = "White"; // "Blue", "Pink", "Green", "White"
}

public class TrainedCharaEntry
{
    public int TrainedCharaId { get; set; }
    public int CardId { get; set; }
    public string CharaName { get; set; } = string.Empty;
    public int Speed { get; set; }
    public int Stamina { get; set; }
    public int Power { get; set; }
    public int Guts { get; set; }
    public int Wisdom { get; set; }
    public int RankScore { get; set; }
    public int Wins { get; set; }

    public List<LegacyFactorEntry> SelfFactors { get; set; } = new();
    public List<LegacyFactorEntry> AncestorFactors { get; set; } = new();

    // Summary helpers
    public int TotalBlueStars => SelfBlueStars + AncestorBlueStars;
    public int SelfBlueStars => SelfFactors.Where(f => f.Category == "Blue").Sum(f => f.Stars);
    public int AncestorBlueStars => AncestorFactors.Where(f => f.Category == "Blue").Sum(f => f.Stars);

    public int TotalPinkStars => SelfPinkStars + AncestorPinkStars;
    public int SelfPinkStars => SelfFactors.Where(f => f.Category == "Pink").Sum(f => f.Stars);
    public int AncestorPinkStars => AncestorFactors.Where(f => f.Category == "Pink").Sum(f => f.Stars);

    public string BlueSummary
    {
        get
        {
            var blueCounts = new Dictionary<string, int>();
            foreach (var f in SelfFactors.Concat(AncestorFactors).Where(x => x.Category == "Blue"))
            {
                blueCounts[f.Name] = blueCounts.GetValueOrDefault(f.Name, 0) + f.Stars;
            }
            if (blueCounts.Count == 0) return "No Blue Factors";
            return string.Join(" • ", blueCounts.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {kv.Value}★"));
        }
    }

    public string PrimarySelfBlue
    {
        get
        {
            var selfBlue = SelfFactors.FirstOrDefault(f => f.Category == "Blue");
            return selfBlue != null ? $"{selfBlue.Name} {selfBlue.Stars}★" : "None";
        }
    }

    public string PrimarySelfPink
    {
        get
        {
            var selfPink = SelfFactors.FirstOrDefault(f => f.Category == "Pink");
            return selfPink != null ? $"{selfPink.Name} {selfPink.Stars}★" : "None";
        }
    }
}

public class LegacyRecommendationTier
{
    public int TrainedCharaId { get; set; }
    public string CharaName { get; set; } = string.Empty;
    public string RoleBadge { get; set; } = string.Empty; // e.g. "PRIMARY PARENT (9★ BLUE)"
    public string BlueStatsBadge { get; set; } = string.Empty; // e.g. "18★ Blue (Power 9★, Stamina 6★)"
    public string PinkAptitudeBadge { get; set; } = string.Empty; // e.g. "Pace Chaser 3★, Turf 2★"
    public string HighlightFactor { get; set; } = string.Empty; // Unique skill or race factor
    public string RecommendationReason { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsPlayerOwned { get; set; } = true;
}

public class TrainedCharaInventory
{
    private static readonly Lazy<TrainedCharaInventory> _lazy = new(() => new TrainedCharaInventory());
    public static TrainedCharaInventory Instance => _lazy.Value;

    public List<TrainedCharaEntry> Characters { get; } = new();
    public string? LoadedFileName { get; private set; }
    public bool IsLoaded => Characters.Count > 0;

    private TrainedCharaInventory() { }

    public bool LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return false;
            string json = File.ReadAllText(filePath);
            bool success = LoadFromJson(json);
            if (success)
            {
                LoadedFileName = Path.GetFileName(filePath);
            }
            return success;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load trained chara file: {ex.Message}");
            return false;
        }
    }

    public bool LoadFromJson(string json)
    {
        try
        {
            var catalog = LegacyCatalog.Instance;
            using var doc = JsonDocument.Parse(json);
            Characters.Clear();

            foreach (var elem in doc.RootElement.EnumerateArray())
            {
                var entry = new TrainedCharaEntry
                {
                    TrainedCharaId = elem.GetProperty("trained_chara_id").GetInt32(),
                    CardId = elem.GetProperty("card_id").GetInt32(),
                    Speed = elem.GetProperty("speed").GetInt32(),
                    Stamina = elem.GetProperty("stamina").GetInt32(),
                    Power = elem.GetProperty("power").GetInt32(),
                    Guts = elem.GetProperty("guts").GetInt32(),
                    Wisdom = elem.GetProperty("wiz").GetInt32(),
                    RankScore = elem.GetProperty("rank_score").GetInt32(),
                    Wins = elem.GetProperty("wins").GetInt32()
                };

                entry.CharaName = catalog.GetCharaName(entry.CardId);

                // Parse Self factors
                if (elem.TryGetProperty("factor_info_array", out var selfArr))
                {
                    foreach (var fElem in selfArr.EnumerateArray())
                    {
                        int fid = fElem.GetProperty("factor_id").GetInt32();
                        entry.SelfFactors.Add(ParseFactor(fid, catalog));
                    }
                }

                // Parse Ancestor factors (positions 10, 20, 11, 12, 21, 22)
                if (elem.TryGetProperty("succession_chara_array", out var succArr))
                {
                    foreach (var pElem in succArr.EnumerateArray())
                    {
                        if (pElem.TryGetProperty("factor_info_array", out var pFactors))
                        {
                            foreach (var fElem in pFactors.EnumerateArray())
                            {
                                int fid = fElem.GetProperty("factor_id").GetInt32();
                                entry.AncestorFactors.Add(ParseFactor(fid, catalog));
                            }
                        }
                    }
                }

                Characters.Add(entry);
            }

            return Characters.Count > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to parse trained chara json: {ex.Message}");
            return false;
        }
    }

    public void Clear()
    {
        Characters.Clear();
        LoadedFileName = null;
    }

    public static LegacyFactorEntry ParseFactor(int factorId, LegacyCatalog catalog)
    {
        int stars = factorId % 100;
        if (stars == 0) stars = 1;
        int typeGroup = factorId / 100;

        string category = "White";
        if (typeGroup >= 1 && typeGroup <= 5)
        {
            category = "Blue";
        }
        else if (typeGroup >= 11 && typeGroup <= 34)
        {
            category = "Pink";
        }
        else if (factorId >= 10000000)
        {
            category = "Green"; // Unique Character skill factor
        }

        string name = catalog.GetFactorName(factorId);

        return new LegacyFactorEntry
        {
            FactorId = factorId,
            Name = name,
            Stars = stars,
            Category = category
        };
    }

    public List<LegacyRecommendationTier> EvaluateBestLegacies(BuildPreset preset, int count = 3)
    {
        if (Characters.Count == 0) return new List<LegacyRecommendationTier>();

        var candidates = new List<(TrainedCharaEntry chara, int score, string role, string reason)>();
        string pid = preset.Id.ToLowerInvariant();

        // Target stats for preset (e.g. Long -> Stamina 1st, Speed 2nd; Sprint -> Speed 1st, Power 2nd)
        var preferredStats = GetPreferredStats(pid);

        foreach (var c in Characters)
        {
            int score = 0;

            // 1. Blue Factor Points (Lineage total & self 3-star value)
            int selfBlue = c.SelfBlueStars;
            int totalBlue = c.TotalBlueStars;
            score += totalBlue * 120;
            score += selfBlue * 250; // extra weight for high self blue

            // 2. Stat relevance
            foreach (var f in c.SelfFactors.Concat(c.AncestorFactors).Where(x => x.Category == "Blue"))
            {
                if (preferredStats.Contains(f.Name))
                {
                    score += f.Stars * 180;
                }
            }

            // 3. Pink Aptitude relevance (Surface, Distance, Strategy)
            foreach (var f in c.SelfFactors.Concat(c.AncestorFactors).Where(x => x.Category == "Pink"))
            {
                if (IsAptitudeRelevant(pid, f.Name))
                {
                    score += f.Stars * 200;
                }
            }

            // 4. Unique green skill synergy
            var uniqueGreen = c.SelfFactors.FirstOrDefault(x => x.Category == "Green");
            if (uniqueGreen != null)
            {
                score += 150;
            }

            // 5. General stats and wins
            score += Math.Min(300, c.Wins * 10);
            score += (c.RankScore / 100);

            string roleBadge = selfBlue >= 3 ? $"PRIMARY PARENT ({totalBlue}★ BLUE)" : $"GRANDPARENT ({totalBlue}★ BLUE)";
            string reason = GenerateLegacyReason(preset, c, preferredStats);

            candidates.Add((c, score, roleBadge, reason));
        }

        var result = new List<LegacyRecommendationTier>();
        var usedCharaNames = new HashSet<string>();

        // Pick top distinct characters to allow parent 1 & parent 2 legacy pairing
        foreach (var item in candidates.OrderByDescending(x => x.score))
        {
            if (result.Count >= count) break;
            // Allow top legacy options (can have same horse if player wants best build, but prefer diverse if available)
            var c = item.chara;

            var selfPink = c.SelfFactors.FirstOrDefault(f => f.Category == "Pink");
            string pinkStr = selfPink != null ? $"{selfPink.Name} {selfPink.Stars}★" : "Aptitude Balanced";

            var selfGreen = c.SelfFactors.FirstOrDefault(f => f.Category == "Green");
            string greenStr = selfGreen != null ? selfGreen.Name : "Standard Legacy Sparks";

            result.Add(new LegacyRecommendationTier
            {
                TrainedCharaId = c.TrainedCharaId,
                CharaName = $"{c.CharaName} (ID #{c.TrainedCharaId})",
                RoleBadge = item.role,
                BlueStatsBadge = $"{c.TotalBlueStars}★ Blue Lineage ({c.BlueSummary})",
                PinkAptitudeBadge = pinkStr,
                HighlightFactor = greenStr,
                RecommendationReason = item.reason,
                Score = item.score,
                IsPlayerOwned = true
            });
            usedCharaNames.Add(c.CharaName);
        }

        return result;
    }

    private static HashSet<string> GetPreferredStats(string pid)
    {
        return pid switch
        {
            "sprint" => new HashSet<string> { "Speed", "Power" },
            "mile" => new HashSet<string> { "Speed", "Power" },
            "medium" => new HashSet<string> { "Speed", "Stamina", "Power" },
            "long" => new HashSet<string> { "Stamina", "Speed" },
            "dirt" => new HashSet<string> { "Power", "Speed" },
            "runner" or "front_runner" => new HashSet<string> { "Speed", "Wit" },
            "leader" or "pace_setter" => new HashSet<string> { "Power", "Speed", "Stamina" },
            "betwixt" or "late_bloomer" => new HashSet<string> { "Power", "Speed", "Stamina" },
            "chaser" => new HashSet<string> { "Power", "Stamina", "Speed" },
            "guts_meta" => new HashSet<string> { "Guts", "Speed", "Power" },
            "wisdom_engine" or "wisdom_sp" => new HashSet<string> { "Wit", "Speed" },
            _ => new HashSet<string> { "Speed", "Stamina", "Power" }
        };
    }

    private static bool IsAptitudeRelevant(string pid, string factorName)
    {
        return pid switch
        {
            "sprint" => factorName is "Sprint" or "Turf",
            "mile" => factorName is "Mile" or "Turf",
            "medium" => factorName is "Medium" or "Turf",
            "long" => factorName is "Long" or "Turf",
            "dirt" => factorName is "Dirt",
            "runner" or "front_runner" => factorName is "Front Runner" or "Turf",
            "leader" or "pace_setter" => factorName is "Pace Chaser" or "Turf",
            "betwixt" or "late_bloomer" => factorName is "Late Surger" or "Turf",
            "chaser" => factorName is "End Closer" or "Turf",
            _ => factorName is "Turf"
        };
    }

    private static string GenerateLegacyReason(BuildPreset preset, TrainedCharaEntry c, HashSet<string> preferred)
    {
        var matchedBlue = c.SelfFactors.Concat(c.AncestorFactors)
            .Where(f => f.Category == "Blue" && preferred.Contains(f.Name))
            .ToList();

        string keyBlueStr = matchedBlue.Count > 0
            ? string.Join(", ", matchedBlue.GroupBy(b => b.Name).Select(g => $"{g.Key} +{g.Sum(x => x.Stars)}★"))
            : $"{c.PrimarySelfBlue}";

        var pink = c.SelfFactors.FirstOrDefault(f => f.Category == "Pink");
        string pinkStr = pink != null ? $" • Grants {pink.Name} {pink.Stars}★ aptitude growth" : "";

        return $"Transfers {keyBlueStr} to hit required {preset.Name} stat caps{pinkStr} with {c.TotalBlueStars}★ total lineage spark potential.";
    }
}

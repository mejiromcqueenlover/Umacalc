using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umacalc.Models;

public class PlayerCardEntry
{
    [JsonPropertyName("support_card_id")]
    public int SupportCardId { get; set; }

    [JsonPropertyName("limit_break_count")]
    public int LimitBreakCount { get; set; }

    [JsonPropertyName("extra_data")]
    public PlayerCardExtraData? ExtraData { get; set; }

    public int Level => ExtraData?.Level ?? (LimitBreakCount == 4 ? 50 : 30 + LimitBreakCount * 5);
    public int MaxLevel => ExtraData?.MaxLevel ?? (LimitBreakCount == 4 ? 50 : 30 + LimitBreakCount * 5);
}

public class PlayerCardExtraData
{
    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("max_level")]
    public int MaxLevel { get; set; }

    [JsonPropertyName("best_training")]
    public int BestTraining { get; set; }
}

public class PlayerInventory
{
    public string SourceFilePath { get; set; } = string.Empty;
    public string FileName => string.IsNullOrEmpty(SourceFilePath) ? string.Empty : Path.GetFileName(SourceFilePath);
    public DateTime LoadedAt { get; set; } = DateTime.Now;

    public Dictionary<int, PlayerCardEntry> OwnedCards { get; } = new();

    public int TotalCards => OwnedCards.Count;
    public int SsrCount => OwnedCards.Values.Count(c => c.SupportCardId >= 30000);
    public int SrCount => OwnedCards.Values.Count(c => c.SupportCardId >= 20000 && c.SupportCardId < 30000);
    public int RCount => OwnedCards.Values.Count(c => c.SupportCardId < 20000);

    public string SummaryText => $"{TotalCards} Cards Loaded ({SsrCount} SSR, {SrCount} SR, {RCount} R)";

    public static PlayerInventory? LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        try
        {
            string json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var entries = JsonSerializer.Deserialize<List<PlayerCardEntry>>(json, options);
            if (entries == null || entries.Count == 0) return null;

            var inv = new PlayerInventory { SourceFilePath = filePath };
            foreach (var entry in entries)
            {
                // If duplicates exist, keep the one with higher limit break
                if (inv.OwnedCards.TryGetValue(entry.SupportCardId, out var existing))
                {
                    if (entry.LimitBreakCount > existing.LimitBreakCount ||
                        (entry.LimitBreakCount == existing.LimitBreakCount && entry.Level > existing.Level))
                    {
                        inv.OwnedCards[entry.SupportCardId] = entry;
                    }
                }
                else
                {
                    inv.OwnedCards[entry.SupportCardId] = entry;
                }
            }
            return inv;
        }
        catch
        {
            return null;
        }
    }

    public List<SupportCardTier> EvaluateBestTrainingCards(BuildPreset preset, int count = 4)
    {
        var catalog = SupportCardCatalog.Instance;
        var relevantSpecs = GetRelevantSpecialities(preset);

        var candidates = new List<(SupportCardInfo card, PlayerCardEntry entry, int tb, int score)>();

        foreach (var owned in OwnedCards.Values)
        {
            var card = catalog.GetCard(owned.SupportCardId);
            if (card == null) continue;
            if (!relevantSpecs.Contains(card.Speciality.ToUpperInvariant())) continue;

            int tb = card.GetTrainingBonus(owned.LimitBreakCount);
            if (tb <= 0) continue;

            int specRate = card.GetSpecialtyRate(owned.LimitBreakCount);
            int rarityBonus = card.Rarity switch { "SSR" => 120, "SR" => 60, _ => 10 };
            int lbBonus = owned.LimitBreakCount * 25;
            int lvlBonus = owned.Level;

            int affinityBonus = HasPresetAffinity(preset.Id, card.CharaName) ? 400 : 0;

            // Score strongly values Training Bonus percentage, preset affinity, speciality rate and LB
            int score = (tb * 1500) + affinityBonus + (specRate * 12) + rarityBonus + lbBonus + lvlBonus;
            candidates.Add((card, owned, tb, score));
        }

        if (candidates.Count == 0)
        {
            return preset.TrainingBonusCards;
        }

        var result = new List<SupportCardTier>();
        var usedCharaIds = new HashSet<int>();

        // Get preset-tailored slot targets (e.g., Long -> [STA, STA, SPD, SPD])
        var slotTargets = GetTargetSlotSequence(preset);

        // Fill each targeted slot with the best card matching that speciality
        foreach (var targetSpec in slotTargets)
        {
            if (result.Count >= count) break;

            var match = candidates
                .Where(c => c.card.Speciality.Equals(targetSpec, StringComparison.OrdinalIgnoreCase) && !usedCharaIds.Contains(c.card.CharaId))
                .OrderByDescending(c => c.score)
                .FirstOrDefault();

            if (match.card != null)
            {
                result.Add(CreateTrainingTier(preset.Id, match.card, match.entry, match.tb));
                usedCharaIds.Add(match.card.CharaId);
            }
        }

        // If any spots remain unfilled, fill from best remaining candidates matching the preset
        if (result.Count < count)
        {
            var remaining = candidates
                .Where(c => !usedCharaIds.Contains(c.card.CharaId))
                .OrderByDescending(c => c.score);

            foreach (var c in remaining)
            {
                if (result.Count >= count) break;
                result.Add(CreateTrainingTier(preset.Id, c.card, c.entry, c.tb));
                usedCharaIds.Add(c.card.CharaId);
            }
        }

        return result;
    }

    public List<SupportCardTier> EvaluateBestInspirationCards(BuildPreset preset, int count = 3)
    {
        var catalog = SupportCardCatalog.Instance;
        var relevantSpecs = GetRelevantSpecialities(preset);

        var candidates = new List<(SupportCardInfo card, PlayerCardEntry entry, int hintLv, int hintRate, int score)>();

        foreach (var owned in OwnedCards.Values)
        {
            var card = catalog.GetCard(owned.SupportCardId);
            if (card == null) continue;
            if (!relevantSpecs.Contains(card.Speciality.ToUpperInvariant())) continue;

            int hintLv = card.GetHintLevel(owned.LimitBreakCount);
            int hintRate = card.GetHintRate(owned.LimitBreakCount);

            if (hintLv < 2 && hintRate < 20) continue;

            int rarityBonus = card.Rarity switch { "SSR" => 50, "SR" => 30, _ => 10 };
            int affinityBonus = HasPresetAffinity(preset.Id, card.CharaName) ? 500 : 0;
            int score = (hintLv * 250) + (hintRate * 8) + affinityBonus + rarityBonus + (owned.LimitBreakCount * 15);
            candidates.Add((card, owned, hintLv, hintRate, score));
        }

        if (candidates.Count == 0)
        {
            return preset.InspirationBonusCards;
        }

        var result = new List<SupportCardTier>();
        var usedCharaIds = new HashSet<int>();
        var sorted = candidates.OrderByDescending(c => c.score).ToList();

        foreach (var c in sorted)
        {
            if (result.Count >= count) break;
            if (usedCharaIds.Contains(c.card.CharaId)) continue;

            result.Add(CreateInspirationTier(preset.Id, c.card, c.entry, c.hintLv, c.hintRate));
            usedCharaIds.Add(c.card.CharaId);
        }

        return result;
    }

    private static List<string> GetTargetSlotSequence(BuildPreset preset)
    {
        return preset.Id.ToLowerInvariant() switch
        {
            "sprint" => new List<string> { "SPD", "SPD", "SPD", "WIS" },
            "mile" => new List<string> { "SPD", "SPD", "WIS", "POW" },
            "medium" => new List<string> { "SPD", "STA", "SPD", "STA" },
            "long" => new List<string> { "STA", "STA", "SPD", "SPD" },
            "dirt" => new List<string> { "POW", "POW", "SPD", "SPD" },
            "runner" or "front_runner" => new List<string> { "SPD", "SPD", "WIS", "WIS" },
            "leader" or "pace_setter" => new List<string> { "SPD", "SPD", "POW", "WIS" },
            "betwixt" or "late_bloomer" => new List<string> { "POW", "SPD", "STA", "POW" },
            "chaser" => new List<string> { "POW", "POW", "SPD", "STA" },
            "guts_meta" => new List<string> { "GUT", "GUT", "SPD", "WIS" },
            "factor_farm" => new List<string> { "SPD", "STA", "POW", "WIS" },
            "wisdom_engine" or "wisdom_sp" => new List<string> { "WIS", "WIS", "SPD", "SPD" },
            "sp" or "sp_pow" => new List<string> { "SPD", "SPD", "POW", "POW" },
            "ss" or "sp_sta" => new List<string> { "SPD", "SPD", "STA", "STA" },
            "sw" or "sp_wis" => new List<string> { "SPD", "SPD", "WIS", "WIS" },
            _ => preset.DeckComposition.Select(d => d.ShortLabel.ToUpperInvariant()).Take(4).ToList()
        };
    }

    private static bool HasPresetAffinity(string presetId, string charaName)
    {
        return presetId.ToLowerInvariant() switch
        {
            "runner" or "front_runner" => charaName is "Mihono Bourbon" or "Silence Suzuka" or "Twin Turbo" or "Smart Falcon" or "Maruzensky" or "Daitaku Helios" or "Seiun Sky",
            "leader" or "pace_setter" => charaName is "Tokai Teio" or "Fine Motion" or "Nishino Flower" or "Taiki Shuttle" or "Oguri Cap" or "Special Week",
            "betwixt" or "late_bloomer" => charaName is "Vodka" or "Grass Wonder" or "Nice Nature" or "Symboli Rudolf" or "Mejiro Dober" or "Agnes Digital" or "Yukino Bijin",
            "chaser" => charaName is "Narita Taishin" or "Gold Ship" or "Tamamo Cross" or "Mr. C.B." or "Admire Vega" or "Shinko Windy" or "Ines Fujin",
            "sprint" => charaName is "Biko Pegasus" or "Taiki Shuttle" or "Curren Chan" or "Sakura Bakushin O" or "King Halo" or "Daitaku Helios",
            "mile" => charaName is "Vodka" or "Yaeno Muteki" or "Fine Motion" or "Kitasan Black" or "Shinko Windy" or "Nishino Flower" or "Daitaku Helios",
            "medium" => charaName is "Super Creek" or "Kitasan Black" or "Yaeno Muteki" or "Oguri Cap" or "Agnes Tachyon" or "Riko Kashimoto" or "Daiwa Scarlet",
            "long" => charaName is "Super Creek" or "Manhattan Cafe" or "Grass Wonder" or "Tanino Gimlet" or "Seiun Sky" or "Mayano Top Gun" or "Biwa Hayahide",
            "dirt" => charaName is "Smart Falcon" or "Oguri Cap" or "Agnes Digital" or "El Condor Pasa" or "Shinko Windy" or "Haru Urara",
            "guts_meta" => charaName is "Biwa Hayahide" or "Sakura Laurel" or "Tamamo Cross" or "Mejiro McQueen" or "Zenno Rob Roy" or "Grass Wonder",
            "wisdom_engine" or "wisdom_sp" => charaName is "Fine Motion" or "Mr. C.B." or "Nice Nature" or "Satono Diamond" or "Marvelous Sunday" or "Sweep Tosho",
            _ => false
        };
    }

    private static SupportCardTier CreateTrainingTier(string presetId, SupportCardInfo card, PlayerCardEntry owned, int tb)
    {
        string lbText = owned.LimitBreakCount == 4 ? $"MLB Lv {owned.Level}" : $"{owned.LimitBreakCount}LB Lv {owned.Level}";
        string reason = GetPresetTailoredTrainingReason(presetId, card, owned, tb);

        return new SupportCardTier
        {
            CardName = $"{card.CharaName} ({card.Rarity})",
            Speciality = card.Speciality,
            ColorHex = card.ColorHex,
            BonusStat = $"+{tb}% Training Effect",
            SkillOrReason = reason,
            IsPlayerOwned = true,
            OwnershipBadge = lbText,
            OwnershipColor = owned.LimitBreakCount == 4 ? "#10B981" : "#38BDF8"
        };
    }

    private static SupportCardTier CreateInspirationTier(string presetId, SupportCardInfo card, PlayerCardEntry owned, int hintLv, int hintRate)
    {
        string lbText = owned.LimitBreakCount == 4 ? $"MLB Lv {owned.Level}" : $"{owned.LimitBreakCount}LB Lv {owned.Level}";
        int discount = hintLv * 10;
        string rateStr = hintRate > 0 ? $" (+{hintRate}% Rate)" : "";
        string reason = GetPresetTailoredInspirationReason(presetId, card, owned, hintLv, discount);

        return new SupportCardTier
        {
            CardName = $"{card.CharaName} ({card.Rarity})",
            Speciality = card.Speciality,
            ColorHex = card.ColorHex,
            BonusStat = $"Hint Lv {hintLv}{rateStr}",
            SkillOrReason = reason,
            IsPlayerOwned = true,
            OwnershipBadge = lbText,
            OwnershipColor = owned.LimitBreakCount == 4 ? "#10B981" : "#FBBF24"
        };
    }

    private static string GetPresetTailoredTrainingReason(string presetId, SupportCardInfo card, PlayerCardEntry owned, int tb)
    {
        string spec = card.Speciality.ToUpperInvariant();
        string titlePart = string.IsNullOrEmpty(card.CardTitle) ? "" : $"{card.CardTitle} • ";

        return presetId.ToLowerInvariant() switch
        {
            "sprint" => spec switch
            {
                "SPD" => $"{titlePart}Top sprint speed engine (+{tb}%) • Caps 1200 top speed in short races",
                "WIS" => $"{titlePart}Wisdom facility engine (+{tb}%) • Recovers energy while building early positioning",
                _ => $"{titlePart}High training bonus (+{tb}%) for sprint burst"
            },
            "mile" => spec switch
            {
                "SPD" => $"{titlePart}Speed multiplier (+{tb}%) • Essential speed backbone for mile pace",
                "POW" => $"{titlePart}Power powerhouse (+{tb}%) • Powers final-turn burst and lane overtaking",
                "WIS" => $"{titlePart}Wisdom multiplier (+{tb}%) • Ensures high skill trigger rates for mile skills",
                _ => $"{titlePart}Universal training boost (+{tb}%)"
            },
            "medium" => spec switch
            {
                "STA" => $"{titlePart}Medium distance endurance anchor (+{tb}%) • Reaches 750+ Stamina threshold",
                "SPD" => $"{titlePart}Reliable speed growth (+{tb}%) • Keeps pace with Derby leaders",
                "POW" => $"{titlePart}Power climb engine (+{tb}%) • Overcomes final stretch uphill slopes",
                _ => $"{titlePart}Balanced stat engine (+{tb}%)"
            },
            "long" => spec switch
            {
                "STA" => $"{titlePart}Long distance stamina pillar (+{tb}%) • Sustains 2500m-3600m marathon endurance",
                "SPD" => $"{titlePart}Crucial speed engine (+{tb}%) • Secures 1200 Speed cap while focusing Stamina turns",
                "PAL" => $"{titlePart}Friend/Pal dating events (+{tb}%) • Stamina recovery & motivation cures",
                _ => $"{titlePart}High training bonus (+{tb}%) for long distance"
            },
            "dirt" => spec switch
            {
                "POW" => $"{titlePart}Power powerhouse (+{tb}%) • Plows through heavy dirt track friction",
                "SPD" => $"{titlePart}Speed engine (+{tb}%) • Fast dirt breakaway acceleration",
                _ => $"{titlePart}Dirt track booster (+{tb}%)"
            },
            "runner" or "front_runner" => card.CharaName is "Mihono Bourbon" or "Silence Suzuka" or "Twin Turbo" or "Smart Falcon"
                ? $"{titlePart}Core Front-Runner card (+{tb}%) • Groundwork & runaway lead synergy"
                : (spec == "SPD"
                    ? $"{titlePart}Front-runner speed engine (+{tb}%) • Takes and defends the inside rail lead"
                    : $"{titlePart}Front-runner wisdom engine (+{tb}%) • Prevents mid-race pace drop"),
            "leader" or "pace_setter" => card.CharaName is "Tokai Teio" or "Fine Motion" or "Nishino Flower" or "Taiki Shuttle"
                ? $"{titlePart}Signature Pace Setter card (+{tb}%) • Direct attack & mid-pack position control"
                : $"{titlePart}High training multiplier (+{tb}%) to shadow front runners",
            "betwixt" or "late_bloomer" => spec switch
            {
                "POW" => $"{titlePart}Heavy power engine (+{tb}%) • Powers devastating mid-pack overtake surges",
                "SPD" => $"{titlePart}Outside lane charge engine (+{tb}%) • Late spurt speed",
                _ => $"{titlePart}Late bloomer stat multiplier (+{tb}%)"
            },
            "chaser" => card.CharaName is "Tamamo Cross" or "Gold Ship" or "Shinko Windy" or "Mr. C.B."
                ? $"{titlePart}Core Chaser engine (+{tb}%) • Devastating late-race rear overtake power"
                : (spec == "POW"
                    ? $"{titlePart}Maximum power yields (+{tb}%) • Crucial for final straightaway spurt"
                    : $"{titlePart}Endurance & speed anchor (+{tb}%) for late charge"),
            "guts_meta" => spec switch
            {
                "GUT" => $"{titlePart}Root training anchor (+{tb}%) • Multi-stat yields across Speed, Power & Guts",
                "SPD" => $"{titlePart}Speed complement (+{tb}%) • Balances high guts training",
                _ => $"{titlePart}Guts meta synergy card (+{tb}%)"
            },
            "factor_farm" => $"{titlePart}Universal training bonus (+{tb}%) • Pushes all stats toward 600/1100 for 3★ Blue factors",
            "wisdom_engine" or "wisdom_sp" => spec switch
            {
                "WIS" => $"{titlePart}Wisdom training engine (+{tb}%) • Energy recovery & massive SP gains",
                _ => $"{titlePart}Turn-saving stat multiplier (+{tb}%)"
            },
            "sp" or "sp_pow" => $"{titlePart}Focused 2-stat engine (+{tb}%) • Raw speed & overpowering burst",
            "ss" or "sp_sta" => $"{titlePart}Focused 2-stat engine (+{tb}%) • Top speed & classic endurance",
            "sw" or "sp_wis" => $"{titlePart}Focused 2-stat engine (+{tb}%) • Zero-energy wisdom training synergy",
            _ => $"{titlePart}Training multiplier (+{tb}%) • Specialty Rate +{card.GetSpecialtyRate(owned.LimitBreakCount)}"
        };
    }

    private static string GetPresetTailoredInspirationReason(string presetId, SupportCardInfo card, PlayerCardEntry owned, int hintLv, int discount)
    {
        string titlePart = string.IsNullOrEmpty(card.CardTitle) ? "" : $"{card.CardTitle} • ";

        return presetId.ToLowerInvariant() switch
        {
            "sprint" => $"{titlePart}Sprint acceleration & corner tricks at -{discount}% SP discount",
            "mile" => $"{titlePart}Tail Held High & mile burst skills at -{discount}% SP discount",
            "medium" => $"{titlePart}Middle distance recovery & mid-leg acceleration at -{discount}% SP discount",
            "long" => $"{titlePart}Long distance stamina recovery & pacing skills at -{discount}% SP discount",
            "dirt" => $"{titlePart}Dirt track passives & sand overtaking skills at -{discount}% SP discount",
            "runner" or "front_runner" => card.CharaName is "Mihono Bourbon" or "Silence Suzuka" or "Twin Turbo"
                ? $"{titlePart}Essential Groundwork (地固め) & escape skills at -{discount}% SP discount"
                : $"{titlePart}Front-runner positioning & early lead tricks at -{discount}% SP discount",
            "leader" or "pace_setter" => $"{titlePart}Speed Star & Senkou corner hints at -{discount}% SP discount",
            "betwixt" or "late_bloomer" => $"{titlePart}Tail Held High, Ramp Up & Sashi overtake hints at -{discount}% SP discount",
            "chaser" => $"{titlePart}Straightaway Spurt, Oikomi Corner & late charge hints at -{discount}% SP discount",
            "guts_meta" => $"{titlePart}Guts stamina conservation & position fight hints at -{discount}% SP discount",
            "factor_farm" => $"{titlePart}High-frequency hint battery to unlock maximum White Factor skill sparks",
            "wisdom_engine" or "wisdom_sp" => $"{titlePart}Massive skill point discounts (-{discount}% off) across key skills",
            _ => $"{titlePart}Hint battery granting -{discount}% skill point discount"
        };
    }

    private static HashSet<string> GetRelevantSpecialities(BuildPreset preset)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var slot in preset.DeckComposition)
        {
            set.Add(slot.ShortLabel);
        }

        // Add from stat weights if not already present
        foreach (var kvp in preset.Weights)
        {
            if (kvp.Value > 0)
            {
                string shortLabel = kvp.Key switch
                {
                    StatType.Speed => "SPD",
                    StatType.Stamina => "STA",
                    StatType.Power => "POW",
                    StatType.Guts => "GUT",
                    StatType.Wisdom => "WIS",
                    _ => ""
                };
                if (!string.IsNullOrEmpty(shortLabel)) set.Add(shortLabel);
            }
        }

        // Always include PAL / Friend if not explicitly restricted
        set.Add("PAL");

        return set;
    }
}

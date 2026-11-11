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

            // Score strongly values Training Bonus percentage, plus speciality rate and LB
            int score = (tb * 1500) + (specRate * 12) + rarityBonus + lbBonus + lvlBonus;
            candidates.Add((card, owned, tb, score));
        }

        if (candidates.Count == 0)
        {
            return preset.TrainingBonusCards;
        }

        // Pick balanced best cards across the preset's slots
        var result = new List<SupportCardTier>();
        var usedCharaIds = new HashSet<int>();

        // Sort candidates by score descending
        var sorted = candidates.OrderByDescending(c => c.score).ToList();

        // Target specialities in order of deck slots
        var primarySpecs = preset.DeckComposition.Select(d => d.ShortLabel.ToUpperInvariant()).Distinct().ToList();

        // First pass: at least one top card for each primary speciality in the preset
        foreach (var spec in primarySpecs)
        {
            var match = sorted.FirstOrDefault(c => c.card.Speciality.Equals(spec, StringComparison.OrdinalIgnoreCase) && !usedCharaIds.Contains(c.card.CharaId));
            if (match.card != null)
            {
                result.Add(CreateTrainingTier(match.card, match.entry, match.tb));
                usedCharaIds.Add(match.card.CharaId);
                sorted.Remove(match);
            }
            if (result.Count >= count) break;
        }

        // Second pass: fill remaining spots with the absolute best remaining scorers
        foreach (var c in sorted)
        {
            if (result.Count >= count) break;
            if (usedCharaIds.Contains(c.card.CharaId)) continue;

            result.Add(CreateTrainingTier(c.card, c.entry, c.tb));
            usedCharaIds.Add(c.card.CharaId);
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
            int score = (hintLv * 250) + (hintRate * 8) + rarityBonus + (owned.LimitBreakCount * 15);
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

            result.Add(CreateInspirationTier(c.card, c.entry, c.hintLv, c.hintRate));
            usedCharaIds.Add(c.card.CharaId);
        }

        return result;
    }

    private static SupportCardTier CreateTrainingTier(SupportCardInfo card, PlayerCardEntry owned, int tb)
    {
        string lbText = owned.LimitBreakCount == 4 ? $"MLB Lv {owned.Level}" : $"{owned.LimitBreakCount}LB Lv {owned.Level}";
        string reason = string.IsNullOrEmpty(card.CardTitle)
            ? $"Training multiplier from your collection (Specialty Priority +{card.GetSpecialtyRate(owned.LimitBreakCount)})"
            : $"{card.CardTitle} • Specialty Rate +{card.GetSpecialtyRate(owned.LimitBreakCount)}";

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

    private static SupportCardTier CreateInspirationTier(SupportCardInfo card, PlayerCardEntry owned, int hintLv, int hintRate)
    {
        string lbText = owned.LimitBreakCount == 4 ? $"MLB Lv {owned.Level}" : $"{owned.LimitBreakCount}LB Lv {owned.Level}";
        int discount = hintLv * 10;
        string rateStr = hintRate > 0 ? $" (+{hintRate}% Rate)" : "";
        string reason = string.IsNullOrEmpty(card.CardTitle)
            ? $"Hint event battery granting -{discount}% skill point discount"
            : $"{card.CardTitle} • -{discount}% SP hint discount";

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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Umacalc.Models;

public class SkillHintSuggestion
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty; // e.g. "Kitasan Black [SPD 0LB]" or "Legacy: Mejiro McQueen"
    public string SourceType { get; set; } = "Support Card"; // "Support Card" or "Legacy Spark"
    public string SourceColor { get; set; } = "#3B82F6";
    public int EstimatedHintLv { get; set; } = 3;
    public int SpDiscountPct => Math.Min(50, EstimatedHintLv * 10);
    public int EvaluationPoints { get; set; } = 217;
    public string SynergyReason { get; set; } = string.Empty;
    public bool IsGold { get; set; }
}

public class CareerPointPrediction
{
    public int EstimatedStatScore { get; set; }
    public int EstimatedSkillScore { get; set; }
    public int EstimatedTotalScore => EstimatedStatScore + EstimatedSkillScore;
    public string PredictedRank { get; set; } = "A";
    public string RankBadgeColor { get; set; } = "#F59E0B";
    public int TotalEstimatedSkillsAcquired { get; set; }
    public int TotalDiscountedSpSaved { get; set; }
    public List<SkillHintSuggestion> RecommendedSkillHints { get; set; } = new();

    public static CareerPointPrediction Calculate(
        BuildPreset preset,
        ScenarioData scenario,
        IEnumerable<SupportCardTier> supportCards,
        IEnumerable<LegacyRecommendationTier> legacies)
    {
        var prediction = new CareerPointPrediction();
        var skillCatalog = SkillCatalog.Instance;
        var cardCatalog = SupportCardCatalog.Instance;

        // 1. Calculate Estimated Stat Score from Scenario & Preset distribution
        // Typical completion stats: 4000 - 4500 base stats for URA/MANT/Aoharu
        int cap = scenario.StatCap;
        int trainable = scenario.DefaultTrainableTurns;
        
        // Base stats estimation: ~750 - 950 per targeted stat + 400 untargeted
        int estimatedSpeed = preset.Weights.GetValueOrDefault(StatType.Speed, 0) > 0 ? Math.Min(cap, 1050) : 650;
        int estimatedStamina = preset.Weights.GetValueOrDefault(StatType.Stamina, 0) > 0 ? Math.Min(cap, 950) : 550;
        int estimatedPower = preset.Weights.GetValueOrDefault(StatType.Power, 0) > 0 ? Math.Min(cap, 950) : 600;
        int estimatedGuts = preset.Weights.GetValueOrDefault(StatType.Guts, 0) > 0 ? Math.Min(cap, 850) : 450;
        int estimatedWisdom = preset.Weights.GetValueOrDefault(StatType.Wisdom, 0) > 0 ? Math.Min(cap, 900) : 550;

        int totalStatSum = estimatedSpeed + estimatedStamina + estimatedPower + estimatedGuts + estimatedWisdom;
        
        // In Umamusume, stat evaluation point formula scales exponentially with stats
        // Around 4000 total stats corresponds to ~10,500 - 11,500 stat evaluation points
        prediction.EstimatedStatScore = (int)(totalStatSum * 2.65);

        // 2. Discover available skill hints from Support Cards
        var availableSkills = new List<SkillHintSuggestion>();
        var seenSkillNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Collect skills from Support Cards
        foreach (var sc in supportCards)
        {
            var cardInfo = cardCatalog.AllCards.FirstOrDefault(c => c.DisplayName.Equals(sc.CardName, StringComparison.OrdinalIgnoreCase)
                                                                 || c.CharaName.Equals(sc.CardName, StringComparison.OrdinalIgnoreCase));
            if (cardInfo != null)
            {
                var skills = skillCatalog.GetSkillsForCard(cardInfo.Id);
                foreach (var sk in skills)
                {
                    if (seenSkillNames.Contains(sk.Name)) continue;
                    seenSkillNames.Add(sk.Name);

                    string reason = GetSkillSynergyReason(preset.Id, sk.Name);
                    availableSkills.Add(new SkillHintSuggestion
                    {
                        SkillId = sk.Id,
                        SkillName = sk.Name,
                        SourceName = $"{cardInfo.CharaName} ({sc.OwnershipBadge})",
                        SourceType = "Support Card",
                        SourceColor = sc.ColorHex,
                        EstimatedHintLv = 3,
                        EvaluationPoints = sk.Grade > 0 ? sk.Grade : 217,
                        SynergyReason = reason,
                        IsGold = sk.IsGold
                    });
                }
            }
        }

        // Collect skills from Legacies
        foreach (var leg in legacies)
        {
            if (!string.IsNullOrEmpty(leg.HighlightFactor) && !leg.HighlightFactor.Contains("Standard"))
            {
                if (!seenSkillNames.Contains(leg.HighlightFactor))
                {
                    seenSkillNames.Add(leg.HighlightFactor);
                    availableSkills.Add(new SkillHintSuggestion
                    {
                        SkillId = 0,
                        SkillName = leg.HighlightFactor,
                        SourceName = $"Legacy: {leg.CharaName}",
                        SourceType = "Legacy Spark",
                        SourceColor = "#0D9488",
                        EstimatedHintLv = 2,
                        EvaluationPoints = 170,
                        SynergyReason = $"Unique inherited skill factor granting massive mid-race acceleration.",
                        IsGold = false
                    });
                }
            }
        }

        // Rank skills based on preset synergy and points
        var rankedSkills = availableSkills
            .OrderByDescending(s => ScoreSkill(preset.Id, s))
            .Take(6)
            .ToList();

        prediction.RecommendedSkillHints = rankedSkills;
        prediction.TotalEstimatedSkillsAcquired = Math.Max(12, 10 + rankedSkills.Count);
        
        // Typical skill evaluation: ~12-16 skills * 217-300 pts = ~3,000 - 4,800 points
        prediction.EstimatedSkillScore = 3200 + rankedSkills.Sum(s => s.EvaluationPoints);
        prediction.TotalDiscountedSpSaved = rankedSkills.Sum(s => s.SpDiscountPct * 2) * 5;

        // Determine Rank
        int total = prediction.EstimatedTotalScore;
        prediction.PredictedRank = GetRankFromScore(total);
        prediction.RankBadgeColor = GetRankColor(prediction.PredictedRank);

        return prediction;
    }

    private static int ScoreSkill(string presetId, SkillHintSuggestion s)
    {
        int score = s.EvaluationPoints;
        string pid = presetId.ToLowerInvariant();
        string name = s.SkillName.ToLowerInvariant();

        if (s.SourceType == "Legacy Spark") score += 400; // Inherited parent unique spark factor is highest priority
        if (name.Contains("tail held high") || name.Contains("preferred position")) score += 200;
        if (pid.Contains("sprint") && (name.Contains("sprint") || name.Contains("corner") || name.Contains("lead"))) score += 300;
        if (pid.Contains("mile") && (name.Contains("mile") || name.Contains("overtake"))) score += 300;
        if (pid.Contains("long") && (name.Contains("long") || name.Contains("endurance") || name.Contains("stamina"))) score += 300;
        if (pid.Contains("runner") && (name.Contains("runner") || name.Contains("groundwork") || name.Contains("escape"))) score += 300;
        if (pid.Contains("leader") && (name.Contains("leader") || name.Contains("speed star") || name.Contains("pace"))) score += 300;
        if (pid.Contains("chaser") && (name.Contains("chaser") || name.Contains("spurt") || name.Contains("rear"))) score += 300;

        return score;
    }

    private static string GetSkillSynergyReason(string presetId, string skillName)
    {
        string pid = presetId.ToLowerInvariant();
        if (skillName.Contains("Tail Held High")) return "Essential mid-race speed trigger when activating 3+ skills.";
        if (skillName.Contains("Preferred Position")) return "Crucial positioning skill ensuring ideal pacing in the middle leg.";
        if (skillName.Contains("Left-Handed") || skillName.Contains("Right-Handed")) return "Passive speed boost matching core championship racecourse turns.";
        if (skillName.Contains("Straightaways")) return "Velocity multiplier along the homestretch straightaway.";
        if (skillName.Contains("Corners")) return "Prevents deceleration and stabilizes acceleration when banking corners.";
        
        return $"Strong skill point economy hint tailored for {presetId} build requirements.";
    }

    public static string GetRankFromScore(int score)
    {
        if (score >= 19500) return "UG";
        if (score >= 17500) return "SS+";
        if (score >= 15900) return "SS";
        if (score >= 14500) return "S+";
        if (score >= 13500) return "S";
        if (score >= 12100) return "A+";
        if (score >= 10500) return "A";
        if (score >= 9000) return "B+";
        return "B";
    }

    public static string GetRankColor(string rank)
    {
        return rank switch
        {
            "UG" => "#EC4899",
            "SS+" => "#F59E0B",
            "SS" => "#EAB308",
            "S+" => "#10B981",
            "S" => "#06B6D4",
            "A+" => "#3B82F6",
            "A" => "#6366F1",
            _ => "#94A3B8"
        };
    }
}

namespace Umacalc.Models;

public class DeckSlot
{
    public string Speciality { get; set; } = string.Empty; // "Speed", "Stamina", "Power", "Guts", "Wisdom", "Friend"
    public string ShortLabel { get; set; } = string.Empty;
    public int Count { get; set; }
    public string ColorHex { get; set; } = "#3B82F6";
    public string DisplayText => $"{Count}x {ShortLabel}";
}

public class BuildPreset
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DeckRecommendation { get; set; } = string.Empty;
    public string KeyCardsSuggestion { get; set; } = string.Empty;
    public string InheritanceTip { get; set; } = string.Empty;
    public Dictionary<StatType, int> Weights { get; set; } = new();
    public List<DeckSlot> DeckComposition { get; set; } = new();

    public static List<BuildPreset> GetDefaultPresets()
    {
        return new List<BuildPreset>
        {
            // === 1. DISTANCE BUILDS ===
            new BuildPreset
            {
                Id = "sprint",
                Name = "Sprint (1000m - 1400m)",
                Category = "Race Distance",
                Summary = "SPD (High) + WIS + POW",
                Description = "Short distance races require maximum top speed and early lane positioning. Stamina is almost negligible.",
                DeckRecommendation = "4 Speed • 2 Wisdom (or 3 Speed • 2 Wisdom • 1 Friend)",
                KeyCardsSuggestion = "Kitasan Black (SPD), Biko Pegasus (SPD), Fine Motion (WIS), Nice Nature (WIS), Tazuna (FRIEND)",
                InheritanceTip = "Borrow 9★ Power parent sparks to reach 900+ Power without dedicating training turns.",
                Weights = new()
                {
                    { StatType.Speed, 3 },
                    { StatType.Wisdom, 2 },
                    { StatType.Power, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 4, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Wisdom", ShortLabel = "WIS", Count = 2, ColorHex = "#059669" }
                }
            },
            new BuildPreset
            {
                Id = "mile",
                Name = "Mile Meta (1600m)",
                Category = "Race Distance",
                Summary = "SPD (Max) + POW + WIS",
                Description = "The standard Champions Meeting build. Requires overwhelming top speed, strong final-turn acceleration, and wisdom for skills.",
                DeckRecommendation = "3 Speed • 2 Wisdom • 1 Friend (or 3 Speed • 2 Power • 1 Friend)",
                KeyCardsSuggestion = "Kitasan Black (SPD), Narita Top Road (SPD), Fine Motion (WIS), Mr. C.B. (WIS), Tazuna / Kashimoto (FRIEND)",
                InheritanceTip = "Inherit Stamina & Power factors (~600 Stamina target for late spurt without heals).",
                Weights = new()
                {
                    { StatType.Speed, 3 },
                    { StatType.Power, 2 },
                    { StatType.Wisdom, 2 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 3, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Wisdom", ShortLabel = "WIS", Count = 2, ColorHex = "#059669" },
                    new DeckSlot { Speciality = "Friend", ShortLabel = "PAL", Count = 1, ColorHex = "#F59E0B" }
                }
            },
            new BuildPreset
            {
                Id = "medium",
                Name = "Medium (2000m - 2400m)",
                Category = "Race Distance",
                Summary = "SPD + STA + POW + WIS",
                Description = "Balanced classic triple crown races (Derby, Satsuki Sho, Tenno Sho Autumn). Needs solid stamina (~750-800) plus high speed and power.",
                DeckRecommendation = "3 Speed • 2 Stamina • 1 Friend (or 2 Speed • 2 Stamina • 1 Power • 1 Friend)",
                KeyCardsSuggestion = "Super Creek (STA - Pure Gold Maestro), Kitasan Black (SPD), Narita Top Road (SPD), Riko Kashimoto (FRIEND)",
                InheritanceTip = "Borrow 9★ Power parent sparks to supplement acceleration without diluting Stamina training.",
                Weights = new()
                {
                    { StatType.Speed, 2 },
                    { StatType.Stamina, 2 },
                    { StatType.Power, 2 },
                    { StatType.Wisdom, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 3, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Stamina", ShortLabel = "STA", Count = 2, ColorHex = "#EA580C" },
                    new DeckSlot { Speciality = "Friend", ShortLabel = "PAL", Count = 1, ColorHex = "#F59E0B" }
                }
            },
            new BuildPreset
            {
                Id = "long",
                Name = "Long Distance (2500m - 3600m)",
                Category = "Race Distance",
                Summary = "STA (Max) + SPD + POW",
                Description = "Marathon races like Tenno Sho Spring (3200m) and Kikka Sho (3000m). Prioritizes surviving stamina exhaustion above all else.",
                DeckRecommendation = "3 Stamina • 2 Speed • 1 Friend (or 2 Stamina • 2 Speed • 1 Power • 1 Friend)",
                KeyCardsSuggestion = "Super Creek (STA - Maestro), Manhattan Cafe (STA), Kitasan Black (SPD), Riko Kashimoto (FRIEND)",
                InheritanceTip = "Inherit Stamina & Speed factors to comfortably break 1000+ Stamina with 2 Gold recoveries.",
                Weights = new()
                {
                    { StatType.Stamina, 3 },
                    { StatType.Speed, 2 },
                    { StatType.Power, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Stamina", ShortLabel = "STA", Count = 3, ColorHex = "#EA580C" },
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 2, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Friend", ShortLabel = "PAL", Count = 1, ColorHex = "#F59E0B" }
                }
            },
            new BuildPreset
            {
                Id = "dirt",
                Name = "Dirt Track Powerhouse",
                Category = "Race Distance",
                Summary = "POW (Max) + SPD + STA",
                Description = "Dirt racing (Tokyo Daishoten, Champions Cup). Dirt tracks heavily reduce momentum, requiring massive Power acceleration to surge forward.",
                DeckRecommendation = "3 Power • 2 Speed • 1 Friend (or 3 Speed • 2 Power • 1 Friend)",
                KeyCardsSuggestion = "Vodka (POW), El Condor Pasa (POW), Oguri Cap (POW), Kitasan Black (SPD), Tazuna (FRIEND)",
                InheritanceTip = "Borrow 9★ Stamina factors (~600 Stamina target for Dirt mile/medium tracks).",
                Weights = new()
                {
                    { StatType.Power, 3 },
                    { StatType.Speed, 2 },
                    { StatType.Stamina, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Power", ShortLabel = "POW", Count = 3, ColorHex = "#D97706" },
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 2, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Friend", ShortLabel = "PAL", Count = 1, ColorHex = "#F59E0B" }
                }
            },

            // === 2. RUNNING STYLES ===
            new BuildPreset
            {
                Id = "runner",
                Name = "Front Runner (Nige / 逃げ)",
                Category = "Running Style",
                Summary = "SPD (High) + WIS (High) + POW",
                Description = "Taking the lead early and holding it. High Wisdom ensures immediate gate skill activation to take the rail before the first corner.",
                DeckRecommendation = "3 Speed • 2 Wisdom • 1 Friend (or 4 Speed • 2 Wisdom)",
                KeyCardsSuggestion = "Silence Suzuka (SPD), Kitasan Black (SPD), Mihono Bourbon (WIS - Groundwork/Jiban), Fine Motion (WIS)",
                InheritanceTip = "Borrow 9★ Power factors to assist acceleration when coming out of the final turn.",
                Weights = new()
                {
                    { StatType.Speed, 3 },
                    { StatType.Wisdom, 2 },
                    { StatType.Power, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 3, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Wisdom", ShortLabel = "WIS", Count = 2, ColorHex = "#059669" },
                    new DeckSlot { Speciality = "Friend", ShortLabel = "PAL", Count = 1, ColorHex = "#F59E0B" }
                }
            },
            new BuildPreset
            {
                Id = "leader",
                Name = "Pace Setter (Senkou / 先行)",
                Category = "Running Style",
                Summary = "SPD + POW + WIS",
                Description = "Stalking right behind the leader and kicking into top gear entering the home stretch. Power and consistent wisdom triggers are key.",
                DeckRecommendation = "3 Speed • 2 Power • 1 Wisdom (or 3 Speed • 2 Wisdom • 1 Friend)",
                KeyCardsSuggestion = "Kitasan Black (SPD), Taiki Shuttle (SPD), Oguri Cap (POW), Fine Motion (WIS), Tazuna (FRIEND)",
                InheritanceTip = "Inherit Stamina factors to easily clear career race requirements without Stamina card slots.",
                Weights = new()
                {
                    { StatType.Speed, 2 },
                    { StatType.Power, 2 },
                    { StatType.Wisdom, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 3, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Power", ShortLabel = "POW", Count = 2, ColorHex = "#D97706" },
                    new DeckSlot { Speciality = "Wisdom", ShortLabel = "WIS", Count = 1, ColorHex = "#059669" }
                }
            },
            new BuildPreset
            {
                Id = "betwixt",
                Name = "Betwixt / Late (Sashi / 差し)",
                Category = "Running Style",
                Summary = "POW (Max) + SPD + GUT",
                Description = "Mid-pack runner needing immense Power and Guts to punch through heavy traffic and climb slopes in the final 400m.",
                DeckRecommendation = "3 Power • 2 Speed • 1 Friend (or 2 Speed • 2 Power • 1 Guts • 1 Friend)",
                KeyCardsSuggestion = "Vodka (POW), Marvelous Sunday (POW), Mejiro Dober (POW), Kitasan Black (SPD), Riko Kashimoto (FRIEND)",
                InheritanceTip = "Inherit Stamina factors to withstand hard mid-pack bumping and uphill surges.",
                Weights = new()
                {
                    { StatType.Power, 3 },
                    { StatType.Speed, 2 },
                    { StatType.Guts, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Power", ShortLabel = "POW", Count = 3, ColorHex = "#D97706" },
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 2, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Friend", ShortLabel = "PAL", Count = 1, ColorHex = "#F59E0B" }
                }
            },
            new BuildPreset
            {
                Id = "chaser",
                Name = "Chaser / End (Oikomi / 追込)",
                Category = "Running Style",
                Summary = "POW (Max) + SPD + STA",
                Description = "Gold Ship and Narita Taishin style. Lurks at the very back of the pack and launches a devastating sprint around the outside.",
                DeckRecommendation = "3 Power • 2 Stamina • 1 Speed (or 2 Speed • 2 Power • 2 Stamina)",
                KeyCardsSuggestion = "Super Creek (STA - Maestro), Oguri Cap (POW), Vodka (POW), Kitasan Black (SPD), Riko Kashimoto (FRIEND)",
                InheritanceTip = "Inherit Speed & Power sparks so you can dedicate training to Stamina and Power.",
                Weights = new()
                {
                    { StatType.Power, 3 },
                    { StatType.Speed, 2 },
                    { StatType.Stamina, 2 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Power", ShortLabel = "POW", Count = 3, ColorHex = "#D97706" },
                    new DeckSlot { Speciality = "Stamina", ShortLabel = "STA", Count = 2, ColorHex = "#EA580C" },
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 1, ColorHex = "#2563EB" }
                }
            },

            // === 3. META & UTILITY ARCHETYPES ===
            new BuildPreset
            {
                Id = "guts_meta",
                Name = "Guts Training Meta (Root Training)",
                Category = "Meta & Strategy",
                Summary = "GUT (Max) + SPD + WIS",
                Description = "Climax & MANT root meta. Training Guts gives huge secondary Speed and Power gains, capping 3-4 stats simultaneously.",
                DeckRecommendation = "3 Guts • 2 Speed • 1 Wisdom (or 4 Guts • 2 Speed)",
                KeyCardsSuggestion = "Haru Urara (GUT - Free SSR), Matikanefukukitaru (GUT), Yukino Bijin (GUT), Kitasan Black (SPD), Fine Motion (WIS)",
                InheritanceTip = "Borrow 9★ Power factors to effortlessly cap Power alongside Guts and Speed.",
                Weights = new()
                {
                    { StatType.Guts, 3 },
                    { StatType.Speed, 2 },
                    { StatType.Wisdom, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Guts", ShortLabel = "GUT", Count = 3, ColorHex = "#DB2777" },
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 2, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Wisdom", ShortLabel = "WIS", Count = 1, ColorHex = "#059669" }
                }
            },
            new BuildPreset
            {
                Id = "factor_farm",
                Name = "Inheritance & Factor Farming",
                Category = "Meta & Strategy",
                Summary = "All 5 Stats Equal (B-Rank Target)",
                Description = "Targets 600+ (B-rank) across all five stats to maximize the chances of spawning rare 3-star Blue Factors for parent Umas.",
                DeckRecommendation = "2 Speed • 1 Stamina • 1 Power • 1 Guts • 1 Wisdom",
                KeyCardsSuggestion = "High training-effect generalists: Biko Pegasus (SPD), Super Creek (STA), Fine Motion (WIS), Kitasan Black (SPD)",
                InheritanceTip = "Pair with high compatibility parents (G1 trophy overlap) to boost factor inheritance rates.",
                Weights = new()
                {
                    { StatType.Speed, 1 },
                    { StatType.Stamina, 1 },
                    { StatType.Power, 1 },
                    { StatType.Guts, 1 },
                    { StatType.Wisdom, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 2, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Stamina", ShortLabel = "STA", Count = 1, ColorHex = "#EA580C" },
                    new DeckSlot { Speciality = "Power", ShortLabel = "POW", Count = 1, ColorHex = "#D97706" },
                    new DeckSlot { Speciality = "Guts", ShortLabel = "GUT", Count = 1, ColorHex = "#DB2777" },
                    new DeckSlot { Speciality = "Wisdom", ShortLabel = "WIS", Count = 1, ColorHex = "#059669" }
                }
            },
            new BuildPreset
            {
                Id = "wisdom_engine",
                Name = "Wisdom & Skill Point Engine",
                Category = "Meta & Strategy",
                Summary = "WIS (Max) + SPD (High)",
                Description = "Wisdom training recovers energy while building stats. Perfect for stocking massive skill points and building debuff Umas.",
                DeckRecommendation = "3 Wisdom • 2 Speed • 1 Friend (or 4 Wisdom • 2 Speed)",
                KeyCardsSuggestion = "Fine Motion (WIS), Nice Nature (WIS), Mr. C.B. (WIS), Kitasan Black (SPD), Tazuna (FRIEND)",
                InheritanceTip = "Borrow 9★ Stamina & Power factors so you never need to leave Speed and Wisdom training.",
                Weights = new()
                {
                    { StatType.Wisdom, 3 },
                    { StatType.Speed, 2 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Wisdom", ShortLabel = "WIS", Count = 3, ColorHex = "#059669" },
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 2, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Friend", ShortLabel = "PAL", Count = 1, ColorHex = "#F59E0B" }
                }
            },
            new BuildPreset
            {
                Id = "sp",
                Name = "Speed + Power Duo",
                Category = "Meta & Strategy",
                Summary = "SPD (50%) + POW (50%)",
                Description = "Classic focused 2-stat build. All trainable turns split 50/50 between Speed and Power.",
                DeckRecommendation = "3 Speed • 3 Power (or 3 Speed • 2 Power • 1 Friend)",
                KeyCardsSuggestion = "Kitasan Black (SPD), Biko Pegasus (SPD), Vodka (POW), El Condor Pasa (POW), Tazuna (FRIEND)",
                InheritanceTip = "Borrow 9★ Stamina factors for medium/mile races.",
                Weights = new()
                {
                    { StatType.Speed, 1 },
                    { StatType.Power, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 3, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Power", ShortLabel = "POW", Count = 3, ColorHex = "#D97706" }
                }
            },
            new BuildPreset
            {
                Id = "ss",
                Name = "Speed + Stamina Duo",
                Category = "Meta & Strategy",
                Summary = "SPD (50%) + STA (50%)",
                Description = "Dedicated long pace build. All trainable turns split 50/50 between Speed and Stamina.",
                DeckRecommendation = "3 Speed • 3 Stamina (or 3 Speed • 2 Stamina • 1 Friend)",
                KeyCardsSuggestion = "Kitasan Black (SPD), Narita Top Road (SPD), Super Creek (STA), Manhattan Cafe (STA), Kashimoto (FRIEND)",
                InheritanceTip = "Borrow 9★ Power factors to maintain strong corner acceleration.",
                Weights = new()
                {
                    { StatType.Speed, 1 },
                    { StatType.Stamina, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 3, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Stamina", ShortLabel = "STA", Count = 3, ColorHex = "#EA580C" }
                }
            },
            new BuildPreset
            {
                Id = "sw",
                Name = "Speed + Wisdom Duo",
                Category = "Meta & Strategy",
                Summary = "SPD (50%) + WIS (50%)",
                Description = "Maximum turn density and energy efficiency. Speed and Wisdom split 50/50.",
                DeckRecommendation = "3 Speed • 3 Wisdom (or 3 Speed • 2 Wisdom • 1 Friend)",
                KeyCardsSuggestion = "Kitasan Black (SPD), Silence Suzuka (SPD), Fine Motion (WIS), Nice Nature (WIS), Tazuna (FRIEND)",
                InheritanceTip = "Borrow 9★ Power factors.",
                Weights = new()
                {
                    { StatType.Speed, 1 },
                    { StatType.Wisdom, 1 }
                },
                DeckComposition = new()
                {
                    new DeckSlot { Speciality = "Speed", ShortLabel = "SPD", Count = 3, ColorHex = "#2563EB" },
                    new DeckSlot { Speciality = "Wisdom", ShortLabel = "WIS", Count = 3, ColorHex = "#059669" }
                }
            }
        };
    }
}

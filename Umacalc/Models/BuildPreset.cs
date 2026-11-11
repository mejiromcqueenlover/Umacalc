using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Umacalc.Models;

public class DeckSlot
{
    public string Speciality { get; set; } = string.Empty;
    public string ShortLabel { get; set; } = string.Empty;
    public int Count { get; set; }
    public string ColorHex { get; set; } = "#3B82F6";
    public string DisplayText => $"{Count}x {ShortLabel}";
}

public class SupportCardTier
{
    public string CardName { get; set; } = string.Empty;
    public string Speciality { get; set; } = string.Empty; // "SPD", "STA", "POW", "GUT", "WIS", "PAL"
    public string ColorHex { get; set; } = "#3B82F6";
    public string BonusStat { get; set; } = string.Empty; // e.g. "+20% Training Effect"
    public string SkillOrReason { get; set; } = string.Empty;

    // Player ownership attributes from umadump
    public bool IsPlayerOwned { get; set; } = false;
    public string OwnershipBadge { get; set; } = string.Empty; // e.g. "MLB Lv 50", "3LB Lv 45"
    public string OwnershipColor { get; set; } = "#10B981";
}

public class BuildPreset : INotifyPropertyChanged
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

    private List<SupportCardTier> _trainingBonusCards = new();
    public List<SupportCardTier> TrainingBonusCards
    {
        get => _trainingBonusCards;
        set
        {
            _trainingBonusCards = value;
            OnPropertyChanged();
        }
    }

    private List<SupportCardTier> _inspirationBonusCards = new();
    public List<SupportCardTier> InspirationBonusCards
    {
        get => _inspirationBonusCards;
        set
        {
            _inspirationBonusCards = value;
            OnPropertyChanged();
        }
    }

    public List<SupportCardTier> DefaultTrainingBonusCards { get; set; } = new();
    public List<SupportCardTier> DefaultInspirationBonusCards { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ApplyPlayerInventory(PlayerInventory inventory)
    {
        if (DefaultTrainingBonusCards.Count == 0 && TrainingBonusCards.Count > 0)
        {
            DefaultTrainingBonusCards = new List<SupportCardTier>(TrainingBonusCards);
        }
        if (DefaultInspirationBonusCards.Count == 0 && InspirationBonusCards.Count > 0)
        {
            DefaultInspirationBonusCards = new List<SupportCardTier>(InspirationBonusCards);
        }

        TrainingBonusCards = inventory.EvaluateBestTrainingCards(this, 4);
        InspirationBonusCards = inventory.EvaluateBestInspirationCards(this, 3);
    }

    public void RevertToDefaultCards()
    {
        if (DefaultTrainingBonusCards.Count > 0)
        {
            TrainingBonusCards = new List<SupportCardTier>(DefaultTrainingBonusCards);
        }
        if (DefaultInspirationBonusCards.Count > 0)
        {
            InspirationBonusCards = new List<SupportCardTier>(DefaultInspirationBonusCards);
        }
    }

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
                KeyCardsSuggestion = "Kitasan Black, Biko Pegasus, Fine Motion, Nice Nature, Taiki Shuttle",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Biko Pegasus (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+20% Training Effect", SkillOrReason = "Highest universal training bonus in the game + Speed bonus" },
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Power bonus, +35% friendship multiplier" },
                    new SupportCardTier { CardName = "Fine Motion (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Wisdom bonus, huge energy recovery & SP gains" },
                    new SupportCardTier { CardName = "Sweep Tosho (SR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+10% Training Effect", SkillOrReason = "Top budget Speed card with +1 Speed bonus" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Taiki Shuttle (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Gives Corner Accel & Front-Runner Tricks at 40% SP discount" },
                    new SupportCardTier { CardName = "Curren Chan (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Sprint-specific positioning & sprint acceleration hints" },
                    new SupportCardTier { CardName = "Silence Suzuka (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "Hint Lv 2 (+30% Rate)", SkillOrReason = "Left Turn & early lead rail-taking hints" }
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
                KeyCardsSuggestion = "Kitasan Black, Narita Top Road, Fine Motion, Mr. C.B., Vodka, Yaeno Muteki",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+15% Training Effect", SkillOrReason = "Essential speed & power growth engine" },
                    new SupportCardTier { CardName = "Narita Top Road (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+10-20% Fan Scaling", SkillOrReason = "Scales up to +20% training effect with fan count" },
                    new SupportCardTier { CardName = "Fine Motion (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Wisdom bonus & high rainbow value" },
                    new SupportCardTier { CardName = "Vodka (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Power bonus for fierce mile burst" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Yaeno Muteki (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+50% Rate)", SkillOrReason = "Top meta skill: Tail Held High (Play Over) at max discount" },
                    new SupportCardTier { CardName = "Vodka (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Gives Breathless (Hao) gold acceleration skill" },
                    new SupportCardTier { CardName = "Mr. C.B. (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Corner Maestro & mile positioning hints" }
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
                KeyCardsSuggestion = "Super Creek (Maestro), Kitasan Black, Narita Top Road, Riko Kashimoto",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Super Creek (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Stamina bonus, top endurance engine" },
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+15% Training Effect", SkillOrReason = "Reliable Speed & Power growth" },
                    new SupportCardTier { CardName = "Oguri Cap (SR/SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Power bonus, top training efficiency" },
                    new SupportCardTier { CardName = "Riko Kashimoto (SSR)", Speciality = "PAL", ColorHex = "#F59E0B", BonusStat = "+10% Training Effect", SkillOrReason = "Provides +80 Stamina from dating events" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Super Creek (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "Hint Lv 3 (+30% Rate)", SkillOrReason = "Guaranteed Gold Arc Maestro recovery + Corner Recovery" },
                    new SupportCardTier { CardName = "Yaeno Muteki (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+50% Rate)", SkillOrReason = "Tail Held High & Aggressive Middle hints" },
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "Hint Lv 2 (+30% Rate)", SkillOrReason = "Corner Recovery, Straightaway Accel, Arc Meister" }
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
                KeyCardsSuggestion = "Super Creek (Maestro), Manhattan Cafe, Kitasan Black, Seiun Sky, Riko Kashimoto",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Super Creek (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Stamina bonus, mandatory for long distance" },
                    new SupportCardTier { CardName = "Manhattan Cafe (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Stamina bonus, high stamina rainbow stat spikes" },
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+15% Training Effect", SkillOrReason = "Keeps speed near 1200 while focusing stamina" },
                    new SupportCardTier { CardName = "Riko Kashimoto (SSR)", Speciality = "PAL", ColorHex = "#F59E0B", BonusStat = "+10% Training Effect", SkillOrReason = "Stamina management & motivation cures" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Super Creek (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "Hint Lv 3", SkillOrReason = "Arc Maestro (Single most important skill for 3000m+)" },
                    new SupportCardTier { CardName = "Manhattan Cafe (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Long distance passives, Cooldown & Stamina Eater" },
                    new SupportCardTier { CardName = "Seiun Sky (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "Hint Lv 3 (+30% Rate)", SkillOrReason = "Tailwind, Long Straight Acceleration hints" }
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
                KeyCardsSuggestion = "Vodka, El Condor Pasa, Oguri Cap, Kitasan Black, Smart Falcon, Tazuna",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Biko Pegasus (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+20% Training Effect", SkillOrReason = "Powers up every training facility visited" },
                    new SupportCardTier { CardName = "Vodka (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Power bonus, explosive dirt bursts" },
                    new SupportCardTier { CardName = "El Condor Pasa (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Power bonus, high friendship multiplier" },
                    new SupportCardTier { CardName = "Oguri Cap (SR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+15% Training Effect", SkillOrReason = "Top tier budget power multiplier" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "El Condor Pasa (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Dirt Runner, Killer Pace, Mid-race passing" },
                    new SupportCardTier { CardName = "Smart Falcon (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Exclusive dirt skills & position recovery" },
                    new SupportCardTier { CardName = "Vodka (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+30% Rate)", SkillOrReason = "Breathless acceleration hints" }
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
                KeyCardsSuggestion = "Silence Suzuka, Kitasan Black, Mihono Bourbon, Fine Motion, Twin Turbo",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Power bonus, top front-runner engine" },
                    new SupportCardTier { CardName = "Silence Suzuka (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Speed bonus, high specialty priority" },
                    new SupportCardTier { CardName = "Fine Motion (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Wisdom bonus, max skill trigger consistency" },
                    new SupportCardTier { CardName = "Biko Pegasus (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+20% Training Effect", SkillOrReason = "Highest speed multiplier" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Mihono Bourbon (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Mandatory: Groundwork (Jiban) + Runner Corner hints" },
                    new SupportCardTier { CardName = "Silence Suzuka (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "Hint Lv 3 (+50% Rate)", SkillOrReason = "Escape Expert, Rapid Pace, Left Turn" },
                    new SupportCardTier { CardName = "Twin Turbo (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Early Gate Sprint, Leading Tactics" }
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
                KeyCardsSuggestion = "Kitasan Black, Taiki Shuttle, Oguri Cap, Fine Motion, Tazuna",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+15% Training Effect", SkillOrReason = "Universal speed + power multiplier" },
                    new SupportCardTier { CardName = "Oguri Cap (SR/SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Power bonus, top pace-setter card" },
                    new SupportCardTier { CardName = "Fine Motion (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Wisdom bonus, keeps energy high" },
                    new SupportCardTier { CardName = "Taiki Shuttle (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Speed bonus" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Taiki Shuttle (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Final Corner Accel & Leader Strategy at 40% discount" },
                    new SupportCardTier { CardName = "Oguri Cap (SR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+30% Rate)", SkillOrReason = "Corner Speed & Nutritionist hints" },
                    new SupportCardTier { CardName = "Fine Motion (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "Hint Lv 2 (+30% Rate)", SkillOrReason = "Right Turn, Fall Girl, Leader Corner" }
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
                KeyCardsSuggestion = "Vodka, Marvelous Sunday, Mejiro Dober, Yaeno Muteki, Kitasan Black",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Vodka (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Power bonus, core sashi engine" },
                    new SupportCardTier { CardName = "Marvelous Sunday (SR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "+15% Training Effect", SkillOrReason = "Huge universal multiplier for secondary training" },
                    new SupportCardTier { CardName = "Mejiro Dober (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Power bonus" },
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Power bonus" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Yaeno Muteki (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+50% Rate)", SkillOrReason = "Tail Held High, Betwixt Acceleration" },
                    new SupportCardTier { CardName = "Vodka (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Hao, In-Between Position Expert" },
                    new SupportCardTier { CardName = "Mejiro Dober (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Late spurt cornering skills" }
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
                KeyCardsSuggestion = "Super Creek (Maestro), Inari One, Hishi Amazon, Oguri Cap, Kitasan Black",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Super Creek (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Stamina bonus, vital stamina foundation" },
                    new SupportCardTier { CardName = "Oguri Cap (SR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Power bonus, top budget power multiplier" },
                    new SupportCardTier { CardName = "Vodka (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Power bonus" },
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+15% Training Effect", SkillOrReason = "High rainbow power" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Super Creek (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "Hint Lv 3", SkillOrReason = "Arc Maestro (Crucial for Gold Ship long runs)" },
                    new SupportCardTier { CardName = "Inari One (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Chaser Surge, Straightaway Blitz at max discount" },
                    new SupportCardTier { CardName = "Hishi Amazon (SR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+30% Rate)", SkillOrReason = "Chaser Corner Acceleration & Sharp Eye hints" }
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
                KeyCardsSuggestion = "Haru Urara (Free SSR), Matikanefukukitaru, Yukino Bijin, Kitasan Black, Fine Motion",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Haru Urara (SSR)", Speciality = "GUT", ColorHex = "#DB2777", BonusStat = "+15% Training Effect", SkillOrReason = "Free welfare SSR with enormous stat scaling" },
                    new SupportCardTier { CardName = "Matikanefukukitaru (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+10% Training Effect", SkillOrReason = "+1 to all stats bonus at year start" },
                    new SupportCardTier { CardName = "Yukino Bijin (SSR)", Speciality = "GUT", ColorHex = "#DB2777", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Guts bonus" },
                    new SupportCardTier { CardName = "Biko Pegasus (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+20% Training Effect", SkillOrReason = "Boosts Guts facilities when visiting" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Yukino Bijin (SSR)", Speciality = "GUT", ColorHex = "#DB2777", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Non-Stop Girl & Guts Endurance hints" },
                    new SupportCardTier { CardName = "Matikanefukukitaru (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "Hint Lv 3 (+30% Rate)", SkillOrReason = "Super Lucky Seven & mood boosts" },
                    new SupportCardTier { CardName = "Fine Motion (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "Hint Lv 2 (+30% Rate)", SkillOrReason = "Fall Runner & Right Turn hints" }
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
                KeyCardsSuggestion = "Biko Pegasus, Super Creek, Fine Motion, Kitasan Black, Marvelous Sunday",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Biko Pegasus (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+20% Training Effect", SkillOrReason = "Universal multiplier across all 5 stat facilities" },
                    new SupportCardTier { CardName = "Marvelous Sunday (SR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "+15% Training Effect", SkillOrReason = "Boosts any training facility visited" },
                    new SupportCardTier { CardName = "Oguri Cap (SR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+15% Training Effect", SkillOrReason = "High power & general efficiency" },
                    new SupportCardTier { CardName = "Super Creek (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "+10% Training Effect", SkillOrReason = "Effortlessly guarantees 600+ Stamina" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "Hint Lv 2", SkillOrReason = "Universal white skills to pass down to offspring" },
                    new SupportCardTier { CardName = "Fine Motion (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "Hint Lv 2", SkillOrReason = "Right Turn, Fall Girl, Corner Wisdom" },
                    new SupportCardTier { CardName = "Super Creek (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "Hint Lv 3", SkillOrReason = "Corner Recovery white skill for factor roll" }
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
                KeyCardsSuggestion = "Fine Motion, Nice Nature, Mr. C.B., Kitasan Black, Tazuna",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Fine Motion (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Wisdom bonus, god-tier rainbow gains" },
                    new SupportCardTier { CardName = "Nice Nature (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Wisdom bonus, high skill point generation" },
                    new SupportCardTier { CardName = "Mr. C.B. (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Wisdom bonus" },
                    new SupportCardTier { CardName = "Biko Pegasus (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+20% Training Effect", SkillOrReason = "Maximizes Speed gains on mixed turns" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Nice Nature (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "Hint Lv 3 (+50% Rate)", SkillOrReason = "Red Debuffs: Hesitation, Trick, Murmur, Evil Eye" },
                    new SupportCardTier { CardName = "Mr. C.B. (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Corner Maestro & position recovery hints" },
                    new SupportCardTier { CardName = "Fine Motion (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "Hint Lv 2 (+30% Rate)", SkillOrReason = "Right Turn, Fall Girl, Corner Wisdom" }
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
                KeyCardsSuggestion = "Kitasan Black, Biko Pegasus, Vodka, El Condor Pasa, Oguri Cap",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Biko Pegasus (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+20% Training Effect", SkillOrReason = "Highest speed multiplier" },
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Power bonus" },
                    new SupportCardTier { CardName = "Vodka (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Power bonus" },
                    new SupportCardTier { CardName = "Oguri Cap (SR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Power bonus" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Yaeno Muteki (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+50% Rate)", SkillOrReason = "Tail Held High" },
                    new SupportCardTier { CardName = "Vodka (SSR)", Speciality = "POW", ColorHex = "#D97706", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Hao acceleration hints" }
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
                KeyCardsSuggestion = "Kitasan Black, Narita Top Road, Super Creek, Manhattan Cafe",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Super Creek (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Stamina bonus, top recovery engine" },
                    new SupportCardTier { CardName = "Manhattan Cafe (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Stamina bonus" },
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+15% Training Effect", SkillOrReason = "Keeps speed near 1200 while stamina grows" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Super Creek (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "Hint Lv 3", SkillOrReason = "Arc Maestro" },
                    new SupportCardTier { CardName = "Manhattan Cafe (SSR)", Speciality = "STA", ColorHex = "#EA580C", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Long distance stamina passives" }
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
                KeyCardsSuggestion = "Kitasan Black, Silence Suzuka, Fine Motion, Nice Nature, Mr. C.B.",
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
                },
                TrainingBonusCards = new()
                {
                    new SupportCardTier { CardName = "Biko Pegasus (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+20% Training Effect", SkillOrReason = "Max speed training multiplier" },
                    new SupportCardTier { CardName = "Fine Motion (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Wisdom bonus" },
                    new SupportCardTier { CardName = "Kitasan Black (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "+15% Training Effect", SkillOrReason = "+1 Power bonus" },
                    new SupportCardTier { CardName = "Nice Nature (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "+10% Training Effect", SkillOrReason = "+1 Wisdom bonus" }
                },
                InspirationBonusCards = new()
                {
                    new SupportCardTier { CardName = "Silence Suzuka (SSR)", Speciality = "SPD", ColorHex = "#2563EB", BonusStat = "Hint Lv 3 (+50% Rate)", SkillOrReason = "Escape Expert & Left Turn hints" },
                    new SupportCardTier { CardName = "Mihono Bourbon (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Groundwork (Jiban) & Runner Corner" },
                    new SupportCardTier { CardName = "Mr. C.B. (SSR)", Speciality = "WIS", ColorHex = "#059669", BonusStat = "Hint Lv 3 (+40% Rate)", SkillOrReason = "Corner Maestro" }
                }
            }
        };
    }
}

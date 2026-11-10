namespace Umacalc.Models;

public class ScenarioData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string JapaneseName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DefaultTrainableTurns { get; set; } = 60;
    public int StatCap { get; set; } = 1200;
    public string Tips { get; set; } = string.Empty;

    public string DisplayTitle => $"{Name} ({JapaneseName})";

    public static List<ScenarioData> GetGlobalScenarios()
    {
        return new List<ScenarioData>
        {
            new ScenarioData
            {
                Id = "ura",
                Name = "URA Finals",
                JapaneseName = "新設！URAファイナルズ",
                Description = "The classic foundational training scenario. Pure friendship training and rainbow procs.",
                DefaultTrainableTurns = 60,
                StatCap = 1200,
                Tips = "Aim for high bond levels early to activate Rainbow Friendship Training by Classic summer camp."
            },
            new ScenarioData
            {
                Id = "aoharu",
                Name = "Aoharu Cup (Unity Cup)",
                JapaneseName = "アオハル杯 ～輝け、チームの絆～",
                Description = "Team recruitment and soul explosion training. Features 5 scheduled team tournament showdowns.",
                DefaultTrainableTurns = 56,
                StatCap = 1200,
                Tips = "Prioritize team practice arrows and trigger Aoharu Explosions on stats you need to max out."
            },
            new ScenarioData
            {
                Id = "mant",
                Name = "Make a new track!! (MANT / Climax)",
                JapaneseName = "Make a new track!! ～クライマックス開幕～",
                Description = "Shop coin & item scenario. Default is 50 trainable turns (~20-25 races). Heavy 35-race coin farming builds use ~40-42 turns, while casual/stat builds use 50-55 turns.",
                DefaultTrainableTurns = 50,
                StatCap = 1200,
                Tips = "Adjust turns to match your race count: heavy coin farming (~35 races) = ~40 turns; normal runs (~20 races) = ~50-55 turns. Use the +5 button anytime!"
            },
            new ScenarioData
            {
                Id = "grandlive",
                Name = "Our Grand Live",
                JapaneseName = "つなげ、照らせ、ひかれ。私たちのグランドライブ",
                Description = "Music lesson and performance concert scenario. Concerts take place at milestones with very high trainable turns.",
                DefaultTrainableTurns = 64,
                StatCap = 1600,
                Tips = "Buy songs early to stack friendship multipliers and lesson discounts for explosive end-game training."
            },
            new ScenarioData
            {
                Id = "grandmasters",
                Name = "Grand Masters - To the Heirs",
                JapaneseName = "グランドマスターズ -継ぐ者達へ-",
                Description = "Three Goddesses scenario (Growt, Stella, Apollo). Collect spirit shards to unleash powerful Goddess wisdom activations.",
                DefaultTrainableTurns = 63,
                StatCap = 1700,
                Tips = "Time Goddess Wisdom activations during Summer Camp or multi-rainbow turns for exponential stat spikes."
            },
            new ScenarioData
            {
                Id = "larc",
                Name = "Project L'Arc",
                JapaneseName = "プロジェクトL'Arc",
                Description = "Overseas expedition scenario to conquer the Prix de l'Arc de Triomphe in France. Uses SS Match meter gains.",
                DefaultTrainableTurns = 60,
                StatCap = 1750,
                Tips = "Build SS Match meter fast before foreign expeditions to maximize training level boosts in France."
            },
            new ScenarioData
            {
                Id = "uaf",
                Name = "U.A.F. Ready GO!",
                JapaneseName = "U.A.F. Ready GO! ～アスリートのキラメキ～",
                Description = "15-sport competition scenario with Red/Blue/Yellow color disciplines and intense synergy gear links.",
                DefaultTrainableTurns = 62,
                StatCap = 1800,
                Tips = "Chain sport level completions to trigger massive color bursts across your max-out target stats."
            },
            new ScenarioData
            {
                Id = "custom",
                Name = "Custom Scenario",
                JapaneseName = "カスタムシナリオ",
                Description = "Fully configurable trainable turns for specialized testing, custom racing schedules, or future scenarios.",
                DefaultTrainableTurns = 60,
                StatCap = 1200,
                Tips = "Set your exact trainable turns directly to match your Uma's schedule."
            }
        };
    }
}

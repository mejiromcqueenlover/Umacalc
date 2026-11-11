using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umacalc.Models;

public class SupportCardInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("charaId")]
    public int CharaId { get; set; }

    [JsonPropertyName("charaName")]
    public string CharaName { get; set; } = string.Empty;

    [JsonPropertyName("cardTitle")]
    public string CardTitle { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("rarity")]
    public string Rarity { get; set; } = "SSR";

    [JsonPropertyName("speciality")]
    public string Speciality { get; set; } = "SPD";

    [JsonPropertyName("colorHex")]
    public string ColorHex { get; set; } = "#2563EB";

    [JsonPropertyName("trainingBonus")]
    public int[] TrainingBonus { get; set; } = Array.Empty<int>();

    [JsonPropertyName("hintLevel")]
    public int[] HintLevel { get; set; } = Array.Empty<int>();

    [JsonPropertyName("hintRate")]
    public int[] HintRate { get; set; } = Array.Empty<int>();

    [JsonPropertyName("specialtyRate")]
    public int[] SpecialtyRate { get; set; } = Array.Empty<int>();

    public int GetTrainingBonus(int lb)
    {
        if (TrainingBonus == null || TrainingBonus.Length == 0) return 0;
        int idx = Math.Clamp(lb, 0, TrainingBonus.Length - 1);
        return TrainingBonus[idx];
    }

    public int GetHintLevel(int lb)
    {
        if (HintLevel == null || HintLevel.Length == 0) return 1;
        int idx = Math.Clamp(lb, 0, HintLevel.Length - 1);
        return HintLevel[idx];
    }

    public int GetHintRate(int lb)
    {
        if (HintRate == null || HintRate.Length == 0) return 0;
        int idx = Math.Clamp(lb, 0, HintRate.Length - 1);
        return HintRate[idx];
    }

    public int GetSpecialtyRate(int lb)
    {
        if (SpecialtyRate == null || SpecialtyRate.Length == 0) return 0;
        int idx = Math.Clamp(lb, 0, SpecialtyRate.Length - 1);
        return SpecialtyRate[idx];
    }
}

public class SupportCardCatalog
{
    private static SupportCardCatalog? _instance;
    public static SupportCardCatalog Instance => _instance ??= LoadCatalog();

    private readonly Dictionary<int, SupportCardInfo> _cardsById = new();
    private readonly List<SupportCardInfo> _allCards = new();

    public IReadOnlyList<SupportCardInfo> AllCards => _allCards;

    private SupportCardCatalog(IEnumerable<SupportCardInfo> cards)
    {
        foreach (var c in cards)
        {
            _cardsById[c.Id] = c;
            _allCards.Add(c);
        }
    }

    public SupportCardInfo? GetCard(int id)
    {
        _cardsById.TryGetValue(id, out var card);
        return card;
    }

    private static SupportCardCatalog LoadCatalog()
    {
        try
        {
            // 1. Try local Assets directory relative to executable or AppContext
            string[] possiblePaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Assets", "support_card_catalog.json"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "support_card_catalog.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "support_card_catalog.json"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Umacalc\Assets\support_card_catalog.json"))
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var items = JsonSerializer.Deserialize<List<SupportCardInfo>>(json);
                    if (items != null && items.Count > 0)
                    {
                        return new SupportCardCatalog(items);
                    }
                }
            }

            // 2. Try Embedded resource stream
            var asm = Assembly.GetExecutingAssembly();
            var resName = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("support_card_catalog.json", StringComparison.OrdinalIgnoreCase));
            if (resName != null)
            {
                using var stream = asm.GetManifestResourceStream(resName);
                if (stream != null)
                {
                    var items = JsonSerializer.Deserialize<List<SupportCardInfo>>(stream);
                    if (items != null && items.Count > 0)
                    {
                        return new SupportCardCatalog(items);
                    }
                }
            }
        }
        catch
        {
            // Fallback handled below
        }

        return new SupportCardCatalog(Enumerable.Empty<SupportCardInfo>());
    }
}

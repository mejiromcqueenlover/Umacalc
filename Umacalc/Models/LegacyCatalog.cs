using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Umacalc.Models;

public class LegacyCatalog
{
    private static readonly Lazy<LegacyCatalog> _lazy = new(() => new LegacyCatalog());
    public static LegacyCatalog Instance => _lazy.Value;

    public Dictionary<int, string> FactorNames { get; } = new();
    public Dictionary<int, string> CharaNames { get; } = new();
    public Dictionary<int, int> CardToChara { get; } = new();

    private LegacyCatalog()
    {
        LoadCatalog();
    }

    private void LoadCatalog()
    {
        try
        {
            string? json = null;
            var assembly = Assembly.GetExecutingAssembly();
            var resName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("legacy_catalog.json", StringComparison.OrdinalIgnoreCase));

            if (resName != null)
            {
                using var stream = assembly.GetManifestResourceStream(resName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    json = reader.ReadToEnd();
                }
            }

            if (string.IsNullOrEmpty(json))
            {
                string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "legacy_catalog.json");
                if (File.Exists(localPath))
                {
                    json = File.ReadAllText(localPath);
                }
            }

            if (!string.IsNullOrEmpty(json))
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("factorNames", out var fNames))
                {
                    foreach (var prop in fNames.EnumerateObject())
                    {
                        if (int.TryParse(prop.Name, out int fid))
                            FactorNames[fid] = prop.Value.GetString() ?? $"Factor #{fid}";
                    }
                }

                if (root.TryGetProperty("charaNames", out var cNames))
                {
                    foreach (var prop in cNames.EnumerateObject())
                    {
                        if (int.TryParse(prop.Name, out int cid))
                            CharaNames[cid] = prop.Value.GetString() ?? $"Chara #{cid}";
                    }
                }

                if (root.TryGetProperty("cardToChara", out var c2c))
                {
                    foreach (var prop in c2c.EnumerateObject())
                    {
                        if (int.TryParse(prop.Name, out int cardId))
                            CardToChara[cardId] = prop.Value.GetInt32();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load legacy_catalog.json: {ex.Message}");
        }
    }

    public string GetCharaName(int cardId)
    {
        if (CardToChara.TryGetValue(cardId, out int charaId))
        {
            if (CharaNames.TryGetValue(charaId, out string? name))
            {
                return name;
            }
        }
        return $"Chara #{cardId}";
    }

    public string GetFactorName(int factorId)
    {
        if (FactorNames.TryGetValue(factorId, out string? name))
        {
            return name;
        }
        return $"Factor #{factorId}";
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Umacalc.Models;

public class SkillInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Grade { get; set; }
    public int Rarity { get; set; }
    public int Icon { get; set; }
    public bool IsGold => Rarity >= 2 || Grade >= 260;
}

public class SkillCatalog
{
    private static readonly Lazy<SkillCatalog> _lazy = new(() => new SkillCatalog());
    public static SkillCatalog Instance => _lazy.Value;

    public Dictionary<int, SkillInfo> Skills { get; } = new();
    public Dictionary<int, List<int>> CardSkills { get; } = new();

    private SkillCatalog()
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
                .FirstOrDefault(n => n.EndsWith("skill_catalog.json", StringComparison.OrdinalIgnoreCase));

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
                string[] possiblePaths = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "Assets", "skill_catalog.json"),
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "skill_catalog.json"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "skill_catalog.json"),
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Umacalc\Assets\skill_catalog.json"))
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        json = File.ReadAllText(path);
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(json))
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("skills", out var skillsElem))
                {
                    foreach (var prop in skillsElem.EnumerateObject())
                    {
                        if (int.TryParse(prop.Name, out int sid))
                        {
                            var sObj = prop.Value;
                            Skills[sid] = new SkillInfo
                            {
                                Id = sid,
                                Name = sObj.GetProperty("name").GetString() ?? $"Skill #{sid}",
                                Grade = sObj.GetProperty("grade").GetInt32(),
                                Rarity = sObj.GetProperty("rarity").GetInt32(),
                                Icon = sObj.GetProperty("icon").GetInt32()
                            };
                        }
                    }
                }

                if (root.TryGetProperty("cardSkills", out var csElem))
                {
                    foreach (var prop in csElem.EnumerateObject())
                    {
                        if (int.TryParse(prop.Name, out int cardId))
                        {
                            var list = new List<int>();
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                list.Add(item.GetInt32());
                            }
                            CardSkills[cardId] = list;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load skill_catalog.json: {ex.Message}");
        }
    }

    public SkillInfo? GetSkill(int skillId)
    {
        return Skills.GetValueOrDefault(skillId);
    }

    public List<SkillInfo> GetSkillsForCard(int cardId)
    {
        if (CardSkills.TryGetValue(cardId, out var sids))
        {
            return sids.Select(id => Skills.GetValueOrDefault(id)).Where(s => s != null).Cast<SkillInfo>().ToList();
        }
        return new List<SkillInfo>();
    }
}

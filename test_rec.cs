using System;
using System.Linq;
using Umacalc.Models;
var presets = BuildPreset.GetDefaultPresets();
var p = presets.First(x => x.Id == "leader" || x.Id == "pace_setter");
Console.WriteLine($"PRESET: {p.Name}");
foreach (var c in p.TrainingBonusCards) Console.WriteLine($"  [DEFAULT TRAIN] {c.Speciality} {c.CardName}");
foreach (var c in p.FriendshipBonusCards) Console.WriteLine($"  [DEFAULT FRIEND] {c.Speciality} {c.CardName}");
foreach (var c in p.InspirationBonusCards) Console.WriteLine($"  [DEFAULT INSP] {c.Speciality} {c.CardName}");
var inv = PlayerInventory.LoadFromFile(@"C:\Users\mejiromcqueenlover\Downloads\support_card_data.json");
if (inv != null) {
    var tc = inv.EvaluateBestTrainingCards(p, 4);
    Console.WriteLine("OWNED TRAINING:");
    foreach (var c in tc) Console.WriteLine($"  {c.Speciality} {c.CardName} ({c.OwnershipBadge})");
    var fc = inv.EvaluateBestFriendshipCards(p, 3);
    Console.WriteLine("OWNED FRIENDSHIP:");
    foreach (var c in fc) Console.WriteLine($"  {c.Speciality} {c.CardName} ({c.OwnershipBadge})");
    var ic = inv.EvaluateBestInspirationCards(p, 3);
    Console.WriteLine("OWNED INSPIRATION:");
    foreach (var c in ic) Console.WriteLine($"  {c.Speciality} {c.CardName} ({c.OwnershipBadge})");
}
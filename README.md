# Umacalc - Umamusume Stat Gain Calculator

A clean, responsive C# WPF desktop companion application designed for *Umamusume: Pretty Derby* players to plan, optimize, and track **trainable turn budgets** across their career runs.

---

## 🌟 Key Features

1. **Pure Trainable Turn Focus**:
   - Only turns spent training are counted—no clutter from resting or racing turns.
   - Automatically initializes with the exact trainable turn count per scenario:
     - **URA Finals**: 60 trainable turns
     - **Aoharu Cup (Unity Cup)**: 56 trainable turns
     - **Make a new track!! (MANT / Climax)**: 50 trainable turns (with `+5` quick-adds for casual/normal runs)
     - **Our Grand Live**: 64 trainable turns
     - **Grand Masters - To the Heirs**: 63 trainable turns
     - **Project L'Arc**: 60 trainable turns
     - **U.A.F. Ready GO!**: 62 trainable turns
     - **Custom Scenario**: Customizable trainable turns budget
   - Quick `+` and `-` buttons in the banner let you tweak your total budget at any time.

2. **Support Card Speciality & Deck Recommendations**:
   - Every build preset includes an exact **6-Card Support Deck Blueprint** (card specialities, counts, and deck split ratio).
   - Shows **Key Example Cards / Staples** (e.g. *Kitasan Black*, *Super Creek*, *Fine Motion*, *Tazuna*, *Riko Kashimoto*).
   - Provides **Parent Factor Inheritance Advice** (e.g. which 9★ sparks to borrow so you don't waste turns training off-stats).
   - Visual deck slot pills highlight the recommended card types on every preset button and in the active deck panel.

3. **15 One-Click Build Presets**:
   - **Race Distance**:
     - *Sprint (1000m - 1400m)*: `Deck: 4 SPD • 2 WIS`
     - *Mile Meta (1600m)*: `Deck: 3 SPD • 2 WIS • 1 PAL`
     - *Medium (2000m - 2400m)*: `Deck: 3 SPD • 2 STA • 1 PAL`
     - *Long Distance (2500m - 3600m)*: `Deck: 3 STA • 2 SPD • 1 PAL`
     - *Dirt Track Powerhouse*: `Deck: 3 POW • 2 SPD • 1 PAL`
   - **Running Style**:
     - *Front Runner (Nige / 逃げ)*: `Deck: 3 SPD • 2 WIS • 1 PAL`
     - *Pace Setter (Senkou / 先行)*: `Deck: 3 SPD • 2 POW • 1 WIS`
     - *Betwixt / Late (Sashi / 差し)*: `Deck: 3 POW • 2 SPD • 1 PAL`
     - *Chaser / End (Oikomi / 追込)*: `Deck: 3 POW • 2 STA • 1 SPD`
   - **Meta & Strategy**:
     - *Guts Root Meta (根性育成)*: `Deck: 3 GUT • 2 SPD • 1 WIS`
     - *Inheritance Factor Farm (All B)*: `Deck: 2 SPD • 1 STA • 1 POW • 1 GUT • 1 WIS`
     - *Wisdom & SP Farm*: `Deck: 3 WIS • 2 SPD • 1 PAL`
     - *Speed + Power Duo*: `Deck: 3 SPD • 3 POW`
     - *Speed + Stamina Duo*: `Deck: 3 SPD • 3 STA`
     - *Speed + Wisdom Duo*: `Deck: 3 SPD • 3 WIS`

---

## 🚀 How to Run

```powershell
dotnet run --project Umacalc\Umacalc.csproj
```

### Run Tests
```powershell
.\Umacalc.Tests\bin\Debug\net10.0-windows\Umacalc.Tests.exe
```

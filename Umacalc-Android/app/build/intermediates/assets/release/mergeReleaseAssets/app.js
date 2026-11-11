/**
 * Umacalc - Umamusume Stat Gain & Turn Calculator (Android & Mobile Core)
 */

const SCENARIOS = [
  { id: "ura", name: "URA Finals", jpName: "新設！URAファイナルズ", turns: 60, statCap: 1200, tips: "Aim for high bond levels early (Junior year) to activate Rainbow Friendship Training by Classic summer camp. Only trainable turns are counted." },
  { id: "aoharu", name: "Aoharu Cup", jpName: "アオハル杯 ～輝け、チームの絆～", turns: 56, statCap: 1200, tips: "Prioritize Aoharu Bomb detonations and Soul Bursts. Training with teammates boosts multiple stats simultaneously." },
  { id: "mant", name: "MANT / Climax", jpName: "Make a new track!! ～クライマックス開幕～", turns: 50, statCap: 1200, tips: "High race frequency (25-35+ races) builds shop coins for mega BBQ, whistles, and stat manuals. If you run fewer races, click '+5' in the budget box to add extra training turns!" },
  { id: "grandlive", name: "Our Grand Live", jpName: "つなげ、照らせ、ひびけ。ひびけ！グランドライブ", turns: 64, statCap: 1600, tips: "High turn density. Maximize Performance Coins to purchase Live songs and concert buffs early for massive friendship spikes." },
  { id: "grandmasters", name: "Grand Masters", jpName: "プロジェクトL'Arc / グランドマスターズ", turns: 63, statCap: 2000, tips: "Balance Wisdom & Goddess Fragment accumulation to activate Triple Goddess Knowledge bursts during summer camps." },
  { id: "larc", name: "Project L'Arc", jpName: "プロジェクトL'Arc", turns: 60, statCap: 2000, tips: "Focus on SS Match activations to increase Rivalry levels before overseas trips to Longchamp." },
  { id: "uaf", name: "U.A.F. Ready GO!", jpName: "U.A.F. Ready GO! ～アスリートのキラメキ～", turns: 62, statCap: 2000, tips: "Rotate sports genres to level up all 15 Olympic disciplines evenly for the grand finale." },
  { id: "custom", name: "Custom Scenario", jpName: "カスタム", turns: 60, statCap: 1200, tips: "Manually adjust trainable turns with '+' and '-' to match your exact run strategy." }
];

const PRESETS = [
  {
    id: "sprint",
    name: "Sprint (1000m - 1400m)",
    category: "Race Distance",
    weights: { Speed: 3, Wisdom: 2, Power: 1, Stamina: 0, Guts: 0 },
    deck: "4 Speed • 2 Wisdom (or 3 Speed • 2 Wisdom • 1 Friend)",
    composition: [
      { label: "4x SPD", color: "#2563EB" },
      { label: "2x WIS", color: "#059669" }
    ],
    inheritance: "Borrow 9★ Power parent sparks to reach 900+ Power without dedicating training turns.",
    trainingCards: [
      { name: "Biko Pegasus (SSR)", spec: "SPD", color: "#2563EB", bonus: "+20% Training Effect", reason: "Highest universal training multiplier in the game + Speed bonus" },
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "+15% Training Effect", reason: "+1 Power bonus, +35% friendship multiplier" },
      { name: "Fine Motion (SSR)", spec: "WIS", color: "#059669", bonus: "+15% Training Effect", reason: "+1 Wisdom bonus, huge energy recovery & SP gains" },
      { name: "Sweep Tosho (SR)", spec: "SPD", color: "#2563EB", bonus: "+10% Training Effect", reason: "Top budget Speed card with +1 Speed bonus" }
    ],
    inspirationCards: [
      { name: "Taiki Shuttle (SSR)", spec: "SPD", color: "#2563EB", bonus: "Hint Lv 3 (+40% Rate)", reason: "Corner Accel & Front-Runner Tricks at 40% SP discount" },
      { name: "Curren Chan (SSR)", spec: "WIS", color: "#059669", bonus: "Hint Lv 3 (+40% Rate)", reason: "Sprint-specific positioning & sprint acceleration hints" },
      { name: "Silence Suzuka (SSR)", spec: "SPD", color: "#2563EB", bonus: "Hint Lv 2 (+30% Rate)", reason: "Left Turn & early lead rail-taking hints" }
    ]
  },
  {
    id: "mile",
    name: "Mile Meta (1600m)",
    category: "Race Distance",
    weights: { Speed: 3, Power: 2, Wisdom: 2, Stamina: 0, Guts: 0 },
    deck: "3 Speed • 2 Wisdom • 1 Friend (or 3 Speed • 2 Power • 1 Friend)",
    composition: [
      { label: "3x SPD", color: "#2563EB" },
      { label: "2x WIS", color: "#059669" },
      { label: "1x PAL", color: "#F59E0B" }
    ],
    inheritance: "Inherit Stamina & Power factors (~600 Stamina target for late spurt without heals).",
    trainingCards: [
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "+15% Training Effect", reason: "Essential speed & power growth engine" },
      { name: "Narita Top Road (SSR)", spec: "SPD", color: "#2563EB", bonus: "+10-20% Fan Scaling", reason: "Scales up to +20% training effect with fan count" },
      { name: "Fine Motion (SSR)", spec: "WIS", color: "#059669", bonus: "+15% Training Effect", reason: "+1 Wisdom bonus & high rainbow value" },
      { name: "Vodka (SSR)", spec: "POW", color: "#D97706", bonus: "+10% Training Effect", reason: "+1 Power bonus for fierce mile burst" }
    ],
    inspirationCards: [
      { name: "Yaeno Muteki (SSR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+50% Rate)", reason: "Tail Held High (Play Over / 尻尾上がり) at max discount" },
      { name: "Vodka (SSR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+40% Rate)", reason: "Breathless (Hao) gold acceleration skill" },
      { name: "Mr. C.B. (SSR)", spec: "WIS", color: "#059669", bonus: "Hint Lv 3 (+40% Rate)", reason: "Corner Maestro & mile positioning hints" }
    ]
  },
  {
    id: "medium",
    name: "Medium (2000m - 2400m)",
    category: "Race Distance",
    weights: { Speed: 2, Stamina: 2, Power: 2, Wisdom: 1, Guts: 0 },
    deck: "3 Speed • 2 Stamina • 1 Friend (or 2 Speed • 2 Stamina • 1 Power • 1 Friend)",
    composition: [
      { label: "3x SPD", color: "#2563EB" },
      { label: "2x STA", color: "#EA580C" },
      { label: "1x PAL", color: "#F59E0B" }
    ],
    inheritance: "Borrow 9★ Power parent sparks to supplement acceleration without diluting Stamina training.",
    trainingCards: [
      { name: "Super Creek (SSR)", spec: "STA", color: "#EA580C", bonus: "+10% Training Effect", reason: "+1 Stamina bonus, top endurance engine" },
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "+15% Training Effect", reason: "Reliable Speed & Power growth" },
      { name: "Oguri Cap (SR/SSR)", spec: "POW", color: "#D97706", bonus: "+15% Training Effect", reason: "+1 Power bonus, top training efficiency" },
      { name: "Riko Kashimoto (SSR)", spec: "PAL", color: "#F59E0B", bonus: "+10% Training Effect", reason: "Provides +80 Stamina from dating events" }
    ],
    inspirationCards: [
      { name: "Super Creek (SSR)", spec: "STA", color: "#EA580C", bonus: "Hint Lv 3 (+30% Rate)", reason: "Guaranteed Gold Arc Maestro recovery + Corner Recovery" },
      { name: "Yaeno Muteki (SSR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+50% Rate)", reason: "Tail Held High & Aggressive Middle hints" },
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "Hint Lv 2 (+30% Rate)", reason: "Corner Recovery, Straightaway Accel, Arc Meister" }
    ]
  },
  {
    id: "long",
    name: "Long Distance (2500m - 3600m)",
    category: "Race Distance",
    weights: { Stamina: 3, Speed: 2, Power: 1, Wisdom: 0, Guts: 0 },
    deck: "3 Stamina • 2 Speed • 1 Friend (or 2 Stamina • 2 Speed • 1 Power • 1 Friend)",
    composition: [
      { label: "3x STA", color: "#EA580C" },
      { label: "2x SPD", color: "#2563EB" },
      { label: "1x PAL", color: "#F59E0B" }
    ],
    inheritance: "Inherit Stamina & Speed factors to comfortably break 1000+ Stamina with 2 Gold recoveries.",
    trainingCards: [
      { name: "Super Creek (SSR)", spec: "STA", color: "#EA580C", bonus: "+10% Training Effect", reason: "+1 Stamina bonus, mandatory for long distance" },
      { name: "Manhattan Cafe (SSR)", spec: "STA", color: "#EA580C", bonus: "+10% Training Effect", reason: "+1 Stamina bonus, high stamina rainbow stat spikes" },
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "+15% Training Effect", reason: "Keeps speed near 1200 while focusing stamina" },
      { name: "Riko Kashimoto (SSR)", spec: "PAL", color: "#F59E0B", bonus: "+10% Training Effect", reason: "Stamina management & motivation cures" }
    ],
    inspirationCards: [
      { name: "Super Creek (SSR)", spec: "STA", color: "#EA580C", bonus: "Hint Lv 3", reason: "Arc Maestro (Single most important skill for 3000m+)" },
      { name: "Manhattan Cafe (SSR)", spec: "STA", color: "#EA580C", bonus: "Hint Lv 3 (+40% Rate)", reason: "Long distance passives, Cooldown & Stamina Eater" },
      { name: "Seiun Sky (SSR)", spec: "STA", color: "#EA580C", bonus: "Hint Lv 3 (+30% Rate)", reason: "Tailwind, Long Straight Acceleration hints" }
    ]
  },
  {
    id: "dirt",
    name: "Dirt Track Powerhouse",
    category: "Race Distance",
    weights: { Power: 3, Speed: 2, Stamina: 1, Wisdom: 0, Guts: 0 },
    deck: "3 Power • 2 Speed • 1 Friend (or 3 Speed • 2 Power • 1 Friend)",
    composition: [
      { label: "3x POW", color: "#D97706" },
      { label: "2x SPD", color: "#2563EB" },
      { label: "1x PAL", color: "#F59E0B" }
    ],
    inheritance: "Borrow 9★ Stamina factors (~600 Stamina target for Dirt mile/medium tracks).",
    trainingCards: [
      { name: "Biko Pegasus (SSR)", spec: "SPD", color: "#2563EB", bonus: "+20% Training Effect", reason: "Powers up every training facility visited" },
      { name: "Vodka (SSR)", spec: "POW", color: "#D97706", bonus: "+10% Training Effect", reason: "+1 Power bonus, explosive dirt bursts" },
      { name: "El Condor Pasa (SSR)", spec: "POW", color: "#D97706", bonus: "+10% Training Effect", reason: "+1 Power bonus, high friendship multiplier" },
      { name: "Oguri Cap (SR)", spec: "POW", color: "#D97706", bonus: "+15% Training Effect", reason: "Top tier budget power multiplier" }
    ],
    inspirationCards: [
      { name: "El Condor Pasa (SSR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+40% Rate)", reason: "Dirt Runner, Killer Pace, Mid-race passing" },
      { name: "Smart Falcon (SSR)", spec: "SPD", color: "#2563EB", bonus: "Hint Lv 3 (+40% Rate)", reason: "Exclusive dirt skills & position recovery" },
      { name: "Vodka (SSR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+30% Rate)", reason: "Breathless acceleration hints" }
    ]
  },
  {
    id: "runner",
    name: "Front Runner (Nige / 逃げ)",
    category: "Running Style",
    weights: { Speed: 3, Wisdom: 2, Power: 1, Stamina: 0, Guts: 0 },
    deck: "3 Speed • 2 Wisdom • 1 Friend (or 4 Speed • 2 Wisdom)",
    composition: [
      { label: "3x SPD", color: "#2563EB" },
      { label: "2x WIS", color: "#059669" },
      { label: "1x PAL", color: "#F59E0B" }
    ],
    inheritance: "Borrow 9★ Power factors to assist acceleration when coming out of the final turn.",
    trainingCards: [
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "+15% Training Effect", reason: "+1 Power bonus, top front-runner engine" },
      { name: "Silence Suzuka (SSR)", spec: "SPD", color: "#2563EB", bonus: "+10% Training Effect", reason: "+1 Speed bonus, high specialty priority" },
      { name: "Fine Motion (SSR)", spec: "WIS", color: "#059669", bonus: "+15% Training Effect", reason: "+1 Wisdom bonus, max skill trigger consistency" },
      { name: "Biko Pegasus (SSR)", spec: "SPD", color: "#2563EB", bonus: "+20% Training Effect", reason: "Highest speed multiplier" }
    ],
    inspirationCards: [
      { name: "Mihono Bourbon (SSR)", spec: "WIS", color: "#059669", bonus: "Hint Lv 3 (+40% Rate)", reason: "Groundwork (Jiban) + Runner Corner hints" },
      { name: "Silence Suzuka (SSR)", spec: "SPD", color: "#2563EB", bonus: "Hint Lv 3 (+50% Rate)", reason: "Escape Expert, Rapid Pace, Left Turn" },
      { name: "Twin Turbo (SSR)", spec: "SPD", color: "#2563EB", bonus: "Hint Lv 3 (+40% Rate)", reason: "Early Gate Sprint, Leading Tactics" }
    ]
  },
  {
    id: "leader",
    name: "Pace Setter (Senkou / 先行)",
    category: "Running Style",
    weights: { Speed: 2, Power: 2, Wisdom: 1, Stamina: 0, Guts: 0 },
    deck: "3 Speed • 2 Power • 1 Wisdom (or 3 Speed • 2 Wisdom • 1 Friend)",
    composition: [
      { label: "3x SPD", color: "#2563EB" },
      { label: "2x POW", color: "#D97706" },
      { label: "1x WIS", color: "#059669" }
    ],
    inheritance: "Inherit Stamina factors to easily clear career race requirements without Stamina card slots.",
    trainingCards: [
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "+15% Training Effect", reason: "Universal speed + power multiplier" },
      { name: "Oguri Cap (SR/SSR)", spec: "POW", color: "#D97706", bonus: "+15% Training Effect", reason: "+1 Power bonus, top pace-setter card" },
      { name: "Fine Motion (SSR)", spec: "WIS", color: "#059669", bonus: "+15% Training Effect", reason: "+1 Wisdom bonus, keeps energy high" },
      { name: "Taiki Shuttle (SSR)", spec: "SPD", color: "#2563EB", bonus: "+10% Training Effect", reason: "+1 Speed bonus" }
    ],
    inspirationCards: [
      { name: "Taiki Shuttle (SSR)", spec: "SPD", color: "#2563EB", bonus: "Hint Lv 3 (+40% Rate)", reason: "Final Corner Accel & Leader Strategy at 40% discount" },
      { name: "Oguri Cap (SR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+30% Rate)", reason: "Corner Speed & Nutritionist hints" },
      { name: "Fine Motion (SSR)", spec: "WIS", color: "#059669", bonus: "Hint Lv 2 (+30% Rate)", reason: "Right Turn, Fall Girl, Leader Corner" }
    ]
  },
  {
    id: "betwixt",
    name: "Betwixt / Late (Sashi / 差し)",
    category: "Running Style",
    weights: { Power: 3, Speed: 2, Guts: 1, Stamina: 0, Wisdom: 0 },
    deck: "3 Power • 2 Speed • 1 Friend (or 2 Speed • 2 Power • 1 Guts • 1 Friend)",
    composition: [
      { label: "3x POW", color: "#D97706" },
      { label: "2x SPD", color: "#2563EB" },
      { label: "1x PAL", color: "#F59E0B" }
    ],
    inheritance: "Inherit Stamina factors to withstand hard mid-pack bumping and uphill surges.",
    trainingCards: [
      { name: "Vodka (SSR)", spec: "POW", color: "#D97706", bonus: "+10% Training Effect", reason: "+1 Power bonus, core sashi engine" },
      { name: "Marvelous Sunday (SR)", spec: "WIS", color: "#059669", bonus: "+15% Training Effect", reason: "Huge universal multiplier for secondary training" },
      { name: "Mejiro Dober (SSR)", spec: "POW", color: "#D97706", bonus: "+10% Training Effect", reason: "+1 Power bonus" },
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "+15% Training Effect", reason: "+1 Power bonus" }
    ],
    inspirationCards: [
      { name: "Yaeno Muteki (SSR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+50% Rate)", reason: "Tail Held High, Betwixt Acceleration" },
      { name: "Vodka (SSR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+40% Rate)", reason: "Hao, In-Between Position Expert" },
      { name: "Mejiro Dober (SSR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+40% Rate)", reason: "Late spurt cornering skills" }
    ]
  },
  {
    id: "chaser",
    name: "Chaser / End (Oikomi / 追込)",
    category: "Running Style",
    weights: { Power: 3, Speed: 2, Stamina: 2, Wisdom: 0, Guts: 0 },
    deck: "3 Power • 2 Stamina • 1 Speed (or 2 Speed • 2 Power • 2 Stamina)",
    composition: [
      { label: "3x POW", color: "#D97706" },
      { label: "2x STA", color: "#EA580C" },
      { label: "1x SPD", color: "#2563EB" }
    ],
    inheritance: "Inherit Speed & Power sparks so you can dedicate training to Stamina and Power.",
    trainingCards: [
      { name: "Super Creek (SSR)", spec: "STA", color: "#EA580C", bonus: "+10% Training Effect", reason: "+1 Stamina bonus, vital stamina foundation" },
      { name: "Oguri Cap (SR)", spec: "POW", color: "#D97706", bonus: "+15% Training Effect", reason: "+1 Power bonus, top budget power multiplier" },
      { name: "Vodka (SSR)", spec: "POW", color: "#D97706", bonus: "+10% Training Effect", reason: "+1 Power bonus" },
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "+15% Training Effect", reason: "High rainbow power" }
    ],
    inspirationCards: [
      { name: "Super Creek (SSR)", spec: "STA", color: "#EA580C", bonus: "Hint Lv 3", reason: "Arc Maestro (Crucial for Gold Ship long runs)" },
      { name: "Inari One (SSR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+40% Rate)", reason: "Chaser Surge, Straightaway Blitz at max discount" },
      { name: "Hishi Amazon (SR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+30% Rate)", reason: "Chaser Corner Acceleration & Sharp Eye hints" }
    ]
  },
  {
    id: "guts_meta",
    name: "Guts Training Meta (Root Training)",
    category: "Meta & Strategy",
    weights: { Guts: 3, Speed: 2, Wisdom: 1, Stamina: 0, Power: 0 },
    deck: "3 Guts • 2 Speed • 1 Wisdom (or 4 Guts • 2 Speed)",
    composition: [
      { label: "3x GUT", color: "#DB2777" },
      { label: "2x SPD", color: "#2563EB" },
      { label: "1x WIS", color: "#059669" }
    ],
    inheritance: "Borrow 9★ Power factors to effortlessly cap Power alongside Guts and Speed.",
    trainingCards: [
      { name: "Haru Urara (SSR)", spec: "GUT", color: "#DB2777", bonus: "+15% Training Effect", reason: "Free welfare SSR with enormous stat scaling" },
      { name: "Matikanefukukitaru (SSR)", spec: "SPD", color: "#2563EB", bonus: "+10% Training Effect", reason: "+1 to all stats bonus at year start" },
      { name: "Yukino Bijin (SSR)", spec: "GUT", color: "#DB2777", bonus: "+10% Training Effect", reason: "+1 Guts bonus" },
      { name: "Biko Pegasus (SSR)", spec: "SPD", color: "#2563EB", bonus: "+20% Training Effect", reason: "Boosts Guts facilities when visiting" }
    ],
    inspirationCards: [
      { name: "Yukino Bijin (SSR)", spec: "GUT", color: "#DB2777", bonus: "Hint Lv 3 (+40% Rate)", reason: "Non-Stop Girl & Guts Endurance hints" },
      { name: "Matikanefukukitaru (SSR)", spec: "SPD", color: "#2563EB", bonus: "Hint Lv 3 (+30% Rate)", reason: "Super Lucky Seven & mood boosts" },
      { name: "Fine Motion (SSR)", spec: "WIS", color: "#059669", bonus: "Hint Lv 2 (+30% Rate)", reason: "Fall Runner & Right Turn hints" }
    ]
  },
  {
    id: "factor_farm",
    name: "Inheritance & Factor Farming",
    category: "Meta & Strategy",
    weights: { Speed: 1, Stamina: 1, Power: 1, Guts: 1, Wisdom: 1 },
    deck: "2 Speed • 1 Stamina • 1 Power • 1 Guts • 1 Wisdom",
    composition: [
      { label: "2x SPD", color: "#2563EB" },
      { label: "1x STA", color: "#EA580C" },
      { label: "1x POW", color: "#D97706" },
      { label: "1x GUT", color: "#DB2777" },
      { label: "1x WIS", color: "#059669" }
    ],
    inheritance: "Pair with high compatibility parents (G1 trophy overlap) to boost factor inheritance rates.",
    trainingCards: [
      { name: "Biko Pegasus (SSR)", spec: "SPD", color: "#2563EB", bonus: "+20% Training Effect", reason: "Universal multiplier across all 5 stat facilities" },
      { name: "Marvelous Sunday (SR)", spec: "WIS", color: "#059669", bonus: "+15% Training Effect", reason: "Boosts any training facility visited" },
      { name: "Oguri Cap (SR)", spec: "POW", color: "#D97706", bonus: "+15% Training Effect", reason: "High power & general efficiency" },
      { name: "Super Creek (SSR)", spec: "STA", color: "#EA580C", bonus: "+10% Training Effect", reason: "Effortlessly guarantees 600+ Stamina" }
    ],
    inspirationCards: [
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "Hint Lv 2", reason: "Universal white skills to pass down to offspring" },
      { name: "Fine Motion (SSR)", spec: "WIS", color: "#059669", bonus: "Hint Lv 2", reason: "Right Turn, Fall Girl, Corner Wisdom" },
      { name: "Super Creek (SSR)", spec: "STA", color: "#EA580C", bonus: "Hint Lv 3", reason: "Corner Recovery white skill for factor roll" }
    ]
  },
  {
    id: "wisdom_engine",
    name: "Wisdom & Skill Point Engine",
    category: "Meta & Strategy",
    weights: { Wisdom: 3, Speed: 2, Stamina: 0, Power: 0, Guts: 0 },
    deck: "3 Wisdom • 2 Speed • 1 Friend (or 4 Wisdom • 2 Speed)",
    composition: [
      { label: "3x WIS", color: "#059669" },
      { label: "2x SPD", color: "#2563EB" },
      { label: "1x PAL", color: "#F59E0B" }
    ],
    inheritance: "Borrow 9★ Stamina & Power factors so you never need to leave Speed and Wisdom training.",
    trainingCards: [
      { name: "Fine Motion (SSR)", spec: "WIS", color: "#059669", bonus: "+15% Training Effect", reason: "+1 Wisdom bonus, god-tier rainbow gains" },
      { name: "Nice Nature (SSR)", spec: "WIS", color: "#059669", bonus: "+10% Training Effect", reason: "+1 Wisdom bonus, high skill point generation" },
      { name: "Mr. C.B. (SSR)", spec: "WIS", color: "#059669", bonus: "+10% Training Effect", reason: "+1 Wisdom bonus" },
      { name: "Biko Pegasus (SSR)", spec: "SPD", color: "#2563EB", bonus: "+20% Training Effect", reason: "Maximizes Speed gains on mixed turns" }
    ],
    inspirationCards: [
      { name: "Nice Nature (SSR)", spec: "WIS", color: "#059669", bonus: "Hint Lv 3 (+50% Rate)", reason: "Red Debuffs: Hesitation, Trick, Murmur, Evil Eye" },
      { name: "Mr. C.B. (SSR)", spec: "WIS", color: "#059669", bonus: "Hint Lv 3 (+40% Rate)", reason: "Corner Maestro & position recovery hints" },
      { name: "Fine Motion (SSR)", spec: "WIS", color: "#059669", bonus: "Hint Lv 2 (+30% Rate)", reason: "Right Turn, Fall Girl, Corner Wisdom" }
    ]
  },
  {
    id: "sp",
    name: "Speed + Power Duo",
    category: "Meta & Strategy",
    weights: { Speed: 1, Power: 1, Stamina: 0, Guts: 0, Wisdom: 0 },
    deck: "3 Speed • 3 Power (or 3 Speed • 2 Power • 1 Friend)",
    composition: [
      { label: "3x SPD", color: "#2563EB" },
      { label: "3x POW", color: "#D97706" }
    ],
    inheritance: "Borrow 9★ Stamina factors for medium/mile races.",
    trainingCards: [
      { name: "Biko Pegasus (SSR)", spec: "SPD", color: "#2563EB", bonus: "+20% Training Effect", reason: "Highest speed multiplier" },
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "+15% Training Effect", reason: "+1 Power bonus" },
      { name: "Vodka (SSR)", spec: "POW", color: "#D97706", bonus: "+10% Training Effect", reason: "+1 Power bonus" },
      { name: "Oguri Cap (SR)", spec: "POW", color: "#D97706", bonus: "+15% Training Effect", reason: "+1 Power bonus" }
    ],
    inspirationCards: [
      { name: "Yaeno Muteki (SSR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+50% Rate)", reason: "Tail Held High" },
      { name: "Vodka (SSR)", spec: "POW", color: "#D97706", bonus: "Hint Lv 3 (+40% Rate)", reason: "Hao acceleration hints" }
    ]
  },
  {
    id: "ss",
    name: "Speed + Stamina Duo",
    category: "Meta & Strategy",
    weights: { Speed: 1, Stamina: 1, Power: 0, Guts: 0, Wisdom: 0 },
    deck: "3 Speed • 3 Stamina (or 3 Speed • 2 Stamina • 1 Friend)",
    composition: [
      { label: "3x SPD", color: "#2563EB" },
      { label: "3x STA", color: "#EA580C" }
    ],
    inheritance: "Borrow 9★ Power factors to maintain strong corner acceleration.",
    trainingCards: [
      { name: "Super Creek (SSR)", spec: "STA", color: "#EA580C", bonus: "+10% Training Effect", reason: "+1 Stamina bonus, top recovery engine" },
      { name: "Manhattan Cafe (SSR)", spec: "STA", color: "#EA580C", bonus: "+10% Training Effect", reason: "+1 Stamina bonus" },
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "+15% Training Effect", reason: "Keeps speed near 1200 while stamina grows" }
    ],
    inspirationCards: [
      { name: "Super Creek (SSR)", spec: "STA", color: "#EA580C", bonus: "Hint Lv 3", reason: "Arc Maestro" },
      { name: "Manhattan Cafe (SSR)", spec: "STA", color: "#EA580C", bonus: "Hint Lv 3 (+40% Rate)", reason: "Long distance stamina passives" }
    ]
  },
  {
    id: "sw",
    name: "Speed + Wisdom Duo",
    category: "Meta & Strategy",
    weights: { Speed: 1, Wisdom: 1, Stamina: 0, Power: 0, Guts: 0 },
    deck: "3 Speed • 3 Wisdom (or 3 Speed • 2 Wisdom • 1 Friend)",
    composition: [
      { label: "3x SPD", color: "#2563EB" },
      { label: "3x WIS", color: "#059669" }
    ],
    inheritance: "Borrow 9★ Power factors.",
    trainingCards: [
      { name: "Biko Pegasus (SSR)", spec: "SPD", color: "#2563EB", bonus: "+20% Training Effect", reason: "Max speed training multiplier" },
      { name: "Fine Motion (SSR)", spec: "WIS", color: "#059669", bonus: "+15% Training Effect", reason: "+1 Wisdom bonus" },
      { name: "Kitasan Black (SSR)", spec: "SPD", color: "#2563EB", bonus: "+15% Training Effect", reason: "+1 Power bonus" },
      { name: "Nice Nature (SSR)", spec: "WIS", color: "#059669", bonus: "+10% Training Effect", reason: "+1 Wisdom bonus" }
    ],
    inspirationCards: [
      { name: "Silence Suzuka (SSR)", spec: "SPD", color: "#2563EB", bonus: "Hint Lv 3 (+50% Rate)", reason: "Escape Expert & Left Turn hints" },
      { name: "Mihono Bourbon (SSR)", spec: "WIS", color: "#059669", bonus: "Hint Lv 3 (+40% Rate)", reason: "Groundwork (Jiban) & Runner Corner" },
      { name: "Mr. C.B. (SSR)", spec: "WIS", color: "#059669", bonus: "Hint Lv 3 (+40% Rate)", reason: "Corner Maestro" }
    ]
  }
];

const STAT_METAS = [
  { type: "Speed", name: "Speed", jp: "スピード", icon: "images/speed.png", color: "#3B82F6", secGain: "+Power" },
  { type: "Stamina", name: "Stamina", jp: "スタミナ", icon: "images/stamina.png", color: "#EA580C", secGain: "+Guts" },
  { type: "Power", name: "Power", jp: "パワー", icon: "images/power.png", color: "#D97706", secGain: "+Stamina" },
  { type: "Guts", name: "Guts", jp: "根性", icon: "images/guts.png", color: "#DB2777", secGain: "+Speed & Power" },
  { type: "Wisdom", name: "Wisdom", jp: "賢さ", icon: "images/wisdom.png", color: "#059669", secGain: "+Speed & Energy" }
];

class UmacalcApp {
  constructor() {
    this.selectedScenarioId = "ura";
    this.trainableTurns = 60;
    this.totalTurnsTrained = 0;
    this.activePresetId = "mile";
    this.history = [];

    this.playerInventory = null;
    this.showOnlyOwnedCards = true;
    this.inventoryFileName = "";

    this.stats = {
      Speed: { selected: true, assigned: 24, used: 0, weight: 3 },
      Stamina: { selected: false, assigned: 0, used: 0, weight: 0 },
      Power: { selected: true, assigned: 16, used: 0, weight: 2 },
      Guts: { selected: false, assigned: 0, used: 0, weight: 0 },
      Wisdom: { selected: true, assigned: 20, used: 0, weight: 2 }
    };

    this.init();
  }

  init() {
    this.loadFromStorage();
    this.renderScenarios();
    this.renderPresets();
    this.renderActiveBlueprint();
    this.renderStatCards();
    this.updateBanner();
    this.updateHistory();
    this.attachEventListeners();
  }

  loadFromStorage() {
    try {
      const saved = localStorage.getItem("umacalc_mobile_state");
      if (saved) {
        const data = JSON.parse(saved);
        if (data.scenarioId) this.selectedScenarioId = data.scenarioId;
        if (typeof data.trainableTurns === "number") this.trainableTurns = data.trainableTurns;
        if (typeof data.totalTurnsTrained === "number") this.totalTurnsTrained = data.totalTurnsTrained;
        if (data.activePresetId) this.activePresetId = data.activePresetId;
        if (data.stats) this.stats = data.stats;
        if (Array.isArray(data.history)) this.history = data.history;
      }

      const savedInv = localStorage.getItem("umacalc_player_inventory");
      if (savedInv) {
        this.playerInventory = JSON.parse(savedInv);
        this.inventoryFileName = localStorage.getItem("umacalc_player_inventory_name") || "support_card_data.json";
      }
    } catch (e) {
      console.warn("Could not load from storage:", e);
    }
  }

  saveToStorage() {
    try {
      const state = {
        scenarioId: this.selectedScenarioId,
        trainableTurns: this.trainableTurns,
        totalTurnsTrained: this.totalTurnsTrained,
        activePresetId: this.activePresetId,
        stats: this.stats,
        history: this.history
      };
      localStorage.stringify && localStorage.setItem("umacalc_mobile_state", JSON.stringify(state));
    } catch (e) {
      console.warn("Could not save to storage:", e);
    }
  }

  getScenario() {
    return SCENARIOS.find(s => s.id === this.selectedScenarioId) || SCENARIOS[0];
  }

  getPreset() {
    return PRESETS.find(p => p.id === this.activePresetId) || PRESETS[0];
  }

  getRemainingTurns() {
    return Math.max(0, this.trainableTurns - this.totalTurnsTrained);
  }

  recalculateBudget() {
    const selectedStats = Object.keys(this.stats).filter(k => this.stats[k].selected);
    if (selectedStats.length === 0) {
      Object.keys(this.stats).forEach(k => { this.stats[k].assigned = 0; });
      this.renderStatCards();
      this.updateBanner();
      this.saveToStorage();
      return;
    }

    const totalWeight = selectedStats.reduce((sum, k) => sum + Math.max(1, this.stats[k].weight), 0);
    let remainingToDistribute = this.trainableTurns;

    selectedStats.forEach(k => {
      const w = Math.max(1, this.stats[k].weight);
      const allocated = Math.floor(this.trainableTurns * w / totalWeight);
      this.stats[k].assigned = allocated;
      remainingToDistribute -= allocated;
    });

    // Remainder
    let idx = 0;
    while (remainingToDistribute > 0) {
      const k = selectedStats[idx % selectedStats.length];
      this.stats[k].assigned++;
      remainingToDistribute--;
      idx++;
    }

    // Unselected stats get 0 assigned
    Object.keys(this.stats).forEach(k => {
      if (!this.stats[k].selected) {
        this.stats[k].assigned = 0;
      }
    });

    this.renderStatCards();
    this.updateBanner();
    this.saveToStorage();
  }

  applyPreset(presetId) {
    const preset = PRESETS.find(p => p.id === presetId);
    if (!preset) return;

    this.activePresetId = presetId;
    Object.keys(this.stats).forEach(k => {
      const w = preset.weights[k] || 0;
      if (w > 0) {
        this.stats[k].selected = true;
        this.stats[k].weight = w;
      } else {
        this.stats[k].selected = false;
        this.stats[k].weight = 0;
      }
    });

    this.recalculateBudget();
    this.renderPresets();
    this.renderActiveBlueprint();
    this.triggerHaptic(20);
  }

  trainStat(statName) {
    this.triggerHaptic(25);

    const s = this.stats[statName];
    if (!s) return;

    // Auto-expand budget if at or exceeding limit
    if (this.totalTurnsTrained >= this.trainableTurns) {
      this.trainableTurns++;
    }
    if (s.used >= s.assigned) {
      s.assigned++;
    }

    s.used++;
    this.totalTurnsTrained++;

    const remStatTurns = Math.max(0, s.assigned - s.used);
    this.history.unshift({
      stat: statName,
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      text: `Trained ${statName} (${remStatTurns} turns left)`
    });

    this.renderStatCards();
    this.updateBanner();
    this.updateHistory();
    this.saveToStorage();
  }

  adjustAssigned(statName, delta) {
    const s = this.stats[statName];
    if (!s) return;
    if (s.assigned + delta >= 0) {
      s.assigned += delta;
      this.trainableTurns = Math.max(this.totalTurnsTrained, this.trainableTurns + delta);
      this.renderStatCards();
      this.updateBanner();
      this.saveToStorage();
    }
  }

  undo() {
    if (this.history.length === 0) return;
    const last = this.history.shift();
    if (last && last.stat && this.stats[last.stat] && this.stats[last.stat].used > 0) {
      this.stats[last.stat].used--;
      if (this.totalTurnsTrained > 0) this.totalTurnsTrained--;
    }
    this.renderStatCards();
    this.updateBanner();
    this.updateHistory();
    this.saveToStorage();
    this.triggerHaptic(15);
  }

  reset() {
    if (!confirm("Reset all training progress to turn 0?")) return;
    this.totalTurnsTrained = 0;
    this.history = [];
    Object.keys(this.stats).forEach(k => {
      this.stats[k].used = 0;
    });
    const scen = this.getScenario();
    this.trainableTurns = scen.turns;
    this.recalculateBudget();
    this.renderStatCards();
    this.updateBanner();
    this.updateHistory();
    this.saveToStorage();
    this.triggerHaptic(30);
  }

  triggerHaptic(ms = 20) {
    if (window.AndroidBridge && typeof window.AndroidBridge.triggerHapticFeedback === "function") {
      try { window.AndroidBridge.triggerHapticFeedback(ms); return; } catch (_) {}
    }
    if (navigator.vibrate) {
      try { navigator.vibrate(ms); } catch (_) {}
    }
  }

  openFloatingHud() {
    const hudPayload = {
      totalRemaining: this.getRemainingTurns(),
      stats: {
        Speed: Math.max(0, this.stats.Speed.assigned - this.stats.Speed.used),
        Stamina: Math.max(0, this.stats.Stamina.assigned - this.stats.Stamina.used),
        Power: Math.max(0, this.stats.Power.assigned - this.stats.Power.used),
        Guts: Math.max(0, this.stats.Guts.assigned - this.stats.Guts.used),
        Wisdom: Math.max(0, this.stats.Wisdom.assigned - this.stats.Wisdom.used)
      }
    };

    if (window.AndroidBridge && typeof window.AndroidBridge.startFloatingHud === "function") {
      window.AndroidBridge.startFloatingHud(JSON.stringify(hudPayload));
    } else {
      // Show web in-app floating mini-HUD for browser testing
      this.showWebFloatingHud(hudPayload);
    }
  }

  showWebFloatingHud(payload) {
    const hud = document.getElementById("webFloatingHud");
    if (!hud) return;
    hud.classList.add("visible");
    this.updateWebFloatingHud(payload);
  }

  updateWebFloatingHud(payload) {
    const turnsBadge = document.getElementById("hudTurnsBadge");
    if (turnsBadge) turnsBadge.textContent = `${payload.totalRemaining} Left`;

    ["Speed", "Stamina", "Power", "Guts", "Wisdom"].forEach(k => {
      const btn = document.getElementById(`webHudBtn_${k}`);
      if (btn) {
        const short = k.substring(0, 3).toUpperCase();
        btn.innerHTML = `${short}<br>${payload.stats[k]}`;
      }
    });
  }

  updateBanner() {
    const rem = this.getRemainingTurns();
    document.getElementById("trainableRemainingTxt").textContent = rem;
    document.getElementById("turnsProgressTxt").innerHTML = `<strong>${this.totalTurnsTrained} / ${this.trainableTurns}</strong> trained (${rem} left)`;
    document.getElementById("trainableBudgetTxt").textContent = this.trainableTurns;

    const pct = this.trainableTurns > 0 ? Math.min(100, Math.round((this.totalTurnsTrained / this.trainableTurns) * 100)) : 0;
    document.getElementById("bannerProgressBar").style.width = `${pct}%`;

    // Suggestion
    const selected = Object.keys(this.stats).filter(k => this.stats[k].selected);
    let suggestionText = "All goals reached! Balance extra turns freely.";
    if (selected.length > 0) {
      const focus = selected.reduce((prev, curr) => {
        const prevRem = this.stats[prev].assigned - this.stats[prev].used;
        const currRem = this.stats[curr].assigned - this.stats[curr].used;
        return (currRem > prevRem) ? curr : prev;
      });
      const remStat = Math.max(0, this.stats[focus].assigned - this.stats[focus].used);
      if (remStat > 0) {
        suggestionText = `Next Suggestion: Focus <strong>${focus}</strong> (${remStat} turns left)`;
      }
    }
    document.getElementById("suggestionBox").innerHTML = suggestionText;

    // Update in-app floating HUD if visible
    if (document.getElementById("webFloatingHud")?.classList.contains("visible")) {
      this.updateWebFloatingHud({
        totalRemaining: rem,
        stats: {
          Speed: Math.max(0, this.stats.Speed.assigned - this.stats.Speed.used),
          Stamina: Math.max(0, this.stats.Stamina.assigned - this.stats.Stamina.used),
          Power: Math.max(0, this.stats.Power.assigned - this.stats.Power.used),
          Guts: Math.max(0, this.stats.Guts.assigned - this.stats.Guts.used),
          Wisdom: Math.max(0, this.stats.Wisdom.assigned - this.stats.Wisdom.used)
        }
      });
    }
  }

  renderScenarios() {
    const sel = document.getElementById("scenarioSelect");
    sel.innerHTML = "";
    SCENARIOS.forEach(scen => {
      const opt = document.createElement("option");
      opt.value = scen.id;
      opt.textContent = `${scen.name} (${scen.jpName})`;
      if (scen.id === this.selectedScenarioId) opt.selected = true;
      sel.appendChild(opt);
    });

    const current = this.getScenario();
    document.getElementById("scenGuideName").textContent = current.name;
    document.getElementById("scenGuideJp").textContent = current.jpName;
    document.getElementById("scenGuideCap").textContent = current.statCap;
    document.getElementById("scenGuideTips").textContent = current.tips;
  }

  renderPresets() {
    const container = document.getElementById("presetsChipsContainer");
    if (!container) return;

    let html = `
      <div class="presets-group-title dist">Race Distance Presets:</div>
      <div class="preset-chip-list">
    `;

    PRESETS.filter(p => p.category === "Race Distance").forEach(p => {
      const activeClass = p.id === this.activePresetId ? "active" : "";
      html += `
        <button class="preset-chip ${activeClass}" onclick="window.UmacalcApp.applyPreset('${p.id}')">
          <span>${p.name}</span>
          <span class="tag">${p.deck.split('•')[0].trim()}</span>
        </button>
      `;
    });

    html += `</div><div class="presets-group-title style">Running Style Presets:</div><div class="preset-chip-list">`;

    PRESETS.filter(p => p.category === "Running Style").forEach(p => {
      const activeClass = p.id === this.activePresetId ? "active" : "";
      html += `
        <button class="preset-chip ${activeClass}" onclick="window.UmacalcApp.applyPreset('${p.id}')">
          <span>${p.name}</span>
          <span class="tag">${p.deck.split('•')[0].trim()}</span>
        </button>
      `;
    });

    html += `</div><div class="presets-group-title meta">Meta & Specialty Presets:</div><div class="preset-chip-list">`;

    PRESETS.filter(p => p.category === "Meta & Strategy").forEach(p => {
      const activeClass = p.id === this.activePresetId ? "active" : "";
      html += `
        <button class="preset-chip ${activeClass}" onclick="window.UmacalcApp.applyPreset('${p.id}')">
          <span>${p.name}</span>
          <span class="tag">${p.deck.split('•')[0].trim()}</span>
        </button>
      `;
    });

    html += `</div>`;
    container.innerHTML = html;
  }

  openUmadumpFilePicker() {
    const input = document.getElementById("umadumpFileInput");
    if (input) input.click();
  }

  handleUmadumpFileSelect(input) {
    const file = input.files && input.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const data = JSON.parse(e.target.result);
        if (Array.isArray(data)) {
          const map = {};
          data.forEach(item => {
            const id = item.support_card_id || item.card_id;
            if (!id) return;
            const lb = item.limit_break_count != null ? item.limit_break_count : 0;
            const lvl = item.extra_data && item.extra_data.level ? item.extra_data.level : (lb === 4 ? 50 : 30 + lb * 5);
            if (!map[id] || lb > map[id].lb || (lb === map[id].lb && lvl > map[id].lvl)) {
              map[id] = { id, lb, lvl };
            }
          });
          this.playerInventory = map;
          this.inventoryFileName = file.name;
          this.showOnlyOwnedCards = true;
          try {
            localStorage.setItem("umacalc_player_inventory", JSON.stringify(map));
            localStorage.setItem("umacalc_player_inventory_name", file.name);
          } catch (_) {}
          this.renderActiveBlueprint();
        } else {
          alert("Invalid JSON format: Expected an array of support cards from umadump.");
        }
      } catch (err) {
        alert("Failed to parse JSON: " + err.message);
      }
    };
    reader.readAsText(file);
    input.value = "";
  }

  clearPlayerInventory() {
    this.playerInventory = null;
    this.inventoryFileName = "";
    try {
      localStorage.removeItem("umacalc_player_inventory");
      localStorage.removeItem("umacalc_player_inventory_name");
    } catch (_) {}
    this.renderActiveBlueprint();
  }

  toggleOwnedOnly() {
    this.showOnlyOwnedCards = !this.showOnlyOwnedCards;
    this.renderActiveBlueprint();
  }

  evaluateBestCards(preset) {
    if (!this.playerInventory || !this.showOnlyOwnedCards || !window.SUPPORT_CARD_CATALOG) {
      return {
        training: preset.trainingCards,
        inspiration: preset.inspirationCards,
        isCustom: false
      };
    }

    const relevantSpecs = new Set();
    preset.composition.forEach(c => {
      const parts = c.label.split(" ");
      if (parts.length > 1) relevantSpecs.add(parts[1].toUpperCase());
    });
    // Add weights
    Object.keys(preset.weights).forEach(k => {
      if (preset.weights[k] > 0) {
        const spec = k === "Speed" ? "SPD" : k === "Stamina" ? "STA" : k === "Power" ? "POW" : k === "Guts" ? "GUT" : "WIS";
        relevantSpecs.add(spec);
      }
    });
    relevantSpecs.add("PAL");

    const catalog = window.SUPPORT_CARD_CATALOG;
    const ownedMap = this.playerInventory;

    // Evaluate Training Bonus
    const trainingCandidates = [];
    // Evaluate Inspiration Bonus
    const inspirationCandidates = [];

    Object.keys(ownedMap).forEach(idStr => {
      const id = parseInt(idStr, 10);
      const owned = ownedMap[id];
      const card = catalog.find(c => c.id === id);
      if (!card) return;
      if (!relevantSpecs.has(card.speciality.toUpperCase())) return;

      const lb = Math.min(4, Math.max(0, owned.lb));
      const tb = (card.trainingBonus && card.trainingBonus[lb]) || 0;
      const hintLv = (card.hintLevel && card.hintLevel[lb]) || 1;
      const hintRate = (card.hintRate && card.hintRate[lb]) || 0;
      const specRate = (card.specialtyRate && card.specialtyRate[lb]) || 0;

      const rarityBonus = card.rarity === "SSR" ? 120 : card.rarity === "SR" ? 60 : 10;
      const ownershipBadge = lb === 4 ? `MLB Lv ${owned.lvl}` : `${lb}LB Lv ${owned.lvl}`;

      if (tb > 0) {
        const score = (tb * 1500) + (specRate * 12) + rarityBonus + (lb * 25) + owned.lvl;
        trainingCandidates.push({
          card,
          owned,
          tb,
          specRate,
          score,
          ownershipBadge
        });
      }

      if (hintLv >= 2 || hintRate >= 20) {
        const inspScore = (hintLv * 250) + (hintRate * 8) + (card.rarity === "SSR" ? 50 : 25) + (lb * 15);
        inspirationCandidates.push({
          card,
          owned,
          hintLv,
          hintRate,
          inspScore,
          ownershipBadge
        });
      }
    });

    // Select top 4 training cards
    trainingCandidates.sort((a, b) => b.score - a.score);
    const chosenTraining = [];
    const usedCharaIds = new Set();

    trainingCandidates.forEach(c => {
      if (chosenTraining.length >= 4) return;
      if (usedCharaIds.has(c.card.charaId)) return;
      usedCharaIds.add(c.card.charaId);

      chosenTraining.push({
        name: `${c.card.charaName} (${c.card.rarity})`,
        spec: c.card.speciality,
        color: c.card.colorHex,
        bonus: `+${c.tb}% Training Effect`,
        ownershipBadge: c.ownershipBadge,
        reason: c.card.cardTitle ? `${c.card.cardTitle} • Specialty Rate +${c.specRate}` : `Training multiplier from your collection`
      });
    });

    // Select top 3 inspiration cards
    inspirationCandidates.sort((a, b) => b.inspScore - a.inspScore);
    const chosenInspiration = [];
    const usedInspCharaIds = new Set();

    inspirationCandidates.forEach(c => {
      if (chosenInspiration.length >= 3) return;
      if (usedInspCharaIds.has(c.card.charaId)) return;
      usedInspCharaIds.add(c.card.charaId);

      const rateStr = c.hintRate > 0 ? ` (+${c.hintRate}% Rate)` : "";
      chosenInspiration.push({
        name: `${c.card.charaName} (${c.card.rarity})`,
        spec: c.card.speciality,
        color: c.card.colorHex,
        bonus: `Hint Lv ${c.hintLv}${rateStr}`,
        ownershipBadge: c.ownershipBadge,
        reason: c.card.cardTitle ? `${c.card.cardTitle} • -${c.hintLv * 10}% SP hint discount` : `Hint battery from your collection`
      });
    });

    return {
      training: chosenTraining.length > 0 ? chosenTraining : preset.trainingCards,
      inspiration: chosenInspiration.length > 0 ? chosenInspiration : preset.inspirationCards,
      isCustom: true
    };
  }

  renderActiveBlueprint() {
    const preset = this.getPreset();
    const container = document.getElementById("activeBlueprintContainer");
    if (!container) return;

    const evaluated = this.evaluateBestCards(preset);

    let slotsHtml = "";
    preset.composition.forEach(c => {
      slotsHtml += `<span class="deck-slot-pill" style="background-color: ${c.color}">${c.label}</span>`;
    });

    let trainingCardsHtml = "";
    evaluated.training.forEach(c => {
      const badgeHtml = c.ownershipBadge ? `<span class="card-ownership-badge">${c.ownershipBadge}</span>` : "";
      trainingCardsHtml += `
        <div class="tier-card-item">
          <div class="tier-card-top">
            <div>
              <span class="card-spec-badge" style="background-color: ${c.color}">${c.spec}</span>
              <span class="card-name">${c.name}</span>
              ${badgeHtml}
            </div>
            <span class="bonus-stat-pill training">${c.bonus}</span>
          </div>
          <div class="card-reason">${c.reason}</div>
        </div>
      `;
    });

    let inspirationCardsHtml = "";
    evaluated.inspiration.forEach(c => {
      const badgeHtml = c.ownershipBadge ? `<span class="card-ownership-badge sub">${c.ownershipBadge}</span>` : "";
      inspirationCardsHtml += `
        <div class="tier-card-item">
          <div class="tier-card-top">
            <div>
              <span class="card-spec-badge" style="background-color: ${c.color}">${c.spec}</span>
              <span class="card-name">${c.name}</span>
              ${badgeHtml}
            </div>
            <span class="bonus-stat-pill inspiration">${c.bonus}</span>
          </div>
          <div class="card-reason">${c.reason}</div>
        </div>
      `;
    });

    // Umadump status
    const isLoaded = !!this.playerInventory;
    const count = isLoaded ? Object.keys(this.playerInventory).length : 0;
    const filterBtnText = this.showOnlyOwnedCards ? "✓ Showing My Cards" : "Show All Meta";
    const statusTxt = isLoaded
      ? `${this.inventoryFileName || "support_card_data.json"}: ${count} Cards Active`
      : "Load support_card_data.json from umadump to show cards you own";

    const syncBarHtml = `
      <div class="umadump-sync-bar">
        <div class="umadump-sync-left">
          <span class="badge" style="background: #064e3b; color: #34d399; font-weight: 800;">UMADUMP</span>
          <span>${statusTxt}</span>
        </div>
        <div class="umadump-sync-right">
          ${isLoaded ? `
            <button class="btn btn-sm" style="background: #1e293b; border-color: #38bdf8; color: #38bdf8; font-size: 0.68rem;" onclick="window.UmacalcApp.toggleOwnedOnly()">${filterBtnText}</button>
          ` : ""}
          <button class="btn btn-sm btn-primary" style="font-size: 0.68rem;" onclick="window.UmacalcApp.openUmadumpFilePicker()">📂 Select support_card_data.json</button>
          ${isLoaded ? `
            <button class="btn btn-sm btn-danger" style="font-size: 0.68rem;" onclick="window.UmacalcApp.clearPlayerInventory()">✕ Clear</button>
          ` : ""}
        </div>
      </div>
    `;

    container.innerHTML = `
      <div class="deck-blueprint">
        <div class="blueprint-header">
          <div class="blueprint-title">
            <span class="badge">RECOMMENDED SUPPORT DECK</span>
            <h3>Active: ${preset.name}</h3>
          </div>
          <div class="deck-slots">${slotsHtml}</div>
        </div>

        <div class="blueprint-slots-text" style="margin-bottom: 8px;">
          <strong>Slot Allocation:</strong> ${preset.deck}
        </div>

        ${syncBarHtml}

        <!-- 2-Column Best Cards -->
        <div class="cards-comparison-grid">
          <!-- Column 0: Training Bonus -->
          <div class="card-tier-column training">
            <div class="column-header training">
              <span class="badge training">TRAINING BONUS</span>
              <h4>Top Multipliers (+10-20%)</h4>
            </div>
            <div class="column-sub">Universal stat boosts across all facilities when training with you:</div>
            ${trainingCardsHtml}
          </div>

          <!-- Column 1: Inspiration Bonus -->
          <div class="card-tier-column inspiration">
            <div class="column-header inspiration">
              <span class="badge inspiration">INSPIRATION BONUS</span>
              <h4>Top Hint Rate &amp; Discounts</h4>
            </div>
            <div class="column-sub">Triggers '!' events with Hint Lv 3-4 (-40-50% SP cost) and key skills:</div>
            ${inspirationCardsHtml}
          </div>
        </div>

        <!-- Row 3: Inheritance -->
        <div class="inheritance-advice-box">
          <strong>Parent Sparks &amp; Inheritance Advice:</strong> ${preset.inheritance}
        </div>
      </div>
    `;
  }

  renderStatCards() {
    const container = document.getElementById("statCardsGrid");
    if (!container) return;

    let html = "";
    STAT_METAS.forEach(meta => {
      const s = this.stats[meta.type];
      const remaining = s.assigned - s.used;
      const isTargeted = s.selected;

      let btnLabel = "TURNS LEFT";
      let btnTurns = remaining > 0 ? remaining : 0;
      let btnExtra = "";

      if (!isTargeted) {
        btnLabel = "NOT TARGET";
        btnTurns = "—";
      } else if (remaining <= 0) {
        btnLabel = "GOAL MET!";
        btnTurns = `+${s.used - s.assigned} Bonus`;
      }

      const pct = s.assigned > 0 ? Math.min(100, Math.round((s.used / s.assigned) * 100)) : 0;

      html += `
        <div class="stat-card card-${meta.type}">
          <label class="stat-target-toggle">
            <input type="checkbox" ${isTargeted ? "checked" : ""} onchange="window.UmacalcApp.toggleTarget('${meta.type}', this.checked)">
            <span>Target</span>
          </label>

          <img src="${meta.icon}" class="stat-icon" alt="${meta.name}">
          <div class="stat-name">${meta.name}</div>

          <button class="stat-train-btn btn-${meta.type}" onclick="window.UmacalcApp.trainStat('${meta.type}')">
            <span class="btn-label">${btnLabel}</span>
            <span class="btn-turns">${btnTurns}</span>
          </button>

          <div class="stat-micro-progress">
            <div class="fill fill-${meta.type}" style="width: ${pct}%"></div>
          </div>

          <div class="stat-assigned-ctrl">
            <button onclick="window.UmacalcApp.adjustAssigned('${meta.type}', -1)">-</button>
            <span>${s.assigned}</span>
            <button onclick="window.UmacalcApp.adjustAssigned('${meta.type}', 1)">+</button>
          </div>
        </div>
      `;
    });

    container.innerHTML = html;
  }

  toggleTarget(statName, checked) {
    if (this.stats[statName]) {
      this.stats[statName].selected = checked;
      if (!checked) {
        this.stats[statName].weight = 0;
      } else if (this.stats[statName].weight === 0) {
        this.stats[statName].weight = 1;
      }
      this.recalculateBudget();
    }
  }

  updateHistory() {
    const container = document.getElementById("historyList");
    if (!container) return;
    if (this.history.length === 0) {
      container.innerHTML = `<div style="padding: 10px; color: #94a3b8; text-align: center; font-size: 0.72rem;">No actions recorded yet. Tap any stat button to train!</div>`;
      return;
    }
    let html = "";
    this.history.forEach(item => {
      html += `
        <div class="history-item">
          <span style="font-weight: bold; color: #f8fafc;">${item.text}</span>
          <span style="color: #94a3b8; font-size: 0.65rem;">${item.time}</span>
        </div>
      `;
    });
    container.innerHTML = html;
  }

  attachEventListeners() {
    // Scenario change
    document.getElementById("scenarioSelect").addEventListener("change", (e) => {
      this.selectedScenarioId = e.target.value;
      const scen = this.getScenario();
      this.trainableTurns = scen.turns;
      this.recalculateBudget();
      this.renderScenarios();
      this.saveToStorage();
    });

    // Budget adjust
    document.getElementById("btnBudgetMinus").addEventListener("click", () => {
      if (this.trainableTurns > this.totalTurnsTrained) {
        this.trainableTurns--;
        this.recalculateBudget();
      }
    });
    document.getElementById("btnBudgetPlus").addEventListener("click", () => {
      this.trainableTurns++;
      this.recalculateBudget();
    });
    document.getElementById("btnQuickAdd5").addEventListener("click", () => {
      this.trainableTurns += 5;
      this.recalculateBudget();
      this.triggerHaptic(20);
    });

    // Top action buttons
    document.getElementById("btnFloatingHud").addEventListener("click", () => this.openFloatingHud());
    document.getElementById("btnReset").addEventListener("click", () => this.reset());

    // Action bar buttons
    document.getElementById("btnLogRest").addEventListener("click", () => {
      this.trainableTurns++;
      this.history.unshift({
        stat: "Rest",
        time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        text: "Logged Rest / Infirmary (+1 Turn Added)"
      });
      this.updateBanner();
      this.updateHistory();
      this.saveToStorage();
      this.triggerHaptic(15);
    });

    document.getElementById("btnLogRace").addEventListener("click", () => {
      this.history.unshift({
        stat: "Race",
        time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        text: "Logged Race (+1 Turn Logged)"
      });
      this.updateHistory();
      this.saveToStorage();
      this.triggerHaptic(15);
    });

    document.getElementById("btnRecalculate").addEventListener("click", () => this.recalculateBudget());
    document.getElementById("btnUndo").addEventListener("click", () => this.undo());

    // Tab navigation
    document.querySelectorAll(".tab-btn").forEach(btn => {
      btn.addEventListener("click", (e) => {
        document.querySelectorAll(".tab-btn").forEach(b => b.classList.remove("active"));
        document.querySelectorAll(".tab-pane").forEach(p => p.classList.remove("active"));
        const tabId = e.target.getAttribute("data-tab");
        e.target.classList.add("active");
        document.getElementById(tabId)?.classList.add("active");
      });
    });

    // Web Floating HUD close
    document.getElementById("btnWebHudClose")?.addEventListener("click", () => {
      document.getElementById("webFloatingHud")?.classList.remove("visible");
    });
  }
}

// Instantiate on DOM ready
document.addEventListener("DOMContentLoaded", () => {
  window.UmacalcApp = new UmacalcApp();
});

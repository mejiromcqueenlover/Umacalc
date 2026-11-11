# Umacalc - Android Port (Umamusume Stat Gain & Turn Planner)

A production-grade, instantly-releasable Android port of **Umacalc**, the companion stat gain and trainable turn calculator for *Umamusume: Pretty Derby*.

---

## 🌟 Key Features

1. **Full Android Companion Dashboard**
   - **All Global Scenarios Supported**: URA Finals (60t), Aoharu Cup (56t), MANT / Climax (50t), Our Grand Live (64t), Grand Masters (63t), Project L'Arc (60t), U.A.F. Ready GO! (62t), plus Custom mode.
   - **Interactive Stat Clicker**: Speed, Stamina, Power, Guts, Wisdom with remaining turn counters, auto-expanding budgets (never stuck at 0 turns in MANT), and progress bars.
   - **15 Build Presets**:
     - *Distances*: Sprint, Mile Meta, Medium, Long Distance, Dirt Track Powerhouse.
     - *Running Styles*: Front Runner (Nige), Pace Setter (Senkou), Betwixt (Sashi), Chaser (Oikomi).
     - *Meta & Specials*: Guts Root Meta, Factor Farm (all B-rank), Wisdom & SP Engine, Speed/Power, Speed/Stamina, Speed/Wisdom.
   - **Support Card Blueprints**:
     - **Top Training Bonus Cards**: Cards with +10% to +20% universal Training Effectiveness multipliers (Biko Pegasus, Kitasan Black, Fine Motion, Super Creek, Oguri Cap).
     - **Top Inspiration Bonus Cards**: High Hint Rate Up (+30% to +50%) and Hint Lv 3-4 cards (Yaeno Muteki, Vodka, Taiki Shuttle, Mihono Bourbon) for 40-50% SP discounts on key gold/white skills.
     - **Parent Sparks Guidance**: Blue 9★ parent factor recommendations and white G1 compatibility advice.
   - **Silent Haptic Feedback**: Uses Android vibrator for subtle tactile clicks without system beeps.
   - **Offline Persistence**: State automatically persists via `SharedPreferences` and `LocalStorage`.

2. **Native Android Floating Overlay HUD (`FloatingHudService`)**
   - Floats directly on top of the running *Umamusume: Pretty Derby* mobile game!
   - Compact drag-to-move widget on screen with:
     - Real-time remaining trainable turns badge
     - 5 mini stat buttons (`SPD`, `STA`, `POW`, `GUT`, `WIS`) showing remaining assigned turns
     - One-tap counting with haptic feedback
     - Minimize/Expand toggle and 1-tap return to the full app

---

## 🚀 How to Build & Release

### Method 1: Automated GitHub Actions (Instant Cloud Release)
1. Push this repository to GitHub.
2. The included workflow `.github/workflows/android-release.yml` triggers automatically on push or tag (e.g. `git tag v1.0.0 && git push --tags`).
3. Download the signed **Release APK** and **Google Play AAB** directly from GitHub Actions Artifacts or Releases!

### Method 2: Local Build with Android Studio / Gradle
1. Open the `Umacalc-Android` directory in **Android Studio** (or install JDK 17+ and Android SDK).
2. To build a debug APK:
   ```bash
   ./gradlew assembleDebug
   ```
   Output: `app/build/outputs/apk/debug/app-debug.apk`
3. To build a release APK & Google Play Bundle (AAB):
   ```bash
   ./gradlew assembleRelease bundleRelease
   ```
   Outputs:
   - APK: `app/build/outputs/apk/release/app-release-unsigned.apk`
   - AAB: `app/build/outputs/bundle/release/app-release.aab`

---

## 📱 Instant Mobile Preview (Run on Phone without compiling)

If you want to use or preview the application immediately on your phone right now without waiting for Android Studio:

1. Open PowerShell and run:
   ```powershell
   cd Umacalc-Mobile
   powershell -ExecutionPolicy Bypass -File .\start_mobile_preview.ps1
   ```
2. Open the displayed URL (e.g. `http://192.168.x.x:5000`) on your Android phone's Chrome or Firefox browser.
3. Tap the browser menu `⋮` -> **"Add to Home screen" / "Install app"**.
4. Umacalc will install as a standalone, 100% offline mobile app with app icon on your home screen!

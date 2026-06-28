# 🎯 3-Strike Logic — Full System Analysis & Implementation Plan

> Analysis of the three-strike mechanic across **StoneGenerator Scene** and
> **StoneCuttingScene_Classic** — covering every prefab, UI element, script, and current bugs.
> Includes a complete implementation plan to make the system fully functional.

---

## 📦 Asset Locations

| Asset | Path |
|---|---|
| `3-StrikeSelectionPanel-Classic.prefab` | `Assets/UnityTechnologies/ParticlePack/EffectExamples/Legacy Particles/Prefabs/Strike-Prefab/` |
| `StrikeText.prefab` | same folder |
| `Visibility (1).prefab` | same folder |
| `StrikeSystem.cs` | `Assets/Scripts/StrikeSystem.cs` |
| `StoneGenerator.cs` | `Assets/Scripts/Stone/StoneGenerator.cs` |
| `JadeCuttingGame.cs` | `Assets/Scripts/JadeCuttingGame.cs` |
| `ClassicChiselController.cs` | `Assets/Scripts/Chisel/ClassicChiselController.cs` |
| `WinLoseManager.cs` | `Assets/Scripts/MVC/Controllers/WinLoseManager.cs` |

---

## 🖼️ Prefab Analysis

### 1. `3-StrikeSelectionPanel-Classic.prefab`

**Purpose:** Visual three-slot strike indicator panel.

**Hierarchy:**

```
3-StrikeSelectionPanel-Classic          <- Root panel
├── First-StrikeBg        (Image, Left)
│   └── FirstStrikeComplete (Image overlay — should fill on strike 1)
├── Mid-StrikeBg          (Image, Center)
│   └── SecondStrikeComplete (Image overlay — should fill on strike 2)
└── Right-StrikeBg        (Image, Right)
    └── ThirdStrikeComplete  (Image overlay — should fill on strike 3)
```

**Key Layout Values (from prefab):**

| Property | Value |
|---|---|
| Root `LocalScale` | `(0.05572708, 0.05572708, 0.05572708)` |
| Root `AnchoredPosition` (default) | `(31, -228)` |
| Root `SizeDelta` | `(4058.29, 1170)` |
| Root `Image.Color` | `RGBA(1,1,1, 0.392)` — semi-transparent bg |
| Each BG Slot `Scale` | `(8.6544, 8.6544, 8.6544)` |
| Each BG Slot `SizeDelta` | `(100, 100)` |
| BG Sprite GUID | `db85d6ef...` (empty circle) |
| Complete Overlay Sprite GUID | `6cad0eb3...` (filled circle) |
| Complete Overlay `Image.Color` | `RGBA(1,1,1,1)` — always visible |

**Scene-specific position overrides:**

| Scene | AnchoredPosition |
|---|---|
| `StoneGenerator Scene` | `(101, -232)` |
| `StoneCuttingScene_Classic` | `(31, -228)` |

---

### 2. `StrikeText.prefab`

| Property | Value |
|---|---|
| Default text | `"STRIKS: 0 / 3"` |
| FontSize | `36`, Bold |
| FontColor | Light grey `RGBA(0.643, 0.647, 0.651, 1)` |
| AnchoredPos (scene override) | `(200, -81)` |
| LocalScale (scene override) | `1.4274738` uniform |

---

### 3. `Visibility (1).prefab`

Top Bar companion panel (torch inspection metrics — separate from strike logic).

| Child | Default Text |
|---|---|
| `VisibilityText` | `"VISIVILITY: 0%"` *(typo in prefab)* |
| `EstimatedValueText` | `"EST VALUE: ???"` |

---

## 🔴 FUNCTIONALITY AUDIT — What Is Actually Working?

### `3-StrikeSelectionPanel-Classic` — ❌ NOT FUNCTIONAL

| Check | Result |
|---|---|
| Panel exists in both scenes | ✅ Yes |
| Panel is visible on screen | ✅ Yes |
| `FirstStrikeComplete` fills on strike 1 | ❌ NO — no code drives it |
| `SecondStrikeComplete` fills on strike 2 | ❌ NO — no code drives it |
| `ThirdStrikeComplete` fills on strike 3 | ❌ NO — no code drives it |
| Connected to `StoneGenerator` | ❌ NO — zero references in code |

> **Root Cause:** The three `XxxStrikeComplete` Image overlays are always fully rendered
> at `RGBA(1,1,1,1)`. There is no script that reads `currentStrikes` and toggles or
> colors them. The panel is purely cosmetic decoration right now.

---

### `StrikeText` — ❌ NOT FUNCTIONAL in StoneGenerator Scene / ✅ FUNCTIONAL in Classic

| Scene | `strikeText` assigned? | Updates? |
|---|---|---|
| `StoneCuttingScene_Classic` | ✅ `fileID: 53307162` | ✅ Yes — `"Strikes: X / 100"` |
| `StoneGenerator Scene` | ❌ `fileID: 0` (null) | ❌ Never updates |

---

### `maxStrikes` — ❌ BROKEN in BOTH scenes

| Scene | Value set | Expected | Result |
|---|---|---|---|
| `StoneCuttingScene_Classic` | **100** | 3 | Never triggers lose on 3 strikes |
| `StoneGenerator Scene` | **100** | 3 | Never triggers lose on 3 strikes |

---

### Win/Lose Logic — ⚠️ PARTIALLY FUNCTIONAL

| Condition | Code exists? | Works? |
|---|---|---|
| All anchors destroyed → `readyForFinalHit = true` | ✅ | ✅ Yes |
| `readyForFinalHit` + strike → `RevealJadeRoutine()` → WIN | ✅ | ✅ Yes (if player hits after anchors gone) |
| `currentStrikes >= maxStrikes` → LOSE | ✅ | ❌ Never fires (maxStrikes = 100) |
| Strike fills panel slot visually | ❌ Missing | ❌ Not implemented |

---

## 🧠 Current Code Flow (Actual)

```
Tool hits stone
      │
      ▼
StoneGenerator.RegisterToolStrike()
      │
      ├── readyForFinalHit == true?
      │        YES → RevealJadeRoutine() → WIN panel  ✅
      │
      └── readyForFinalHit == false
               currentStrikes++  (goes up to 100 before anything happens)
               UpdateStrikeUI()  → strikeText only (panel icons ignored)
               currentStrikes >= 100? → LOSE panel  ❌ (unreachable in practice)
```

---

## 🎯 DESIRED Behavior (Your Requirements)

```
Strike 1 → FirstStrikeComplete fills (icon becomes active)
Strike 2 → SecondStrikeComplete fills
Strike 3 → ThirdStrikeComplete fills
               │
               ├── readyForFinalHit == true (all anchors gone)?
               │        → RevealJadeRoutine() → WIN panel  ✅
               │
               └── readyForFinalHit == false (stone still has anchors)?
                        → ShowLosePanel()  ❌ → LOSE panel
```

---

## 🏗️ Implementation Plan

### Overview of Changes Required

| # | What | Where | Type |
|---|---|---|---|
| 1 | Add 3 `Image` references for the strike complete overlays | `StoneGenerator.cs` | Code change |
| 2 | Add method to fill/reset icon slots | `StoneGenerator.cs` | Code change |
| 3 | Rewrite `RegisterToolStrike()` — new win/lose logic | `StoneGenerator.cs` | Code change |
| 4 | Fix `maxStrikes = 3` | Both scenes (Inspector) | Scene fix |
| 5 | Assign `strikeText` reference | StoneGenerator Scene (Inspector) | Scene fix |
| 6 | Wire the 3 overlay images to StoneGenerator in Inspector | Both scenes | Scene wiring |

---

### Step 1 — Add Strike Icon Fields to `StoneGenerator.cs`

Add to the `[Header("--- Victory/Loss Elements ---")]` section:

```csharp
[Header("--- Strike Panel Icons ---")]
[Tooltip("Assign FirstStrikeComplete GameObject from 3-StrikeSelectionPanel-Classic")]
public GameObject strikeIcon1;  // FirstStrikeComplete

[Tooltip("Assign SecondStrikeComplete GameObject from 3-StrikeSelectionPanel-Classic")]
public GameObject strikeIcon2;  // SecondStrikeComplete

[Tooltip("Assign ThirdStrikeComplete GameObject from 3-StrikeSelectionPanel-Classic")]
public GameObject strikeIcon3;  // ThirdStrikeComplete
```

Add to `Start()` — reset icons on game load:

```csharp
ResetStrikeIcons();
```

---

### Step 2 — Add `FillStrikeIcon()` and `ResetStrikeIcons()` methods

```csharp
void FillStrikeIcon(int strikeIndex)
{
    // strikeIndex is 1-based (1, 2, 3)
    GameObject icon = strikeIndex == 1 ? strikeIcon1
                    : strikeIndex == 2 ? strikeIcon2
                    : strikeIcon3;
    if (icon != null) icon.SetActive(true);
}

void ResetStrikeIcons()
{
    if (strikeIcon1 != null) strikeIcon1.SetActive(false);
    if (strikeIcon2 != null) strikeIcon2.SetActive(false);
    if (strikeIcon3 != null) strikeIcon3.SetActive(false);
}
```

---

### Step 3 — Rewrite `RegisterToolStrike()`

**Current (broken):**
```csharp
public void RegisterToolStrike()
{
    if (isGameOver) return;

    if (readyForFinalHit)
    {
        isGameOver = true;
        StartCoroutine(RevealJadeRoutine());   // WIN
        return;
    }

    currentStrikes++;
    UpdateStrikeUI();

    if (currentStrikes >= maxStrikes && !isGameOver)
    {
        isGameOver = true;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (WinLoseManager.Instance != null) WinLoseManager.Instance.ShowLosePanel();
    }
}
```

**New (correct):**
```csharp
public void RegisterToolStrike()
{
    if (isGameOver) return;

    // ── CASE 1: readyForFinalHit ──────────────────────────────
    // All anchors are gone. This hit is the final chisel strike.
    // The stone breaks and jade reveals → WIN
    if (readyForFinalHit)
    {
        isGameOver = true;
        StartCoroutine(RevealJadeRoutine());
        return;
    }

    // ── CASE 2: Normal strike ─────────────────────────────────
    currentStrikes++;
    UpdateStrikeUI();
    FillStrikeIcon(currentStrikes);   // ← fill the Nth icon slot

    // ── CASE 3: 3rd strike reached ───────────────────────────
    if (currentStrikes >= maxStrikes && !isGameOver)
    {
        isGameOver = true;

        // Check if jade was ready — final chance win or hard loss
        if (readyForFinalHit)
        {
            // Anchors just cleared — jade reveals on 3rd strike
            StartCoroutine(RevealJadeRoutine());   // WIN
        }
        else
        {
            // Stone still had anchors → hard loss
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (WinLoseManager.Instance != null) WinLoseManager.Instance.ShowLosePanel();
        }
    }
}
```

---

### Step 4 — Scene Inspector Fixes (Both Scenes)

#### `StoneCuttingScene_Classic`
- `StoneGenerator.maxStrikes` → change from `100` to **`3`**
- Wire Inspector:
  - `strikeIcon1` → drag `FirstStrikeComplete` Image object
  - `strikeIcon2` → drag `SecondStrikeComplete` Image object
  - `strikeIcon3` → drag `ThirdStrikeComplete` Image object
  - `strikeText` → already assigned ✅

#### `StoneGenerator Scene`
- `StoneGenerator.maxStrikes` → change from `100` to **`3`**
- Wire Inspector:
  - `strikeText` → drag the `StrikeText` prefab instance
  - `strikeIcon1` → drag `FirstStrikeComplete` Image object
  - `strikeIcon2` → drag `SecondStrikeComplete` Image object
  - `strikeIcon3` → drag `ThirdStrikeComplete` Image object

---

### Step 5 — Make Icons Start Transparent (Prefab Fix)

Currently all three `XxxStrikeComplete` images have `m_Color: {r:1, g:1, b:1, a:1}` — fully
visible at all times. They need to start **transparent** so filling them is visible.

In the **prefab** or via `ResetStrikeIcons()` in `Start()`, set alpha to `0` at game start.
The `ResetStrikeIcons()` call in `Start()` handles this via code (no prefab edit needed).

---

## 🔄 Full Corrected Flow Diagram

```
Game Start
    └── ResetStrikeIcons()
            All 3 slots: alpha = 0 (invisible/empty)
            currentStrikes = 0

Player hits stone (not a final hit)
    └── RegisterToolStrike()
            currentStrikes = 1
            FillStrikeIcon(1)  → FirstStrikeComplete active = true  ✅
            UpdateStrikeUI()   → StrikeText: "STRIKES: 1"

Player hits stone again
    └── RegisterToolStrike()
            currentStrikes = 2
            FillStrikeIcon(2)  → SecondStrikeComplete active = true  ✅
            UpdateStrikeUI()   → StrikeText: "STRIKES: 2"

Player hits stone (3rd time)
    └── RegisterToolStrike()
            currentStrikes = 3
            FillStrikeIcon(3)  → ThirdStrikeComplete active = true  ✅
            UpdateStrikeUI()   → StrikeText: "STRIKES: 3"
            currentStrikes >= maxStrikes (3)?  YES
                    │
            ┌───────┴──────────────────────────────┐
            │                                       │
    readyForFinalHit == true?             readyForFinalHit == false?
    (All anchors already gone)            (Anchors still exist)
    (Updates StrikeText:                  │
     "FINAL STEP: USE CHISEL!")             │
            │                                       │
            ▼                                       ▼
    RevealJadeRoutine()                   gameOverPanel.SetActive(true)
    Jade bursts out, WIN panel            WinLoseManager.ShowLosePanel()
    WinLoseManager.ShowWinPanel()         ← LOSE PANEL SHOWS
    ← WIN PANEL SHOWS

Player destroys all anchors FIRST, then strikes
    └── Update() detects HitAnchor count == 0
            readyForFinalHit = true
            strikeText.text = "FINAL STEP: USE CHISEL!"
    └── Player strikes once more
            RegisterToolStrike()
            readyForFinalHit == true → RevealJadeRoutine() → WIN
            (strike counter is NOT incremented in this path)
```

---

## 📋 Files to Modify

| File | Change |
|---|---|
| [`StoneGenerator.cs`](Assets/Scripts/Stone/StoneGenerator.cs) | Add 3 Image fields + `FillStrikeIcon()` + `ResetStrikeIcons()` + rewrite `RegisterToolStrike()` |
| `StoneCuttingScene_Classic.unity` | Inspector: `maxStrikes = 3`, wire 3 icon Images |
| `StoneGenerator Scene.unity` | Inspector: `maxStrikes = 3`, wire `strikeText` + 3 icon Images |

> The prefab `3-StrikeSelectionPanel-Classic.prefab` itself does NOT need code changes.
> The icons start transparent via `ResetStrikeIcons()` on `Start()`.

---

## ⚠️ Issues Summary (Updated)

| # | Issue | Scene | Severity | Fix |
|---|---|---|---|---|
| 1 | `StoneGenerator.strikeText` unassigned | StoneGenerator Scene | 🔴 HIGH | Assign in Inspector |
| 2 | `maxStrikes = 100` in both scenes | Both | 🔴 HIGH | Set to 3 in Inspector |
| 3 | Strike panel icons never updated by code | Both | 🔴 HIGH | Add Image fields + FillStrikeIcon() |
| 4 | Strike icons always visible (alpha=1) at start | Both | 🟡 MEDIUM | ResetStrikeIcons() in Start() |
| 5 | Typo `"VISIVILITY"` in Visibility prefab | Both | 🟢 LOW | Fix prefab text |

---

## 🏗️ Architecture After Implementation

```
StoneGenerator.cs
    ├── int maxStrikes = 3               ← set in Inspector (was 100)
    ├── int currentStrikes = 0
    ├── Image strikeIcon1                ← FirstStrikeComplete
    ├── Image strikeIcon2                ← SecondStrikeComplete
    ├── Image strikeIcon3                ← ThirdStrikeComplete
    ├── TextMeshProUGUI strikeText       ← StrikeText prefab (both scenes)
    │
    ├── Start()
    │     └── ResetStrikeIcons()         ← all icons transparent
    │
    ├── RegisterToolStrike()
    │     ├── readyForFinalHit → WIN (RevealJadeRoutine)
    │     ├── currentStrikes++
    │     ├── FillStrikeIcon(n)          ← lights up Nth slot
    │     └── n >= 3 → readyForFinalHit? → WIN or LOSE
    │
    └── RevealJadeRoutine()
          └── WinLoseManager.ShowWinPanel()

WinLoseManager.cs       ← ShowWinPanel() / ShowLosePanel() (unchanged)
3-StrikeSelectionPanel  ← visual only, driven by Image color via StoneGenerator
StrikeText prefab       ← text "Strikes: X / 3" driven by UpdateStrikeUI()
```

---

## ✅ Implementation Status: COMPLETE (GameObject SetActive Toggling)

The coding and YAML patching stages are fully complete:

1. **`StoneGenerator.cs`** has been updated to:
   - Use `GameObject` fields for `strikeIcon1`, `strikeIcon2`, and `strikeIcon3`.
   - Implement `ResetStrikeIcons()` using `SetActive(false)` to disable the overlay GameObjects by default on Start.
   - Implement `FillStrikeIcon(int)` using `SetActive(true)` to enable the overlay GameObjects on each strike.
2. **Scene YAML files** are patched to `maxStrikes: 3` in both scenes.

---


## 🛠️ Required Manual Wiring in Unity Editor

Because nested scene/prefab hierarchies are complex and volatile to patch directly in YAML, you must perform these final quick wiring steps in the Unity Editor:

### 1. In `StoneGenerator Scene`:
1. Open `StoneGenerator Scene` in Unity.
2. Locate the **`StoneGenerator`** GameObject in the Hierarchy.
3. In the Inspector for the `StoneGenerator` script:
   - Wire the **`Strike Text`** field: Drag the **`StrikeText`** GameObject (from the UI canvas/TopBar container hierarchy).
   - Wire the **`Strike Icon 1`** field: Drag **`FirstStrikeComplete`** (child of `3-StrikeSelectionPanel-Classic` -> `First-StrikeBg`).
   - Wire the **`Strike Icon 2`** field: Drag **`SecondStrikeComplete`** (child of `3-StrikeSelectionPanel-Classic` -> `Mid-StrikeBg`).
   - Wire the **`Strike Icon 3`** field: Drag **`ThirdStrikeComplete`** (child of `3-StrikeSelectionPanel-Classic` -> `Right-StrikeBg`).
4. Save the scene.

### 2. In `StoneCuttingScene_Classic`:
1. Open `StoneCuttingScene_Classic` in Unity.
2. Locate the **`StoneGenerator`** GameObject in the Hierarchy.
3. In the Inspector for the `StoneGenerator` script:
   - Wire the **`Strike Icon 1`** field: Drag **`FirstStrikeComplete`** (child of `3-StrikeSelectionPanel-Classic` -> `First-StrikeBg`).
   - Wire the **`Strike Icon 2`** field: Drag **`SecondStrikeComplete`** (child of `3-StrikeSelectionPanel-Classic` -> `Mid-StrikeBg`).
   - Wire the **`Strike Icon 3`** field: Drag **`ThirdStrikeComplete`** (child of `3-StrikeSelectionPanel-Classic` -> `Right-StrikeBg`).
4. Save the scene.

---

# 🔦 Visibility & Estimated Value — Analysis & Fix Plan

## 🔍 System Findings

### 1. Hardcoded Stone Size (Visibility Index)
- **Current Behavior:** In `TorchInspectionManager.InspectStone()`, the lookup index `sizeIndex` is hardcoded to `1` (Medium size).
- **Correct Behavior:** `sizeIndex` must correspond to the actual size of the spawned stone (`0` = Small, `1` = Medium, `2` = Large) to retrieve the correct visibility percentage from the table shown in `visibility ss.png`.
- **Source of Truth:** `GlobalStoneData.CurrentStone.StoneSize` (which maps to `StoneSizeType` enum).

### 2. Hardcoded Points (Estimated Value)
- **Current Behavior:** `CalculateEstimatedValue()` uses a hardcoded base score of `1000` points.
- **Correct Behavior:** The base score should be retrieved dynamically from `GlobalStoneData.CurrentBlueprint.challenge_points` (or fallback models) so the appraisal matches the actual level value (e.g. 80,000 Pts).
- **Formatting:** Format ranges with thousands separators (e.g., `Est. Value: 21,250 - 148,750 Pts`).

### 3. Unassigned UI Components (StoneGenerator Scene)
- **Current Behavior:** In the `StoneGenerator Scene`, the `visibilityPercentageText` and `estimatedValueText` properties in the script are `null` (unassigned). No inspection info displays.
- **Fix Behavior:** Auto-discover the `Visibility (1)` prefab child text objects dynamically at runtime on `Start()` if they are left unassigned.

---

## 🛠️ Implementation Plan for Visibility & Appraisals

### Step 1 — Modify `TorchInspectionManager.cs`

1. **Auto-Discovery in `Start()`:**
   Find TMPro references dynamically if they are left unassigned:
   ```csharp
   void FindTextMeshProReferences()
   {
       GameObject visibilityPanel = GameObject.Find("Visibility (1)");
       if (visibilityPanel != null)
       {
           TextMeshProUGUI[] texts = visibilityPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
           foreach (var t in texts)
           {
               if (t.gameObject.name == "VisibilityText" && visibilityPercentageText == null)
               {
                   visibilityPercentageText = t;
               }
               else if (t.gameObject.name == "EstimatedValueText" && estimatedValueText == null)
               {
                   estimatedValueText = t;
               }
           }
       }
   }
   ```

2. **Retrieve Stone Size dynamically:**
   In `InspectStone()`, fetch `sizeIndex` from `GlobalStoneData.CurrentStone.StoneSize`:
   ```csharp
   int sizeIndex = 1; // Default to Medium
   if (GlobalStoneData.CurrentStone != null)
   {
       sizeIndex = (int)GlobalStoneData.CurrentStone.StoneSize;
   }
   if (sizeIndex < 0 || sizeIndex > 2) sizeIndex = 1; // Safeguard
   ```

3. **Retrieve Level Value dynamically:**
   In `CalculateEstimatedValue()`, fetch the actual blueprint points:
   ```csharp
   private int GetActualStonePoints()
   {
       if (GlobalStoneData.CurrentBlueprint != null)
       {
           return GlobalStoneData.CurrentBlueprint.challenge_points;
       }

       // fallback: check if StoneGenerator exists in scene and try to get the blueprint/points
       GameObject liveStone = GameObject.FindGameObjectWithTag("Stone");
       if (liveStone != null)
       {
           StoneGenerator sg = liveStone.GetComponent<StoneGenerator>();
           if (sg != null)
           {
               if (sg.currentStoneModel != null && sg.currentStoneModel.parsedBlueprint != null)
               {
                   return sg.currentStoneModel.parsedBlueprint.challenge_points;
               }
           }
       }

       return 1000; // default backup points
   }
   ```

4. **Update Estimated Value formatting:**
   ```csharp
   private void CalculateEstimatedValue(int visibilityPercent)
   {
       int actualPoints = GetActualStonePoints(); 
       float inaccuracyMargin = 1f - (visibilityPercent / 100f); 

       int minEstimate = Mathf.RoundToInt(actualPoints * (1f - inaccuracyMargin));
       int maxEstimate = Mathf.RoundToInt(actualPoints * (1f + inaccuracyMargin));

       if (estimatedValueText != null) 
           estimatedValueText.text = $"Est. Value: {minEstimate.ToString("N0")} - {maxEstimate.ToString("N0")} Pts";
   }
   ```

---

## ✅ Fix Status: COMPLETE (Visibility & Appraisals)

The code changes are fully complete:
- **`TorchInspectionManager.cs`** has been updated to:
  - Implement runtime auto-discovery of TMPro text component references (`FindTextMeshProReferences()`).
  - Read `GlobalStoneData.CurrentStone.StoneSize` dynamically for accurate table index lookup.
  - Fetch `GlobalStoneData.CurrentBlueprint.challenge_points` dynamically for true-value calculation.
  - Fix default typos (`"VISIVILITY"`) at start.
  - Update texts dynamically from `0%` to `X%` and `??? Pts` to actual points range on torch activation.
  - Reset texts back to `Visibility: 0%` and `Est. Value: ??? Pts` dynamically when the torch is turned off.
  - Format output values beautifully with commas.

*System Updated & Implemented — 2026-06-23*

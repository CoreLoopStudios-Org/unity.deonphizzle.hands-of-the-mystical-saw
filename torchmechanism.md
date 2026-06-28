# 🔦 Torch Mechanism – Full Analysis

> Covers: `StoneGenerator Scene.unity` and `StoneCuttingScene_Classic.unity`

---

## 1. Overview

The torch system is split across **four scripts** and two architectural layers:

| Layer | Scripts | Responsibility |
|---|---|---|
| **Visual / Physical** | `SimpleTorch.cs`, `TorchManager.cs`, `TorchFollower.cs` | Light on/off, mesh visibility, 3D cursor following mouse |
| **Logic / Inspection** | `TorchInspectionManager.cs`, `StoneSpinController.cs` | Material swapping (X-Ray), state gating, UI percentages |

The two layers are connected by a single static boolean: **`StoneSpinController.GlobalTorchActive`**.

---

## 2. Component Map (Per Scene)

### StoneGenerator Scene

| GameObject | Script Attached | Role |
|---|---|---|
| `StoneSpinController` (on stone holder) | `StoneSpinController.cs` | Master controller – handles spin, torch toggle, commit |
| `TorchFollower` (torch 3D model) | `TorchFollower.cs` | Follows mouse over stone surface via raycast |
| `TorchManager (2)` | `TorchInspectionManager.cs` | Handles material states & visibility % UI |
| Torch light child objects | `SimpleTorch.cs` | Simple enable/disable of light + visual mesh |

### StoneCuttingScene_Classic

| GameObject | Script Attached | Role |
|---|---|---|
| `StoneSpinController` (on stone holder) | `StoneSpinController.cs` | Same as above |
| `TorchFollower` (torch 3D model) | `TorchFollower.cs` | Same as above |
| `TorchManager (1)` | `TorchInspectionManager.cs` | Same logic, **different UI/Particle references** |
| Torch light child objects | `TorchManager.cs` | Slightly extended version of `SimpleTorch` with `isActive` tracking |

> ⚠️ **Naming Confusion**: GameObjects are named "TorchManager", but they run `TorchInspectionManager.cs`. The standalone `TorchManager.cs` script is assigned only to torch light child objects, not to the "TorchManager" GameObjects.

---

## 3. Script-by-Script Breakdown

### 3.1 `StoneSpinController.cs` — The Gatekeeper

**Path**: `Assets/Scripts/Stone/StoneSpinController.cs`

This is the **entry point** for the torch in both scenes.

```csharp
public static bool GlobalTorchActive = false; // Read by TorchFollower every frame
```

**Key method: `ToggleTorch()`**

```
User presses [Space] or taps Torch Button
         │
         ▼
ToggleTorch() is called
         │
         ├─ isTorchActive = true
         │   ├─ GlobalTorchActive = true        ← TorchFollower reads this
         │   ├─ isSpinning = false              ← Stone stops spinning
         │   ├─ stoneRenderer.material = xRayMaterial ← Old legacy X-Ray
         │   └─ TorchInspectionManager.Instance.InspectStone() ← Triggers %
         │
         └─ isTorchActive = false
             ├─ GlobalTorchActive = false
             ├─ isSpinning = true               ← Stone resumes spinning
             └─ stoneRenderer.material = originalMaterial
```

**Usage limits**:
- `maxTorchUses` (default 3) – after 3 torch activations, the torch button is disabled.
- `CommitFreeze()` also deactivates the torch if it was on.

---

### 3.2 `TorchFollower.cs` — 3D Visual Cursor

**Path**: `Assets/Scripts/Torch/TorchFollower.cs`

This script drives the **physical torch model** that floats over the stone surface.

**Every frame in `Update()`:**
```
Check StoneSpinController.GlobalTorchActive
    │
    FALSE → SetTorchVisibility(false) → return
    │
    TRUE → Cast ray from Camera to mouse position
               │
               ├─ Ray hits object tagged "Stone" or name contains "Stone"
               │   ├─ Show torch mesh + light
               │   ├─ Lerp position to (hit.point + hit.normal * hoverDistance)
               │   └─ Slerp rotation to face stone surface normal
               │
               └─ Ray misses or hits non-stone → Hide torch
```

**Settings exposed in Inspector:**
| Field | Default | Description |
|---|---|---|
| `hoverDistance` | 0.5f | How far above the stone surface the torch floats |
| `rotationOffset` | (0,0,0) | Corrects model orientation if torch faces wrong direction |

---

### 3.3 `TorchInspectionManager.cs` — X-Ray & Inspection Logic

**Path**: `Assets/Scripts/Stone/TorchInspectionManager.cs`

This is the **core logic script** that handles:
- Material swapping based on wet/dry and torch on/off state
- Shader parameter `_TorchPosition` update (creates X-Ray reveal effect)
- Visibility percentage and estimated value calculation

#### 4 Material States

```
                 Torch OFF          Torch ON
Stone Dry    │  dryMaterial     │  dryXRayMaterial  │
Stone Wet    │  wetMaterial     │  wetXRayMaterial  │
```

Material is assigned every time `TurnOnTorch()`, `TurnOffTorch()`, or `ApplyWaterFromButton()` is called.

#### Torch Position Shader Update (in `Update()`)

```csharp
// Runs every frame while torch is active
renderer.material.SetVector("_TorchPosition", torchTransform.position);
```

This passes the 3D world position of the `TorchFollower` object into the X-Ray shader so the reveal "spotlight" follows the torch model.

When torch is turned off, `_TorchPosition` is reset to `(0, 10000, 0)` to push the effect off-screen and prevent shader glitches.

#### Visibility & Estimated Value Calculation

```csharp
// sizeIndex = 1 (hardcoded — Medium stone only)
int[,] dryVisibility  = { { 9, 11, 13 }, { 6, 8, 10 }, { 4, 6, 8 } };
int[,] wetVisibility  = { { 29, 32, 36 }, { 25, 28, 32 }, { 21, 25, 29 } };
```

The lookup table uses `[stoneSize, torchSize]` to determine visibility percentage.
- Stone size is **hardcoded to index 1 (Medium)** ← Potential bug/limitation.
- Torch size is controlled by `currentTorch` enum (`Small=0`, `Medium=1`, `Large=2`).

Estimated value is derived from 1000 base points ± the inaccuracy margin (100% - visibility%):
```
minEstimate = 1000 * (1 - inaccuracyMargin)
maxEstimate = 1000 * (1 + inaccuracyMargin)
```

---

### 3.4 `SimpleTorch.cs` / `TorchManager.cs` — Light Toggle Helpers

**Paths**: `Assets/Scripts/Torch/SimpleTorch.cs`, `Assets/Scripts/Torch/TorchManager.cs`

These are near-identical utility scripts used on **child GameObjects** containing the torch `Light` component and the torch mesh.

| Feature | SimpleTorch | TorchManager |
|---|---|---|
| Toggle light on/off | ✅ | ✅ |
| Toggle mesh on/off | ✅ | ✅ |
| Force pixel render mode | ✅ | ✅ |
| `isActive` state tracking | ❌ | ✅ |
| Debug.Log position | ❌ | ✅ |

`TorchManager.cs` is a slightly richer version. Both expose `ToggleTorch(bool state)` for external calling.

---

## 4. Full Data Flow Diagram

```
[User Input: Spacebar / Touch Button]
            │
            ▼
  StoneSpinController.ToggleTorch()
            │
            ├───► GlobalTorchActive = true/false
            │               │
            │               ▼
            │     TorchFollower.Update()
            │       - Reads GlobalTorchActive
            │       - Raycasts from camera → mouse → Stone
            │       - Moves torch model to stone surface
            │       - Torch position fed to shader via:
            │         TorchInspectionManager sets _TorchPosition
            │
            ├───► isSpinning toggled (stone stops/resumes)
            │
            ├───► stoneRenderer.material = xRayMaterial (legacy path)
            │       OR
            │     TorchInspectionManager.UpdateStoneMaterial()
            │       - Picks from 4 materials (dry/wet × off/on)
            │
            └───► TorchInspectionManager.InspectStone()
                    - Computes visibility % from lookup table
                    - Updates visibilityPercentageText UI
                    - Updates estimatedValueText UI
```

---

## 5. Differences Between Scenes

| Aspect | StoneGenerator Scene | StoneCuttingScene_Classic |
|---|---|---|
| TorchManager object | `TorchManager (2)` | `TorchManager (1)` |
| Script on TorchManager GO | `TorchInspectionManager.cs` | `TorchInspectionManager.cs` |
| Light script | `SimpleTorch.cs` | `TorchManager.cs` |
| UI references | Generator-specific TextMeshPro refs | Classic-specific TextMeshPro refs |
| Particle system | Generator water particle | Classic water particle |
| Core logic | **Identical** | **Identical** |

---

## 6. Known Issues & Limitations

| # | Issue | Location | Impact |
|---|---|---|---|
| 1 | `sizeIndex` is **hardcoded to 1** (Medium) | `TorchInspectionManager.cs:138` | Stone size has no effect on inspection result |
| 2 | `StoneSpinController.ToggleTorch()` sets `xRayMaterial` directly **and** `TorchInspectionManager` also swaps material → double-swap on turn-on | `StoneSpinController.cs:264`, `TorchInspectionManager.cs:129` | Last assignment wins; can cause one-frame flicker |
| 3 | `_TorchPosition` is set in both `StoneSpinController.UpdateTorchPosition()` **and** `TorchInspectionManager.Update()` — two separate writers | Both scripts | Whichever runs last in frame order wins |
| 4 | `TorchFollower` checks both `.CompareTag("Stone")` and `.name.Contains("Stone")` — inconsistency | `TorchFollower.cs:35` | Tag-based approach preferred; name-based is fragile |
| 5 | `TorchManager.cs` and `SimpleTorch.cs` are near-duplicates | Both files | Code duplication; one should be removed |

---

## 7. How to Extend / Fix

### Fix: sizeIndex hardcode
In `TorchInspectionManager.cs`, expose a `stoneSizeIndex` field and set it from the stone spawner:
```csharp
// Replace:
int sizeIndex = 1;
// With:
public int stoneSizeIndex = 1; // Set externally by StoneSpawner
```

### Fix: Double material assignment
Remove the `stoneRenderer.material = xRayMaterial` line from `StoneSpinController.ToggleTorch()` and let `TorchInspectionManager` handle all material changes exclusively via `TurnOnTorch()` / `TurnOffTorch()`.

### Fix: Dual `_TorchPosition` writers
Remove `UpdateTorchPosition()` from `StoneSpinController` entirely. `TorchInspectionManager.Update()` already handles this via `torchTransform`.

### Fix: Torch visibility tag inconsistency
Use only the Tag check in `TorchFollower`:
```csharp
// Replace:
if (hit.collider.CompareTag("Stone") || hit.collider.name.Contains("Stone"))
// With:
if (hit.collider.CompareTag("Stone"))
```
Make sure all stone objects have the `Stone` tag assigned in the Inspector.

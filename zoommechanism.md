# 🔍 Zoom & Torch-Zoom Mechanism – Full Analysis

> Covers: `StoneGenerator Scene.unity` and `StoneCuttingScene_Classic.unity`

---

## 1. Overview

There are **two separate and independent zoom systems** in the project. They serve completely different purposes and run in different contexts:

| System | Script | Trigger | Technique | Scene |
|---|---|---|---|---|
| **General / Torch Zoom** | `ToolCameraManager.cs` | UI Button (`ZoomInOnStone` / `ZoomOutToDefault`) | FOV interpolation + background parallax | Both |
| **Hammer Camera Follow** | `HitController.cs` (inside `HammerController`) | Mouse held (LMB) | Camera position interpolation (Z-axis) | Both |

> ℹ️ There is **no dedicated torch-specific zoom script**. Torch zoom is handled by the same `ToolCameraManager` — the torch and stone inspection just share the general zoom system.

---

## 2. System 1 — `ToolCameraManager.cs` (General / Torch Zoom)

**Path**: `Assets/Scripts/Tool/ToolCameraManager.cs`

This is the **primary zoom system** used by UI buttons during stone inspection and torch usage.

### 2.1 How It Works

```
User taps [Zoom In Button] on UI
         │
         ▼
  ToolCameraManager.ZoomInOnStone()
         │
         └─ isZoomingIn = true
                   │
                   ▼ (LateUpdate runs every frame)
         cam.fieldOfView lerps → zoomInFOV (default: 30°)
         backgroundImage.localScale lerps → defaultBgScale * backgroundZoomOutScale (default: ×0.9)

User taps [Zoom Out Button]
         │
         ▼
  ToolCameraManager.ZoomOutToDefault()
         │
         └─ isZoomingIn = false
                   │
                   ▼ (LateUpdate runs every frame)
         cam.fieldOfView lerps → defaultFOV (saved at Start)
         backgroundImage.localScale lerps → defaultBgScale (original scale)
```

### 2.2 Inspector Settings

| Field | Default | Description |
|---|---|---|
| `zoomInFOV` | `30f` | Target FOV when fully zoomed in. Lower = more zoom. |
| `zoomSpeed` | `5f` | Lerp speed for both FOV and background scale |
| `backgroundImage` | (ref) | `RectTransform` of the 2D background canvas image |
| `backgroundZoomOutScale` | `0.9f` | Multiplier for background during zoom (0.9 = 10% shrink) |
| `nearClipPlane` | `0.01f` | Applied to camera at Start to prevent close-object clipping |

### 2.3 Initialization (in `Start()`)

```csharp
defaultFOV = cam.fieldOfView;           // Saves original FOV (usually 60°)
cam.nearClipPlane = nearClipPlane;      // Prevents models from being clipped when close
defaultBgScale = backgroundImage.localScale; // Saves original background scale
```

### 2.4 Per-Frame Zoom (in `LateUpdate()`)

Uses `LateUpdate` (not `Update`) so it runs **after all other scripts**, ensuring camera changes happen last — preventing jitter from tool controllers fighting the camera.

```csharp
// Zoom IN path
cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, zoomInFOV, Time.deltaTime * zoomSpeed);
backgroundImage.localScale = Vector3.Lerp(backgroundImage.localScale, defaultBgScale * 0.9f, ...);

// Zoom OUT path
cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, defaultFOV, Time.deltaTime * zoomSpeed);
backgroundImage.localScale = Vector3.Lerp(backgroundImage.localScale, defaultBgScale, ...);
```

### 2.5 Singleton Access

```csharp
public static ToolCameraManager Instance;
```

Available for any script to call `ToolCameraManager.Instance.ZoomInOnStone()` from code (not just UI buttons).

### 2.6 Torch Zoom Relationship

There is **no separate torch zoom mode** in code. When the torch is activated via `StoneSpinController.ToggleTorch()`:
- Tools are disabled (via `ToolController.Update()` checking `GlobalTorchActive`)
- The Zoom In button on the UI can still be pressed by the player manually
- `ToolCameraManager` handles the zoom the same way regardless of torch state

> ⚠️ **Key Finding**: "Torch Zoom" is simply the general zoom system used **during** torch inspection. They share the same `ToolCameraManager` — there is no separate torch-specific zoom path.

---

## 3. System 2 — `HammerController.cs` (Hammer Camera Follow Zoom)

**Path**: `Assets/Scripts/Hammer/HitController.cs`

This is a **secondary, hammer-specific zoom** that works differently from the FOV system.

### 3.1 How It Works

This zoom does **not change the FOV**. Instead, it physically moves the camera position along the Z-axis when the player holds down the mouse button with the hammer selected.

```
Player holds LMB (with hammer selected)
         │
         ▼
  HammerController.Update() → mouse button down
         │
         └─ enableCameraFollow = true?
               └─ targetCamPos = initialCamPos + new Vector3(0, 0, cameraZoomAmount)
                  mainCam.transform.position = Lerp(current, targetCamPos, dt * cameraSmoothSpeed)

Player releases LMB
         │
         ▼
  HammerController.Update() → mouse button up
         └─ mainCam.transform.position = Lerp(current, initialCamPos, dt * cameraSmoothSpeed)
```

### 3.2 Inspector Settings

| Field | Default | Description |
|---|---|---|
| `enableCameraFollow` | `true` | Toggle the camera zoom on/off entirely |
| `cameraZoomAmount` | `2f` | How many units the camera moves forward on Z (+Z = closer to stone) |
| `cameraSmoothSpeed` | `5f` | Lerp speed of the physical camera move |

### 3.3 Key Difference from ToolCameraManager

| Aspect | ToolCameraManager | HammerController |
|---|---|---|
| Zoom technique | FOV change | Camera position move (Z) |
| Trigger | UI Button (tap) | Mouse button held |
| State | Toggle (stays zoomed until button pressed again) | Held (returns on release) |
| Background effect | Yes (parallax scale) | No |
| Works during torch? | Yes (UI still accessible) | No (hammer disabled when `GlobalTorchActive`) |

---

## 4. Full Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     ZOOM SYSTEMS                             │
├──────────────────────────┬──────────────────────────────────┤
│   ToolCameraManager      │    HammerController               │
│   (FOV-based zoom)       │    (Position-based zoom)          │
│                          │                                    │
│  Trigger: UI Button      │  Trigger: LMB held                │
│  State: Toggle           │  State: Hold                       │
│  FOV: 60° → 30°          │  Position: +2 units on Z          │
│  Background: ×0.9 scale  │  Background: unchanged            │
│  LateUpdate              │  Update                            │
│  Works with torch: YES   │  Works with torch: NO              │
└──────────────────────────┴──────────────────────────────────┘
             │
             │ Both read from / write to:
             ▼
        Camera.main (Main Camera)
```

---

## 5. Torch + Zoom Interaction (Combined View)

When the torch is active (`GlobalTorchActive = true`):

```
GlobalTorchActive = true (set by StoneSpinController.ToggleTorch)
         │
         ├─ ToolController.Update() → returns early (tool disabled)
         ├─ ToolSwitcher.Select*() → returns early (all tool switches blocked)
         ├─ HammerController.Update() → hammer selected = false, returns to start
         │
         └─ ToolCameraManager is UNAFFECTED
               └─ Player can still press Zoom In / Zoom Out buttons
               └─ FOV zoom works normally during torch inspection
```

**Sequence of a typical torch inspection with zoom:**

```
1. Stone is spinning
2. Player presses [Torch Button]
   → Stone stops spinning
   → GlobalTorchActive = true
   → All tools disabled
   → TorchFollower begins showing 3D torch cursor on stone
3. Player taps [Zoom In Button]
   → ToolCameraManager.ZoomInOnStone()
   → FOV transitions from 60° to 30° over ~0.2s
   → Background shrinks 10%
4. Player moves mouse over stone
   → TorchFollower tracks stone surface
   → TorchInspectionManager sends _TorchPosition to shader
   → X-Ray reveal follows torch
5. Player taps [Zoom Out Button]
   → ToolCameraManager.ZoomOutToDefault()
   → FOV returns to 60°
6. Player presses [Torch Button] again
   → Torch deactivates
   → Stone resumes spinning
   → Tools re-enabled
```

---

## 6. ToolController Zoom Lock (Important)

`ToolController.cs` contains this guard at the top of `Update()`:

```csharp
if (StoneSpinController.GlobalTorchActive)
{
    isSelected = false;  // Force deselect the tool
    ReturnToStart();     // Move tool back to rest position
    return;              // Skip all tool movement logic
}
```

This effectively **prevents any tool-based zoom** (like the hammer's position-based zoom) while the torch is active. Only the FOV-based `ToolCameraManager` zoom remains accessible.

---

## 7. Known Issues & Limitations

| # | Issue | Location | Impact |
|---|---|---|---|
| 1 | `ToolCameraManager.ZoomInOnStone()` is **never called from code** — only via Unity UI Button OnClick events. No code-driven zoom. | `ToolCameraManager.cs:82` | Zoom cannot be triggered by game events (e.g. auto-zoom when torch activates) |
| 2 | Hammer zoom moves **camera position** while `ToolCameraManager` changes **FOV** — if both run at once, they conflict | `HitController.cs:88`, `ToolCameraManager.cs:59` | In practice avoided because hammer is disabled during torch, but could be triggered simultaneously |
| 3 | `defaultFOV` is captured at `Start()` — if another script changes FOV before Start runs, the saved baseline is wrong | `ToolCameraManager.cs:39` | Rare but possible in additive scene loading |
| 4 | `backgroundImage` is nullable — if not assigned in Inspector, zoom still works but background has no parallax response | `ToolCameraManager.cs:62` | Silent failure; no null warning |
| 5 | No `zoomOutFOV` field — zoom out always returns to `defaultFOV` captured at startup; cannot set a different "normal" FOV | `ToolCameraManager.cs` | Inflexible if the game needs dynamic baseline FOV changes |

---

## 8. Recommended Fixes / Improvements

### Auto-zoom when torch activates
Trigger `ToolCameraManager.Instance.ZoomInOnStone()` inside `StoneSpinController.ToggleTorch()`:
```csharp
// In StoneSpinController.ToggleTorch(), after setting GlobalTorchActive = true:
if (ToolCameraManager.Instance != null)
    ToolCameraManager.Instance.ZoomInOnStone();

// And on deactivation:
if (ToolCameraManager.Instance != null)
    ToolCameraManager.Instance.ZoomOutToDefault();
```

### Fix potential backgroundImage null warning
Add a defensive null-check log in `Start()`:
```csharp
if (backgroundImage == null)
    Debug.LogWarning("[ToolCameraManager] backgroundImage is not assigned. Parallax disabled.");
```

### Expose zoomOutFOV for flexibility
```csharp
// Add field:
public float zoomOutFOV = 0f; // 0 = use defaultFOV
// In ZoomOutToDefault():
float targetFOV = zoomOutFOV > 0 ? zoomOutFOV : defaultFOV;
cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
```

### Prevent hammer zoom conflicting with FOV zoom
Add a guard in `HammerController.Update()`:
```csharp
// Skip hammer camera follow if ToolCameraManager is actively zooming
if (enableCameraFollow && mainCam != null && ToolCameraManager.Instance != null && !ToolCameraManager.Instance.IsZooming)
{
    // ... existing zoom code ...
}
```
This requires adding a public `bool IsZooming => isZoomingIn;` property to `ToolCameraManager`.

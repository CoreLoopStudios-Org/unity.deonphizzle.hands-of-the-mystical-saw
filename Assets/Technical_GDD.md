# Technical Game Design Document (GDD)

## Project Overview
**Title:** Hands of the Mystical Saw
**Genre:** Action / Puzzle / Physics‑Based Cutting
**Engine:** Unity (2021+)
**Target Platforms:** PC (Windows) – future mobile support possible.

## Core Gameplay Loop
1. **Player enters a scene** (MainMenu → select mode).
2. **Select a stone** (generated or pre‑placed in the level).
3. **Aim the mystical saw** using the virtual joystick / mouse.
4. **Slice the stone** – the `CubeSlicer` component cuts the mesh along a plane.
5. **Physics reaction** – sliced pieces receive impulse (`force` parameter) and fall.
6. **Score / feedback** – points are awarded based on precision, number of pieces, and time.
7. **Repeat** – new stones are spawned, or the player proceeds to the next challenge.

## Playable Scenes
| Scene | Purpose | Main Scripts / Systems |
|-------|---------|------------------------|
| **MainMenu.unity** | UI entry point, mode selection. | `VirtualJoystick.cs` (UI), `AnchorBlinker.cs` (menu effects). |
| **PredictorScene.unity** | Shows predictive trajectory / tutorial. | `JadeCuttingGame.cs` (tutorial logic). |
| **StoneCuttingScene_Classic.unity** | Core gameplay – classic stone‑cutting mode. | `CubeSlicer.cs`, `SliceController.cs`, `HammerAction.cs`, `StrikeSystem.cs`, `destroyOnHit.cs`, `moveAtDirection.cs`. |
| **StoneGenerator Scene.unity** | Procedural generation of stone meshes. | `Stone.cs` (scriptable object), generation utilities in `GenerateStone` folder. |
| **StoneViewerScene.unity** | Visual inspection of generated stones, debugging. | Uses `CubeSlicer.cs` for slicing preview. |
| **modelcheckscene.unity** | Development / QA scene to validate meshes and colliders. | Minimal – uses `CubeSlicer.cs` for quick cuts. |

## Key Systems & Scripts
- **`CubeSlicer.cs`** – Performs mesh slicing, creates new GameObjects with mesh colliders, applies force, and re‑adds the slicer component for recursive cuts.
- **`SliceController.cs`** – Handles player input, creates slicing planes, and calls `CubeSlicer.Slice`.
- **`HammerAction.cs`** – Detects hammer swing input, triggers slice operation, plays VFX/SFX.
- **`StrikeSystem.cs`** – Manages hit detection, combo logic, and scoring.
- **`VirtualJoystick.cs`** – Provides on‑screen joystick for mobile/touch control.
- **`AnchorBlinker.cs`** – Visual cue for selectable objects (used in menus).
- **`destroyOnHit.cs` & `moveAtDirection.cs`** – Simple physics helpers for debris.
- **Scriptable Objects (`Stone` folder)** – Store stone properties (hardness, reward, prefab reference).
- **Input System** – Configured via `InputSystem_Actions.inputactions` for actions `Slice`, `Move`, `Select`.

## Game Flow Overview
```mermaid
graph LR
    A[Start – MainMenu] --> B{Select Mode}
    B -->|Classic| C[StoneCuttingScene_Classic]
    B -->|Tutorial| D[PredictorScene]
    C --> E[Spawn Stone]
    E --> F[Player Aims Saw]
    F --> G[Slice Trigger (Hammer/Swipe)]
    G --> H[CubeSlicer.Slice]
    H --> I[Create Pieces + Apply Force]
    I --> J[Score Calculation]
    J --> K{More Stones?}
    K -->|Yes| E
    K -->|No| L[Level Complete]
    L --> M[Return to MainMenu]
``` 

## Technical Details
### Mesh Slicing
- Uses **local‑space plane** conversion (`transform.InverseTransformDirection`).
- Generates two vertex/triangle lists (`leftVerts/rightVerts`).
- Caps the sliced area with a material (`capMaterial`).
- Adds a **convex `MeshCollider`** and a **`Rigidbody`** for physics.
- Recursively attaches `CubeSlicer` to new pieces for further cuts.

### Physics
- Force magnitude is configurable via `CubeSlicer.force` (default `3f`).
- `Rigidbody` is created for each slice with `ForceMode.Impulse`.
- Colliders are set to **convex** for stability.

### Input
- Unity Input System asset (`InputSystem_Actions.inputactions`).
- Actions:
  - `Slice` (triggered by left mouse button / screen tap).
  - `Move` (virtual joystick or WASD).
  - `Select` (UI navigation).

### Scoring & Progression
- Points per slice = `baseScore * precisionMultiplier`.
- Precision is derived from angle between slice normal and ideal cut plane (metadata stored in `Stone` scriptable object).
- Combo counter increments when consecutive successful slices occur within a time window.
- Progress saved using `PlayerPrefs` (current level, high score).

## Asset Organization (Assets folder)
```
Assets/
├─ ALL‑SCENE‑IS HERE/          # All .unity scenes
├─ CubeSlicer.cs               # Core slicing logic
├─ SliceController.cs          # Input handling (to be reviewed)
├─ Scripts/
│   ├─ AnchorBlinker.cs
│   ├─ HammerAction.cs
│   ├─ JadeCuttingGame.cs
│   ├─ StrikeSystem.cs
│   ├─ destroyOnHit.cs
│   └─ moveAtDirection.cs
├─ Scriptable Object/          # Stone definitions
├─ Materials/                  # capMaterial, UI materials
├─ Prefabs/                    # Stone prefab, saw prefab
└─ Technical_GDD.md           # <<‑ THIS FILE
```

## Next Steps / Open Items
- Verify that **`SliceController.cs`** exists (currently missing in file list). If absent, create a thin wrapper that forwards input to `CubeSlicer`.
- Populate **`Stone` scriptable objects** with proper hardness values.
- Add UI hooks in **MainMenu** to load appropriate scenes via `SceneManager.LoadScene`.
- Implement a **tutorial flow** in `PredictorScene` using `JadeCuttingGame.cs`.
- Optimize mesh slicing for large meshes (consider asynchronous job system).

---
*Document generated automatically from project analysis. Place this file in the `Assets` folder to keep it version‑controlled with the rest of the project.*

# Project Agents & Architecture: unity.coremechanism.deonphizzle

This document provides a technical overview of the project's systems, scripts, and architectural patterns to assist developers and AI agents in navigating the codebase.

## 🏛 Architecture: Model-View-Controller (MVC)

The project follows an MVC-like structure to separate data, logic, and presentation.

### 📦 Models (`Assets/Scripts/MVC/Models/`)
*   **`CurrentStoneModel.cs`**: A `ScriptableObject` that acts as the source of truth for the active stone. It integrates with the `StoneCutter.Sdk` and parses JSON blueprints.
*   **`StoneBlueprint.cs`**: Defines the data structure for a stone (physics, rotation patterns, anchor network, and jade core details).
*   **`StoneServer.cs`**: A Singleton that persists game state and stone lists across scenes.
*   **`DataManager.cs`**: Manages global persistent data (points, tier, username).

### 🎮 Controllers (`Assets/Scripts/MVC/Controllers/`)
*   **`JadeCuttingGame.cs`**: The central orchestrator for the game loop, timer, and high-level game state.
*   **`ToolManager.cs` / `ToolSwitcher.cs`**: Handle tool selection, activation, and success chance logic.
*   **`StoneSpinController.cs`**: Manages the movement and rotation logic of the stone based on its blueprint.

### 🖼 Views (`Assets/Scripts/MVC/Views/`)
*   UI managers like `CanvasManager`, `ButtonGroupManager`, and `StoneItemUI` handle user input and visual updates.

---

## 💎 Core Gameplay Mechanics

### 1. Stone Generation (`StoneGenerator.cs`)
*   **Procedural Spawning**: Dynamically generates the stone mesh, internal **Jade Core**, and **Hit Anchors**.
*   **Anchors**: Targets that must be interacted with to progress.
*   **Reveal Logic**: Handles the shattering effect when the stone is successfully cut.

### 2. Tool System
Each tool has unique interaction logic:
*   **Hammer (`NewHammerController.cs`)**: Uses a mechanical swing sequence with `Linecast` for impact detection.
*   **Saw (`SawController.cs`)**: Implements mesh slicing using the `EzySlice` library.
*   **Chisel & Dremel (Base Rotation & Aiming)**
    *   **Forward-Backward Rotation Analysis**:
        *   **Input**: Two UI buttons (Left/Right) configured with `Event Trigger` (`PointerDown` to start, `PointerUp` to stop).
        *   **Mechanic**: These buttons control `baseRotationDirection` (-1, 0, 1).
        *   **Forward Analysis**: Pressing the buttons rotates the `rootBone` around the `baseRotationAxis`. By default, this axis is `Vector3.right`, meaning the "Left/Right" buttons actually perform a **Forward-Backward tilt** (Pitch) of the tool base.
        *   **Backward Analysis (State Sync)**: The rotation buttons use `Transform.Rotate()` (direct scene modification), while the Joystick uses internal state variables (`currentAimSide`, `currentAimUp`). 
        *   **Caveat**: If the base is rotated via buttons and then the joystick is used, the tool may "snap" back to its previous orientation because the buttons do not update the joystick's internal aim variables.
    *   **Chisel (`ManualChiselController.cs` & `Chisel_rigged`)**
        *   **Architecture (Version 2 - Finalized)**:
            *   **Rigging System**: Operates on a 3-tier bone hierarchy:
                *   **Root Bone**: Handles side-to-side (Horizontal) rotation (via Joystick) and Forward-Backward tilt (via Buttons).
                *   **Tilt Bone**: Handles up-and-down (Vertical) pivoting.
                *   **Extend Bone**: Handles the linear "strike" movement (piston-like action).
                *   **Chisel Tip**: A dedicated transform at the tool's point used for precise collision tracking.
            *   **Aiming Mechanics**:
                *   Uses **Incremental Movement**: Joystick input is added (`+=`) to current rotation states (`currentAimUp`, `currentAimSide`) rather than mapped absolutely.
                *   Includes deadzones (0.05f) and configurable clamping to match the model's mechanical limits.
            *   **Hitting Logic (Continuous Linecast)**:
                *   During the strike extension, the controller performs a `Physics.RaycastAll` in every frame between the `chiselTip`'s previous and current world positions.
                *   **Self-Collision Prevention**: Uses `IsChildOf` filtering to ensure the ray ignores the chisel's own colliders.
                *   **Hit Registration**: If a target (Stone, Jade, or Anchor) is detected, the movement stops instantly at the impact point.
                *   **Logic Trigger**: Automatically calls `StoneGenerator.RegisterToolStrike()` and handles `HitAnchor` destruction.
    *   **Legacy Version (Version 1 - `ChiselController.cs`)**:
        *   Uses absolute position Lerping toward a mouse-click point. 
        *   Lacks rigged bone support and continuous collision; prone to penetrating objects.
*   **Dremel (`DremelToolController.cs` & `Dramel_rigged`)**:
    *   **Architecture (Version 3 - Manual Movement Update)**: Uses a stationary mechanical base with a focus on manual precision extension.
        *   **Rigging System**: 
            *   **Root Bone** (`Root`): Handles side-to-side rotation (Joystick X).
            *   **Tilt Bone** (`Up_down_1`): Up-and-down pivoting (Joystick Y).
            *   **Extend Bone** (`Up_down_extended`): Handles linear Forward/Backward movement via UI buttons (`MoveForward`/`MoveBackward`).
            *   **Dremel Tip**: Continuously spinning bit at the end of the extend bone.
        *   **Manual Movement Mechanic**: 
            *   The UI buttons (formerly Left/Right, now Forward/Backward) translate the `Up_down_extended` bone along the `strikeAxis`.
            *   Includes internal clamping to ensure the drill bit stays between its `initialPosition` and `maxExtensionDistance`.
        *   **Aiming & Movement**: Utilizes incremental joystick input for smooth aiming, preventing snapping.
        *   **Hitting Logic (Continuous Grinding)**:
            *   When the automated `StartGrinding()` sequence is triggered, the tool extends and performs continuous `Linecast` to detect the stone surface.
            *   Once contact is made, triggers a grinding coroutine that applies damage (dents/sparks/strikes) every `0.15s` as long as contact is maintained.
    *   **Legacy Version**: Operated as a free-floating handheld tool using screen-to-world raycasting.

### 3. Torch & Inspection
*   **X-Ray Mechanic**: Controlled by `SimpleTorch.cs` and `StoneSpinController.cs`. It swaps the stone's material to a custom X-ray shader to reveal internal contents.
*   **Game State Integration**: Activating the torch usually pauses stone movement and increases tool success rates.

---

## 🛠 Key Technical Components

| Component | File Path | Responsibility |
| :--- | :--- | :--- |
| **Game Loop** | `JadeCuttingGame.cs` | Timer, Prize, Strikes, and Game Over logic. |
| **Movement** | `StoneSpinController.cs` | Oscillation, Linear, Circular, and Chaotic patterns. |
| **Strikes** | `StrikeSystem.cs` | Visual UI management for failed attempts. |
| **Data SDK** | `CurrentStoneModel.cs` | Bridge between game logic and external stone data. |

## ⚠️ Known Conventions & Notes
*   **Global State**: `StoneSpinController.GlobalTorchActive` is a static flag used to lock tool interactions when the torch is in use.
*   **Tags & Layers**: Uses "Stone", "Jade", and "Anchor" tags extensively for raycasting and collision filtering.
*   **External Libs**: Depends on `EzySlice` for real-time mesh cutting.

---

*Last Updated: May 2026*

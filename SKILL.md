---
name: unity-jade-stone-cutting
description: Analyze, modify, and build core game mechanisms for the Unity Jade Stone Cutting Core simulation project.
---

# Unity Jade Stone Cutting Simulation Guide

This skill guide provides the rules, design patterns, and system overviews for developers and agents working on the Unity Jade Stone Cutting project.

## 🏛 Architectural Pattern: MVC

This project maintains a clean separation of concerns using Model-View-Controller (MVC):

- **Models (`Assets/Scripts/MVC/Models/`)**:
  - [CurrentStoneModel.cs](file:///C:/Users/User/Documents/GitHub/unity.deonphizzle.hands-of-the-mystical-saw/Assets/Scripts/MVC/Models/CurrentStoneModel.cs): Source of truth for the active stone. Integrates with the SDK and parses JSON blueprints.
  - [StoneBlueprint.cs](file:///C:/Users/User/Documents/GitHub/unity.deonphizzle.hands-of-the-mystical-saw/Assets/Scripts/MVC/Models/StoneBlueprint.cs): Defines the structural, physics, and positional properties of each stone.
  - [StoneServer.cs](file:///C:/Users/User/Documents/GitHub/unity.deonphizzle.hands-of-the-mystical-saw/Assets/Scripts/MVC/Models/StoneServer.cs): Singleton for game state persistence.
  - [DataManager.cs](file:///C:/Users/User/Documents/GitHub/unity.deonphizzle.hands-of-the-mystical-saw/Assets/Scripts/MVC/Models/DataManager.cs): Persistent user metrics (tier, score).

- **Controllers (`Assets/Scripts/MVC/Controllers/`)**:
  - [JadeCuttingGame.cs](file:///C:/Users/User/Documents/GitHub/unity.deonphizzle.hands-of-the-mystical-saw/Assets/Scripts/JadeCuttingGame.cs): Core orchestrator managing the primary game loop, timers, win/loss conditions, and game state.
  - [ToolManager.cs](file:///C:/Users/User/Documents/GitHub/unity.deonphizzle.hands-of-the-mystical-saw/Assets/Scripts/Tool/ToolManager.cs) & `ToolSwitcher`: Coordinate tool activation, mechanical limits, and tool-specific configurations.
  - [StoneSpinController.cs](file:///C:/Users/User/Documents/GitHub/unity.deonphizzle.hands-of-the-mystical-saw/Assets/Scripts/Stone/StoneSpinController.cs): Drives the stone's movement patterns (Oscillation, Linear, Circular, Chaotic) based on its blueprint.

- **Views (`Assets/Scripts/MVC/Views/`)**:
  - UI controllers like `CanvasManager`, `ButtonGroupManager`, and `StoneItemUI` update text fields, strike indicators, and handle pointer triggers.

---

## 🛠 Tool & Rigging Specifications

### 1. Rigged Chisel Mechanism (`ManualChiselController.cs`)
- **Bone Hierarchy**:
  - **Root Bone**: Manages horizontal rotation (Joystick X) and pitch/tilt (via UI Buttons).
  - **Tilt Bone**: Handles vertical pivot adjustments (Joystick Y).
  - **Extend Bone**: Performs linear extension (piston action) to execute a strike.
  - **Chisel Tip**: Dedicated Transform at the physical tip for linecast tracking.
- **Physics & Collision**:
  - Continuous `Physics.RaycastAll` / `Linecast` is performed every frame of the extension stroke between the tip's previous and current position to prevent tunneling.
  - Self-collision is avoided by validating against tool child components.
  - Movement halts immediately upon hit registration.

### 2. Rigged Dremel/Dramel Mechanism (`DremelToolController.cs`)
- **Bone Hierarchy**:
  - **Root Bone**: Handles side-to-side rotation (Joystick X).
  - **Tilt Bone**: Handles vertical pivoting (Joystick Y).
  - **Extend Bone**: Handles manual translation forward/backward along the strike axis via UI buttons.
- **Grinding Logic**:
  - Uses continuous linecast checks.
  - Once contact is established, a grinding coroutine damages targets at regular `0.15s` intervals.

### 3. Saw Cutting (`SawController.cs` & `SliceController.cs`)
- **Framework**: Powered by `EzySlice`.
- Performs physical cutting of raw stone hulls to separate shell meshes and reveal underlying jade structures.

### 4. Torch/Inspection (`SimpleTorch.cs`)
- Swaps stone renderers to use custom X-Ray/Wet shaders (e.g., `WetXRayShader`).
- Pauses stone physics/movement and updates interactive chances when active.

---

## 📝 Coding Standards & Rules

1. **Aiming Mechanics**: Implement *Incremental Aiming* rather than direct absolute mapping for joysticks to prevent snapping when toggling inputs.
2. **Deadzones & Bounds**: Always enforce structural deadzones (e.g., `0.05f`) and clamp physical bone rotations to within mechanical limits.
3. **Collision Checks**: Always use continuous linecast sweeps (frame-to-frame position delta checks) for rapid-extension tools to avoid clipping issues.
4. **State Management**: Sync UI indicators with the active controller state (e.g., updating the `StrikeSystem` counters, locking tool action when the torch is active via `GlobalTorchActive`).

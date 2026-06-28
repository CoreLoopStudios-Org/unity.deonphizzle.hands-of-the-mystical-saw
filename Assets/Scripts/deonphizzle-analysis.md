# Project Analysis: unity.coremechanism.deonphizzle

## Overview
This project is a Unity-based jade stone cutting game. It features a sophisticated stone generation system, various tools for interaction, and a meta-game layer including profiles, leaderboards, and a marketplace. The project follows an MVC-like architecture to separate data from game logic and UI.

## Architecture
The project is organized into several key modules:

### 1. Model-View-Controller (MVC)
Located in `Assets/Scripts/MVC/`, this structure handles the separation of concerns:
- **Models:**
  - `CurrentStoneModel.cs`: A ScriptableObject that stores the state of the active stone, integrating with an external `StoneCutter.Sdk`.
  - `StoneBlueprint.cs`: Defines the data structure for a stone, including physics, material, rotation patterns, and jade core details.
  - `StoneServer.cs`: A Singleton that persists game state and generated stone lists across scenes.
  - `DataManager.cs`: Manages global player data like points, tier, and username.
- **Controllers:**
  - `LeaderboardManager.cs`: Manages fetching and displaying leaderboard data.
  - `ProfileManager.cs`: Handles player profile updates and UI.
  - `ToolManager.cs`: Orchestrates tool selection and success chance calculations.
- **Views:**
  - Various scripts like `CanvasManager`, `ButtonGroupManager`, and `StoneItemUI` handle UI updates and user input.

### 2. Core Gameplay Mechanics
- **Stone Generation (`StoneGenerator.cs`):** Dynamically spawns stones based on `StoneBlueprint`. It places "anchors" (targets) and a hidden "jade core" inside the stone.
- **Stone Movement (`StoneSpinController.cs`):** Implements complex movement patterns such as Oscillation, Linear, Circular, and Chaotic. It also handles the "Torch" mechanic.
- **Tool System (`ToolController.cs`, `ToolManager.cs`):** Supports multiple tools (Hammer, Saw, Chisel). Tools move toward the mouse position and interact with the stone via Raycasts and Collisions.
- **Cutting & Slicing:** Uses `EzySlice` for mesh slicing (in `ToolController.cs`) and handles stone "shattering" in `StoneGenerator.cs` to reveal the jade core.
- **Scoring & Strikes:** `JadeCuttingGame.cs` and `StrikeSystem.cs` manage the game loop, timing, strikes (failures), and point rewards.

### 3. Torch & Inspection
- **Torch Mechanic:** Allows players to see inside the stone using an X-ray material. This is managed by `TorchManager.cs`, `SimpleTorch.cs`, and integrated into `StoneSpinController.cs`.
- **Game State Interaction:** Using the torch often freezes the stone's spin and increases the success chance of tool actions but may consume "Torch Uses" or decay the score.

### 4. Meta-Game & UI
- **Marketplace (`StoneMarketManager.cs`):** Allows players to select different stones to cut.
- **Shop (`ShopManager.cs`):** Potentially handles purchasing tools or upgrades.
- **Progression:** Players earn points, move up tiers (Silver, Gold, Diamond), and track stats like "Perfect Cuts" vs "Failures".

## Key Scripts and Their Roles
| Script | Role |
| :--- | :--- |
| `JadeCuttingGame.cs` | Central game loop, timer, and score manager. |
| `StoneGenerator.cs` | Procedural stone and anchor spawning. |
| `StoneSpinController.cs` | Handles stone rotation and movement patterns. |
| `ToolController.cs` | Mouse-driven tool movement and collision logic. |
| `StrikeSystem.cs` | Manages the visual and logical state of strikes. |
| `DataManager.cs` | Global persistent data management. |
| `StoneServer.cs` | Singleton for cross-scene stone data persistence. |

## Technical Observations
- **External Dependencies:** Uses `EzySlice` for slicing and a custom `StoneCutter.Sdk`.
- **UI System:** Heavily uses `TextMeshPro` for high-quality text rendering.
- **Design Patterns:** Uses Singleton (`StoneServer`, `DataManager`), Observer (Actions in `CurrentStoneModel`), and MVC.
- **ScriptableObjects:** Used effectively for data definitions (`StoneDataSO`, `CurrentStoneModel`).

## Conclusion
The project is well-structured and scalable, with a clear separation between gameplay logic, data, and UI. The combination of procedural generation and a structured MVC approach allows for easy addition of new stones, tools, and meta-game features.

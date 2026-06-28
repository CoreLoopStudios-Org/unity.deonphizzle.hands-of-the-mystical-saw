# Project Analysis by Copilot

## Summary
This is a Unity 6 jade stone cutting game built around an MVC-style structure. The project combines procedural stone generation, tool interaction, torch-based inspection, and a meta layer with profiles, leaderboards, and a stone market.

## Core Architecture
- **Models:** `DataManager`, `StoneServer`, `CurrentStoneModel`, `StoneBlueprint`, `StoneChallengeData`
- **Controllers:** `JadeCuttingGame`, `ToolManager`, `StoneMarketManager`, `ModeSelectionManager`, `MainMenuController`
- **Views:** `StoneItemUI`, `UIThemeApplier`, `GlobalPointsUI`, `LeaderboardItemUI`, `PlayerProfileUI`

## Main Gameplay Flow
1. Main menu opens.
2. Player browses market / predictor / profile / leaderboard.
3. Selecting a stone routes to either `StoneGenerator Scene` or `StoneCuttingScene_Classic`.
4. Gameplay uses stone generation, tool selection, torch inspection, strikes, and victory/loss panels.

## Important Systems
- **Theme system:** `GameModeManager` stores `Classic` / `Modern` in `PlayerPrefs` and updates UI via `UIThemeApplier`.
- **Persistent state:** `DataManager` stores points and tier; `StoneServer` stores generated stones and chosen mode.
- **Stone generation:** `StoneGenerator` spawns the stone, jade core, and anchors from blueprint data.
- **Predictor mode:** `PredictorUIManager` builds `StoneChallengeData` and injects it into generated stones.
- **Tool system:** `ToolManager` activates hammer, saw, or chisel controllers.
- **Torch/inspection:** `StoneSpinController` handles torch state, spin, and commit freeze logic.

## Scene Map
- `MainMenu.unity`
- `PredictorScene.unity`
- `StoneViewerScene.unity`
- `StoneGenerator Scene.unity`
- `StoneCuttingScene_Classic.unity`

## Notes
- Scene loading is duplicated across several scripts, so changes to route names should be kept in sync.
- Theme switching is already wired through multiple UI entry points, so it should stay centralized in `GameModeManager`.
- `Assets/Scripts/AGENTS.md` contains the project conventions and should be followed for future edits.

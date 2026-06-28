# Setting Mechanism Analysis & Implementation Plan

This document details the analysis of sound, music, and gyroscope (tilt) systems in the project, and outlines the implementation plan for the settings panel toggles in `SettingPanel-Modern` and `SettingPanel-Classic`.

---

## 1. System Analysis

### 1.1 Gyroscope (Tilt Control)
- **Mechanic:** The game uses accelerometer-based tilt controls to steer/aim tools (chisels, saws, hammers, dremels).
- **Core Controller:** All tools fetch tilt input through the static class `GyroCalibration` using `GyroCalibration.GetCalibratedAcceleration()`.
- **Active Tools using Gyro:**
  - `ClassicChiselController.cs`
  - `ManualChiselController.cs`
  - `DremelControlle.cs` (Dremel)
  - `HammerController.cs` (in `HitController.cs`)
  - `NewHammerController.cs`
  - `ClassicSawController.cs`
  - `SawArmController.cs`
  - `SawToolController.cs`
  - `ToolController.cs`
- **Centralized Solution:** Since all tools retrieve tilt input via `GyroCalibration.GetCalibratedAcceleration()`, we can centralize the Gyro On/Off functionality directly inside `GyroCalibration`. If `PlayerPrefs.GetInt("GyroEnabled", 1) == 0`, we return `Vector3.zero`. This instantly and safely disables gyro input across all tools in the game without altering the physics or logic of the individual tool controllers.

### 1.2 Sound (Sound Effects / SFX)
- **Mechanic:** Sound effects are played in two ways:
  1. **One-shot sounds:** Played via `AudioSource.PlayClipAtPoint(clip, position, volume)`.
  2. **Looping/Persistent sounds:** Played via an `AudioSource` component attached to the active tool's GameObject (e.g. `DremelControlle`, saw controllers).
  3. **UI sounds:** Button click sounds played globally by `ButtonSoundManager.cs`.
- **Implementation:**
  - Check `PlayerPrefs.GetInt("SoundEnabled", 1) == 1` before calling `PlayClipAtPoint` or playing UI sounds in `ButtonSoundManager`.
  - Set `audioSource.mute = (PlayerPrefs.GetInt("SoundEnabled", 1) == 0)` in the `Update()` loop of persistent tool AudioSources so they update instantly if toggled mid-game.

### 1.3 Music (Background Music / BGM)
- **Mechanic:** Background music is managed by two persisting singleton components:
  1. `SceneBackgroundMusicManager.cs` (handles Main Menu & Predictor Scene BGM).
  2. `GameplayBackgroundMusicManager.cs` (handles classic/modern Gameplay BGM).
- **Implementation:**
  - Introduce `UpdateMusicStatus()` in both managers to set their `AudioSource.mute = (PlayerPrefs.GetInt("MusicEnabled", 1) == 0)`.
  - Call `UpdateMusicStatus()` when the setting is toggled, on start, and when new scenes are loaded. This mutes/unmutes the BGM instantly while maintaining the track play time/fade state.

---

## 2. Settings Panels UI Hierarchy
From `MainMenu.unity` and `Screenshot_53.png`, both settings panels (`SettingPanel-Classic` and `SettingPanel-Modern`) follow this consistent Toggle UI pattern:

```text
SettingPanel-Modern / SettingPanel-Classic
└── SettingMenue
    └── Settingobject
        ├── GyroModeOnOff (Toggle component)
        │   └── Background
        │       ├── OFF (GameObject - inactive state visual)
        │       └── ON (GameObject - active state visual)
        ├── SoundOnOff (Toggle component)
        │   └── Background
        │       ├── OFF (GameObject - inactive state visual)
        │       └── ON (GameObject - active state visual)
        └── MusicOnOff (Toggle component)
            └── Background
                ├── OFF (GameObject - inactive state visual)
                └── OFF (1) / ON (GameObject - active state visual)
```

---

## 3. Step-by-Step Implementation Plan

### Step 1: Update Settings Panel Controller
We will update `SettingsPanelController.cs` to manage the UI toggle components and their ON/OFF children. The script will automatically discover the references on `OnEnable()` using a robust fallback search, and handle events.

### Step 2: Centralize Gyro toggle in `GyroCalibration.cs`
We will modify `GyroCalibration.GetCalibratedAcceleration()` to check `PlayerPrefs.GetInt("GyroEnabled", 1) == 1`.

### Step 3: Implement Sound toggle in SFX Scripts
- Update the `PlayClipAtPoint` calls in `ChiselController.cs`, `ClassicChiselController.cs`, `ManualChiselController.cs`, and `NewHammerController.cs`.
- Update the `Update()` loops of `DremelControlle.cs`, `ClassicSawController.cs`, `SawArmController.cs`, and `SawToolController.cs` to mute/unmute their AudioSources.
- Update `ButtonSoundManager.cs` to respect `SoundEnabled`.

### Step 4: Implement Music toggle in BGM Managers
- Update `SceneBackgroundMusicManager.cs` and `GameplayBackgroundMusicManager.cs` to respect `MusicEnabled` and support runtime updating.

---

## 5. Implementation Status: COMPLETE

All parts of the implementation plan have been completed and are active:
- **Toggles Integration:** [SettingsPanelController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/MVC/Controllers/SettingsPanelController.cs) handles binding, listeners, state initialization, and visual toggling for `ON`/`OFF` sub-objects.
- **Centralized Gyro Control:** [GyroCalibration.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Tool/GyroCalibration.cs) returns `Vector3.zero` when `GyroEnabled` preference is disabled.
- **Sound Mute Integration:** SFX scripts check `SoundEnabled` before playing sounds or update their AudioSource mute status at runtime.
- **Music Mute Integration:** Both background music managers update their music status dynamically at runtime using `.mute` adjustments based on the `MusicEnabled` preference.

# Dremel Hitting and Sound Effects Mechanism Analysis

This document details the analysis of how the Dremel hits the stone and plays sound effects in the classic scene, and outlines how the identical mechanism is applied to the modern Dremel in the generator scene.

---

## 1. Classic Scene Analysis (`DramelController-classic` & `Dramel_rigged-Classic`)

### A. Hitting / Strike Mechanism
* **Trigger**: The click-to-strike check in `Update()` runs when the tool is equipped. If the user clicks/taps on a valid target (tagged `"Stone"` or `"Jade"`, or containing `StoneGenerator` / `HitAnchor` components) and the pointer is not over a UI element:
  - It triggers the automatic strike by calling `StartGrinding()`.
* **Bone Translation**:
  - `StartGrinding()` initiates the `GrindingRoutine()` coroutine.
  - The `extendBone` is translated forward from its initial local position along the `strikeAxis` (aligned to local X: `(1, 0, 0)`) by `autoStrikeDistance` (5 units) using `Vector3.MoveTowards` at `approachSpeed`.
* **Continuous Collision Check**:
  - Every frame during the extension, a raycast (`Physics.RaycastAll`) is performed from the previous frame's tip position to the current frame's tip position to detect contact.
* **Grinding loop**:
  - If a hit is detected, the extension stops and a loop runs for `grindDuration` (1.0 second).
  - Every frame during this duration, sparks (`hitEffectPrefab` / `Sparks yellow`) are instantiated at the hit point, and a dent (`dentPrefab` / `SawControllerManager`) is spawned on the stone.
  - Hits are registered on the stone generator via `RegisterToolStrike()` or `AnchorDestroyed()` (for anchors) to break off fragments.
* **Retraction**:
  - After 1.0 second, the Dremel retracts to its initial position at `returnSpeed`.

### B. Sound Effects Mechanism
* **Audio Source**: An `AudioSource` component is attached to the `DramelController-classic` GameObject.
* **Drilling Loop Sound**:
  - During the 1.0-second hit/grind duration, the flag `isGrindingThisFrame` is set to `true`.
  - In `LateUpdate()`, the controller checks if the tool is equipped and `isGrindingThisFrame` is true:
    - If the `AudioSource` is not playing or has a different clip, it assigns `primaryHitSound` (`Drilling-SSound.wav`), sets `loop = true`, and plays it.
  - As soon as the grinding loop finishes and retraction begins, `isGrindingThisFrame` becomes `false`, and the `AudioSource` is stopped immediately.

---

## 2. Application to Modern Scene (`DramelController-modern` & `Dramel_rigged-modern`)

To ensure the modern Dremel behaves identically to the classic Dremel in `StoneGenerator Scene.unity`, we configure it to use the exact same script and settings:

### A. Component and Script Sharing
- The `DremelToolController.cs` script is shared between both classic and modern game controllers. Because the click-to-strike, raycast hitting, and audio playback logic is coded inside the script, both tools inherit this mechanism automatically.

### B. Scene Parameter Verification (Modern Scene)
- **GameObject**: `DramelController-modern` in `StoneGenerator Scene.unity`.
- **Strike Axis**: Set to `{x: 1, y: 0, z: 0}` (Local X-axis, matching the physical extension axis of the rigged model).
- **Sound Clip**: `primaryHitSound` is correctly mapped to `Drilling-SSound.wav` (GUID `4db7c41aec19bec4e90eff9bb1b7dd76`).
- **Visual Effects**: `hitEffectPrefab` is mapped to `Sparks yellow` (GUID `9b21333b068a5084ba09535839bee3c8`) and `dentPrefab` is mapped to `SawControllerManager` (GUID `ebfc857ccbd0fcd4b9a7086f7a46ea6f`).
- **UI Button Toggling**:
  - Emptied `SetupButtonListeners()` inside `DremelToolController` on compile so the manual buttons do not move the tool.
  - Dynamic showing/hiding is handled via the script's `EquipDremel()` and `EquipSaw()` methods. When the Dremel is equipped, it disables the `Forward-Backward` UI panel. When the Saw is equipped, it re-enables it.

---

## 3. Implementation Verification Steps
1. Verify that `DramelController-modern` in the generator scene compiles cleanly.
2. Confirm the references in the Unity Inspector for `DramelController-modern`.
3. Confirm that clicking the stone in the generator scene triggers the grinding strike and plays the drilling sound loop during contact.

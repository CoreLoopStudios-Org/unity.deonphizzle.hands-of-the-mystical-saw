# Saw Movement Mechanism

This document describes the mechanical and mathematical calculations used to control the movements of the **Saw Arm Assembly** using the joystick and button inputs in [SawArmController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/Saw/SawArmController.cs).

---

## 1. Input Processing Flow

The movement mechanism processes input through a multi-stage pipeline to ensure that mechanical sweeps are smooth, predictable, and do not suffer from diagonal drift.

```mermaid
graph TD
    RawInput["Raw Joystick Input Vector (X, Y)"] --> InvertCheck{"Invert Checked?"}
    InvertCheck -->|Yes| ApplyInvert["Flip X or Y sign"]
    InvertCheck -->|No| CompareMagnitude["Compare Abs(X) vs Abs(Y)"]
    ApplyInvert --> CompareMagnitude
    
    CompareMagnitude -->|Abs(X) >= Abs(Y)| LockHorizontal["Lock to Horizontal (Set Y = 0)"]
    CompareMagnitude -->|Abs(Y) > Abs(X)| LockVertical["Lock to Vertical (Set X = 0)"]
    
    LockHorizontal --> ApplyYaw["Rotate rootBone around rootRotationAxis"]
    LockVertical --> ApplyPitch["Rotate upDownBone around tiltRotationAxis"]
```

---

## 2. Axis Locking Mechanism
To prevent the saw from drifting diagonally when aiming, the script evaluates which direction the user is pushing the joystick most aggressively:

*   **Dominant Axis Determination:**
    ```csharp
    if (Mathf.Abs(joyX) >= Mathf.Abs(joyY))
    {
        joyY = 0f; // Lock vertical movement
    }
    else
    {
        joyX = 0f; // Lock horizontal movement
    }
    ```
    This ensures that when a player drags the joystick to slide left/right, the saw moves **exclusively left/right** and will not tilt. Conversely, vertical drags result in **pure up/down tilts** without horizontal rotation.

---

## 3. Rotational Mathematics (Yaw & Pitch)

Rotations are computed in local coordinate space and applied relative to the initial starting rotations configured in the Unity Editor. This avoids gimbal lock and ensures the rig operates predictably regardless of parent orientation.

### A. Horizontal Sweep (Yaw Pivot - `rootBone`)
*   **Input**: Joystick X (`joyX`)
*   **Variable Clamping**: The angle is accumulated and clamped:
    $$\theta_{\text{root}} = \text{Clamp}(\theta_{\text{root}} + \text{joyX} \times \text{rootTurnSpeed} \times \Delta t, \ \theta_{\text{minRoot}}, \ \theta_{\text{maxRoot}})$$
*   **Quaternion Application**:
    $$\mathbf{R}_{\text{root}} = \mathbf{R}_{\text{initialRoot}} \times \mathbf{q}(\theta_{\text{root}}, \mathbf{v}_{\text{rootAxis}})$$
    *Where $\mathbf{q}(\theta, \mathbf{v})$ represents a Quaternion rotation of $\theta$ degrees around the normalized axis vector $\mathbf{v}$.*

### B. Vertical Aiming (Pitch Hinge - `upDownBone`)
*   **Input**: Joystick Y (`joyY`)
*   **Variable Clamping**: The tilt angle is accumulated (subtracted to match standard joystick behavior) and clamped:
    $$\theta_{\text{tilt}} = \text{Clamp}(\theta_{\text{tilt}} - \text{joyY} \times \text{tiltSpeed} \times \Delta t, \ \theta_{\text{minTilt}}, \ \theta_{\text{maxTilt}})$$
*   **Quaternion Application**:
    $$\mathbf{R}_{\text{tilt}} = \mathbf{R}_{\text{initialTilt}} \times \mathbf{q}(\theta_{\text{tilt}}, \mathbf{v}_{\text{tiltAxis}})$$

---

## 4. Linear Translation (Piston Extension - `extendBone`)

Manual piston movements translate the blade forward or backward along a customizable coordinate axis.

*   **Axis Definition:** The movement is defined locally by `extendAxis`.
*   **Translation Equation:**
    $$\mathbf{P}_{\text{extend}} = \mathbf{P}_{\text{initialExtend}} + (\mathbf{v}_{\text{extendAxis}} \times \text{currentExtension})$$
    *Where $\text{currentExtension}$ is clamped between $-\text{maxBackwardDistance}$ and $\text{maxForwardDistance}$.*
*   **Preemptive Collision Linecast:** Prior to moving forward, the script performs a linecast along `collisionCheckAxis` (or falls back to `extendAxis` if not defined) to prevent physical clipping into the stone layer:
    $$\mathbf{V}_{\text{worldMove}} = \text{TransformDirection}(\mathbf{v}_{\text{collisionCheckAxis}})$$
    $$\text{Linecast Range} = \mathbf{P}_{\text{blade}} \to \mathbf{P}_{\text{blade}} + \mathbf{V}_{\text{worldMove}} \times (\text{moveStep} + r_{\text{blade}})$$

---

## 5. Movement Parameters Reference

| Setting | Default Value | Role |
| :--- | :--- | :--- |
| **`rootRotationAxis`** | `(0, 1, 0)` | Vertical turntable axis. |
| **`tiltRotationAxis`** | `(0, 0, 1)` | Horizontal hinge pitch axis. |
| **`invertJoystickX`** | `true` | Matches swipe left -> saw left. |
| **`extendAxis`** | `(0, 0, 1)` | Local translation direction of the piston bone. |
| **`collisionCheckAxis`** | `(0, 0, 0)` | Linecast direction. Defaults to `extendAxis` if unset. |
| **`maxForwardDistance`** | `15` | Maximum forward slide distance. |
| **`maxBackwardDistance`** | `0` | Maximum retract distance. |

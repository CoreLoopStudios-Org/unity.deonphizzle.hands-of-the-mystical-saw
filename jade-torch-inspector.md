# 🔦 Jade Torch Inspector — Analysis & Implementation Plan

> Detailed audit of the stone and jade rendering mechanics under the torch light.
> Contains a complete implementation plan to make the inner jade core visible when the torch is ON.

---

## 🔍 Core Rendering Mechanics

In the current MVC design:
1. **Outer Stone Mesh:** A mesh is generated and assigned realistic stone materials.
2. **Inner Jade Core:** A scaled-down version (`coreScaleRatio` between `0.70` and `0.95`) of the stone mesh is spawned inside the outer stone. It is assigned a bright green jade material (`Shini jade material`).
3. **Appraisal Torch:** When the player activates the torch, the outer stone's material switches to `DryXRAY` or `WetXRAY` (which use `New Shader Graph.shadergraph`), and updates the shader property `_TorchPosition` to the hit point on the stone surface.

---

## 🛑 Why the Jade Core is Currently Invisible

Our audit found two critical reasons why the jade core remains blocked from view:

### 1. The Opaque Material constraint
- Both `DryXRAY.mat` and `WetXRAY.mat` are configured as **Opaque** (`_Surface: 0` with `_ZWrite: 1` and `_AlphaClip: 0`).
- Because they are opaque, Unity's render pipeline writes the outer stone's depth to the depth buffer first and discards all pixels rendering behind it (which includes the inner jade). Even if the shader graph tries to lower the alpha channel, it has no effect in opaque mode.

### 2. World-Space vs. Local-Space Coordinate Mismatch
- `TorchInspectionManager` and `StoneSpinController` feed world-space coordinate points into `_TorchPosition` (e.g., `x=5.2, y=1.5, z=-2.3`).
- If the shader graph calculates the distance using the vertex's **Object Space Position** instead of **World Space Position**, the distance calculation will evaluate to a large mismatch, disabling the transparency circle mask completely.

---

## 🏗️ Step-by-Step Implementation Plan

To make the inner jade core visible under the torch light, we will apply both Editor/Material fixes and code adjustments.

### Step 1 — Material Pipeline Configuration (Editor Fix)

Configure the X-Ray materials in the Unity Editor to support transparency blending:

1. Open **`DryXRAY.mat`** and **`WetXRAY.mat`** (located in `Assets/Material/StoneFirstMaterial/`) in the Inspector.
2. Under **Surface Options**:
   - Change **Surface Type** from `Opaque` to **`Transparent`**.
   - Ensure **Blending Mode** is set to **`Alpha`**.
   - Disable **Depth Write** (`_ZWrite` = 0) so transparent pixels do not cull the geometry inside.

---

### Step 2 — Shader Graph Distance Mask verification

1. Open **`New Shader Graph.shadergraph`** in the Shader Graph Editor.
2. Ensure the node calculating the distance to `_TorchPosition` uses a **World Space Position** node as the input (not Object/Local Space).
3. The distance formula inside the graph must evaluate:
   $$\text{Mask} = 1.0 - \text{Saturate}\left(\frac{\text{Distance}(\text{Position (World)}, \text{_TorchPosition})}{\text{_TorchRadius}}\right)$$
4. Feed this mask output into the shader graph's **Alpha** block.

---

### Step 3 — Dynamic Jade Glow Boosting (C# Implementation)

To make the jade core pop out vividly inside the transparent torch circle, we will dynamically boost its emission intensity when the torch is ON:

1. **Update `StoneGenerator.cs`:**
   Keep a reference to the jade core's material and add a method to change its emission:
   ```csharp
   private Material jadeMaterialInstance;

   // Inside SpawnStoneExactPredictorStyle() where newJadeMat is created:
   // Store newJadeMat into jadeMaterialInstance:
   jadeMaterialInstance = newJadeMat;
   ```
   Add a public method to control the glow state:
   ```csharp
   public void SetJadeGlow(bool active)
   {
       if (jadeMaterialInstance != null)
       {
           Color baseColor = jadeMaterialInstance.color;
           if (active)
           {
               // Boost emission intensity to shine through the x-ray mask
               jadeMaterialInstance.SetColor("_EmissionColor", baseColor * 2.5f);
           }
           else
           {
               // Restore normal subtle emission
               jadeMaterialInstance.SetColor("_EmissionColor", baseColor * 0.4f);
           }
       }
   }
   ```

2. **Trigger Glow from `TorchInspectionManager.cs`:**
   Locate the stone generator and toggle the glow state when turning the torch on or off:
   ```csharp
   private void ToggleJadeGlow(bool active)
   {
       GameObject liveStone = GameObject.FindGameObjectWithTag("Stone");
       if (liveStone != null)
       {
           StoneGenerator sg = liveStone.GetComponent<StoneGenerator>();
           if (sg != null)
           {
               sg.SetJadeGlow(active);
           }
       }
   }
   ```
   Call `ToggleJadeGlow(true)` in `TurnOnTorch()`, and `ToggleJadeGlow(false)` in `TurnOffTorch()`.

---

## ✅ Implementation Status: COMPLETE

All parts of the Jade Torch Inspector fix have been fully implemented:
1. **Material transparency patch complete:** `DryXRAY.mat`, `WetXRAY.mat`, and `StoneXRayMat.mat` have been configured as Transparent with Alpha blending (`_Surface: 1`, `_SrcBlend: 5`, `_DstBlend: 10`, `_ZWrite: 0`) and registered under `RenderType: Transparent`.
2. **C# dynamic glow boosting code complete:** Built dynamic glow controls on torch activation/deactivation.

*System Updated & Implemented — 2026-06-23*

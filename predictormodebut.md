# Predictor Mode Build Analysis & Diagnostics

This document details the analysis of why the Predictor Mode fails to work in standalone builds, along with an architectural fix.

---

## 🔍 Root Cause Analysis

After building the application, Predictor Mode fails to generate stones or load due to three critical issues:

1. **Unreachable External API Server (`http://23.26.207.43:8084`)**:
   In [PredictorController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/MVC/Controllers/PredictorController.cs), the controller is configured to communicate with a remote server:
   ```csharp
   public string apiUrl = "http://23.26.207.43:8084";
   ```
   In the built game, `useTestServer` is disabled (`false`) by default inside `PredictorScene.unity`. The build tries to send HTTP requests to this external IP. If the player lacks internet access, the port is blocked, or the remote server is offline, the connection fails.

2. **Silent Failure and Empty Data State**:
   When the connection fails, the exception is caught, but no backup or fallback logic runs. Consequently:
   * `GlobalStoneData.CurrentStone` remains `null`.
   * `GlobalStoneData.CurrentBlueprint` remains `null`.
   * The generated stone is never registered under `StoneServer.Instance.liveStonesList`.
   As a result, transitioning to the gameplay scene breaks immediately because there is no stone blueprint to generate.

3. **Redundant HTTP Request Blocks**:
   The controller contains duplicate `SaveStone` try-catch blocks that do not resolve the failure, leading to redundant timeouts and bad flow control.

---

## 🛠 Fix Implementation Plan

We can make Predictor Mode 100% robust and offline-friendly by implementing a **Local Fallback Pattern** inside [PredictorController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/MVC/Controllers/PredictorController.cs):

* Attempt connection to the remote API.
* If the API call fails or times out, catch the exception, print a warning, and **instantly fall back to local/mock stone generation**.
* This ensures that whether the user is online or offline, in build or in editor, the Predictor mode *never* breaks.

Here is the proposed refactored structure for `OnGenerateButtonClick` in [PredictorController.cs](file:///C:/Users/User/Documents/GitHub/unity.coremechanism.deonphizzle/Assets/Scripts/MVC/Controllers/PredictorController.cs):

```csharp
    public async Task OnGenerateButtonClick(
        StoneSize size, StoneDensity density, StoneStress stress, FractureTolerance fracture,
        JadeColor color, JadeQuantity quantity, StoneAnchor anchor, AdversityLevel adversity,
        float speed, float rotationAngle, RotationPattern pattern, SpinSpeed spin, int anchorPoints)
    {
        int sdkJadeCount = quantity == JadeQuantity.Single ? 1 : (quantity == JadeQuantity.Few ? 3 : 5);
        string sdkColorHex = GetHexColor(color);
        StoneSizeType sdkSize = (StoneSizeType)((int)size);
        
        int randomGeneratedSize = UnityEngine.Random.Range(minSizeLimit, maxSizeLimit + 1);
        string calculatedSizeLabel = "Medium";
        if (randomGeneratedSize >= largeThreshold) calculatedSizeLabel = "Large";
        else if (randomGeneratedSize >= mediumThreshold) calculatedSizeLabel = "Medium";
        else if (randomGeneratedSize >= smallThreshold) calculatedSizeLabel = "Small";
        
        int calculatedPoints = UnityEngine.Random.Range(4000, 7000);
        if (calculatedSizeLabel == "Small") calculatedPoints = UnityEngine.Random.Range(2000, 4000);
        else if (calculatedSizeLabel == "Large") calculatedPoints = UnityEngine.Random.Range(7000, 10001);

        StoneBlueprint blueprint = new StoneBlueprint
        {
            stone_uid = System.Guid.NewGuid().ToString(),                   
            challenge_points = calculatedPoints,       
            total_weight_kg = UnityEngine.Random.Range(5, 30),              
            stone_icon_index = UnityEngine.Random.Range(0, 4),              
            stone_size_label = calculatedSizeLabel,
            physics_and_material = new PhysicsAndMaterial { size_scale = 1.0f, density = density.ToString(), stress = stress.ToString(), fracture_tolerance = fracture.ToString() },
            rotation_system = new RotationSystem { speed = speed, rotation_angle = rotationAngle, rotation_pattern = pattern.ToString(), spin_speed = spin.ToString() },
            anchor_network = new AnchorNetwork { type = anchor.ToString(), point_count = anchorPoints },
            jade_core = new JadeCore { color_rating = sdkColorHex, quantity_mass = sdkJadeCount },
            adversity_level = adversity.ToString()
        };

        string jsonContext = JsonUtility.ToJson(blueprint);

        // If explicitly set to test server, skip API call and go straight to local generation
        if (useTestServer)
        {
            GenerateLocalMockStone(blueprint, sdkSize, sdkJadeCount, jsonContext);
            await Task.Delay(200); 
            return; 
        }

        // Try API server first
        try
        {
            Debug.Log("🌐 Attempting API connection to save generated stone...");
            StoneData savedStone = await _client.SaveStone("Generated Jade", sdkSize, sdkColorHex.Replace("#", ""), sdkJadeCount, jsonContext);
            
            GlobalStoneData.CurrentStone = savedStone;
            GlobalStoneData.CurrentBlueprint = blueprint;

            if (StoneServer.Instance != null)
            {
                StoneServer.Instance.AddNewGeneratedStone(blueprint);
            }

            if (activeStoneModel != null) activeStoneModel.UpdateStoneData(savedStone);
            await Task.Delay(200);
            return; // Success!
        }
        catch (System.Exception ex) 
        { 
            Debug.LogWarning($"❌ API Connection Failed: {ex.Message}. Falling back to Local Mock Stone Generation."); 
        }

        // Fallback: Generate mock data locally
        GenerateLocalMockStone(blueprint, sdkSize, sdkJadeCount, jsonContext);
        await Task.Delay(200);
    }

    private void GenerateLocalMockStone(StoneBlueprint blueprint, StoneSizeType sdkSize, int sdkJadeCount, string jsonContext)
    {
        Debug.Log("<color=yellow>🌐 [Local Generator]</color> Generating local mock stone data...");
        StoneData mockStone = new StoneData
        {
            Id = 999, 
            Name = "Mock Jade Matrix (Local)",
            StoneSize = sdkSize,
            JadeCount = sdkJadeCount,
            JsonContext = jsonContext
        };

        GlobalStoneData.CurrentStone = mockStone;
        GlobalStoneData.CurrentBlueprint = blueprint;

        if (StoneServer.Instance != null)
        {
            StoneServer.Instance.AddNewGeneratedStone(blueprint);
        }

        if (activeStoneModel != null) activeStoneModel.UpdateStoneData(mockStone);
    }
```

using UnityEngine;
using StoneCutter.Sdk;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public static class GlobalStoneData
{
    public static StoneData CurrentStone;
    public static StoneBlueprint CurrentBlueprint;
}

public class PredictorController : MonoBehaviour
{
    private StoneCutterClient _client;
    public string apiUrl = "http://23.26.207.43:8084"; 
    public CurrentStoneModel activeStoneModel; 

    [Header("--- Server Testing ---")]
    public bool useTestServer = true; 
    
    [Header("--- Auto Size Generation Settings ---")]
    public int minSizeLimit = 2000;        
    public int maxSizeLimit = 10000;     
    
    public int smallThreshold = 3000;     
    public int mediumThreshold = 6000;     
    public int largeThreshold = 9000;    
   

    private void Awake() => _client = new StoneCutterClient(apiUrl);
    private void OnDestroy() => _client?.Dispose();

    public async Task OnGenerateButtonClick(
        StoneSize size, StoneDensity density, StoneStress stress, FractureTolerance fracture,
        JadeColor color, JadeQuantity quantity, StoneAnchor anchor, AdversityLevel adversity,
        float speed, float rotationAngle, RotationPattern pattern, SpinSpeed spin, int anchorPoints)
    {
        int sdkJadeCount = quantity == JadeQuantity.Single ? 1 : (quantity == JadeQuantity.Few ? 3 : 5);
        string sdkColorHex = GetHexColor(color);
        StoneSizeType sdkSize = (StoneSizeType)((int)size);
        
        int randomGeneratedSize = UnityEngine.Random.Range(minSizeLimit, maxSizeLimit + 1);
        
        string calculatedSizeLabel = "Tiny"; 


        if (randomGeneratedSize >= largeThreshold)
        {
            calculatedSizeLabel = "Large";
        }
        else if (randomGeneratedSize >= mediumThreshold)
        {
            calculatedSizeLabel = "Medium";
        }
        else if (randomGeneratedSize >= smallThreshold)
        {
            calculatedSizeLabel = "Small";
        }
        
        int calculatedPoints = 2000; 

        if (calculatedSizeLabel == "Small" || calculatedSizeLabel == "Tiny")
        {
            calculatedPoints = UnityEngine.Random.Range(2000, 4000);
        }
        else if (calculatedSizeLabel == "Medium")
        {
            calculatedPoints = UnityEngine.Random.Range(4000, 7000);
        }
        else if (calculatedSizeLabel == "Large")
        {
            calculatedPoints = UnityEngine.Random.Range(7000, 10001); 
        }
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

        if (useTestServer)
        {
            GenerateLocalMockStone(blueprint, sdkSize, sdkJadeCount, jsonContext);
            await Task.Delay(200); 
            return; 
        }

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
            return;
        }
        catch (System.Exception ex) 
        { 
            Debug.LogWarning($"❌ API Connection Failed: {ex.Message}. Falling back to Local Mock Stone Generation."); 
        }

        // Fallback: Generate mock data locally if connection fails
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

    private string GetHexColor(JadeColor color)
    {
        return color switch {
            JadeColor.PaleGreen => "98FB98",
            JadeColor.DeepGreen => "006400",
            JadeColor.Emerald => "50C878",
            JadeColor.Imperial => "1C542D",
            _ => "FFFFFF"
        };
    }
}
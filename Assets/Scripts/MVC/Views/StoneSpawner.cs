using UnityEngine;
using System.Collections.Generic;
using StoneCutter.Sdk;

public class StoneSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint; 
    public GameObject stonePrefab; 

    [Header("--- Realistic Stone Looks ---")]
    public Material[] realisticStoneMaterials; 

    [Header("--- Realistic Jade Looks ---")]
    public Material jadeBaseMaterial;

    [Header("--- Surface Details (Anchors) ---")]
    public GameObject anchorPrefab; 
    // 🛑 Crack Material Removed

    private GameObject currentStoneInstance;
    private GameObject currentJadeCore; 

    private void Start()
    {
        SpawnStone();
    }

    void SpawnStone()
    {
        if (GlobalStoneData.CurrentStone == null || GlobalStoneData.CurrentBlueprint == null)
        {
            Debug.LogError("❌ No Stone Data found!");
            return;
        }

        if (currentStoneInstance != null) Destroy(currentStoneInstance);
        if (currentJadeCore != null) Destroy(currentJadeCore);

        // 1. Main stone spawn
        currentStoneInstance = Instantiate(stonePrefab, spawnPoint.position, spawnPoint.rotation);
        
        // The size of the stone
        float scaleMultiplier = GlobalStoneData.CurrentStone.StoneSize switch { StoneSizeType.Small => 0.5f, StoneSizeType.Large => 1.0f, _ => 0.75f };
        currentStoneInstance.transform.localScale = Vector3.one * scaleMultiplier;

        ApplyRandomStoneMaterial();
        
        // 2. Rotation and Physics
        ApplyBlueprintPhysics(GlobalStoneData.CurrentBlueprint);
        
        // 3. The inner jade core
        SpawnInnerJadeCore(GlobalStoneData.CurrentStone.JadeCount, GlobalStoneData.CurrentBlueprint.jade_core.color_rating);

        // 4. Anchor Spawn Only (Cracks Removed)
        int anchorCount = GlobalStoneData.CurrentBlueprint.anchor_network?.point_count ?? 4;
        SpawnAnchorsSimple(anchorCount);
        
        Debug.Log("<color=cyan>✅ 3D Stone Spawned! Anchors are inside the stone (Cracks removed).</color>");
    }

    private void SpawnInnerJadeCore(int jadeCount, string hexColor)
    {
        ColorUtility.TryParseHtmlString("#" + hexColor.Replace("#", ""), out Color jadeColor);

        currentJadeCore = Instantiate(stonePrefab, currentStoneInstance.transform.position, currentStoneInstance.transform.rotation);
        currentJadeCore.transform.SetParent(currentStoneInstance.transform);

        float coreScaleRatio = jadeCount >= 5 ? 0.95f : (jadeCount >= 3 ? 0.85f : 0.70f);
        currentJadeCore.transform.localScale = Vector3.one * coreScaleRatio;

        Renderer jadeRenderer = currentJadeCore.GetComponentInChildren<Renderer>();
        if (jadeRenderer != null && jadeBaseMaterial != null) 
        {
            Material newJadeMat = new Material(jadeBaseMaterial);
            newJadeMat.color = jadeColor;
            newJadeMat.EnableKeyword("_EMISSION");
            newJadeMat.SetColor("_EmissionColor", jadeColor * 0.4f);
            jadeRenderer.material = newJadeMat;
        }
        
        // All physics in inner Z are deleted
        foreach (Collider col in currentJadeCore.GetComponentsInChildren<Collider>()) Destroy(col);
        if (currentJadeCore.GetComponent<Rigidbody>() != null) Destroy(currentJadeCore.GetComponent<Rigidbody>());
        if (currentJadeCore.GetComponent<StoneRotator>() != null) Destroy(currentJadeCore.GetComponent<StoneRotator>());
    }

    private void ApplyRandomStoneMaterial()
    {
        if (realisticStoneMaterials != null && realisticStoneMaterials.Length > 0)
        {
            Renderer stoneRenderer = currentStoneInstance.GetComponentInChildren<Renderer>();
            if (stoneRenderer != null)
            {
                stoneRenderer.material = realisticStoneMaterials[Random.Range(0, realisticStoneMaterials.Length)];
            }
        }
    }

    private void ApplyBlueprintPhysics(StoneBlueprint bp)
    {
        Rigidbody rb = currentStoneInstance.GetComponent<Rigidbody>();
        if (rb == null) rb = currentStoneInstance.AddComponent<Rigidbody>();

        rb.mass = bp.physics_and_material.density == "Heavy" ? 50f : (bp.physics_and_material.density == "Light" ? 5f : 20f);
        rb.useGravity = false; 
        rb.isKinematic = true; 

        if (bp.rotation_system.speed > 0)
        {
            StoneRotator rotator = currentStoneInstance.GetComponent<StoneRotator>();
            if (rotator == null) rotator = currentStoneInstance.AddComponent<StoneRotator>();
            
            rotator.speed = bp.rotation_system.speed;
            if (bp.rotation_system.spin_speed == "Fast") rotator.speed *= 2f;
            rotator.direction = bp.rotation_system.rotation_pattern == "LeftToRight" ? Vector3.up : Vector3.down;
        }
    }

    // ==========================================
    // 🌟 Simple Math Logic (Without Raycast - Inside the stone)
    // ==========================================

    private void SpawnAnchorsSimple(int count)
    {
        if (anchorPrefab == null) return;

        Renderer rend = currentStoneInstance.GetComponentInChildren<Renderer>();
        
        // Radius that the anchors are built into the rock
        float radius = rend != null ? Mathf.Min(rend.bounds.extents.x, rend.bounds.extents.y, rend.bounds.extents.z) * 0.6f : 0.3f;
        Vector3 center = rend != null ? rend.bounds.center : currentStoneInstance.transform.position;

        for (int i = 0; i < count; i++)
        {
            Vector3 randomDir = Random.onUnitSphere; 
            Vector3 spawnPos = center + randomDir * radius;

            GameObject anchor = Instantiate(anchorPrefab, spawnPos, Quaternion.LookRotation(randomDir));
            anchor.transform.SetParent(currentStoneInstance.transform);
        }
    }
}

// 🛑 StoneRotator class
public class StoneRotator : MonoBehaviour
{
    public float speed;
    public Vector3 direction;

    void Update()
    {
        transform.Rotate(direction * speed * Time.deltaTime);
    }
}

//WITH crack 

// using UnityEngine;
// using System.Collections.Generic;
// using StoneCutter.Sdk;
//
// public class StoneSpawner : MonoBehaviour
// {
//     [Header("Spawn Settings")]
//     public Transform spawnPoint; 
//     public GameObject stonePrefab; 
//
//     [Header("--- Realistic Stone Looks ---")]
//     public Material[] realisticStoneMaterials; 
//
//     [Header("--- Realistic Jade Looks ---")]
//     public Material jadeBaseMaterial;
//
//     [Header("--- Surface Details (Anchors & Cracks) ---")]
//     public GameObject anchorPrefab; 
//     public Material glowingCrackMaterial; 
//
//     private GameObject currentStoneInstance;
//     private GameObject currentJadeCore; 
//
//     private void Start()
//     {
//         SpawnStone();
//     }
//
//     void SpawnStone()
//     {
//         if (GlobalStoneData.CurrentStone == null || GlobalStoneData.CurrentBlueprint == null)
//         {
//             Debug.LogError("❌ No Stone Data found!");
//             return;
//         }
//
//         if (currentStoneInstance != null) Destroy(currentStoneInstance);
//         if (currentJadeCore != null) Destroy(currentJadeCore);
//
// // 1. Main stone spawn
//         currentStoneInstance = Instantiate(stonePrefab, spawnPoint.position, spawnPoint.rotation);
//         
// // 🌟 Fix 1: The size of the stone is slightly smaller than before
//         float scaleMultiplier = GlobalStoneData.CurrentStone.StoneSize switch { StoneSizeType.Small => 0.5f, StoneSizeType.Large => 1.0f, _ => 0.75f };
//         currentStoneInstance.transform.localScale = Vector3.one * scaleMultiplier;
//
//         ApplyRandomStoneMaterial();
//         
// // 2. Rotation and Physics
//         ApplyBlueprintPhysics(GlobalStoneData.CurrentBlueprint);
//         
// // 3. The inner jade core
//         SpawnInnerJadeCore(GlobalStoneData.CurrentStone.JadeCount, GlobalStoneData.CurrentBlueprint.jade_core.color_rating);
//
// // 4. Anchor and crack spawn using simple math
//         int anchorCount = GlobalStoneData.CurrentBlueprint.anchor_network?.point_count ?? 4;
//         SpawnAnchorsSimple(anchorCount);
//         
//         SpawnFracturesSimple(GlobalStoneData.CurrentBlueprint.physics_and_material.fracture_tolerance, GlobalStoneData.CurrentBlueprint.jade_core.color_rating);
//
//         Debug.Log("<color=cyan>✅ 3D Stone Spawned! Anchors and Cracks are now INSIDE the stone.</color>");
//     }
//
//     private void SpawnInnerJadeCore(int jadeCount, string hexColor)
//     {
//         ColorUtility.TryParseHtmlString("#" + hexColor.Replace("#", ""), out Color jadeColor);
//
//         currentJadeCore = Instantiate(stonePrefab, currentStoneInstance.transform.position, currentStoneInstance.transform.rotation);
//         currentJadeCore.transform.SetParent(currentStoneInstance.transform);
//
//         float coreScaleRatio = jadeCount >= 5 ? 0.95f : (jadeCount >= 3 ? 0.85f : 0.70f);
//         currentJadeCore.transform.localScale = Vector3.one * coreScaleRatio;
//
//         Renderer jadeRenderer = currentJadeCore.GetComponentInChildren<Renderer>();
//         if (jadeRenderer != null && jadeBaseMaterial != null) 
//         {
//             Material newJadeMat = new Material(jadeBaseMaterial);
//             newJadeMat.color = jadeColor;
//             newJadeMat.EnableKeyword("_EMISSION");
//             newJadeMat.SetColor("_EmissionColor", jadeColor * 0.4f);
//             jadeRenderer.material = newJadeMat;
//         }
//         
// // Deleted all physics of the inner jade
//         foreach (Collider col in currentJadeCore.GetComponentsInChildren<Collider>()) Destroy(col);
//         if (currentJadeCore.GetComponent<Rigidbody>() != null) Destroy(currentJadeCore.GetComponent<Rigidbody>());
//         if (currentJadeCore.GetComponent<StoneRotator>() != null) Destroy(currentJadeCore.GetComponent<StoneRotator>());
//     }
//
//     private void ApplyRandomStoneMaterial()
//     {
//         if (realisticStoneMaterials != null && realisticStoneMaterials.Length > 0)
//         {
//             Renderer stoneRenderer = currentStoneInstance.GetComponentInChildren<Renderer>();
//             if (stoneRenderer != null)
//             {
//                 stoneRenderer.material = realisticStoneMaterials[Random.Range(0, realisticStoneMaterials.Length)];
//             }
//         }
//     }
//
//     private void ApplyBlueprintPhysics(StoneBlueprint bp)
//     {
//         Rigidbody rb = currentStoneInstance.GetComponent<Rigidbody>();
//         if (rb == null) rb = currentStoneInstance.AddComponent<Rigidbody>();
//
//         rb.mass = bp.physics_and_material.density == "Heavy" ? 50f : (bp.physics_and_material.density == "Light" ? 5f : 20f);
//         rb.useGravity = false; 
//         rb.isKinematic = true; 
//
//         if (bp.rotation_system.speed > 0)
//         {
//             StoneRotator rotator = currentStoneInstance.GetComponent<StoneRotator>();
//             if (rotator == null) rotator = currentStoneInstance.AddComponent<StoneRotator>();
//             
//             rotator.speed = bp.rotation_system.speed;
//             if (bp.rotation_system.spin_speed == "Fast") rotator.speed *= 2f;
//             rotator.direction = bp.rotation_system.rotation_pattern == "LeftToRight" ? Vector3.up : Vector3.down;
//         }
//     }
//
//     // ==========================================
// // 🌟 Simple Math Logic (Without Raycast - Inside the stone)
//     // ==========================================
//
//     private void SpawnAnchorsSimple(int count)
//     {
//         if (anchorPrefab == null) return;
//
//         Renderer rend = currentStoneInstance.GetComponentInChildren<Renderer>();
//         
// // 🌟 Fix 2: Radius reduced a lot (from 1.05f to 0.6f) so anchors are built inside rocks
//         float radius = rend != null ? Mathf.Min(rend.bounds.extents.x, rend.bounds.extents.y, rend.bounds.extents.z) * 0.6f : 0.3f;
//         Vector3 center = rend != null ? rend.bounds.center : currentStoneInstance.transform.position;
//
//         for (int i = 0; i < count; i++)
//         {
//             Vector3 randomDir = Random.onUnitSphere; 
//             Vector3 spawnPos = center + randomDir * radius;
//
//             GameObject anchor = Instantiate(anchorPrefab, spawnPos, Quaternion.LookRotation(randomDir));
//             anchor.transform.SetParent(currentStoneInstance.transform);
//         }
//     }
//
//     private void SpawnFracturesSimple(string tolerance, string hexColor)
//     {
//         if (glowingCrackMaterial == null) return;
//
//         int crackCount = tolerance == "Fragile" ? 5 : (tolerance == "Normal" ? 3 : 1);
//         ColorUtility.TryParseHtmlString("#" + hexColor.Replace("#", ""), out Color crackColor);
//
//         Renderer rend = currentStoneInstance.GetComponentInChildren<Renderer>();
//         
// // 🌟 Fix 3: Cracks are also inserted into the stone (Radius * 0.65f)
//         float radius = rend != null ? Mathf.Min(rend.bounds.extents.x, rend.bounds.extents.y, rend.bounds.extents.z) * 0.65f : 0.35f;
//
//         for (int i = 0; i < crackCount; i++)
//         {
//             GameObject crackObj = new GameObject($"CrackLine_{i}");
//             crackObj.transform.SetParent(currentStoneInstance.transform);
//             crackObj.transform.localPosition = Vector3.zero;
//
//             LineRenderer lr = crackObj.AddComponent<LineRenderer>();
//             Material instancedMat = new Material(glowingCrackMaterial);
//             instancedMat.SetColor("_EmissionColor", crackColor * 3.0f); 
//             lr.material = instancedMat;
//             
//             lr.startWidth = 0.05f; 
//             lr.endWidth = 0.01f;
//             lr.useWorldSpace = false;
//
//             int pointsCount = 10;
//             lr.positionCount = pointsCount;
//             Vector3 startDir = Random.onUnitSphere;
//             
//             for (int p = 0; p < pointsCount; p++)
//             {
//                 Vector3 currentDir = Vector3.Slerp(startDir, Random.onUnitSphere, p * 0.15f);
//                 currentDir += Random.insideUnitSphere * 0.15f; 
//                 
//                 lr.SetPosition(p, currentDir.normalized * radius);
//             }
//         }
//     }
// }
//
// // 🛑 StoneRotator class
// public class StoneRotator : MonoBehaviour
// {
//     public float speed;
//     public Vector3 direction;
//
//     void Update()
//     {
//         transform.Rotate(direction * speed * Time.deltaTime);
//     }
// }
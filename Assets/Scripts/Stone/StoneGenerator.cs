using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using StoneCutter.Sdk; 

public class StoneGenerator : MonoBehaviour
{
    [Header("--- Data Source (MVC Architecture) ---")]
    public CurrentStoneModel currentStoneModel; 
    [HideInInspector] public StoneChallengeData predictorData;
    [HideInInspector] public bool isPredictorMode = false;

    [Header("--- Stone Size Control ---")]
    [Range(0.1f, 5f)] 
    public float customSizeMultiplier = 1f; 

    [Header("--- Exact Predictor Spawn Settings ---")]
    public GameObject stonePrefab; 
    public Material[] realisticStoneMaterials; 
    public Material jadeBaseMaterial; 
    
    [Header("--- Anchor Prefabs & Settings ---")]
    public GameObject primaryAnchorPrefab;   // Will be used for the first anchor
    public GameObject secondaryAnchorPrefab; // Will be used for all other anchors
    
    [Tooltip("Adjust the size of the spawned anchors.")]
    [Range(0.1f, 5f)]
    public float anchorSizeMultiplier = 1f;  // 🌟 NEW: Control anchor size from inspector

    [Header("--- Cutting Game Rules ---")]
    public int maxStrikes = 3; 
    private int currentStrikes = 0;
    private int totalAnchors = 0;
    private int destroyedAnchors = 0;
        
    [Header("--- Victory/Loss Elements ---")]
    public GameObject bigExplosionPrefab; 
    public TextMeshProUGUI strikeText; 
    public GameObject victoryPanel; 
    public GameObject gameOverPanel; 

    [Header("--- Strike Panel Icons (3-StrikeSelectionPanel-Classic) ---")]
    public GameObject strikeIcon1;  // FirstStrikeComplete GameObject
    public GameObject strikeIcon2;  // SecondStrikeComplete GameObject
    public GameObject strikeIcon3;  // ThirdStrikeComplete GameObject 
    
    [Header("--- Reward UI ---")]
    public TextMeshProUGUI earnedPointsText; 

    private GameObject currentJadeCore; 
    private Material jadeMaterialInstance; 
    private bool isGameOver = false;
    private bool readyForFinalHit = false; 

    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;

    void Awake()
    {
        if (stonePrefab == null || primaryAnchorPrefab == null || secondaryAnchorPrefab == null || jadeBaseMaterial == null) return;
        
        SpawnStoneExactPredictorStyle();
        
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;
    }

    void Start()
    {
        UpdateStrikeUI(); 
        ResetStrikeIcons();
        if(victoryPanel != null) victoryPanel.SetActive(false);
        if(gameOverPanel != null) gameOverPanel.SetActive(false);

        LoadPredictorData();
        PassDataToSpinController();
    }

    void Update()
    {
        if (!isGameOver)
        {
            if (!readyForFinalHit && currentJadeCore != null)
            {
                if (GetComponentsInChildren<HitAnchor>().Length == 0)
                {
                    readyForFinalHit = true;
                    if(strikeText != null) strikeText.text = "FINAL STEP: USE CHISEL!";
                }
            }
        }
    }

    void LoadPredictorData()
    {
        StoneBlueprint bp = GlobalStoneData.CurrentBlueprint;
        if (bp == null && currentStoneModel != null) bp = currentStoneModel.parsedBlueprint;

        if (bp != null && bp.predictor_challenge_data != null)
        {
            var data = bp.predictor_challenge_data;
            if (data.rotationPattern != StoneChallengeData.RotationalPattern.None || data.manualSpeedSlider > 0)
            {
                predictorData = data;
                isPredictorMode = true;
                return;
            }
        }
        isPredictorMode = false;
    }

    void PassDataToSpinController()
    {
        StoneSpinController spinController = FindObjectOfType<StoneSpinController>();
        if (spinController != null)
        {
            float passedAngle = 0f;
            float passedSpeed = 0f;

            StoneBlueprint bp = GlobalStoneData.CurrentBlueprint;
            if (bp != null && bp.rotation_system != null)
            {
                passedAngle = bp.rotation_system.rotation_angle;
                passedSpeed = bp.rotation_system.speed;
            }

            spinController.ReceiveStoneData(this.transform, predictorData, isPredictorMode, passedAngle, passedSpeed);
        }
    }
    
    void SpawnStoneExactPredictorStyle()
    {
        StoneBlueprint bp = GlobalStoneData.CurrentBlueprint;
        bool hasData = bp != null;

        float baseRequestedScale = 0.015f; 
        int jadeCount = 1;
        string hexColor = "50C878"; 
        totalAnchors = 4;

        if (hasData)
        {
            baseRequestedScale = GlobalStoneData.CurrentStone.StoneSize switch { 
                StoneSizeType.Small => 0.010f, 
                StoneSizeType.Large => 0.020f, 
                _ => 0.015f 
            };
            jadeCount = GlobalStoneData.CurrentStone.JadeCount;
            hexColor = bp.jade_core.color_rating;
            totalAnchors = bp.anchor_network.point_count;
        }

        transform.localScale = Vector3.one * (baseRequestedScale * customSizeMultiplier);
        gameObject.tag = "Stone"; 
        gameObject.layer = LayerMask.NameToLayer("Stone");

        GameObject tempStone = Instantiate(stonePrefab);
        tempStone.SetActive(false); 
        
        MeshFilter prefMF = tempStone.GetComponentInChildren<MeshFilter>();
        MeshRenderer prefMR = tempStone.GetComponentInChildren<MeshRenderer>();

        if (prefMF != null && prefMR != null)
        {
            MeshFilter myMF = gameObject.GetComponent<MeshFilter>();
            if (myMF == null) myMF = gameObject.AddComponent<MeshFilter>();
            myMF.sharedMesh = prefMF.sharedMesh;

            MeshRenderer myMR = gameObject.GetComponent<MeshRenderer>();
            if (myMR == null) myMR = gameObject.AddComponent<MeshRenderer>();
            myMR.sharedMaterials = prefMR.sharedMaterials;

            MeshCollider mc = gameObject.GetComponent<MeshCollider>();
            if (mc == null) mc = gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = prefMF.sharedMesh;
            mc.convex = true;
        }
        Destroy(tempStone); 

        if (realisticStoneMaterials != null && realisticStoneMaterials.Length > 0)
        {
            Renderer stoneRenderer = GetComponent<Renderer>();
            if (stoneRenderer != null) stoneRenderer.material = realisticStoneMaterials[Random.Range(0, realisticStoneMaterials.Length)];
        }
        
        ColorUtility.TryParseHtmlString("#" + hexColor.Replace("#", ""), out Color jadeColor);
        
        currentJadeCore = Instantiate(stonePrefab, transform.position, transform.rotation);
        currentJadeCore.transform.SetParent(this.transform);
        currentJadeCore.transform.localPosition = Vector3.zero;
        currentJadeCore.transform.localRotation = Quaternion.identity;

        float coreScaleRatio = jadeCount >= 5 ? 0.95f : (jadeCount >= 3 ? 0.85f : 0.70f);
        currentJadeCore.transform.localScale = Vector3.one * coreScaleRatio;

        if (currentJadeCore.GetComponent<Rigidbody>() != null) Destroy(currentJadeCore.GetComponent<Rigidbody>());

        Transform[] allChildren = currentJadeCore.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            child.gameObject.tag = "Jade";
            child.gameObject.layer = LayerMask.NameToLayer("Stone");

            MeshFilter mf = child.GetComponent<MeshFilter>();
            if (mf != null)
            {
                MeshCollider mc = child.GetComponent<MeshCollider>();
                if (mc == null) mc = child.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = true;
                mc.isTrigger = true; 
            }
        }

        Renderer jadeRenderer = currentJadeCore.GetComponentInChildren<Renderer>();
        if (jadeRenderer != null && jadeBaseMaterial != null) 
        {
            Material newJadeMat = new Material(jadeBaseMaterial);
            newJadeMat.color = jadeColor;
            newJadeMat.EnableKeyword("_EMISSION");
            newJadeMat.SetColor("_EmissionColor", jadeColor * 0.4f);
            jadeMaterialInstance = newJadeMat;
            
            Material[] jadeMats = new Material[jadeRenderer.sharedMaterials.Length];
            for (int m = 0; m < jadeMats.Length; m++)
            {
                jadeMats[m] = newJadeMat;
            }
            jadeRenderer.materials = jadeMats;
        }

        Renderer outerRend = GetComponent<Renderer>();
        float radius = outerRend != null ? Mathf.Min(outerRend.bounds.extents.x, outerRend.bounds.extents.y, outerRend.bounds.extents.z) * 0.6f : 0.3f;
        Vector3 center = outerRend != null ? outerRend.bounds.center : transform.position;

        for (int i = 0; i < totalAnchors; i++)
        {
            Vector3 randomDir = Random.onUnitSphere; 
            Vector3 anchorPos = center + randomDir * radius;

            GameObject selectedPrefab = (i == 0) ? primaryAnchorPrefab : secondaryAnchorPrefab;

            GameObject anchor = Instantiate(selectedPrefab, anchorPos, Quaternion.LookRotation(randomDir));
            anchor.transform.SetParent(this.transform);

            // 🌟 NEW: Apply the size multiplier to the spawned anchor
            anchor.transform.localScale = Vector3.one * anchorSizeMultiplier;

            Collider anchorCol = anchor.GetComponent<Collider>();
            if (anchorCol != null) anchorCol.isTrigger = true;

            HitAnchor anchorScript = anchor.GetComponent<HitAnchor>();
            if (anchorScript == null) anchorScript = anchor.AddComponent<HitAnchor>();
            anchorScript.stoneManager = this;
            anchorScript.isPrimary = (i == 0); 
        }
    }

    void FillStrikeIcon(int strikeIndex)
    {
        GameObject icon = strikeIndex == 1 ? strikeIcon1
                        : strikeIndex == 2 ? strikeIcon2
                        : strikeIcon3;
        if (icon != null) icon.SetActive(true);
    }

    void ResetStrikeIcons()
    {
        if (strikeIcon1 != null) strikeIcon1.SetActive(false);
        if (strikeIcon2 != null) strikeIcon2.SetActive(false);
        if (strikeIcon3 != null) strikeIcon3.SetActive(false);
    }

    public void RegisterToolStrike()
    {
        if (isGameOver) return; 

        if (readyForFinalHit)
        {
            isGameOver = true; 
            currentStrikes++;
            FillStrikeIcon(currentStrikes);
            StartCoroutine(RevealJadeRoutine());
            return;
        }

        currentStrikes++;
        UpdateStrikeUI(); 
        FillStrikeIcon(currentStrikes);
        
        if (currentStrikes >= maxStrikes && !isGameOver)
        {
            isGameOver = true; 
            if (readyForFinalHit)
            {
                StartCoroutine(RevealJadeRoutine());
            }
            else
            {
                if (gameOverPanel != null) gameOverPanel.SetActive(true); 
                if (WinLoseManager.Instance != null) WinLoseManager.Instance.ShowLosePanel();
            }
        }
    }

    public void AnchorDestroyed(HitAnchor anchor)
    {
        if (isGameOver) return; 
        destroyedAnchors++;
    }

    void UpdateStrikeUI()
    {
        if(strikeText != null && !readyForFinalHit) strikeText.text = "STRIKES: " + currentStrikes;
    }

    System.Collections.IEnumerator RevealJadeRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        if (bigExplosionPrefab != null)
        {
            GameObject boom = Instantiate(bigExplosionPrefab, transform.position, Quaternion.identity);
            Destroy(boom, 2f); 
        }

        MeshRenderer outerRenderer = GetComponent<MeshRenderer>();
        Collider outerCollider = GetComponent<Collider>();
        if (outerRenderer != null) outerRenderer.enabled = false;
        if (outerCollider != null) outerCollider.enabled = false;

        if (currentJadeCore != null)
        {
            currentJadeCore.transform.SetParent(null); 
            
            Rigidbody jadeRb = currentJadeCore.GetComponent<Rigidbody>();
            if (jadeRb == null) jadeRb = currentJadeCore.AddComponent<Rigidbody>();
            
            foreach (Collider col in currentJadeCore.GetComponentsInChildren<Collider>()) 
            {
                col.isTrigger = false; 
            }
            
            jadeRb.linearDamping = 1.5f;         
            jadeRb.angularDamping = 10f;   
            
            jadeRb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
            jadeRb.AddTorque(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)), ForceMode.Impulse);
        }

        foreach(HitAnchor childAnchor in GetComponentsInChildren<HitAnchor>())
        {
            Destroy(childAnchor.gameObject);
        }

        yield return new WaitForSeconds(1.5f);

        int earnedPoints = 0;
        if (GlobalStoneData.CurrentBlueprint != null)
        {
            earnedPoints = GlobalStoneData.CurrentBlueprint.challenge_points;
        }

        if (DataManager.Instance != null)
        {
            DataManager.Instance.AddPoints(earnedPoints);
        }
        else
        {
            int currentTotalPoints = PlayerPrefs.GetInt("PlayerTotalPoints", 0);
            PlayerPrefs.SetInt("PlayerTotalPoints", currentTotalPoints + earnedPoints);
            PlayerPrefs.Save();
        }

        if (earnedPointsText != null)
        {
            earnedPointsText.text = "+ " + earnedPoints.ToString("N0") + " Points!";
        }

        if (victoryPanel != null) victoryPanel.SetActive(true);

        if (WinLoseManager.Instance != null) WinLoseManager.Instance.ShowWinPanel();
    }

    public void UseTorch()
    {
        TorchManager tm = FindAnyObjectByType<TorchManager>();
        if (tm != null) tm.ToggleTorch(true); 
    }

    public void CommitStone()
    {
        Debug.Log("Commit Button Clicked! Stone is revealing...");
    }

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
}
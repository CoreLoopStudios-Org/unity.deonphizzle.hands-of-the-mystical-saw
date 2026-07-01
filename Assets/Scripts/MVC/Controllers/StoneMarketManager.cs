using System.Collections.Generic;
using UnityEngine;

public class StoneMarketManager : MonoBehaviour
{
    [Header("Grid Setup")]
    public Transform contentPanel;     
    public GameObject stoneItemPrefab; 

    [Header("Server Dummy Data")]
    public List<StoneDataSO> availableStones;

    private void Start() // 🌟 I gave Start instead of OnEnable
    {
        Debug.Log("🚀 StoneMarketManager is starting...");
        GenerateMarketCards();
    }

    public void GenerateMarketCards()
    {
        if (stoneItemPrefab == null) 
        {
            Debug.LogError("❌ Prefab is missing in Inspector!");
            return;
        }
        if (contentPanel == null)
        {
            Debug.LogError("❌ Content Panel is missing in Inspector!");
            return;
        }

        // Delete previous cards
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        int totalSpawned = 0;

        // 1. Spawning Dummy Stones (from Scriptable Objects)
        if (availableStones != null && availableStones.Count > 0)
        {
            foreach (var stoneSO in availableStones)
            {
                if (stoneSO == null) continue; 

                GameObject newCard = Instantiate(stoneItemPrefab, contentPanel);
                StoneItemUI uiScript = newCard.GetComponent<StoneItemUI>();

                if (uiScript != null)
                {
                    uiScript.Setup(stoneSO);
                    totalSpawned++;
                }
            }
        }

        // 2. 🌟 Live stone spawning (Local Server data from Predictor Mode)
        if (StoneServer.Instance != null && StoneServer.Instance.liveStonesList.Count > 0)
        {
            foreach (var liveStone in StoneServer.Instance.liveStonesList)
            {
                if (liveStone == null) continue;

                GameObject newCard = Instantiate(stoneItemPrefab, contentPanel);
                StoneItemUI uiScript = newCard.GetComponent<StoneItemUI>();

                if (uiScript != null)
                {
                    // New function to send live blueprint data
                    uiScript.SetupLiveStone(liveStone); 
                    totalSpawned++;
                }
            }
        }

        Debug.Log($"✅ Market Generation Complete! Total Stones Spawned: {totalSpawned}");
    }
}
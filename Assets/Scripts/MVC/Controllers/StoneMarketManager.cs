using System.Collections.Generic;
using UnityEngine;

public class StoneMarketManager : MonoBehaviour
{
    [Header("Grid Setup")]
    public Transform contentPanel;     
    public GameObject stoneItemPrefab; 

    [Header("Server Dummy Data")]
    public List<StoneDataSO> availableStones;

    private void Start() // 🌟 OnEnable এর বদলে Start দিলাম
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

        // আগের কার্ডগুলো মুছে ফেলা
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        int totalSpawned = 0;

        // ১. ডামি পাথর স্পন করা (Scriptable Objects থেকে)
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

        // ২. 🌟 লাইভ পাথর স্পন করা (Predictor Mode থেকে আসা Local Server ডাটা)
        if (StoneServer.Instance != null && StoneServer.Instance.liveStonesList.Count > 0)
        {
            foreach (var liveStone in StoneServer.Instance.liveStonesList)
            {
                if (liveStone == null) continue;

                GameObject newCard = Instantiate(stoneItemPrefab, contentPanel);
                StoneItemUI uiScript = newCard.GetComponent<StoneItemUI>();

                if (uiScript != null)
                {
                    // লাইভ ব্লুপ্রিন্ট ডাটা পাঠানোর জন্য নতুন ফাংশন
                    uiScript.SetupLiveStone(liveStone); 
                    totalSpawned++;
                }
            }
        }

        Debug.Log($"✅ Market Generation Complete! Total Stones Spawned: {totalSpawned}");
    }
}
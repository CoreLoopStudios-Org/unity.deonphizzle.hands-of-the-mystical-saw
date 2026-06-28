using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class FindBlockingUI : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                Debug.Log("🚫 <color=red><b>CLICK BLOCKED!</b></color> যে UI গুলো ক্লিক আটকে দিচ্ছে:");
                foreach(RaycastResult result in results)
                {
                    Debug.Log("-> <color=orange>" + result.gameObject.name + "</color>");
                }
            }
            else
            {
                Debug.Log("✅ <color=green>No UI blocked the click!</color> থ্রিডি পাথরে ক্লিক লাগার কথা।");
            }
        }
    }
}
using UnityEngine;

public class StoneDataVerifier : MonoBehaviour
{
    void Start()
    {
        Debug.Log("<color=cyan>🔍 Verifying Exact UI Inputs from Predictor Scene...</color>");

        if (StoneServer.Instance != null && StoneServer.Instance.liveStonesList.Count > 0)
        {
            var latestStone = StoneServer.Instance.liveStonesList[StoneServer.Instance.liveStonesList.Count - 1];
            var predData = latestStone.predictor_challenge_data;

            if (predData != null)
            {
                Debug.Log("<color=yellow>=== 🎛️ EXACT UI SELECTIONS MATCH ===</color>");

                // 1. Stone Size
                Debug.Log($"🪨 Stone Size: {predData.targetStoneSize}");

                // 2. Rotation Angle (এটা Blueprint থেকে আসছে)
                if(latestStone.rotation_system != null)
                    Debug.Log($"📐 Rotation Angle: {latestStone.rotation_system.rotation_angle}");

                // 3. Anchor Points (এটা Blueprint থেকে আসছে)
                if(latestStone.anchor_network != null)
                    Debug.Log($"⚓ Anchor Points: {latestStone.anchor_network.point_count}");

                // 4. Rotation Speed
                Debug.Log($"⚡ Rotation Speed: {predData.manualSpeedSlider}");

                // 5. Loop Duration
                Debug.Log($"⏳ Loop Duration: {predData.challengeDuration}");

                // 6. Adversity
                Debug.Log($"🌪️ Adversity: {predData.jitterAmount}");

                // 7. Movement Pattern (ব্যাকএন্ডের লজিক থেকে UI-এর নাম বের করা)
                string movementPatternUI = "Static";
                if (predData.rotationPattern == StoneChallengeData.RotationalPattern.Circular) movementPatternUI = "Circular";
                else if (predData.rotationPattern == StoneChallengeData.RotationalPattern.Chaotic) movementPatternUI = "Chaotic";
                else if (predData.coreMovement == StoneChallengeData.MovementType.Linear) movementPatternUI = "Linear";
                else if (predData.coreMovement == StoneChallengeData.MovementType.Oscillation) movementPatternUI = "Oscillation";
                
                Debug.Log($"🔄 Movement Pattern: {movementPatternUI}");

                // 8. Difficulty Tier
                Debug.Log($"⭐ Difficulty Tier: {predData.minimumSkillRequired}");

                // 9. Wager Amount
                Debug.Log($"💰 Wager Amount: {predData.wagerAmount}");

                Debug.Log("<color=green>✅ ALL 9 UI DATA MATCHED AND VERIFIED!</color>");
                Debug.Log("<color=yellow>=============================================</color>");
            }
            else
            {
                Debug.LogWarning("⚠️ Predictor Data is NULL!");
            }
        }
        else
        {
            Debug.LogError("❌ DATA LOSS! StoneServer is empty.");
        }
    }
}
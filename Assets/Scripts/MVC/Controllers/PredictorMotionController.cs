// using UnityEngine;
//
// public class PredictorMotionController : MonoBehaviour
// {
//     [Header("Data Source (MVC Architecture)")]
//     public CurrentStoneModel currentStoneModel; // 🌟 তোমার MVC স্ক্রিপ্টেবল অবজেক্ট
//
//     [Header("Predictor Challenge Data")]
//     public StoneChallengeData challengeData;
//     public bool isDataLoaded = false;
//
//     // পাথরের আদি পজিশন মনে রাখার জন্য
//     private Vector3 originalPosition;
//     private float timer = 0f;
//
//     void Start()
//     {
//         originalPosition = transform.position;
//         LoadPredictorData();
//     }
//
//     void LoadPredictorData()
//     {
//         // 🌟 PlayerPrefs এর বদলে তোমার ScriptableObject থেকে ডেটা নিচ্ছি
//         if (currentStoneModel != null && currentStoneModel.parsedBlueprint != null && currentStoneModel.parsedBlueprint.predictor_challenge_data != null)
//         {
//             challengeData = currentStoneModel.parsedBlueprint.predictor_challenge_data;
//             isDataLoaded = true;
//             Debug.Log("✅ [Motion Engine]: Predictor Data Loaded from MVC ScriptableObject! Pattern: " + challengeData.rotationPattern);
//         }
//         else
//         {
//             Debug.LogWarning("⚠️ No Predictor Data found in CurrentStoneModel! Running as normal static stone.");
//         }
//     }
//
//     void Update()
//     {
//         if (!isDataLoaded || challengeData == null) return;
//
//         timer += Time.deltaTime;
//         
//         // প্রতিটি ফ্রেমের শুরুতে পাথরকে তার বেস পজিশনে নিয়ে আসবো (যাতে Jitter বা Oscillation লিমিট ক্রস না করে)
//         Vector3 currentTargetPosition = originalPosition;
//
//         // ==========================================
//         // 🌟 ১. Core Movement (পাথরের পজিশন পরিবর্তন)
//         // ==========================================
//         float moveSpeed = challengeData.manualSpeedSlider > 0 ? challengeData.manualSpeedSlider * 0.1f : 1f;
//
//         if (challengeData.coreMovement == StoneChallengeData.MovementType.Oscillation)
//         {
//             // ডানে-বামে দোল খাবে (Ping-Pong)
//             float offset = Mathf.Sin(timer * moveSpeed) * 2f; 
//             currentTargetPosition += new Vector3(offset, 0, 0);
//         }
//         else if (challengeData.coreMovement == StoneChallengeData.MovementType.Drift)
//         {
//             // আস্তে আস্তে সরে যাবে এবং originalPosition আপডেট হবে
//             originalPosition += Vector3.right * (challengeData.driftSpeed * Time.deltaTime);
//             currentTargetPosition = originalPosition;
//         }
//
//         // ==========================================
//         // 🌟 ২. Rotational Pattern & The "BAIT TRAP"
//         // ==========================================
//         float rotSpeed = challengeData.manualSpeedSlider > 0 ? challengeData.manualSpeedSlider : 20f;
//         
//         // 😈 Bait Pattern Logic: সময়ের অর্ধেক পার হলে হঠাৎ উল্টো দিকে দ্বিগুণ স্পিডে ঘুরবে!
//         if (challengeData.globalTrap == StoneChallengeData.PredictorMetaTrap.BaitPattern)
//         {
//             float halfTime = (int)challengeData.challengeDuration / 2f;
//             if (timer > halfTime) 
//             {
//                 rotSpeed = -rotSpeed * 1.5f; // ধোঁকা! ডিরেকশন রিভার্স এবং স্পিড আপ!
//             }
//         }
//
//         if (challengeData.rotationPattern == StoneChallengeData.RotationalPattern.Circular)
//         {
//             transform.Rotate(Vector3.up, rotSpeed * Time.deltaTime);
//         }
//         else if (challengeData.rotationPattern == StoneChallengeData.RotationalPattern.Chaotic)
//         {
//             float chaoticX = (Mathf.PerlinNoise(timer, 0) - 0.5f) * rotSpeed;
//             float chaoticY = (Mathf.PerlinNoise(0, timer) - 0.5f) * rotSpeed;
//             transform.Rotate(new Vector3(chaoticX, chaoticY, 0) * Time.deltaTime);
//         }
//
//         // ==========================================
//         // 🌟 ৩. Adversity: Jitter (পাথর কাঁপুনি)
//         // ==========================================
//         if (challengeData.jitterAmount > 0)
//         {
//             // Jitter value অনুযায়ী রেন্ডম পজিশন শিফট (থরথর করে কাঁপবে)
//             float jitterStr = challengeData.jitterAmount * 0.01f;
//             Vector3 randomJitter = Random.insideUnitSphere * jitterStr;
//             currentTargetPosition += randomJitter;
//         }
//
//         // ফাইনাল পজিশন অ্যাপ্লাই করা
//         transform.position = currentTargetPosition;
//     }
// }
// using UnityEngine;
//
// public class PredictorMotionController : MonoBehaviour
// {
//     [Header("Data Source (MVC Architecture)")]
// public CurrentStoneModel currentStoneModel; // 🌟 your MVC scriptable object
//
//     [Header("Predictor Challenge Data")]
//     public StoneChallengeData challengeData;
//     public bool isDataLoaded = false;
//
// // To remember the initial position of the stone
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
// // 🌟 Taking data from your ScriptableObject instead of PlayerPrefs
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
// // Bring the stone to its base position at the start of each frame (so as not to cross jitter or oscillation limits)
//         Vector3 currentTargetPosition = originalPosition;
//
//         // ==========================================
// // 🌟 1. Core Movement (change in stone position)
//         // ==========================================
//         float moveSpeed = challengeData.manualSpeedSlider > 0 ? challengeData.manualSpeedSlider * 0.1f : 1f;
//
//         if (challengeData.coreMovement == StoneChallengeData.MovementType.Oscillation)
//         {
// // Ping-Pong
//             float offset = Mathf.Sin(timer * moveSpeed) * 2f; 
//             currentTargetPosition += new Vector3(offset, 0, 0);
//         }
//         else if (challengeData.coreMovement == StoneChallengeData.MovementType.Drift)
//         {
// // will slowly move and update the originalPosition
//             originalPosition += Vector3.right * (challengeData.driftSpeed * Time.deltaTime);
//             currentTargetPosition = originalPosition;
//         }
//
//         // ==========================================
// // 🌟 2. Rotational Pattern & The "BAIT TRAP"
//         // ==========================================
//         float rotSpeed = challengeData.manualSpeedSlider > 0 ? challengeData.manualSpeedSlider : 20f;
//         
// // 😈 Bait Pattern Logic: When half of the time passes, it will suddenly turn in the opposite direction at double speed!
//         if (challengeData.globalTrap == StoneChallengeData.PredictorMetaTrap.BaitPattern)
//         {
//             float halfTime = (int)challengeData.challengeDuration / 2f;
//             if (timer > halfTime) 
//             {
// rotSpeed = -rotSpeed * 1.5f; // cheat! Reverse direction and speed up!
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
// // 🌟 3. Adversity: Jitter
//         // ==========================================
//         if (challengeData.jitterAmount > 0)
//         {
// // Random position shift according to jitter value
//             float jitterStr = challengeData.jitterAmount * 0.01f;
//             Vector3 randomJitter = Random.insideUnitSphere * jitterStr;
//             currentTargetPosition += randomJitter;
//         }
//
// // Apply final position
//         transform.position = currentTargetPosition;
//     }
// }
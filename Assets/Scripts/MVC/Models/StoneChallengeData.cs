using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StoneChallengeData
{
    [Header("1. Metadata & Ecosystem (Predictor Info)")]
    public string predictorId;            // কে বানিয়েছে
    public int deterministicSeed;         // Seed Lock (যাতে রিপ্লে সব সময় সেম থাকে)
    public float reputationScore;         // Predictor-এর Reputation & Ranking

    [Header("2. AI Difficulty & Skill Tree")]
    public SkillTier minimumSkillRequired; // কোন স্কিলের প্লেয়ার এটা খেলতে পারবে (Initiate - Mythic)
    public float difficultyMultiplier;     // কঠিন লেভেলের জন্য রিওয়ার্ড মাল্টিপ্লায়ার

    [Header("3. Core Movement & Rotational Patterns")]
    public MovementType coreMovement;      // Linear, Diagonal, Z-Axis, etc.
    public RotationalPattern rotationPattern; // Circular, Spiral, Chaotic, etc.
    public LoopBehavior loopBehavior;      // Continuous, Ping-Pong, Escalating, etc.
    public LoopDuration challengeDuration; // 10s to 120s constraints

    [Header("4. Speed & Time Controls")]
    public SpeedControl speedMode;         // Slow, Manual, Accel, Decel
    public float manualSpeedSlider;        // কাস্টম স্পিড
    public AnimationCurve speedCurve;      // Acceleration/Deceleration কার্ভের জন্য (Pro level!)

    [Header("5. Advanced Adversity Injection")]
    public float jitterAmount;             // পাথর কতটা কাঁপবে (Jitter)
    public float driftSpeed;               // আস্তে আস্তে সরে যাওয়া (Drift)
    public Vector3 loopOffset;             // লুপ অফসেট (প্লেয়ারের মেমরি ধোঁকা দেওয়ার জন্য)

    [Header("6. Predictor Meta Playbook (Psychological Traps)")]
    public PredictorMetaTrap globalTrap;   // Bait pattern, False Symmetry, Speed Deception

    [Header("7. Phase Timeline Editor (Multi-phase motion)")]
    public List<PhaseBehavior> phaseTimeline = new List<PhaseBehavior>();
    
    // 🌟 নতুন টাইম স্টেপ সিকোয়েন্স সেভ রাখার জন্য লিস্ট
    public System.Collections.Generic.List<TimeStepData> movementSequence = new System.Collections.Generic.List<TimeStepData>();

    [Header("8. Player Constraints & Scoring (The Pressure)")]
    public int maxStrikesAllowed = 3;      // Three-strike limit
    public float commitFreezeTime = 2f;    // Commit freeze lock timing
    public int allowedTorchPeeks = 3;      // Limited reveals (Torch)
    public float scoreDecayRate;           // Continuous score decay during gameplay
    public float targetScoreToWin;         // জেতার জন্য মিনিমাম কত পয়েন্ট লাগবে

    [Header("9. Stone Size & Economy (Flowchart Update)")]
    public StoneSize targetStoneSize;      // Small, Medium, Large (অ্যাঙ্করের পরিমাণ নির্ধারণ করবে)
    public int wagerAmount;                // Predictor কত পয়েন্ট বাজি ধরছে
    public bool isFairnessValidated;       // AI কি চেক করেছে যে এটা জেতা সম্ভব?


    // =======================================================
    // 🌟 ENUMS: তোমার গেম ডিজাইন ডকুমেন্ট অনুযায়ী ক্যাটাগরি 🌟
    // =======================================================

    public enum SkillTier { Initiate, Cutter, Carver, MasterCutter, Grandmaster, Mythic }
    public enum MovementType { Static, Linear, Diagonal, ZAxisDepth, Oscillation, Drift }
    public enum RotationalPattern { None, Circular, Elliptical, Spiral, Triangular, Square, Teardrop, Chaotic }
    public enum LoopBehavior { Continuous, PingPong, Segmented, Randomized, Escalating }
    public enum LoopDuration { Beginner_10s = 10, Balanced_15s = 15, Competitive_30s = 30, Advanced_60s = 60, HighSkill_90s = 90, Expert_120s = 120 }
    public enum SpeedControl { PresetSlow, PresetMedium, PresetFast, ManualSlider, AccelerationCurve, DecelerationCurve }
    public enum PredictorMetaTrap { None, BaitPattern, FalseSymmetry, PhaseTrap, SpeedDeception, AdversityStack }
    public enum StoneSize { Small, Medium, Large } // 🌟 নতুন যোগ করা হলো
}

// 🌟 Multi-phase মোশনের জন্য আলাদা স্ট্রাকচার (ফেজ অনুযায়ী আচরণ বদলানো)
[System.Serializable]
public class PhaseBehavior
{
    public float startTime; // এই ফেজ কখন শুরু হবে (যেমন: ২০ সেকেন্ড পর)
    public float duration;  // কতক্ষণ চলবে
    public StoneChallengeData.MovementType phaseMovement; // নতুন ফেজে মুভমেন্ট কেমন হবে
    public StoneChallengeData.RotationalPattern phasePattern; 
    public StoneChallengeData.PredictorMetaTrap trapType; // এই ফেজে হঠাৎ করে ধোঁকা দেওয়া
    public float phaseSpeedMultiplier; // হঠাৎ স্পিড বাড়ানো বা কমানো
}
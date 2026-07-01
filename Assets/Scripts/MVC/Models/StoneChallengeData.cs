using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StoneChallengeData
{
    [Header("1. Metadata & Ecosystem (Predictor Info)")]
    public string predictorId;            // Who made it
    public int deterministicSeed;         // Seed Lock (so replays are always the same)
    public float reputationScore;         // Predictor's Reputation & Ranking

    [Header("2. AI Difficulty & Skill Tree")]
    public SkillTier minimumSkillRequired; // Any skill player can play this (Initiate - Mythic)
    public float difficultyMultiplier;     // Reward multiplier for harder levels

    [Header("3. Core Movement & Rotational Patterns")]
    public MovementType coreMovement;      // Linear, Diagonal, Z-Axis, etc.
    public RotationalPattern rotationPattern; // Circular, Spiral, Chaotic, etc.
    public LoopBehavior loopBehavior;      // Continuous, Ping-Pong, Escalating, etc.
    public LoopDuration challengeDuration; // 10s to 120s constraints

    [Header("4. Speed & Time Controls")]
    public SpeedControl speedMode;         // Slow, Manual, Accel, Decel
    public float manualSpeedSlider;        // custom speed
    public AnimationCurve speedCurve;      // For Acceleration/Deceleration curves (Pro level!)

    [Header("5. Advanced Adversity Injection")]
    public float jitterAmount;             // how much the stone will jitter
    public float driftSpeed;               // Drift
    public Vector3 loopOffset;             // loop offset (to trick the player's memory)

    [Header("6. Predictor Meta Playbook (Psychological Traps)")]
    public PredictorMetaTrap globalTrap;   // Bait pattern, False Symmetry, Speed Deception

    [Header("7. Phase Timeline Editor (Multi-phase motion)")]
    public List<PhaseBehavior> phaseTimeline = new List<PhaseBehavior>();
    
    // 🌟 List to store new time step sequences
    public System.Collections.Generic.List<TimeStepData> movementSequence = new System.Collections.Generic.List<TimeStepData>();

    [Header("8. Player Constraints & Scoring (The Pressure)")]
    public int maxStrikesAllowed = 3;      // Three-strike limit
    public float commitFreezeTime = 2f;    // Commit freeze lock timing
    public int allowedTorchPeeks = 3;      // Limited reveals (Torch)
    public float scoreDecayRate;           // Continuous score decay during gameplay
    public float targetScoreToWin;         // Minimum number of points needed to win

    [Header("9. Stone Size & Economy (Flowchart Update)")]
    public StoneSize targetStoneSize;      // Small, Medium, Large (will determine the size of the anchor)
    public int wagerAmount;                // How many points the Predictor is betting on
    public bool isFairnessValidated;       // Did the AI check that it's possible to win?


    // =======================================================
    // 🌟 ENUMS: Categories according to your game design document 🌟
    // =======================================================

    public enum SkillTier { Initiate, Cutter, Carver, MasterCutter, Grandmaster, Mythic }
    public enum MovementType { Static, Linear, Diagonal, ZAxisDepth, Oscillation, Drift }
    public enum RotationalPattern { None, Circular, Elliptical, Spiral, Triangular, Square, Teardrop, Chaotic }
    public enum LoopBehavior { Continuous, PingPong, Segmented, Randomized, Escalating }
    public enum LoopDuration { Beginner_10s = 10, Balanced_15s = 15, Competitive_30s = 30, Advanced_60s = 60, HighSkill_90s = 90, Expert_120s = 120 }
    public enum SpeedControl { PresetSlow, PresetMedium, PresetFast, ManualSlider, AccelerationCurve, DecelerationCurve }
    public enum PredictorMetaTrap { None, BaitPattern, FalseSymmetry, PhaseTrap, SpeedDeception, AdversityStack }
    public enum StoneSize { Small, Medium, Large } // 🌟 Newly added
}

// 🌟 Separate structure for multi-phase motion (changing behavior according to phase)
[System.Serializable]
public class PhaseBehavior
{
    public float startTime; // When will this phase start (ie: after 20 seconds)
    public float duration;  // How long to run
    public StoneChallengeData.MovementType phaseMovement; // What will the movement look like in the new phase?
    public StoneChallengeData.RotationalPattern phasePattern; 
    public StoneChallengeData.PredictorMetaTrap trapType; // Abort this phase
    public float phaseSpeedMultiplier; // Increase or decrease speed suddenly
}
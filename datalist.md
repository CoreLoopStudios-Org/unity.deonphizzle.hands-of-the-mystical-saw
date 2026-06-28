# 📊 Data Flow: Predictor Mode → Player Mode
### Backend Analysis & Data Reference List
> Full analysis of the DeonPhizzle Unity project — tracing every data field from its origin in **Predictor Mode** to its consumption in **Player Mode**, and what goes to the backend server.

---

## 🗺️ System Overview (Flow Map)

\\\
[Predictor Role]                         [Backend / Server]               [Player Role]
     │                                          │                               │
 PredictorUIManager                        StoneServer                    StoneSpawner
 (UI Selections)                       (liveStonesList)                  (3D Stone Spawn)
     │                                          │                               │
 TimeStepSequenceManager                  GlobalStoneData               JadeCuttingGame
 (Movement Sequence)                   (CurrentBlueprint)               (Gameplay Loop)
     │                                          │                               │
 PredictorController                       PlayerPrefs                   PredictorMotionController
 (API / Blueprint Build)             (PendingStoneChallenge)            (Stone Movement)
     │                                          │                               │
 StoneBlueprint                          StoneChallengeData                WinLoseManager
 (Full JSON Payload)                    (Challenge Rules)                 (Result + Points)
     │                                          │                               │
 StoneCutter API                          DataManager                    DataManager
 (http://23.26.207.43:8084)            (Points / Tier)                  (Points Update)
\\\

---

## 📋 SECTION 1 — PREDICTOR MODE INPUT DATA

### 1.1 — Stone Properties (Physical)
| # | Field Name | Data Type | Source Class | Value Range | Description |
|---|-----------|-----------|-------------|-------------|-------------|
| 1 | StoneSize | Enum | PredictorUIManager.selectedSize | Small / Medium / Large | Stone physical size tier |
| 2 | StoneDensity | Enum (random) | PredictorController | Light / Normal / Heavy | Stone material weight |
| 3 | StoneStress | Enum (random) | PredictorController | Low / Medium / High | Internal stress level |
| 4 | FractureTolerance | Enum (random) | PredictorController | Fragile / Normal / Strong | How easily it breaks |
| 5 | stone_size_label | string | PredictorController (calculated) | Tiny / Small / Medium / Large | Display label from random int |
| 6 | total_weight_kg | int (random) | PredictorController | 5 – 30 kg | Displayed on market card |
| 7 | stone_icon_index | int (random) | PredictorController | 0 – 3 | Which icon to show on card |
| 8 | size_scale | float | PhysicsAndMaterial | 1.0f (default) | 3D mesh scale |

### 1.2 — Jade Core Properties
| # | Field Name | Data Type | Source Class | Value Range | Description |
|---|-----------|-----------|-------------|-------------|-------------|
| 9 | JadeColor | Enum (random) | PredictorController | PaleGreen / DeepGreen / Emerald / Imperial | Jade color |
| 10 | color_rating | string (hex) | PredictorController.GetHexColor() | 98FB98 / 006400 / 50C878 / 1C542D | Hex used for 3D jade render |
| 11 | JadeQuantity | Enum (random) | PredictorController | Single / Few / Many | Number of jade cores |
| 12 | quantity_mass | int | PredictorController | 1 / 3 / 5 | Jade count inside stone |

### 1.3 — Rotation & Movement System
| # | Field Name | Data Type | Source Class | Value Range | Description |
|---|-----------|-----------|-------------|-------------|-------------|
| 13 | selectedSpeed | float | PredictorUIManager | 10 – 100 | Rotation speed (slider mapped) |
| 14 | selectedAngle | float | PredictorUIManager | Custom | Rotation angle |
| 15 | RotationPattern | Enum (random) | PredictorController | LeftToRight / RightToLeft | Spin direction |
| 16 | SpinSpeed | Enum (random) | PredictorController | Slow / Fast | Speed multiplier |
| 17 | rotation_pattern | string | RotationSystem | LeftToRight / RightToLeft | Stored in blueprint |
| 18 | spin_speed | string | RotationSystem | Slow / Fast | Stored in blueprint |
| 19 | selectedPatternString | string | PredictorUIManager | Static / Linear / Oscillation / Circular / Chaotic | Core movement pattern |
| 20 | coreMovement | Enum | StoneChallengeData | Static / Linear / Diagonal / ZAxisDepth / Oscillation / Drift | Movement type |
| 21 | rotationPattern | Enum | StoneChallengeData | None / Circular / Elliptical / Spiral / Triangular / Square / Teardrop / Chaotic | Rotational style |

### 1.4 — Anchor Network
| # | Field Name | Data Type | Source Class | Value Range | Description |
|---|-----------|-----------|-------------|-------------|-------------|
| 22 | StoneAnchor | Enum (random) | PredictorController | Free / Grounded / WallAttached | Anchor type |
| 23 | selectedAnchors | int | PredictorUIManager | 1 – 5 | Number of anchor points |
| 24 | anchor_network.type | string | AnchorNetwork | Free / Grounded / WallAttached | Anchor type stored |
| 25 | anchor_network.point_count | int | AnchorNetwork | 1 – 5 | Anchors placed in 3D scene |

### 1.5 — Adversity & Difficulty
| # | Field Name | Data Type | Source Class | Value Range | Description |
|---|-----------|-----------|-------------|-------------|-------------|
| 26 | AdversityLevel | Enum (random) | PredictorController | Low / Medium / High | Blueprint adversity |
| 27 | adversity_level | string | StoneBlueprint | Low / Medium / High | Stored in blueprint |
| 28 | currentAdversity | int | PredictorUIManager | 0 – 10 | Jitter intensity |
| 29 | jitterAmount | float | StoneChallengeData | 0 – 10 | Stone shake in Player Mode |
| 30 | driftSpeed | float | StoneChallengeData | Custom | Stone drift velocity |
| 31 | loopOffset | Vector3 | StoneChallengeData | Custom | Offset to confuse player |
| 32 | globalTrap | Enum | StoneChallengeData | None / BaitPattern / FalseSymmetry / PhaseTrap / SpeedDeception / AdversityStack | Psychological trap |

### 1.6 — Challenge Parameters
| # | Field Name | Data Type | Source Class | Value Range | Description |
|---|-----------|-----------|-------------|-------------|-------------|
| 33 | selectedDifficulty | Enum | PredictorUIManager | Initiate / Cutter / Carver / MasterCutter / Grandmaster / Mythic | Skill tier required |
| 34 | minimumSkillRequired | Enum | StoneChallengeData | (same as above) | Stored in challenge |
| 35 | selectedDuration | Enum | PredictorUIManager | Beginner_10s / Balanced_15s / Competitive_30s / Advanced_60s / HighSkill_90s / Expert_120s | Loop duration |
| 36 | challengeDuration | Enum | StoneChallengeData | 10s / 15s / 30s / 60s / 90s / 120s | Duration stored |
| 37 | manualSpeedSlider | float | StoneChallengeData | 10 – 100 | Speed for motion engine |
| 38 | speedMode | Enum | StoneChallengeData | PresetSlow / PresetMedium / PresetFast / ManualSlider / AccelerationCurve / DecelerationCurve | Speed control type |
| 39 | speedCurve | AnimationCurve | StoneChallengeData | Custom | Accel/decel curve |

### 1.7 — Wager & Economy
| # | Field Name | Data Type | Source Class | Value Range | Description |
|---|-----------|-----------|-------------|-------------|-------------|
| 40 | currentWagerString | string | PredictorUIManager | 0 – 999999 | Wager input (numpad) |
| 41 | wagerAmount | int | StoneChallengeData | 0 – 999999 | Points staked by Predictor |
| 42 | challenge_points | int | StoneBlueprint | 2000 – 10000 | Reward if player wins |
| 43 | maxStrikesAllowed | int | StoneChallengeData | 3 (default) | Max failed hits allowed |
| 44 | allowedTorchPeeks | int | StoneChallengeData | 3 (default) | Limited reveal count |
| 45 | scoreDecayRate | float | StoneChallengeData | Custom | Points lost per second with torch |
| 46 | targetScoreToWin | float | StoneChallengeData | Custom | Min points to win |
| 47 | commitFreezeTime | float | StoneChallengeData | 2f (default) | Freeze lock on commit |
| 48 | isFairnessValidated | bool | StoneChallengeData | true / false | AI fairness check |

### 1.8 — Time Step Sequence (Multi-Phase Movement)
| # | Field Name | Data Type | Source Class | Value Range | Description |
|---|-----------|-----------|-------------|-------------|-------------|
| 49 | savedSteps | List<TimeStepData> | TimeStepSequenceManager | Up to 5 entries | Ordered movement sequence |
| 50 | TimeStepData.duration | float | TimeStepData | 1s – full loop time | Duration of this phase |
| 51 | TimeStepData.movementPattern | string | TimeStepData | Static / Linear / Oscillation / Circular / Chaotic | Pattern for this phase |
| 52 | movementSequence | List<TimeStepData> | StoneChallengeData | Up to 5 entries | Final packed sequence |

### 1.9 — Phase Timeline (Advanced Multi-Phase)
| # | Field Name | Data Type | Source Class | Value Range | Description |
|---|-----------|-----------|-------------|-------------|-------------|
| 53 | phaseTimeline | List<PhaseBehavior> | StoneChallengeData | Multiple phases | Full multi-phase setup |
| 54 | PhaseBehavior.startTime | float | PhaseBehavior | Custom | When phase begins |
| 55 | PhaseBehavior.duration | float | PhaseBehavior | Custom | How long phase lasts |
| 56 | PhaseBehavior.phaseMovement | Enum | PhaseBehavior | MovementType values | Movement for phase |
| 57 | PhaseBehavior.phasePattern | Enum | PhaseBehavior | RotationalPattern values | Rotation for phase |
| 58 | PhaseBehavior.trapType | Enum | PhaseBehavior | PredictorMetaTrap values | Trap in this phase |
| 59 | PhaseBehavior.phaseSpeedMultiplier | float | PhaseBehavior | Custom | Speed change per phase |

---

## 📦 SECTION 2 — BACKEND / SERVER DATA

### 2.1 — StoneBlueprint (Full JSON Payload to API)
| # | Field Name | Data Type | API Endpoint | Description |
|---|-----------|-----------|-------------|-------------|
| 60 | stone_uid | string (GUID) | POST /stones | Unique stone identifier |
| 61 | challenge_points | int | POST /stones | Reward value (2000–10000) |
| 62 | total_weight_kg | int | POST /stones | Stone weight metadata |
| 63 | stone_icon_index | int | POST /stones | Market card display icon |
| 64 | stone_size_label | string | POST /stones | Small / Medium / Large |
| 65 | physics_and_material.size_scale | float | POST /stones | 3D scale |
| 66 | physics_and_material.density | string | POST /stones | Light / Normal / Heavy |
| 67 | physics_and_material.stress | string | POST /stones | Low / Medium / High |
| 68 | physics_and_material.fracture_tolerance | string | POST /stones | Fragile / Normal / Strong |
| 69 | rotation_system.speed | float | POST /stones | Rotation speed |
| 70 | rotation_system.rotation_angle | float | POST /stones | Angle in degrees |
| 71 | rotation_system.rotation_pattern | string | POST /stones | LeftToRight / RightToLeft |
| 72 | rotation_system.spin_speed | string | POST /stones | Slow / Fast |
| 73 | anchor_network.type | string | POST /stones | Anchor type |
| 74 | anchor_network.point_count | int | POST /stones | Number of anchors |
| 75 | jade_core.color_rating | string (hex) | POST /stones | Jade hex color |
| 76 | jade_core.quantity_mass | int | POST /stones | Jade count (1/3/5) |
| 77 | adversity_level | string | POST /stones | Low / Medium / High |
| 78 | predictor_challenge_data | StoneChallengeData | Internal only | Full GDD challenge ruleset |

### 2.2 — Server Response (StoneData from SDK)
| # | Field Name | Data Type | Source | Description |
|---|-----------|-----------|--------|-------------|
| 79 | StoneData.Id | int | API Response | Server-assigned stone ID |
| 80 | StoneData.Name | string | API Response | Stone name |
| 81 | StoneData.StoneSize | StoneSizeType | API Response | Small / Medium / Large |
| 82 | StoneData.JadeCount | int | API Response | Jade count |
| 83 | StoneData.JsonContext | string (JSON) | API Response | Full blueprint JSON |

### 2.3 — GlobalStoneData (In-Memory Cross-Scene Bridge)
| # | Field Name | Data Type | Source | Description |
|---|-----------|-----------|--------|-------------|
| 84 | GlobalStoneData.CurrentStone | StoneData | PredictorController | SDK stone after server save |
| 85 | GlobalStoneData.CurrentBlueprint | StoneBlueprint | PredictorController | Full blueprint with challenge data |

### 2.4 — StoneServer (Local In-Memory Server)
| # | Field Name | Data Type | Source | Description |
|---|-----------|-----------|--------|-------------|
| 86 | StoneServer.liveStonesList | List<StoneBlueprint> | PredictorController | All generated stones in session |
| 87 | StoneServer.ChosenMode | GameMode | GameModeManager | Classic or Modern |

### 2.5 — PlayerPrefs (Persistent Local Storage)
| # | Key | Data Type | Written By | Read By | Description |
|---|-----|-----------|-----------|---------|-------------|
| 88 | PlayerTotalPoints | int | DataManager | DataManager, ProfileManager | Cumulative score |
| 89 | PendingStoneChallenge | string (JSON) | PredictorUIManager | PredictorMotionController | StoneChallengeData JSON |
| 90 | SavedGameTheme | int | GameModeManager | GameModeManager, StoneServer | 0=Classic, 1=Modern |
| 91 | AutoOpenStoneMarket | int | WinLoseManager | MainMenuController | Signal to open store |

### 2.6 — JSON Stone File Format (Full Server Schema)
| # | Field | Description |
|---|-------|-------------|
| 92 | stone_metadata.stone_uid | Unique stone ID |
| 93 | stone_metadata.predictor_id | Who created this stone |
| 94 | stone_metadata.wager_points | Wager placed |
| 95 | stone_metadata.creation_timestamp | ISO 8601 timestamp |
| 96 | physics_and_material.size_scale | Float mesh scale |
| 97 | physics_and_material.density | Float density value |
| 98 | physics_and_material.internal_stress | Float stress 0–1 |
| 99 | physics_and_material.fracture_tolerance | Float tolerance 0–1 |
| 100 | physics_and_material.mesh_seed_id | Int for procedural mesh |
| 101 | rotation_system.base_speed | Slow / Medium / Fast |
| 102 | rotation_system.pattern_sequence[].pattern_id | Sequence order |
| 103 | rotation_system.pattern_sequence[].duration | Phase duration (seconds) |
| 104 | rotation_system.pattern_sequence[].direction | clockwise / counter_clockwise |
| 105 | anchor_network.primary_anchor.position | Vector3 XYZ |
| 106 | anchor_network.primary_anchor.adversities[] | String array of effects |
| 107 | anchor_network.secondary_anchors[].id | sec_1, sec_2... |
| 108 | anchor_network.secondary_anchors[].position | Vector3 XYZ |
| 109 | anchor_network.secondary_anchors[].adversities[] | String array of effects |
| 110 | anchor_network.hidden_anchor.is_present | Bool |
| 111 | anchor_network.hidden_anchor.position | Vector3 XYZ |
| 112 | jade_core.color_rating | Imperial_Green_5Star etc. |
| 113 | jade_core.clarity_percentage | Float 0–100 |
| 114 | jade_core.quantity_mass | Float mass value |
| 115 | jade_core.calculated_base_value | Base reward in points |
| 116 | ai_validation.is_solvable | Bool — fairness check |
| 117 | ai_validation.minimum_strikes_required | Optimal minimum hits |
| 118 | ai_validation.optimal_strike_vectors[] | Array of Vector3 hit points |

---

## 🎮 SECTION 3 — PLAYER MODE CONSUMED DATA

### 3.1 — Stone Spawning (StoneSpawner.cs)
| # | Field | Source | Used For |
|---|-------|--------|---------|
| 119 | CurrentStone.StoneSize | GlobalStoneData | 3D scale multiplier (Small=0.5, Large=1.0, Medium=0.75) |
| 120 | CurrentBlueprint.physics_and_material.density | GlobalStoneData | Rigidbody mass (Heavy=50, Light=5, Normal=20) |
| 121 | CurrentBlueprint.rotation_system.speed | GlobalStoneData | StoneRotator speed value |
| 122 | CurrentBlueprint.rotation_system.spin_speed | GlobalStoneData | Speed multiplier (Fast = x2) |
| 123 | CurrentBlueprint.rotation_system.rotation_pattern | GlobalStoneData | Rotation direction |
| 124 | CurrentBlueprint.jade_core.color_rating | GlobalStoneData | Jade material color |
| 125 | CurrentStone.JadeCount | GlobalStoneData | Jade core scale ratio |
| 126 | CurrentBlueprint.anchor_network.point_count | GlobalStoneData | Anchors placed in 3D space |

### 3.2 — Stone Motion (PredictorMotionController.cs)
| # | Field | Source | Used For |
|---|-------|--------|---------|
| 127 | challengeData.coreMovement | StoneChallengeData | Linear / Oscillation / Drift position update |
| 128 | challengeData.rotationPattern | StoneChallengeData | Circular / Chaotic rotation per-frame |
| 129 | challengeData.manualSpeedSlider | StoneChallengeData | Motion engine speed reference |
| 130 | challengeData.jitterAmount | StoneChallengeData | Random shake magnitude (*0.01f) |
| 131 | challengeData.driftSpeed | StoneChallengeData | Drift velocity per second |
| 132 | challengeData.globalTrap | StoneChallengeData | BaitPattern logic |
| 133 | challengeData.challengeDuration | StoneChallengeData | Half-time trap calculation |
| 134 | movementSequence[n].duration | StoneChallengeData | Duration per phase step |
| 135 | movementSequence[n].movementPattern | StoneChallengeData | Pattern per phase step |

### 3.3 — Gameplay Rules (JadeCuttingGame.cs)
| # | Field | Source | Used For |
|---|-------|--------|---------|
| 136 | totalPrize | JadeCuttingGame / ChallengeData | Prize pool in UI |
| 137 | wageredAmount | JadeCuttingGame / wagerAmount | Wager displayed |
| 138 | timeRemaining | JadeCuttingGame | Countdown timer (150s default) |
| 139 | scoreDecayRate | JadeCuttingGame / StoneChallengeData | Prize decay per second during torch |
| 140 | maxStrikes | JadeCuttingGame / maxStrikesAllowed | Strike counter limit |
| 141 | currentStrikes | JadeCuttingGame | Current number of failed hits |
| 142 | isTorchActive | JadeCuttingGame | Reveals jade but decays score |

### 3.4 — Strike System (StrikeSystem.cs)
| # | Field | Source | Used For |
|---|-------|--------|---------|
| 143 | strikes | List<StrikeVisual> | Visual strike bars array |
| 144 | currentStrikes | StrikeSystem | Failed hit count |
| 145 | StrikeVisual.activeColor | StrikeSystem | Color per strike bar |

### 3.5 — Post-Game Data
| # | Field | Source | Used For |
|---|-------|--------|---------|
| 146 | DataManager.totalPoints | DataManager | Running total score |
| 147 | DataManager.tierProgressPoints | DataManager | Progress within tier |
| 148 | DataManager.tierMaxPoints | DataManager | Max points for tier bar |
| 149 | DataManager.userName | DataManager | Display name |
| 150 | DataManager.tier | DataManager | Tier label |
| 151 | AutoOpenStoneMarket | PlayerPrefs | Signal to auto-open Stone Market |

---

## 👤 SECTION 4 — USER PROFILE DATA

| # | Field | Data Type | Description |
|---|-------|-----------|-------------|
| 152 | userId | string | Unique player identifier |
| 153 | userName | string | Player display name |
| 154 | avatarUrl | string | Server image URL |
| 155 | totalPoints | int | All-time cumulative score |
| 156 | currentTier | int | Numeric tier level |
| 157 | currentPoints | int | Points within tier |
| 158 | maxPointsForTier | int | Tier cap |
| 159 | stonesPlayed | int | Total games played |
| 160 | perfectCuts | int | Successful cuts count |
| 161 | failedCuts | int | Failed cuts count |
| 162 | tierStatus | string | Tier name label |

---

## 🏆 SECTION 5 — LEADERBOARD DATA

| # | Field | Data Type | Description |
|---|-------|-----------|-------------|
| 163 | rank | int | Global leaderboard rank |
| 164 | playerName | string | Player display name |
| 165 | tier | string | Diamond / Gold / Silver etc. |
| 166 | points | int | Total points for ranking |
| 167 | avatarUrl | string | Player avatar URL |

---

## 🏪 SECTION 6 — STONE MARKET DATA

### 6.1 — Stone Market Card Fields
| # | Field | Source | Displayed As |
|---|-------|--------|-------------|
| 168 | blueprint.challenge_points | StoneBlueprint | Points value on card |
| 169 | blueprint.total_weight_kg | StoneBlueprint | Weight: X.Xkg |
| 170 | blueprint.stone_size_label | StoneBlueprint / predictor_challenge_data | Size: Small/Medium/Large |
| 171 | blueprint.stone_icon_index | StoneBlueprint | Icon sprite selection |
| 172 | predictor_challenge_data.targetStoneSize | StoneChallengeData | Overrides size label (priority) |

### 6.2 — Stone Market Sources
| Source | Manager | Description |
|--------|---------|-------------|
| availableStones (ScriptableObject) | StoneMarketManager | Dummy stones from StoneDataSO assets |
| StoneServer.liveStonesList | StoneMarketManager | Live stones from Predictor Mode session |

---

## 🔄 SECTION 7 — COMPLETE DATA HANDOFF SEQUENCE (11 Steps)

`
STEP 1: Predictor sets values in PredictorUIManager
        → selectedSize, selectedDifficulty, selectedDuration,
          selectedPatternString, selectedSpeed, selectedAngle,
          selectedAnchors, currentAdversity, currentWagerString

STEP 2: TimeStepSequenceManager builds movementSequence
        → savedSteps: List<TimeStepData> [{duration, movementPattern}, ...]

STEP 3: PredictorUIManager.OnGenerateButtonPressed()
        → Builds StoneChallengeData with all selections
        → Serializes to JSON: JsonUtility.ToJson(newChallenge)
        → Saves to PlayerPrefs["PendingStoneChallenge"]

STEP 4: PredictorController.OnGenerateButtonClick()
        → Builds StoneBlueprint with all sub-objects
        → Calculates challenge_points, stone_size_label, stone_uid
        → Calls StoneCutterClient.SaveStone() → REST API (or mock)
        → Saves to GlobalStoneData.CurrentStone + .CurrentBlueprint

STEP 5: MVC Bridge — injects challenge into blueprint
        → StoneServer.Instance.AddNewGeneratedStone(blueprint)
        → latestStone.predictor_challenge_data = newChallenge
        → GlobalStoneData.CurrentBlueprint.predictor_challenge_data = newChallenge

STEP 6: StoneMarket shows live stone card (Player selects)
        → StoneMarketManager reads StoneServer.liveStonesList
        → StoneItemUI.SetupLiveStone(blueprint) renders card UI

STEP 7: Player clicks "Accept" (StoneItemUI.ManualAcceptClick)
        → Sets GlobalStoneData.CurrentBlueprint = selectedBlueprint
        → Loads scene by GameModeManager.currentTheme
          Classic → StoneCuttingScene_Classic
          Modern  → StoneGenerator Scene

STEP 8: Player Mode — StoneSpawner reads GlobalStoneData
        → Spawns 3D stone with size, rotation, jade color, anchors

STEP 9: PredictorMotionController reads from ScriptableObject / PlayerPrefs
        → Applies coreMovement, rotationPattern, jitter, drift, bait traps

STEP 10: JadeCuttingGame runs gameplay
         → Timer, prize decay, torch peeks, strike tracking

STEP 11: WinLoseManager → Result
         → DataManager.AddPoints() / SaveAndNotify()
         → PlayerPrefs["PlayerTotalPoints"] updated
         → PlayerPrefs["AutoOpenStoneMarket"] = 1 → Stone Market opens
`

---

## 🌐 SECTION 8 — API ENDPOINT REFERENCE

| Endpoint | Method | Data Sent | Response |
|----------|--------|-----------|----------|
| http://23.26.207.43:8084 | Base URL | — | — |
| /stones (inferred) | POST | name, sdkSize, hexColor, jadeCount, jsonContext | StoneData (Id, Name, StoneSize, JadeCount, JsonContext) |

> NOTE: useTestServer = true by default — mock data used with Id = 999 and Name = "Mock Jade Matrix".
> When useTestServer = false, StoneCutterClient.SaveStone() connects to the real REST API.

---

## 📌 SECTION 9 — DATA THAT MUST GO TO BACKEND (Priority Ranked)

| Priority | Data Field | Reason |
|----------|-----------|--------|
| CRITICAL | stone_uid | Unique identifier for multiplayer linking |
| CRITICAL | predictor_id | Match predictor to their stones |
| CRITICAL | wager_points / wagerAmount | Economy integrity |
| CRITICAL | challenge_points | Reward calculation |
| CRITICAL | predictor_challenge_data (full JSON) | Player receives correct challenge |
| HIGH | movementSequence (TimeStepData list) | Stone motion replay accuracy |
| HIGH | ai_validation.is_solvable | Fairness guarantee |
| HIGH | ai_validation.optimal_strike_vectors | Backend difficulty verification |
| MEDIUM | physics_and_material.* | Physical challenge accuracy |
| MEDIUM | rotation_system.* | Movement behavior |
| MEDIUM | anchor_network.* | Anchor placement in 3D |
| MEDIUM | jade_core.* | Reward display accuracy |
| LOW | stone_icon_index | UI display only |
| LOW | total_weight_kg | UI display only |
| LOW | stone_size_label | UI display only |
| LOW | creation_timestamp | Logging / analytics |

---

*Generated by Antigravity — DeonPhizzle Unity Project | June 2026*

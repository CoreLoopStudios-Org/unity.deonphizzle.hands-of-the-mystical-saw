# MVC Architecture & Server-Client SDK Data Specification

This document details the data structures, API endpoints, and system architecture for the Jade Stone Cutting game. It is designed to guide backend developers in building a production-ready SDK and server endpoints to replace the current client-side simulated server.

---

## 🚀 Quick Reference: Data Communicated via Backend SDK

Before diving into the detailed MVC architecture, here is the summary list of all data payloads that are communicated between the Unity client and the backend SDK:

### 1. Stone Creation & Publishing Payload (Sent via `POST /api/stones`)
When a player creates a custom stone in Predictor Mode, the following fields are transmitted to save the stone:
*   **`name`** *(string)*: Display name of the stone (e.g., `"Generated Jade"`).
*   **`stoneSize`** *(int)*: Categorized size: `0` = Small, `1` = Medium, `2` = Large.
*   **`stoneColor`** *(string)*: Hex code of the jade core (e.g., `"50C878"`).
*   **`jadeCount`** *(int)*: Number of jade objects nested inside (`1`, `3`, or `5`).
*   **`jsonContext`** *(string)*: A serialized JSON string containing the full **[StoneBlueprint](#31-stone-blueprint-stoneblueprint)** (detailing nested physics, rotation system, anchor networks, phase behavior, and the time-series movement timeline sequence).

### 2. User Profile & Progress Payload (Sent via `POST /api/users/profile` & `GET`)
Used to load player profiles, track stats, and update player tiers:
*   **`userId`** *(string)*: Unique player identifier.
*   **`userName`** *(string)*: Player nickname/display name.
*   **`avatarUrl`** *(string)*: Remote link to the player's avatar graphic.
*   **`totalPoints`** *(int)*: Overall points balance.
*   **`currentTier`** *(int)*: Numeric rank indicator of the tier.
*   **`currentPoints`** *(int)*: Current points accumulated inside the current tier.
*   **`maxPointsForTier`** *(int)*: Threshold points required to advance to the next tier.
*   **`stonesPlayed`** *(int)*: Total career stones cutting challenges entered.
*   **`perfectCuts`** *(int)*: Total career challenges won without triggering any strikes.
*   **`failedCuts`** *(int)*: Total career challenges failed (strikes $\ge$ limit).
*   **`tierStatus`** *(string)*: Text representation of rank (e.g., `"Gold"`, `"Diamond"`).

### 3. Match / Challenge Lifecycle Transaction Payloads
To secure points, prevent cheating, and manage wagers, the following transactional data is communicated:
*   **`wagerAmount`** *(int)*: Points deducted from the player's profile balance to initiate a challenge card.
*   **`stone_uid`** *(string)*: Unique identifier of the accepted stone challenge.
*   **`deterministicSeed`** *(int)*: Seed locked by the predictor to validate movement integrity and replay consistency.
*   **`isCompleted`** *(bool)*: Boolean success flag sent on client game-over.
*   **`strikes`** *(int)*: Total strike count registered during the gameplay session.
*   **`pointsAwarded`** *(int)*: Verified points added to the player's account upon completion.
*   **`savedGameTheme`** *(int)*: Selected game mode preference (`0` = Classic, `1` = Modern).

---

## 1. End-to-End Data Flow

The game operates on a Model-View-Controller (MVC) architecture. Below is the lifecycle of a stone from its definition in the Predictor UI to its instantiation in gameplay, up to scoring and leaderboard updates:

```mermaid
flowchart TD
    subgraph UI_PREDICTOR [1. Predictor UI & Configuration]
        A[PredictorUIManager] -->|User inputs parameters| B[StoneChallengeData]
        A -->|User designs timeline| C[movementSequence: TimeStepData[]]
        B -->|Add sequence| B_Seq[StoneChallengeData with Sequences]
        B_Seq -->|Save JSON payload| PP[(PlayerPrefs: PendingStoneChallenge)]
    end

    subgraph CONTROLLER_GENERATOR [2. SDK & Server Persistence]
        D[PredictorController] -->|Reads choices + randomizes materials| E[StoneBlueprint]
        E -->|Injects GDD challenge| E_Full[Complete StoneBlueprint]
        E_Full -->|Call SDK: SaveStone| SDK[StoneCutterClient SDK]
        SDK -->|HTTP POST /api/stones| BE[Backend Server Database]
        E_Full -->|Simulate locally| SS[(StoneServer: liveStonesList)]
    end

    subgraph MARKETPLAY [3. Marketplace Discovery]
        SS -->|Retrieve live stones| SMM[StoneMarketManager]
        SO[(Scriptable Objects: campaign stones)] -->|Retrieve static stones| SMM
        SMM -->|Populate grid cards| UI_Card[StoneItemUI Cards]
        UI_Card -->|Accept Click| GS[GlobalStoneData.CurrentBlueprint]
    end

    subgraph GAMEPLAY [4. Gameplay Spawning & Control]
        GS -->|Load scene| SG[StoneGenerator]
        SG -->|Mesh scale & materials| MainMesh[Stone Game Object]
        SG -->|Core scaling & color| JadeCore[Inner Jade Core]
        SG -->|Spawn anchors| Anchors[Target Anchors]
        GS -->|Pass movement rules| SC[StoneSpinController]
        SC -->|Iterate timeline steps| ActiveSeq[Active TimeStep Motion]
        SC -->|Apply noise & drift| Adv[Adversity Jitter]
    end

    subgraph RESOLUTION [5. Scoring & Leaderboard]
        SG -->|All anchors hit + final chisel cut| Win[Game Won]
        SG -->|Strikes exceed maxStrikes| Lose[Game Over]
        Win -->|Retrieve challenge_points| DM[DataManager]
        DM -->|AddPoints| Balance[Update UserProfileData]
        Balance -->|HTTP POST /api/profiles| BE
        BE -->|HTTP GET /api/leaderboard| LM[LeaderboardManager]
        LM -->|Update UI grid| UI_Leader[Leaderboard UI]
    end
```

---

## 2. MVC Architectural Breakdown

### 2.1 Models (Data & State)
*   **`StoneBlueprint`**: The primary data container representing the full physics, visual properties, and GDD rules of a stone.
*   **`StoneChallengeData`**: Holds GDD motion rules, traps, constraints, wagers, and difficulty profiles defined during stone prediction.
*   **`CurrentStoneModel`**: ScriptableObject containing the currently active SDK stone record (`StoneData`) and its parsed `StoneBlueprint`.
*   **`StoneServer`**: A singleton that currently simulates the backend in-memory, storing a list of dynamically generated stones (`liveStonesList`) and synchronizing the selected game mode (Modern vs. Classic).
*   **`DataManager`**: Persists global points, tier progression, and player preferences (currently using local `PlayerPrefs`).
*   **`UserProfileData`**: Holds profile statistics, tiers, and user career details (perfect/failed cuts).

### 2.2 Controllers (Business Logic & Flow)
*   **`PredictorController`**: Connects the UI configurations to the `StoneCutterClient` SDK. It converts high-level parameters into a unified JSON context and pushes it to the server.
*   **`StoneMarketManager`**: Orchestrates marketplace listings. It fetches both standard campaign stones (Scriptable Objects) and custom, live-generated player stones from the server.
*   **`LeaderboardManager`**: Handles leaderboard fetching and maps rankings to the UI.
*   **`WinLoseManager`**: Manages win/loss panels and handles scene navigation between cutting rooms and the main menu.

### 2.3 Views (Visual Renderers & UI Listeners)
*   **`StoneSpawner` & `StoneGenerator`**: Interpret the blueprint properties to spawn physical 3D stones, configure shaders (jade color, emission), scale bounds, and instantiate anchor points.
*   **`StoneSpinController`**: Handles the time-series sequence translation. It runs a timer and updates physical positions (oscillations, linear movement, chaotic rotations) and applies jitter.
*   **`PredictorUIManager`**: Captures user configurations, runs numpad wagers, and formats custom movement steps.
*   **`StoneItemUI`**: Renderers for marketplace item cards. Captures selection inputs to route players to Classic or Modern scenes.

---

## 3. Data Schema & Payloads (Backend Developer Reference)

For the backend to fully support this architecture, it must store and transmit the following models. Currently, the client packs the entire **`StoneBlueprint`** structure inside the `jsonContext` field of the existing `StoneData` database table.

### 3.1 Stone Blueprint (`StoneBlueprint`)
The base payload containing the stone's physical, visual, and behavioral characteristics.

```json
{
  "stone_uid": "7876d75c-bfde-4b11-bdfc-8828b4c5ee49",
  "challenge_points": 8500,
  "total_weight_kg": 24,
  "stone_icon_index": 2,
  "stone_size_label": "Medium",
  "adversity_level": "Medium",
  "physics_and_material": {
    "size_scale": 1.0,
    "density": "Normal",
    "stress": "Medium",
    "fracture_tolerance": "Normal"
  },
  "rotation_system": {
    "speed": 60.0,
    "rotation_angle": 45.0,
    "rotation_pattern": "LeftToRight",
    "spin_speed": "Normal"
  },
  "anchor_network": {
    "type": "Grounded",
    "point_count": 4
  },
  "jade_core": {
    "color_rating": "50C878",
    "quantity_mass": 3
  },
  "predictor_challenge_data": {
    "predictorId": "user_maya_99",
    "deterministicSeed": 45892,
    "reputationScore": 4.5,
    "minimumSkillRequired": "Cutter",
    "difficultyMultiplier": 1.5,
    "coreMovement": "Oscillation",
    "rotationPattern": "Circular",
    "loopBehavior": "PingPong",
    "challengeDuration": 30,
    "speedMode": "ManualSlider",
    "manualSpeedSlider": 60.0,
    "jitterAmount": 15.0,
    "driftSpeed": 0.5,
    "loopOffset": {
      "x": 0.0,
      "y": 1.0,
      "z": 0.0
    },
    "globalTrap": "FalseSymmetry",
    "phaseTimeline": [
      {
        "startTime": 0.0,
        "duration": 15.0,
        "phaseMovement": "Oscillation",
        "phasePattern": "Circular",
        "trapType": "None",
        "phaseSpeedMultiplier": 1.0
      },
      {
        "startTime": 15.0,
        "duration": 15.0,
        "phaseMovement": "Chaotic",
        "phasePattern": "None",
        "trapType": "SpeedDeception",
        "phaseSpeedMultiplier": 2.0
      }
    ],
    "movementSequence": [
      {
        "duration": 5.0,
        "movementPattern": "Linear"
      },
      {
        "duration": 10.0,
        "movementPattern": "Oscillation"
      }
    ],
    "maxStrikesAllowed": 3,
    "commitFreezeTime": 2.0,
    "allowedTorchPeeks": 3,
    "scoreDecayRate": 50.0,
    "targetScoreToWin": 5000.0,
    "targetStoneSize": "Medium",
    "wagerAmount": 500,
    "isFairnessValidated": true
  }
}
```

### 3.2 User Profile (`UserProfileData`)
Transmitted when loading player states, upgrading levels, or finishing challenges.

```json
{
  "userId": "usr_948194",
  "userName": "Maya",
  "avatarUrl": "https://cdn.stonecutter.io/avatars/maya.png",
  "totalPoints": 45000,
  "currentTier": 3,
  "currentPoints": 12897,
  "maxPointsForTier": 20000,
  "stonesPlayed": 1245,
  "perfectCuts": 426,
  "failedCuts": 127,
  "tierStatus": "Gold"
}
```

### 3.3 Leaderboard Entry (`LeaderboardPlayer`)
Returned as lists for leaderboard updates.

```json
{
  "rank": 3,
  "playerName": "Md. Gazi Fahim",
  "tier": "Gold",
  "points": 117500,
  "avatarUrl": "https://cdn.stonecutter.io/avatars/fahim.png"
}
```

---

## 4. Stone Generation Rules (Gameplay Configurations)

When the backend generates or validates a stone, it must implement these specific matching rules to ensure consistent client rendering:

### 4.1 Physical Bounds Scaling
The raw size scale affects the outer shell mesh dimensions. In the Unity Engine, sizes map to local scale multipliers:
*   **`Small` / `Tiny`**: Scales the mesh to **`0.5x`** (or base `0.010f` local scale).
*   **`Medium`**: Scales the mesh to **`0.75x`** (or base `0.015f` local scale).
*   **`Large`**: Scales the mesh to **`1.0x`** (or base `0.020f` local scale).

### 4.2 Points Reward Bounds (Fair Economy)
Stones have automatically calculated challenge rewards based on their size categorization:
*   **`Small` / `Tiny`** (Size < 3,000 units): Yields random reward between **2,000 and 4,000 points**.
*   **`Medium`** (Size between 3,000 and 9,000 units): Yields random reward between **4,000 and 7,000 points**.
*   **`Large`** (Size > 9,000 units): Yields random reward between **7,000 and 10,000 points**.

### 4.3 Jade Core Scaling
The internal jade mesh scale is calculated relative to the quantity of core jade pieces to give a visual representation of the core mass:
*   **`quantity_mass` >= 5**: Core scale ratio is **`0.95x`** of the outer stone shell.
*   **`quantity_mass` >= 3**: Core scale ratio is **`0.85x`** of the outer stone shell.
*   **`quantity_mass` < 3**: Core scale ratio is **`0.70x`** of the outer stone shell.

---

## 5. API Expansion Requirements

To implement a full server backend, the developer should expand the client SDK with the following REST routes and structures:

### 5.1 Game Mode Synchronization
*   **`GET /api/users/profile`**: Loads current profile data (`UserProfileData`).
*   **`POST /api/users/theme`**: Persists active theme choice (`Modern` vs `Classic`).

### 5.2 Stone Market Operations
*   **`GET /api/market/stones`**: Returns a paged list of active marketplace stones. Must return standard system-generated challenge stones mixed with player-generated (Predictor) stones.
*   **`POST /api/stones`**: Saves the `StoneBlueprint` context.

### 5.3 Transactional Points & Scoring
*   **`POST /api/market/accept`**: Verifies if the user has enough points to fulfill the `wagerAmount` of the selected stone. Deducts points securely on the server.
*   **`POST /api/challenge/complete`**: Sent on successful completion of a cut. The backend must validate the completion parameters (seed check, strikes count) and add the `challenge_points` to the user's total database balance.

### 5.4 Leaderboards
*   **`GET /api/leaderboard`**: Returns global player ranking lists sorted by point values.

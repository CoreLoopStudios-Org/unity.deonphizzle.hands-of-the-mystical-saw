# 📋 Variable List (Only Data Fields)

This document provides a flat reference list of all data variables, fields, and types used in the communication between the Unity client (Predictor/Player Mode) and the Backend Server.

---

### 1. SDK Save & Indexing Parameters
*   **`name`** (`string`): Display name of the stone.
*   **`sdkSize`** (`int` Enum): Stone size tier (0 = Small, 1 = Medium, 2 = Large).
*   **`sdkColorHex`** (`string`): Hexadecimal color code of the jade core.
*   **`sdkJadeCount`** (`int`): Count of internal jade core pieces (1, 3, 5).
*   **`jsonContext`** (`string`): Serialized JSON context of the full `StoneBlueprint`.

---

### 2. Stone Blueprint Fields (`jsonContext`)
*   **`stone_uid`** (`string`): Globally unique identifier for the stone.
*   **`challenge_points`** (`int`): Point reward for successfully cutting the stone.
*   **`total_weight_kg`** (`int`): Visual weight of the stone displayed on market card.
*   **`stone_icon_index`** (`int`): Sprite index of the stone card icon.
*   **`stone_size_label`** (`string`): Size classification text ("Small", "Medium", "Large").
*   **`adversity_level`** (`string`): Mapped difficulty adversity tier ("Low", "Medium", "High").

#### A. Physical and Material Properties (`physics_and_material`)
*   **`size_scale`** (`float`): Local 3D scale multiplier.
*   **`density`** (`string`): Material weight classification ("Light", "Normal", "Heavy").
*   **`stress`** (`string`): Internal physical stress tier.
*   **`fracture_tolerance`** (`string`): Crack/fracture resistance tier.

#### B. Rotation System (`rotation_system`)
*   **`speed`** (`float`): Rotation velocity value.
*   **`rotation_angle`** (`float`): Angle of rotation axis in degrees.
*   **`rotation_pattern`** (`string`): Direction pattern ("LeftToRight", "RightToLeft").
*   **`spin_speed`** (`string`): Speed scaling setting ("Slow", "Fast").

#### C. Anchor Network (`anchor_network`)
*   **`type`** (`string`): Structural placement style ("Free", "Grounded", "WallAttached").
*   **`point_count`** (`int`): Count of targets/anchor points required to be hit.

#### D. Jade Core (`jade_core`)
*   **`color_rating`** (`string`): Color hex code used to paint the inner jade mesh.
*   **`quantity_mass`** (`int`): Jade count rating.

#### E. Predictor Challenge Data (`predictor_challenge_data`)
*   **`predictorId`** (`string`): Player ID of the creator.
*   **`deterministicSeed`** (`int`): Fixed seed for deterministic physics replay.
*   **`reputationScore`** (`float`): Rating/reputation score of the creator.
*   **`minimumSkillRequired`** (`int` Enum): Skill tier constraint (Initiate, Cutter, Carver, etc.).
*   **`difficultyMultiplier`** (`float`): Score payout scaling index.
*   **`coreMovement`** (`int` Enum): Mapped translation type (Static, Linear, Oscillation, etc.).
*   **`rotationPattern`** (`int` Enum): Rotational motion type (None, Circular, Chaotic, etc.).
*   **`loopBehavior`** (`int` Enum): Loop pattern (Continuous, Ping-Pong, etc.).
*   **`challengeDuration`** (`int` Enum): Timer threshold in seconds (10, 15, 30, 60, 90, 120).
*   **`speedMode`** (`int` Enum): Speed controller configuration.
*   **`manualSpeedSlider`** (`float`): Selected speed value.
*   **`speedCurve`** (`AnimationCurve`): Custom acceleration/deceleration graph.
*   **`jitterAmount`** (`float`): Vibration/shake intensity.
*   **`driftSpeed`** (`float`): Translational drift velocity.
*   **`loopOffset`** (`Vector3`): Vector3 offset parameters (`x`, `y`, `z`).
*   **`globalTrap`** (`int` Enum): Mapped decoy gameplay trap type.
*   **`maxStrikesAllowed`** (`int`): Maximum count of failed cuts before failure.
*   **`commitFreezeTime`** (`float`): Input locking timeframe.
*   **`allowedTorchPeeks`** (`int`): Total scan peeks allowed.
*   **`scoreDecayRate`** (`float`): Payout reduction rate per second of scanning.
*   **`targetScoreToWin`** (`float`): Target score required to pass.
*   **`targetStoneSize`** (`int` Enum): Mapped size designation (0 = Small, 1 = Medium, 2 = Large).
*   **`wagerAmount`** (`int`): Points staked by the predictor.
*   **`isFairnessValidated`** (`bool`): Solvability validation flag.
*   **`phaseTimeline`** (`List<PhaseBehavior>`): List of behaviors containing:
    *   `startTime` (`float`)
    *   `duration` (`float`)
    *   `phaseMovement` (`int` Enum)
    *   `phasePattern` (`int` Enum)
    *   `trapType` (`int` Enum)
    *   `phaseSpeedMultiplier` (`float`)
*   **`movementSequence`** (`List<TimeStepData>`): Custom time steps list containing:
    *   `duration` (`float`)
    *   `movementPattern` (`string`)

---

### 3. Server Response Stone Data (`StoneData`)
*   **`Id`** (`int`): Primary database identifier.
*   **`Name`** (`string`): Custom title of the stone.
*   **`StoneSize`** (`int` Enum): Stone sizing classification.
*   **`JadeCount`** (`int`): Amount of jade pieces.
*   **`JsonContext`** (`string`): The full serialized `StoneBlueprint` context.

---

### 4. User Profile Data (`UserProfileData`)
*   **`userId`** (`string`): Unique identifier of the player.
*   **`userName`** (`string`): Player username display name.
*   **`avatarUrl`** (`string`): Remote asset URL of the avatar image.
*   **`totalPoints`** (`int`): Total account point balance.
*   **`currentTier`** (`int`): Active user tier index number.
*   **`currentPoints`** (`int`): Points accumulated in the current tier.
*   **`maxPointsForTier`** (`int`): Progress goal for next tier.
*   **`stonesPlayed`** (`int`): Lifetime challenges played count.
*   **`perfectCuts`** (`int`): Matches completed with zero strikes.
*   **`failedCuts`** (`int`): Total matches failed (strikes limit exceeded).
*   **`tierStatus`** (`string`): Display name of the user tier status (e.g., "Gold").

---

### 5. Leaderboard Entry (`LeaderboardPlayer`)
*   **`rank`** (`int`): Leaderboard relative position index.
*   **`playerName`** (`string`): Player display name.
*   **`tier`** (`string`): Display status name of player's tier.
*   **`points`** (`int`): Score used for ranking.
*   **`avatarUrl`** (`string`): Avatar image link.

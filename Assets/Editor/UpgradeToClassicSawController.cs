#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeToClassicSawController : EditorWindow
{
    [MenuItem("Tools/Assign Classic Saw Controller")]
    public static void UpgradeSaw()
    {
        // Try to find the classic saw GameObject in the active scene
        GameObject classicSaw = GameObject.Find("Saw_rigged -newclassic");
        if (classicSaw == null)
        {
            classicSaw = GameObject.Find("Saw_rigged");
        }

        if (classicSaw == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find 'Saw_rigged -newclassic' or 'Saw_rigged' in the current scene.", "OK");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Assign Classic Saw Controller");
        int groupIndex = Undo.GetCurrentGroup();

        // 1. Get the current SawArmController component
        SawArmController oldController = classicSaw.GetComponent<SawArmController>();
        if (oldController == null)
        {
            ClassicSawController existing = classicSaw.GetComponent<ClassicSawController>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Notice", "ClassicSawController is already attached to this saw.", "OK");
                return;
            }

            ClassicSawController newController = Undo.AddComponent<ClassicSawController>(classicSaw);
            Undo.CollapseUndoOperations(groupIndex);
            EditorUtility.DisplayDialog("Success", "Added new ClassicSawController to GameObject.", "OK");
            return;
        }

        // 2. Cache all the values from the old SawArmController component
        bool isEquipped = oldController.isEquipped;
        VirtualJoystick virtualJoystick = oldController.virtualJoystick;
        Button forwardButton = oldController.forwardButton;
        Button backwardButton = oldController.backwardButton;
        Transform rootBone = oldController.rootBone;
        Transform upDownBone = oldController.upDownBone;
        Transform extendBone = oldController.extendBone;
        Transform sawBlade = oldController.sawBlade;
        float bladeSpinSpeed = oldController.bladeSpinSpeed;
        Vector3 spinAxis = oldController.spinAxis;
        Vector3 rootRotationAxis = oldController.rootRotationAxis;
        float rootTurnSpeed = oldController.rootTurnSpeed;
        float minRootAngle = oldController.minRootAngle;
        float maxRootAngle = oldController.maxRootAngle;
        Vector3 tiltRotationAxis = oldController.tiltRotationAxis;
        float tiltSpeed = oldController.tiltSpeed;
        float minTiltZ = oldController.minTiltZ;
        float maxTiltZ = oldController.maxTiltZ;
        bool invertJoystickY = oldController.invertJoystickY;
        bool invertJoystickX = oldController.invertJoystickX;
        float extendSpeed = oldController.extendSpeed;
        float maxForwardDistance = oldController.maxForwardDistance;
        float maxBackwardDistance = oldController.maxBackwardDistance;
        Vector3 extendAxis = oldController.extendAxis;
        Vector3 collisionCheckAxis = oldController.collisionCheckAxis;
        float bladeRadius = oldController.bladeRadius;
        LayerMask stoneLayer = oldController.stoneLayer;
        GameObject sawCutMarkPrefab = oldController.sawCutMarkPrefab;
        float grindInterval = oldController.grindInterval;
        Vector3 bladeCutNormal = oldController.bladeCutNormal;
        float sliceCooldown = oldController.sliceCooldown;
        Material crossSectionMaterial = oldController.crossSectionMaterial;
        Material jadeCrossSectionMaterial = oldController.jadeCrossSectionMaterial;
        ParticleSystem sparksParticle = oldController.sparksParticle;
        ParticleSystem waterEffectParticle = oldController.waterEffectParticle;
        AudioSource sawAudioSource = oldController.sawAudioSource;
        AudioClip sawingSound = oldController.sawingSound;
        AudioClip sliceSound = oldController.sliceSound;

        // 3. Remove the old SawArmController component
        Undo.DestroyObjectImmediate(oldController);

        // 4. Add the new ClassicSawController component
        ClassicSawController newCtrl = Undo.AddComponent<ClassicSawController>(classicSaw);

        // 5. Restore all cached values
        newCtrl.isEquipped = isEquipped;
        newCtrl.virtualJoystick = virtualJoystick;
        newCtrl.forwardButton = forwardButton;
        newCtrl.backwardButton = backwardButton;
        newCtrl.rootBone = rootBone;
        newCtrl.upDownBone = upDownBone;
        newCtrl.extendBone = extendBone;
        newCtrl.sawBlade = sawBlade;
        newCtrl.bladeSpinSpeed = bladeSpinSpeed;
        newCtrl.spinAxis = spinAxis;
        newCtrl.rootRotationAxis = rootRotationAxis;
        newCtrl.rootTurnSpeed = rootTurnSpeed;
        newCtrl.minRootAngle = minRootAngle;
        newCtrl.maxRootAngle = maxRootAngle;
        newCtrl.tiltRotationAxis = tiltRotationAxis;
        newCtrl.tiltSpeed = tiltSpeed;
        newCtrl.minTiltZ = minTiltZ;
        newCtrl.maxTiltZ = maxTiltZ;
        newCtrl.invertJoystickY = invertJoystickY;
        newCtrl.invertJoystickX = invertJoystickX;
        newCtrl.extendSpeed = extendSpeed;
        newCtrl.maxForwardDistance = maxForwardDistance;
        newCtrl.maxBackwardDistance = maxBackwardDistance;
        newCtrl.extendAxis = extendAxis;
        newCtrl.collisionCheckAxis = collisionCheckAxis;
        newCtrl.bladeRadius = bladeRadius;
        newCtrl.stoneLayer = stoneLayer;
        newCtrl.sawCutMarkPrefab = sawCutMarkPrefab;
        newCtrl.grindInterval = grindInterval;
        newCtrl.bladeCutNormal = bladeCutNormal;
        newCtrl.sliceCooldown = sliceCooldown;
        newCtrl.crossSectionMaterial = crossSectionMaterial;
        newCtrl.jadeCrossSectionMaterial = jadeCrossSectionMaterial;
        newCtrl.sparksParticle = sparksParticle;
        newCtrl.waterEffectParticle = waterEffectParticle;
        newCtrl.sawAudioSource = sawAudioSource;
        newCtrl.sawingSound = sawingSound;
        newCtrl.sliceSound = sliceSound;

        // 6. Force classic specific values just in case
        newCtrl.rootRotationAxis = new Vector3(1, 0, 0);
        newCtrl.tiltRotationAxis = new Vector3(0, 0, 1);
        newCtrl.extendAxis = new Vector3(-1, 0, 0);
        newCtrl.invertJoystickX = true;
        newCtrl.invertJoystickY = false;

        // Mark scene dirty so it prompts to save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Undo.CollapseUndoOperations(groupIndex);

        EditorUtility.DisplayDialog("Success", "Successfully swapped SawArmController to ClassicSawController while preserving all values!\n\nPlease save the scene (Ctrl+S).", "OK");
    }
}
#endif

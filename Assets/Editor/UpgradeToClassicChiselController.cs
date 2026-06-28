#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class UpgradeToClassicChiselController : EditorWindow
{
    [MenuItem("Tools/Assign Classic Chisel Controller")]
    public static void UpgradeChisel()
    {
        // Try to find the classic chisel GameObject in the scene
        GameObject classicChisel = GameObject.Find("Chissel_classic_rigged-");
        if (classicChisel == null)
        {
            classicChisel = GameObject.Find("Chissel_classic_rigged");
        }

        if (classicChisel == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find 'Chissel_classic_rigged-' or 'Chissel_classic_rigged' in the current scene.", "OK");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Assign Classic Chisel Controller");
        int groupIndex = Undo.GetCurrentGroup();

        // 1. Get the current ManualChiselController component if it exists
        ManualChiselController oldController = classicChisel.GetComponent<ManualChiselController>();
        if (oldController == null)
        {
            // If it doesn't have ManualChiselController, maybe it already has ClassicChiselController?
            ClassicChiselController existing = classicChisel.GetComponent<ClassicChiselController>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Notice", "ClassicChiselController is already attached to the object.", "OK");
                return;
            }
            
            // Otherwise, we can just add a new ClassicChiselController component
            ClassicChiselController newController = Undo.AddComponent<ClassicChiselController>(classicChisel);
            Undo.CollapseUndoOperations(groupIndex);
            EditorUtility.DisplayDialog("Success", "Added new ClassicChiselController to GameObject.", "OK");
            return;
        }

        // 2. Cache all the values from the old ManualChiselController component
        bool isEquipped = oldController.isEquipped;
        VirtualJoystick joystick = oldController.joystick;
        Transform rootBone = oldController.rootBone;
        Transform tiltBone = oldController.tiltBone;
        Transform extendBone = oldController.extendBone;
        Transform chiselTip = oldController.chiselTip;
        float baseTurnSpeed = oldController.baseTurnSpeed;
        Vector3 baseRotationAxis = oldController.baseRotationAxis;
        float headAimSpeed = oldController.headAimSpeed;
        float minTiltUp = oldController.minTiltUp;
        float maxTiltUp = oldController.maxTiltUp;
        float minTiltSide = oldController.minTiltSide;
        float maxTiltSide = oldController.maxTiltSide;
        bool swapJoystickAxes = oldController.swapJoystickAxes;
        float maxExtensionDistance = oldController.maxExtensionDistance;
        float hitSpeed = oldController.hitSpeed;
        float returnSpeed = oldController.returnSpeed;
        Vector3 strikeAxis = oldController.strikeAxis;
        float hitSoundVolume = oldController.hitSoundVolume;
        GameObject hitEffectPrefab = oldController.hitEffectPrefab;
        Vector3 particleRotationOffset = oldController.particleRotationOffset;
        AudioClip primaryHitSound = oldController.primaryHitSound;
        GameObject secondaryHitEffectPrefab = oldController.secondaryHitEffectPrefab;
        Vector3 secondaryParticleRotationOffset = oldController.secondaryParticleRotationOffset;
        AudioClip secondaryHitSound = oldController.secondaryHitSound;

        // 3. Remove the old ManualChiselController component
        Undo.DestroyObjectImmediate(oldController);

        // 4. Add the new ClassicChiselController component
        ClassicChiselController newCtrl = Undo.AddComponent<ClassicChiselController>(classicChisel);

        // 5. Restore all cached values
        newCtrl.isEquipped = isEquipped;
        newCtrl.joystick = joystick;
        newCtrl.rootBone = rootBone;
        newCtrl.tiltBone = tiltBone;
        newCtrl.extendBone = extendBone;
        newCtrl.chiselTip = chiselTip;
        newCtrl.baseTurnSpeed = baseTurnSpeed;
        newCtrl.baseRotationAxis = baseRotationAxis;
        newCtrl.headAimSpeed = headAimSpeed;
        newCtrl.minTiltUp = minTiltUp;
        newCtrl.maxTiltUp = maxTiltUp;
        newCtrl.minTiltSide = minTiltSide;
        newCtrl.maxTiltSide = maxTiltSide;
        newCtrl.swapJoystickAxes = swapJoystickAxes;
        newCtrl.maxExtensionDistance = maxExtensionDistance;
        newCtrl.hitSpeed = hitSpeed;
        newCtrl.returnSpeed = returnSpeed;
        newCtrl.strikeAxis = strikeAxis;
        newCtrl.hitSoundVolume = hitSoundVolume;
        newCtrl.hitEffectPrefab = hitEffectPrefab;
        newCtrl.particleRotationOffset = particleRotationOffset;
        newCtrl.primaryHitSound = primaryHitSound;
        newCtrl.secondaryHitEffectPrefab = secondaryHitEffectPrefab;
        newCtrl.secondaryParticleRotationOffset = secondaryParticleRotationOffset;
        newCtrl.secondaryHitSound = secondaryHitSound;

        // 6. Force classic specific values just in case
        newCtrl.invertVertical = true;
        newCtrl.invertHorizontal = true;
        newCtrl.rootTurnAxis = ClassicChiselController.ControlAxis.X;
        newCtrl.tiltTurnAxis = ClassicChiselController.ControlAxis.Z;

        // Mark scene dirty so it prompts to save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Undo.CollapseUndoOperations(groupIndex);

        EditorUtility.DisplayDialog("Success", "Successfully swapped ManualChiselController to ClassicChiselController while preserving all values!\n\nPlease save the scene (Ctrl+S).", "OK");
    }
}
#endif

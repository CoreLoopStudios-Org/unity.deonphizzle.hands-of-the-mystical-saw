#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to automatically bind missing audio clips (Hammer hit, Chisel hit, Dremel hit, Saw loop, Saw slice) 
/// to the respective tool scripts in all gameplay scenes.
/// </summary>
public class FixToolSounds : MonoBehaviour
{
    private const string HammerHitSoundPath = "Assets/Sprites/Audio/Hammer/StoneHitSound.wav";
    private const string ChiselHitSoundPath = "Assets/Sprites/Audio/Chisel/chisel.wav";
    private const string SawingSoundPath = "Assets/Sprites/Audio/sawing saund.mp3";
    private const string SliceSoundPath = "Assets/Sprites/Audio/sawaudiosource.wav";
    private const string DrillingSoundPath = "Assets/Sprites/Audio/Dramel/Drilling-SSound.wav";

    private static readonly string[] GameplayScenes = new string[]
    {
        "Assets/ALL-SCENE-IS HERE/StoneCuttingScene_Classic.unity",
        "Assets/ALL-SCENE-IS HERE/StoneGenerator Scene.unity",
        "Assets/ALL-SCENE-IS HERE/PredictorScene.unity"
    };

    [MenuItem("Tools/Fix Scene Tool Sounds")]
    public static void FixSounds()
    {
        // Load Audio Assets
        AudioClip hammerClip = AssetDatabase.LoadAssetAtPath<AudioClip>(HammerHitSoundPath);
        AudioClip chiselClip = AssetDatabase.LoadAssetAtPath<AudioClip>(ChiselHitSoundPath);
        AudioClip sawingClip = AssetDatabase.LoadAssetAtPath<AudioClip>(SawingSoundPath);
        AudioClip sliceClip = AssetDatabase.LoadAssetAtPath<AudioClip>(SliceSoundPath);
        AudioClip drillingClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DrillingSoundPath);

        if (hammerClip == null) Debug.LogWarning($"[Fix Sounds] Hammer sound asset not found at: {HammerHitSoundPath}");
        if (chiselClip == null) Debug.LogWarning($"[Fix Sounds] Chisel sound asset not found at: {ChiselHitSoundPath}");
        if (sawingClip == null) Debug.LogWarning($"[Fix Sounds] Sawing loop sound asset not found at: {SawingSoundPath}");
        if (sliceClip == null) Debug.LogWarning($"[Fix Sounds] Slice sound asset not found at: {SliceSoundPath}");
        if (drillingClip == null) Debug.LogWarning($"[Fix Sounds] Drilling sound asset not found at: {DrillingSoundPath}");

        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        bool anyModified = false;

        foreach (string scenePath in GameplayScenes)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning($"[Fix Sounds] Scene file not found at: {scenePath}");
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneDirty = false;

            // 1. Fix NewHammerControllers (Hammer)
            NewHammerController[] hammers = GameObject.FindObjectsByType<NewHammerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var hammer in hammers)
            {
                if (hammer.primaryHitSound == null && hammerClip != null)
                {
                    hammer.primaryHitSound = hammerClip;
                    EditorUtility.SetDirty(hammer);
                    sceneDirty = true;
                    Debug.Log($"[Fix Sounds] Fixed NewHammerController (primaryHitSound) in scene: {scenePath}");
                }
            }

            // 2. Fix Chisel Controllers (ManualChiselController, ClassicChiselController, ChiselController)
            ManualChiselController[] manualChisels = GameObject.FindObjectsByType<ManualChiselController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var chisel in manualChisels)
            {
                if (chisel.primaryHitSound == null && chiselClip != null)
                {
                    chisel.primaryHitSound = chiselClip;
                    EditorUtility.SetDirty(chisel);
                    sceneDirty = true;
                    Debug.Log($"[Fix Sounds] Fixed ManualChiselController (primaryHitSound) in scene: {scenePath}");
                }
            }

            ClassicChiselController[] classicChisels = GameObject.FindObjectsByType<ClassicChiselController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var chisel in classicChisels)
            {
                if (chisel.primaryHitSound == null && chiselClip != null)
                {
                    chisel.primaryHitSound = chiselClip;
                    EditorUtility.SetDirty(chisel);
                    sceneDirty = true;
                    Debug.Log($"[Fix Sounds] Fixed ClassicChiselController (primaryHitSound) in scene: {scenePath}");
                }
            }

            ChiselController[] generalChisels = GameObject.FindObjectsByType<ChiselController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var chisel in generalChisels)
            {
                if (chisel.primaryHitSound == null && chiselClip != null)
                {
                    chisel.primaryHitSound = chiselClip;
                    EditorUtility.SetDirty(chisel);
                    sceneDirty = true;
                    Debug.Log($"[Fix Sounds] Fixed ChiselController (primaryHitSound) in scene: {scenePath}");
                }
            }

            // 3. Fix Dremel Controllers
            DremelToolController[] dremels = GameObject.FindObjectsByType<DremelToolController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var dremel in dremels)
            {
                if (dremel.primaryHitSound != drillingClip && drillingClip != null)
                {
                    dremel.primaryHitSound = drillingClip;
                    EditorUtility.SetDirty(dremel);
                    sceneDirty = true;
                    Debug.Log($"[Fix Sounds] Assigned Drilling-SSound to DremelToolController in scene: {scenePath}");
                }
            }

            // 4. Fix Saw Controllers (ClassicSawController, SawArmController, SawToolController)
            ClassicSawController[] classicSaws = GameObject.FindObjectsByType<ClassicSawController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var saw in classicSaws)
            {
                bool sawModified = false;
                if (saw.sawingSound == null && sawingClip != null) { saw.sawingSound = sawingClip; sawModified = true; }
                if (saw.sliceSound == null && sliceClip != null) { saw.sliceSound = sliceClip; sawModified = true; }
                if (sawModified)
                {
                    EditorUtility.SetDirty(saw);
                    sceneDirty = true;
                    Debug.Log($"[Fix Sounds] Fixed ClassicSawController sounds in scene: {scenePath}");
                }
            }

            SawArmController[] sawArms = GameObject.FindObjectsByType<SawArmController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var saw in sawArms)
            {
                bool sawModified = false;
                if (saw.sawingSound == null && sawingClip != null) { saw.sawingSound = sawingClip; sawModified = true; }
                if (saw.sliceSound == null && sliceClip != null) { saw.sliceSound = sliceClip; sawModified = true; }
                if (sawModified)
                {
                    EditorUtility.SetDirty(saw);
                    sceneDirty = true;
                    Debug.Log($"[Fix Sounds] Fixed SawArmController sounds in scene: {scenePath}");
                }
            }

            SawToolController[] sawTools = GameObject.FindObjectsByType<SawToolController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var saw in sawTools)
            {
                bool sawModified = false;
                if (saw.sawingSound == null && sawingClip != null) { saw.sawingSound = sawingClip; sawModified = true; }
                if (saw.sliceSound == null && sliceClip != null) { saw.sliceSound = sliceClip; sawModified = true; }
                if (sawModified)
                {
                    EditorUtility.SetDirty(saw);
                    sceneDirty = true;
                    Debug.Log($"[Fix Sounds] Fixed SawToolController sounds in scene: {scenePath}");
                }
            }

            if (sceneDirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                anyModified = true;
            }
        }

        // Restore original scene
        if (!string.IsNullOrEmpty(originalScenePath) && System.IO.File.Exists(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        if (anyModified)
        {
            EditorUtility.DisplayDialog("Success", "All tool sounds have been configured and scenes saved successfully!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Notice", "All tool sounds are already configured in all scenes.", "OK");
        }
    }

    [MenuItem("Tools/Analyze Old Tool Sounds")]
    public static void AnalyzeOldSounds()
    {
        string scenePath = "Assets/ALL-SCENE-IS HERE/StoneGenerator Scene.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            EditorUtility.DisplayDialog("Error", $"StoneGenerator Scene not found at {scenePath}", "OK");
            return;
        }

        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("=== OLD TOOL SOUNDS ANALYSIS ===\n");

        // Find all objects in scene (including inactive ones)
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        bool foundAny = false;

        foreach (var go in allObjects)
        {
            // Look for objects containing "OLD", "PREVIOUS", or "classic"
            if (go.scene.isLoaded && 
                (go.name.Contains("OLD") || go.name.Contains("PREVIOUS") || go.name.Contains("classic")))
            {
                foundAny = true;
                report.AppendLine($"GameObject: {go.name} (Active: {go.activeSelf})");

                // Check for AudioSource
                AudioSource[] sources = go.GetComponentsInChildren<AudioSource>(true);
                foreach (var source in sources)
                {
                    report.AppendLine($"  - AudioSource on '{source.gameObject.name}':");
                    report.AppendLine($"    Clip: {(source.clip != null ? source.clip.name : "None")}");
                    report.AppendLine($"    Volume: {source.volume}");
                    report.AppendLine($"    PlayOnAwake: {source.playOnAwake}");
                    report.AppendLine($"    Loop: {source.loop}");
                }

                // Check for MonoBehaviours and their AudioClip fields
                MonoBehaviour[] components = go.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var comp in components)
                {
                    if (comp == null) continue;
                    System.Type type = comp.GetType();
                    
                    // Use reflection to find any AudioClip fields
                    var fields = type.GetFields(System.Reflection.BindingFlags.Public | 
                                                 System.Reflection.BindingFlags.NonPublic | 
                                                 System.Reflection.BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        if (field.FieldType == typeof(AudioClip))
                        {
                            AudioClip clip = (AudioClip)field.GetValue(comp);
                            report.AppendLine($"  - Script '{type.Name}' on '{comp.gameObject.name}':");
                            report.AppendLine($"    Field '{field.Name}': {(clip != null ? clip.name : "None")}");
                        }
                    }
                }
                report.AppendLine();
            }
        }

        // Restore original scene
        if (!string.IsNullOrEmpty(originalScenePath) && System.IO.File.Exists(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        if (!foundAny)
        {
            report.AppendLine("No old/previous/classic tool GameObjects were found in the scene.");
        }

        string message = report.ToString();
        Debug.Log(message);
        EditorUtility.DisplayDialog("Old Tool Sound Analysis", message, "OK");
    }

    [MenuItem("Tools/Apply Old Tool Sounds to Active Tools")]
    public static void ApplyOldSoundsToActive()
    {
        string scenePath = "Assets/ALL-SCENE-IS HERE/StoneGenerator Scene.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            EditorUtility.DisplayDialog("Error", $"StoneGenerator Scene not found at {scenePath}", "OK");
            return;
        }

        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        AudioClip oldChiselPrimaryClip = null;
        AudioClip oldChiselSecondaryClip = null;
        AudioClip oldDremelClip = null;

        // 1. Locate the old tools and harvest their clips
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject oldChiselGo = null;
        GameObject oldDremelGo = null;

        foreach (var go in allObjects)
        {
            if (go.scene.isLoaded)
            {
                if (go.name == "PREVIOUSCHISEL-OLD")
                {
                    oldChiselGo = go;
                }
                // Try to find if there is any other old dremel object
                else if (go.name.Contains("Dramel") && (go.name.Contains("OLD") || go.name.Contains("PREVIOUS") || go.name.Contains("classic") || !go.activeSelf))
                {
                    // Ensure it is not the active modern rigged dremel
                    if (go.name != "Dramel_rigged-modern" && go.name != "DramelController-modern")
                    {
                        oldDremelGo = go;
                    }
                }
            }
        }

        System.Text.StringBuilder log = new System.Text.StringBuilder();
        log.AppendLine("=== SOUND HARVESTING LOG ===\n");

        if (oldChiselGo != null)
        {
            log.AppendLine($"Found Old Chisel: {oldChiselGo.name}");
            
            // Search all components for chisel scripts
            var manual = oldChiselGo.GetComponentInChildren<ManualChiselController>(true);
            if (manual != null)
            {
                oldChiselPrimaryClip = manual.primaryHitSound;
                oldChiselSecondaryClip = manual.secondaryHitSound;
            }
            else
            {
                var classic = oldChiselGo.GetComponentInChildren<ClassicChiselController>(true);
                if (classic != null)
                {
                    oldChiselPrimaryClip = classic.primaryHitSound;
                    oldChiselSecondaryClip = classic.secondaryHitSound;
                }
                else
                {
                    var general = oldChiselGo.GetComponentInChildren<ChiselController>(true);
                    if (general != null)
                    {
                        oldChiselPrimaryClip = general.primaryHitSound;
                        oldChiselSecondaryClip = general.secondaryHitSound;
                    }
                }
            }

            // Fallback: Check AudioSource
            if (oldChiselPrimaryClip == null)
            {
                AudioSource src = oldChiselGo.GetComponentInChildren<AudioSource>(true);
                if (src != null) oldChiselPrimaryClip = src.clip;
            }

            log.AppendLine($"  - Harvested Primary Clip: {(oldChiselPrimaryClip != null ? oldChiselPrimaryClip.name : "None")}");
            log.AppendLine($"  - Harvested Secondary Clip: {(oldChiselSecondaryClip != null ? oldChiselSecondaryClip.name : "None")}");
        }
        else
        {
            log.AppendLine("[-] Old Chisel 'PREVIOUSCHISEL-OLD' NOT found in scene.");
        }

        if (oldDremelGo != null)
        {
            log.AppendLine($"Found Old Dremel: {oldDremelGo.name}");
            var dremelComp = oldDremelGo.GetComponentInChildren<DremelToolController>(true);
            if (dremelComp != null)
            {
                oldDremelClip = dremelComp.primaryHitSound;
            }

            // Fallback: Check AudioSource
            if (oldDremelClip == null)
            {
                AudioSource src = oldDremelGo.GetComponentInChildren<AudioSource>(true);
                if (src != null) oldDremelClip = src.clip;
            }
            log.AppendLine($"  - Harvested Dremel Clip: {(oldDremelClip != null ? oldDremelClip.name : "None")}");
        }
        else
        {
            log.AppendLine("[-] Old Dremel NOT found in scene.");
        }

        log.AppendLine("\n=== APPLYING TO NEW ACTIVE TOOLS ===\n");

        bool modified = false;

        // 2. Find new active tools and apply harvested clips
        GameObject newChiselGo = GameObject.Find("Chisel_rigged -modern");
        if (newChiselGo == null) newChiselGo = GameObject.Find("Chisel_rigged-modern");
        if (newChiselGo != null)
        {
            var newChisel = newChiselGo.GetComponentInChildren<ManualChiselController>(true);
            if (newChisel != null)
            {
                if (oldChiselPrimaryClip != null) { newChisel.primaryHitSound = oldChiselPrimaryClip; modified = true; }
                if (oldChiselSecondaryClip != null) { newChisel.secondaryHitSound = oldChiselSecondaryClip; modified = true; }
                log.AppendLine($"Applied harvested clips to new Chisel: {newChiselGo.name}");
                EditorUtility.SetDirty(newChisel);
            }
        }
        else
        {
            log.AppendLine("[-] New active Chisel GameObject 'Chisel_rigged -modern' NOT found.");
        }

        GameObject newDremelGo = GameObject.Find("Dramel_rigged-modern");
        if (newDremelGo != null)
        {
            // The DremelToolController might be on DramelController-modern (the manager) instead of Dramel_rigged-modern
            var newDremel = newDremelGo.GetComponentInChildren<DremelToolController>(true);
            if (newDremel == null)
            {
                var mgr = GameObject.Find("DramelController-modern");
                if (mgr != null) newDremel = mgr.GetComponentInChildren<DremelToolController>(true);
            }

            if (newDremel != null)
            {
                if (oldDremelClip != null)
                {
                    newDremel.primaryHitSound = oldDremelClip;
                    modified = true;
                    log.AppendLine($"Applied harvested clip to new Dremel: {newDremel.gameObject.name}");
                    EditorUtility.SetDirty(newDremel);
                }
            }
        }
        else
        {
            log.AppendLine("[-] New active Dremel GameObject 'Dramel_rigged-modern' NOT found.");
        }

        if (modified)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            log.AppendLine("\n[+] SUCCESS: Scene saved with new configurations.");
        }
        else
        {
            log.AppendLine("\n[-] No changes were made (either clips were null or active targets were missing).");
        }

        // Restore original scene
        if (!string.IsNullOrEmpty(originalScenePath) && System.IO.File.Exists(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        string reportMsg = log.ToString();
        Debug.Log(reportMsg);
        EditorUtility.DisplayDialog("Sound Copy Status", reportMsg, "OK");
    }
}
#endif

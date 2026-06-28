#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to automate setting up the GameplayBackgroundMusicManager in both gameplay scenes.
/// Runs automatically on compilation via InitializeOnLoad.
/// </summary>
[InitializeOnLoad]
public class SetupGameplayMusic : MonoBehaviour
{
    private const string GameplayMusicAssetPath = "Assets/Sprites/Audio/Game-PlayMusic.mp3";
    private static readonly string[] GameplayScenes = new string[]
    {
        "Assets/ALL-SCENE-IS HERE/StoneCuttingScene_Classic.unity",
        "Assets/ALL-SCENE-IS HERE/StoneGenerator Scene.unity"
    };

    static SetupGameplayMusic()
    {
        // Run auto-setup after compilation has completed and Editor is ready
        EditorApplication.delayCall += AutoSetup;
    }

    private static void AutoSetup()
    {
        // Do not interrupt play mode or if scenes are transitioning
        if (EditorApplication.isPlaying) return;

        bool needsSetup = false;
        string originalScenePath = EditorSceneManager.GetActiveScene().path;

        // Fast check: check if either scene is missing the manager
        foreach (string scenePath in GameplayScenes)
        {
            if (!System.IO.File.Exists(scenePath)) continue;

            // Open the scene to check if the manager exists
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject managerObj = GameObject.Find("GameplayBackgroundMusicManager");
            if (managerObj == null)
            {
                needsSetup = true;
                break;
            }
        }

        // Restore the original active scene
        if (!string.IsNullOrEmpty(originalScenePath) && System.IO.File.Exists(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        // Run setup if any scene is missing the manager
        if (needsSetup)
        {
            Debug.Log("[Gameplay Music Setup] Auto-setup triggered: Configuring missing GameplayBackgroundMusicManager in gameplay scenes.");
            SetupMusicInScenes(false);
        }
    }

    [MenuItem("Tools/Setup Gameplay Music")]
    public static void ManualSetup()
    {
        SetupMusicInScenes(true);
    }

    public static void SetupMusicInScenes(bool showDialog)
    {
        // Load the Audio Clip
        AudioClip musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(GameplayMusicAssetPath);
        if (musicClip == null)
        {
            Debug.LogError($"[Gameplay Music Setup] Could not find Gameplay Music Audio Clip at path: {GameplayMusicAssetPath}");
            if (showDialog) EditorUtility.DisplayDialog("Error", $"Could not find gameplay music file at path:\n{GameplayMusicAssetPath}\n\nPlease verify it exists in the project.", "OK");
            return;
        }

        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        bool anyModified = false;

        foreach (string scenePath in GameplayScenes)
        {
            if (System.IO.File.Exists(scenePath))
            {
                // Open Scene
                Scene activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                
                // --- SETUP GAMEPLAY MUSIC MANAGER ---
                GameObject managerObj = GameObject.Find("GameplayBackgroundMusicManager");
                bool isNewObj = false;
                
                if (managerObj == null)
                {
                    managerObj = new GameObject("GameplayBackgroundMusicManager");
                    isNewObj = true;
                }

                GameplayBackgroundMusicManager manager = managerObj.GetComponent<GameplayBackgroundMusicManager>();
                if (manager == null)
                {
                    manager = managerObj.AddComponent<GameplayBackgroundMusicManager>();
                }

                AudioSource audioSource = managerObj.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = managerObj.AddComponent<AudioSource>();
                }

                // Configure properties
                manager.backgroundMusicClip = musicClip;
                manager.activeScenes = new List<string> { "StoneCuttingScene_Classic", "StoneGenerator Scene" };
                manager.targetVolume = 0.5f;
                manager.fadeDuration = 0.8f;

                audioSource.clip = musicClip;
                audioSource.loop = true;
                audioSource.playOnAwake = false;

                EditorUtility.SetDirty(managerObj);

                // Mark scene dirty and save
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                
                Debug.Log($"[Gameplay Music Setup] Successfully configured manager in scene: {scenePath} (Created GameObject: {isNewObj})");
                anyModified = true;
            }
        }

        // Restore original scene
        if (!string.IsNullOrEmpty(originalScenePath) && System.IO.File.Exists(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        if (anyModified && showDialog)
        {
            EditorUtility.DisplayDialog("Success", "Gameplay Audio Setup completed successfully!\n\nBoth Classic and Modern gameplay scenes have been updated and saved with the GameplayBackgroundMusicManager.", "Awesome");
        }
    }

    [MenuItem("Tools/Verify Gameplay Music Setup")]
    public static void VerifyGameplaySetup()
    {
        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("=== GAMEPLAY AUDIO MANAGERS VERIFICATION REPORT ===\n");

        bool setupCorrectly = true;

        foreach (string scenePath in GameplayScenes)
        {
            report.AppendLine($"Checking Scene: {scenePath}");
            if (!System.IO.File.Exists(scenePath))
            {
                report.AppendLine("  [-] ERROR: Scene file does not exist on disk.");
                setupCorrectly = false;
                continue;
            }

            // Open scene in editor to check contents
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            GameObject managerObj = GameObject.Find("GameplayBackgroundMusicManager");
            if (managerObj == null)
            {
                report.AppendLine("  [-] ERROR: 'GameplayBackgroundMusicManager' GameObject NOT found in hierarchy.");
                setupCorrectly = false;
            }
            else
            {
                GameplayBackgroundMusicManager manager = managerObj.GetComponent<GameplayBackgroundMusicManager>();
                if (manager == null)
                {
                    report.AppendLine("  [-] ERROR: 'GameplayBackgroundMusicManager' component NOT attached.");
                    setupCorrectly = false;
                }
                else
                {
                    report.AppendLine("  [+] SUCCESS: 'GameplayBackgroundMusicManager' component is attached.");
                    if (manager.backgroundMusicClip == null)
                    {
                        report.AppendLine("  [-] ERROR: Music 'backgroundMusicClip' field is NULL (not assigned).");
                        setupCorrectly = false;
                    }
                    else
                    {
                        report.AppendLine($"  [+] SUCCESS: Music 'backgroundMusicClip' is set to: {manager.backgroundMusicClip.name}");
                    }
                }

                AudioSource source = managerObj.GetComponent<AudioSource>();
                if (source == null)
                {
                    report.AppendLine("  [-] ERROR: AudioSource component NOT attached.");
                    setupCorrectly = false;
                }
                else
                {
                    report.AppendLine("  [+] SUCCESS: AudioSource component is attached.");
                }
            }
            report.AppendLine();
        }

        // Restore original scene
        if (!string.IsNullOrEmpty(originalScenePath) && System.IO.File.Exists(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        string resultTitle = setupCorrectly ? "Verification Successful" : "Verification Failed";
        string resultMessage = report.ToString();
        
        Debug.Log(resultMessage);
        EditorUtility.DisplayDialog(resultTitle, resultMessage, "OK");
    }
}
#endif

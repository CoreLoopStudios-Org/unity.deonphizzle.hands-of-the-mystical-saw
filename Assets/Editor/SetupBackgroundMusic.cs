#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to automate and verify the creation of the BackgroundMusicManager and ButtonSoundManager in MainMenu and Predictor scenes.
/// </summary>
public class SetupBackgroundMusic : MonoBehaviour
{
    private const string MusicAssetPath = "Assets/Music/backgrund musicnew.wav";
    private const string ClickSoundAssetPath = "Assets/Music/Button-Click.wav";
    private static readonly string[] ScenePaths = new string[]
    {
        "Assets/ALL-SCENE-IS HERE/MainMenu.unity",
        "Assets/ALL-SCENE-IS HERE/PredictorScene.unity"
    };

    [MenuItem("Tools/Setup Background Music")]
    public static void SetupMusicInScenes()
    {
        // Load the Audio Clips
        AudioClip musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicAssetPath);
        if (musicClip == null)
        {
            Debug.LogError($"[Background Music Setup] Could not find Background Music Audio Clip at path: {MusicAssetPath}");
            EditorUtility.DisplayDialog("Error", $"Could not find background music file at path:\n{MusicAssetPath}\n\nPlease make sure the file is imported and exists in your Unity project.", "OK");
            return;
        }

        AudioClip clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClickSoundAssetPath);
        if (clickClip == null)
        {
            Debug.LogError($"[Background Music Setup] Could not find Button Click Audio Clip at path: {ClickSoundAssetPath}");
            EditorUtility.DisplayDialog("Error", $"Could not find button click sound file at path:\n{ClickSoundAssetPath}\n\nPlease make sure the file is imported and exists in your Unity project.", "OK");
            return;
        }

        // Keep track of the active scene to restore it later
        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        bool anyModified = false;

        foreach (string scenePath in ScenePaths)
        {
            if (System.IO.File.Exists(scenePath))
            {
                // Open Scene
                Scene activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                
                // --- SETUP BACKGROUND MUSIC ---
                GameObject musicManagerObj = GameObject.Find("BackgroundMusicManager");
                bool isMusicNew = false;
                
                if (musicManagerObj == null)
                {
                    musicManagerObj = new GameObject("BackgroundMusicManager");
                    isMusicNew = true;
                }

                SceneBackgroundMusicManager musicManager = musicManagerObj.GetComponent<SceneBackgroundMusicManager>();
                if (musicManager == null)
                {
                    musicManager = musicManagerObj.AddComponent<SceneBackgroundMusicManager>();
                }

                AudioSource musicAudioSource = musicManagerObj.GetComponent<AudioSource>();
                if (musicAudioSource == null)
                {
                    musicAudioSource = musicManagerObj.AddComponent<AudioSource>();
                }

                musicManager.backgroundMusicClip = musicClip;
                musicManager.activeScenes = new List<string> { "MainMenu", "PredictorScene" };
                musicAudioSource.clip = musicClip;
                musicAudioSource.loop = true;
                musicAudioSource.playOnAwake = false;

                EditorUtility.SetDirty(musicManagerObj);

                // --- SETUP BUTTON CLICK SOUNDS ---
                GameObject buttonSoundObj = GameObject.Find("ButtonSoundManager");
                bool isButtonSoundNew = false;
                
                if (buttonSoundObj == null)
                {
                    buttonSoundObj = new GameObject("ButtonSoundManager");
                    isButtonSoundNew = true;
                }

                ButtonSoundManager buttonSoundManager = buttonSoundObj.GetComponent<ButtonSoundManager>();
                if (buttonSoundManager == null)
                {
                    buttonSoundManager = buttonSoundObj.AddComponent<ButtonSoundManager>();
                }

                AudioSource buttonAudioSource = buttonSoundObj.GetComponent<AudioSource>();
                if (buttonAudioSource == null)
                {
                    buttonAudioSource = buttonSoundObj.AddComponent<AudioSource>();
                }

                buttonSoundManager.buttonClickClip = clickClip;
                buttonAudioSource.clip = clickClip;
                buttonAudioSource.loop = false;
                buttonAudioSource.playOnAwake = false;

                EditorUtility.SetDirty(buttonSoundObj);

                // Mark scene dirty and save
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                
                Debug.Log($"[Background Music Setup] Successfully set up audio managers in scene: {scenePath} (Created music object: {isMusicNew}, click object: {isButtonSoundNew})");
                anyModified = true;
            }
            else
            {
                Debug.LogWarning($"[Background Music Setup] Scene file not found at: {scenePath}");
            }
        }

        // Restore original scene
        if (!string.IsNullOrEmpty(originalScenePath) && System.IO.File.Exists(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        if (anyModified)
        {
            EditorUtility.DisplayDialog("Success", "Audio Setup completed successfully!\n\nBoth MainMenu and PredictorScene have been updated and saved with the background music and button click sound managers.", "Awesome");
        }
        else
        {
            EditorUtility.DisplayDialog("Warning", "No scenes were updated. Check the Unity console for details.", "OK");
        }
    }

    [MenuItem("Tools/Verify Background Music Setup")]
    public static void VerifySetup()
    {
        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("=== AUDIO MANAGERS VERIFICATION REPORT ===\n");

        bool setupCorrectly = true;

        foreach (string scenePath in ScenePaths)
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
            
            // 1. Check Background Music
            GameObject musicManagerObj = GameObject.Find("BackgroundMusicManager");
            if (musicManagerObj == null)
            {
                report.AppendLine("  [-] ERROR: 'BackgroundMusicManager' GameObject NOT found in hierarchy.");
                setupCorrectly = false;
            }
            else
            {
                SceneBackgroundMusicManager manager = musicManagerObj.GetComponent<SceneBackgroundMusicManager>();
                if (manager == null)
                {
                    report.AppendLine("  [-] ERROR: 'SceneBackgroundMusicManager' component NOT attached to the music GameObject.");
                    setupCorrectly = false;
                }
                else
                {
                    report.AppendLine("  [+] SUCCESS: 'SceneBackgroundMusicManager' component is attached.");
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

                AudioSource source = musicManagerObj.GetComponent<AudioSource>();
                if (source == null)
                {
                    report.AppendLine("  [-] ERROR: AudioSource component NOT attached to the music GameObject.");
                    setupCorrectly = false;
                }
                else
                {
                    report.AppendLine("  [+] SUCCESS: AudioSource component is attached for music.");
                }
            }

            // 2. Check Button Sounds
            GameObject buttonSoundObj = GameObject.Find("ButtonSoundManager");
            if (buttonSoundObj == null)
            {
                report.AppendLine("  [-] ERROR: 'ButtonSoundManager' GameObject NOT found in hierarchy.");
                setupCorrectly = false;
            }
            else
            {
                ButtonSoundManager manager = buttonSoundObj.GetComponent<ButtonSoundManager>();
                if (manager == null)
                {
                    report.AppendLine("  [-] ERROR: 'ButtonSoundManager' component NOT attached to the button sound GameObject.");
                    setupCorrectly = false;
                }
                else
                {
                    report.AppendLine("  [+] SUCCESS: 'ButtonSoundManager' component is attached.");
                    if (manager.buttonClickClip == null)
                    {
                        report.AppendLine("  [-] ERROR: Click sound 'buttonClickClip' field is NULL (not assigned).");
                        setupCorrectly = false;
                    }
                    else
                    {
                        report.AppendLine($"  [+] SUCCESS: Click sound 'buttonClickClip' is set to: {manager.buttonClickClip.name}");
                    }
                }

                AudioSource source = buttonSoundObj.GetComponent<AudioSource>();
                if (source == null)
                {
                    report.AppendLine("  [-] ERROR: AudioSource component NOT attached to the button sound GameObject.");
                    setupCorrectly = false;
                }
                else
                {
                    report.AppendLine("  [+] SUCCESS: AudioSource component is attached for button clicks.");
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

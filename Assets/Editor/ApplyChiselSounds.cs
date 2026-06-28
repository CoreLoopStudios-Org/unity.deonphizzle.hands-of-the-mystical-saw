#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ApplyChiselSounds : MonoBehaviour
{
    private const string ChiselHitSoundMp3Path = "Assets/Music/chisel-hit-sound.mp3";
    private const string ChiselWavPath = "Assets/Sprites/Audio/Chisel/chisel.wav";

    [MenuItem("Tools/Apply Chisel-Hit-Sound (MP3) to Chisels")]
    public static void ApplyMp3Sound()
    {
        ApplySound(ChiselHitSoundMp3Path);
    }

    [MenuItem("Tools/Apply Chisel (Wav) to Chisels")]
    public static void ApplyWavSound()
    {
        ApplySound(ChiselWavPath);
    }

    private static void ApplySound(string audioPath)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
        if (clip == null)
        {
            EditorUtility.DisplayDialog("Error", $"Audio clip not found at path: {audioPath}", "OK");
            return;
        }

        string[] scenes = new string[]
        {
            "Assets/ALL-SCENE-IS HERE/StoneGenerator Scene.unity",
            "Assets/ALL-SCENE-IS HERE/StoneCuttingScene_Classic.unity"
        };

        string originalScenePath = EditorSceneManager.GetActiveScene().path;
        bool anyModified = false;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"Applying {clip.name} as primaryHitSound:\n");

        foreach (string scenePath in scenes)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                sb.AppendLine($"[-] Scene not found: {scenePath}");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneDirty = false;

            GameObject[] rootObjects = scene.GetRootGameObjects();

            // 1. Modern Chisel in StoneGenerator Scene
            if (scenePath.Contains("StoneGenerator"))
            {
                GameObject modernChisel = FindObjectByName(rootObjects, "Chisel_rigged -modern");
                if (modernChisel == null) modernChisel = FindObjectByName(rootObjects, "Chisel_rigged-modern");

                if (modernChisel != null)
                {
                    var manual = modernChisel.GetComponentInChildren<ManualChiselController>(true);
                    if (manual != null)
                    {
                        manual.primaryHitSound = clip;
                        EditorUtility.SetDirty(manual);
                        sceneDirty = true;
                        sb.AppendLine($"[+] Applied to ManualChiselController on '{modernChisel.name}' in {scene.name}");
                    }
                }
                else
                {
                    sb.AppendLine($"[-] Modern Chisel 'Chisel_rigged -modern' not found in {scene.name}");
                }
            }

            // 2. Classic Chisel in StoneCuttingScene_Classic
            if (scenePath.Contains("Classic"))
            {
                GameObject classicChisel = FindObjectByName(rootObjects, "Chissel_classic_rigged-");
                if (classicChisel == null) classicChisel = FindObjectByName(rootObjects, "Chissel_classic_rigged");

                if (classicChisel != null)
                {
                    var classic = classicChisel.GetComponentInChildren<ClassicChiselController>(true);
                    if (classic != null)
                    {
                        classic.primaryHitSound = clip;
                        EditorUtility.SetDirty(classic);
                        sceneDirty = true;
                        sb.AppendLine($"[+] Applied to ClassicChiselController on '{classicChisel.name}' in {scene.name}");
                    }
                    else
                    {
                        var general = classicChisel.GetComponentInChildren<ChiselController>(true);
                        if (general != null)
                        {
                            general.primaryHitSound = clip;
                            EditorUtility.SetDirty(general);
                            sceneDirty = true;
                            sb.AppendLine($"[+] Applied to ChiselController on '{classicChisel.name}' in {scene.name}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"[-] Classic Chisel 'Chissel_classic_rigged-' not found in {scene.name}");
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
            EditorUtility.DisplayDialog("Success", sb.ToString(), "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Notice", "No changes were applied.", "OK");
        }
    }

    private static GameObject FindObjectByName(GameObject[] roots, string name)
    {
        foreach (var root in roots)
        {
            if (root.name == name) return root;
            var child = FindDeepChild(root.transform, name);
            if (child != null) return child.gameObject;
        }
        return null;
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
#endif

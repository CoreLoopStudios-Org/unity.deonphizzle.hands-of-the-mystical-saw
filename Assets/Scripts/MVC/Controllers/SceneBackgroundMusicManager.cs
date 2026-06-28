using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the background music across various scenes in the project.
/// Persists across scene transitions and fades audio in and out smoothly.
/// </summary>
public class SceneBackgroundMusicManager : MonoBehaviour
{
    public static SceneBackgroundMusicManager Instance { get; private set; }

    [Header("Audio Settings")]
    [Tooltip("The audio clip to play as background music.")]
    public AudioClip backgroundMusicClip;

    [Range(0f, 1f)]
    [Tooltip("Target volume for the background music.")]
    public float targetVolume = 0.5f;

    [Tooltip("How long it takes to fade in or fade out the music.")]
    public float fadeDuration = 0.8f;

    [Header("Scene Configuration")]
    [Tooltip("Scenes in which this background music should play.")]
    public List<string> activeScenes = new List<string> { "MainMenu", "PredictorScene" };

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        Debug.Log("[BGM] Awake called on SceneBackgroundMusicManager.");

        // Ensure it is a root object so DontDestroyOnLoad works properly
        if (transform.parent != null)
        {
            Debug.Log("[BGM] Detaching manager from parent to allow DontDestroyOnLoad.");
            transform.SetParent(null);
        }

        // Singleton pattern to ensure only one instance persists
        if (Instance != null && Instance != this)
        {
            Debug.Log($"[BGM] Duplicate manager found on {gameObject.name}. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure AudioSource exists
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        
        if (backgroundMusicClip != null)
        {
            audioSource.clip = backgroundMusicClip;
            Debug.Log($"[BGM] Audio clip assigned successfully: {backgroundMusicClip.name}");
        }
        else
        {
            Debug.LogWarning("[BGM] Warning: No backgroundMusicClip assigned to the music manager.");
        }

        // Fallback: if list is somehow empty due to Unity serialization overrides
        if (activeScenes == null || activeScenes.Count == 0)
        {
            activeScenes = new List<string> { "MainMenu", "PredictorScene" };
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Trigger verification for the initial scene loaded
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"[BGM] Start called. Current scene: {currentScene}");
        EvaluateSceneMusic(currentScene);
        UpdateMusicStatus();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
        {
            Debug.Log($"[BGM] Scene loaded: {scene.name}");
            EvaluateSceneMusic(scene.name);
        }
    }

    private void EvaluateSceneMusic(string sceneName)
    {
        UpdateMusicStatus();
        if (activeScenes != null && activeScenes.Contains(sceneName))
        {
            Debug.Log($"[BGM] Scene '{sceneName}' matches active scenes list.");
            
            // The scene matches: play/resume music
            if (!audioSource.isPlaying)
            {
                if (audioSource.clip == null && backgroundMusicClip != null)
                {
                    audioSource.clip = backgroundMusicClip;
                }
                
                if (audioSource.clip != null)
                {
                    audioSource.volume = 0f; // Start at 0 volume for smooth fade-in
                    audioSource.Play();
                    Debug.Log($"[BGM] AudioSource started playing '{audioSource.clip.name}'. Initiating fade-in to {targetVolume}.");
                    StartFade(targetVolume);
                }
                else
                {
                    Debug.LogError("[BGM] Cannot play background music because no AudioClip is assigned.");
                }
            }
            else
            {
                // If it is already playing, ensure it fades up to the target volume (in case it was fading out)
                Debug.Log($"[BGM] Music already playing. Ensuring target volume {targetVolume}.");
                StartFade(targetVolume);
            }
        }
        else
        {
            Debug.Log($"[BGM] Scene '{sceneName}' is NOT in active scenes list.");
            // The scene does not match: stop/fade out music
            if (audioSource.isPlaying)
            {
                Debug.Log("[BGM] Fading out and stopping music.");
                StartFade(0f, () => {
                    audioSource.Stop();
                    Debug.Log("[BGM] Music stopped.");
                });
            }
        }
    }

    private void StartFade(float target, System.Action onComplete = null)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeVolume(target, onComplete));
    }

    private IEnumerator FadeVolume(float target, System.Action onComplete)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        // Fallback: If duration is invalid or zero, transition instantly
        if (fadeDuration <= 0f)
        {
            audioSource.volume = target;
            onComplete?.Invoke();
            yield break;
        }

        // Use unscaledDeltaTime to ignore game pauses
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, target, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = target;
        onComplete?.Invoke();
    }

    public void UpdateMusicStatus()
    {
        if (audioSource != null)
        {
            audioSource.mute = (PlayerPrefs.GetInt("MusicEnabled", 1) == 0);
        }
    }
}

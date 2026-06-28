using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages gameplay background music. Persists across gameplay scenes (classic and modern),
/// starting the music on enter and fading it out / stopping it when navigating to menus.
/// </summary>
public class GameplayBackgroundMusicManager : MonoBehaviour
{
    public static GameplayBackgroundMusicManager Instance { get; private set; }

    [Header("Audio Settings")]
    [Tooltip("The audio clip to play as gameplay background music.")]
    public AudioClip backgroundMusicClip;

    [Range(0f, 1f)]
    [Tooltip("Target volume for the gameplay background music.")]
    public float targetVolume = 0.5f;

    [Tooltip("Fading speed duration in seconds.")]
    public float fadeDuration = 0.8f;

    [Header("Scene Configuration")]
    [Tooltip("Scenes where the gameplay music is allowed to play.")]
    public List<string> activeScenes = new List<string> { "StoneCuttingScene_Classic", "StoneGenerator Scene" };

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        Debug.Log("[GameplayBGM] Awake called on GameplayBackgroundMusicManager.");

        // Detach from parent to allow DontDestroyOnLoad to succeed
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        // Singleton validation
        if (Instance != null && Instance != this)
        {
            Debug.Log($"[GameplayBGM] Duplicate gameplay manager found on {gameObject.name}. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure AudioSource is present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure default AudioSource properties
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        
        if (backgroundMusicClip != null)
        {
            audioSource.clip = backgroundMusicClip;
        }
        else
        {
            Debug.LogWarning("[GameplayBGM] Warning: No backgroundMusicClip assigned to the gameplay music manager.");
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
        // Check initial active scene
        EvaluateSceneMusic(SceneManager.GetActiveScene().name);
        UpdateMusicStatus();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
        {
            Debug.Log($"[GameplayBGM] New scene loaded: {scene.name}");
            EvaluateSceneMusic(scene.name);
        }
    }

    private void EvaluateSceneMusic(string sceneName)
    {
        UpdateMusicStatus();
        if (activeScenes != null && activeScenes.Contains(sceneName))
        {
            Debug.Log($"[GameplayBGM] Scene '{sceneName}' matches active gameplay list.");
            if (!audioSource.isPlaying)
            {
                if (audioSource.clip == null && backgroundMusicClip != null)
                {
                    audioSource.clip = backgroundMusicClip;
                }
                
                if (audioSource.clip != null)
                {
                    audioSource.volume = 0f;
                    audioSource.Play();
                    Debug.Log($"[GameplayBGM] Started playing gameplay music clip '{audioSource.clip.name}'. Fading in.");
                    StartFade(targetVolume);
                }
                else
                {
                    Debug.LogError("[GameplayBGM] Cannot play gameplay music because no AudioClip is assigned.");
                }
            }
            else
            {
                // Already playing, ensure it is faded up to the target volume
                StartFade(targetVolume);
            }
        }
        else
        {
            Debug.Log($"[GameplayBGM] Scene '{sceneName}' is NOT in active gameplay list.");
            if (audioSource.isPlaying)
            {
                Debug.Log("[GameplayBGM] Exited gameplay. Fading out and stopping gameplay music.");
                StartFade(0f, () => {
                    audioSource.Stop();
                    Debug.Log("[GameplayBGM] Gameplay music stopped.");
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

        if (fadeDuration <= 0f)
        {
            audioSource.volume = target;
            onComplete?.Invoke();
            yield break;
        }

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

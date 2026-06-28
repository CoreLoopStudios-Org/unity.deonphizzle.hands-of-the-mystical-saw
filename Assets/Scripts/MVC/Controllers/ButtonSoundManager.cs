using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Automatically finds and binds to all buttons in the game to play a click sound.
/// Persists across scenes and handles dynamically spawned buttons.
/// </summary>
public class ButtonSoundManager : MonoBehaviour
{
    public static ButtonSoundManager Instance { get; private set; }

    [Header("Audio Settings")]
    [Tooltip("The audio clip to play when a button is clicked.")]
    public AudioClip buttonClickClip;

    [Range(0f, 1f)]
    [Tooltip("Volume of the button click sound.")]
    public float clickVolume = 0.8f;

    private AudioSource audioSource;
    private HashSet<Button> registeredButtons = new HashSet<Button>();

    private void Awake()
    {
        Debug.Log("[ButtonSound] Awake called on ButtonSoundManager.");

        // Ensure it is a root object so DontDestroyOnLoad works properly
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.Log($"[ButtonSound] Duplicate manager found on {gameObject.name}. Destroying duplicate.");
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

        // Configure AudioSource for UI sounds
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        if (buttonClickClip != null)
        {
            Debug.Log($"[ButtonSound] Button click clip assigned: {buttonClickClip.name}");
        }
        else
        {
            Debug.LogWarning("[ButtonSound] Warning: No buttonClickClip assigned to the button sound manager.");
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
        RegisterAllButtonsInActiveScene();
        StartCoroutine(PollNewButtonsRoutine());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
        {
            // Clear out references from destroyed scenes
            registeredButtons.RemoveWhere(b => b == null);
            RegisterAllButtonsInActiveScene();
        }
    }

    private void RegisterAllButtonsInActiveScene()
    {
        // Clean up any missing references
        registeredButtons.RemoveWhere(b => b == null);

        // Find all buttons in the active scene (including inactive ones)
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        int newlyRegistered = 0;

        foreach (Button button in buttons)
        {
            // Ensure the button is part of a loaded scene, not a project asset/prefab, and not already registered
            if (button != null && button.gameObject.scene.isLoaded && !registeredButtons.Contains(button))
            {
                button.onClick.AddListener(PlayClickSound);
                registeredButtons.Add(button);
                newlyRegistered++;
            }
        }

        if (newlyRegistered > 0)
        {
            Debug.Log($"[ButtonSound] Registered {newlyRegistered} new buttons. Total registered: {registeredButtons.Count}");
        }
    }

    private IEnumerator PollNewButtonsRoutine()
    {
        // Periodically check for newly spawned/instantiated buttons at runtime
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            RegisterAllButtonsInActiveScene();
        }
    }

    private void PlayClickSound()
    {
        if (PlayerPrefs.GetInt("SoundEnabled", 1) == 0) return;

        if (audioSource != null && buttonClickClip != null)
        {
            audioSource.PlayOneShot(buttonClickClip, clickVolume);
            Debug.Log($"[ButtonSound] Played click sound: {buttonClickClip.name}");
        }
    }
}

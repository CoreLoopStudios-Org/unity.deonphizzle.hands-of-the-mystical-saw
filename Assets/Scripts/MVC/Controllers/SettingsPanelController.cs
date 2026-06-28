using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("--- Settings Toggles ---")]
    public Toggle gyroToggle;
    public GameObject gyroOnVisual;
    public GameObject gyroOffVisual;

    public Toggle soundToggle;
    public GameObject soundOnVisual;
    public GameObject soundOffVisual;

    public Toggle musicToggle;
    public GameObject musicOnVisual;
    public GameObject musicOffVisual;

    private void OnEnable()
    {
        AutoBindToggles();
        InitializeToggleStates();
    }

    private void Start()
    {
        AutoBindToggles();
        InitializeToggleStates();

        if (gyroToggle != null)
        {
            gyroToggle.onValueChanged.RemoveListener(OnGyroToggleChanged);
            gyroToggle.onValueChanged.AddListener(OnGyroToggleChanged);
        }
        if (soundToggle != null)
        {
            soundToggle.onValueChanged.RemoveListener(OnSoundToggleChanged);
            soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        }
        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);
            musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        }
    }

    private void AutoBindToggles()
    {
        // Try to find Gyro
        if (gyroToggle == null)
        {
            Transform t = FindTransformContains(transform, "GyroMode");
            if (t != null) gyroToggle = t.GetComponent<Toggle>();
        }
        if (gyroToggle != null && (gyroOnVisual == null || gyroOffVisual == null))
        {
            Transform bg = gyroToggle.transform.Find("Background");
            if (bg != null && bg.childCount >= 2)
            {
                gyroOffVisual = bg.GetChild(0).gameObject;
                gyroOnVisual = bg.GetChild(1).gameObject;
            }
        }

        // Try to find Sound
        if (soundToggle == null)
        {
            Transform t = FindTransformContains(transform, "Sound");
            if (t != null) soundToggle = t.GetComponent<Toggle>();
        }
        if (soundToggle != null && (soundOnVisual == null || soundOffVisual == null))
        {
            Transform bg = soundToggle.transform.Find("Background");
            if (bg != null && bg.childCount >= 2)
            {
                soundOffVisual = bg.GetChild(0).gameObject;
                soundOnVisual = bg.GetChild(1).gameObject;
            }
        }

        // Try to find Music
        if (musicToggle == null)
        {
            Transform t = FindTransformContains(transform, "Music");
            if (t != null) musicToggle = t.GetComponent<Toggle>();
        }
        if (musicToggle != null && (musicOnVisual == null || musicOffVisual == null))
        {
            Transform bg = musicToggle.transform.Find("Background");
            if (bg != null && bg.childCount >= 2)
            {
                musicOffVisual = bg.GetChild(0).gameObject;
                musicOnVisual = bg.GetChild(1).gameObject;
            }
        }
    }

    private Transform FindTransformContains(Transform current, string namePart)
    {
        if (current.name.Contains(namePart)) return current;
        for (int i = 0; i < current.childCount; i++)
        {
            Transform found = FindTransformContains(current.GetChild(i), namePart);
            if (found != null) return found;
        }
        return null;
    }

    private void InitializeToggleStates()
    {
        bool gyroEnabled = PlayerPrefs.GetInt("GyroEnabled", 1) == 1;
        bool soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

        if (gyroToggle != null)
        {
            gyroToggle.SetIsOnWithoutNotify(gyroEnabled);
            UpdateGyroVisuals(gyroEnabled);
        }
        if (soundToggle != null)
        {
            soundToggle.SetIsOnWithoutNotify(soundEnabled);
            UpdateSoundVisuals(soundEnabled);
        }
        if (musicToggle != null)
        {
            musicToggle.SetIsOnWithoutNotify(musicEnabled);
            UpdateMusicVisuals(musicEnabled);
        }
    }

    private void OnGyroToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("GyroEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
        UpdateGyroVisuals(isOn);
        Debug.Log($"[Settings] Gyro toggled: {isOn}");
    }

    private void OnSoundToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("SoundEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
        UpdateSoundVisuals(isOn);
        Debug.Log($"[Settings] Sound toggled: {isOn}");
    }

    private void OnMusicToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
        UpdateMusicVisuals(isOn);
        Debug.Log($"[Settings] Music toggled: {isOn}");

        // Propagate change to active BGM managers
        if (SceneBackgroundMusicManager.Instance != null)
            SceneBackgroundMusicManager.Instance.UpdateMusicStatus();
        if (GameplayBackgroundMusicManager.Instance != null)
            GameplayBackgroundMusicManager.Instance.UpdateMusicStatus();
    }

    private void UpdateGyroVisuals(bool isOn)
    {
        if (gyroOnVisual != null) gyroOnVisual.SetActive(isOn);
        if (gyroOffVisual != null) gyroOffVisual.SetActive(!isOn);
    }

    private void UpdateSoundVisuals(bool isOn)
    {
        if (soundOnVisual != null) soundOnVisual.SetActive(isOn);
        if (soundOffVisual != null) soundOffVisual.SetActive(!isOn);
    }

    private void UpdateMusicVisuals(bool isOn)
    {
        if (musicOnVisual != null) musicOnVisual.SetActive(isOn);
        if (musicOffVisual != null) musicOffVisual.SetActive(!isOn);
    }

    public void SelectClassicMode()
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetTheme(GameModeManager.GameTheme.Classic);
        }
        else
        {
            Debug.LogError("GameModeManager Instance not found!");
        }
    }

    public void SelectModernMode()
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetTheme(GameModeManager.GameTheme.Modern);
        }
        else
        {
            Debug.LogError("GameModeManager Instance not found!");
        }
    }

    public void OpenModeSelectionPanel()
    {
        if (ModeSelectionManager.Instance != null)
        {
            ModeSelectionManager.Instance.ShowPanel();
        }
        else
        {
            ModeSelectionManager manager = Object.FindFirstObjectByType<ModeSelectionManager>(FindObjectsInactive.Include);
            if (manager != null)
            {
                manager.ShowPanel();
            }
            else
            {
                Debug.LogError("ModeSelectionManager Instance not found in this scene!");
            }
        }
    }
}


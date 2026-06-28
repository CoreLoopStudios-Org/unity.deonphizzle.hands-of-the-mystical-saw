using UnityEngine;
using UnityEngine.SceneManagement;

public class AcceptMatchController : MonoBehaviour
{
    public void OnClickAcceptButton()
    {
        if (GameModeManager.Instance == null)
        {
            Debug.LogError("GameModeManager Instance not found!");
            return;
        }

        if (GameModeManager.Instance.currentTheme == GameModeManager.GameTheme.Classic)
        {
            Debug.Log("Starting Classic Match...");
            SceneManager.LoadScene("StoneCuttingScene_Classic"); 
        }
        else
        {
            Debug.Log("Starting Modern Match...");
            SceneManager.LoadScene("StoneGenerator Scene");
        }
    }
}

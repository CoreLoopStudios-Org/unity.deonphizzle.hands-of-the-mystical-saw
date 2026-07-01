using UnityEngine;
using UnityEngine.SceneManagement; // Required to load the scene

public class SceneSwitcher : MonoBehaviour
{
    // Any scene can be loaded with this function
    public void GoToScene(string sceneName)
    {
        Debug.Log("Loading: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    // To exit the game
    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }
}
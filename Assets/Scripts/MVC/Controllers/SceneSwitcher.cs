using UnityEngine;
using UnityEngine.SceneManagement; // সিন লোড করার জন্য অবশ্যই লাগবে

public class SceneSwitcher : MonoBehaviour
{
    // এই ফাংশনটি দিয়ে যেকোনো সিন লোড করা যাবে
    public void GoToScene(string sceneName)
    {
        Debug.Log("Loading: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    // গেম থেকে বের হওয়ার জন্য
    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }
}
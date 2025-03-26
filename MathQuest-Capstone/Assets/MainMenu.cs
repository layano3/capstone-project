using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadQuests()
    {
        SceneManager.LoadSceneAsync(2);
    }
    
    public void LoadProfile()
    {
        SceneManager.LoadSceneAsync(3);
    }

    public void LoadSettings()
    {
        SceneManager.LoadSceneAsync(4);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void LoadGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

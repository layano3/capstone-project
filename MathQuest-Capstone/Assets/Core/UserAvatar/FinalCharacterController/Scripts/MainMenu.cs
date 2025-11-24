using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadQuests()
    {
        // Destroy the existing QuestManager instance before going to the quest list scene
        if (QuestManager.Instance != null)
        {
            Destroy(QuestManager.Instance.gameObject);
        }

        SceneManager.LoadSceneAsync(3);
    }

    public void LoadProfile()
    {
        SceneManager.LoadSceneAsync(4);
    }

    public void LoadSettings()
    {
        SceneManager.LoadSceneAsync(5);
    }

    public void BackToMainMenu()
    {
        // Destroy the existing QuestManager instance
        if (QuestManager.Instance != null)
        {
            Destroy(QuestManager.Instance.gameObject);
        }
        SceneManager.LoadSceneAsync(1);
    }

    public void LoadGame()
    {
        // AudioManager.Instance.StopMusic();
        
        // Update activity tracker when entering game
        if (ActivityTracker.Instance != null)
        {
            ActivityTracker.Instance.StartExploring("GamePlay");
        }
        
        SceneManager.LoadSceneAsync(6);
    }

    
    public void QuitGame()
    {
        Application.Quit();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathSceneButtons : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "main menu";

    // Load from last save
    public void LoadLastSave()
    {
        Debug.Log("Death screen: Load last save");

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("No SaveManager found.");
            return;
        }

        SaveManager.Instance.LoadGameFromMenu();
    }

    // Quit to main menu
    public void QuitToMenu()
    {
        Debug.Log("Death screen: Quit to menu");

        Time.timeScale = 1f; // important if game was paused

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
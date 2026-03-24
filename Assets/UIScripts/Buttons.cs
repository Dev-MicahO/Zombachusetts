using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void PlayButton()
    {
        SceneChanger.Instance.LoadScene("Battlescene");
    }

    public void QuitButton()
    {
        Debug.Log("exited");
        Application.Quit();
    }

    public void BackButton()
    {
        SceneChanger.Instance.LoadScene("main menu");
    }

    public void QuitToDesktop()
    {
        PlayerPrefs.Save();

        Debug.Log("Game Saved!");

        Application.Quit();
        
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        PlayerPrefs.Save();

        Debug.Log("Game Saved!");

        SceneChanger.Instance.LoadScene("main menu");
    }
    

}
using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{

    // pause menu panels
    public GameObject pauseMenu;
    public GameObject settingsMenu;

    // player control system to enable use of keys
    private PlayerControls controls;

    // boolean to keep track of the paused state
    private bool isPaused = false;

    // allows you to use key presses to close menus
    void Awake()
    {
        controls = new PlayerControls();

        controls.gameUI.Pause.performed += ctx =>
        {
            if (settingsMenu.activeSelf)
            {
                Time.timeScale =1f;
                CloseSettings();
            }
            else
                TogglePause();
        };
    }

     void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    // toggles between the paused and unpaused state
    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

    }

    // toggles the settings panel on
    public void OpenSettings()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    // toggles the settings panel off
    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    // disables all menu panels and returns the time to normal rate
    public void ResumeGame()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        Time.timeScale = 1f;
    }

      public void QuitToDesktop()
    {
        PlayerPrefs.Save();

        Debug.Log("Game Saved!");

        Application.Quit();
        
    }

    // button code for the quit to title button in the pause menu 
    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        PlayerPrefs.Save();

        Debug.Log("Game Saved!");

        SceneChanger.Instance.LoadScene("main menu");
    }

    
}

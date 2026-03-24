using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{

    public GameObject pauseMenu;
    public GameObject settingsMenu;

    private PlayerControls controls;
    private bool isPaused = false;

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

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    public void OpenSettings()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        Time.timeScale = 1f;
    }
}

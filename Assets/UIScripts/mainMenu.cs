using UnityEngine;
using UnityEngine.InputSystem;

public class mainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    
    public Animator settingsButtonAnimator;
    public Animator backButtonAnimator;

    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();

        controls.MainMenuUI.closeSettings.performed += ctx =>
        {
            if (settingsPanel.activeSelf)
            {
                CloseSettings();
            }
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

    public void PlayButton()
    {
        SceneChanger.Instance.LoadScene("Battlescene");
    }

    // button code for the quit button on the main menu
    public void QuitButton()
    {
        Debug.Log("exited");
        Application.Quit();
    }

    // button code for the back button on the settings menu 
    public void BackButton()
    {
        SceneChanger.Instance.LoadScene("main menu");
    }

    // button code for the quit to desktop button in the pause menu
    public void QuitToDesktop()
    {
        PlayerPrefs.Save();

        Debug.Log("Game Saved!");

        Application.Quit();
        
    }

    private void ResetButton(Animator animator)
    {
        if (animator == null) return;

        animator.ResetTrigger("Pressed");
        animator.ResetTrigger("Normal");
        animator.Play("Normal", 0, 0f);
        animator.Update(0f);
    }

    public void OpenSettings()
    {
        ResetButton(settingsButtonAnimator);

        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        ResetButton(backButtonAnimator);

        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void LoadGameButton()
    {
        Debug.Log("Load button pressed");

        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("No SaveManager found.");
            return;
        }

        SaveManager.Instance.LoadGameFromMenu();
    }
}
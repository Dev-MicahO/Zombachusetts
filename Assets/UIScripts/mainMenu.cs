using UnityEngine;
using UnityEngine.InputSystem;

public class mainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject classPanel;
    
    public Animator settingsButtonAnimator;
    public Animator backButtonAnimator;
    public Animator backButton2Animator;
    public Animator playButtonAnimator;

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
        ResetButton(playButtonAnimator);

        mainMenuPanel.SetActive(false);
        classPanel.SetActive(true);
    }

    // button code for the quit button on the main menu
    public void QuitButton()
    {
        Debug.Log("exited");
        Application.Quit();
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

        public void CloseClassPanel()
    {
        ResetButton(backButton2Animator);

        classPanel.SetActive(false);
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

    // Class picking 
    public void SelectWarrior()
    {
        SelectClassAndStart(PlayerClass.Warrior);
    }

    public void SelectMage()
    {
        SelectClassAndStart(PlayerClass.Mage);
    }

    public void SelectDoctor()
    {
        SelectClassAndStart(PlayerClass.Doctor);
    }
    public void SelectThief()
    {
        SelectClassAndStart(PlayerClass.Thief);
    }

     // Method to pick the class and start the game
    private void SelectClassAndStart(PlayerClass chosenClass)
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.SetClass(chosenClass);
            GameSession.Instance.isRandomEncounter = false;
        }

        SceneChanger.Instance.LoadScene("Battlescene");
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class mainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

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

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
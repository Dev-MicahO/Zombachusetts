using UnityEngine;

public class SaveButtonClick : MonoBehaviour
{
    public void SaveGame()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("No SaveManager found.");
            return;
        }

        SaveManager.Instance.SaveGame();
    }
}
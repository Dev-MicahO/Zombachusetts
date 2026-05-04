using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    public int playerCurrentHP;
    public Vector3 playerPosition;
    public string currentScene;
    public int playerLevel;
    public int playerXP;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    private SaveData pendingLoadData;
    private bool shouldApplyLoadedData = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null);

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // saves various player states
    public void SaveGame()
    {
        Debug.Log("SaveGame button pressed");
        SaveData data = new SaveData();

        data.playerCurrentHP = GameSession.Instance.playerCurrentHP;
        data.playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        data.currentScene = SceneManager.GetActiveScene().name;
        data.playerLevel = GameSession.Instance.playerLevel;
        data.playerXP = GameSession.Instance.playerXP;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Game saved to: " + SavePath);
    }

    // loads from play button
    public void LoadGameFromMenu()
    {
        if (Instance == null)
        {
            Debug.LogError("No SaveManager instance!");
            return;
        }

        if (!File.Exists(SavePath))
        {
            SceneManager.LoadScene(1);
            return;
        }

        string json = File.ReadAllText(SavePath);
        pendingLoadData = JsonUtility.FromJson<SaveData>(json);

        shouldApplyLoadedData = true;

        SceneManager.LoadScene(pendingLoadData.currentScene);
    }

    // applies after scene loads
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!shouldApplyLoadedData || pendingLoadData == null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("No player found in loaded scene.");
            return;
        }

        GameSession.Instance.playerCurrentHP = pendingLoadData.playerCurrentHP;
        GameSession.Instance.playerLevel = pendingLoadData.playerLevel;
        GameSession.Instance.playerXP = pendingLoadData.playerXP;

        player.transform.position = pendingLoadData.playerPosition;

        shouldApplyLoadedData = false;
        pendingLoadData = null;

        Debug.Log("Game loaded.");
    }

    // delete save from settings menu if you want
    public void DeleteSaveData()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save deleted.");
        }
        else
        {
            Debug.Log("No save file found.");
        }
    }

    // finds save
    public bool SaveExists()
    {
        return File.Exists(SavePath);
    }
}
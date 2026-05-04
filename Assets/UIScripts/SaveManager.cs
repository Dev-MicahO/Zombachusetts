using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// SaveData class to hold all the necessary information for saving/loading the game state.
[System.Serializable]
public class SaveData
{
    public int playerCurrentHP;
    public Vector3 playerPosition;
    public string currentScene;
    public int playerLevel;
    public int playerXP;

    // Destroyed/interacted objects
    public List<string> destroyedObjects = new List<string>();

    // Inventory / key item flags
    public bool GoldenBeagle;
    public bool SuspicousBrain;
    public bool RubyDagger;
    public bool KevlarVest;

    // Party member flags
    public bool hasPartyMember2;
    public bool hasPartyMember3;
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

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("Save failed: No Player object found in this scene.");
            return;
        }

        SaveData data = new SaveData();

        Vector3 savePosition = SnapToPlayerGrid(player.transform.position);

        // Save clean tile/grid position.
        savePosition.x = Mathf.Round(savePosition.x);
        savePosition.y = Mathf.Round(savePosition.y);

        data.playerCurrentHP = GameSession.Instance.playerCurrentHP;
        data.playerPosition = savePosition;
        data.currentScene = SceneManager.GetActiveScene().name;
        data.playerLevel = GameSession.Instance.playerLevel;
        data.playerXP = GameSession.Instance.playerXP;
        data.destroyedObjects = new List<string>(GameSession.Instance.destroyedObjects);
        data.GoldenBeagle = GameSession.Instance.GoldenBeagle;
        data.SuspicousBrain = GameSession.Instance.SuspicousBrain;
        data.RubyDagger = GameSession.Instance.RubyDagger;
        data.KevlarVest = GameSession.Instance.KevlarVest;

        data.hasPartyMember2 = GameSession.Instance.hasPartyMember2;
        data.hasPartyMember3 = GameSession.Instance.hasPartyMember3;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Game saved to: " + SavePath);
        Debug.Log("Saved scene: " + data.currentScene);
        Debug.Log("Saved position: " + data.playerPosition);
    }

    public void LoadGameFromMenu()
    {
        if (!File.Exists(SavePath))
        {
            SceneManager.LoadScene(1);
            return;
        }

        string json = File.ReadAllText(SavePath);
        pendingLoadData = JsonUtility.FromJson<SaveData>(json);

        shouldApplyLoadedData = true;

        if (GameSession.Instance != null)
        {
            GameSession.Instance.currentOverworldScene = pendingLoadData.currentScene;

            GameSession.Instance.randomEncounterReturnScene = pendingLoadData.currentScene;
            GameSession.Instance.isRandomEncounter = false;
            GameSession.Instance.isFinalBossFight = false;
            GameSession.Instance.hasReturnPosition = false;
            GameSession.Instance.loadingReturnToPreviousScene = false;
            GameSession.Instance.loadingTargetScene = "";
        }

        SceneManager.LoadScene(pendingLoadData.currentScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!shouldApplyLoadedData || pendingLoadData == null)
            return;

        StartCoroutine(ApplyLoadedDataAfterFrame());
    }

    private IEnumerator ApplyLoadedDataAfterFrame()
    {
        // Wait a little longer so spawn scripts, PlayerController, and camera logic finish first.
        yield return null;
        yield return new WaitForEndOfFrame();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("No player found in loaded scene.");
            yield break;
        }

        GameSession.Instance.playerCurrentHP = pendingLoadData.playerCurrentHP;
        GameSession.Instance.playerLevel = pendingLoadData.playerLevel;
        GameSession.Instance.playerXP = pendingLoadData.playerXP;
        GameSession.Instance.destroyedObjects = new HashSet<string>(pendingLoadData.destroyedObjects);

        GameSession.Instance.GoldenBeagle = pendingLoadData.GoldenBeagle;
        GameSession.Instance.SuspicousBrain = pendingLoadData.SuspicousBrain;
        GameSession.Instance.RubyDagger = pendingLoadData.RubyDagger;
        GameSession.Instance.KevlarVest = pendingLoadData.KevlarVest;

        GameSession.Instance.hasPartyMember2 = pendingLoadData.hasPartyMember2;
        GameSession.Instance.hasPartyMember3 = pendingLoadData.hasPartyMember3;

        Vector3 loadedPosition = SnapToPlayerGrid(pendingLoadData.playerPosition);

        // Force safe 2D position.
        loadedPosition.z = player.transform.position.z;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = loadedPosition;
        }
        else
        {
            player.transform.position = loadedPosition;
        }

        Physics2D.SyncTransforms();

        Debug.Log("Loaded scene: " + pendingLoadData.currentScene);
        Debug.Log("Loaded position: " + loadedPosition);

        shouldApplyLoadedData = false;
        pendingLoadData = null;

        Debug.Log("Game loaded.");
    }

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

    public bool SaveExists()
    {
        return File.Exists(SavePath);
    }

    //Fix incorrect player position when loading a save
    private Vector3 SnapToPlayerGrid(Vector3 position)
    {
        float gridSize = 1f;

        float xOffset = -0.54f;
        float yOffset = -0.52f;

        position.x = Mathf.Round((position.x - xOffset) / gridSize) * gridSize + xOffset;
        position.y = Mathf.Round((position.y - yOffset) / gridSize) * gridSize + yOffset;

        return position;
    }
}
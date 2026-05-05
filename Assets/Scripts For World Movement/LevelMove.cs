using UnityEngine;

public class LevelMove : MonoBehaviour
{
    // Script to allow the player to move to a different level
    [Header("Scene To Load")]
    public string sceneToLoad;

    [Header("Where Player Should Spawn In That Scene")]
    public Vector2 returnSpawnPosition;

    [Header("Loading Screen")]
    public bool useLoadingScreen = true;
    public float loadingScreenDuration = 0.25f;

    private bool isLoading = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (GameSession.Instance == null)
        {
            Debug.LogError("ReturnToPreviousScene: GameSession instance not found.");
            return;
        }

        isLoading = true;

        // Tell PlayerController in the next scene where to place the player.
        GameSession.Instance.returnPlayerPosition = new Vector3(
            returnSpawnPosition.x,
            returnSpawnPosition.y,
            other.transform.position.z
        );

        GameSession.Instance.hasReturnPosition = true;

        // Keep scene tracking consistent for battle returns/saves.
        GameSession.Instance.currentOverworldScene = sceneToLoad;
        GameSession.Instance.randomEncounterReturnScene = sceneToLoad;

        if (useLoadingScreen)
        {
            GameSession.Instance.loadingTargetScene = sceneToLoad;
            GameSession.Instance.loadingReturnToPreviousScene = false;
            GameSession.Instance.loadingScreenDuration = loadingScreenDuration;

            UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }
}
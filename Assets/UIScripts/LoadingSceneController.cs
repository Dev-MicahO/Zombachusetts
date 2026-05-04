using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadAfterDelay());
    }

    IEnumerator LoadAfterDelay()
    {
        float delay = 0.25f;

        if (GameSession.Instance != null)
            delay = GameSession.Instance.loadingScreenDuration;

        yield return new WaitForSeconds(delay);

        if (GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.loadingTargetScene))
        {
            string targetScene = GameSession.Instance.loadingTargetScene;

            Debug.Log("Loading screen sending player to: " + targetScene);

            GameSession.Instance.loadingTargetScene = "";
            GameSession.Instance.loadingReturnToPreviousScene = false;

            SceneManager.LoadScene(targetScene);
            yield break;
        }

        Debug.LogWarning("Loading screen had no target scene set.");
    }
}
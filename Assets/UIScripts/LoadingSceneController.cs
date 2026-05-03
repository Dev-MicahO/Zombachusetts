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
        float delay = 1f;

        if (GameSession.Instance != null)
            delay = GameSession.Instance.loadingScreenDuration;

        yield return new WaitForSeconds(delay);

        if (GameSession.Instance != null && GameSession.Instance.loadingReturnToPreviousScene)
        {
            GameSession.Instance.loadingReturnToPreviousScene = false;
            GameSession.Instance.loadingTargetScene = "";

            SceneChanger.Instance.PreviousScene();
            yield break;
        }

        if (GameSession.Instance != null && !string.IsNullOrEmpty(GameSession.Instance.loadingTargetScene))
        {
            string targetScene = GameSession.Instance.loadingTargetScene;
            GameSession.Instance.loadingTargetScene = "";

            SceneManager.LoadScene(targetScene);
            yield break;
        }

        Debug.LogWarning("Loading screen had no target scene set.");
    }
}
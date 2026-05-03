using UnityEngine;

public class FinalFightTrigger : MonoBehaviour
{
    private bool fightTriggered = false;
    void OnTriggerEnter2D(Collider2D other) 
    {
        Debug.Log(other.name);
       

        if(fightTriggered)
        {
            return;
        }

        
        if (GameSession.Instance == null)
        {
            Debug.LogError("GameSession instance not found.");
            return;
        }

        if (SceneChanger.Instance == null)
        {
            Debug.LogError("SceneChanger instance not found.");
            return;
        }

        fightTriggered = true;
        if (!GameSession.Instance.FinalBossDefeated)
        {
        GameSession.Instance.isRandomEncounter = false;
        GameSession.Instance.isFinalBossFight = true;
        GameSession.Instance.returnPlayerPosition = transform.position;
        GameSession.Instance.hasReturnPosition = true;

        // Send player to loading screen first, then Loading scene sends them to battle.
        GameSession.Instance.loadingTargetScene = "Battlescene";
        GameSession.Instance.loadingReturnToPreviousScene = false;
        GameSession.Instance.loadingScreenDuration = 0.25f;

        SceneChanger.Instance.LoadScene("Loading");
        }
    }

    }

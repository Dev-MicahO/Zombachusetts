using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance;

    [Header("Battle State")]
    public bool isRandomEncounter = false;
    public bool tutorialBattleCompleted = false;

    [Header("Return To Overworld")]
    public Vector3 returnPlayerPosition = Vector3.zero;
    public bool hasReturnPosition = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

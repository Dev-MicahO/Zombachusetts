using UnityEngine;

public class OverworldPlayerSpawn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (GameSession.Instance != null && GameSession.Instance.hasReturnPosition)
        {
            transform.position = GameSession.Instance.returnPlayerPosition;
            GameSession.Instance.hasReturnPosition = false;
        }
        
    }
}

using UnityEngine;

public class HealPickup : MonoBehaviour
{
    [SerializeField] private int healAmount = 25;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("HealPickup triggered by: " + other.name);

        if (!other.CompareTag("Player"))
            return;

        if (GameSession.Instance == null)
            return;

        if (GameSession.Instance.playerCurrentHP >= GameSession.Instance.playerMaxHP)
        {
            Debug.Log("Player is full HP, so pickup was not consumed.");
            return;
        }

        GameSession.Instance.HealPlayer(healAmount);
        Debug.Log("Health pack collected.");
        Destroy(gameObject);
    }
}
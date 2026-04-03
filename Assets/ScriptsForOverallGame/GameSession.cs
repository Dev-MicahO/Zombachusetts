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

    [Header("Persistent Player Combat Stats")]
    public int playerLevel = 1;
    public int playerXP = 0;
    public int xpToNextLevel = 100;

    public int playerMaxHP = 100;
    public int playerCurrentHP = 100;

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
    // Grabs stats from unit to save in gamesession.
    public void InitializePlayerStatsFromUnit(Unit playerUnit)
    {
        if (playerUnit == null)
            return;

        // Only initialize once, or whenever current HP is invalid
        if (playerMaxHP <= 0)
            playerMaxHP = playerUnit.maxHealth;

        if (playerCurrentHP <= 0 || playerCurrentHP > playerMaxHP)
            playerCurrentHP = playerMaxHP;
    }
    // Function to set the players hp when battle loads
    public void SetPlayerHP(int hp)
    {
        playerCurrentHP = Mathf.Clamp(hp, 0, playerMaxHP);
    }
    // Heal player after battle
    public void HealPlayer(int amount)
    {
        playerCurrentHP = Mathf.Clamp(playerCurrentHP + amount, 0, playerMaxHP);
    }

    // Here is how xp is added
    public void AddXP(int amount)
    {
        playerXP += amount;

        while (playerXP >= xpToNextLevel)
        {
            playerXP -= xpToNextLevel;
            LevelUp();
        }
    }

    // You leveled up congrats
    private void LevelUp()
    {
        playerLevel++;
        playerMaxHP += 10;
        playerCurrentHP = playerMaxHP;
        xpToNextLevel += 25;

        Debug.Log("Level up! Player is now level " + playerLevel);
    }
}
using Unity.VisualScripting;
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

    [Header("Player Class")]
    public PlayerClass selectedClass = PlayerClass.Warrior;

    [Header("Persistent Player Combat Stats")]
    public int playerLevel = 1;
    public int playerXP = 0;
    public int xpToNextLevel = 100;

    public int playerMaxHP = 100;
    public int playerCurrentHP = 100;

    public int playerMaxSP = 10;
    public int playerMinDamage = 10;
    public int playerMaxDamage = 25;

    [Header("Level Progression")]
    public int hpPerLevel = 10;
    public int spPerLevel = 2;
    public int minDamagePerLevel = 2;
    public int maxDamagePerLevel = 2;
    [Header("Party Members Acquired")]
    public bool hasPartyMember2 = false;
    public bool hasPartyMember3 = false;

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

    // Sets the player's class and applies base stats for that class.
    public void SetClass(PlayerClass newClass)
    {
        selectedClass = newClass;
        ApplyBaseClassStats();
    }

    // Sets base stats for each class.
    public void ApplyBaseClassStats()
    {
    switch (selectedClass)
        {
            case PlayerClass.Warrior:
                playerMaxHP = 100;
                playerMaxSP = 10;
                playerMinDamage = 15;
                playerMaxDamage = 25;

                hpPerLevel = 12;
                spPerLevel = 2;
                minDamagePerLevel = 2;
                maxDamagePerLevel = 3;
                break;

            case PlayerClass.Mage:
                playerMaxHP = 75;
                playerMaxSP = 16;
                playerMinDamage = 10;
                playerMaxDamage = 18;

                hpPerLevel = 8;
                spPerLevel = 4;
                minDamagePerLevel = 1;
                maxDamagePerLevel = 2;
                break;

            case PlayerClass.Doctor:
                playerMaxHP = 90;
                playerMaxSP = 14;
                playerMinDamage = 11;
                playerMaxDamage = 19;

                hpPerLevel = 10;
                spPerLevel = 3;
                minDamagePerLevel = 1;
                maxDamagePerLevel = 2;
                break;

            case PlayerClass.Thief:
                playerMaxHP = 85;
                playerMaxSP = 12;
                playerMinDamage = 13;
                playerMaxDamage = 22;

                hpPerLevel = 9;
                spPerLevel = 2;
                minDamagePerLevel = 2;
                maxDamagePerLevel = 2;
                break;
        }

        playerCurrentHP = playerMaxHP;
        playerLevel = 1;
        playerXP = 0;
        xpToNextLevel = 100;
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
        
        if (playerMinDamage <= 0)
            playerMinDamage = playerUnit.minDamage;

        if (playerMaxDamage <= 0)
            playerMaxDamage = playerUnit.maxDamage;
    }

    // Applies the current stats from gamesession to the player unit.
    public void ApplyStatsToUnit(Unit playerUnit)
    {
        if (playerUnit == null)
            return;

        playerUnit.maxHealth = playerMaxHP;
        playerUnit.currentHealth = playerCurrentHP;
        playerUnit.minDamage = playerMinDamage;
        playerUnit.maxDamage = playerMaxDamage;
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
        playerMaxHP += hpPerLevel;
        playerCurrentHP = playerMaxHP;
        playerMaxSP += spPerLevel;
        playerMinDamage += minDamagePerLevel;
        playerMaxDamage += maxDamagePerLevel;

        playerCurrentHP = playerMaxHP;
        xpToNextLevel += 25;

        Debug.Log("Level up! Player is now level " + playerLevel);
    }
}
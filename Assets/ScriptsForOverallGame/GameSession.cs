using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class GameSession : MonoBehaviour
{   
    public static GameSession Instance;

    public HashSet<string> destroyedObjects = new HashSet<string>();
    
    public bool GoldenBeagle = false;
    public bool SuspicousBrain = false;
    public bool RubyDagger = false;
    public bool KevlarVest = false;

    [Header("Loading Screen")]
    public string loadingTargetScene = "";
    public bool loadingReturnToPreviousScene = false;
    public float loadingScreenDuration = 1.0f;

    [Header("Battle State")]
    public bool isRandomEncounter = false;
    public bool isFinalBossFight = false;
    public bool FinalBossDefeated = false;
    public bool tutorialBattleCompleted = false;

    [Header("Return To Overworld")]
    public Vector3 returnPlayerPosition = Vector3.zero;
    public bool hasReturnPosition = false;
    
    [Header("Scene Tracking")]
    public string currentOverworldScene = "PlayerHouse";
    public string randomEncounterReturnScene = "";

    [Header("Player Class")]
    public PlayerClass selectedClass = PlayerClass.Warrior;

    [Header("Persistent Player Combat Stats")]
    public int playerLevel = 1;
    public int playerXP = 0;
    public int maxLevel = 10;
    private int[] xpRequiredPerLevel =
    {
    0,    // level 1
    50,   // level 2
    75,   // level 3
    150,  // level 4
    300,  // level 5
    650,  // level 6
    1200,  // level 7
    1800,  // level 8
    2400,  // level 9
    3000   // level 10
    };
    
    public enum EncounterArea
    {
        Forest,
        Cave,
        City,
    }

    [Header("Encounter Area")]
    public string currentEncounterArea = "Forest";
    public bool IsInSafeArea()
    {
        return currentEncounterArea.ToLower() == "safe";
    }

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

                hpPerLevel = 20;
                spPerLevel = 2;
                minDamagePerLevel = 5;
                maxDamagePerLevel = 10;
                break;

            case PlayerClass.Mage:
                playerMaxHP = 75;
                playerMaxSP = 16;
                playerMinDamage = 15;
                playerMaxDamage = 22;

                hpPerLevel = 10;
                spPerLevel = 2;
                minDamagePerLevel = 7;
                maxDamagePerLevel = 9;
                break;

            case PlayerClass.Doctor:
                playerMaxHP = 90;
                playerMaxSP = 14;
                playerMinDamage = 13;
                playerMaxDamage = 19;

                hpPerLevel = 15;
                spPerLevel = 3;
                minDamagePerLevel = 5;
                maxDamagePerLevel = 7;
                break;

            case PlayerClass.Thief:
                playerMaxHP = 85;
                playerMaxSP = 12;
                playerMinDamage = 15;
                playerMaxDamage = 22;

                hpPerLevel = 10;
                spPerLevel = 3;
                minDamagePerLevel = 6;
                maxDamagePerLevel = 8;
                break;
        }

        playerCurrentHP = playerMaxHP;
        playerLevel = 1;
        playerXP = 0;
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

        while (playerLevel < maxLevel && playerXP >= xpRequiredPerLevel[playerLevel])
        {
            LevelUp();
        }
    }
    // You leveled up congrats
    private void LevelUp()
    {
        if (playerLevel >= maxLevel)
            return;

        playerLevel++;
        playerMaxHP += hpPerLevel;
        playerCurrentHP = playerMaxHP;
        playerMaxSP += spPerLevel;
        playerMinDamage += minDamagePerLevel;
        playerMaxDamage += maxDamagePerLevel;

        playerCurrentHP = playerMaxHP;

        Debug.Log("Level up! Player is now level " + playerLevel);
    }

    public void setItemTrue(string itemName)
    {
        Debug.Log("setItemTrue called with: " + itemName);
        if(itemName == "Golden Beagle")
        {
            GoldenBeagle = true;
            Debug.Log("setItemTrue called with: " + itemName + " | Golden Beagle is: " + GoldenBeagle);
        }
        else if(itemName == "Suspicous Brain")
        {
            SuspicousBrain = true;
            Debug.Log("setItemTrue called with: " + itemName + " | Suspicous Brain is: " + SuspicousBrain);

        }
        else if(itemName == "Ruby Dagger")
        {
            RubyDagger = true;
            playerMinDamage += 5;
            playerMaxDamage += 5;
            Debug.Log("setItemTrue called with: " + itemName + " | Ruby Dagger is: " + RubyDagger);

        }
        else if(itemName == "Kevlar Vest")
        {
            KevlarVest = true;
            playerMaxHP = playerMaxHP + 20;
            Debug.Log("setItemTrue called with: " + itemName + " | Kevlar Vest is: " + KevlarVest);

        }
    }

    public void setPartyMemberTrue(string partyMember)
    {
        Debug.Log("Party Member equals" + partyMember);
        if(partyMember == "Big Bam")
        {
            hasPartyMember2 = true;
        }
        else if(partyMember == "Old Man")
        {
            hasPartyMember3 = true;
        }

    }

    public bool getItemStatus(string itemName)
    {
        Debug.Log("getItemStatus called with: " + itemName);
        if(itemName == "Golden Beagle")
        {
            Debug.Log("getItemStatus called with: " + itemName + " | Golden Beagle is: " + GoldenBeagle);
            return GoldenBeagle;
        }
        else if(itemName == "Suspicous Brain")
        {
            Debug.Log("getItemStatus called with: " + itemName + " | Suspicous Brain is: " + SuspicousBrain);
            return SuspicousBrain;
        }
        else if(itemName == "Ruby Dagger")
        {
            Debug.Log("getItemStatus called with: " + itemName + " | Ruby Dagger is: " + RubyDagger);
            return RubyDagger;
        }
        else if(itemName == "Kevlar Vest")
        {
            Debug.Log("getItemStatus called with: " + itemName + " | Kevlar Vest is: " + KevlarVest);
            return KevlarVest;
        }
        
        return false;
        
    }
}
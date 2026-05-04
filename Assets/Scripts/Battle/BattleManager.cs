using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

#region Data Classes
[System.Serializable]
public class EnemyVisualData
{
    public string enemyName;
    public Sprite enemySprite;
    public Vector3 enemyScale = Vector3.one;
}
#endregion

public class BattleManager : MonoBehaviour
{
    #region Inspector Fields and Runtime State
    public BattleState state;

    // Units
    [Header("Units")]
    public Unit playerUnit;
    public Unit enemyUnit;
    public Unit zombieUnit;
    public Unit zombie2Unit; // AOE will be hard so im going to add a second zombie after the 1st one dies
    public Unit zombie3Unit;

    public Unit bossUnit;
    public Unit scriptedBoss1Unit;
    public Unit scriptedBoss2Unit;
    public Unit wifeUnit;
    public Unit partyMember2Unit;
    public Unit partyMember3Unit;

    
    // Enemey Slots for mutli-enemy fights
    [Header("Enemy Slots")]
    public Transform enemySlot1;
    public Transform enemySlot2;
    public Transform enemySlot3;

   // Sprites  
    [Header("Player Sprites")]
    public SpriteRenderer playerSpriteRenderer;
    public Sprite playerNormalSprite;
    public Sprite playerBlockingSprite;

    // Battle Text
    [Header("Battle Text")]
    public TextMeshProUGUI battleText;
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI enemy2HPText;
    public TextMeshProUGUI enemy3HPText;
    public TextMeshProUGUI partyMember1HPText;
    public TextMeshProUGUI partyMember2HPText;
    public TextMeshProUGUI partyMember3HPText;
    public TextMeshProUGUI playerSPText;
    public TextMeshProUGUI Skill1ButtonText;
    public TextMeshProUGUI Skill2ButtonText;
    public TextMeshProUGUI Skill3ButtonText;
    public TextMeshProUGUI dialogueText;

    // Typewriter effect for Dialogue
    [Header("Dialogue Typewriter")]
    public float dialogueTypeSpeed = 0.06f;
    private Coroutine activeTypingCoroutine;

    // Skip Intro Function
    private bool skipCutsceneRequested = false;
    private bool isInCutscene = false;



    // HP Bars and SP Bars
    [Header("HP/SP Bars")]
    public Image playerHPBarFill;
    public Image partyMember1HPBarFill;
    public Image partyMember2HPBarFill;
    public Image partyMember3HPBarFill;
    public Image enemyHPBarFill;
    public Image enemy2HPBarFill;
    public Image enemy3HPBarFill;
    public Image playerSPBarFill;

    private float playerTargetHPFill;
    private float enemyTargetHPFill;
    private float enemy2TargetHPFill;
    private float enemy3TargetHPFill;
    private float partyMember1TargetHPFill;
    private float partyMember2TargetHPFill;
    private float partyMember3TargetHPFill;
    private float playerTargetSPFill;
    public float hpBarSpeed = 2f;

    // Action Buttons
    [Header("Action Buttons")]
    public Button attackButton;
    public Button defendButton;
    public Button skillButton;
    public Button fleeButton;

    // Skill Buttons
    [Header("Skill Buttons")]
    public Button Skill1Button;
    public Button Skill2Button;
    public Button Skill3Button;
    public Button backButton;
    
    [Header("Skill Unlock Levels")]
    public int skill2UnlockLevel = 3;
    public int skill3UnlockLevel = 5;
    // Target Buttons
    [Header("Target Buttons")]
    public Button targetButton1;
    public Button targetButton2;
    public Button targetButton3;
    public Button targetBackButton;

    public TextMeshProUGUI targetButton1Text;
    public TextMeshProUGUI targetButton2Text;
    public TextMeshProUGUI targetButton3Text;

    // Panels / UI
    [Header("UI Elements")]
    public GameObject damagePopupPrefab;
    public GameObject actionPanel;
    public GameObject skillPanel;
    public GameObject zombieObject;
    public GameObject zombie2Object;
    public GameObject zombie3Object;
    public GameObject bossObject;
    public GameObject scriptedBoss1Object;
    public GameObject scriptedBoss2Object;
    public GameObject wifeObject;
    public GameObject partyMember2Object;
    public GameObject partyMember3Object;
    public GameObject wifeUIObject;
    public GameObject skipHint;
    public GameObject targetPanel;

    // Damage Popup Points
    [Header("Damage Popup Points")]
    public Transform playerDamagePoint;
    public Transform enemyDamagePoint;
    public Transform enemy1DamagePoint;
    public Transform enemy2DamagePoint;
    public Transform enemy3DamagePoint;
    public Transform wifeDamagePoint;

    // Canvas
    [Header("Canvas")]
    public Canvas battleCanvas;
    public CanvasGroup transitionOverlay;

    // Player SP
    [Header("Player SP")]
    public int playerMaxSP = 10;
    public int playerCurrentSP = 10;
    public int spRegenPerTurn = 2;
    private bool playerUsedSkillLastTurn = false;

    // Skills
    [Header("Player Skills")]
    // Skill names and costs are stored in variables for easier management
    private string skill1Name;
    private string skill2Name;
    private string skill3Name;

    private int skill1Cost;
    private int skill2Cost;
    private int skill3Cost;

    // Targeting
    private enum TargetSelectionType
    {
        None,
        Enemy,
        Ally
    }

    private enum PendingTargetAction
    {
        None,
        BasicAttack,
        WarriorSkill1,
        MageSkill1,
        DoctorSkill1,
        DoctorSkill2,
        ThiefSkill1,
        ThiefSkill3
    }

    private TargetSelectionType currentTargetSelectionType = TargetSelectionType.None;
    private PendingTargetAction pendingTargetAction = PendingTargetAction.None;

    private List<Unit> currentSelectableTargets = new List<Unit>();

    // Warrior Skills
    [Header("Warrior Skills")]
    public int warriorSkill1Cost = 3;   // Shoulder Bash
    public int warriorSkill1MinDamage = 18;
    public int warriorSkill1MaxDamage = 24;

    public int warriorSkill2Cost = 5;   // All Out Attack
    public int warriorSkill2MinDamage = 28;
    public int warriorSkill2MaxDamage = 36;

    public int warriorSkill3Cost = 4;   // Rage
    public int warriorRageBonusDamage = 8;
    public int warriorRageDurationTurns = 2;

    // Mage Skills
    [Header("Mage Skills")]
    public int mageSkill1Cost = 4; // Blood Pact Bolt
    public int mageSkill1MinDamage = 30;
    public int mageSkill1MaxDamage = 40;
    public int mageSkill1SelfDamagePercent = 8; // Percentage of max health taken as self-damage

    public int mageSkill2Cost = 4; // Grasp of the Abyss
    public int mageSkill2MinDamage = 30;
    public int mageSkill2MaxDamage = 40;
    public int mageParalyzeChance = 30;

    public int mageSkill3Cost = 4; // Forbidden Knowledge
    public int mageFKBonusDamage = 12;
    public int mageFKDurationTurns = 3;
    public int mageSkill3SelfDamagePercent = 6; // Percentage of max health taken as self-damage
    public int mageConfusionChancePercent = 35;

    // Doctor Skills
    [Header("Doctor Skills")]
    public int doctorSkill1Cost = 3; // Patch Wounds
    public int doctorSkill1MinHeal = 20;
    public int doctorSkill1MaxHeal = 30;

    public int doctorSkill2Cost = 5; // Field Surgery
    public int doctorSkill2MinHeal = 40;
    public int doctorSkill2MaxHeal = 60;

    public int doctorSkill3Cost = 5; // Happy Gas
    public int doctorHGHealPerTurn;
    public int doctorSkill3BaseHeal = 6;
    public int doctorHGDurationTurns = 3;
    public int doctorHGDodgeBonusPercent = 15;
    
    // Thief Skills
    [Header("Thief Skills")]
    public int thiefSkill1Cost = 3; // Pocket Sand
    public int thiefBlindChancePercent = 40;
    public int thiefBlindDurationTurns = 2;

    public int thiefSkill2Cost = 4; // Stealth
    public int thiefStealthDurationTurns = 3;
    
    public int thiefSkill3Cost = 4; // Sneaky Strike
    public int thiefSkill3MinDamage = 18;
    public int thiefSkill3MaxDamage = 26;

    // CRITS!?
    [Header("Critical Hits")]
    public int playerCritChancePercent = 15;
    public float playerCritMultiplier = 1.5f;

    public int enemyCritChancePercent = 10;
    public float enemyCritMultiplier = 1.5f;
    // For balance purposes mage and thief should have higher crit stats
    [Header("Class Critical Hit Bonuses")]
    public int mageCritChancePercent = 30;
    public float mageCritMultiplier = 2.5f;

    public int thiefCritChancePercent = 40;
    public float thiefCritMultiplier = 1.75f;

    // Encounter / Boss progression
    private bool zombie2Spawned = false; // Flag to track if the second zombie has been spawned
    private bool bossFightStarted = false;
    private int bossMoveIndex = 0;
    private bool bossDefenseLowered = false;

    [Header("Boss Skills")]
    public int feralSwipeMinDamage = 8;
    public int feralSwipeMaxDamage = 14;
    public int infectedBiteMinDamage = 14;
    public int infectedBiteMaxDamage = 20;
    public int agonizedLungeMinDamage = 22;
    public int agonizedLungeMaxDamage = 30;

    // Random Encounter
    private bool isRandomEncounterBattle = false;
    private int currentRandomEncounterEnemyCount = 1;
    private bool isScriptedBossFight = false;

    [Header("Random Encounter Enemy Visuals")]
    public EnemyVisualData[] forestEnemyVisuals;
    public EnemyVisualData[] caveEnemyVisuals;
    public EnemyVisualData[] cityEnemyVisuals;

    // Base stats for random encounters (used to apply area scaling)
    private int zombieBaseMaxHP;
    private int zombieBaseMinDamage;
    private int zombieBaseMaxDamage;

    private int zombie2BaseMaxHP;
    private int zombie2BaseMinDamage;
    private int zombie2BaseMaxDamage;

    private int zombie3BaseMaxHP;
    private int zombie3BaseMinDamage;
    private int zombie3BaseMaxDamage;

    // Rewards Section
    [Header("Rewards")]
    public int randomEncounterXPReward = 25;
    public int randomEncounterVictoryHeal = 20;
    public int randomEncounterFleeHeal = 5;
    
    [Header("Area Encounter Scaling")]
    public int forestXPPerEnemy = 25;
    public int caveXPPerEnemy = 50;
    public int cityXPPerEnemy = 90;

    public float forestEnemyHealthMultiplier = 1.0f;
    public float caveEnemyHealthMultiplier = 2.5f;
    public float cityEnemyHealthMultiplier = 4.0f;

    public float forestEnemyDamageMultiplier = 1.0f;
    public float caveEnemyDamageMultiplier = 1.5f;
    public float cityEnemyDamageMultiplier = 2.0f;

    // Flee RNG
    [Header("Flee Settings")]
    [Range(0f, 1f)] public float fleeSuccessChance = 0.7f;

    // Tutorial
    private int tutorialStep = 0;

    // Player status effects
    
    // Bleed
    private bool playerBleeding = false;
    private int bleedTurnsRemaining = 0;
    // Rage
    private bool playerRageActive = false;
    private int rageTurnsRemaining = 0;

    // Forbidden Knowledge Confusion
    private bool PlayerFKActive = false;
    private int fkTurnsRemaining = 0;
    private bool playerConfused = false;
    private int confusionTurnsRemaining = 0;

    // Doctor Happy Gas + Field Surgery turn skip
    private bool doctorSkipNextTurn = false;
    private bool HGActive = false;
    private int HGTurnsRemaining = 0;

    // Thief Stealth
    private bool thiefStealthed = false;
    private int stealthTurnsRemaining = 0;
    private bool thiefNextAttackDoubleDamage = false;

    // Status Effects
    [Header("Bleed Effect")]
    public int bleedDamage = 5;
    public int bleedChancePercent = 30;
    public int bleedDuration = 2;

    private bool enemyParalyzed = false;
    private List<Unit> stunnedEnemies = new List<Unit>();
    private List<Unit> stunResistantEnemies = new List<Unit>();
    private int enemyParalyzedTurnsRemaining = 0;
    private bool enemyBlinded = false;
    private int enemyBlindedTurnsRemaining = 0;

    //Effects
    [Header("Hit Feedback")]
    // How long a target shakes when hit
    public float shakeDuration = 0.2f;

    // How much the target moves while shaking
    public float shakeMagnitude = 0.05f;

    // How long the target flashes when hit

    public float flashDuration = 0.15f;

    // The color the target flashes when hit (light red)
    public Color flashColor = new Color(1f, 0.7f, 0.7f, 1f);

    // The color the target flashes when Infected Bite hits (Green)
    public Color infectedBiteFlashColor = new Color(0.7f, 1f, 0.5f, 1f);

    // The color the player flashes when Rage is up
    public Color rageFlashColor = new Color(1f, 0.6f, 0.3f, 1f);

    // Damage Popup for crits (Yellow)
    public Color criticalDamageColor = Color.yellow;

    // Popup Color for bleed damage (Dark Red)
    public Color bleedDamageColor = new Color(0.65f, 0f, 0f, 1f);

    // Cheats
    [Header("Debug")]
    private bool debugMaxDamage = false;
    private bool debugRandomEncounterFromTutorial = false;

    // Original values to restore later
    private int originalMinDamage;
    private int originalMaxDamage;
    #endregion

    #region Unity Lifecycle Methods and Class Setup
    
    void Start()
    {
        Debug.Log("BattleManager Start called");
        
        if (GameSession.Instance != null)
        {
            GameSession.Instance.InitializePlayerStatsFromUnit(playerUnit);
            ApplyPersistentPlayerStats();
        }

        SelectedClassSetup();


        // Start both HP bars at full
        playerTargetHPFill = 1f;
        enemyTargetHPFill = 1f;

        // Start SP at max as well
        playerCurrentSP = playerMaxSP;
        playerTargetSPFill = 1f;

        // Set the first enemy to the zombie
        enemyUnit = zombieUnit;
        StoreBaseRandomEnemyStats();

        // Show zombie at start, hide boss until needed
        zombieObject.SetActive(true);
        zombie2Object.SetActive(false);
        zombie3Object.SetActive(false);
        bossObject.SetActive(false);

        // Hide scripted bosses until needed
        if (scriptedBoss1Object != null)
            scriptedBoss1Object.SetActive(false);

        if (scriptedBoss2Object != null)
            scriptedBoss2Object.SetActive(false);
        // Wife is visible during the first phase as a companion, then becomes the boss in the second phase.
        wifeObject.SetActive(true);
        wifeUIObject.SetActive(true);
        // Party Members are hidden until needed
        partyMember2Object.SetActive(false);
        partyMember3Object.SetActive(false);

        // Shows the main action panel and disables buttons until setup is done
        ShowActionPanel();
        SetActionButtonsInteractable(false);


        originalMinDamage = playerUnit.minDamage;
        originalMaxDamage = playerUnit.maxDamage;

        // Initialize transition UI (used for boss intro and Intro dialogue)
        transitionOverlay.alpha = 1f;
        transitionOverlay.blocksRaycasts = true;
        dialogueText.text = "";
        dialogueText.gameObject.SetActive(false);
        activeTypingCoroutine = null;
        skipHint.SetActive(false);

        // Start the battle setup sequence
        state = BattleState.START;
        if (GameSession.Instance != null)
        {
            isRandomEncounterBattle = GameSession.Instance.isRandomEncounter;
            isScriptedBossFight = GameSession.Instance.isFinalBossFight;
        }
        // Actually add good logic for setitng player health
        if (GameSession.Instance != null)
        {
            GameSession.Instance.InitializePlayerStatsFromUnit(playerUnit);
        }
        
        UpdateEnemyHPUIVisibility();

        if (isRandomEncounterBattle)
        {
            SetupRandomEncounterBattle();
        }
        else if (isScriptedBossFight)
        {
            SetupScriptedBossFight();
        }
        else
        {
            StartCoroutine(PlayIntroDialogue());
        }
    }
    
    // Method to get the current player class from the GameSession for easier access in BattleManager without directly referencing GameSession multiple times. This is used for setting up skills and other class-specific logic.
    PlayerClass GetCurrentPlayerClass()
    {
        if (GameSession.Instance != null)
            return GameSession.Instance.selectedClass;

        return PlayerClass.Warrior; // Default to Warrior if no GameSession found
    }
   
   // Method to set up skill names and costs based on the player's class. This is called at the start of the battle to initialize the skill UI and logic. 
    void SelectedClassSetup()
    {
        PlayerClass currentClass = GetCurrentPlayerClass();

        switch (currentClass)
        {
            case PlayerClass.Warrior:
                skill1Name = "Shoulder Bash";
                skill2Name = "All Out Attack";
                skill3Name = "Rage";
                
                skill1Cost = warriorSkill1Cost;
                skill2Cost = warriorSkill2Cost;
                skill3Cost = warriorSkill3Cost;
                break;
            
            case PlayerClass.Mage:
                skill1Name = "Blood Pact Bolt";
                skill2Name = "Grasp of the Abyss";
                skill3Name = "Forbidden Knowledge";
                
                skill1Cost = mageSkill1Cost;
                skill2Cost = mageSkill2Cost;
                skill3Cost = mageSkill3Cost;
                break;
            case PlayerClass.Doctor:
                skill1Name = "Patch Wounds";
                skill2Name = "Field Surgery";
                skill3Name = "Happy Gas";
                
                skill1Cost = doctorSkill1Cost;
                skill2Cost = doctorSkill2Cost;
                skill3Cost = doctorSkill3Cost;
                break;
            case PlayerClass.Thief:
                skill1Name = "Pocket Sand";
                skill2Name = "Stealth";
                skill3Name = "Sneaky Strike";
                
                skill1Cost = thiefSkill1Cost;
                skill2Cost = thiefSkill2Cost;
                skill3Cost = thiefSkill3Cost;
                break;
        }
    
    }

    

     void ApplyPersistentPlayerStats()
    {
        if (GameSession.Instance == null)
            return;

        GameSession.Instance.ApplyStatsToUnit(playerUnit);

        playerMaxSP = GameSession.Instance.playerMaxSP;
    }
    #endregion
    
    #region UI Helpers
    
    // Shows the normal action panel and hides the skill panel
    void ShowActionPanel()
    {
        actionPanel.SetActive(true);
        skillPanel.SetActive(false);
        targetPanel.SetActive(false);
    }

    // Shows the skill panel and hides the normal action panel
    void ShowSkillPanel()
    {
        actionPanel.SetActive(false);
        skillPanel.SetActive(true);
        targetPanel.SetActive(false);
    }

    // Shows the targeting panel and hides the normal action panel
    void ShowTargetPanel()
    {
        actionPanel.SetActive(false);
        skillPanel.SetActive(false);
        targetPanel.SetActive(true);
    }

    // Made a method for updating battle text i got tired of typing BattleText.text = "some message" in every method that needs to update the battle text.
    // Makes it easier to later add animations, typing effects, etc.
    void SetBattleText(string message)
    {
        battleText.text = message;
    }

        // Updates HP text and sets the target fill values for the HP bars Ensures UI always reflects current HP after changes
    void UpdateHPText()
    {
        UpdateEnemyHPUIVisibility();
        UpdatePartyMemberHPUI();

        playerHPText.text = playerUnit.unitName + " HP: " + playerUnit.currentHealth + "/" + playerUnit.maxHealth;
        playerTargetHPFill = (float)playerUnit.currentHealth / playerUnit.maxHealth;


        // Tutorial/story battle: always use ONLY the first enemy HP UI
        if (!isRandomEncounterBattle)
        {
            enemyHPText.text = enemyUnit.unitName + " HP: " + enemyUnit.currentHealth + "/" + enemyUnit.maxHealth;
            enemyTargetHPFill = (float)enemyUnit.currentHealth / enemyUnit.maxHealth;

            enemy2TargetHPFill = 0f;
            enemy3TargetHPFill = 0f;
            return;
        }

        // Random encounter: use separate enemy HP UI slots
        enemyHPText.text = zombieUnit.unitName + " HP: " + zombieUnit.currentHealth + "/" + zombieUnit.maxHealth;
        enemyTargetHPFill = (float)zombieUnit.currentHealth / zombieUnit.maxHealth;

        if (zombie2Object.activeSelf)
        {
            enemy2HPText.text = zombie2Unit.unitName + " HP: " + zombie2Unit.currentHealth + "/" + zombie2Unit.maxHealth;
            enemy2TargetHPFill = (float)zombie2Unit.currentHealth / zombie2Unit.maxHealth;
        }

        if (zombie3Object.activeSelf)
        {
            enemy3HPText.text = zombie3Unit.unitName + " HP: " + zombie3Unit.currentHealth + "/" + zombie3Unit.maxHealth;
            enemy3TargetHPFill = (float)zombie3Unit.currentHealth / zombie3Unit.maxHealth;
        }
    }
    
    // Methods for Party Member Health UI - Since the player can be joined by the wife in the first phase, and potentially other allies in future battles, we need a flexible way to update multiple party member HP UI elements. These methods handle showing/hiding the correct number of party member slots and updating their HP text and bars based on the current living party members.
    void UpdatePartyMemberHPUI()
    {
        UpdatePartySlotUI(0, wifeObject, wifeUnit, partyMember1HPText, partyMember1HPBarFill, ref partyMember1TargetHPFill);
        UpdatePartySlotUI(1, partyMember2Object, partyMember2Unit, partyMember2HPText, partyMember2HPBarFill, ref partyMember2TargetHPFill);
        UpdatePartySlotUI(2, partyMember3Object, partyMember3Unit, partyMember3HPText, partyMember3HPBarFill, ref partyMember3TargetHPFill);
    }
    void UpdatePartySlotUI(int slotIndex, GameObject memberObject, Unit memberUnit, TextMeshProUGUI hpText, Image hpBar, ref float targetFill)
    {
        bool hasMember = !bossFightStarted && memberObject != null && memberObject.activeSelf && memberUnit != null && !memberUnit.IsDead();

        if (hpText != null)
            hpText.gameObject.SetActive(hasMember);

        if (hpBar != null)
            hpBar.transform.parent.gameObject.SetActive(hasMember);

        if (!hasMember)
        {
            targetFill = 0f;
            return;
        }

        hpText.text = memberUnit.unitName + " HP: " + memberUnit.currentHealth + "/" + memberUnit.maxHealth;
        targetFill = (float)memberUnit.currentHealth / memberUnit.maxHealth;
    }

    // Shows or hides the second enemy's HP UI elements based on whether the second zombie is currently active in the battle. Called when updating HP text to ensure the correct UI is shown for the current enemy.
    void UpdateEnemyHPUIVisibility()
    {
        bool showExtraEnemyUI = isRandomEncounterBattle;

        bool showEnemy2UI = showExtraEnemyUI && zombie2Object.activeSelf;
        bool showEnemy3UI = showExtraEnemyUI && zombie3Object.activeSelf;

        if (enemy2HPText != null)
            enemy2HPText.gameObject.SetActive(showEnemy2UI);

        if (enemy2HPBarFill != null)
            enemy2HPBarFill.transform.parent.gameObject.SetActive(showEnemy2UI);

        if (enemy3HPText != null)
            enemy3HPText.gameObject.SetActive(showEnemy3UI);

        if (enemy3HPBarFill != null)
            enemy3HPBarFill.transform.parent.gameObject.SetActive(showEnemy3UI);
    }

    // Updates SP text and sets the target fill value for the SP bar Ensures UI always reflects current SP after changes
    void UpdateSPUI()
    {
        playerSPText.text = "SP: " + playerCurrentSP + "/" + playerMaxSP;
        playerTargetSPFill = (float)playerCurrentSP / playerMaxSP;
        UpdateSkillButtonsUI();
    }

    // Updates skill button labels and enables/disables them based on whether the player has enough SP or not.
    void UpdateSkillButtonsUI()
    {
        int playerLevel = 1;

        if (GameSession.Instance != null)
            playerLevel = GameSession.Instance.playerLevel;

        bool skill2Unlocked = playerLevel >= skill2UnlockLevel;
        bool skill3Unlocked = playerLevel >= skill3UnlockLevel;

        Skill1ButtonText.text = skill1Name + "\nSP " + skill1Cost;

        if (skill2Unlocked)
            Skill2ButtonText.text = skill2Name + "\nSP " + skill2Cost;
        else
            Skill2ButtonText.text = "Locked\nLv " + skill2UnlockLevel;

        if (skill3Unlocked)
            Skill3ButtonText.text = skill3Name + "\nSP " + skill3Cost;
        else
            Skill3ButtonText.text = "Locked\nLv " + skill3UnlockLevel;

        Skill1Button.interactable = playerCurrentSP >= skill1Cost;
        Skill2Button.interactable = skill2Unlocked && playerCurrentSP >= skill2Cost;
        Skill3Button.interactable = skill3Unlocked && playerCurrentSP >= skill3Cost;
    }

    // Instantly updates all UI elements (HP/SP text and bars)
    // Used when switching enemies or initializing battle state
    void RefreshBattleUIImmediate()
    {
        UpdateHPText();
        UpdateSPUI();

        playerHPBarFill.fillAmount = playerTargetHPFill;
        enemyHPBarFill.fillAmount = enemyTargetHPFill;
        if (enemy2HPBarFill != null)
        enemy2HPBarFill.fillAmount = enemy2TargetHPFill;
        if (enemy3HPBarFill != null)
            enemy3HPBarFill.fillAmount = enemy3TargetHPFill;
        playerSPBarFill.fillAmount = playerTargetSPFill;

        if (partyMember1HPBarFill != null)
            partyMember1HPBarFill.fillAmount = partyMember1TargetHPFill;

        if (partyMember2HPBarFill != null)
            partyMember2HPBarFill.fillAmount = partyMember2TargetHPFill;

        if (partyMember3HPBarFill != null)
            partyMember3HPBarFill.fillAmount = partyMember3TargetHPFill;
    }

    // Attempts to spend SP for a skill Returns false if not enough SP and shows error message to the player
    bool TrySpendSP(int cost)
    {
        if (playerCurrentSP < cost)
        {
            SetBattleText("Not enough SP!");
            return false;
        }

        playerCurrentSP -= cost;
        UpdateSPUI();
        return true;
    }

    // Enables or disables the main action buttons
    void SetActionButtonsInteractable(bool isInteractable)
    {
        attackButton.interactable = isInteractable;
        defendButton.interactable = isInteractable;
        skillButton.interactable = isInteractable;
        fleeButton.interactable = isInteractable;
    }
    #endregion

    #region Cutscenes and Tutorial Setup
    // Handles fading in/out the transition overlay used for the boss intro sequence.
    // Fades to a target alpha over a given duration and optionally blocks raycasts to prevent player input during transitions.
    IEnumerator FadeOverlay(float targetAlpha, float duration)
    {
        transitionOverlay.blocksRaycasts = true;

        float startAlpha = transitionOverlay.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transitionOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        transitionOverlay.alpha = targetAlpha;

        if (targetAlpha <= 0f)
        {
            transitionOverlay.blocksRaycasts = false;
        }
    }
    // Make skips look better
    IEnumerator QuickCutsceneSkipFlash()
    {
        transitionOverlay.blocksRaycasts = true;

        transitionOverlay.alpha = 1f;
        yield return new WaitForSeconds(0.15f);

        yield return StartCoroutine(FadeOverlay(0f, 0.35f));

        transitionOverlay.blocksRaycasts = false;
    }

    // New method for a typewriter effect
    IEnumerator TypeDialogue(string message)
    {
        dialogueText.text = "";

        foreach (char letter in message)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueTypeSpeed);
        }

        activeTypingCoroutine = null;
    }
    // Displays a dialogue message during transitions (like the boss intro) for a specified duration. Shows the dialogue text, waits, then can be hidden by the caller after the transition is complete.
    IEnumerator ShowTransitionDialogue(string message, float duration)
    {
        dialogueText.gameObject.SetActive(true);

        if (activeTypingCoroutine != null)
        {
            StopCoroutine(activeTypingCoroutine);
            activeTypingCoroutine = null;
        }

        dialogueText.text = "";

        activeTypingCoroutine = StartCoroutine(TypeDialogue(message));
        yield return activeTypingCoroutine;

        yield return new WaitForSeconds(duration);
    }
    
    // The Intro Dialogue
    IEnumerator PlayIntroDialogue()
    {
        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);
        skipHint.SetActive(true);

        isInCutscene = true;
        skipCutsceneRequested = false;

        // Fade to black
        yield return StartCoroutine(FadeOverlay(1f, 1f));
        if (ShouldSkipIntro()) goto SKIP;

        yield return new WaitForSeconds(0.2f);
        if (ShouldSkipIntro()) goto SKIP;

        yield return StartCoroutine(ShowTransitionDialogue("I just dropped off my kid at school...", 2f));
        if (ShouldSkipIntro()) goto SKIP;

        yield return StartCoroutine(ShowTransitionDialogue("And it was a Beautiful Day.", 2f));
        if (ShouldSkipIntro()) goto SKIP;

        yield return StartCoroutine(ShowTransitionDialogue("But then... the event happens...", 1.5f));
        if (ShouldSkipIntro()) goto SKIP;

        yield return StartCoroutine(ShowTransitionDialogue("WELCOME", 2.5f));
        if (ShouldSkipIntro()) goto SKIP;

        yield return StartCoroutine(ShowTransitionDialogue("TO", 2.5f));
        if (ShouldSkipIntro()) goto SKIP;

        yield return StartCoroutine(ShowTransitionDialogue("SUPER ZOMBACHUSETTS:\n WIFEHUNT 3\n \"THE THIRD EPIC SQUEAKEL\"\n THE RECKONING\n YOU WILL NOT SURVIVE\n 'ADVANCED WARFARE 2' TM ", 2.5f));
        if (ShouldSkipIntro()) goto SKIP;

        dialogueText.text = "";

        yield return StartCoroutine(FadeOverlay(0f, 1f));
        dialogueText.gameObject.SetActive(false);
        skipHint.SetActive(false);

        isInCutscene = false;
        yield return StartCoroutine(SetupBattle());
        yield break;

    SKIP:

        Debug.Log("Intro skipped");

        if (activeTypingCoroutine != null)
        {
            StopCoroutine(activeTypingCoroutine);
            activeTypingCoroutine = null;
        }

        dialogueText.text = "";
        dialogueText.gameObject.SetActive(false);
        skipHint.SetActive(false);

        // Snap to gameplay-ready state
        RefreshBattleUIImmediate();

        // Flash instead of hard cut
        yield return StartCoroutine(QuickCutsceneSkipFlash());

        isInCutscene = false;
        skipCutsceneRequested = false;

        // Continue into battle flow
        StartNextAllyPhase();
    }
    
    // Handles the opening setup for the battle
    IEnumerator SetupBattle()
    {
        SetActionButtonsInteractable(false);

        SetBattleText("A zombie appears!");
        RefreshBattleUIImmediate();

        tutorialStep = 0;

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

     IEnumerator StartSecondZombieFight()
    {
        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);

        SetBattleText(enemyUnit.unitName + " was defeated!");
        yield return new WaitForSeconds(1.5f);

        SetBattleText("Another zombie shambles forward!");
        yield return new WaitForSeconds(1.5f);

        zombieObject.SetActive(false);
        zombie2Object.SetActive(true);

        enemyUnit = zombie2Unit;
        ClearAllEnemyStunData();
        zombie2Spawned = true;

        RefreshBattleUIImmediate();

        SetBattleText(enemyUnit.unitName + " enters the battle!");
        yield return new WaitForSeconds(1.5f);

        StartNextAllyPhase();
    }

    // Will Spawn Zombie Clint
    IEnumerator StartScriptedBoss2Fight()
    {
        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);

        SetBattleText(scriptedBoss1Unit.unitName + " was defeated!");
        yield return new WaitForSeconds(1.5f);

        scriptedBoss1Object.SetActive(false);
        scriptedBoss2Object.SetActive(true);

        enemyUnit = scriptedBoss2Unit;
        ClearAllEnemyStunData();
        scriptedBoss2Unit.currentHealth = scriptedBoss2Unit.maxHealth;

        RefreshBattleUIImmediate();

        SetBattleText(scriptedBoss2Unit.unitName + " enters the battle!");
        yield return new WaitForSeconds(1.5f);

        StartNextAllyPhase();
    }

    // Handles the transition sequence when the wife becomes the boss.
    // This includes fading to black, showing dialogue about the transformation, swapping visuals from the tutorial phase to the boss fight, and then fading back in to start the boss fight.
    IEnumerator PlayBossTransition()
    {
        isInCutscene = true;
        skipCutsceneRequested = false;
        skipHint.SetActive(true);

        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);

        // Fade to black
        yield return StartCoroutine(FadeOverlay(1f, 1f));
        if (skipCutsceneRequested) goto SKIP;

        // Hide tutorial battle visuals
        zombie2Object.SetActive(false);
        zombie3Object.SetActive(false);
        wifeObject.SetActive(false);
        wifeUIObject.SetActive(false);
        // Safety check remove scripted bosses
        if (scriptedBoss1Object != null)
            scriptedBoss1Object.SetActive(false);

        if (scriptedBoss2Object != null)
            scriptedBoss2Object.SetActive(false);

        // Show dialogue
        yield return StartCoroutine(ShowTransitionDialogue(wifeUnit.unitName + " doesn't look well...", 2.5f));
        if (skipCutsceneRequested) goto SKIP;
        yield return StartCoroutine(ShowTransitionDialogue("Her breathing grows ragged.", 2.5f));
        if (skipCutsceneRequested) goto SKIP;
        yield return StartCoroutine(ShowTransitionDialogue("The infection takes hold.", 2.5f));
        if (skipCutsceneRequested) goto SKIP;
        yield return StartCoroutine( ShowTransitionDialogue(wifeUnit.unitName + " turns on you.", 2.5f));
        if (skipCutsceneRequested) goto SKIP;

        // Clear dialogue before fade-in
        dialogueText.text = "";

        // Set up boss fight
        bossObject.SetActive(true);
        bossFightStarted = true;
        enemyUnit = bossUnit;

        bossMoveIndex = 0;
        bossDefenseLowered = false;
        ClearAllEnemyStunData();

        RefreshBattleUIImmediate();
       
        // Fade back in
        yield return StartCoroutine(FadeOverlay(0f, 1f));

        dialogueText.gameObject.SetActive(false);

        SetBattleText(enemyUnit.unitName + " enters the battle!");
        yield return new WaitForSeconds(1.5f);
        isInCutscene = false;
        skipCutsceneRequested = false;
        skipHint.SetActive(false);

        StartNextAllyPhase();
        yield break;

        // Jump here if you dont wanna watch the cutscene :/ loser
    SKIP:

        Debug.Log("Boss transition skipped");

        if (activeTypingCoroutine != null)
        {
            StopCoroutine(activeTypingCoroutine);
            activeTypingCoroutine = null;
        }

        dialogueText.text = "";
        dialogueText.gameObject.SetActive(false);
        skipHint.SetActive(false);

        // Force visuals into boss state
        zombie2Object.SetActive(false);
        zombie3Object.SetActive(false);
        wifeObject.SetActive(false);
        wifeUIObject.SetActive(false);

        bossObject.SetActive(true);
        bossFightStarted = true;
        enemyUnit = bossUnit;

        bossMoveIndex = 0;
        bossDefenseLowered = false;
        ClearAllEnemyStunData();

        RefreshBattleUIImmediate();

        // Quick cinematic flash instead of a hard cut
        yield return StartCoroutine(QuickCutsceneSkipFlash());

        isInCutscene = false;
        skipCutsceneRequested = false;
        skipHint.SetActive(false);

        SetBattleText(enemyUnit.unitName + " enters the battle!");
        yield return new WaitForSeconds(1.5f);

        StartNextAllyPhase();
        yield break;
    }

    IEnumerator PlayFinalBossTransition()
    {
        isInCutscene = true;
        skipCutsceneRequested = false;
        skipHint.SetActive(true);

        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);

        // Fade to black
        yield return StartCoroutine(FadeOverlay(1f, 1f));
        if (skipCutsceneRequested) goto SKIP;

        // Hide final fight phase 2 boss
        if (scriptedBoss1Object != null)
            scriptedBoss1Object.SetActive(false);

        if (scriptedBoss2Object != null)
            scriptedBoss2Object.SetActive(false);

        zombieObject.SetActive(false);
        zombie2Object.SetActive(false);
        zombie3Object.SetActive(false);
        partyMember2Object.SetActive(false);
        partyMember3Object.SetActive(false);

        wifeObject.SetActive(false);
        wifeUIObject.SetActive(false);

        // TEMP final fight dialogue.
        yield return StartCoroutine(ShowTransitionDialogue("You hear heavy footsteps approaching.", 2.5f));
        if (skipCutsceneRequested) goto SKIP;

        yield return StartCoroutine(ShowTransitionDialogue("As they approach, \nyou realize the true extent of the threat.", 2.5f));
        if (skipCutsceneRequested) goto SKIP;

        yield return StartCoroutine(ShowTransitionDialogue("Duncan sheds a tear. \nas he sees the remnants \nof the one he once loved.", 2.5f));
        if (skipCutsceneRequested) goto SKIP;

        yield return StartCoroutine(ShowTransitionDialogue(wifeUnit.unitName + " now stands before you. \n For the final time..", 2.5f));
        if (skipCutsceneRequested) goto SKIP;


        yield return StartCoroutine(ShowTransitionDialogue("Your allies abandon you,\n Leaving this battle up to you.", 2.5f));
        if (skipCutsceneRequested) goto SKIP;

        dialogueText.text = "";

        // Set up wife boss phase
        bossObject.SetActive(true);
        bossFightStarted = true;
        enemyUnit = bossUnit;

        bossUnit.currentHealth = bossUnit.maxHealth;

        bossMoveIndex = 0;
        bossDefenseLowered = false;
        ClearAllEnemyStunData();
        enemyParalyzed = false;
        enemyBlinded = false;

        RefreshBattleUIImmediate();

        yield return StartCoroutine(FadeOverlay(0f, 1f));

        dialogueText.gameObject.SetActive(false);

        SetBattleText(enemyUnit.unitName + " enters the final battle!");
        yield return new WaitForSeconds(1.5f);

        isInCutscene = false;
        skipCutsceneRequested = false;
        skipHint.SetActive(false);

        StartNextAllyPhase();
        yield break;

    SKIP:

        Debug.Log("Final boss transition skipped");

        if (activeTypingCoroutine != null)
        {
            StopCoroutine(activeTypingCoroutine);
            activeTypingCoroutine = null;
        }

        dialogueText.text = "";
        dialogueText.gameObject.SetActive(false);
        skipHint.SetActive(false);

        if (scriptedBoss1Object != null)
            scriptedBoss1Object.SetActive(false);

        if (scriptedBoss2Object != null)
            scriptedBoss2Object.SetActive(false);

        zombieObject.SetActive(false);
        zombie2Object.SetActive(false);
        zombie3Object.SetActive(false);

        wifeObject.SetActive(false);
        wifeUIObject.SetActive(false);

        bossObject.SetActive(true);
        bossFightStarted = true;
        enemyUnit = bossUnit;

        bossUnit.currentHealth = bossUnit.maxHealth;

        bossMoveIndex = 0;
        bossDefenseLowered = false;
        ClearAllEnemyStunData();
        enemyParalyzed = false;
        enemyBlinded = false;

        RefreshBattleUIImmediate();

        yield return StartCoroutine(QuickCutsceneSkipFlash());

        isInCutscene = false;
        skipCutsceneRequested = false;
        skipHint.SetActive(false);

        SetBattleText(enemyUnit.unitName + " enters the final battle!");
        yield return new WaitForSeconds(1.5f);

        StartNextAllyPhase();
        yield break;
    }

    // Starts the second phase of battle by swapping from zombie to boss (This is handled by PlayBossTransition Now)
    IEnumerator StartBossFight()
    {
        yield return StartCoroutine(PlayBossTransition());
    }
   
    bool ShouldSkipIntro()
    {
        return skipCutsceneRequested;
    }
    #endregion

    #region Random Encounter Setup and Area Scaling
     // Different methods and helpers to setup different sprites/names/scales for random encounters
    EnemyVisualData[] GetCurrentEncounterVisualSet()
    {
        string area = "Forest";

        if (GameSession.Instance != null)
            area = GameSession.Instance.currentEncounterArea;

        area = area.ToLower();

        if (area == "cave")
            return caveEnemyVisuals;

        if (area == "city")
            return cityEnemyVisuals;

        return forestEnemyVisuals;
    }

    void ApplyRandomEncounterEnemyVisuals(int enemyCount)
    {
        EnemyVisualData[] visualSet = GetCurrentEncounterVisualSet();

        ApplyRandomVisualToEnemy(zombieObject, zombieUnit, visualSet, enemyCount, 1);

        if (enemyCount >= 2)
            ApplyRandomVisualToEnemy(zombie2Object, zombie2Unit, visualSet, enemyCount, 2);

        if (enemyCount == 3)
            ApplyRandomVisualToEnemy(zombie3Object, zombie3Unit, visualSet, enemyCount, 3);
    }

    void ApplyRandomVisualToEnemy(GameObject enemyObject, Unit enemyUnit, EnemyVisualData[] visualSet, int enemyCount, int enemyNumber)
    {
        if (enemyObject == null || enemyUnit == null)
            return;

        if (visualSet == null || visualSet.Length == 0)
            return;

        EnemyVisualData chosenVisual = visualSet[Random.Range(0, visualSet.Length)];

        SpriteRenderer spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && chosenVisual.enemySprite != null)
            spriteRenderer.sprite = chosenVisual.enemySprite;

        enemyObject.transform.localScale = chosenVisual.enemyScale;

        if (enemyCount > 1)
            enemyUnit.unitName = chosenVisual.enemyName + " " + enemyNumber;
        else
            enemyUnit.unitName = chosenVisual.enemyName;
    }
    // Methods and helpers for scaling enemy stats based on the current area
    string GetCurrentEncounterArea()
    {
        if (GameSession.Instance == null)
            return "forest";

        return GameSession.Instance.currentEncounterArea.ToLower();
    }

    float GetCurrentEnemyHealthMultiplier()
    {
        string area = GetCurrentEncounterArea();

        if (area == "cave")
            return caveEnemyHealthMultiplier;

        if (area == "city")
            return cityEnemyHealthMultiplier;

        return forestEnemyHealthMultiplier;
    }

    float GetCurrentEnemyDamageMultiplier()
    {
        string area = GetCurrentEncounterArea();

        if (area == "cave")
            return caveEnemyDamageMultiplier;

        if (area == "city")
            return cityEnemyDamageMultiplier;

        return forestEnemyDamageMultiplier;
    }

    int GetCurrentXPPerEnemy()
    {
        string area = GetCurrentEncounterArea();

        if (area == "cave")
            return caveXPPerEnemy;

        if (area == "city")
            return cityXPPerEnemy;

        return forestXPPerEnemy;
    }
    void ApplyRandomEncounterEnemyStats()
    {
        float hpMultiplier = GetCurrentEnemyHealthMultiplier();
        float damageMultiplier = GetCurrentEnemyDamageMultiplier();

        ApplyScaledEnemyStats(
            zombieUnit,
            zombieBaseMaxHP,
            zombieBaseMinDamage,
            zombieBaseMaxDamage,
            hpMultiplier,
            damageMultiplier
        );

        ApplyScaledEnemyStats(
            zombie2Unit,
            zombie2BaseMaxHP,
            zombie2BaseMinDamage,
            zombie2BaseMaxDamage,
            hpMultiplier,
            damageMultiplier
        );

        ApplyScaledEnemyStats(
            zombie3Unit,
            zombie3BaseMaxHP,
            zombie3BaseMinDamage,
            zombie3BaseMaxDamage,
            hpMultiplier,
            damageMultiplier
        );
    }

    void ApplyScaledEnemyStats(
        Unit enemy,
        int baseMaxHP,
        int baseMinDamage,
        int baseMaxDamage,
        float hpMultiplier,
        float damageMultiplier
    )
    {
        if (enemy == null)
            return;

        enemy.maxHealth = Mathf.RoundToInt(baseMaxHP * hpMultiplier);
        enemy.currentHealth = enemy.maxHealth;

        enemy.minDamage = Mathf.RoundToInt(baseMinDamage * damageMultiplier);
        enemy.maxDamage = Mathf.RoundToInt(baseMaxDamage * damageMultiplier);
    }

    // Stores the base stats of the random encounter enemies    
    void StoreBaseRandomEnemyStats()
    {
        zombieBaseMaxHP = zombieUnit.maxHealth;
        zombieBaseMinDamage = zombieUnit.minDamage;
        zombieBaseMaxDamage = zombieUnit.maxDamage;

        zombie2BaseMaxHP = zombie2Unit.maxHealth;
        zombie2BaseMinDamage = zombie2Unit.minDamage;
        zombie2BaseMaxDamage = zombie2Unit.maxDamage;

        zombie3BaseMaxHP = zombie3Unit.maxHealth;
        zombie3BaseMinDamage = zombie3Unit.minDamage;
        zombie3BaseMaxDamage = zombie3Unit.maxDamage;
    }

    // Method to setup a random encounter battle with just a zombie
    void SetupRandomEncounterBattle()
    {   
        // Re-Hide any leftover enemy visuals from the previous random encounter
        zombieObject.SetActive(false);
        zombie2Object.SetActive(false);
        zombie3Object.SetActive(false);
        bossObject.SetActive(false);

        int roll = Random.Range(1, 101);
        

        int enemyCount;
        // Random Chance between 1-3 enemies
        if (roll <= 50)
            enemyCount = 1;     // 50%
        else if (roll <= 85)
            enemyCount = 2;     // 35%
        else
            enemyCount = 3;     // 15%

        ApplyRandomEncounterEnemyVisuals(enemyCount);
        ApplyRandomEncounterEnemyStats();

        currentRandomEncounterEnemyCount = enemyCount;
        Debug.Log("Starting random encounter battle in area: " + GetCurrentEncounterArea());

        transitionOverlay.alpha = 0f;
        transitionOverlay.blocksRaycasts = false;
        dialogueText.gameObject.SetActive(false);
        activeTypingCoroutine = null;
        skipHint.SetActive(false);

        state = BattleState.PLAYERTURN;
        
        // Hide the wife in random encounters
        wifeObject.SetActive(false);
        wifeUIObject.SetActive(false);
        
        // Show party members in random encounter
        bool hasPM2 = GameSession.Instance != null && GameSession.Instance.hasPartyMember2;
        bool hasPM3 = GameSession.Instance != null && GameSession.Instance.hasPartyMember3;

        partyMember2Object.SetActive(hasPM2);
        partyMember3Object.SetActive(hasPM3);

        // Always spawn at least 1
        zombieObject.SetActive(true);
        zombieUnit.currentHealth = zombieUnit.maxHealth;

        if (enemySlot1 != null)
            zombieObject.transform.position = enemySlot1.position;

        // 2nd enemy
        if (enemyCount >= 2)
        {
            zombie2Object.SetActive(true);
            zombie2Unit.currentHealth = zombie2Unit.maxHealth;

            if (enemySlot2 != null)
                zombie2Object.transform.position = enemySlot2.position;
        }

        // 3rd enemy
        if (enemyCount == 3)
        {
            zombie3Object.SetActive(true);
            zombie3Unit.currentHealth = zombie3Unit.maxHealth;

            if (enemySlot3 != null)
                zombie3Object.transform.position = enemySlot3.position;
        }
         
        if (enemySlot1 != null)
        zombieObject.transform.position = enemySlot1.position;

        if (enemySlot2 != null)
        zombie2Object.transform.position = enemySlot2.position;

        if (enemySlot3 != null)
        zombie3Object.transform.position = enemySlot3.position;

        enemyUnit = zombieUnit;
        // Load persistent player stats into the battle unit
        if (GameSession.Instance != null)
        {
            GameSession.Instance.InitializePlayerStatsFromUnit(playerUnit);
            ApplyPersistentPlayerStats();
        }

        ClearAllEnemyStunData();

        // Reset SP
        playerCurrentSP = playerMaxSP;
        playerTargetSPFill = 1f;

        // Reset UI
        RefreshBattleUIImmediate();
        UpdatePartyMemberHPUI();

        SetBattleText("You have been ambushed!");
        ShowActionPanel();
        SetActionButtonsInteractable(true);
    }

    void SetupScriptedBossFight()
    {
        isScriptedBossFight = true;
        isRandomEncounterBattle = false;
        bossFightStarted = false;
        zombie2Spawned = false;

        if (GameSession.Instance != null)
        {
            GameSession.Instance.isRandomEncounter = false;
        }

        // Hide normal encounter/tutorial enemies
        zombieObject.SetActive(false);
        zombie2Object.SetActive(false);
        zombie3Object.SetActive(false);
        bossObject.SetActive(false);

        // Hide wife as companion for this fight
        wifeObject.SetActive(false);
        wifeUIObject.SetActive(false);

        // Show party members in boss fight
        bool hasPM2 = GameSession.Instance != null && GameSession.Instance.hasPartyMember2;
        bool hasPM3 = GameSession.Instance != null && GameSession.Instance.hasPartyMember3;

        partyMember2Object.SetActive(hasPM2);
        partyMember3Object.SetActive(hasPM3);

        // Show scripted boss 1
        scriptedBoss1Object.SetActive(true);
        scriptedBoss2Object.SetActive(false);

        scriptedBoss1Unit.currentHealth = scriptedBoss1Unit.maxHealth;
        scriptedBoss2Unit.currentHealth = scriptedBoss2Unit.maxHealth;
        bossUnit.currentHealth = bossUnit.maxHealth;

        enemyUnit = scriptedBoss1Unit;

        transitionOverlay.alpha = 0f;
        transitionOverlay.blocksRaycasts = false;
        dialogueText.gameObject.SetActive(false);
        activeTypingCoroutine = null;
        skipHint.SetActive(false);

        if (GameSession.Instance != null)
        {
            GameSession.Instance.InitializePlayerStatsFromUnit(playerUnit);
            ApplyPersistentPlayerStats();
        }

        playerCurrentSP = playerMaxSP;
        playerTargetSPFill = 1f;

        RefreshBattleUIImmediate();

        SetBattleText(scriptedBoss1Unit.unitName + " stands in your way!");
        ShowActionPanel();

        state = BattleState.PLAYERTURN;
        SetActionButtonsInteractable(true);
    }
    #endregion

    #region Combat Methods and Helpers
    // Struct to hold boss attack data for cleaner code when determining boss attacks (Refactored from multiple variables and if statements in EnemyTurn)
    private struct BossAttackData
    {
        public string attackName;
        public int damage;
        public string moveType;

        public BossAttackData(string attackName, int damage, string moveType)
        {
            this.attackName = attackName;
            this.damage = damage;
            this.moveType = moveType;
        }
    }

    /* Applies all temporary player damage modifiers:
    - Rage buff bonus damage
    - Boss defense break bonus (from Agonized Lunge)
    Returns final damage and flags used for combat text
    */
    int ApplyPlayerDamageBonuses(int baseDamage, out bool exploitedOpening, out bool rageBoosted)
    {
        int finalDamage = baseDamage;
        exploitedOpening = false;
        rageBoosted = false;

        // Rage adds bonus damage
        if (playerRageActive)
        {
            finalDamage += warriorRageBonusDamage;
            rageBoosted = true;
        }

        // Boss defense break from Agonized Lunge adds bonus damage once
        if (bossFightStarted && enemyUnit == bossUnit && bossDefenseLowered)
        {
            finalDamage += 5;
            bossDefenseLowered = false;
            exploitedOpening = true;
        }

        return finalDamage;
    }

    // He he boiah we getting real critty
    int ApplyCriticalHit(int baseDamage,int critChancePercent,float critMultiplier,out bool isCritical)
    {
        int critRoll = Random.Range(1, 101);
        isCritical = critRoll <= critChancePercent;

        if (isCritical)
        {
            return Mathf.RoundToInt(baseDamage * critMultiplier);
        }

        return baseDamage;
    }
    // Gets the appropriate crit chance based on player class
    int GetPlayerCritChance()
    {
        switch (GetCurrentPlayerClass())
        {
            case PlayerClass.Mage:
                return mageCritChancePercent;

            case PlayerClass.Thief:
                return thiefCritChancePercent;

            default:
                return playerCritChancePercent;
        }
    }
    // Gets the appropriate crit multiplier based on player class
    float GetPlayerCritMultiplier()
    {
        switch (GetCurrentPlayerClass())
        {
            case PlayerClass.Mage:
                return mageCritMultiplier;

            case PlayerClass.Thief:
                return thiefCritMultiplier;

            default:
                return playerCritMultiplier;
        }
    }

    /* Applies damage to the enemy and triggers all hit feedback:
    - Crits 1.5x Multiplier and different combat text
    - Shake animation
    - Flash effect
    - Damage popup
    - HP UI update
    */
    void DamageEnemy(Unit target, int damage, bool isCritical = false)
    {
        if (target == null)
            return;

        target.TakeDamage(damage);
        enemyUnit = target;

        StartCoroutine(ShakeTarget(target.transform));
        StartCoroutine(FlashTarget(GetUnitSpriteRenderer(target)));

        Transform popupPoint = GetDamagePointForEnemy(target);

        if (isCritical)
            ShowDamagePopup(popupPoint, damage, "-", criticalDamageColor);
        else
            ShowDamagePopup(popupPoint, damage);

        UpdateHPText();
    }

    // AOE version of DamageEnemy that can hit multiple targets 
    void DamageAllEnemies(int damage, bool isCritical = false)
    {
        List<Unit> enemies = GetLivingEnemies();

        foreach (Unit target in enemies)
        {
            target.TakeDamage(damage);
            StartCoroutine(ShakeTarget(target.transform));
            StartCoroutine(FlashTarget(GetUnitSpriteRenderer(target)));

            Transform popupPoint = GetDamagePointForEnemy(target);

            if (isCritical)
                ShowDamagePopup(popupPoint, damage, "-", criticalDamageColor);
            else
                ShowDamagePopup(popupPoint, damage);
        }

        if (enemies.Count > 0)
        {
            enemyUnit = enemies[0];
        }

        UpdateHPText();
    }
    
    /*Applies damage to a party member (player OR wife) and triggers hit feedback.
    
    Includes:
    - Damage application
    - Shake effects (stronger for boss Lunge)
    - Flash effects (green for Bite, normal otherwise)
    - Damage popup (supports crit color)
    - HP UI update
    
    This replaces the old DamagePlayer() and allows enemies to target multiple allies.
    */
    void DamageAlly(Unit target, int damage, string bossMoveType, bool isCritical = false)
    {
        target.TakeDamage(damage);
        // Actually update HP for all sessions
        if (target == playerUnit && isRandomEncounterBattle)
        {
            SyncPlayerHPToSession();
        }

        if (bossMoveType == "Lunge")
            StartCoroutine(ShakeTargetCustom(target.transform, 0.5f, 0.2f));
        else
            StartCoroutine(ShakeTarget(target.transform));

        if (bossMoveType == "Bite")
            StartCoroutine(FlashTargetColor(GetUnitSpriteRenderer(target), infectedBiteFlashColor));
        else
            StartCoroutine(FlashTarget(GetUnitSpriteRenderer(target)));

        Transform damagePoint = GetDamagePointForAlly(target);

        if (isCritical)
            ShowDamagePopup(damagePoint, damage, "-", criticalDamageColor);
        else
            ShowDamagePopup(damagePoint, damage);

        UpdateHPText();
    }


    /* Determines which boss move to use based on turn order (rotation) (Refactored from EnemyTurn for cleaner code and separation of concerns) There was a lot of spaghetti code ....
    Also handles special effects like enabling defense break from Lunge
    Returns packaged attack data for EnemyTurn()
    */
    BossAttackData GetBossAttack()
    {
        BossAttackData attackData;

        if (bossMoveIndex == 0)
        {
            attackData = new BossAttackData(
                enemyUnit.unitName + " uses Feral Swipe!",
                Random.Range(feralSwipeMinDamage, feralSwipeMaxDamage + 1),
                "Swipe"
            );
        }
        else if (bossMoveIndex == 1)
        {
            attackData = new BossAttackData(
                enemyUnit.unitName + " uses Infected Bite!",
                Random.Range(infectedBiteMinDamage, infectedBiteMaxDamage + 1),
                "Bite"
            );
        }
        else
        {
            bossDefenseLowered = true;
            attackData = new BossAttackData(
                enemyUnit.unitName + " uses Agonized Lunge!",
                Random.Range(agonizedLungeMinDamage, agonizedLungeMaxDamage + 1),
                "Lunge"
            );
        }

        bossMoveIndex++;
        if (bossMoveIndex > 2)
            bossMoveIndex = 0;

        return attackData;
    }

    /* These helper methods generate combat text based on: (Refactored from multiple if statements in PlayerAttack, PlayerShoulderBash, and PlayerAllOutAttack for cleaner code and separation of concerns)
    - Whether Rage boosted the attack
    - Whether the boss defense break was exploited
    Keeps combat text logic separate from gameplay logic
    */
    string GetBasicAttackMessage(int damage, bool exploitedOpening, bool rageBoosted)
    {
        if (exploitedOpening && rageBoosted)
            return "Rage and the opening let you deal " + damage + " damage!";
        if (exploitedOpening)
            return "You exploited the opening and dealt " + damage + " damage!";
        if (rageBoosted)
            return "Fueled by Rage, you dealt " + damage + " damage!";

        return "You attacked for " + damage + " damage!";
    }
    string GetShoulderBashMessage(int damage, bool exploitedOpening, bool rageBoosted)
    {
        if (exploitedOpening && rageBoosted)
            return "Rage empowers Shoulder Bash for " + damage + " damage and a stun!";
        if (exploitedOpening)
            return "Shoulder Bash exploits the opening and deals " + damage + " damage!";
        if (rageBoosted)
            return "Rage empowers Shoulder Bash for " + damage + " damage and a stun!";

        return "Shoulder Bash deals " + damage + " damage and stuns the enemy!";
    }

    string GetAllOutAttackMessage(int damage, bool exploitedOpening, bool rageBoosted)
    {
        if (exploitedOpening && rageBoosted)
            return "Rage and the opening power All Out Attack for " + damage + " damage!";
        if (exploitedOpening)
            return "All Out Attack exploits the opening and deals " + damage + " damage!";
        if (rageBoosted)
            return "Rage powers All Out Attack for " + damage + " damage!";

        return "All Out Attack deals " + damage + " damage!";
    }
    #endregion

    #region Visual Feedback Helpers    
    void SetPlayerBlockingSprite()
    {
        if (playerSpriteRenderer != null && playerBlockingSprite != null)
        {
            playerSpriteRenderer.sprite = playerBlockingSprite;
        }
    }

    void SetPlayerNormalSprite()
    {
        if (playerSpriteRenderer != null && playerNormalSprite != null)
        {
            playerSpriteRenderer.sprite = playerNormalSprite;
        }
    }

    // Spawns a floating damage number at a given world position
    void ShowDamagePopup(
        Transform damagePoint,
        int damage,
        string prefix = "-",
        Color? textColor = null
    )
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(damagePoint.position);

        GameObject popupObj = Instantiate(damagePopupPrefab, battleCanvas.transform);

        RectTransform popupRect = popupObj.GetComponent<RectTransform>();
        popupRect.position = screenPos;

        DamagePopup popup = popupObj.GetComponent<DamagePopup>();
        popup.Setup(damage, prefix, textColor);
    }

        // Standard shake effect for almost every hit
    IEnumerator ShakeTarget(Transform target)
    {
        Vector3 originalPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-shakeMagnitude, shakeMagnitude);
            float offsetY = Random.Range(-shakeMagnitude, shakeMagnitude);

            target.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPosition;
    }

    // This is the method to have a Stronger shake used for heavy attacks (boss abilities, etc.)
    IEnumerator ShakeTargetCustom(Transform target, float duration, float magnitude)
    {
        Vector3 originalPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-magnitude, magnitude);
            float offsetY = Random.Range(-magnitude, magnitude);

            target.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPosition;
    }

    // Default flash (light red) used when taking damage
    IEnumerator FlashTarget(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null)
            yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
    }

    // Custom flash color for special effects (Infected bite, rage, etc.)
    IEnumerator FlashTargetColor(SpriteRenderer spriteRenderer, Color customColor)
    {
        if (spriteRenderer == null)
            yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = customColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
    }

    // Gets the SpriteRenderer from a Unit object
    SpriteRenderer GetUnitSpriteRenderer(Unit unit)
    {
        return unit.GetComponent<SpriteRenderer>();
    }
    #endregion
    
    #region Turn Flow and Status Effects
    /* Begins the player's turn:
    - Shows action panel
    - Starts turn logic (bleed, SP regen, etc.)
    */
    void PlayerTurn()
    {
        ShowActionPanel();
        SetActionButtonsInteractable(false);
        StartCoroutine(BeginPlayerTurn());
    }

    /* Seperate method Handles start-of-turn effects:
    - Applies bleed damage
    - Regenerates SP
    Then transitions into input phase
    */
    IEnumerator BeginPlayerTurn()
    {
        if (playerUnit.IsDead())
        {
            StartCoroutine(PartyTurn());
            yield break;
        }
        // Doctor Field Surgery Turn Skip  Logic
        if (doctorSkipNextTurn)
        {
            doctorSkipNextTurn = false;
            SetBattleText(playerUnit.unitName + " must clean the equipment and skips this turn!");
            yield return new WaitForSeconds(1f);

            if (!wifeUnit.IsDead())
            {
                state = BattleState.BUSY;
                StartCoroutine(PartyTurn());
            }
            else
            {
                state = BattleState.ENEMYTURN;
                StartCoroutine(EnemyGroupTurn());
            }

            yield break;
        }

        if (playerBleeding)
        {
            yield return StartCoroutine(ApplyBleedEffect());

            if (playerUnit.IsDead())
            {
                if (IsPartyDefeated())
                {
                    state = BattleState.LOST;
                    StartCoroutine(EndBattle());
                }
                else
                {
                    StartCoroutine(PartyTurn());
                }

                yield break;
            }
        }

        // Happy Gas Logic
        if (HGActive)
        {
            int gasHeal = doctorHGHealPerTurn;
            playerUnit.currentHealth = Mathf.Min(playerUnit.maxHealth, playerUnit.currentHealth + gasHeal);

            if (isRandomEncounterBattle)
            {
                SyncPlayerHPToSession();
            }

            UpdateHPText();
            ShowDamagePopup(playerDamagePoint, gasHeal, "+", Color.green);
            SetBattleText("Happy Gas restores " + gasHeal + " HP!");
            yield return new WaitForSeconds(0.75f);

            HGTurnsRemaining--;
            if (HGTurnsRemaining <= 0)
            {
                HGActive = false;
            }
        }

        // Confusion Logic

        if (playerConfused)
        {
            int confusionRoll = Random.Range(1, 101);

            if (confusionRoll <= mageConfusionChancePercent)
            {
                int selfHit = Mathf.Max(1, playerUnit.GetDamage() / 4);
                playerUnit.TakeDamage(selfHit);

                if (isRandomEncounterBattle)
                {
                    SyncPlayerHPToSession();
                }

                UpdateHPText();
                
                // You died to confusion lmfao
                if (playerUnit.IsDead())
                {
                    if (IsPartyDefeated())
                    {
                        state = BattleState.LOST;
                        StartCoroutine(EndBattle());
                    }
                    else
                    {
                        StartCoroutine(PartyTurn());
                    }

                    yield break;
                }

                ShowDamagePopup(playerDamagePoint, selfHit, "-", criticalDamageColor);
                SetBattleText(playerUnit.unitName + " is confused and hurts themself!");
                yield return new WaitForSeconds(1f);
            }

            confusionTurnsRemaining--;
            if (confusionTurnsRemaining <= 0)
            {
                playerConfused = false;
            }
        }

        if (thiefStealthed)
        {
            stealthTurnsRemaining--;

            if (stealthTurnsRemaining <= 0)
            {
                thiefStealthed = false;
                thiefNextAttackDoubleDamage = false;
                SetBattleText("You slip out of stealth.");
                yield return new WaitForSeconds(0.75f);
            }
        }

        if (!playerUsedSkillLastTurn)
        {
            playerCurrentSP = Mathf.Min(playerMaxSP, playerCurrentSP + spRegenPerTurn);
        }
        else
        {
            yield return new WaitForSeconds(0.75f);
        }

        playerUsedSkillLastTurn = false;
        UpdateSPUI();

        StartPlayerInputPhase();
    }

    // Enables player input after all start-of-turn effects are resolved dont ask why theres 3 player methods it was to help cleanup the spam of code in PlayerTurn and make it easier to read and manage overall
    void StartPlayerInputPhase()
    {
        SetActionButtonsInteractable(true);
        backButton.interactable = true;

        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit)
        {
            // First part of tutorial Explain Attack Button
            if (tutorialStep == 0)
            {
                SetBattleText("Tutorial:Use your basic attack to fight the zombie!");
                defendButton.interactable = false;
                skillButton.interactable = false;
                fleeButton.interactable = false;
                return;
            }
            // Second Part of the tutorial Explain Defend Button
            if (tutorialStep == 1)
            {
                SetBattleText("Tutorial: Use the defend button to take less damage for a turn.");
                attackButton.interactable = false;
                skillButton.interactable = false;
                fleeButton.interactable = false;
                return;
            }
            //Third Part of the tutorial Explain Skills and SP
            if (tutorialStep == 2)
            {
                SetBattleText("Tutorial: SP is required to use skills and regenerates when you don't use a skill, Try opening the skills panel!");
                attackButton.interactable = false;
                defendButton.interactable = false;
                skillButton.interactable = true;
                fleeButton.interactable = false;
                return;
            }

            //Fourth Part of the tutorial explain Flee button and end
            if (tutorialStep == 3)
            {
                SetBattleText("Tutorial: Try to flee.");
                attackButton.interactable = false;
                defendButton.interactable = false;
                skillButton.interactable = false;
                fleeButton.interactable = true;
                return;
            }
            SetBattleText("Choose an action.");
        }
    }

    // WOO HOOO ANOTHER PLAYER METHOD :D
    /* This one was for the Common setup for all player actions:
    - Sets state to BUSY
    - Disables buttons
    - Optionally returns UI to main action panel
    Prevents repeated setup code across actions basically state = Bat..and show action panel :)
    */
    void BeginPlayerAction(bool returnToActionPanel = true)
    {
        state = BattleState.BUSY;

        if (returnToActionPanel)
        {
            ShowActionPanel();
        }

        SetActionButtonsInteractable(false);
    }

    /* Wowsers not a player method :D
     Applies bleed damage over time:
    - Deals damage at start of turn
    - Decreases duration
    - Removes effect when finished
    */
    IEnumerator ApplyBleedEffect()
    {
        if (!playerBleeding)
            yield break;

        SetBattleText(playerUnit.unitName + " suffers bleed damage!");
        yield return new WaitForSeconds(0.75f);

        playerUnit.TakeDamage(bleedDamage);
        // Doesn't do anything right now but if random enemies get skills this will already be done
        if (isRandomEncounterBattle)
        {
            SyncPlayerHPToSession();
        }
        StartCoroutine(ShakeTarget(playerUnit.transform));
        StartCoroutine(FlashTarget(GetUnitSpriteRenderer(playerUnit)));
        ShowDamagePopup(playerDamagePoint, bleedDamage, "-", bleedDamageColor);
        UpdateHPText();

        bleedTurnsRemaining--;

        if (bleedTurnsRemaining <= 0)
        {
            playerBleeding = false;
            SetBattleText(playerUnit.unitName + " stops bleeding.");
            yield return new WaitForSeconds(0.75f);
        }
        else
        {
            SetBattleText(playerUnit.unitName + " bleeds for " + bleedDamage + " damage!");
            yield return new WaitForSeconds(0.75f);
        }
    }

    // Handles skipping the enemy turn when stunned -> Immediately returns control to the player
    IEnumerator HandleEnemyStunSkip(Unit stunnedEnemy)
    {
        RemoveStunFromEnemy(stunnedEnemy);

        SetBattleText(stunnedEnemy.unitName + " is stunned and cannot move!");
        yield return new WaitForSeconds(1f);
    }
    // Overhauled Stun Mechanics
    bool IsEnemyStunned(Unit enemy)
    {
        return enemy != null && stunnedEnemies.Contains(enemy);
    }

    void ApplyStunToEnemy(Unit enemy)
    {
        if (enemy == null)
            return;

        if (!stunnedEnemies.Contains(enemy))
            stunnedEnemies.Add(enemy);

        if (!stunResistantEnemies.Contains(enemy))
            stunResistantEnemies.Add(enemy);
    }

    void RemoveStunFromEnemy(Unit enemy)
    {
        if (enemy == null)
            return;

        if (stunnedEnemies.Contains(enemy))
            stunnedEnemies.Remove(enemy);
    }

    bool EnemyResistsStun(Unit enemy)
    {
        return enemy != null && stunResistantEnemies.Contains(enemy);
    }

    void RemoveStunResistance(Unit enemy)
    {
        if (enemy == null)
            return;

        if (stunResistantEnemies.Contains(enemy))
            stunResistantEnemies.Remove(enemy);
    }

    void ClearExpiredStunResistanceAfterPlayerAction()
    {
        // After the player chooses any non-Shoulder-Bash action,
        // enemies stop resisting stun on the next player turn.
        stunResistantEnemies.Clear();
    }

    void ClearAllEnemyStunData()
    {
        stunnedEnemies.Clear();
        stunResistantEnemies.Clear();
    }

    /* Handles what happens after the player attacks:
 - If enemies die → transition to boss or win
 - Otherwise → proceed to enemy turn
 More refactoring to clean up code overall
 */
    IEnumerator ResolveEnemyDefeatOrContinue()
    {
        yield return new WaitForSeconds(1.5f);
        HideDefeatedEnemies();
        

        if (enemyUnit.IsDead())
        {    
            if (TryHandleScriptedBossDefeat())
                yield break;

            // RANDOM ENCOUNTER END
            if (isRandomEncounterBattle)
            {   
                if (AreAllEnemiesDefeated())
                {
                    ApplyRandomEncounterVictoryRewards();
                    state = BattleState.WON;
                    StartCoroutine(ReturnToOverworldAfterBattle());
                    yield break;
                }

                // SCRIPTED BOSS FIGHT PHASES
                if (isScriptedBossFight)
                {
                    if (enemyUnit == scriptedBoss1Unit)
                    {
                        StartCoroutine(StartScriptedBoss2Fight());
                        yield break;
                    }
                    else if (enemyUnit == scriptedBoss2Unit)
                    {
                        StartCoroutine(PlayBossTransition());
                        yield break;
                    }
                    else if (enemyUnit == bossUnit)
                    {
                        state = BattleState.WON;
                        StartCoroutine(EndBattle());
                        yield break;
                    }
                }
                else
                {
                    List<Unit> remainingEnemies = GetLivingEnemies();

                    if (remainingEnemies.Count > 0)
                    {
                        enemyUnit = remainingEnemies[0];
                        UpdateHPText();
                    }

                    SetBattleText("Enemy defeated!");
                    yield return new WaitForSeconds(1f);

                    state = BattleState.ENEMYTURN;
                    StartCoroutine(EnemyGroupTurn());
                    yield break;
                }
            }

            // Zombie 1 dies -> bring in Zombie 2
            if (!zombie2Spawned && enemyUnit == zombieUnit)
            {
                StartCoroutine(StartSecondZombieFight());
            }
            // Zombie 2 dies -> begin wife boss transition
            else if (!bossFightStarted && enemyUnit == zombie2Unit)
            {
                StartCoroutine(PlayBossTransition());
            }
            // Boss dies -> win
            else
            {
                state = BattleState.WON;
                StartCoroutine(EndBattle());
            }
        }
        else
        {
            if (!wifeUnit.IsDead() && !bossFightStarted)
            {
                state = BattleState.BUSY;
                StartCoroutine(PartyTurn());
            }
            else
            {
                state = BattleState.ENEMYTURN;
                StartCoroutine(EnemyGroupTurn());
            }
        }
    }

    /* Handles what happens after the enemy attacks:
    - If player dies → end battle
    - Otherwise → return to player turn
    */
    IEnumerator ResolvePlayerDefeatOrContinue()
    {
        yield return new WaitForSeconds(1.5f);

        StartNextAllyPhase();
    }

    // Reduces remaining Buff duration after an attack -> Disables Buff when duration expires
    void ConsumeBuffTurn()
    {
        bool rageExpiredThisTurn = false;
        bool fkExpiredThisTurn = false;

        if (playerRageActive)
        {
            rageTurnsRemaining--;

            if (rageTurnsRemaining <= 0)
            {
                playerRageActive = false;
                rageTurnsRemaining = 0;
                rageExpiredThisTurn = true;
            }
        }

        if (PlayerFKActive)
        {
            fkTurnsRemaining--;

            if (fkTurnsRemaining <= 0)
            {
                PlayerFKActive = false;
                fkTurnsRemaining = 0;
                fkExpiredThisTurn = true;
            }
        }

        if (rageExpiredThisTurn && fkExpiredThisTurn)
        {
            SetBattleText("Your empowering effects fade.");
        }
        else if (rageExpiredThisTurn)
        {
            SetBattleText("Rage fades.");
        }
        else if (fkExpiredThisTurn)
        {
            SetBattleText("Forbidden Knowledge fades.");
        }
    }
   
   /*Handles transition back to the player's side after the enemy finishes its turn.

    - If both allies are dead → end battle (LOSS)
    - If player is dead but wife is alive → skip player turn and go to PartyTurn()
    - Otherwise → start normal PlayerTurn()

    */
    void StartNextAllyPhase()
    {
        SetPlayerNormalSprite();
        
        if (IsPartyDefeated())
        {
            state = BattleState.LOST;
            StartCoroutine(EndBattle());
            return;
        }

        if (playerUnit.IsDead())
        {
            state = BattleState.BUSY;
            StartCoroutine(PartyTurn());
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    // Consolidated scripted boss defeat handling for PC & Party Members
    bool TryHandleScriptedBossDefeat()
    {
        if (!isScriptedBossFight)
            return false;

        if (enemyUnit == scriptedBoss1Unit)
        {
            StartCoroutine(StartScriptedBoss2Fight());
            return true;
        }

        if (enemyUnit == scriptedBoss2Unit)
        {
            StartCoroutine(PlayFinalBossTransition());
            return true;
        }

        if (enemyUnit == bossUnit)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle());
            return true;
        }

        return false;
    }
    #endregion

    #region Targeting and Button Handlers
    // Target Panel Logic
    void OpenTargetPanel(List<Unit> targets, TargetSelectionType selectionType, PendingTargetAction actionType)
    {
        currentSelectableTargets = targets;
        currentTargetSelectionType = selectionType;
        pendingTargetAction = actionType;

        ShowTargetPanel();

        targetButton1.gameObject.SetActive(false);
        targetButton2.gameObject.SetActive(false);
        targetButton3.gameObject.SetActive(false);

        if (targets.Count > 0)
        {
            targetButton1.gameObject.SetActive(true);
            targetButton1Text.text = targets[0].unitName;
        }

        if (targets.Count > 1)
        {
            targetButton2.gameObject.SetActive(true);
            targetButton2Text.text = targets[1].unitName;
        }

        if (targets.Count > 2)
        {
            targetButton3.gameObject.SetActive(true);
            targetButton3Text.text = targets[2].unitName;
        }

        targetBackButton.gameObject.SetActive(true);

        if (selectionType == TargetSelectionType.Enemy)
            SetBattleText("Choose an enemy target.");
        else
            SetBattleText("Choose an ally target.");
    }

    // Called when the Attack button is pressed
    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        // Prevent double clicking the attack button
        SetActionButtonsInteractable(false);

        List<Unit> enemies = GetLivingEnemies();

        if (enemies.Count == 0)
            return;

        if (enemies.Count == 1)
        {
            StartCoroutine(PlayerAttack(enemies[0]));
        }
        else
        {
            OpenTargetPanel(enemies, TargetSelectionType.Enemy, PendingTargetAction.BasicAttack);
        }
    }

    // Called when the Defend button is pressed
    public void OnDefendButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        SetActionButtonsInteractable(false);
        StartCoroutine(PlayerDefend());
    }

    // Called when the Skill button is pressed
    public void OnSkillButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        UpdateSkillButtonsUI();
        ShowSkillPanel();
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            backButton.interactable = false;
            SetBattleText("Tutorial: Use any skill.");
        }
        else
        {
            backButton.interactable = true;
            SetBattleText("Choose a skill.");
        }
    }

    // Called when the Back button on the skill panel is pressed
    public void OnBackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        return;

        ShowActionPanel();
        SetBattleText("Choose an action.");
    }

    // Called when the Flee button is pressed
    public void OnFleeButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        // Stop player from fleeing from Tutorial Battle
        if (!isRandomEncounterBattle)
        {
            SetBattleText("You Cannot Run From This Battle!");

        // End the tutorial when player uses flee button for the first time
        if (!bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 3)
        {
            tutorialStep = 4;
            SetActionButtonsInteractable(true);
        }
        return;
    }

        StartCoroutine(PlayerFlee());
    }

    // Called when the 1st skill button is pressed
    public void OnSkill1Button()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        switch (GetCurrentPlayerClass())
        {
            case PlayerClass.Warrior:
            {
                List<Unit> enemies = GetLivingEnemies();
                if (enemies.Count == 1)
                    StartCoroutine(PlayerShoulderBash(enemies[0]));
                else
                    OpenTargetPanel(enemies, TargetSelectionType.Enemy, PendingTargetAction.WarriorSkill1);
                break;
            }

            case PlayerClass.Mage:
            {
                List<Unit> enemies = GetLivingEnemies();
                if (enemies.Count == 1)
                    StartCoroutine(PlayerBloodPactBolt(enemies[0]));
                else
                    OpenTargetPanel(enemies, TargetSelectionType.Enemy, PendingTargetAction.MageSkill1);
                break;
            }

            case PlayerClass.Doctor:
            {
                List<Unit> allies = GetLivingAllies();
                if (allies.Count == 1)
                    StartCoroutine(PlayerPatchWounds(allies[0]));
                else
                    OpenTargetPanel(allies, TargetSelectionType.Ally, PendingTargetAction.DoctorSkill1);
                break;
            }

            case PlayerClass.Thief:
            {
                List<Unit> enemies = GetLivingEnemies();
                if (enemies.Count == 1)
                    StartCoroutine(PlayerPocketSand(enemies[0]));
                else
                    OpenTargetPanel(enemies, TargetSelectionType.Enemy, PendingTargetAction.ThiefSkill1);
                break;
            }
        }
    }

    // Called when the 2nd skill button is pressed
        public void OnSkill2Button()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        switch (GetCurrentPlayerClass())
        {
            case PlayerClass.Warrior:
                StartCoroutine(PlayerAllOutAttack());
                break;

            case PlayerClass.Mage:
                StartCoroutine(PlayerGraspOfTheAbyss());
                break;

            case PlayerClass.Doctor:
            {
                List<Unit> allies = GetLivingAllies();
                if (allies.Count == 1)
                    StartCoroutine(PlayerFieldSurgery(allies[0]));
                else
                    OpenTargetPanel(allies, TargetSelectionType.Ally, PendingTargetAction.DoctorSkill2);
                break;
            }

            case PlayerClass.Thief:
                StartCoroutine(PlayerStealth());
                break;
        }
    }

    // Called when the 3rd skill button is pressed
    public void OnSkill3Button()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        switch (GetCurrentPlayerClass())
        {
            case PlayerClass.Warrior:
                StartCoroutine(PlayerRage());
                break;

            case PlayerClass.Mage:
                StartCoroutine(PlayerForbiddenKnowledge());
                break;

            case PlayerClass.Doctor:
                StartCoroutine(PlayerHappyGas());
                break;

            case PlayerClass.Thief:
            {
                List<Unit> enemies = GetLivingEnemies();
                if (enemies.Count == 1)
                    StartCoroutine(PlayerSneakyStrike(enemies[0]));
                else
                    OpenTargetPanel(enemies, TargetSelectionType.Enemy, PendingTargetAction.ThiefSkill3);
                break;
            }
        }
    }
    
    // Target selection button for enemy 1
    public void OnTargetButton1()
    {
        SelectTargetByIndex(0);
    }
    // Target selection button for enemy 2
    public void OnTargetButton2()
    {
        SelectTargetByIndex(1);
    }
    // Target selection button for enemy 3
    public void OnTargetButton3()
    {
        SelectTargetByIndex(2);
    }

    // Seperate back button logic for targeting panel so stuff doesnt break. Allows player to back out of target selection without performing an action and returns them to the main action panel.
    public void OnTargetBackButton()
    {
        currentSelectableTargets.Clear();
        currentTargetSelectionType = TargetSelectionType.None;
        pendingTargetAction = PendingTargetAction.None;

        ShowActionPanel();
        SetActionButtonsInteractable(true);
        SetBattleText("Choose an action.");
    }

    // Select Target Method
    void SelectTargetByIndex(int index)
    {
        if (index < 0 || index >= currentSelectableTargets.Count)
            return;
        
        SetActionButtonsInteractable(false);

        Unit selectedTarget = currentSelectableTargets[index];

        currentSelectableTargets.Clear();
        currentTargetSelectionType = TargetSelectionType.None;

        PendingTargetAction actionToRun = pendingTargetAction;
        pendingTargetAction = PendingTargetAction.None;

        switch (actionToRun)
        {
            case PendingTargetAction.BasicAttack:
                StartCoroutine(PlayerAttack(selectedTarget));
                break;

            case PendingTargetAction.WarriorSkill1:
                StartCoroutine(PlayerShoulderBash(selectedTarget));
                break;

            case PendingTargetAction.MageSkill1:
                StartCoroutine(PlayerBloodPactBolt(selectedTarget));
                break;

            case PendingTargetAction.DoctorSkill1:
                StartCoroutine(PlayerPatchWounds(selectedTarget));
                break;

            case PendingTargetAction.DoctorSkill2:
                StartCoroutine(PlayerFieldSurgery(selectedTarget));
                break;

            case PendingTargetAction.ThiefSkill1:
                StartCoroutine(PlayerPocketSand(selectedTarget));
                break;

            case PendingTargetAction.ThiefSkill3:
                StartCoroutine(PlayerSneakyStrike(selectedTarget));
                break;
        }
    }
    #endregion
   
    #region Unit List Helpers
    /* Checks if the entire player party has been defeated.
    Returns true only if BOTH the player and the companion (wife) are dead.
    
    Used to determine if the battle should end in a loss.
    */
    bool IsPartyDefeated()
    {
        if (isRandomEncounterBattle ||bossFightStarted || isScriptedBossFight)
            return playerUnit.IsDead();

        return playerUnit.IsDead() && wifeUnit.IsDead();
    }

    /*
    Shorya's code to have an enemy pick a random target Nice work man!
    Selects a random valid target from the player's party.

    - If both player and wife are alive → randomly picks one
    - If only one is alive → returns that one
    - If both are dead → returns null
    Used by EnemyTurn() to determine who gets attacked.
    */
    Unit GetRandomLivingAlly()
    {
        // Sort through list of possible targets
        List<Unit> possibleTargets = new List<Unit>();
        // If duncan is alive hes a target
        if (!playerUnit.IsDead())
            possibleTargets.Add(playerUnit);
        
        if (!bossFightStarted)
        {
            if (wifeObject != null && wifeObject.activeSelf && !wifeUnit.IsDead())
                possibleTargets.Add(wifeUnit);

            if (partyMember2Object != null && partyMember2Object.activeSelf && !partyMember2Unit.IsDead())
                possibleTargets.Add(partyMember2Unit);

            if (partyMember3Object != null && partyMember3Object.activeSelf && !partyMember3Unit.IsDead())
                possibleTargets.Add(partyMember3Unit);
        }

        if (possibleTargets.Count == 0)
            return null;

        return possibleTargets[Random.Range(0, possibleTargets.Count)];
    }

    /*
    Returns the correct damage popup position for the given ally.
    
    - Player → playerDamagePoint
    - Wife → wifeDamagePoint
    
    Ensures damage numbers appear in the correct location on screen.
    */
    Transform GetDamagePointForAlly(Unit target)
    {
        if (target == wifeUnit)
            return wifeDamagePoint;

        return playerDamagePoint;
    }
    /* 
    Returns the correct damage popup position for the given enemy.
    - Zombie 1 → enemy1DamagePoint
    - Zombie 2 → enemy2DamagePoint
    - Zombie 3 -> enemy3DamagePoint

    Ensures damage numbers appear in the correct location on screen.
    */
    Transform GetDamagePointForEnemy(Unit target)
    {
        // Tutorial/story battle only uses the first enemy slot
        if (!isRandomEncounterBattle)
            return enemy1DamagePoint;

        // Random/multi-enemy battles use separate slots
        if (target == zombieUnit)
            return enemy1DamagePoint;

        if (target == zombie2Unit)
            return enemy2DamagePoint;

        if (target == zombie3Unit)
            return enemy3DamagePoint;

        return enemy1DamagePoint;
    }

    // Gets a list of all living enemies (zombie 1, zombie 2, boss) Used for targeting when player uses skills that can hit multiple enemies or when the boss has multi-target attacks
    List<Unit> GetLivingEnemies()
    {
        List<Unit> enemies = new List<Unit>();

        if (zombieObject.activeSelf && !zombieUnit.IsDead())
            enemies.Add(zombieUnit);

        if (zombie2Object.activeSelf && !zombie2Unit.IsDead())
            enemies.Add(zombie2Unit);

        if (zombie3Object.activeSelf && !zombie3Unit.IsDead())
            enemies.Add(zombie3Unit);

        if (scriptedBoss1Object != null && scriptedBoss1Object.activeSelf && !scriptedBoss1Unit.IsDead())
            enemies.Add(scriptedBoss1Unit);

        if (scriptedBoss2Object != null && scriptedBoss2Object.activeSelf && !scriptedBoss2Unit.IsDead())
            enemies.Add(scriptedBoss2Unit);

        if (bossObject.activeSelf && !bossUnit.IsDead())
            enemies.Add(bossUnit);

        return enemies;
    }

    // Pretty easy to tell what this does
    bool AreAllEnemiesDefeated()
    {
    return GetLivingEnemies().Count == 0;
    }
    // Removes defeated enemies from the battle scene and updates the UI accordingly Called after the player defeats an enemy to clear them from the battlefield
    void HideDefeatedEnemies()
    {
        if (zombieUnit.IsDead() && zombieObject.activeSelf)
            zombieObject.SetActive(false);

        if (zombie2Unit.IsDead() && zombie2Object.activeSelf)
            zombie2Object.SetActive(false);

        if (zombie3Unit.IsDead() && zombie3Object.activeSelf)
            zombie3Object.SetActive(false);
        
        if (scriptedBoss1Unit != null && scriptedBoss1Unit.IsDead() && scriptedBoss1Object.activeSelf)
            scriptedBoss1Object.SetActive(false);

        if (scriptedBoss2Unit != null && scriptedBoss2Unit.IsDead() && scriptedBoss2Object.activeSelf)
            scriptedBoss2Object.SetActive(false);

        UpdateHPText();
    }
    List<Unit> GetLivingAllies()
    {
        List<Unit> allies = new List<Unit>();

        if (!playerUnit.IsDead())
            allies.Add(playerUnit);

        if (!isRandomEncounterBattle && !bossFightStarted && wifeObject.activeSelf && !wifeUnit.IsDead())
            allies.Add(wifeUnit);

        return allies;
    }

    List<Unit> GetLivingPartyMembers()
    {
        List<Unit> partyMembers = new List<Unit>();

        if (!bossFightStarted && wifeObject.activeSelf && !wifeUnit.IsDead())
            partyMembers.Add(wifeUnit);
        
        if (partyMember2Object != null && partyMember2Object.activeSelf && !partyMember2Unit.IsDead())
        partyMembers.Add(partyMember2Unit);

        if (partyMember3Object != null && partyMember3Object.activeSelf && !partyMember3Unit.IsDead())
        partyMembers.Add(partyMember3Unit);
    

        return partyMembers;
    }
    #endregion
 
    #region Player Actions - Basic and Warrior Skills
    // ================= PLAYER ACTIONS =================

    // Basic attack:
    // - Applies damage modifiers
    // - Deals damage
    // - Advances turn
    IEnumerator PlayerAttack(Unit target)
    {
        BeginPlayerAction(true);
        ClearExpiredStunResistanceAfterPlayerAction();
    
        // Extra safety: if the attack came from the target panel,
        // make sure target buttons are not still usable.
        currentSelectableTargets.Clear();
        currentTargetSelectionType = TargetSelectionType.None;
        pendingTargetAction = PendingTargetAction.None;

        yield return new WaitForSeconds(0.5f);

        enemyUnit = target;

        bool exploitedOpening;
        bool rageBoosted;
        bool isCritical;

        int damage = ApplyPlayerDamageBonuses(playerUnit.GetDamage(), out exploitedOpening, out rageBoosted);

        if (thiefStealthed && thiefNextAttackDoubleDamage)
        {
            damage *= 2;

            thiefStealthed = false;
            stealthTurnsRemaining = 0;
            thiefNextAttackDoubleDamage = false;

            SetBattleText("You strike from stealth!");
            yield return new WaitForSeconds(0.5f);
        }

        if (PlayerFKActive)
        {
            damage += mageFKBonusDamage;
        }

        damage = ApplyCriticalHit(damage, GetPlayerCritChance(), GetPlayerCritMultiplier(), out isCritical);

        DamageEnemy(target, damage, isCritical);

        string attackMessage = GetBasicAttackMessage(damage, exploitedOpening, rageBoosted);

        if (isCritical)
        {
            attackMessage = "Critical hit! " + attackMessage;
        }

        SetBattleText(attackMessage);
        
        // Advance tutorial after using basic attack for the first time in the tutorial battle
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 0)
        {
            tutorialStep = 1;
        }

        if (playerRageActive || PlayerFKActive)
        {
            ConsumeBuffTurn();
        }

        yield return StartCoroutine(ResolveEnemyDefeatOrContinue());
    }

    // Defend action:
    // - Reduces damage from next enemy attack
    IEnumerator PlayerDefend()
    {
        BeginPlayerAction(false);
        ClearExpiredStunResistanceAfterPlayerAction();

        playerUnit.isDefending = true;
        SetPlayerBlockingSprite();
        SetBattleText("You brace for impact!");
        // Advance tutorial after defending for the first time
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 1)
        {
            tutorialStep = 2;
        }

        yield return new WaitForSeconds(1f);

        if (!wifeUnit.IsDead())
        {
            state = BattleState.BUSY;
            StartCoroutine(PartyTurn());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyGroupTurn());
        }
    }

    // Ends the battle if you have good RNG!
    // Otherwise you're getting tagged for free
    IEnumerator PlayerFlee()
    {
        BeginPlayerAction(false);
        ClearExpiredStunResistanceAfterPlayerAction();

        float fleeRoll = Random.value;

        // If you're goated
        if (fleeRoll <= fleeSuccessChance)
        {
            SetBattleText("You escaped!");

            ApplyRandomEncounterFleeRecovery();

            yield return new WaitForSeconds(1f);

            state = BattleState.FLED;
            StartCoroutine(EndBattle());
        }
        else // Damn you're unlucky jit.
        {
            SetBattleText("You failed to escape!");
            yield return new WaitForSeconds(1f);

            SetBattleText("The enemy gets a free attack!");
            yield return new WaitForSeconds(1f);

            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyGroupTurn());
        }
    }

    // Deals damage and stuns the enemy for one turn
    IEnumerator PlayerShoulderBash(Unit target)
    {
        if (!TrySpendSP(skill1Cost))
            yield break;
        
        playerUsedSkillLastTurn = true;

        BeginPlayerAction();

        enemyUnit = target;

        SetBattleText("You use Shoulder Bash!");
        yield return new WaitForSeconds(0.5f);

        bool exploitedOpening;
        bool rageBoosted;
        bool isCritical;

        int baseDamage = playerUnit.GetDamage() + Random.Range(warriorSkill1MinDamage, warriorSkill1MaxDamage + 1);

        int damage = ApplyPlayerDamageBonuses(baseDamage, out exploitedOpening, out rageBoosted);

        damage = ApplyCriticalHit(damage, GetPlayerCritChance(), GetPlayerCritMultiplier(), out isCritical);

        DamageEnemy(target, damage, isCritical);

        bool stunApplied = false;

        if (EnemyResistsStun(target))
        {
            RemoveStunResistance(target);
        }
        else
        {
            ApplyStunToEnemy(target);
            stunApplied = true;
        }

        string attackMessage;

        if (stunApplied)
        {
            attackMessage = GetShoulderBashMessage(damage, exploitedOpening, rageBoosted);
        }
        else
        {
            attackMessage = "Shoulder Bash deals " + damage + " damage, but " + target.unitName + " resists the stun!";
        }

        if (isCritical)
        {
            attackMessage = "Critical hit! " + attackMessage;
        }

        SetBattleText(attackMessage);
	    // Advance Tutorial if player uses Shoulder Bash for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }

        if (playerRageActive || PlayerFKActive)
        {
            ConsumeBuffTurn();
        }

        yield return StartCoroutine(ResolveEnemyDefeatOrContinue());
    }

    // High damage AOE attack
    IEnumerator PlayerAllOutAttack()
    {
        if (!TrySpendSP(skill2Cost))
            yield break;
        
        playerUsedSkillLastTurn = true;

        BeginPlayerAction();
        ClearExpiredStunResistanceAfterPlayerAction();

        SetBattleText("You use All Out Attack!");
        yield return new WaitForSeconds(0.5f);

        bool exploitedOpening;
        bool rageBoosted;
        bool isCritical;

        int baseDamage = playerUnit.GetDamage() + Random.Range(warriorSkill2MinDamage, warriorSkill2MaxDamage + 1);

        int damage = ApplyPlayerDamageBonuses(baseDamage, out exploitedOpening, out rageBoosted);

        damage = ApplyCriticalHit(damage, GetPlayerCritChance(), GetPlayerCritMultiplier(), out isCritical);

        DamageAllEnemies(damage, isCritical);

        string attackMessage = GetAllOutAttackMessage(damage, exploitedOpening, rageBoosted);

        if (isCritical)
        {
            attackMessage = "Critical hit! " + attackMessage;
        }

        SetBattleText(attackMessage);
	    //Advance Tutorial if player uses All Out Attack for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }

        if (playerRageActive)
        {
            ConsumeBuffTurn();
        }

        yield return StartCoroutine(ResolveEnemyDefeatOrContinue());
    }

    // Buff that increases player damage for multiple turns
    // Also triggers visual feedback to indicate Rage is active
    IEnumerator PlayerRage()
    {
        if (!TrySpendSP(skill3Cost))
            yield break;
        playerUsedSkillLastTurn = true;
        BeginPlayerAction();
        ClearExpiredStunResistanceAfterPlayerAction();

        playerRageActive = true;
        rageTurnsRemaining = warriorRageDurationTurns;

        SetBattleText(playerUnit.unitName + " uses Rage!");
        // Player flashes orange to indicate Rage is active
        StartCoroutine(FlashTargetColor(GetUnitSpriteRenderer(playerUnit), rageFlashColor));
        // Player also shakes to show the power of Rage
        StartCoroutine(ShakeTargetCustom(playerUnit.transform, 0.1f, 0.07f));
        yield return new WaitForSeconds(0.75f);

        SetBattleText(playerUnit.unitName + "'s strength surges!");
        //Advance Tutorial if player uses Rage for the first time in the tutorial!
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }
        yield return new WaitForSeconds(1.0f);

        if (!wifeUnit.IsDead())
        {
            state = BattleState.BUSY;
            StartCoroutine(PartyTurn());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyGroupTurn());
        }
    }
    #endregion
    
    #region Player Actions - Mage
    // Mage Skills
    IEnumerator PlayerBloodPactBolt(Unit target)
    {
        if (!TrySpendSP(skill1Cost))
            yield break;
        playerUsedSkillLastTurn = true;
        BeginPlayerAction();
        ClearExpiredStunResistanceAfterPlayerAction();

        enemyUnit = target;

        SetBattleText("You cast Blood Pact Bolt!");
	    //Advance Tutorial if player uses Blood Pact Bolt for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }
	    yield return new WaitForSeconds(1.0f);

        int selfDamage = Mathf.CeilToInt(playerUnit.maxHealth * (mageSkill1SelfDamagePercent / 100f));
        playerUnit.TakeDamage(selfDamage);

        UpdateHPText();

        if (playerUnit.IsDead())
        {   // You died to your own skill, what a dummy
            SetBattleText("The blood price was too great!");
            yield return new WaitForSeconds(1f);

            if (IsPartyDefeated())
            {
                state = BattleState.LOST;
                StartCoroutine(EndBattle());
            }
            else
            {
                StartCoroutine(PartyTurn());
            }

            yield break;
        }

        if (isRandomEncounterBattle)
        {
            SyncPlayerHPToSession();
        }

        ShowDamagePopup(playerDamagePoint, selfDamage, "-", bleedDamageColor);
        StartCoroutine(ShakeTarget(playerUnit.transform));
        StartCoroutine(FlashTarget(GetUnitSpriteRenderer(playerUnit)));

        yield return new WaitForSeconds(0.5f);

        bool exploitedOpening;
        bool rageBoosted;
        bool isCritical;

        int baseDamage = playerUnit.GetDamage() + Random.Range(mageSkill1MinDamage, mageSkill1MaxDamage + 1);

        int damage = ApplyPlayerDamageBonuses(baseDamage, out exploitedOpening, out rageBoosted);

        if (PlayerFKActive)
        {
            damage += mageFKBonusDamage;
        }

        damage = ApplyCriticalHit(damage, GetPlayerCritChance(), GetPlayerCritMultiplier(), out isCritical);

        DamageEnemy(target, damage, isCritical);

        SetBattleText("Blood Pact Bolt deals " + damage + " damage at the cost of blood!");

        if (PlayerFKActive)
        {
            ConsumeBuffTurn();
        }

        yield return StartCoroutine(ResolveEnemyDefeatOrContinue());
    }

       IEnumerator PlayerGraspOfTheAbyss()
    {
        if (!TrySpendSP(skill2Cost))
            yield break;
        playerUsedSkillLastTurn = true;
        BeginPlayerAction();
        ClearExpiredStunResistanceAfterPlayerAction();

        SetBattleText("You summon Grasp of the Abyss!");
	
	// Advance Tutorial if player uses GOTA for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }
        yield return new WaitForSeconds(1.0f);

        bool exploitedOpening;
        bool rageBoosted;
        bool isCritical;

        int baseDamage = playerUnit.GetDamage() + Random.Range(mageSkill2MinDamage, mageSkill2MaxDamage + 1);

        int damage = ApplyPlayerDamageBonuses(baseDamage, out exploitedOpening, out rageBoosted);

        if (PlayerFKActive)
            damage += mageFKBonusDamage;

        damage = ApplyCriticalHit(damage, GetPlayerCritChance(), GetPlayerCritMultiplier(), out isCritical);

        DamageAllEnemies(damage, isCritical);

        int paralyzeRoll = Random.Range(1, 101);
        if (paralyzeRoll <= mageParalyzeChance)
        {
            enemyParalyzed = true;
            enemyParalyzedTurnsRemaining = 1;
        }

        bossDefenseLowered = true;

        if (enemyParalyzed)
            SetBattleText("Grasp of the Abyss hits all enemies for " + damage + " damage and paralyzes them!");
        else
            SetBattleText("Grasp of the Abyss hits all enemies for " + damage + " damage and lowers their defenses!");

        if (PlayerFKActive)
        {
            ConsumeBuffTurn();
        }

        yield return StartCoroutine(ResolveEnemyDefeatOrContinue());
    }

    IEnumerator PlayerForbiddenKnowledge()
    {
        if (!TrySpendSP(skill3Cost))
            yield break;
        playerUsedSkillLastTurn = true;
        BeginPlayerAction();
        ClearExpiredStunResistanceAfterPlayerAction();

        int selfDamage = Mathf.CeilToInt(playerUnit.maxHealth * (mageSkill3SelfDamagePercent / 100f));
        playerUnit.TakeDamage(selfDamage);

        UpdateHPText();

        // Congrats dumbass you killed yourself.
        if (playerUnit.IsDead())
        { 
            SetBattleText("Forbidden Knowledge consumes you!");
            yield return new WaitForSeconds(1f);

            if (IsPartyDefeated())
            {
                state = BattleState.LOST;
                StartCoroutine(EndBattle());
            }
            else
            {
                StartCoroutine(PartyTurn());
            }

            yield break;
        }

        if (isRandomEncounterBattle)
        {
            SyncPlayerHPToSession();
        }

        PlayerFKActive = true;
        fkTurnsRemaining = mageFKDurationTurns;

        int confusionRoll = Random.Range(1, 101);
        if (confusionRoll <= mageConfusionChancePercent)
        {
            playerConfused = true;
            confusionTurnsRemaining = 3;
        }

        UpdateHPText();
        ShowDamagePopup(playerDamagePoint, selfDamage, "-", bleedDamageColor);

        SetBattleText("Forbidden Knowledge floods your mind with power!");
        // Advance Tutorial if player uses Forbidden Knowledge for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }
        yield return new WaitForSeconds(1f);

        if (!wifeUnit.IsDead())
        {
            state = BattleState.BUSY;
            StartCoroutine(PartyTurn());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyGroupTurn());
        }
    }
    #endregion

    #region Player Actions - Doctor
    // Doctors Skills
    IEnumerator PlayerPatchWounds(Unit target)
    {
        if (!TrySpendSP(skill1Cost))
            yield break;
        playerUsedSkillLastTurn = true;
        BeginPlayerAction();
        ClearExpiredStunResistanceAfterPlayerAction();


        int baseHeal = playerUnit.GetDamage() / 2;
        int healAmount = baseHeal + Random.Range(doctorSkill1MinHeal, doctorSkill1MaxHeal + 1);
        target.currentHealth = Mathf.Min(target.maxHealth, target.currentHealth + healAmount);

        if (target == playerUnit && isRandomEncounterBattle)
        {
            SyncPlayerHPToSession();
        }

        UpdateHPText();
        ShowDamagePopup(GetDamagePointForAlly(target), healAmount, "+", Color.green);

        SetBattleText("Patch Wounds restores " + healAmount + " HP to " + target.unitName + "!");
        //Advance Tutorial if player uses Patch Wounds for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }
        yield return new WaitForSeconds(1f);


        if (!wifeUnit.IsDead())
        {
            state = BattleState.BUSY;
            StartCoroutine(PartyTurn());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyGroupTurn());
        }
    }
    IEnumerator PlayerFieldSurgery(Unit target)
    {
        if (!TrySpendSP(skill2Cost))
            yield break;
        playerUsedSkillLastTurn = true;
        BeginPlayerAction();
        ClearExpiredStunResistanceAfterPlayerAction();


        int baseHeal = playerUnit.GetDamage() / 2;
        int healAmount = baseHeal + Random.Range(doctorSkill2MinHeal, doctorSkill2MaxHeal + 1);
        target.currentHealth = Mathf.Min(target.maxHealth, target.currentHealth + healAmount);

        if (target == playerUnit)
        {
            playerBleeding = false;
            bleedTurnsRemaining = 0;
        }

        if (target == playerUnit && isRandomEncounterBattle)
        {
            SyncPlayerHPToSession();
        }

        doctorSkipNextTurn = true;

        UpdateHPText();
        ShowDamagePopup(GetDamagePointForAlly(target), healAmount, "+", Color.green);

	    //Advance Tutorial if player uses Field Surgery for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }
	    yield return new WaitForSeconds(1f);

        SetBattleText("Field Surgery restores " + healAmount + " HP to " + target.unitName + " and removes status effects!");
        yield return new WaitForSeconds(1f);

        if (!wifeUnit.IsDead())
        {
            state = BattleState.BUSY;
            StartCoroutine(PartyTurn());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyGroupTurn());
        }
    }
    IEnumerator PlayerHappyGas()
    {
        if (!TrySpendSP(skill3Cost))
            yield break;
        playerUsedSkillLastTurn = true;
        BeginPlayerAction();
        ClearExpiredStunResistanceAfterPlayerAction();


        HGActive = true;
        doctorHGHealPerTurn = (playerUnit.GetDamage() / 3) + doctorSkill3BaseHeal;
        HGTurnsRemaining = doctorHGDurationTurns;

        SetBattleText("Happy Gas fills the area with a healing cloud!");
        // Advance Tutorial if player uses Happy Gas for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }
        yield return new WaitForSeconds(1f);

        if (!wifeUnit.IsDead())
        {
            state = BattleState.BUSY;
            StartCoroutine(PartyTurn());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyGroupTurn());
        }
    }
    #endregion
    
    #region Player Actions - Thief
    // Thief Skills
    IEnumerator PlayerPocketSand(Unit target)
    {
        if (!TrySpendSP(skill1Cost))
            yield break;
        playerUsedSkillLastTurn = true;
        BeginPlayerAction();
        ClearExpiredStunResistanceAfterPlayerAction();


        enemyUnit = target;

        int blindRoll = Random.Range(1, 101);

        if (blindRoll <= thiefBlindChancePercent)
        {
            enemyBlinded = true;
            enemyBlindedTurnsRemaining = thiefBlindDurationTurns;
            SetBattleText("Pocket Sand blinds " + target.unitName + "!");
        }
        else
        {
            SetBattleText("Pocket Sand hits " + target.unitName + ", but they fight through it!");
        }
	
 	    //Advance Tutorial if player uses Pocket Sand for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }
        yield return new WaitForSeconds(1f);

        if (!wifeUnit.IsDead())
        {
            state = BattleState.BUSY;
            StartCoroutine(PartyTurn());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyGroupTurn());
        }
    }

    IEnumerator PlayerStealth()
    {
        if (!TrySpendSP(skill2Cost))
            yield break;
        playerUsedSkillLastTurn = true;
        BeginPlayerAction();
        ClearExpiredStunResistanceAfterPlayerAction();

        thiefStealthed = true;
        stealthTurnsRemaining = thiefStealthDurationTurns;
        thiefNextAttackDoubleDamage = true;

        SetBattleText("You vanish into the shadows!");
        // Advance Tutorial if player uses Stealth for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }
        yield return new WaitForSeconds(1f);

        if (!wifeUnit.IsDead())
        {
            state = BattleState.BUSY;
            StartCoroutine(PartyTurn());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyGroupTurn());
        }
    }

    IEnumerator PlayerSneakyStrike(Unit target)
    {
        if (!TrySpendSP(skill3Cost))
            yield break;
        playerUsedSkillLastTurn = true;
        BeginPlayerAction();
        ClearExpiredStunResistanceAfterPlayerAction();

        enemyUnit = target;

        yield return new WaitForSeconds(0.5f);

        bool exploitedOpening;
        bool rageBoosted;
        bool isCritical;

        int baseDamage = playerUnit.GetDamage() + Random.Range(thiefSkill3MinDamage, thiefSkill3MaxDamage + 1);

        int damage = ApplyPlayerDamageBonuses(baseDamage,out exploitedOpening, out rageBoosted);

        if (thiefStealthed)
        {
            damage *= 3;
            thiefStealthed = false;
            stealthTurnsRemaining = 0;
            thiefNextAttackDoubleDamage = false;
        }

        damage = ApplyCriticalHit(damage, GetPlayerCritChance(), GetPlayerCritMultiplier(), out isCritical);

        DamageEnemy(target, damage, isCritical);

        SetBattleText("Sneaky Strike deals " + damage + " damage to " + target.unitName + "!");

	    //Advance Tutorial if player uses Sneaky Strike for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }
        yield return StartCoroutine(ResolveEnemyDefeatOrContinue());
    }
    #endregion

    #region Party Turns
    /* Handles the Party Member's automatic turn.
    
    Behavior:
    - Skips turn if party member is dead
    - Attacks enemy automatically
    - Can crit using player crit stats (for now)
    - Can exploit boss defense break (Agonized Lunge)
    
    After attacking:
    - If enemy dies → resolve victory or boss phase
    - Otherwise → proceed to EnemyTurn()
    
    This runs between PlayerTurn() and EnemyTurn().
    */
    IEnumerator PartyTurn()
    {
        yield return StartCoroutine(PartyMemberTurns());
    }
    IEnumerator PartyMemberTurns()
    {
        //if (isRandomEncounterBattle || bossFightStarted)
        if(bossFightStarted && !isScriptedBossFight)
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyGroupTurn());
            yield break;
        }

        List<Unit> partyMembers = GetLivingPartyMembers();

        if (partyMembers.Count == 0)
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyGroupTurn());
            yield break;
        }

        state = BattleState.BUSY;

        foreach (Unit partyMember in partyMembers)
        {
            if (partyMember == null || partyMember.IsDead())
                continue;

            if (enemyUnit == null || enemyUnit.IsDead())
                break;

            SetBattleText(partyMember.unitName + " attacks!");
            yield return new WaitForSeconds(0.5f);

            bool exploitedOpening = false;
            bool isCritical = false;

            int damage = partyMember.GetDamage();

            if (bossFightStarted && enemyUnit == bossUnit && bossDefenseLowered)
            {
                damage += 5;
                bossDefenseLowered = false;
                exploitedOpening = true;
            }

            damage = ApplyCriticalHit(damage, playerCritChancePercent, playerCritMultiplier, out isCritical);

            if (partyMember == partyMember2Unit)
            {
                DamageAllEnemies(damage, isCritical);
                SetBattleText(partyMember.unitName + " hits all enemies for " + damage + " damage!");
            }
            else
            {
                DamageEnemy(enemyUnit, damage, isCritical);

                if (isCritical)
                    SetBattleText("Critical hit! " + partyMember.unitName + " dealt " + damage + " damage!");
                else if (exploitedOpening)
                    SetBattleText(partyMember.unitName + " exploited the opening for " + damage + " damage!");
                else
                    SetBattleText(partyMember.unitName + " dealt " + damage + " damage!");
            }
            yield return new WaitForSeconds(1.5f);

            if (enemyUnit.IsDead())
            {
                
                if (isRandomEncounterBattle)
                {
                    HideDefeatedEnemies();

                    if (AreAllEnemiesDefeated())
                    {
                        ApplyRandomEncounterVictoryRewards();
                        state = BattleState.WON;
                        StartCoroutine(ReturnToOverworldAfterBattle());
                        yield break;
                    }

                    List<Unit> remainingEnemies = GetLivingEnemies();

                    if (remainingEnemies.Count > 0)
                    {
                        enemyUnit = remainingEnemies[0];
                        UpdateHPText();
                    }

                    SetBattleText("Enemy defeated!");
                    yield return new WaitForSeconds(1f);

                    continue;
                }

                if (TryHandleScriptedBossDefeat())
                    yield break;

                if (enemyUnit == zombieUnit && !zombie2Spawned)
                {
                    StartCoroutine(StartSecondZombieFight());
                    yield break;
                }
                else if (!bossFightStarted && enemyUnit == zombie2Unit)
                {
                    StartCoroutine(StartBossFight());
                    yield break;
                }
                else
                {
                    state = BattleState.WON;
                    StartCoroutine(EndBattle());
                    yield break;
                }
            }
        }

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyGroupTurn());
    }
    #endregion

    #region Enemy Turns
    /*
    Handles enemy behavior each turn:
    - Checks stun
    - Selects attack (boss or normal)
    - Applies damage and effects (bleed, defense break)
    - Transitions back to player or ends battle
    */
    IEnumerator EnemyTurn()
    {
        state = BattleState.BUSY;

         if (enemyParalyzed)
        {
            SetBattleText(enemyUnit.unitName + " is paralyzed and cannot move!");
            yield return new WaitForSeconds(1f);

            enemyParalyzedTurnsRemaining--;
            if (enemyParalyzedTurnsRemaining <= 0)
                enemyParalyzed = false;

            StartNextAllyPhase();
            yield break;
        }

        if (IsEnemyStunned(enemyUnit))
        {
            yield return StartCoroutine(HandleEnemyStunSkip(enemyUnit));
            yield break;
        }

        string attackName = "Enemy attacks!";
        int damage = 0;
        bool blocked = false;
        bool isCritical = false;
        string bossMoveType = "";

        if (bossFightStarted && enemyUnit == bossUnit)
        {
            BossAttackData attackData = GetBossAttack();
            attackName = attackData.attackName;
            damage = attackData.damage;
            bossMoveType = attackData.moveType;
        }
        else
        {
            attackName = "Enemy attacks!";
            damage = enemyUnit.GetDamage();
        }

        damage = ApplyCriticalHit(damage, enemyCritChancePercent, enemyCritMultiplier, out isCritical);

        SetBattleText(attackName);
        yield return new WaitForSeconds(0.5f);

        Unit target = GetRandomLivingAlly();

        // Stealth Logic: Later when companions are added make it target the non stealthed unit.
        if (thiefStealthed && target == playerUnit)
        {
            if (!isRandomEncounterBattle && !bossFightStarted && !wifeUnit.IsDead())
            {
                target = wifeUnit;
            }
            else
            {
                SetBattleText(enemyUnit.unitName + " cannot find you!");
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(ResolvePlayerDefeatOrContinue());
                yield break;
            }
        }

        if (target == null)
        {
            state = BattleState.LOST;
            StartCoroutine(EndBattle());
            yield break;
        }

        if (target == playerUnit && playerUnit.isDefending)
        {
            damage = Mathf.Max(1, damage / 2);
            playerUnit.isDefending = false;
            blocked = true;
            SetPlayerNormalSprite();
        }
        
        // Hit chance calculation with modifiers for blind and happy gas
        int hitChancePercent = 100;

        if (enemyBlinded)
            hitChancePercent -= 50;

        if (HGActive)
            hitChancePercent -= doctorHGDodgeBonusPercent;

        int hitRoll = Random.Range(1, 101);
        if (hitRoll > hitChancePercent)
        {
            SetBattleText(enemyUnit.unitName + " misses!");
            yield return new WaitForSeconds(1f);

            if (enemyBlinded)
            {
                enemyBlindedTurnsRemaining--;
                if (enemyBlindedTurnsRemaining <= 0)
                    enemyBlinded = false;
            }

            yield break;
        }

        DamageAlly(target, damage, bossMoveType, isCritical);

        if (blocked)
        {
            if (isCritical)
            SetBattleText( target.unitName + " blocked part of a critical hit! Enemy dealt " + damage + " damage!");
            else
            SetBattleText(target.unitName+ " blocked part of the damage! Enemy dealt "+ damage+ " damage!");
        }
        else if (bossMoveType == "Bite")
        {
            if (isCritical)
                SetBattleText("Critical hit! "+ attackName+ " It dealt "+ damage+ " damage to "+ target.unitName+ "!");
            else
                SetBattleText(
                    attackName + " It dealt " + damage + " damage to " + target.unitName + "!"
                );
            yield return new WaitForSeconds(1f);

            if (target == playerUnit)
            {
                int bleedRoll = Random.Range(1, 101);

                if (bleedRoll <= bleedChancePercent)
                {
                    playerBleeding = true;
                    bleedTurnsRemaining = bleedDuration;
                    SetBattleText(playerUnit.unitName + " starts bleeding!");
                }
                else
                {
                    SetBattleText("The wound looks nasty...");
                }
            }
            else
            {
                SetBattleText("The wound looks nasty...");
            }

            yield return new WaitForSeconds(0.75f);
        }
        else if (bossMoveType == "Lunge")
        {
            if (isCritical)SetBattleText("Critical hit! "+ attackName+ " It dealt " + damage + " damage to "+ target.unitName + "!" );
            else
                SetBattleText(attackName + " It dealt " + damage + " damage to " + target.unitName + "!");
            yield return new WaitForSeconds(1f);
            SetBattleText(enemyUnit.unitName + "'s guard is lowered!");
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            if (isCritical)
            SetBattleText("Critical hit!\n"+ attackName + "\nIt dealt " + damage + " damage to " + target.unitName + ".");
            else
            SetBattleText(attackName + "\nIt dealt " + damage + " damage to " + target.unitName + ".");
        }

         if (enemyBlinded)
        {
            enemyBlindedTurnsRemaining--;
            if (enemyBlindedTurnsRemaining <= 0)
                enemyBlinded = false;
        }
        // Rudamentary reveal logic since there are no true aoe attacks from enemies;
        if (thiefStealthed && bossMoveType == "Lunge")
        {
            thiefStealthed = false;
            stealthTurnsRemaining = 0;
            SetBattleText("The area attack reveals you!");
            yield return new WaitForSeconds(0.75f);
        }

        yield return new WaitForSeconds(0.75f);
    }
    // Handles the enemy turn for each living enemy in a group fight
    IEnumerator EnemyGroupTurn()
    {
        state = BattleState.BUSY;

        List<Unit> enemies = GetLivingEnemies();

        foreach (Unit actingEnemy in enemies)
        {
            if (actingEnemy == null || actingEnemy.IsDead())
                continue;

            enemyUnit = actingEnemy;
            UpdateHPText();

            yield return StartCoroutine(EnemyTurn());

            if (IsPartyDefeated())
            {
                state = BattleState.LOST;
                StartCoroutine(EndBattle());
                yield break;
            }
        }

        if (IsPartyDefeated())
        {
            state = BattleState.LOST;
            StartCoroutine(EndBattle());
            yield break;
        }

        yield return StartCoroutine(ResolvePlayerDefeatOrContinue());
    }
    #endregion

    #region Rewards and Persistence
    void SyncPlayerHPToSession()
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.SetPlayerHP(playerUnit.currentHealth);
        }
    }

    // New Method To give the player XP and keep persistent HP
    void ApplyRandomEncounterVictoryRewards()
    {
        if (GameSession.Instance == null)
        {
            return;
        }
        
        int totalXPReward = GetCurrentXPPerEnemy() * currentRandomEncounterEnemyCount;
        int totalHealReward = randomEncounterVictoryHeal * currentRandomEncounterEnemyCount;

        GameSession.Instance.AddXP(totalXPReward);
        //Apply leveled stats again before healing
        ApplyPersistentPlayerStats();

        // Heal player for winning a random encounter
        GameSession.Instance.HealPlayer(totalHealReward);

        // Push healed values back into the active battle unit so UI matches immediately
        ApplyPersistentPlayerStats();

        UpdateHPText();
        UpdateSPUI();
    }

    // 
    void ApplyRandomEncounterFleeRecovery()
    {
        if (GameSession.Instance == null)
            return;

        GameSession.Instance.HealPlayer(randomEncounterFleeHeal);

        // Push healed values back into the active battle unit so UI matches immediately
        ApplyPersistentPlayerStats();

        UpdateHPText();
        UpdateSPUI();
    }
    #endregion
    #region Update Loop and Debug Commands 
   // Smoothly animates HP bars every frame toward their target values
    void Update()
    {
        if (partyMember1HPBarFill != null && partyMember1HPBarFill.gameObject.activeSelf)
            partyMember1HPBarFill.fillAmount = Mathf.Lerp(partyMember1HPBarFill.fillAmount, partyMember1TargetHPFill, Time.deltaTime * hpBarSpeed);

        if (partyMember2HPBarFill != null && partyMember2HPBarFill.gameObject.activeSelf)
            partyMember2HPBarFill.fillAmount = Mathf.Lerp(partyMember2HPBarFill.fillAmount, partyMember2TargetHPFill, Time.deltaTime * hpBarSpeed);

        if (partyMember3HPBarFill != null && partyMember3HPBarFill.gameObject.activeSelf)
            partyMember3HPBarFill.fillAmount = Mathf.Lerp(partyMember3HPBarFill.fillAmount, partyMember3TargetHPFill, Time.deltaTime * hpBarSpeed);

        if (isInCutscene && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            skipCutsceneRequested = true;
        }
        
        // Debug Key For Max Damage
        if (state == BattleState.PLAYERTURN &&Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            debugMaxDamage = !debugMaxDamage;

            if(debugMaxDamage)
            {
                playerUnit.minDamage = 1000;
                playerUnit.maxDamage = 1000;
                Debug.Log("DEBUG: Max damage enabled");
                SetBattleText("Damage values set to max");
            }
            else
            {
                playerUnit.minDamage = originalMinDamage;
                playerUnit.maxDamage = originalMaxDamage;
                Debug.Log("DEBUG: Max damage disabled");
                SetBattleText("Damage values returned to normal");
            }
        }

        // Debug key to start random encounter battle for testing purposes
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("DEBUG: Starting random encounter test");

            StopAllCoroutines();

            isInCutscene = false;
            skipCutsceneRequested = false;

            isRandomEncounterBattle = true;
            bossFightStarted = false;
            zombie2Spawned = false;

            playerUnit.currentHealth = playerUnit.maxHealth;
            debugRandomEncounterFromTutorial = true;

            // Show party members in random encounter for debug purposes
            if (GameSession.Instance != null)
            {
                GameSession.Instance.hasPartyMember2 = true;
                GameSession.Instance.hasPartyMember3 = true;
                GameSession.Instance.currentEncounterArea = "Forest";
            }

            SetupRandomEncounterBattle();
        }

        // DEBUG: Instant end battle (guaranteed flee)
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("DEBUG: Force end battle");

            StopAllCoroutines();

            state = BattleState.FLED;

            if (isRandomEncounterBattle)
            {
                StartCoroutine(ReturnToOverworldAfterBattle());
            }
            else
            {
                StartCoroutine(EndBattle());
            }
        }

        // DEBUG: Give a bunch of levels
        if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
        {
            Debug.Log("DEBUG: Set player to max level");

            if (GameSession.Instance != null)
            {
                // Give a big chunk of XP so systems that rely on XP still behave correctly
                GameSession.Instance.AddXP(1500);

                // Re-apply stats to the active battle unit
                ApplyPersistentPlayerStats();

                // Fully heal for testing
                playerUnit.currentHealth = playerUnit.maxHealth;
                playerCurrentSP = playerMaxSP;

                UpdateHPText();
                UpdateSPUI();
            }

            SetBattleText("DEBUG: Max level applied");
        }

        // DEBUG: Give a little bit of xp for testing level up mid battle
        if (Keyboard.current != null && Keyboard.current.periodKey.wasPressedThisFrame)
        {
            Debug.Log("DEBUG: Give XP");

            if (GameSession.Instance != null)
            {
                // Give a big chunk of XP so systems that rely on XP still behave correctly
                GameSession.Instance.AddXP(50);

                // Re-apply stats to the active battle unit
                ApplyPersistentPlayerStats();

                // Fully heal for testing
                playerUnit.currentHealth = playerUnit.maxHealth;
                playerCurrentSP = playerMaxSP;

                UpdateHPText();
                UpdateSPUI();
            }

            SetBattleText("DEBUG: 50 XP applied");
        }

        // DEBUG: Start scripted boss fight / final boss fight testing
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            Debug.Log("DEBUG: Starting scripted boss fight");

            StopAllCoroutines();

            isInCutscene = false;
            skipCutsceneRequested = false;
            // Show party members in random encounter for debug purposes
            if (GameSession.Instance != null)
            {
                GameSession.Instance.hasPartyMember2 = true;
                GameSession.Instance.hasPartyMember3 = true;
            }

            SetupScriptedBossFight();
        }

        playerHPBarFill.fillAmount = Mathf.Lerp(playerHPBarFill.fillAmount, playerTargetHPFill, Time.deltaTime * hpBarSpeed);
        enemyHPBarFill.fillAmount = Mathf.Lerp(enemyHPBarFill.fillAmount, enemyTargetHPFill, Time.deltaTime * hpBarSpeed);
        if (enemy2HPBarFill != null && enemy2HPBarFill.gameObject.activeSelf)
        {
            enemy2HPBarFill.fillAmount = Mathf.Lerp(enemy2HPBarFill.fillAmount,enemy2TargetHPFill,Time.deltaTime * hpBarSpeed);
        }
        if (enemy3HPBarFill != null && enemy3HPBarFill.gameObject.activeSelf)
        {
            enemy3HPBarFill.fillAmount = Mathf.Lerp(enemy3HPBarFill.fillAmount,enemy3TargetHPFill,Time.deltaTime * hpBarSpeed);
        }
        playerSPBarFill.fillAmount = Mathf.Lerp(playerSPBarFill.fillAmount, playerTargetSPFill, Time.deltaTime * hpBarSpeed);
    }
    #endregion

    #region Battle Ending and Scene Return
    /* 
    Coroutine to handle returning to the overworld scene after winning a random encounter battle with just a zombie. 
    Displays a victory message tells you how much xp you got, how much you healed, if you fled no rewards :(
    waits for a moment, then loads the overworld scene specified in the GameSession.
    */
    IEnumerator ReturnToOverworldAfterBattle()
    {
        if (state == BattleState.FLED)
        {
            SetBattleText("You fled from battle and recovered " + randomEncounterFleeHeal + " HP!");
        }
        else if (state == BattleState.WON)
        {
            SetBattleText("You defeated the enemies! Gained "
                + (GetCurrentXPPerEnemy() * currentRandomEncounterEnemyCount)
                + " XP and recovered "
                + (randomEncounterVictoryHeal * currentRandomEncounterEnemyCount)
                + " HP!");
        }
        else if (state == BattleState.LOST)
        {
            SetBattleText("You were defeated...");
        }

        yield return new WaitForSeconds(2f);

        if (GameSession.Instance != null)
        {
            GameSession.Instance.isRandomEncounter = false;
        }

        // Special case: debug random encounter launched from tutorial
        if (debugRandomEncounterFromTutorial)
        {
            debugRandomEncounterFromTutorial = false;

            if (GameSession.Instance != null)
            {
                GameSession.Instance.tutorialBattleCompleted = true;
                GameSession.Instance.isRandomEncounter = false;
                GameSession.Instance.loadingTargetScene = "PlayerHouse";
                GameSession.Instance.loadingReturnToPreviousScene = false;
                GameSession.Instance.loadingScreenDuration = 0.25f;
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
            yield break;
        }

        // Normal random encounter return
        string returnScene = "PlayerHouse";

        if (GameSession.Instance != null)
        {
            if (!string.IsNullOrEmpty(GameSession.Instance.randomEncounterReturnScene))
            {
                returnScene = GameSession.Instance.randomEncounterReturnScene;
            }
            else if (!string.IsNullOrEmpty(GameSession.Instance.currentOverworldScene))
            {
                returnScene = GameSession.Instance.currentOverworldScene;
            }

            Debug.Log("Returning from random encounter to scene: " + returnScene);

            GameSession.Instance.isRandomEncounter = false;
            GameSession.Instance.loadingTargetScene = returnScene;
            GameSession.Instance.loadingReturnToPreviousScene = false;
            GameSession.Instance.loadingScreenDuration = 0.25f;
        }
        else
        {
            Debug.LogWarning("GameSession missing. Returning to PlayerHouse by fallback.");
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
    }


    // Finalizes battle state and displays result message --> Disables all player input
    IEnumerator EndBattle()
    {
        SetActionButtonsInteractable(false);

        bool tutorialCompleted = false;

        if (GameSession.Instance != null)
            tutorialCompleted = GameSession.Instance.tutorialBattleCompleted;

        // Only go to death scene after tutorial is completed
        if (state == BattleState.LOST && tutorialCompleted)
        {
            SetBattleText("You were defeated...");
            yield return new WaitForSeconds(2f);

            if (GameSession.Instance != null)
            {   
                GameSession.Instance.isFinalBossFight = false;
                GameSession.Instance.isRandomEncounter = false;
            }

            SceneChanger.Instance.LoadScene("death");
            yield break;
        }

        // Random Encounter End logic
        if (isRandomEncounterBattle)
        {
            yield return StartCoroutine(ReturnToOverworldAfterBattle());
            yield break;
        }

        if (state == BattleState.WON)
        {
            SetBattleText("You won the battle!");

            // Only the tutorial/story intro fight should set tutorialBattleCompleted.
            if (!isRandomEncounterBattle && !isScriptedBossFight && GameSession.Instance != null)
            {
                GameSession.Instance.tutorialBattleCompleted = true;
                GameSession.Instance.isRandomEncounter = false;

                Debug.Log("Tutorial battle completed.");
            }

            if (isScriptedBossFight && GameSession.Instance != null)
            {
                GameSession.Instance.isFinalBossFight = false;
                GameSession.Instance.isRandomEncounter = false;
                GameSession.Instance.FinalBossDefeated = true;
                Debug.Log("Final boss fight completed.");
            }
        }
        else if (state == BattleState.LOST)
        {
            SetBattleText("You were defeated!");
        }
        else if (state == BattleState.FLED)
        {   
            // Tutorial battle is completed after winning the story/tutorial fight
            if (!isRandomEncounterBattle && GameSession.Instance != null)
            {
                GameSession.Instance.tutorialBattleCompleted = true;
                GameSession.Instance.isRandomEncounter = false;

                Debug.Log("Tutorial battle completed.");
            }
            SetBattleText("You fled from battle!");
        }

        yield return new WaitForSeconds(2f);

        string sceneToLoad = "PlayerHouse";

        if (isScriptedBossFight)
        {
            sceneToLoad = "TheSchool";
        }

        if (GameSession.Instance != null)
        {
            GameSession.Instance.loadingTargetScene = sceneToLoad;
            GameSession.Instance.loadingReturnToPreviousScene = false;
            GameSession.Instance.loadingScreenDuration = 0.5f;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
    }
    #endregion
}
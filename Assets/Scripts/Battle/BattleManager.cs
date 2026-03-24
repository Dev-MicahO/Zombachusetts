using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    public BattleState state;

    // Units
    public Unit playerUnit;
    public Unit enemyUnit;
    public Unit zombieUnit;
    public Unit zombie2Unit; // AOE will be hard so im going to add a second zombie after the 1st one dies
    public Unit bossUnit;
    public Unit wifeUnit;

   // Sprites  
    [Header("Player Sprites")]
    public SpriteRenderer playerSpriteRenderer;
    public Sprite playerNormalSprite;
    public Sprite playerBlockingSprite;

    // Battle Text
    public TextMeshProUGUI battleText;
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI wifeHPText;
    public TextMeshProUGUI playerSPText;
    public TextMeshProUGUI shoulderBashButtonText;
    public TextMeshProUGUI allOutAttackButtonText;
    public TextMeshProUGUI rageButtonText;
    public TextMeshProUGUI dialogueText;

    // Typewriter effect for Dialogue
    [Header("Dialogue Typewriter")]
    public float dialogueTypeSpeed = 0.06f;
    private Coroutine activeTypingCoroutine;

    // Skip Intro Function
    private bool skipIntroRequested = false;
    private bool isInIntro = false;



    // HP Bars and SP Bars
    public Image playerHPBarFill;
    public Image wifeHPBarFill;
    public Image enemyHPBarFill;
    public Image playerSPBarFill;

    private float playerTargetHPFill;
    private float enemyTargetHPFill;
    private float wifeTargetHPFill;
    private float playerTargetSPFill;
    public float hpBarSpeed = 2f;

    // Action Buttons
    public Button attackButton;
    public Button defendButton;
    public Button skillButton;
    public Button fleeButton;

    // Skill Buttons
    public Button shoulderBashButton;
    public Button allOutAttackButton;
    public Button rageButton;
    public Button backButton;

    // Panels / UI
    public GameObject damagePopupPrefab;
    public GameObject actionPanel;
    public GameObject skillPanel;
    public GameObject zombieObject;
    public GameObject zombie2Object;
    public GameObject bossObject;
    public GameObject wifeObject;
    public GameObject wifeUIObject;
    public GameObject skipHint;

    // Damage Popup Points
    public Transform playerDamagePoint;
    public Transform enemyDamagePoint;
    public Transform wifeDamagePoint;

    // Canvas
    public Canvas battleCanvas;
    public CanvasGroup transitionOverlay;

    // Player SP
    [Header("Player SP")]
    public int playerMaxSP = 10;
    public int playerCurrentSP = 10;
    public int spRegenPerTurn = 2;

    // Skills
    [Header("Player Skills")]
    // Warrior Skills
    // Shoulder Bash - A strong attack that stuns the enemy for one turn. Costs SP and has a damage range.
    public int shoulderBashCost = 3;
    public int shoulderBashMinDamage = 18;
    public int shoulderBashMaxDamage = 24;

    // All out attack - Aoe damaging move that hits all opponents for moderately high damage (AOE not implemented yet)
    public int allOutAttackCost = 5;
    public int allOutAttackMinDamage = 28;
    public int allOutAttackMaxDamage = 36;

    // Rage - Buff that increases damage for 2 turns
    public int rageCost = 4;
    public int rageBonusDamage = 8;
    public int rageDurationTurns = 2;

    // CRITS!?
    [Header("Critical Hits")]
    public int playerCritChancePercent = 15;
    public float playerCritMultiplier = 1.5f;

    public int enemyCritChancePercent = 10;
    public float enemyCritMultiplier = 1.5f;

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

    // Tutorial
    private int tutorialStep = 0;

    // Player status effects
    private bool playerBleeding = false;
    private int bleedTurnsRemaining = 0;
    private bool playerRageActive = false;
    private int rageTurnsRemaining = 0;

    // Status Effects
    [Header("Bleed Effect")]
    public int bleedDamage = 5;
    public int bleedChancePercent = 30;
    public int bleedDuration = 2;

    private bool enemyStunned = false;

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

    // Original values to restore later
    private int originalMinDamage;
    private int originalMaxDamage;



    void Start()
    {
        Debug.Log("BattleManager Start called");

        // Start both HP bars at full
        playerTargetHPFill = 1f;
        wifeTargetHPFill = 1f;
        enemyTargetHPFill = 1f;

        // Start SP at max as well
        playerCurrentSP = playerMaxSP;
        playerTargetSPFill = 1f;

        // Set the first enemy to the zombie
        enemyUnit = zombieUnit;

        // Show zombie at start, hide boss until needed
        zombieObject.SetActive(true);
        zombie2Object.SetActive(false);
        bossObject.SetActive(false);
        // Wife is visible during the first phase as a companion, then becomes the boss in the second phase.
        wifeObject.SetActive(true);
        wifeUIObject.SetActive(true);

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
        }

        if (isRandomEncounterBattle)
        {
            SetupRandomEncounterBattle();
        }
        else
        {
            StartCoroutine(PlayIntroDialogue());
        }
    }
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
            finalDamage += rageBonusDamage;
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

    /* Applies damage to the enemy and triggers all hit feedback:
    - Crits 1.5x Multiplier and different combat text
    - Shake animation
    - Flash effect
    - Damage popup
    - HP UI update
    */
    void DamageEnemy(int damage, bool isCritical = false)
    {
        enemyUnit.TakeDamage(damage);
        StartCoroutine(ShakeTarget(enemyUnit.transform));
        StartCoroutine(FlashTarget(GetUnitSpriteRenderer(enemyUnit)));

        if (isCritical)
            ShowDamagePopup(enemyDamagePoint, damage, "-", criticalDamageColor);
        else
            ShowDamagePopup(enemyDamagePoint, damage);

        UpdateHPText();
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

    // Message Helpers end here */

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

    /* Handles what happens after the player attacks:
    - If enemy dies → transition to boss or win
    - Otherwise → proceed to enemy turn
    More refactoring to clean up code overall
    */
    IEnumerator ResolveEnemyDefeatOrContinue()
    {
        yield return new WaitForSeconds(1.5f);

        if (enemyUnit.IsDead())
        {
            // RANDOM ENCOUNTER END
            if (isRandomEncounterBattle)
            {
                state = BattleState.WON;
                StartCoroutine(ReturnToOverworldAfterBattle());
                yield break;
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
                StartCoroutine(WifeTurn());
            }
            else
            {
                state = BattleState.ENEMYTURN;
                StartCoroutine(EnemyTurn());
            }
        }
    }

    /*
    Applies damage to a party member (player OR wife) and triggers hit feedback.
    
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

    // Instantly updates all UI elements (HP/SP text and bars)
    // Used when switching enemies or initializing battle state
    void RefreshBattleUIImmediate()
    {
        UpdateHPText();
        UpdateSPUI();

        playerHPBarFill.fillAmount = playerTargetHPFill;
        wifeHPBarFill.fillAmount = wifeTargetHPFill;
        enemyHPBarFill.fillAmount = enemyTargetHPFill;
        playerSPBarFill.fillAmount = playerTargetSPFill;
    }

    // Shows the normal action panel and hides the skill panel
    void ShowActionPanel()
    {
        actionPanel.SetActive(true);
        skillPanel.SetActive(false);
    }

    // Shows the skill panel and hides the normal action panel
    void ShowSkillPanel()
    {
        actionPanel.SetActive(false);
        skillPanel.SetActive(true);
    }

    // Updates skill button labels and enables/disables them based on whether the player has enough SP or not.
    void UpdateSkillButtonsUI()
    {
        shoulderBashButtonText.text = "Shoulder Bash\nSP " + shoulderBashCost;
        allOutAttackButtonText.text = "All Out Attack\nSP " + allOutAttackCost;
        rageButtonText.text = "Rage\nSP " + rageCost;

        shoulderBashButton.interactable = playerCurrentSP >= shoulderBashCost;
        allOutAttackButton.interactable = playerCurrentSP >= allOutAttackCost;
        rageButton.interactable = playerCurrentSP >= rageCost;
    }

    // Made a method for updating battle text i got tired of typing BattleText.text = "some message" in every method that needs to update the battle text.
    // Makes it easier to later add animations, typing effects, etc.
    void SetBattleText(string message)
    {
        battleText.text = message;
    }

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

    // Updates HP text and sets the target fill values for the HP bars Ensures UI always reflects current HP after changes
    void UpdateHPText()
    {
        playerHPText.text = playerUnit.unitName + " HP: " + playerUnit.currentHealth + "/" + playerUnit.maxHealth;
        wifeHPText.text = wifeUnit.unitName + " HP: " + wifeUnit.currentHealth + "/" + wifeUnit.maxHealth;
        enemyHPText.text = enemyUnit.unitName + " HP: " + enemyUnit.currentHealth + "/" + enemyUnit.maxHealth;

        playerTargetHPFill = (float)playerUnit.currentHealth / playerUnit.maxHealth;
        wifeTargetHPFill = (float)wifeUnit.currentHealth / wifeUnit.maxHealth;
        enemyTargetHPFill = (float)enemyUnit.currentHealth / enemyUnit.maxHealth;
    }

    // Updates SP text and sets the target fill value for the SP bar Ensures UI always reflects current SP after changes
    void UpdateSPUI()
    {
        playerSPText.text = "SP: " + playerCurrentSP + "/" + playerMaxSP;
        playerTargetSPFill = (float)playerCurrentSP / playerMaxSP;
        UpdateSkillButtonsUI();
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
        zombie2Spawned = true;

        RefreshBattleUIImmediate();

        SetBattleText(enemyUnit.unitName + " enters the battle!");
        yield return new WaitForSeconds(1.5f);

        StartNextAllyPhase();
    }

    // Handles the transition sequence when the wife becomes the boss.
    // This includes fading to black, showing dialogue about the transformation, swapping visuals from the tutorial phase to the boss fight, and then fading back in to start the boss fight.
    IEnumerator PlayBossTransition()
    {
        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);

        // Fade to black
        yield return StartCoroutine(FadeOverlay(1f, 1f));

        // Hide tutorial battle visuals
        zombie2Object.SetActive(false);
        wifeObject.SetActive(false);
        wifeUIObject.SetActive(false);

        // Show dialogue
        yield return StartCoroutine(ShowTransitionDialogue(wifeUnit.unitName + " doesn't look well...", 2.5f));
        yield return StartCoroutine(ShowTransitionDialogue("Her breathing grows ragged.", 2.5f));
        yield return StartCoroutine(ShowTransitionDialogue("The infection takes hold.", 2.5f));
        yield return StartCoroutine( ShowTransitionDialogue(wifeUnit.unitName + " turns on you.", 2.5f)
        );

        // Clear dialogue before fade-in
        dialogueText.text = "";

        // Set up boss fight
        bossObject.SetActive(true);
        bossFightStarted = true;
        enemyUnit = bossUnit;

        bossMoveIndex = 0;
        bossDefenseLowered = false;
        enemyStunned = false;

        RefreshBattleUIImmediate();

        // Fade back in
        yield return StartCoroutine(FadeOverlay(0f, 1f));

        dialogueText.gameObject.SetActive(false);

        SetBattleText(enemyUnit.unitName + " enters the battle!");
        yield return new WaitForSeconds(1.5f);

        StartNextAllyPhase();
    }
    
    bool ShouldSkipIntro()
    {
        return skipIntroRequested;
    }

    // Starts the second phase of battle by swapping from zombie to boss (This is handled by PlayBossTransition Now)
    IEnumerator StartBossFight()
    {
        yield return StartCoroutine(PlayBossTransition());
    }

    // The Intro Dialogue

    IEnumerator PlayIntroDialogue()
    {
        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);
        skipHint.SetActive(true);

        isInIntro = true;
        skipIntroRequested = false;

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

        isInIntro = false;
        yield return StartCoroutine(SetupBattle());
        yield break;

    SKIP:

        Debug.Log("Intro skipped");

        // Stop typing if active
        if (activeTypingCoroutine != null)
        {
            StopCoroutine(activeTypingCoroutine);
            activeTypingCoroutine = null;
        }

        dialogueText.text = "";
        dialogueText.gameObject.SetActive(false);
        skipHint.SetActive(false);

        // Immediately fade out overlay
        transitionOverlay.alpha = 0f;
        transitionOverlay.blocksRaycasts = false;

        isInIntro = false;

        yield return StartCoroutine(SetupBattle());
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

    // Method to setup a random encounter battle with just a zombie
    void SetupRandomEncounterBattle()
    {
        Debug.Log("Starting random encounter battle");

        transitionOverlay.alpha = 0f;
        transitionOverlay.blocksRaycasts = false;
        dialogueText.gameObject.SetActive(false);
        activeTypingCoroutine = null;
        skipHint.SetActive(false);

        state = BattleState.PLAYERTURN;

        // Disable wife completely
        wifeObject.SetActive(false);
        wifeUIObject.SetActive(false);

        // Only one zombie
        zombieObject.SetActive(true);
        zombie2Object.SetActive(false);
        bossObject.SetActive(false);

        enemyUnit = zombieUnit;

        // Reset SP
        playerCurrentSP = playerMaxSP;
        playerTargetSPFill = 1f;

        // Reset UI
        RefreshBattleUIImmediate();

        SetBattleText("A zombie appears!");
        ShowActionPanel();
        SetActionButtonsInteractable(true);
    }

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
            StartCoroutine(WifeTurn());
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
                    StartCoroutine(WifeTurn());
                }

                yield break;
            }
        }

        playerCurrentSP = Mathf.Min(playerMaxSP, playerCurrentSP + spRegenPerTurn);
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
                SetBattleText("Tutorial: SP is required to use skills and regenerates each turn, Try opening the skills panel!");
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
    IEnumerator HandleEnemyStunSkip()
    {
        enemyStunned = false;
        SetBattleText(enemyUnit.unitName + " is stunned and cannot move!");
        yield return new WaitForSeconds(1f);

        StartNextAllyPhase();
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

    // Reduces remaining Rage duration after an attack -> Disables Rage when duration expires
    void ConsumeRageTurn()
    {
        if (!playerRageActive)
            return;

        rageTurnsRemaining--;

        if (rageTurnsRemaining <= 0)
        {
            playerRageActive = false;
        }
    }

    /*
    Checks if the entire player party has been defeated.
    Returns true only if BOTH the player and the companion (wife) are dead.
    
    Used to determine if the battle should end in a loss.
    */
    bool IsPartyDefeated()
    {
        if (isRandomEncounterBattle ||bossFightStarted)
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
        // During Random Encounters During boss phase, only the player is considered an ally target
        if (isRandomEncounterBattle || bossFightStarted)
        {
            if (!playerUnit.IsDead())
                return playerUnit;

            return null;
        }

        bool playerAlive = !playerUnit.IsDead();
        bool wifeAlive = !wifeUnit.IsDead();

        if (playerAlive && wifeAlive)
        {
            return Random.Range(0, 2) == 0 ? playerUnit : wifeUnit;
        }
        else if (playerAlive)
        {
            return playerUnit;
        }
        else if (wifeAlive)
        {
            return wifeUnit;
        }

        return null;
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
    Handles transition back to the player's side after the enemy finishes its turn.

    - If both allies are dead → end battle (LOSS)
    - If player is dead but wife is alive → skip player turn and go to WifeTurn()
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
            StartCoroutine(WifeTurn());
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    // Enables or disables the main action buttons
    void SetActionButtonsInteractable(bool isInteractable)
    {
        attackButton.interactable = isInteractable;
        defendButton.interactable = isInteractable;
        skillButton.interactable = isInteractable;
        fleeButton.interactable = isInteractable;
    }

    // Called when the Attack button is pressed
    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        SetActionButtonsInteractable(false);
        StartCoroutine(PlayerAttack());
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

    // Called when the Shoulder Bash skill button is pressed
    public void OnShoulderBashButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerShoulderBash());
    }

    // Called when the All Out Attack skill button is pressed
    public void OnAllOutAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerAllOutAttack());
    }

    // Called when the Rage skill button is pressed
    public void OnRageButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerRage());
    }

    // ================= PLAYER ACTIONS =================

    // Basic attack:
    // - Applies damage modifiers
    // - Deals damage
    // - Advances turn
    IEnumerator PlayerAttack()
    {
        BeginPlayerAction(false);
        yield return new WaitForSeconds(0.5f);

        bool exploitedOpening;
        bool rageBoosted;
        bool isCritical;

        int damage = ApplyPlayerDamageBonuses(playerUnit.GetDamage(), out exploitedOpening,out rageBoosted);
        damage = ApplyCriticalHit(damage,playerCritChancePercent,playerCritMultiplier,out isCritical);

        DamageEnemy(damage, isCritical);

        string attackMessage = GetBasicAttackMessage(damage, exploitedOpening, rageBoosted);

        if (isCritical)
        {
            attackMessage = "Critical hit! " + attackMessage;
        }

        SetBattleText(attackMessage);
        
        //Advance Tutorial after first attack
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 0)
        {
            tutorialStep = 1;
        }

        if (rageBoosted)
        {
            ConsumeRageTurn();
        }

        yield return StartCoroutine(ResolveEnemyDefeatOrContinue());
    }

    // Defend action:
    // - Reduces damage from next enemy attack
    IEnumerator PlayerDefend()
    {
        BeginPlayerAction(false);

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
            StartCoroutine(WifeTurn());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    // Ends the battle immediately (Pussy)
    IEnumerator PlayerFlee()
    {
        BeginPlayerAction(false);

        SetBattleText("You fled from battle!");

        yield return new WaitForSeconds(1f);

        state = BattleState.FLED;
        StartCoroutine(EndBattle());
    }

    // Deals damage and stuns the enemy for one turn
    IEnumerator PlayerShoulderBash()
    {
        if (!TrySpendSP(shoulderBashCost))
            yield break;

        BeginPlayerAction();

        SetBattleText("You use Shoulder Bash!");
        yield return new WaitForSeconds(0.5f);

        bool exploitedOpening;
        bool rageBoosted;
        bool isCritical;

        int damage = ApplyPlayerDamageBonuses(Random.Range(shoulderBashMinDamage, shoulderBashMaxDamage + 1),out exploitedOpening, out rageBoosted);
        damage = ApplyCriticalHit(damage,playerCritChancePercent, playerCritMultiplier,out isCritical);

        DamageEnemy(damage, isCritical);
        enemyStunned = true;
        string attackMessage = GetShoulderBashMessage(damage, exploitedOpening, rageBoosted);
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

        if (rageBoosted)
        {
            ConsumeRageTurn();
        }

        yield return StartCoroutine(ResolveEnemyDefeatOrContinue());
    }

    // High damage attack (intended for AOE later)
    IEnumerator PlayerAllOutAttack()
    {
        if (!TrySpendSP(allOutAttackCost))
            yield break;

        BeginPlayerAction();

        SetBattleText("You use All Out Attack!");
        yield return new WaitForSeconds(0.5f);

        bool exploitedOpening;
        bool rageBoosted;
        bool isCritical;

        int damage = ApplyPlayerDamageBonuses(Random.Range(allOutAttackMinDamage, allOutAttackMaxDamage + 1),out exploitedOpening, out rageBoosted);
        damage = ApplyCriticalHit(damage,playerCritChancePercent,playerCritMultiplier,out isCritical);

        DamageEnemy(damage, isCritical);

        string attackMessage = GetAllOutAttackMessage(damage, exploitedOpening, rageBoosted);
        if (isCritical)
        {
            attackMessage = "Critical hit! " + attackMessage;
        }
        SetBattleText(attackMessage);
        // Advance Tutorial if player uses All Out Attack for the first time in the tutorial
        if (!isRandomEncounterBattle && !bossFightStarted && enemyUnit == zombieUnit && tutorialStep == 2)
        {
            tutorialStep = 3;
            backButton.interactable = true;
        }

        if (rageBoosted)
        {
            ConsumeRageTurn();
        }

        yield return StartCoroutine(ResolveEnemyDefeatOrContinue());
    }

    // Buff that increases player damage for multiple turns
    // Also triggers visual feedback to indicate Rage is active
    IEnumerator PlayerRage()
    {
        if (!TrySpendSP(rageCost))
            yield break;

        BeginPlayerAction();

        playerRageActive = true;
        rageTurnsRemaining = rageDurationTurns;

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
            StartCoroutine(WifeTurn());
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    // ===================================

    /*
    Handles the wife's automatic turn.
    
    Behavior:
    - Skips turn if wife is dead
    - Attacks enemy automatically
    - Can crit using player crit stats (for now)
    - Can exploit boss defense break (Agonized Lunge)
    
    After attacking:
    - If enemy dies → resolve victory or boss phase
    - Otherwise → proceed to EnemyTurn()
    
    This runs between PlayerTurn() and EnemyTurn().
    */
    IEnumerator WifeTurn()
    {
        //If its a random encounter battle dont use wife logic
        if (isRandomEncounterBattle)
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
            yield break;
        }
       
        state = BattleState.BUSY;

        if (bossFightStarted || wifeUnit.IsDead())
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
            yield break;
        }

        SetBattleText(wifeUnit.unitName + " attacks!");
        yield return new WaitForSeconds(0.5f);

        bool exploitedOpening = false;
        bool isCritical = false;

        int damage = wifeUnit.GetDamage();

        if (bossFightStarted && enemyUnit == bossUnit && bossDefenseLowered)
        {
            damage += 5;
            bossDefenseLowered = false;
            exploitedOpening = true;
        }

        damage = ApplyCriticalHit(damage,playerCritChancePercent,playerCritMultiplier,out isCritical);

        DamageEnemy(damage, isCritical);

        string wifeMessage;

        if (exploitedOpening && isCritical)
            wifeMessage ="Critical hit! "+ wifeUnit.unitName + " exploited the opening for "+ damage + " damage!";
        else if (exploitedOpening)
            wifeMessage = wifeUnit.unitName + " exploited the opening for " + damage + " damage!";
        else if (isCritical)
            wifeMessage = "Critical hit! " + wifeUnit.unitName + " dealt " + damage + " damage!";
        else
            wifeMessage = wifeUnit.unitName + " dealt " + damage + " damage!";

        SetBattleText(wifeMessage);

        yield return new WaitForSeconds(1.5f);

        if (enemyUnit.IsDead())
        {
            if (enemyUnit == zombieUnit && !zombie2Spawned)
            {
                StartCoroutine(StartSecondZombieFight());
            }
            else if (!bossFightStarted)
            {
                StartCoroutine(StartBossFight());
            }
            else
            {
                state = BattleState.WON;
                StartCoroutine(EndBattle());
            }
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

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

        if (enemyStunned)
        {
            yield return StartCoroutine(HandleEnemyStunSkip());
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

        yield return StartCoroutine(ResolvePlayerDefeatOrContinue());
    }

    // Smoothly animates HP bars every frame toward their target values
    void Update()
    {
        if (isInIntro && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            skipIntroRequested = true;
        }

        if (state == BattleState.PLAYERTURN &&Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame)
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
        
        playerHPBarFill.fillAmount = Mathf.Lerp(playerHPBarFill.fillAmount,playerTargetHPFill,Time.deltaTime * hpBarSpeed);
        wifeHPBarFill.fillAmount = Mathf.Lerp(wifeHPBarFill.fillAmount,wifeTargetHPFill,Time.deltaTime * hpBarSpeed);
        enemyHPBarFill.fillAmount = Mathf.Lerp(enemyHPBarFill.fillAmount,enemyTargetHPFill, Time.deltaTime * hpBarSpeed);
        playerSPBarFill.fillAmount = Mathf.Lerp(playerSPBarFill.fillAmount,playerTargetSPFill,Time.deltaTime * hpBarSpeed);
    }
    
    /* 
    Coroutine to handle returning to the overworld scene after winning a random encounter battle with just a zombie. 
    Displays a victory message, waits for a moment, then loads the overworld scene specified in the GameSession.
    */
    IEnumerator ReturnToOverworldAfterBattle()
    {
        if (state == BattleState.FLED)
            SetBattleText("You fled from battle!");
        else
            SetBattleText("You defeated the zombie!");

        yield return new WaitForSeconds(2f);

        if (GameSession.Instance != null)
        {
            GameSession.Instance.isRandomEncounter = false;
        }

        SceneChanger.Instance.PreviousScene();
    }


    // Finalizes battle state and displays result message --> Disables all player input
    IEnumerator EndBattle()
    {
        SetActionButtonsInteractable(false);
        
        // Random Encounter End logic
        if (isRandomEncounterBattle)
        {
            yield return StartCoroutine(ReturnToOverworldAfterBattle());
            yield break;
        }
        
        
        // Set Text based on State
        if (state == BattleState.WON) SetBattleText("You won the battle!");
        else if (state == BattleState.LOST) SetBattleText("You were defeated!");
        else if (state == BattleState.FLED) SetBattleText("You fled from battle!");

        yield return new WaitForSeconds(2f);
        SceneChanger.Instance.LoadScene("PlayerHouse");
    }
}

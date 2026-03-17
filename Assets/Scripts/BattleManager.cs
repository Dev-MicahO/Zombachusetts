using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public BattleState state;

    // Units
    public Unit playerUnit;
    public Unit enemyUnit;
    public Unit zombieUnit;
    public Unit bossUnit;

    // Battle Text
    public TextMeshProUGUI battleText;
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI enemyHPText;

    // HP Bars
    public Image playerHPBarFill;
    public Image enemyHPBarFill;
    private float playerTargetHPFill;
    private float enemyTargetHPFill;
    public float hpBarSpeed = 2f;

    // Action Buttons
    public Button attackButton;
    public Button defendButton;
    public Button skillButton;
    public Button fleeButton;

    // Panels / UI
    public GameObject damagePopupPrefab;
    public GameObject actionPanel;
    public GameObject skillPanel;
    public GameObject zombieObject;
    public GameObject bossObject;

    // Damage Popup Points
    public Transform playerDamagePoint;
    public Transform enemyDamagePoint;

    // Canvas
    public Canvas battleCanvas;

    // Skills
    public int powerStrikeDamage = 35;

    // Boss Fight
    private bool bossFightStarted = false;

    //Effects
    [Header("Hit Feedback")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.05f;
    public float flashDuration = 0.15f;
    public Color flashColor = new Color(1f, 0.7f, 0.7f, 1f);

    void Start()
    {
        Debug.Log("BattleManager started");

        playerTargetHPFill = 1f;
        enemyTargetHPFill = 1f;

        enemyUnit = zombieUnit;

        zombieObject.SetActive(true);
        bossObject.SetActive(false);

        ShowActionPanel();
        SetActionButtonsInteractable(false);

        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    void ShowDamagePopup(Transform damagePoint, int damage)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(damagePoint.position);

        GameObject popupObj = Instantiate(damagePopupPrefab, battleCanvas.transform);

        RectTransform popupRect = popupObj.GetComponent<RectTransform>();
        popupRect.position = screenPos;

        DamagePopup popup = popupObj.GetComponent<DamagePopup>();
        popup.Setup(damage);
    }

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

    IEnumerator FlashTarget(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null)
            yield break;

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
    }

    SpriteRenderer GetUnitSpriteRenderer(Unit unit)
    {
        return unit.GetComponent<SpriteRenderer>();
    }

    void RefreshHPUIImmediate()
    {
        UpdateHPText();

        playerHPBarFill.fillAmount = playerTargetHPFill;
        enemyHPBarFill.fillAmount = enemyTargetHPFill;
    }

    void ShowActionPanel()
    {
        actionPanel.SetActive(true);
        skillPanel.SetActive(false);
    }

    void ShowSkillPanel()
    {
        actionPanel.SetActive(false);
        skillPanel.SetActive(true);
    }

    void UpdateHPText()
    {
        playerHPText.text = playerUnit.unitName + " HP: " + playerUnit.currentHealth + "/" + playerUnit.maxHealth;
        enemyHPText.text = enemyUnit.unitName + " HP: " + enemyUnit.currentHealth + "/" + enemyUnit.maxHealth;

        playerTargetHPFill = (float)playerUnit.currentHealth / playerUnit.maxHealth;
        enemyTargetHPFill = (float)enemyUnit.currentHealth / enemyUnit.maxHealth;
    }

    IEnumerator StartBossFight()
    {
        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);

        battleText.text = enemyUnit.unitName + " was defeated!";
        yield return new WaitForSeconds(1.5f);

        battleText.text = "A stronger enemy approaches!";
        yield return new WaitForSeconds(1.5f);

        zombieObject.SetActive(false);
        bossObject.SetActive(true);

        enemyUnit = bossUnit;
        bossFightStarted = true;

        RefreshHPUIImmediate();

        battleText.text = enemyUnit.unitName + " enters the battle!";
        yield return new WaitForSeconds(1.5f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    IEnumerator SetupBattle()
    {
        SetActionButtonsInteractable(false);

        battleText.text = "A zombie appears!";
        UpdateHPText();
        RefreshHPUIImmediate();

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        battleText.text = "Choose an action.";
        SetActionButtonsInteractable(true);
        ShowActionPanel();
    }

    void SetActionButtonsInteractable(bool isInteractable)
    {
        attackButton.interactable = isInteractable;
        defendButton.interactable = isInteractable;
        skillButton.interactable = isInteractable;
        fleeButton.interactable = isInteractable;
    }

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        SetActionButtonsInteractable(false);
        StartCoroutine(PlayerAttack());
    }

    public void OnDefendButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        SetActionButtonsInteractable(false);
        StartCoroutine(PlayerDefend());
    }

    public void OnSkillButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        ShowSkillPanel();
        battleText.text = "Choose a skill.";
    }

    public void OnBackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        ShowActionPanel();
        battleText.text = "Choose an action.";
    }

    public void OnFleeButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerFlee());
    }

    public void OnPowerStrikeButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerPowerStrike());
    }

    IEnumerator PlayerAttack()
    {
        state = BattleState.BUSY;

        yield return new WaitForSeconds(0.5f);

        int damage = playerUnit.GetDamage();

        enemyUnit.TakeDamage(damage);
        StartCoroutine(ShakeTarget(enemyUnit.transform));
        StartCoroutine(FlashTarget(GetUnitSpriteRenderer(enemyUnit)));
        ShowDamagePopup(enemyDamagePoint, damage);
        UpdateHPText();

        battleText.text = "You attacked for " + damage + " damage!";

        yield return new WaitForSeconds(1.5f);

        if (enemyUnit.IsDead())
        {
            if (!bossFightStarted)
            {
                StartCoroutine(StartBossFight());
            }
            else
            {
                state = BattleState.WON;
                EndBattle();
            }
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator PlayerDefend()
    {
        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);

        playerUnit.isDefending = true;
        battleText.text = "You brace for impact!";

        yield return new WaitForSeconds(1f);

        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator PlayerFlee()
    {
        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);

        battleText.text = "You fled from battle!";

        yield return new WaitForSeconds(1f);

        state = BattleState.FLED;
        EndBattle();
    }

    IEnumerator PlayerPowerStrike()
    {
        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);
        ShowActionPanel();

        int psdamage = Random.Range(powerStrikeDamage - 5, powerStrikeDamage + 6);

        battleText.text = "You use Power Strike!";

        yield return new WaitForSeconds(0.5f);

        enemyUnit.TakeDamage(psdamage);
        StartCoroutine(ShakeTarget(enemyUnit.transform));
        StartCoroutine(FlashTarget(GetUnitSpriteRenderer(enemyUnit)));
        ShowDamagePopup(enemyDamagePoint, psdamage);
        UpdateHPText();

        battleText.text = "Power Strike deals " + psdamage + " damage!";

        yield return new WaitForSeconds(1.5f);

        if (enemyUnit.IsDead())
        {
            if (!bossFightStarted)
            {
                StartCoroutine(StartBossFight());
            }
            else
            {
                state = BattleState.WON;
                EndBattle();
            }
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator EnemyTurn()
    {
        state = BattleState.BUSY;

        battleText.text = "Enemy attacks!";

        yield return new WaitForSeconds(0.5f);

        int damage = enemyUnit.GetDamage();
        bool blocked = false;

        if (playerUnit.isDefending)
        {
            damage = Mathf.Max(1, damage / 2);
            playerUnit.isDefending = false;
            blocked = true;
        }

        playerUnit.TakeDamage(damage);
        StartCoroutine(ShakeTarget(playerUnit.transform));
        StartCoroutine(FlashTarget(GetUnitSpriteRenderer(playerUnit)));
        ShowDamagePopup(playerDamagePoint, damage);
        UpdateHPText();

        if (blocked)
            battleText.text = "You blocked part of the damage! Enemy dealt " + damage + " damage!";
        else
            battleText.text = "Enemy attacks for " + damage + " damage!";

        yield return new WaitForSeconds(1.5f);

        if (playerUnit.IsDead())
        {
            state = BattleState.LOST;
            EndBattle();
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    void Update()
    {
        playerHPBarFill.fillAmount = Mathf.Lerp(
            playerHPBarFill.fillAmount,
            playerTargetHPFill,
            Time.deltaTime * hpBarSpeed
        );
        enemyHPBarFill.fillAmount = Mathf.Lerp(
            enemyHPBarFill.fillAmount,
            enemyTargetHPFill,
            Time.deltaTime * hpBarSpeed
        );
    }

    void EndBattle()
    {
        SetActionButtonsInteractable(false);

        if (state == BattleState.WON)
            battleText.text = "You won!";
        else if (state == BattleState.LOST)
            battleText.text = "You were defeated!";
        else if (state == BattleState.FLED)
            battleText.text = "You fled from battle!";
    }
}

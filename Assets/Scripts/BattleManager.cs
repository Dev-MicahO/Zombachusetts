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

    // Damage Popup Points
    public Transform playerDamagePoint;
    public Transform enemyDamagePoint;

    // Canvas
    public Canvas battleCanvas;

    // Skills
    public int powerStrikeDamage = 35;

    void Start()
    {
        Debug.Log("BattleManager started");

        playerTargetHPFill = 1f;
        enemyTargetHPFill = 1f;

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
        playerHPText.text = "Player HP: " + playerUnit.currentHealth + "/" + playerUnit.maxHealth;
        enemyHPText.text = "Enemy HP: " + enemyUnit.currentHealth + "/" + enemyUnit.maxHealth;

        playerTargetHPFill = (float)playerUnit.currentHealth / playerUnit.maxHealth;
        enemyTargetHPFill = (float)enemyUnit.currentHealth / enemyUnit.maxHealth;
    }

    IEnumerator SetupBattle()
    {
        SetActionButtonsInteractable(false);

        battleText.text = "A zombie appears!";
        UpdateHPText();

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

        enemyUnit.TakeDamage(playerUnit.attackPower);
        ShowDamagePopup(enemyDamagePoint, playerUnit.attackPower);
        UpdateHPText();

        battleText.text = "You attacked for " + playerUnit.attackPower + " damage!";

        yield return new WaitForSeconds(1.5f);

        if (enemyUnit.IsDead())
        {
            state = BattleState.WON;
            EndBattle();
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

        state = BattleState.LOST;
        EndBattle();
    }

    IEnumerator PlayerPowerStrike()
    {
        state = BattleState.BUSY;
        SetActionButtonsInteractable(false);
        ShowActionPanel();

        battleText.text = "You use Power Strike!";

        yield return new WaitForSeconds(0.5f);

        enemyUnit.TakeDamage(powerStrikeDamage);
        ShowDamagePopup(enemyDamagePoint, powerStrikeDamage);
        UpdateHPText();

        battleText.text = "Power Strike deals " + powerStrikeDamage + " damage!";

        yield return new WaitForSeconds(1.5f);

        if (enemyUnit.IsDead())
        {
            state = BattleState.WON;
            EndBattle();
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

        int damage = enemyUnit.attackPower;

        if (playerUnit.isDefending)
        {
            damage = Mathf.Max(1, damage / 2);
            playerUnit.isDefending = false;
        }

        playerUnit.TakeDamage(damage);
        ShowDamagePopup(playerDamagePoint, damage);
        UpdateHPText();

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
            battleText.text = "Battle ended.";
    }
}

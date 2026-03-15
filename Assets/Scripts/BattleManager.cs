using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    public BattleState state;

    public Unit playerUnit;
    public Unit enemyUnit;

    public TextMeshProUGUI battleText;
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI enemyHPText;

    public Image playerHPBarFill;
    public Image enemyHPBarFill;

    public GameObject damagePopupPrefab;

    public Transform playerDamagePoint;
    public Transform enemyDamagePoint;

    public Canvas battleCanvas;

    private float playerTargetHPFill;
    private float enemyTargetHPFill;

    public float hpBarSpeed = 2f;

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

    void UpdateHPText()
    {
        playerHPText.text = "Player HP: " + playerUnit.currentHealth + "/" + playerUnit.maxHealth;
        enemyHPText.text = "Enemy HP: " + enemyUnit.currentHealth + "/" + enemyUnit.maxHealth;

        playerTargetHPFill = (float)playerUnit.currentHealth / playerUnit.maxHealth;
        enemyTargetHPFill = (float)enemyUnit.currentHealth / enemyUnit.maxHealth;
    }

    IEnumerator SetupBattle()
    {
        battleText.text = "A zombie appears!";
        UpdateHPText();

        yield return new WaitForSeconds(2f);

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        battleText.text = "Choose an action.";
    }

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN)
            return;

        StartCoroutine(PlayerAttack());
    }

    IEnumerator PlayerAttack()
    {   
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

    IEnumerator EnemyTurn()
    {
        battleText.text = "Enemy attacks for " + enemyUnit.attackPower + " damage!";

        yield return new WaitForSeconds(0.5f);

        playerUnit.TakeDamage(enemyUnit.attackPower);
        ShowDamagePopup(playerDamagePoint, enemyUnit.attackPower);
        UpdateHPText();

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
        playerHPBarFill.fillAmount = Mathf.Lerp(playerHPBarFill.fillAmount, playerTargetHPFill, Time.deltaTime * hpBarSpeed);
        enemyHPBarFill.fillAmount = Mathf.Lerp(enemyHPBarFill.fillAmount, enemyTargetHPFill, Time.deltaTime * hpBarSpeed);
    }

    void EndBattle()
    {
        if (state == BattleState.WON)
            battleText.text = "You won!";
        else
            battleText.text = "You were defeated!";
    }
}
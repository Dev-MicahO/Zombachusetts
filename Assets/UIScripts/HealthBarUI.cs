using UnityEngine;
using UnityEngine.UI;

public class OverworldHealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthFillImage;

    [SerializeField] private Color highHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;

    [SerializeField] [Range(0f, 1f)] private float midHealthThreshold = 0.6f;
    [SerializeField] [Range(0f, 1f)] private float lowHealthThreshold = 0.3f;

    private void OnEnable()
    {
        UpdateHealthBar();
    }

    private void Update()
    {
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        if (healthFillImage == null)
        {
            Debug.LogWarning("OverworldHealthBarUI: healthFillImage is not assigned.");
            return;
        }

        if (GameSession.Instance == null)
        {
            Debug.LogWarning("OverworldHealthBarUI: GameSession.Instance is null.");
            return;
        }

        float currentHP = GameSession.Instance.playerCurrentHP;
        float maxHP = GameSession.Instance.playerMaxHP;

        if (maxHP <= 0)
        {
            Debug.LogWarning("OverworldHealthBarUI: maxHP is 0 or less.");
            return;
        }

        float healthPercent = Mathf.Clamp01(currentHP / maxHP);

        healthFillImage.fillAmount = healthPercent;

        if (healthPercent <= lowHealthThreshold)
        {
            healthFillImage.color = lowHealthColor;
        }
        else if (healthPercent <= midHealthThreshold)
        {
            healthFillImage.color = midHealthColor;
        }
        else
        {
            healthFillImage.color = highHealthColor;
        }
    }
}
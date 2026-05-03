using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathSceneEasterEgg : MonoBehaviour
{
    [Header("Death Message Text")]
    public TextMeshProUGUI deathMessageText;

    [Header("Background Image")]
    public Image backgroundImage;
    public Sprite normalBackground;
    public Sprite rareBackground;

    [Header("Messages")]
    [TextArea(2, 4)]
    public string normalDeathMessage = "You Died.";

    [TextArea(2, 4)]
    public string rareDeathMessage = "Somehow... this is even worse than death.";

    [Header("Easter Egg Chance")]
    public int easterEggChance = 1000; // 1 in 1000

    void Start()
    {
        if (deathMessageText == null)
        {
            Debug.LogWarning("DeathSceneEasterEgg: deathMessageText is not assigned.");
            return;
        }

        int roll = Random.Range(1, easterEggChance + 1);
        bool easterEggTriggered = roll == 1;

        if (easterEggTriggered)
        {
            deathMessageText.text = rareDeathMessage;

            if (backgroundImage != null && rareBackground != null)
                backgroundImage.sprite = rareBackground;

            Debug.Log("Death scene easter egg triggered!");
        }
        else
        {
            deathMessageText.text = normalDeathMessage;

            if (backgroundImage != null && normalBackground != null)
                backgroundImage.sprite = normalBackground;
        }
    }
}
using TMPro;
using UnityEngine;

// Script for the damage popup that appears when a unit takes damage, showing the amount of damage taken and then fading out.
// Handles text display, movement, scaling ("pop" effect), fade out, and destruction.
public class DamagePopup : MonoBehaviour
{
    // Reference to the TextMeshPro UI component that displays the damage value
    public TextMeshProUGUI textMesh;

    // Speed at which the popup moves upward
    public float moveSpeed = 60f;

    // How long the popup stays alive before disappearing
    public float lifetime = 1f;

    // Initial scale when the popup spawns
    public float startScale = 1.5f;

    // How quickly the popup shrinks back to normal size
    public float shrinkSpeed = 8f;

    // Tracks how much time is left before the popup is destroyed
    private float timer;

    // Used to fade the popup out smoothly
    private CanvasGroup canvasGroup;

    // Used to move the popup in UI space
    private RectTransform rectTransform;

    // The target scale we want to shrink toward (normal size)
    private Vector3 targetScale = Vector3.one;

    void Awake()
    {
        // Get references to the RectTransform and CanvasGroup components for movement and fading
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    { // Initialize the timer and set the initial scale for the pop effect
        timer = lifetime;
        transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        // Smoothly shrink the popup back to normal size
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            shrinkSpeed * Time.deltaTime
        );

        // Move popup upward over time
        rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

        // Reduce remaining lifetime
        timer -= Time.deltaTime;

        // Fade out as the timer runs out
        if (canvasGroup != null)
        {
            canvasGroup.alpha = timer / lifetime;
        }

        // Destroy when finished
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    // Sets up the popup with the damage value to display
    public void Setup(int damage, string prefix = "-", Color? textColor = null)
    {
        textMesh.text = prefix + damage;

        if (textColor.HasValue)
            textMesh.color = textColor.Value;
        else
            textMesh.color = Color.white;
    }
}

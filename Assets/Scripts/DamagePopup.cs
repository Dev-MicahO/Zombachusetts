using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float moveSpeed = 60f;
    public float lifetime = 1f;
    public float startScale = 1.5f;
    public float shrinkSpeed =8f;

    private float timer;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 targetScale = Vector3.one;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        timer = lifetime;
        transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {   
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, shrinkSpeed * Time.deltaTime);
        
        // Move popup upward
        rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

        // Reduce Timer
        timer -= Time.deltaTime;

        // Fade out
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

    public void Setup(int damage)
    {
        textMesh.text = "-" + damage;
    }
}
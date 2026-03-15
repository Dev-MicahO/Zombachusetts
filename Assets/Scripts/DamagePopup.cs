using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float moveSpeed = 50f;
    public float lifetime = 1f;

    void Update()
    {
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        lifetime -= Time.deltaTime;

        if (lifetime <=0)
        {
            Destroy(gameObject);
        }
    }

    public void Setup(int damage)
    {
        textMesh.text = "-" + damage;
    }
}

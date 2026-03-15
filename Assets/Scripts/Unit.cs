using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;

    public int maxHealth = 100;
    public int currentHealth;

    public int attackPower = 20;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}
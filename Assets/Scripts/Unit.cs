using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;

    public int maxHealth = 100;
    public int currentHealth;

    public int minDamage = 15;
    public int maxDamage = 25;

    public bool isDefending = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }
    
    public int GetDamage()
    {
        return Random.Range(minDamage, maxDamage + 1);
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
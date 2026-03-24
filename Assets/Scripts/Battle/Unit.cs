using UnityEngine;
// Unit Script for both Player and Enemy, contains health, damage, and other stats. this is self explanatory.
public class Unit : MonoBehaviour
{
    public string unitName;

    public int maxHealth = 100;
    public int currentHealth;

    public int minDamage = 15;
    public int maxDamage = 25;

    public bool isDefending = false;

    // Initialize current health to max health at the start of the game
    void Awake()
    {
        currentHealth = maxHealth;
    }
    
    // Returns a random damage value within the unit's damage range pow pow
    public int GetDamage()
    {
        return Random.Range(minDamage, maxDamage + 1);
    }

    // oof
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
    }
   
    // ded
    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}
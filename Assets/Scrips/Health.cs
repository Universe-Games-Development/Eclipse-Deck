using System;
using UnityEngine;

public class Health : MonoBehaviour {
    [SerializeField] protected int maxHealth = 5;
    [SerializeField] protected int currentHealth = 5;
    [SerializeField] private int damageThreshold = 1; // Integers don't have floating-point precision

    // Events for damage and healing
    public event Action OnDamageTaken;
    public event Action OnHealed;
    public event Action OnDeath;

    private void Awake() {
        currentHealth = maxHealth;
    }

    // Method to initialize health settings
    public void InitializeHealth(int maxHealth, int initialHealth, int damageThreshold) {
        this.maxHealth = maxHealth;
        currentHealth = Mathf.Clamp(initialHealth, 0, maxHealth);
        this.damageThreshold = damageThreshold;
    }

    // Method to set current health
    public void SetHealth(int health) {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
    }

    // Method to set damage threshold
    public void SetDamageThreshold(int threshold) {
        damageThreshold = threshold;
    }

    public void ApplyDamage(int damage) {
        if (damage < damageThreshold) return;
        if (damage < 0) {
            Debug.Log("Unexpected negative damage");
            return;
        }

        int resultHealth = currentHealth - damage;

        currentHealth = Mathf.Clamp(resultHealth, 0, maxHealth); // Mathf.Clamp for cleaner health bounds
        if (currentHealth <= 0) {
            Death();
        } else {
            Hurt();
            OnDamageTaken?.Invoke(); // Invoke damage event
        }
    }

    protected virtual void Heal(int amount) {
        if (amount < 0) {
            Debug.Log("Unexpected negative heal");
            return;
        }

        int result = currentHealth + amount;
        currentHealth = Mathf.Clamp(result, 0, maxHealth);
        Debug.Log("Healed for " + amount);
        OnHealed?.Invoke(); // Invoke heal event
    }

    protected virtual void Hurt() {
        // UI call
        // Audio call
        Debug.Log(gameObject.name + " got hurt");
    }

    protected virtual void Death() {
        OnDeath?.Invoke();
        Debug.Log(gameObject.name + " is destroyed");
    }

    public int GetHealth() {
        return currentHealth;
    }
}

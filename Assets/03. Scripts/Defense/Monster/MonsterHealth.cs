using UnityEngine;

public class MonsterHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        Debug.Log($"{name} 데미지 {amount} 입음 ({currentHealth}/{maxHealth})");

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

public class HeroStatus : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [SerializeField] private bool alive = true;

    public HealthBar healthBar;
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        // for testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10);
        }
    }

    public bool checkAlive()
    {
        return alive;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        if (currentHealth <= 0)
        {
            alive = false;
        }
    }
}

using UnityEngine;

public class MonsterStatus : MonoBehaviour
{
    public int maxHealth = 100;
    public int currHealth;

    public int healPerFrame = 10;

    [SerializeField] private bool alive = true;

    public HealthBar healthBar;

    void Start()
    {
        currHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateDamage(10);
        }
    }

    public bool IsAlive()
    {
        return alive;
    }

    public void CreateDamage(int damage)
    {
        currHealth -= damage;
        healthBar.SetHealth(currHealth);
        if (currHealth <= 0)
        {
            alive = false;
        }
    }

    public void AutoHeal()
    {
        if (alive)
        {
            currHealth = Mathf.Min(currHealth + healPerFrame, maxHealth);
            healthBar.SetHealth(currHealth);
        }
    }
}

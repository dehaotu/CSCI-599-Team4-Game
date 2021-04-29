using UnityEngine;
using Mirror;

public class MonsterStatus : NetworkBehaviour
{

    [SyncVar]
    public int maxHP = 100;

    [SyncVar]
    public int currHP = 100;

    [SyncVar]
    public int autoHealPerFrame = 1;

    [SyncVar]
    [SerializeField] private bool alive = true;

    public HealthBar healthBar;

    void Start()
    {
        //GameManager.Instance.AddCoins(5);

        currHP = maxHP;
        healthBar.SetMaxHealth(maxHP);
    }

    void Update()
    {
        healthBar.SetHealth(currHP);
    }

    public bool IsAlive()
    {
        return alive;
    }

    public void CreateDamage(int damagePoint)
    {
        currHP -= damagePoint;
        if (currHP <= 0)
        {
            alive = false;
        }
    }

    public void CreateHeal(int healPoint)
    {
        if (alive)
        {
            currHP = Mathf.Min(currHP + healPoint, maxHP);
        }
    }

    public void AutoHeal()
    {
        CreateHeal(autoHealPerFrame);
    }

    public int TakeDamage(int damage)
    {
        currHP -= damage;
        if (currHP <= 0)
        {
            alive = false;
            return 5;
        }
        return 0;
    }
}

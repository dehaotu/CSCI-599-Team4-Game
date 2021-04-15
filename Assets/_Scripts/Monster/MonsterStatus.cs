using UnityEngine;
using Mirror;

public class MonsterStatus : NetworkBehaviour
{

    [SyncVar]
    public int maxHP = 100;

    [SyncVar]
    public int currHP = 100;

    [SyncVar]
    public int healPointPerFrame = 1;

    [SyncVar]
    [SerializeField] private bool alive = true;

    public int coins = 0;

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

    // Apply damage and get coins if the monster is dead.
    public int ApplyDamage(int damagePoint)
    {
        if (!alive)
        {
            Debug.LogWarning("ApplyDamange to a dead monster.");
            return 0;
        }
        currHP -= damagePoint;
        if (currHP <= 0)
        {
            alive = false;
            return coins;
        }
        return 0;
    }

    // Apply heal by healPoint.
    public void ApplyHeal(int healPoint)
    {
        if (alive)
        {
            currHP = Mathf.Min(currHP + healPoint, maxHP);
        }
        else
        {
            Debug.LogWarning("ApplyHeal to a dead monster.");
        }
    }

    public void AutoHeal()
    {
        ApplyHeal(healPointPerFrame);
    }
}

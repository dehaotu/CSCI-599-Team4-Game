using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TowerStatus : NetworkBehaviour
{
    [SyncVar]
    public int maxHP = 100;
    [SyncVar]
    public int currHP;
    [SyncVar]
    public bool alive = true;
    public HealthBar healthBar;

    void Start()
    {
        currHP = maxHP;
        alive = true;
    }

    void Update()
    {
        healthBar.SetHealth(currHP);
    }

    public bool IsAlive()
    {
        return alive;
    }

    public void TakeDamage(int damage)
    {
        currHP -= damage;
        if (currHP <= 0)
        {
            alive = false;
        }
    }
}

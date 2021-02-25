using System.Collections;
using UnityEngine;
using Mirror;

public abstract class MonsterBaseController : NetworkBehaviour
{
    public float respawnWaitTime = 5.0f;

    public PolygonCollider2D monsterBaseCollider;

    // Start game.
    public virtual void Start()
    {
        monsterBaseCollider = GetComponent<PolygonCollider2D>();
        InstantiateMonsters();
    }

    // Update per frame.
    public void Update()
    {
        if (IsSpawnable())
        {
            StartCoroutine(DestroyAndInstantiate(respawnWaitTime));
        }
        else
        {
            return;
        }
    }

    // Destroy and instantiate monsters after waiting for several seconds.
    IEnumerator DestroyAndInstantiate(float seconds)
    {
        DestroyMonsters();
        yield return new WaitForSeconds(seconds);
        InstantiateMonsters();
    }


    // Check if the monsters are spwanablel.
    public abstract bool IsSpawnable();

    // Instantiate monsters.
    public abstract void InstantiateMonsters();

    // Destroy monsters.
    public abstract void DestroyMonsters();
}

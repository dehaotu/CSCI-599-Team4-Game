// Server Only Script

using System.Collections;
using UnityEngine;
using Mirror;

public abstract class MonsterBaseController : NetworkBehaviour
{
    public float instantiateWaitTime = 5.0f;
    [SerializeField]
    protected PolygonCollider2D monsterBaseCollider;

    private bool isInitiating = false;

    // Start game.
    public virtual void Start()
    {
        monsterBaseCollider = GetComponent<PolygonCollider2D>();
        Instantiate();
    }

    // Update per frame.
    public virtual void Update()
    {
        if (IsInstantiatable() && !isInitiating)
        {
            StartCoroutine(Reinstantiate(instantiateWaitTime));
        }
        else
        {
            return;
        }
    }

    // Destroy and instantiate monsters after waiting for several seconds.
    IEnumerator Reinstantiate(float seconds)
    {
        isInitiating = true;
        yield return new WaitForSeconds(seconds);
        Instantiate();
        isInitiating = false;
    }

    // Check if the monsters base can be instantiate.
    public abstract bool IsInstantiatable();

    // Instantiate monsters.
    public abstract void Instantiate();
}

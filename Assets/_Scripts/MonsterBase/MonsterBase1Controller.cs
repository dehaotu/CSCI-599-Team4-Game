// Server Only Script

using UnityEngine;
using Mirror;

public class MonsterBase1Controller : MonsterBaseController
{
    // Prefab of the monster.
    public GameObject monster1Prefab;

    // Instance of the monster.
    protected GameObject monster1;

    // Positions of the monsters.
    public Vector2 centralPosition;
    public Vector2 monster1RelativePosition;

    // Status of the monster.
    protected MonsterStatus monster1Status;

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
        if (!monster1Status.IsAlive())
        {
            NetworkServer.Destroy(monster1);
        }
    }

    public override bool IsInstantiatable()
    {
        return monster1 == null;
    }

    public override void Instantiate()
    {

        monster1 = Instantiate(monster1Prefab);
        var monster1Controller = monster1.GetComponent<MonsterController>();
        monster1Controller.SetInitialPosition(position: centralPosition + monster1RelativePosition);
        monster1Controller.SetMosterBaseCollider(collider: monsterBaseCollider);
        monster1Status = monster1.GetComponent<MonsterStatus>();
        NetworkServer.Spawn(monster1);
    }
}

using UnityEngine;
using Mirror;

public class MonsterBase1Controller : MonsterBaseController
{
    // Prefabs of the monsters.
    public GameObject monster1Prefab;
    GameObject monster1;

    // Positions of the monsters.
    public Vector2 centralPosition;
    public Vector2 monster1RelativePosition;

    // Statuses of the monsters.
    MonsterStatus monster1Status;

    public override void Start()
    {
        base.Start();
    }

    public override bool IsSpawnable()
    {
        return monster1 != null && !monster1Status.IsAlive();
    }

    public override void InstantiateMonsters()
    {

        monster1 = Instantiate(monster1Prefab);
        var monster1Controller = monster1.GetComponent<MonsterController>();
        monster1Controller.SetInitialPosition(position: centralPosition + monster1RelativePosition);
        monster1Controller.SetMosterBaseCollider(collider: monsterBaseCollider);
        monster1Status = monster1.GetComponent<MonsterStatus>();
        NetworkServer.Spawn(monster1);
    }

    public override void DestroyMonsters()
    {
        NetworkServer.Destroy(monster1);
    }

    public bool isMonsterAlive()
    {
        return monster1 == null || monster1Status.IsAlive();
    }
}

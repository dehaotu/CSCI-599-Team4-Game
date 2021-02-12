using UnityEngine;

public class MonsterBase3Controller : MonsterBaseController
{
    // Prefabs of the monsters.
    public GameObject monster1Prefab;
    public GameObject monster2Prefab;
    public GameObject monster3Prefab;
    GameObject monster1;
    GameObject monster2;
    GameObject monster3;

    // Positions of the monsters.
    public Vector2 centralPosition;
    public Vector2 monster1RelativePosition;
    public Vector2 monster2RelativePosition;
    public Vector2 monster3RelativePosition;

    // Statuses of the monsters.
    MonsterStatus monster1Status;
    MonsterStatus monster2Status;
    MonsterStatus monster3Status;

    public override void Start()
    {

        base.Start();
    }

    public override bool IsSpawnable()
    {
        return monster1 != null && monster2 != null && monster3 != null &&
            !monster1Status.IsAlive() && !monster2Status.IsAlive() && !monster3Status.IsAlive();
    }

    public override void InstantiateMonsters()
    {
        monster1 = Instantiate(monster1Prefab);
        monster1.GetComponent<MonsterMovementController>().SetInitialPosition(position: centralPosition + monster1RelativePosition);
        monster1.GetComponent<MonsterMovementController>().SetCollider(collider: monsterBaseCollider);
        monster1Status = monster1.GetComponent<MonsterStatus>();

        monster2 = Instantiate(monster1Prefab);
        monster2.GetComponent<MonsterMovementController>().SetInitialPosition(position: centralPosition + monster2RelativePosition);
        monster2.GetComponent<MonsterMovementController>().SetCollider(collider: monsterBaseCollider);
        monster2Status = monster2.GetComponent<MonsterStatus>();

        monster3 = Instantiate(monster1Prefab);
        monster3.GetComponent<MonsterMovementController>().SetInitialPosition(position: centralPosition + monster3RelativePosition);
        monster3.GetComponent<MonsterMovementController>().SetCollider(collider: monsterBaseCollider);
        monster3Status = monster3.GetComponent<MonsterStatus>();
    }

    public override void DestroyMonsters()
    {
        Destroy(monster1);
        Destroy(monster2);
        Destroy(monster3);
    }
}

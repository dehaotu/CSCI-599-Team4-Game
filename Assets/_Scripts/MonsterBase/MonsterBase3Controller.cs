﻿// Server Only Script

using UnityEngine;
using Mirror;

public class MonsterBase3Controller : MonsterBaseController
{
    // Prefabs of the monsters.
    public GameObject monster1Prefab;
    public GameObject monster2Prefab;
    public GameObject monster3Prefab;

    // Instances of the monsters.
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

    public override void Update()
    {
        base.Update();
        if (!monster1Status.IsAlive())
        {
            NetworkServer.Destroy(monster1);
        }
        if (!monster2Status.IsAlive())
        {
            NetworkServer.Destroy(monster2);
        }
        if (!monster3Status.IsAlive())
        {
            NetworkServer.Destroy(monster3);
        }
    }

    public override bool IsInstantiatable()
    {
        return monster1 == null && monster2 == null && monster3 == null;
    }

    public override void Instantiate()
    {

        monster1 = Instantiate(monster1Prefab);
        var monster1Controller = monster1.GetComponent<MonsterController>();
        monster1Controller.SetInitialPosition(position: centralPosition + monster1RelativePosition);
        monster1Controller.SetMosterBaseCollider(collider: monsterBaseCollider);
        monster1Status = monster1.GetComponent<MonsterStatus>();
        NetworkServer.Spawn(monster1);

        monster2 = Instantiate(monster2Prefab);
        var monster2Controller = monster2.GetComponent<MonsterController>();
        monster2Controller.SetInitialPosition(position: centralPosition + monster2RelativePosition);
        monster2Controller.SetMosterBaseCollider(collider: monsterBaseCollider);
        monster2Status = monster2.GetComponent<MonsterStatus>();
        NetworkServer.Spawn(monster2);

        monster3 = Instantiate(monster3Prefab);
        var monster3Controller = monster3.GetComponent<MonsterController>();
        monster3Controller.SetInitialPosition(position: centralPosition + monster3RelativePosition);
        monster3Controller.SetMosterBaseCollider(collider: monsterBaseCollider);
        monster3Status = monster3.GetComponent<MonsterStatus>();
        NetworkServer.Spawn(monster3);
    }
}

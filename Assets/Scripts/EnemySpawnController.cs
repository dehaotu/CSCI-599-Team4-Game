﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class EnemySpawnController : NetworkBehaviour
{
    public float waitingTime = 5f;
    public Vector2 targetPosition;
    public EnemyController minionPrefab;
    public bool isEnemySide = true;
    private float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        NetworkServer.Spawn(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // Create Minion and Set the tag (PlayerMinion, EnemyMinion)
        timer += Time.deltaTime;
        if (timer > waitingTime)
        {
            EnemyController minion = Instantiate(minionPrefab, transform.position, Quaternion.identity) as EnemyController;
            minion.targetPosition = targetPosition;
            if (isEnemySide)
                minion.gameObject.tag = "EnemyMinion";
            else
                minion.gameObject.tag = "PlayerMinion";
            timer = 0f;
        }
    }
}

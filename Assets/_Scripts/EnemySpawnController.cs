using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class EnemySpawnController : NetworkBehaviour
{
    private float waitingTime = 60f;     // wave intervals changed from 5s to 60s
    private int numOfMinions = 2;    // number of minions per wave changed from 3 to 2
    public Vector2 targetPosition;
    // public Transform targetPosition;
    public GameObject minionPrefab;
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
        // timer += Time.deltaTime;
        // if (timer > waitingTime)
        // {
        //     timer = 0f;
        if (timer <= 0.0f)
        {
            timer = waitingTime;

            if (isServer)
            {
                for (int i = 0; i < numOfMinions; i++)
                {
                    GameObject minion = Instantiate(minionPrefab, transform.position, Quaternion.identity) as GameObject;
                    var minionController = minion.GetComponent<EnemyController>();
                    minionController.targetPosition = targetPosition;
                    minionController.gameObject.tag = (isEnemySide) ? "EnemyMinion" : "PlayerMinion";
                    minion.GetComponentInChildren<Text>().text = (isEnemySide) ? "Enemy Minion" : "Player Minion";
                    NetworkServer.Spawn(minion);
                }
            }
        }

        timer -= Time.deltaTime;

    }
}

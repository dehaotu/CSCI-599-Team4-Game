using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class EnemySpawnController : NetworkBehaviour
{
    public float waitingTime = 5f;
    public Vector2 targetPosition;
    public GameObject minionPrefab;
    public bool isEnemySide = true;
    public int numOfMinions = 3;
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
            timer = 0f;
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
    }
}

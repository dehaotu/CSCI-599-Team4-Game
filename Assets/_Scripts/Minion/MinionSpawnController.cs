using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MinionSpawnController : NetworkBehaviour
{
    public float waitingTime = 5f;
    public Vector2 targetPosition;
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
        timer += Time.deltaTime;
        if (timer > waitingTime)
        {
            GameObject minion = Instantiate(minionPrefab, transform.position, Quaternion.identity) as GameObject;
            var minionController = minion.GetComponent<MinionController>();
            minionController.targetPosition = targetPosition;
            minionController.gameObject.tag = (isEnemySide) ? "EnemyMinion" : "PlayerMinion";
            minion.GetComponentInChildren<Text>().text = (isEnemySide) ? "Enemy Minion" : "Player Minion";
            timer = 0f;
            // only spawn if it's server
            if (isServer)
            {
                NetworkServer.Spawn(minion);
            }
        }
    }
}

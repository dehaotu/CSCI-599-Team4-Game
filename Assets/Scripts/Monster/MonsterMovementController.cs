using UnityEngine;
using Mirror;

// A client side script

public class MonsterMovementController : NetworkBehaviour
{

    public float movementSpeed = 1f;

    IsometricCharacterRenderer isoRenderer;
    MonsterStatus monsterStatus;

    Rigidbody2D monster;

    Vector2 mosterInitialPosition;
    Collider2D monsterBaseCollider;

    private void Awake()
    {
        monster = GetComponent<Rigidbody2D>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        monsterStatus = GetComponentInChildren<MonsterStatus>();
    }

    void Update()
    {
        GameObject closestPlayer = GetClosestAlivePlayer();

        // Chase and attack nearest player.
        Vector2 playerPosition = GetPlayerPosition(closestPlayer);
        if (!monsterBaseCollider.bounds.Contains(playerPosition) && monsterStatus.IsAlive())
        {
            // Go back to initial position and heal.
            monsterStatus.AutoHeal();

            Vector2 monsterPosition = monster.position;
            Vector2 inputVector = Vector2.ClampMagnitude(mosterInitialPosition - monsterPosition, 1);
            Vector2 movement = inputVector * movementSpeed;
            Vector2 newMonsterPosition = monsterPosition + movement * Time.fixedDeltaTime;
            isoRenderer.SetDirection(movement);
            monster.MovePosition(newMonsterPosition);
        }
        else if (monsterBaseCollider.bounds.Contains(playerPosition) && monsterStatus.IsAlive())
        {
            // If the player is in the bound, chase player.
            Vector2 monsterPosition = monster.position;
            Vector2 inputVector = Vector2.ClampMagnitude(playerPosition - monsterPosition, 1);
            Vector2 movement = inputVector * movementSpeed;
            Vector2 newMonsterPosition = monsterPosition + movement * Time.fixedDeltaTime;
            isoRenderer.SetDirection(movement);
            monster.MovePosition(newMonsterPosition);
        }
    }

    public void SetMosterBaseCollider(Collider2D collider)
    {
        monsterBaseCollider = collider;
    }

    public void SetInitialPosition(Vector2 position)
    {
        mosterInitialPosition = position;
        monster.position = position;
        isoRenderer.SetDirection(new Vector2(1, -1)); // Set direction to SE
    }

    public GameObject GetClosestAlivePlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float minDistance = Mathf.Infinity;
        Vector3 monsterPosition = monster.transform.position;
        foreach (GameObject player in players)
        {
            // Get player alive status.
            if (player.GetComponent<HeroStatus>().checkAlive())
            {
                Vector3 diffDistance = player.transform.position - monsterPosition;
                float currDistance = diffDistance.sqrMagnitude;
                if (currDistance < minDistance)
                {
                    closestPlayer = player;
                    minDistance = currDistance;
                }
            }
            else
            {
                continue;
            }
        }
        return closestPlayer;
    }

    private Vector3 GetPlayerPosition(GameObject player)
    {
        if (player == null)
            return Vector3.positiveInfinity;
        else
            return player.transform.position;
    }
}

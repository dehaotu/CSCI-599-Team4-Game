using UnityEngine;
using Mirror;

public class MonsterController : NetworkBehaviour
{

    public float movementSpeed = 1f;

    IsometricCharacterRenderer isoRenderer;
    MonsterStatus monsterStatus;

    Rigidbody2D monster;

    Vector2 monsterInitialPosition;
    private Vector2 monsterInitialDirection = new Vector2(1, -1);  // Initial direction to SE
    private float returnInitMinDist = 0.05f;

    private float attackMinDist = 0.5f;

    public int attackDamage;

    [SerializeField] private Collider2D monsterBaseCollider;

    private void Awake()
    {
        monster = GetComponent<Rigidbody2D>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        monsterStatus = GetComponentInChildren<MonsterStatus>();

    }

    void Update()
    {
        if (NoUpdate())
        {
            return;
        }

        // Get closestPlayer.
        GameObject closestPlayer = GetClosestAlivePlayer();

        // Chase and attack closestPlayer.
        Vector2 closestPlayerPosition = GetPlayerPosition(player: closestPlayer);
        if (!monsterBaseCollider.bounds.Contains(point: closestPlayerPosition))
        {
            MoveWithoutHate();
        }
        else if (monsterBaseCollider.bounds.Contains(point: closestPlayerPosition))
        {
            MoveWithHate(playerPosition: closestPlayerPosition);
            AttackWithHate(player: closestPlayer, playerPosition: closestPlayerPosition);
        }
    }

    private bool NoUpdate()
    {
        // If it is not a server, no update.
        if (!isServer)
        {
            return true;
        }
        // If the monster is not alive, no update.
        if (!monsterStatus.IsAlive())
        {
            return true;
        }
        // If the monster is playing attack, no update.
        if (isoRenderer.isPlayingAttack())
        {
            return true;
        }
        return false;
    }

    private void MoveWithoutHate()
    {
        // Go back to initial position and heal.
        monsterStatus.AutoHeal();

        // If the monster is already in the initial position, stop update.
        if (monster.position.Equals(monsterInitialPosition))
        {
            isoRenderer.SetDirection(monsterInitialDirection);
            return;
        }

        // Move the monster to the initial position.
        Vector2 monsterPosition = monster.position;

        if ((monsterPosition - monsterInitialPosition).magnitude > returnInitMinDist)
        {
            // If the distance is larger than the returnInitMinDist, do moving.
            Vector2 inputVector = Vector2.ClampMagnitude(monsterInitialPosition - monsterPosition, 1);
            Vector2 movement = inputVector * movementSpeed;
            Vector2 newMonsterPosition = monsterPosition + movement * Time.fixedDeltaTime;
            isoRenderer.SetDirection(movement);
            monster.MovePosition(newMonsterPosition);
        }
        else
        {
            // If the distance is smaller than the returnInitMinDist, do moving.
            Vector2 newMonsterPosition = monsterInitialPosition;
            monster.MovePosition(newMonsterPosition);
        }
    }

    private void MoveWithHate(Vector2 playerPosition)
    {
        // If the player is in the bound, chase player.
        Vector2 monsterPosition = monster.position;
        Vector2 inputVector = Vector2.ClampMagnitude(playerPosition - monsterPosition, 1);
        Vector2 movement = inputVector * movementSpeed;
        Vector2 newMonsterPosition = monsterPosition + movement * Time.fixedDeltaTime;
        isoRenderer.SetDirection(movement);
        monster.MovePosition(newMonsterPosition);
    }

    private void AttackWithHate(GameObject player, Vector2 playerPosition)
    {
        if ((playerPosition - monster.position).magnitude <= attackMinDist)
        {
            // Do attack action.
            isoRenderer.Attack();

            // -HP
            player.GetComponent<HeroStatus>().TakeDamage(attackDamage);
        }
    }

    // Utils

    public void SetMosterBaseCollider(Collider2D collider)
    {
        monsterBaseCollider = collider;
    }

    public void SetInitialPosition(Vector2 position)
    {
        monsterInitialPosition = position;
        monster.position = position;
        isoRenderer.SetDirection(monsterInitialDirection);
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

    public int ApplyDamage(int damagePoint)
    {
        return monsterStatus.ApplyDamage(damagePoint: damagePoint);
    }
}

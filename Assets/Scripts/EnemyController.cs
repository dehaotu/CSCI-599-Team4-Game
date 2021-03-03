using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class EnemyController : NetworkBehaviour
{
    public int currentHealthPoints = 0;
    public int maxHealthPoints = 30;
    public float movementSpeed = 1f;
    public float waitingTime = 2f;

    public int basicAttackPoints = 10;
    public int basicDefensePoints = 10;

    public float maxChaseDistance = 3f;
    private float timer = 0f;

    //Basic Attributes：
    public HealthBar healthBar;

    public Vector2 targetPosition;

    IsometricCharacterRenderer isoRenderer;
    Rigidbody2D rbody;
    bool stopAction = false;
    bool isPlayerClose = false;
    GameObject targetObject;

    [SerializeField]
    [SyncVar]
    private bool alive = true;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        timer = 0f;
        currentHealthPoints = maxHealthPoints;
        healthBar.SetMaxHealth(maxHealthPoints);
        NetworkServer.Spawn(this.gameObject);
    }

    private GameObject FindNearestObjectByTags(string[] tags)
    {
        List<GameObject> objects = new List<GameObject>();

        foreach (string tag in tags)
        {
            List<GameObject> findObjects = GameObject.FindGameObjectsWithTag(tag).ToList();
            foreach (GameObject findObject in findObjects) objects.Add(findObject);
        }
        var neareastObject = objects
            .OrderBy(obj =>
            {
                return Vector3.Distance(transform.position, obj.transform.position);
            })
        .FirstOrDefault();

        return neareastObject;
    }

    public void Move(Vector2 targetPosition)
    {
        Vector2 currentPos = rbody.position;
        float horizontalInput = targetPosition.x - transform.position.x;
        float verticalInput = targetPosition.y - transform.position.y;
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        Vector2 movement = inputVector * movementSpeed;
        Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
        if (!stopAction) isoRenderer.SetDirection(movement);
        rbody.MovePosition(newPos);
        CmdSyncPos(gameObject.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        string[] findTags = gameObject.tag.Equals("EnemyMinion") ? new string[]{ "Player", "PlayerMinion" } : new string[]{ "EnemyMinion" };
        GameObject findObject = FindNearestObjectByTags(findTags);
        timer += Time.deltaTime;

        if (!alive) NetworkServer.Destroy(this.gameObject);

        if (findObject != null) {
            healthBar.SetHealth(currentHealthPoints);
            Vector2 playerPosition = findObject.transform.position;
            if (Vector2.Distance(playerPosition, rbody.position) <= maxChaseDistance)
            {
                if (!isPlayerClose)
                {
                    Move(playerPosition);
                }
                else if (isPlayerClose && timer > waitingTime)
                {
                    
                    timer = 0f;
                    isoRenderer.SetDirection(Vector2.zero);
                    if (targetObject.tag.Equals("Player"))
                    {
                        targetObject.GetComponent<HeroStatus>().TakeDamage(basicAttackPoints);
                    } else if (targetObject.tag.Equals("PlayerMinion") || targetObject.tag.Equals("EnemyMinion"))
                    {
                        targetObject.GetComponent<EnemyController>().TakeDamage(basicAttackPoints);
                    }
                }
            } else
            {
                Move(targetPosition);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if ((other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("PlayerMinion")) && gameObject.tag.Equals("EnemyMinion"))
        {
            isoRenderer.SetDirection(Vector2.zero);
            isPlayerClose = true;
            targetObject = other.gameObject;
        } else if (gameObject.tag.Equals("PlayerMinion") && other.gameObject.CompareTag("EnemyMinion"))
        {
            isoRenderer.SetDirection(Vector2.zero);
            isPlayerClose = true;
            targetObject = other.gameObject;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        isPlayerClose = false;
        targetObject = null;
    }
    public bool checkAlive()
    {
        return alive;
    }

    [Command]
    protected void CmdSyncPos(Vector2 localPosition)
    {
        RpcSyncPos(localPosition);
    }

    [ClientRpc]
    void RpcSyncPos(Vector2 localPosition)
    {
        if (!isLocalPlayer)
        {
            transform.localPosition = localPosition;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealthPoints -= damage;
        if (currentHealthPoints <= 0)
        {
            alive = false;
        }
    }
}

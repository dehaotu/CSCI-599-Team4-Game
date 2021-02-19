using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float healthPoints = 30;
    public float movementSpeed = 1f;
    public float waitingTime = 2f;

    public int basicAttackPoints = 10;
    public int basicDefensePoints = 10;

    public float maxChaseDistance = 3f;
    private float timer = 0f;

    IsometricCharacterRenderer isoRenderer;
    Rigidbody2D rbody;
    bool stopAction = false;
    bool isPlayerClose = false;
    GameObject targetObject;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        timer = 0f;
    }

    private GameObject FindNearestObjectByTag(string tag)
    {
        var objects = GameObject.FindGameObjectsWithTag(tag).ToList();
        var neareastObject = objects
            .OrderBy(obj =>
            {
                return Vector3.Distance(transform.position, obj.transform.position);
            })
        .FirstOrDefault();

        return neareastObject;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        GameObject player = FindNearestObjectByTag("Player");
        Vector2 playerPosition = player.transform.position;
        if (Vector2.Distance(playerPosition, rbody.position) <= maxChaseDistance)
        {
            if (!isPlayerClose)
            {
                Vector2 currentPos = rbody.position;
                float horizontalInput = playerPosition.x - transform.position.x;
                float verticalInput = playerPosition.y - transform.position.y;
                Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
                inputVector = Vector2.ClampMagnitude(inputVector, 1);
                Vector2 movement = inputVector * movementSpeed;
                Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
                if (!stopAction) isoRenderer.SetDirection(movement);
                rbody.MovePosition(newPos);
            }
            else if (isPlayerClose && timer > waitingTime)
            {
                // Attack the player
                targetObject.GetComponent<HeroStatus>().TakeDamage(basicAttackPoints);
                timer = 0f;
            }
        } else
        {
            isoRenderer.SetDirection(Vector2.zero);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class HeroController : NetworkBehaviour
{

    public float movementSpeed = 1f;
    IsometricCharacterRenderer isoRenderer;
    HeroStatus heroStatus;

    Vector2 destination;
    private NavMeshAgent agent;

    Rigidbody2D rbody;
    bool stopAction = false;

    private Camera playerCamera;

    [SyncVar]
    public GameObject targetObject;

    [SyncVar]
    public bool isEnemyClose = false;

    [SyncVar]
    public bool isMonsterClose = false;
    [SyncVar]
    public bool isTowerClose = false;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        heroStatus = GetComponentInChildren<HeroStatus>();
        playerCamera = GetComponentInChildren<Camera>();
        destination = rbody.position;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Start()
    {
        //disable non-local player inventory
        if (!isLocalPlayer)
        {
            GameObject menu = transform.Find("Inventory Menu").transform.gameObject;
            menu.SetActive(false);
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (isoRenderer.isPlayingAttack()) return;
        float horizontalInput = 0;
        float verticalInput = 0;

        /*if (heroStatus.checkAlive())
        {
            agent.isStopped = false;
        }
        else agent.isStopped = true;*/

        // edited
        if (Input.GetKeyDown(KeyCode.Mouse0) && heroStatus.checkAlive() && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 mousePositionOnScreen = Input.mousePosition;
            Vector2 mousePositionInGame = playerCamera.ScreenToWorldPoint(mousePositionOnScreen);
            destination = mousePositionInGame;
            agent.SetDestination(destination);
        }

        Vector2 inputVector = new Vector2(0, 0);
        Vector2 currentPos = rbody.position;
        inputVector = destination - currentPos;

        //Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        Vector2 movement = inputVector * movementSpeed;
        Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
        /* Debug.Log("Destination:");
         Debug.Log(IsometricCharacterRenderer.DirectionToIndex(movement, 8));*/

        CmdSetDirection(movement);
        if (heroStatus.checkAlive())
        {
            rbody.MovePosition(newPos);
        }
        //test attack
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (heroStatus.checkAlive())
            {
                isoRenderer.Attack();
                CmdAttack();
            }
        }

        if (!heroStatus.checkAlive())
        {
            stopAction = true;
            isoRenderer.Dead();
        }
        else
        {
            stopAction = false;
        }
    }

    private void CmdSetDirection(Vector2 direction)
    {
        if (!stopAction) isoRenderer.SetDirection(direction);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!isLocalPlayer) return;
        CmdUpdateCollisionEnter(other.gameObject);
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (!isLocalPlayer) return;
        CmdUpdateCollisionExit();
    }

    // Commands

    [Command]
    void CmdUpdateCollisionEnter(GameObject otherGameObject)
    {
        if (otherGameObject == null)
        {
            return;
        }
        else if (otherGameObject.CompareTag("EnemyMinion"))
        {
            isoRenderer.SetDirection(Vector2.zero);
            isEnemyClose = true;
            targetObject = otherGameObject;
        }
        else if (otherGameObject.CompareTag("Monster"))
        {
            isoRenderer.SetDirection(Vector2.zero);
            isMonsterClose = true;
            targetObject = otherGameObject;
        }
        else if (otherGameObject.CompareTag("EnemyTower"))
        {
            isoRenderer.SetDirection(Vector2.zero);
            isTowerClose = true;
            targetObject = otherGameObject;
        }
    }

    [Command]
    void CmdUpdateCollisionExit()
    {
        isEnemyClose = false;
        isMonsterClose = false;
        isTowerClose = false;
        targetObject = null;
    }

    [Command]
    private void CmdAttack()
    {
        if (isEnemyClose)
        {
            int coins = targetObject.GetComponent<MinionController>().TakeDamage(heroStatus.BasicAttackPoints);
            heroStatus.EarnCoin(coins);
        }
        else if (isMonsterClose)
        {
            int coins = targetObject.GetComponent<MonsterController>().ApplyDamage(heroStatus.BasicAttackPoints);
            heroStatus.EarnCoin(coins);
        }
        else if (isTowerClose)
        {
            //Debug.Log("In attack");
            targetObject.GetComponent<TowerStatus>().TakeDamage(heroStatus.BasicAttackPoints);
        }
    }
}
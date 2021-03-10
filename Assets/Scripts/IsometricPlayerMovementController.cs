﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class IsometricPlayerMovementController : NetworkBehaviour
{

    public float movementSpeed = 1f;
    IsometricCharacterRenderer isoRenderer;
    HeroStatus heroStatus;

    Vector2 destination;
    private NavMeshAgent agent;

    Rigidbody2D rbody;
    bool stopAction = false;

    private Camera playerCamera;

    GameObject targetObject;
    bool isEnemyClose = false;

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


    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        if (isoRenderer.isPlayingAttack()) return;
        float horizontalInput = 0;
        float verticalInput = 0;
        
        // edited
        if (Input.GetKeyDown(KeyCode.Mouse0) && heroStatus.checkAlive() && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 mousePositionOnScreen = Input.mousePosition;
            Vector2 mousePositionInGame = playerCamera.ScreenToWorldPoint(mousePositionOnScreen);
            destination = mousePositionInGame;
            agent.SetDestination(destination);
            Debug.Log("New destinaion: " + destination);
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
        if (heroStatus.checkAlive()) rbody.MovePosition(newPos);


        //test attack
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isEnemyClose && heroStatus.checkAlive())
            {
                isoRenderer.Attack();
                targetObject.GetComponent<EnemyController>().TakeDamage(heroStatus.BasicAttackPoints);
            }
        }

        if (heroStatus.currentHealth <= 0)
        {
            stopAction = true;
            isoRenderer.Dead();
        }

        // test attack monster
        if (Input.GetKeyDown(KeyCode.M))
        {
            CmdMockAttack();
        }
    }

    private void CmdSetDirection(Vector2 direction)
    {
        if (!stopAction) isoRenderer.SetDirection(direction);
    }

    [Command]
    private void CmdMockAttack()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject monster in monsters)
        {
            // Get player alive status.
            monster.GetComponent<MonsterStatus>().CreateDamage(5);
        }
        Debug.Log("In mock attack");
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("EnemyMinion"))
        {
            isoRenderer.SetDirection(Vector2.zero);
            isEnemyClose = true;
            targetObject = other.gameObject;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        isEnemyClose = false;
        targetObject = null;
    }
}

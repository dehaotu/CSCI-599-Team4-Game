using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IsometricPlayerMovementController : MonoBehaviour
{

    public float movementSpeed = 1f;
    IsometricCharacterRenderer isoRenderer;
    HeroStatus heroStatus;

    Vector2 destination;
    private NavMeshAgent agent;

    Rigidbody2D rbody;
    bool stopAction = false;
    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        heroStatus = GetComponentInChildren<HeroStatus>();
        destination = rbody.position;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }


    // Update is called once per frame
    void Update()
    {
        float horizontalInput = 0;
        float verticalInput = 0;
        Vector2 inputVector = new Vector2(0, 0);
        Vector2 currentPos = rbody.position;
        if (Input.GetKeyDown(KeyCode.Mouse0) && heroStatus.checkAlive())
        {
            Vector2 mousePositionOnScreen = Input.mousePosition;
            Vector2 mousePositionInGame = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);
            destination = mousePositionInGame;
            agent.SetDestination(destination);
            Debug.Log("New destinaion: " + destination);
        }
        inputVector = destination - currentPos;

        //Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        Vector2 movement = inputVector * movementSpeed;
        Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
        /* Debug.Log("Destination:");
         Debug.Log(IsometricCharacterRenderer.DirectionToIndex(movement, 8));*/

         
        if(!stopAction) isoRenderer.SetDirection(movement);
        if (heroStatus.checkAlive()) rbody.MovePosition(newPos);

        //test attack
        if (Input.GetKeyDown(KeyCode.F))
        {
            isoRenderer.Attack();
        }

        if (heroStatus.currentHealth <= 0)
        {
            stopAction = true;
            isoRenderer.Dead();
        }
    }
}

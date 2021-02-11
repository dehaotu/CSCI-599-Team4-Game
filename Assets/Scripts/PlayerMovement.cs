using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Rigidbody2D rigidbody2d;
    // float horizontal; 
    // float vertical;
    
    // // Start is called before the first frame update
    // void Start()
    // {
    //     rigidbody2d = GetComponent<Rigidbody2D>();
    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     horizontal = Input.GetAxis("Horizontal");
    //     vertical = Input.GetAxis("Vertical");
    // }

    // void FixedUpdate()
    // {
    //     Vector2 position = rigidbody2d.position;
    //     position.x = position.x + 3.0f * horizontal * Time.deltaTime;
    //     position.y = position.y + 3.0f * vertical * Time.deltaTime;

    //     rigidbody2d.MovePosition(position);
    // }

    // [SerializeField]
    // [Range(2, 12)]
    public float speed = 4.0f;

    private Vector3 targetPosition;
    private bool isMoving = false;

    void Update(){
        if(Input.GetMouseButton(0)){
            SetTargetPosition();
        }

        if(isMoving){
            Move();
        }

    }


    void SetTargetPosition(){
        targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPosition.z = transform.position.z;

        isMoving = true;
    }

    void Move(){
        // transform.rotation = Quaternion.LookRotation(Vector3.forward, targetPosition);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if(transform.position == targetPosition) {
            isMoving = false;
        }
    }
}

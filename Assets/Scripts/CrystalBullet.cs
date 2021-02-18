using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalBullet : MonoBehaviour
{

    public float speed;
    public float verticalOffset = 0.7f;
    //public GameObject bulletPrefabs;
    private Transform player;
    private Vector2 target;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        target = new Vector2(player.position.x, player.position.y + verticalOffset);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        ShootingDirection();

        if (Mathf.Abs(transform.position.x - target.x) <= .5f && Mathf.Abs(transform.position.y - target.y) <= .5f )
        {
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }

    void ShootingDirection()
    {
        Vector2 direction = new Vector2(player.position.x, player.position.y + verticalOffset);
        direction -= new Vector2(transform.position.x, transform.position.y);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        gameObject.GetComponent<Rigidbody2D>().rotation = angle;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalBullet : MonoBehaviour
{

    public float speed = 6.0f;
    public float verticalOffset = 0.7f;
    //public GameObject bulletPrefabs;
    /*    private Transform player;*/
    // private Vector2 target;

    //add
    private GameObject target;
    private int damage = 5;

    // Start is called before the first frame update
    // void Start()
    // {
    //     player = GameObject.FindGameObjectWithTag("Player").transform;
    //     target = new Vector2(player.position.x, player.position.y + verticalOffset);
    // }

    public void LocateTarget(GameObject _target)
    {
        target = _target;
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector2 direction = target.transform.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (Vector2.Distance(gameObject.transform.position, target.transform.position) <= .5f)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        gameObject.GetComponent<Rigidbody2D>().rotation = angle;
    }

    private void HitTarget()
    {
        Destroy(gameObject);
        Debug.Log("Hit");
        target.GetComponent<HeroStatus>().TakeDamage(damage);

    }

    // void DestroyBullet()
    // {
    //     Destroy(gameObject);
    // }

    // void ShootingDirection()
    // {
    //     Vector2 direction = new Vector2(player.position.x, player.position.y + verticalOffset);
    //     direction -= new Vector2(transform.position.x, transform.position.y);
    //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    //     gameObject.GetComponent<Rigidbody2D>().rotation = angle;
    // }

}
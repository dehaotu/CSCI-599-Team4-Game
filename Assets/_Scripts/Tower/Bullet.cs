using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    private float speed = 6.0f;
    private float verticalOffset = 0.7f;
    private GameObject target;
    private int bulletDamage = 5;

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
        target.GetComponent<HeroStatus>().TakeDamage(bulletDamage);
    }
}
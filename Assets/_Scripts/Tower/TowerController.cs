using UnityEngine;
using Mirror;
public abstract class TowerController : NetworkBehaviour
{
    protected float shootCountdown;
    protected float shootInterval = 1f;
    protected float shootRange = 3.0f;
    protected GameObject target;
    protected float distance;
    public GameObject BulletPrefab;
    private TowerStatus towerStatus;

    protected virtual void Start()
    {
        shootCountdown = shootInterval;
        towerStatus = GetComponent<TowerStatus>();
    }

    protected virtual void Update()
    {
        target = UpdateTarget().nearestTarget;
        distance = UpdateTarget().shortestDistance;
        if (shootCountdown <= 0.0f && target != null && distance <= shootRange && towerStatus.alive)
        {
            Shoot();
            shootCountdown = shootInterval;
        }

        shootCountdown -= Time.deltaTime;
    }

    protected abstract (GameObject nearestTarget, float shortestDistance) UpdateTarget();

    protected virtual void Shoot()
    {
        GameObject bulletInst = (GameObject)Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        Bullet bullet = bulletInst.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.LocateTarget(target);
            bullet.GetComponent<AudioSource>().Play();
        }
    }
}

using UnityEngine;

public class TowerPlayerController : TowerController
{
    private string targetTag = "EnemyMinion";

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override (GameObject nearestTarget, float shortestDistance) UpdateTarget()
    {
        GameObject target = GameObject.FindGameObjectWithTag(targetTag);
        GameObject nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        // change to AI status
        // if (target.GetComponent<HeroStatus>().checkAlive())
        // {
        //     shortestDistance = Vector2.Distance(transform.position, target.transform.position);
        //     nearestTarget = target;
        // }
        return (nearestTarget, shortestDistance);
    }

    protected override void Shoot()
    {
        base.Shoot();
    }
}
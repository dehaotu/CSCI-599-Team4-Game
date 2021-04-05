using UnityEngine;

public class TowerEnemyController : TowerController
{

    private string targetTag = "Player";

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
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HeroStatus>().checkAlive())
            {
                float distance = Vector2.Distance(transform.position, target.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTarget = target;
                }
            }
            else
            {
                continue;
            }
        }
        return (nearestTarget, shortestDistance);
    }

    protected override void Shoot()
    {
        base.Shoot();
    }
}

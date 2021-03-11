using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Crystal : NetworkBehaviour
{

    public int maxCrystalHealth = 100;
    [SyncVar]
    public int currentCrystalHealth = 100;
    public HealthBar CrystalHealthBar;

    [SerializeField] private float timeBtShots;
    public float startTimeBtShots = 1f;
    public float firingRange = 9f;
    public GameObject CrystalBulletPrefab;
    private GameObject player;

    // add
    private string shootTag = "Player";
    private GameObject shootTarget;

    public bool isAlly;

    // Start is called before the first frame update
    void Start()
    {
        timeBtShots = startTimeBtShots;
        InvokeRepeating("UpdateTarget", 0.0f, 0.5f); // invoke UpdateTarget() every 0.5 seconds starts from 0 second
    }

    void FixedUpdate()
    {
        //Get the local player
        if (player == null)
        {
            foreach (var clientPlayer in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (clientPlayer.GetComponent<HeroStatus>().isLocalPlayer)
                {
                    player = clientPlayer;
                }
            }
            //if (player == null) return;
        }
        if (shootTarget == null)
        {
            return;
        }

        if (timeBtShots <= 0.0f && player.GetComponent<HeroStatus>().checkAlive())
        {
            Shoot();
            timeBtShots = startTimeBtShots;
        }

        timeBtShots -= Time.deltaTime;
    }

    private void UpdateTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(shootTag);
        GameObject nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        foreach (var target in targets)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTarget = target;
                if (isAlly && target.tag.Equals("Player")) nearestTarget = null; // ally towers shouldn't attack players
            }
        }

        if (shortestDistance <= firingRange && nearestTarget != null)
        {
            shootTarget = nearestTarget;
        }
        else
        {
            shootTarget = null;
        }
    }


    private void Shoot()
    {
        // "object casting": create a temporary gameObject for Instantiate object
        GameObject bulletInst = (GameObject)Instantiate(CrystalBulletPrefab, transform.position, Quaternion.identity);
        CrystalBullet bullet = bulletInst.GetComponent<CrystalBullet>();

        if (bullet != null)
        {
            bullet.LocateTarget(shootTarget);
        }

        Debug.Log("shoot");
    }
    /*    public void shoot()
        {
            if (timeBtShots <= 0 && player.GetComponent<HeroStatus>().checkAlive())
            {
                GameObject bullet = Instantiate(CrystalBulletPrefab, transform.position, Quaternion.identity);
                timeBtShots = startTimeBtShots;
            }
            else
            {
                timeBtShots -= Time.deltaTime;
            }
        }*/

    public void TakeDamage(int damage)
    {
        if (isServer) currentCrystalHealth -= damage;
        CrystalHealthBar.SetHealth(currentCrystalHealth);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Crystal : NetworkBehaviour
{
    public int maxCrystalHealth = 100;
    public int currentCrystalHealth = 100;
    public HealthBar CrystalHealthBar;

    [SerializeField]private float timeBtShots;
    public float startTimeBtShots = 1f;
    public float firingRange = 9f;
    public GameObject CrystalBulletPrefab;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        timeBtShots = startTimeBtShots;
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
            if (player == null) return;
        }
        
        //For testing
        /*if (Input.GetKey(KeyCode.R))
        {
            TakeDamage(10);
        }*/
        if (Vector2.Distance(transform.position, player.transform.position) <= firingRange)
        {
            shoot();
        }
    }

    public void shoot()
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
    }
    [Command]
    public void CmdTakeDamage(int damage)
    {
        currentCrystalHealth -= damage;
        CrystalHealthBar.SetHealth(currentCrystalHealth);
    }
}
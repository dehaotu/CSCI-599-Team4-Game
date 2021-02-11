using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    public int maxCrystalHealth = 100;
    public int currentCrystalHealth = 100;
    public HealthBar CrystalHealthBar;

    [SerializeField]private float timeBtShots;
    public float startTimeBtShots = 1f;
    public float firingRange = 9f;
    public GameObject CrystalBullet;
    private Transform player;

    // Start is called before the first frame update
    void Start()
    {
        //currentCrystalHealth = maxCrystalHealth;
        //CrystalHealthBar.SetMaxHealth(maxCrystalHealth);

        player = GameObject.FindGameObjectWithTag("Player").transform;
        timeBtShots = startTimeBtShots;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Debug.Log(Vector2.Distance(transform.position, player.position));

        //// for testing
        //if (Input.GetKeyDown(KeyCode.LeftControl))
        //{
        //    TakeDamage(10);
        //}
/*        Debug.Log("Distance: ------");
        Debug.Log(Vector2.Distance(transform.position, player.position));*/
        if (Vector2.Distance(transform.position, player.position) <= firingRange)
        {
            Debug.Log("True");
            if (timeBtShots <= 0 && GameObject.FindGameObjectWithTag("Player").GetComponent<HeroStatus>().checkAlive())
            {
                Instantiate(CrystalBullet, transform.position, Quaternion.identity);
                timeBtShots = startTimeBtShots;

            }
            else
            {

                timeBtShots -= Time.deltaTime;
            }
        }
    }

    //public void TakeDamage(int damage)
    //{
    //    currentCrystalHealth -= damage;
    //    CrystalHealthBar.SetHealth(currentCrystalHealth);
    //}

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.tag == "Tower")
    //    {
    //        TakeDamage(5);

    //    }
    //}
}
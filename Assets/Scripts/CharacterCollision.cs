//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CharacterCollision : MonoBehaviour
//{
//    private HeroStatus heroStatus;
//    private void Start()
//    {
//        heroStatus = gameObject.GetComponentInParent<HeroStatus>();
//    }
//    private void OnTriggerEnter2D(Collider2D collision)
//    {
//        if (collision.gameObject.tag == "Bullet")
//        {
//            heroStatus.TakeDamage(9);
//        }
//    }
//}

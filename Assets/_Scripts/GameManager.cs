using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    //单例模式
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("GameManager").GetComponent<GameManager>();
                
            }
            return _instance;
        }
    }

    [SerializeField]
    private GameObject localPlayer;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (player.GetComponent<HeroStatus>().isLocalPlayer)
            {
                localPlayer = player;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Earn Coins
    public void AddCoins(int amount)
    {
        if(localPlayer != null)
        {
            localPlayer.GetComponent<HeroStatus>().EarnCoin(amount);
        }
        
    }
}

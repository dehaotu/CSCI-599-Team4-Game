using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;


public class StatusBoard : Inventory
{
    //单例模式
    private static StatusBoard _instance;
    public static StatusBoard Instance 
    {
        get 
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("StatusBoardPanel").GetComponent<StatusBoard>();
            }
            return _instance;
        }
    }

    [SerializeField]
    private Text otherPlayerText;

    private GameObject parentLocalPlayer;
    private StatusSection[] statusSections = new StatusSection[3];

    private GameObject[] allPlayers = new GameObject[3];

    public void UpdatePlayerAttribute(string text)
    {
        otherPlayerText = transform.Find("AvatarPlayer/SectionFrame/Attributes").GetComponent<Text>();
        otherPlayerText.text = text;
    }


    private Text boardText;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        boardText = GetComponentInChildren<Text>();
        boardText.text = "Status Board";
        statusSections = gameObject.GetComponentsInChildren<StatusSection>();
        /*foreach (StatusSection s in statusSections) Debug.Log(s.transform.name);*/

        renderSubSections();
        Hide();
    }

    public void setParentLocalPlayer(GameObject localPlayer)
    {
        parentLocalPlayer = localPlayer;
    }

    public void renderSubSections()
    {
        // get all other players besides the local player
        int i = 0;
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (!player.GetComponent<HeroStatus>().isLocalPlayer) allPlayers[i++] = player;
        }

        // get all status sections
        for (int j = 0; j < 3; j++)
        {
            if (allPlayers[j] != null)
                statusSections[j].setPlayer(allPlayers[j].GetComponent<NetworkIdentity>().netId);
        }
    }

    void FixedUpdate()
    {
        renderSubSections();
    }
}
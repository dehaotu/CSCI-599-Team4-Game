using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class StatusSection : NetworkBehaviour
{
    [SerializeField]
    private uint playerNetId;
    private GameObject player;
    private CanvasGroup canvasGroup;
    HeroStatus heroStatus;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            // display this canvas
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            HeroStatus heroStatus = player.GetComponent<HeroStatus>();
            transform.Find("SectionFrame/Attributes").GetComponent<Text>().text = "Attack: " + heroStatus.BasicAttackPoints.ToString() + "\nDefense: " + heroStatus.BasicDefensePoints.ToString();
        } else
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
        }
    }

    public void setPlayer(uint netId)
    {
        if (player == null)
        {
            playerNetId = netId;
            player = NetworkIdentity.spawned[playerNetId].gameObject;
            heroStatus = player.GetComponent<HeroStatus>();
        }

        
/*        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;*/
        //set name
        transform.Find("SectionFrame/Name").GetComponent<Text>().text = heroStatus.transform.Find("Status Canvas/Name").GetComponent<Text>().text;
        transform.Find("SectionFrame/Attributes").GetComponent<Text>().text = "Attack: " + heroStatus.BasicAttackPoints.ToString() + "\nDefense: " + heroStatus.BasicDefensePoints.ToString();

    }
}

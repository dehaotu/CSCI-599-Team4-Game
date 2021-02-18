using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LobbyPanel : MonoBehaviour
{
    public GameObject lobby_backButton;
    public GameObject lobby_newRoomButton;
    public GameObject roomListPanel;

    // Start is called before the first frame update
    void Start()
    {
        lobby_backButton.GetComponentInChildren<Text>().text = "Back";
        lobby_newRoomButton.GetComponentInChildren<Text>().text = "New Room";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

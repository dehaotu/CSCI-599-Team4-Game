using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//The Panel when inside a game room.
public class RoomPanel : MonoBehaviour
{
    public GameObject room_BackButton;
    public GameObject room_ReadyButton;
    public GameObject room_teamPanel;

    // Start is called before the first frame update
    void Start()
    {
        room_BackButton.GetComponentInChildren<Text>().text = "Back";
        room_ReadyButton.GetComponentInChildren<Text>().text = "Ready";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

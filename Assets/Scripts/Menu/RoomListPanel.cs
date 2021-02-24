using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.UI;
using Mirror;


//The Panel that lists all game rooms in lobby.
public class RoomListPanel : MonoBehaviour
{
    
    public GameObject roomRowPrefab;
    public NetworkRoomManager networkManager;
    public MenuController menuController;
    public CustomNetworkDiscovery networkDiscovery;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void UpdateRoom(IPEndPoint idAddress, int numPlayers)
    {
        GameObject newRow = Instantiate(roomRowPrefab, this.transform);
        Button roomRow = newRow.GetComponent<Button>();
        roomRow.GetComponentInChildren<Text>().text = "Room     " + numPlayers.ToString() + "/4";
        roomRow.onClick.AddListener(() => {menuController.onClick_Lobby_RoomRow();});
    }

    
}

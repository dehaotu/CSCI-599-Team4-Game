using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.UI;
using Mirror;


//The Panel that lists all game rooms in lobby.
public class RoomListPanel : MonoBehaviour
{
    private const int NUM_PLAYERS = 4;

    public GameObject roomRowPrefab;
    public MenuController menuController;

    public void UpdateRoom(List<Lobby.Room> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            Lobby.Room room = roomList[i];
            GameObject newRow = Instantiate(roomRowPrefab, this.transform);
            Button roomRow = newRow.GetComponent<Button>();
            roomRow.GetComponentInChildren<Text>().text = room.RoomName + "     " + room.NumPlayers.ToString() + "/" + NUM_PLAYERS.ToString();
            roomRow.onClick.AddListener(() => { menuController.onClick_Lobby_RoomRow(room.uuid); });
            RectTransform tf = roomRow.GetComponent<RectTransform>();
            tf.localPosition = new Vector3(0, 100 - i * (tf.rect.height + 3), 0);
        }
    }
}

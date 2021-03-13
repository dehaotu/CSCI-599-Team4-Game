using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Mirror;

/*
A support class for helper MenuController to show animation effect in menu.
*/
class MenuAnimator
{
    private RectTransform transform;
    private float cameraSpeedX;
    private float cameraSpeedY;
    private float period_rate; //how fast the camera to complete one back and forth
    private float timer = 0;
    private GameObject panel;

    private const float SINE_PERIOD = (float)(Math.PI * 2);

    public MenuAnimator(GameObject panel, float cameraSpeedX, float cameraSpeedY, float period_rate)
    {
        this.panel = panel;
        this.cameraSpeedX = cameraSpeedX;
        this.cameraSpeedY = cameraSpeedY;
        this.period_rate = period_rate;
        transform = panel.GetComponent<RectTransform>();
    }

    public void Update()
    {
        timer = (timer + Time.deltaTime * period_rate) % SINE_PERIOD;
        transform.Translate(new Vector3(cameraSpeedX * -(float)Math.Sin(timer), cameraSpeedY * -(float)Math.Sin(timer), 0));
    }
}

/*
MenuController consists of a menu animator, and different GUI handlers.
This is class is only responsible for Menu's GUI, for network part please see LobbyNetworkManager.
*/
public class MenuController : MonoBehaviour
{
    private bool flag_client_actively_disconnect = false;   // Set this flag if user actively discconect from server so error message will not be shown
    private MenuAnimator animator;  // Used to move the map in the background

    public GameObject backGroundMap;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject loginPanel;
    public GameObject mainMenuPanel;
    public GameObject playerListPanel;
    public GameObject nameInputField;
    public GameObject serverDisconnectPanel;
    public GameConfiguration gameConfiguration;
    public LobbyNetworkManager lobbyNetworkManager;
    public PlayerInfoRow playerInfoRowPrefab;

    private int GUI_ROOMPANEL_HEIGH = 100;      // The height of the room panel to start appending playerinfo row
    private int GUI_ROOMPANEL_PLAYERINFOROW_GAP = 5;    // The gap between playerinfo row
    
    //public string myName {set; get;}    // The name of the playe who controls this computer
    public float cameraSpeedX = 0.4f;
    public float cameraSpeedY = 0.2f;
    public float period_rate = 0.5f; // How fast the camera to complete one back and forth


/************************************************************************************************
                                             GUI                                             
************************************************************************************************/
    // Start is called before the first frame update
    void Start()
    {
        animator = new MenuAnimator(backGroundMap, cameraSpeedX, cameraSpeedY, period_rate);
#if UNITY_SERVER
        //onClickDebugLobbyServer();
#endif
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        animator.Update();
        _updateRoom();
    }

    private void _updateRoom()
    {
        if (lobbyNetworkManager.currRoom == null)
            return;

        foreach (Transform child in playerListPanel.transform)
        {
            Destroy(child.gameObject);
        }
        
        for (int i = 0; i < lobbyNetworkManager.currRoom.PlayerList.Count; i++)
        {
            Player roomPlayer = lobbyNetworkManager.currRoom.PlayerList[i];
            GameObject playerRowObj = Instantiate(playerInfoRowPrefab.gameObject, Vector3.zero, Quaternion.identity, playerListPanel.transform);
            PlayerInfoRow playerRow = playerRowObj.GetComponent<PlayerInfoRow>();
            playerRow.setPlayerNameText(roomPlayer.Name);
            playerRow.setReadyText(roomPlayer.isReady);
            RectTransform tf = playerRowObj.GetComponent<RectTransform>();
            tf.localPosition  = new Vector3(0, GUI_ROOMPANEL_HEIGH - i * (tf.rect.height + GUI_ROOMPANEL_PLAYERINFOROW_GAP), 0);
        }
    }

/************************************************************************************************
                                             LoginPanel                                             
************************************************************************************************/
    public void onClick_LoginPanel_Confirm()
    {
        InputField inputField = nameInputField.GetComponent<InputField>();
        gameConfiguration.myName = inputField.text;
        Debug.Log("Debug: set name to " + gameConfiguration.myName);
        loginPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
/************************************************************************************************
                                             MainMenuPanel                                             
************************************************************************************************/
    public void onClickStartButton()
    {
        _setAllPlanelInactive();
        lobbyPanel.SetActive(true);
        lobbyNetworkManager.ClientConnectLobby();
        //roomDiscovery.StartDiscovery();
    }

    public void onClickSettingButton() {}

    public void onClickExitButton()
    {
        Application.Quit();
    }

    public void onClickDebugLobbyServer() {
        /*
        _setAllPlanelInactive();
        roomPanel.SetActive(true);

        //hide all buttons in roomPanel
        roomPanel.transform.Find("Room_BackButton").gameObject.SetActive(false);
        roomPanel.transform.Find("Room_ReadyButton").gameObject.SetActive(false);

        roomDiscovery.AdvertiseServer();
        networkRoomManager.StartServer();
        flag_isHost = true;
        */
        Debug.Log("trying to connect...");
        //tinsNetworkRoomManager.ClientConnectLobby();
    }
/************************************************************************************************
                                             LobbyPanel                                             
************************************************************************************************/
    public void onClick_Lobby_NewRoomButton()
    {
        //_setAllPlanelInactive();
        //roomPanel.SetActive(true);
        //roomDiscovery.AdvertiseServer();
        //networkRoomManager.StartHost();
        //flag_isHost = true;
    }

    public void onClick_Lobby_BackButton()
    {
        //_setAllPlanelInactive();
        //mainMenuPanel.SetActive(true);

        //roomDiscovery.StartDiscovery();
    }

    public void onClick_Lobby_RoomRow()
    {
        _setAllPlanelInactive();
        //networkRoomManager.StartClient();
        //roomDiscovery.StopDiscovery();
        lobbyNetworkManager.JoinRoom("");
        //flag_isHost = false;
        roomPanel.SetActive(true);
    }
/************************************************************************************************
                                             RoomPanel                                             
************************************************************************************************/
    public void onClick_Room_BackButton()
    {
        /*
        _setAllPlanelInactive();
        lobbyPanel.SetActive(true);
        if (flag_isHost)
        {
            networkRoomManager.StopHost();
        }
        else
        {
            flag_client_actively_disconnect = true;
            networkRoomManager.StopClient();
        }*/
    }

    public void onClick_Room_ReadyButton()
    {
        /*
        foreach (NetworkRoomPlayer roomPlayer in networkRoomManager.roomSlots)
        {
            // If a roomPlayer represents the this computer's client, roomPlayer.isLocalPlayer is true.
            if (roomPlayer.isLocalPlayer)
            {
                roomPlayer.CmdChangeReadyState(!roomPlayer.readyToBegin);
            }
        }*/
        lobbyNetworkManager.sendReadySignal();
    }
/************************************************************************************************
                                             ServerDisconnectPanel                                             
************************************************************************************************/
    public void onClick_ServerDisconnectPanel_Confirm()
    {
        _setAllPlanelInactive();
        mainMenuPanel.SetActive(true);
    }

    public void onServer_Disconnect()
    {
        if (!flag_client_actively_disconnect)
        {
            _setAllPlanelInactive();
            serverDisconnectPanel.SetActive(true);
        }
        flag_client_actively_disconnect = false;
    }

/************************************************************************************************
                                        Helper Functions                                             
************************************************************************************************/
    private void _setAllPlanelInactive()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(false);
        loginPanel.SetActive(false);
        serverDisconnectPanel.SetActive(false);
    }

}

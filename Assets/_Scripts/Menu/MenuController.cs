﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MongoDB.Driver;
using MongoDB.Bson;

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
    enum MenuState
    {
        Login,
        MainMenu,
        ConnectingLobby,
        Lobby,
        JoiningRoom,
        LobbyError,
        Room
    }
    private bool flag_client_actively_disconnect = false;   // Set this flag if user actively discconect from server so error message will not be shown
    private MenuAnimator animator;  // Used to move the map in the background
    private MenuState state;

    public GameObject backGroundMap;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject loginPanel;
    public GameObject mainMenuPanel;
    public GameObject playerListPanel;
    public RoomListPanel roomListPanel;
    public GameObject accountInputField;
    public GameObject passwordInputField;
    public GameObject serverDisconnectPanel;
    public GameObject connectingLobbyPanel;
    public GameObject joiningRoomPanel;
    public GameObject lobbyErrorPanel;
    public GameObject loginErrorPanel;
    public GameObject debugLoginPanel;
    public GameObject debugLoginInputField;
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
        lobbyNetworkManager.Event_ReceiveRoomUpdate = _updateRoom;
        lobbyNetworkManager.Event_ClientDisconnected = onServer_Disconnect;
        lobbyNetworkManager.Event_ReceiveLobbyInfoUpdate += onConnectingLobby;
        lobbyNetworkManager.Event_ReceiveJoinRoomResponse += onReceiveJoinRoomResponse;
        state = MenuState.Login;
        _setAllPlanelInactive();
        loginPanel.SetActive(true);
#if UNITY_SERVER
        //onClickDebugLobbyServer();
#endif
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        animator.Update();
    }

    private void _updateRoom(Lobby.Room room)
    {
        if (room == null)
            return;

        foreach (Transform child in playerListPanel.transform)
        {
            Destroy(child.gameObject);
        }
        
        for (int i = 0; i < room.PlayerList.Count; i++)
        {
            Lobby.Player roomPlayer = room.PlayerList[i];
            GameObject playerRowObj = Instantiate(playerInfoRowPrefab.gameObject, Vector3.zero, Quaternion.identity, playerListPanel.transform);
            PlayerInfoRow playerRow = playerRowObj.GetComponent<PlayerInfoRow>();
            playerRow.setPlayerNameText(roomPlayer.Name);
            playerRow.setReadyText(roomPlayer.IsReady);
            RectTransform tf = playerRowObj.GetComponent<RectTransform>();
            tf.localPosition  = new Vector3(0, GUI_ROOMPANEL_HEIGH - i * (tf.rect.height + GUI_ROOMPANEL_PLAYERINFOROW_GAP), 0);
        }
    }

    private void onConnectingLobby(List<Lobby.Room> roomList)
    {
        roomListPanel.UpdateRoom(roomList);
        if (state == MenuState.ConnectingLobby)
        {
            _setAllPlanelInactive();
            lobbyPanel.SetActive(true);
            state = MenuState.Lobby;
        }
    }

    private void onReceiveJoinRoomResponse(Lobby.ErrorType errorCode)
    {
        if (state != MenuState.JoiningRoom)
            return;

        if (errorCode == Lobby.ErrorType.None)
        {
            _setAllPlanelInactive();
            roomPanel.SetActive(true);
            state = MenuState.Room;
        }
        else
        {
            GameObject textObj = lobbyErrorPanel.transform.Find("Text").gameObject;
            string message = "";
            switch (errorCode)
            {
                case Lobby.ErrorType.InternalError:
                    message = "Lobby server internal error.";
                    break;
                case Lobby.ErrorType.LobbyTimeout:
                    message = "Connection time out.";
                    break;
                case Lobby.ErrorType.RoomFull:
                    message = "The room is full.";
                    break;
                case Lobby.ErrorType.RoomNotExist:
                    message = "The room no loner exist.";
                    break;
            }
            Text text = textObj.GetComponent<Text>();
            text.text = message;

            _setAllPlanelInactive();
            lobbyErrorPanel.SetActive(true);
            state = MenuState.LobbyError;
        }
    }

/************************************************************************************************
                                             LoginPanel                                             
************************************************************************************************/
    class Test_Model
    {
        public ObjectId _id { set; get; }
        public string account { set; get; }
        public string pw { set; get; }
        public string playerName { set; get; }
    }

    public void onClick_LoginPanel_Confirm()
    {
        /*
        InputField inputField = nameInputField.GetComponent<InputField>();
        gameConfiguration.MyName = inputField.text;
        Debug.Log("Debug: set name to " + gameConfiguration.MyName);
        loginPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        state = MenuState.MainMenu;
        */

        InputField accountField = accountInputField.GetComponent<InputField>();
        string account = accountField.text;
        InputField passwordField = passwordInputField.GetComponent<InputField>();
        string password = passwordField.text;

        //--------------------------------------TEST-------------------------------------
        const string MONGO_URI = "mongodb+srv://team4:Team4sGameIsGreat@cluster0.xchoj.mongodb.net";
        const string DATABASE_NAME = "TeamForce";
        MongoClient client;
        IMongoDatabase db;
        client = new MongoClient(MONGO_URI);
        db = client.GetDatabase(DATABASE_NAME);
        Debug.Log("Success connect to MongoDB!");

        IMongoCollection<Test_Model> userCollection = db.GetCollection<Test_Model>("Account");
        Test_Model modelUser = userCollection.Find(user => user.account.Equals(account)).SingleOrDefault();
        if (modelUser != null)
        {
            if (modelUser.pw == password)
            {
                Debug.Log("Login: Found playerName = " + modelUser.playerName);
                gameConfiguration.MyName = modelUser.playerName;
                loginPanel.SetActive(false);
                mainMenuPanel.SetActive(true);
                state = MenuState.MainMenu;
            }
            else
            {
                Debug.Log("Login: Incorrect password!");
                GameObject loginErrorTextObj = loginErrorPanel.transform.Find("Text").gameObject;
                Text errorText = loginErrorTextObj.GetComponent<Text>();
                errorText.text = "Incorrect password!";
                _setAllPlanelInactive();
                loginErrorPanel.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Login: account not found!");
            GameObject loginErrorTextObj = loginErrorPanel.transform.Find("Text").gameObject;
            Text errorText = loginErrorTextObj.GetComponent<Text>();
            errorText.text = "Account not found!";
            _setAllPlanelInactive();
            loginErrorPanel.SetActive(true);
        }
    }

    public void onClick_LoginPanel_DebugLogin()
    {
        _setAllPlanelInactive();
        debugLoginPanel.SetActive(true);
    }
/************************************************************************************************
                                             MainMenuPanel                                             
************************************************************************************************/
    public void onClickStartButton()
    {
        _setAllPlanelInactive();
        connectingLobbyPanel.SetActive(true);
        state = MenuState.ConnectingLobby;
        lobbyNetworkManager.ClientConnectLobby();
    }

    public void onClickSettingButton() {}

    public void onClickExitButton()
    {
        Application.Quit();
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
        flag_client_actively_disconnect = true;
        _setAllPlanelInactive();
        mainMenuPanel.SetActive(true);
        state = MenuState.MainMenu;
        lobbyNetworkManager.Disconnect();
    }

    public void onClick_Lobby_RoomRow(string uuid)
    {
        _setAllPlanelInactive();
        lobbyNetworkManager.JoinRoom(uuid);
        joiningRoomPanel.SetActive(true);
        state = MenuState.JoiningRoom;
    }
    /************************************************************************************************
                                                 RoomPanel                                             
    ************************************************************************************************/
    public void onClick_Room_BackButton()
    {
        lobbyNetworkManager.LeaveRoom();
        _setAllPlanelInactive();
        lobbyPanel.SetActive(true);
        state = MenuState.Lobby;
    }

    public void onClick_Room_ReadyButton()
    {
        lobbyNetworkManager.sendReadySignal();
    }
/************************************************************************************************
                                             ServerDisconnectPanel                                             
************************************************************************************************/
    public void onClick_ServerDisconnectPanel_Confirm()
    {
        _setAllPlanelInactive();
        mainMenuPanel.SetActive(true);
        state = MenuState.MainMenu;
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
                                             LobbyErrorPanel                                            
************************************************************************************************/
    public void onClick_LobbyErrorPanel_Confirm()
    {
        _setAllPlanelInactive();
        lobbyPanel.SetActive(true);
        state = MenuState.Lobby;
    }

/************************************************************************************************
                                             LobbyErrorPanel                                            
************************************************************************************************/
    public void onClick_LoginErrorPanel_Confirm()
    {
        _setAllPlanelInactive();
        loginPanel.SetActive(true);
        state = MenuState.Login;
    }
/************************************************************************************************
                                             DebugLoginPanel                                            
************************************************************************************************/
    public void onClick_DebugLoginPanel_Confirm()
    {
        InputField inputField = debugLoginInputField.GetComponent<InputField>();
        gameConfiguration.MyName = inputField.text;
        Debug.Log("Debug: set name to " + gameConfiguration.MyName);
        _setAllPlanelInactive();
        loginPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        state = MenuState.MainMenu;
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
        connectingLobbyPanel.SetActive(false);
        joiningRoomPanel.SetActive(false);
        lobbyErrorPanel.SetActive(false);
        debugLoginPanel.SetActive(false);
        loginErrorPanel.SetActive(false);
    }

}

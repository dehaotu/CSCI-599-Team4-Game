using System;
using System.Collections;
using System.Collections.Generic;
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
MenuController consists of a menu animator, and different Button handlers.
*/
public class MenuController : MonoBehaviour
{
    private bool isHost;    //true = host, false = client
    private MenuAnimator animator;
    private string myName;

    public GameObject backGroundMap;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject loginPanel;
    public GameObject mainMenuPanel;
    public GameObject nameInputField;
    public CustomRoomManager networkRoomManager;
    public CustomNetworkDiscovery roomDiscovery;

    public float cameraSpeedX = 0.4f;
    public float cameraSpeedY = 0.2f;
    public float period_rate = 0.5f; //how fast the camera to complete one back and forth

    // Start is called before the first frame update
    void Start()
    {
        animator = new MenuAnimator(backGroundMap, cameraSpeedX, cameraSpeedY, period_rate);

        isHost = false;
    }

    // Update is called once per frame
    void Update()
    {
        animator.Update();
    }

    public void onClickExitButton()
    {
        Debug.Log("onClickExitButton");
        Application.Quit();
    }

    public void onClickSettingButton()
    {
        Debug.Log("onClickSettingButton");
    }

    public void onClickStartButton()
    {
        Debug.Log("onClickStartButton");
        //SceneManager.LoadScene ("TestScene");
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        roomDiscovery.StartDiscovery();
    }

    public void onClickDebugLobbyServer()
    {
        Debug.Log("onClickDebugLobbyServer");
        mainMenuPanel.SetActive(false);
    }

    public void onClick_Lobby_BackButton()
    {
        Debug.Log("onClickLobby_backButton");
        lobbyPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        roomDiscovery.StartDiscovery();
    }

    public void onClick_Lobby_NewRoomButton()
    {
        Debug.Log("onClickLobby_NewRoomButton");
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        startHost();
    }

    public void onClick_Lobby_RoomRow()
    {
        Debug.Log("onClick_Lobby_RoomRow");
        lobbyPanel.SetActive(false);
        joinGame();
        roomPanel.SetActive(true);
    }

    public void onClick_Room_BackButton()
    {
        Debug.Log("onClick_Room_BackButton");
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
        if (isHost)
        {
            networkRoomManager.StopHost();
        }
        else
        {
            networkRoomManager.StopClient();
        }
    }

    public void onClick_Room_ReadyButton()
    {
        Debug.Log("onClick_Room_ReadyButton");
    }

    public void onClick_LoginPanel_Confirm()
    {
        Debug.Log("onClick_LoginPanel_Confirm");
        InputField inputField = nameInputField.GetComponent<InputField>();
        myName = inputField.text;
        Debug.Log("Debug: set name to " + myName);
        loginPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void debug_onServerFound()
    {
        Debug.Log("Server Found.");
    }

    public void joinGame()
    {
        networkRoomManager.StartClient();
        roomDiscovery.StopDiscovery();
        isHost = false;
    }

    public void startHost()
    {
        roomDiscovery.AdvertiseServer();
        networkRoomManager.StartHost();
        isHost = true;
    }

}

//Note: Mirror Lobby tutorial
//https://www.youtube.com/watch?v=Fx8efi2MNz0
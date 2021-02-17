using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using UnityEngine.UI;

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
    private MenuAnimator animator;

    public float cameraSpeedX = 0.4f;
    public float cameraSpeedY = 0.2f;
    public float period_rate = 0.5f; //how fast the camera to complete one back and forth

    // Start is called before the first frame update
    void Start()
    {
        GameObject panel = GameObject.Find("BackGroundMap");
        animator = new MenuAnimator(panel, cameraSpeedX, cameraSpeedY, period_rate);

        //set Button Text
        GameObject.Find("StartButton").GetComponentInChildren<Text>().text = "Start";
        GameObject.Find("SettingButton").GetComponentInChildren<Text>().text = "Setting";
        GameObject.Find("ExitButton").GetComponentInChildren<Text>().text = "Exit";
        GameObject.Find("Debug_LobbyServer").GetComponentInChildren<Text>().text = "Debug_Start_LobbyServer";
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
        SceneManager.LoadScene ("TestScene");
    }

    public void onClickDebugLobbyServer()
    {
        Debug.Log("onClickDebugLobbyServer");
    }
}

//Note: Mirror Lobby tutorial
//https://www.youtube.com/watch?v=Fx8efi2MNz0
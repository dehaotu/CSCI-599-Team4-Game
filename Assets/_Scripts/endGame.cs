using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class endGame : NetworkBehaviour
{
    private Canvas endGameCanvas;
    private Canvas topCanvas;
    private CanvasGroup endGameCanvasGroup;
    private Text endGameText;
    private Text endGameCountdownText;
    EvilLordBaseController evilLordBaseController;
    [SyncVar]
    private float endGameCountdown;
    [SyncVar]
    private bool isGameOver;

    private void Start()
    {
        endGameCountdown = 100.0f;
        isGameOver = false;

        endGameCanvas = GameObject.Find("EndGameCanvas").GetComponent<Canvas>();
        endGameCanvasGroup = endGameCanvas.GetComponent<CanvasGroup>();
        endGameText = endGameCanvas.GetComponentInChildren<Text>();
        topCanvas = GameObject.Find("TopCanvas").GetComponent<Canvas>();
        endGameCountdownText = topCanvas.GetComponentInChildren<Text>();
        endGameCanvasGroup.alpha = 0;

        if (isServer)
        {
            evilLordBaseController = GameObject.Find("EvilLordBase").GetComponent<EvilLordBaseController>();
        }
    }

    private void Update()
    {

        if (isServer)
        {
            if (!evilLordBaseController.isMonsterAlive())
            {
                Debug.Log("monster alive" + evilLordBaseController.isMonsterAlive());
                isGameOver = true;
            }
        }

        if (!isGameOver)
        {
            endGameCountdown -= Time.deltaTime;
            endGameCountdownText.text = CountdownFormat(countdown: endGameCountdown);
        }

        if (endGameCountdown > 0.0f & isGameOver == true)
        {
            WinGame();
            DisplayCanvas();
            return;
        }

        else if (endGameCountdown <= 0.0f & isGameOver == false)
        {
            LoseGame();
            DisplayCanvas();
            return;
        }
    }

    private string CountdownFormat(float countdown)
    {
        countdown = Mathf.Clamp(countdown, 0.0f, Mathf.Infinity);
        float minutes = Mathf.FloorToInt(countdown / 60);
        float seconds = Mathf.FloorToInt(countdown % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void WinGame()
    {
        isGameOver = true;
        endGameText.text = "VACTORY";
    }

    private void LoseGame()
    {
        isGameOver = true;
        endGameText.text = "GAME OVER";
    }

    private void DisplayCanvas()
    {
        endGameCanvasGroup.alpha = 1;
        endGameCanvasGroup.blocksRaycasts = true;
        endGameCanvasGroup.interactable = true;
    }

    public void Continue()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Quit()
    {
        Application.Quit();
    }

}

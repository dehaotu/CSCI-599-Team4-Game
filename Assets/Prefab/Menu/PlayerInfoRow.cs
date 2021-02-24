using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoRow : MonoBehaviour
{
    private const string READY_TEXT = "Ready";
    private const string NOT_READY_TEXT = "Not Ready";

    private string playerName;
    private Image image;

    public Text playerNameText;
    public Text readyText;

    public PlayerInfoRow(string playerName, bool isReady)
    {
        setPlayerNameText(playerName);
        setReadyText(isReady);
        image = GetComponent<Image>();
    }

    public void setReadyText(bool isReady) 
    {
        if (isReady)
        {
            readyText.text = READY_TEXT;
            image.color = Color.green;
        }
        else
        {
            readyText.text = NOT_READY_TEXT;
            image.color = Color.yellow;
        }
    }

    public void setPlayerNameText(string playerName)
    {
        this.playerName = playerName;
        playerNameText.text = playerName;
    }
}

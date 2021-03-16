using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChatWindow : MonoBehaviour
{
    public InputField chatMessage;
    public Text chatHistory;
    public Scrollbar scrollbar;
    public static bool isEditingInputField;
    private CanvasGroup canvasGroup;

    public void Awake()
    {
        HeroStatus.OnMessage += OnPlayerMessage;
        canvasGroup = gameObject.GetComponentInParent<Canvas>().GetComponent<CanvasGroup>();
    }

    private void Start() {
        canvasGroup.alpha = 0.0f;
    }

    private void Update() {
        isEditingInputField = chatMessage.GetComponentInChildren<InputField>().isFocused;
        // Debug.Log(isEditingInputField);
        if (!isEditingInputField) {
            if (Input.GetKeyDown(KeyCode.C)) 
            {
                if(canvasGroup.alpha == 0.0f) {
                    canvasGroup.alpha = 1.0f;
                }
                else if (canvasGroup.alpha == 1.0f) {
                    canvasGroup.alpha = 0.0f;
                }
            }
        }
    }

    void OnPlayerMessage(HeroStatus player, string message)
    {
        string prettyMessage = player.isLocalPlayer ?
            $"<color=red>{player.netId}: </color> {message}" :
            $"<color=blue>{player.netId}: </color> {message}";
        AppendMessage(prettyMessage);
        // Debug.Log(message);
    }

    public void OnSend()
    {
        if (chatMessage.text.Trim() == "")
            return;

        // get our player
        HeroStatus player = NetworkClient.connection.identity.GetComponent<HeroStatus>();

        // send a message
        player.CmdSend(chatMessage.text.Trim());

        chatMessage.text = "";
    }

    internal void AppendMessage(string message)
    {
        StartCoroutine(AppendAndScroll(message));
    }

    IEnumerator AppendAndScroll(string message)
    {
        chatHistory.text += message + "\n";

        // it takes 2 frames for the UI to update ?!?!
        yield return null;
        yield return null;

        // slam the scrollbar down
        scrollbar.value = 0;
    }
}

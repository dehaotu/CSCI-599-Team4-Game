using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PreTestSceneController : MonoBehaviour
{
    public CustomNetworkRoomManager customNetworkRoomManager;

    [Tooltip("Turn this on if you want to debug. If on, this scene will not automatically connect to the game server. And you need to open HUD GUI to connect.")]
    public bool DebugMode;

    // Start is called before the first frame update
    void Start()
    {
        if (!DebugMode)
            StartCoroutine(Connect());
    }

    IEnumerator Connect()
    {
        yield return 0; //wait for a frame first
        yield return 0; 
#if !UNITY_SERVER
        GameConfiguration config = GameConfiguration.Instance;
        customNetworkRoomManager.networkAddress = config.GameServerIP;
        customNetworkRoomManager.StartClient();
#endif
    }
}

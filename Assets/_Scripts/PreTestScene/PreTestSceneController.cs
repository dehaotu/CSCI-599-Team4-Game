using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PreTestSceneController : MonoBehaviour
{
    public CustomNetworkRoomManager customNetworkRoomManager;

    // Start is called before the first frame update
    void Start()
    {
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

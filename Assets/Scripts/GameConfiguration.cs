using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* GameConfiguration class is a singleton class that is used for
 * saving information that last between scenes, such as player's login name,
 * game settings, or IP address to game server.
 */
public class GameConfiguration : MonoBehaviour
{
    private static GameConfiguration _instance;
    public static GameConfiguration Instance { get { return _instance; } }

    #region Variables
    public string myName { set; get; }
    public string gameServerIP { set; get; }
    #endregion

    private void Awake()
    {
        if (_instance != null && this != _instance)
        {
            Debug.Log("Additional duplicated GameConfiguration is not allowed and will be deleted.");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}

﻿using UnityEngine;
using UnityEngine.UI;
using Mirror;
using kcp2k;
using System.Net;
using Lobby;

/*
	Documentation: https://mirror-networking.com/docs/Components/NetworkRoomManager.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomManager.html

	See Also: NetworkManager
	Documentation: https://mirror-networking.com/docs/Components/NetworkManager.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

/// <summary>
/// This is a specialized NetworkManager that includes a networked room.
/// The room has slots that track the joined players, and a maximum player count that is enforced.
/// It requires that the NetworkRoomPlayer component be on the room player objects.
/// NetworkRoomManager is derived from NetworkManager, and so it implements many of the virtual functions provided by the NetworkManager class.
/// </summary>
public class CustomNetworkRoomManager : NetworkRoomManager
{
    KcpClient LobbyCommunicationClient;
    [Tooltip("For Game server only: The IP Address of lobby server location for communication.")]
    public string LobbyServerAddress = "localhost";
    [Tooltip("For Game server only: The Port of lobby server location for communication.")]
    public ushort LobbyServerPort = 24601;
    [Tooltip("For Game server only: The name of the server.")]
    public string GameServerName = "Room";
    [Tooltip("The IP address of this game server being deployed to. If empty, it will use local address.")]
    public string myIP = "";

    #region LobbyServer Communication
    public void Awake()
    {
#if UNITY_SERVER
        Debug.Log("Looking for Lobby server at " + LobbyServerAddress + ":" + LobbyServerPort + "...");
        LobbyCommunicationClient = new KcpClient(OnConnected, OnDataReceived, OnDisconnected);
        try
        {
            LobbyCommunicationClient.Connect(LobbyServerAddress, LobbyServerPort, true, 10);
        }
        catch (System.Exception e)
        {
            Debug.Log("Lobby server Not found! (The game server will continue running and remain operational, but no lobby server will forward to players to this game server)");
        }
#endif
    }

    void Update()
    {
#if UNITY_SERVER
        LobbyCommunicationClient.Tick();
#endif
    }

    void OnConnected()
    {
        Debug.Log("Lobby server found.");
        
        if (myIP == "")
            myIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();

        KcpTransport kcpTransport = GetComponent<KcpTransport>();
        if (kcpTransport == null)
        {
            Debug.LogError("Error: cannot get component<KcpTransport> !");
        }
        Debug.Log("Registering myIP=" + myIP + ":" + kcpTransport.Port + " to lobby server.");
        MsgGameServerRegistry registryMsg = new MsgGameServerRegistry(myIP, kcpTransport.Port, GameServerName);
        LobbyCommunicationClient.Send(registryMsg.Serialize(), KcpChannel.Reliable);
        Debug.Log("Register complete!");
    }

    void OnDataReceived(System.ArraySegment<byte> message)
    {

    }

    void OnDisconnected()
    {
        Debug.Log("Disconnected from lobby server.");
    }

#endregion

#region Server Callbacks

    /// <summary>
    /// This is called on the server when the server is started - including when a host is started.
    /// </summary>
    public override void OnRoomStartServer() {
        base.OnRoomStartServer();
    }

    /// <summary>
    /// This is called on the server when the server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnRoomStopServer() {
        base.OnRoomStopServer();
    }

    /// <summary>
    /// This is called on the host when a host is started.
    /// </summary>
    public override void OnRoomStartHost() {
        base.OnRoomStartHost();
    }

    /// <summary>
    /// This is called on the host when the host is stopped.
    /// </summary>
    public override void OnRoomStopHost() {
        base.OnRoomStopHost();
    }

    /// <summary>
    /// This is called on the server when a new client connects to the server.
    /// </summary>
    /// <param name="conn">The new connection.</param>
    public override void OnRoomServerConnect(NetworkConnection conn) {
        base.OnRoomServerConnect(conn);
    }

    /// <summary>
    /// This is called on the server when a client disconnects.
    /// </summary>
    /// <param name="conn">The connection that disconnected.</param>
    public override void OnRoomServerDisconnect(NetworkConnection conn) {
        base.OnRoomServerDisconnect(conn);
    }

    /// <summary>
    /// This is called on the server when a networked scene finishes loading.
    /// </summary>
    /// <param name="sceneName">Name of the new scene.</param>
    public override void OnRoomServerSceneChanged(string sceneName) {
        base.OnRoomServerSceneChanged(sceneName);
    }

    /// <summary>
    /// This allows customization of the creation of the room-player object on the server.
    /// <para>By default the roomPlayerPrefab is used to create the room-player, but this function allows that behaviour to be customized.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <returns>The new room-player object.</returns>
    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnection conn)
    {
        return base.OnRoomServerCreateRoomPlayer(conn);
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>By default the gamePlayerPrefab is used to create the game-player, but this function allows that behaviour to be customized. The object returned from the function will be used to replace the room-player on the connection.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <param name="roomPlayer">The room player object for this connection.</param>
    /// <returns>A new GamePlayer object.</returns>
    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
    {
        Transform startPos = GetStartPosition();
        GameObject playerPrefabPrev = playerPrefab;
        playerPrefabPrev.GetComponent<HeroStatus>().playerName = roomPlayer.GetComponent<CustomRoomPlayer>().playerName;
        GameObject gamePlayer = startPos != null
            ? Instantiate(playerPrefabPrev, startPos.position, startPos.rotation)
            : Instantiate(playerPrefabPrev, Vector3.zero, Quaternion.identity);
        Debug.Log(gamePlayer.GetComponent<HeroStatus>().transform.Find("Status Canvas/Name").GetComponent<Text>().text);
        return gamePlayer;
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>This is only called for subsequent GamePlay scenes after the first one.</para>
    /// <para>See OnRoomServerCreateGamePlayer to customize the player object for the initial GamePlay scene.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    public override void OnRoomServerAddPlayer(NetworkConnection conn)
    {
        base.OnRoomServerAddPlayer(conn);
    }

    /// <summary>
    /// This is called on the server when it is told that a client has finished switching from the room scene to a game player scene.
    /// <para>When switching from the room, the room-player is replaced with a game-player object. This callback function gives an opportunity to apply state from the room-player to the game-player object.</para>
    /// </summary>
    /// <param name="conn">The connection of the player</param>
    /// <param name="roomPlayer">The room player object.</param>
    /// <param name="gamePlayer">The game player object.</param>
    /// <returns>False to not allow this player to replace the room player.</returns>
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }

    /// <summary>
    /// This is called on the server when all the players in the room are ready.
    /// <para>The default implementation of this function uses ServerChangeScene() to switch to the game player scene. By implementing this callback you can customize what happens when all the players in the room are ready, such as adding a countdown or a confirmation for a group leader.</para>
    /// </summary>
    public override void OnRoomServerPlayersReady()
    {
        base.OnRoomServerPlayersReady();
    }

    /// <summary>
    /// This is called on the server when CheckReadyToBegin finds that players are not ready
    /// <para>May be called multiple times while not ready players are joining</para>
    /// </summary>
    public override void OnRoomServerPlayersNotReady() { }

#endregion

#region Client Callbacks

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client enters the room.
    /// </summary>
    public override void OnRoomClientEnter() {
        Object[] roomPlayerList = GameObject.FindObjectsOfType(typeof(CustomRoomPlayer));
        foreach(Object roomPlayerObj in roomPlayerList)
        {
            CustomRoomPlayer roomPlayer = (CustomRoomPlayer)roomPlayerObj;
            if (roomPlayer.isLocalPlayer)
            {
                roomPlayer.CmdChangePlayerName(GameConfiguration.Instance.MyName);
                return;
            }
        }
    }

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client exits the room.
    /// </summary>
    public override void OnRoomClientExit() {
        base.OnRoomClientExit();
    }

    /// <summary>
    /// This is called on the client when it connects to server.
    /// </summary>
    /// <param name="conn">The connection that connected.</param>
    public override void OnRoomClientConnect(NetworkConnection conn) {
        base.OnRoomClientConnect(conn);
    }

    /// <summary>
    /// This is called on the client when disconnected from a server.
    /// </summary>
    /// <param name="conn">The connection that disconnected.</param>
    public override void OnRoomClientDisconnect(NetworkConnection conn) {
        base.OnRoomClientDisconnect(conn);
    }

    /// <summary>
    /// This is called on the client when a client is started.
    /// </summary>
    /// <param name="roomClient">The connection for the room.</param>
    public override void OnRoomStartClient() {
        base.OnRoomStartClient();
    }

    /// <summary>
    /// This is called on the client when the client stops.
    /// </summary>
    public override void OnRoomStopClient() {
        base.OnRoomStopClient();
    }

    /// <summary>
    /// This is called on the client when the client is finished loading a new networked scene.
    /// </summary>
    /// <param name="conn">The connection that finished loading a new networked scene.</param>
    public override void OnRoomClientSceneChanged(NetworkConnection conn) {
        base.OnRoomClientSceneChanged(conn);
    }

    /// <summary>
    /// Called on the client when adding a player to the room fails.
    /// <para>This could be because the room is full, or the connection is not allowed to have more players.</para>
    /// </summary>
    public override void OnRoomClientAddPlayerFailed() {
        base.OnRoomClientAddPlayerFailed();
    }

#endregion

#region Optional UI

    public override void OnGUI()
    {
        base.OnGUI();
    }

#endregion
}

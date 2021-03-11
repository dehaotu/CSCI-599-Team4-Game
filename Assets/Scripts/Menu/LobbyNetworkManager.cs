using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using kcp2k;
using Mirror;

/************************************************************************************************************************
                                                        enums for packet header
************************************************************************************************************************/
enum LobbyPacketHeader: byte
{
    PlayerInfoUpdate,
    LobbyInfoUpdate,
    CreateRoomRequest,
    CreateRoomResponse,
    JoinRoomRequest,
    JoinRoomResponse,
    RoomUpdate,
    ReadySignal,
    StartGame
}
/************************************************************************************************************************
                                                        Player Management class
************************************************************************************************************************/
public class Player
{
    public string Name {set; get;}
    public bool isInRoom {set; get;}
    public bool isReady {set; get;}
    public int connectionId {set; get;}

    // Use this when constructing packets.
    public Player(string Name) {
        this.Name = Name;
        this.isInRoom = false;
        this.isReady = false;
        this.connectionId = connectionId;
    }

    // Use this when a new player connects to the server. 
    public Player(string Name, int connectionId) : this(Name)
    {
        this.connectionId = connectionId;
    }
}

public class Room
{
    public string uuid;
    public string RoomName {set; get;}
    public int numPlayers {set; get;}
    public List<Player> PlayerList;

    public void AquireUUID() 
    {
        if (uuid != null)
            uuid = Guid.NewGuid().ToString();
    }
}
/************************************************************************************************************************
                                                            Packets
************************************************************************************************************************/
// The LobbyMsgHelper are code generators for the below LobbyMsg classes to construct and interpret byte messages.
public static class LobbyMsgHelper
{
    // Given a function that defines what to write into the packet, this converts the packet into a byte array.
    // the BinaryWriter can only write primitive type such as int, string, bool.
    public static ArraySegment<byte> GenerateMsg(Action<BinaryWriter> writingFunction)
    {
        using (MemoryStream m = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(m))
            {
                writingFunction.Invoke(writer);;
            } 
            return new ArraySegment<byte>(m.ToArray());
        }  
    }

    // Given a function that defines what to read, this constructs the packet back from a byte array.
    public static T InterpretMsg<T>(ArraySegment<byte> data, Func<BinaryReader, T> readingFunction) where T: class
    {
        using (MemoryStream m = new MemoryStream(data.Array)) 
        {
            m.Seek(2, SeekOrigin.Begin);    //skip 2 bytes, because the first byte is Kcp header, the second byte is LobbyPacketHeader.
            using (BinaryReader reader = new BinaryReader(m))
            {
                return readingFunction.Invoke(reader);
            }
        }   
    }
}

// All LobbyMessage must be able to be serialized. Deserialize is optional because some message are just signals 
// and contains no information.
abstract public class LobbyMsgBase
{
    abstract public ArraySegment<byte> Serialize();
    public static LobbyMsgBase Desserialize(ArraySegment<byte> data) {
        Log.Error("Calling LobbyMsgBase's Desserialize!");
        return null;
    }
}

// All players need to send this update immediately when they connect to the Lobby server in order to 
// register their information such as their name.
public class MsgPlayerInfoUpdate : LobbyMsgBase
{
    public string Name {set; get;}

    public MsgPlayerInfoUpdate(string Name) { this.Name = Name; }

    override public ArraySegment<byte> Serialize()
    {
        return LobbyMsgHelper.GenerateMsg((writer) => {
            writer.Write((byte)LobbyPacketHeader.PlayerInfoUpdate);
            writer.Write(Name);
        });
       
    }

    public static MsgPlayerInfoUpdate Desserialize(ArraySegment<byte> data) {
        return LobbyMsgHelper.InterpretMsg<MsgPlayerInfoUpdate>(data, (reader) => {
            return new MsgPlayerInfoUpdate(reader.ReadString());
        });
    }
}

// Server will boardcast this message to all players when there is any changes in the lobby (such as new game server 
// available/ numPlayer of a game server changes/ game server is down...) It also sends this message to player who 
// just join the lobby server.
public class MsgLobbyInfoUpdate : LobbyMsgBase
{
    public List<Room> roomList;

    override public ArraySegment<byte> Serialize()
    {
        return LobbyMsgHelper.GenerateMsg((writer) => {
            writer.Write((byte)LobbyPacketHeader.LobbyInfoUpdate);
            writer.Write(roomList.Count);
            foreach (Room room in roomList)
            {
                writer.Write(room.RoomName);
                writer.Write(room.PlayerList.Count);
            }
        });
    }

    public static MsgLobbyInfoUpdate Desserialize(ArraySegment<byte> data) {
        return LobbyMsgHelper.InterpretMsg<MsgLobbyInfoUpdate>(data, (reader) => {
            MsgLobbyInfoUpdate result = new MsgLobbyInfoUpdate();
            int roomListLength = reader.ReadInt32();
            result.roomList = new List<Room>(roomListLength);
            for (int i = 0; i < roomListLength; i++)
            {
                Room room = new Room();
                room.RoomName = reader.ReadString();
                room.numPlayers = reader.ReadInt32();
                result.roomList.Add(room);
            }
            return result;
        });
    }
}

// Sent by client when they want to join a room.
public class MsgJoinRoomRequest : LobbyMsgBase
{
    //To be Changed: client should also send the room's uuid, but since we have only one room for now, so we don't need that
    override public ArraySegment<byte> Serialize()
    {
        return LobbyMsgHelper.GenerateMsg((writer) => {
            writer.Write((byte)LobbyPacketHeader.JoinRoomRequest);
        });
    }
}

// The reply of MsgJoinRoomRequest. If success = false, failureMsg will explain why client cannot join the room.
public class MsgJoinRoomResponse : LobbyMsgBase
{
    public bool success;
    public string failureMsg;

    public MsgJoinRoomResponse(bool success, string failureMsg = "") 
    {
        this.success = success;
        this.failureMsg = failureMsg;
    }

    override public ArraySegment<byte> Serialize()
    {
        return LobbyMsgHelper.GenerateMsg((writer) => {
            writer.Write((byte)LobbyPacketHeader.JoinRoomResponse);
            writer.Write(success);
            writer.Write(failureMsg);
        });
    }

    public static MsgJoinRoomResponse Desserialize(ArraySegment<byte> data) {
        return LobbyMsgHelper.InterpretMsg<MsgJoinRoomResponse>(data, (reader) => {
            bool success = reader.ReadBoolean();
            string failureMsg = reader.ReadString();
            return new MsgJoinRoomResponse(success, failureMsg);
        });
    }
}

// Server send this to all players in the room to update the information.
public class MsgRoomUpdate : LobbyMsgBase
{
    public string RoomName;
    public List<Player> playerList;

    override public ArraySegment<byte> Serialize()
    {
        return LobbyMsgHelper.GenerateMsg((writer) => {
            writer.Write((byte)LobbyPacketHeader.RoomUpdate);
            writer.Write(RoomName);
            writer.Write(playerList.Count);
            foreach (Player player in playerList)
            {
                writer.Write(player.Name);
                writer.Write(player.isReady);
            }
        });
    }

    public static MsgRoomUpdate Desserialize(ArraySegment<byte> data) 
    {
        return LobbyMsgHelper.InterpretMsg<MsgRoomUpdate>(data, (reader) => {
            MsgRoomUpdate result = new MsgRoomUpdate();
            result.RoomName = reader.ReadString();
            int size = reader.ReadInt32();
            result.playerList = new List<Player>();
            for (int i = 0; i < size; i++)
            {
                Player player = new Player(reader.ReadString());
                player.isReady = reader.ReadBoolean();
                result.playerList.Add(player);
            }
            return result;
        });
    }
}

// Client will send this signal to server when they click the ready button.
public class MsgReadySignal : LobbyMsgBase
{
    override public ArraySegment<byte> Serialize()
    {
        return LobbyMsgHelper.GenerateMsg((writer) => {
            writer.Write((byte)LobbyPacketHeader.ReadySignal);
        });
    }
}

// Server tells the client to connect to the game server
public class MsgGameStartSignal : LobbyMsgBase
{
    string GameServerIP;
    int GameServerPort;

    public MsgGameStartSignal(string GameServerIP, int GameServerPort)
    {
        this.GameServerIP = GameServerIP;
        this.GameServerPort = GameServerPort;
    }

    override public ArraySegment<byte> Serialize()
    {
        return LobbyMsgHelper.GenerateMsg((writer) => {
            writer.Write((byte)LobbyPacketHeader.StartGame);
            writer.Write(GameServerIP);
            writer.Write(GameServerPort);
        });
    }

    public static MsgGameStartSignal Desserialize(ArraySegment<byte> data) 
    {
        return LobbyMsgHelper.InterpretMsg<MsgGameStartSignal>(data, (reader) => {
            string ip = reader.ReadString();
            int port = reader.ReadInt32();
            return  new MsgGameStartSignal(ip, port);       
        });
    } 
}
/************************************************************************************************************************
                                                    LobbyNetworkManager
************************************************************************************************************************/
public class LobbyNetworkManager : MonoBehaviour
{
    //constants
    const int MAX_ROOM_PLAYERS = 4;

    //configurations
    [Header("Transport Configuration")]
    public ushort Port = 27777;
    [Tooltip("The IP Address of Lobby server.")]
    public string LobbyServerIP = "localhost";
    [Tooltip("NoDelay is recommended to reduce latency. This also scales better without buffers getting full.")]
    public bool NoDelay = true;
    [Tooltip("KCP internal update interval. 100ms is KCP default, but a lower interval is recommended to minimize latency and to scale to more networked entities.")]
    public uint Interval = 10;
    [Header("Advanced")]
    [Tooltip("KCP fastresend parameter. Faster resend for the cost of higher bandwidth. 0 in normal mode, 2 in turbo mode.")]
    public int FastResend = 2;
    [Tooltip("KCP congestion window. Enabled in normal mode, disabled in turbo mode. Disable this for high scale games if connections get choked regularly.")]
    public bool CongestionWindow = false; // KCP 'NoCongestionWindow' is false by default. here we negate it for ease of use.
    [Tooltip("KCP window size can be modified to support higher loads.")]
    public uint SendWindowSize = 4096; //Kcp.WND_SND; 32 by default. Mirror sends a lot, so we need a lot more.
    [Tooltip("KCP window size can be modified to support higher loads.")]
    public uint ReceiveWindowSize = 4096; //Kcp.WND_RCV; 128 by default. Mirror sends a lot, so we need a lot more.

    //reference
    public MenuController menuController;
    public RoomListPanel roomListPanel;

    //variables
    KcpServer server;
    KcpClient client;

    Dictionary<int, Player> connIdToPlayer;     //for server only.
    List<Room> roomList;
    public Room currRoom;       //for client only. This is null if client is not in any room, else it is the client's current room.


    //public static LobbyNetworkManager selfReference;

    // Call this when the LobbyNetworkManager is being loaded into the scene
    void Awake()
    {
        //selfReference = this;
        DontDestroyOnLoad(transform.gameObject);
        connIdToPlayer = new Dictionary<int, Player>();
        roomList = new List<Room>();

        //To be changed. For now we assume there is already a game server in the list.
#if UNITY_SERVER
        Room gameRoom1 = new Room();
        gameRoom1.RoomName = "Room";
        gameRoom1.PlayerList = new List<Player>();
        roomList.Add(gameRoom1);
#endif
        

#if UNITY_SERVER
        server = new KcpServer(
            (connectionId) => OnServerConnected(connectionId),
            (connectionId, message) => OnServerDataReceived(connectionId, message),
            (connectionId) => OnServerDisconnected(connectionId),
            NoDelay,
            Interval,
            FastResend,
            CongestionWindow,
            SendWindowSize,
            ReceiveWindowSize
        );
        server.Start(Port);
#else
        client = new KcpClient(
            () => OnClientConnected(),
            (message) => OnClientDataReceived(message),
            () => OnClientDisconnected()
        );
#endif
    }

    void Update()
    {
#if UNITY_SERVER
            server.Tick();
#else
            client.Tick();
#endif
    }

    public void ClientConnectLobby()
    {
        Debug.Log("Debug IP: " + LobbyServerIP);
        client.Connect(
            LobbyServerIP, 
            Port,
            NoDelay,
            Interval,
            FastResend,
            CongestionWindow,
            SendWindowSize,
            ReceiveWindowSize
        );
    }
/************************************************************************************************************************
                                                    LobbyNetworkManager(Server)
************************************************************************************************************************/
    // Invoked when a client connects to the server
    void OnServerConnected(int connectionId)
    {
        Debug.Log("OnServerConnected: " + connectionId.ToString());
        // Generate a new player instance for the new connection, put it in dictionary
        connIdToPlayer.Add(connectionId, new Player("NewPlayer"));
    }

    // Invoked when a server receives a message
    void OnServerDataReceived(int connectionId, ArraySegment<byte> message)
    {
        // Read the LobbyPacketHeader in the packet
        MemoryStream m = new MemoryStream(message.Array);
        m.Seek(1, SeekOrigin.Begin);        //skip 1 byte, because the first byte is Kcp header
        BinaryReader reader = new BinaryReader(m);
        LobbyPacketHeader header = (LobbyPacketHeader) reader.ReadByte();
        switch (header)
        {
            case LobbyPacketHeader.PlayerInfoUpdate:
                HandlePlayerInfoUpdate(connectionId, MsgPlayerInfoUpdate.Desserialize(message));
                break;
            case LobbyPacketHeader.JoinRoomRequest:
                HandleJoinRoomRequest(connectionId, null);
                break;
            case LobbyPacketHeader.ReadySignal:
                HandleReadySignal(connectionId, null);
                break;

        }
    }

    // Invoked when a client disconnected from the server
    void OnServerDisconnected(int connectionId)
    {
        Debug.Log("OnServerDisconnected: " + connectionId.ToString());
    }

    // Defines what server should do when received a packet from player.
    // Once complete registering the new player, the lobby server should send the current lobby info to player.
    void HandlePlayerInfoUpdate(int connectionId, MsgPlayerInfoUpdate packet)
    {
        connIdToPlayer[connectionId] = new Player(packet.Name, connectionId);
        Debug.Log(connIdToPlayer[connectionId].Name + " has connected to the Lobby Server.");

        MsgLobbyInfoUpdate msg = new MsgLobbyInfoUpdate();
        msg.roomList = new List<Room>(roomList);
        server.Send(connectionId, msg.Serialize(), KcpChannel.Reliable);
    }

    // When a player join a room, the server should first check if the room has reached numPlayer limit.
    // Then the server will update the room data, and boardcast RoomUpdate to all player in the room.
    // After that, the server should also boardcast LobbyInfoUpdate to all player in the server (because
    // the numPlayer of that server has updated).
    void HandleJoinRoomRequest(int connectionId, MsgJoinRoomRequest packet)
    {
        MsgJoinRoomResponse response;
        if (roomList[0].PlayerList.Count >= MAX_ROOM_PLAYERS)
        {
            response = new MsgJoinRoomResponse(false, "The room is full.");
            server.Send(connectionId, response.Serialize(), KcpChannel.Reliable);
            return;
        }
        Player player = connIdToPlayer[connectionId];
        roomList[0].PlayerList.Add(player);
        MsgRoomUpdate roomUpdateMsg = new MsgRoomUpdate();
        roomUpdateMsg.RoomName = roomList[0].RoomName;
        roomUpdateMsg.playerList = new List<Player>(roomList[0].PlayerList);

        List<int> connList = new List<int>();
        foreach (Player p in roomList[0].PlayerList)
        {
            connList.Add(p.connectionId);
        }
        MultiCast(roomUpdateMsg, connList.ToArray());

        MsgLobbyInfoUpdate lobbyInfoMsg = new MsgLobbyInfoUpdate();
        lobbyInfoMsg.roomList = new List<Room>(roomList);
        BoardCast(lobbyInfoMsg);
    }

    // Flip the player's ready state. Check if all players in the room are ready, if yes, then forward them to the game server.
    void HandleReadySignal(int connectionId, MsgReadySignal packet)
    {
        connIdToPlayer[connectionId].isReady = !connIdToPlayer[connectionId].isReady;

        MsgRoomUpdate roomUpdateMsg = new MsgRoomUpdate();
        roomUpdateMsg.RoomName = roomList[0].RoomName;
        roomUpdateMsg.playerList = new List<Player>(roomList[0].PlayerList);

        List<int> connList = new List<int>();
        foreach (Player p in roomList[0].PlayerList)
        {
            connList.Add(p.connectionId);
        }
        MultiCast(roomUpdateMsg, connList.ToArray());

        //To be changed! We assume there is only one room in the room for now. Server needs to know what room the player is in.
        bool allReady = true;
        foreach (Player player in roomList[0].PlayerList)
        {
            allReady = allReady & player.isReady;
        }

        if (allReady)
        {
            Debug.Log("All player ready. Game Start");
            MsgGameStartSignal gameStartSignal = new MsgGameStartSignal("52.183.4.114", 7777);
            MultiCast(gameStartSignal, connList.ToArray());
        }
    }

    // BoardCast the LobbyMessage to all connections in the list
    void MultiCast(LobbyMsgBase packet, int[] conns)
    {
        foreach (int connectionId in conns)
            server.Send(connectionId, packet.Serialize(), KcpChannel.Reliable);
    }

    // BoardCast the LobbyMessage to all players in the server
    void BoardCast(LobbyMsgBase packet)
    {
        foreach (int connectionId in connIdToPlayer.Keys)
            server.Send(connectionId, packet.Serialize(), KcpChannel.Reliable);
    }

/************************************************************************************************************************
                                                    LobbyNetworkManager(Client)
************************************************************************************************************************/
    // Invoked when a client connects to the lobby server. Send player information immediately.
    void OnClientConnected()
    {
        MsgPlayerInfoUpdate msg = new MsgPlayerInfoUpdate(menuController.myName);
        client.Send(msg.Serialize(), KcpChannel.Reliable);
    }

    // Invoked when a client receives a message from the lobby server.
    void OnClientDataReceived(ArraySegment<byte> message)
    {
        // Read the LobbyPacketHeader in the packet
        MemoryStream m = new MemoryStream(message.Array);
        m.Seek(1, SeekOrigin.Begin);        //skip 1 byte, because the first byte is Kcp header
        BinaryReader reader = new BinaryReader(m);
        LobbyPacketHeader header = (LobbyPacketHeader) reader.ReadByte();
        switch (header)
        {
            case LobbyPacketHeader.LobbyInfoUpdate:
                HandleLobbyInfoUpdate(MsgLobbyInfoUpdate.Desserialize(message));
                break;
            case LobbyPacketHeader.JoinRoomResponse:
                HandleJoinRoomResponse(MsgJoinRoomResponse.Desserialize(message));
                break;
            case LobbyPacketHeader.RoomUpdate:
                HandleRoomUpdate(MsgRoomUpdate.Desserialize(message));
                break;
            case LobbyPacketHeader.StartGame:
                HandleGameStartSignal(MsgGameStartSignal.Desserialize(message));
                break;
        }
    }

    // Invoked when client is disconnected from the lobby server
    void OnClientDisconnected()
    {
        Debug.Log("OnClientDisconnected");
    }

    // Defines what to do when client receives MsgLobbyInfoUpdate packet.
    // Update the LobbyPanel.
    void HandleLobbyInfoUpdate(MsgLobbyInfoUpdate packet)
    {
        roomList = new List<Room>(packet.roomList);
        Debug.Log("Debug: HandleLobbyInfoUpdate");
        for (int i = 0; i < roomList.Count; i++)
        {
            Debug.Log(roomList[0].RoomName + "\t\t" + roomList[0].numPlayers);
        }
        roomListPanel.UpdateRoom(roomList[0].numPlayers);
    }

    // Defines what to do when client receives MsgJoinRoomResponse packet.
    // If success, move to the room Panel
    // if failed, show message window.
    void HandleJoinRoomResponse(MsgJoinRoomResponse packet)
    {
        if (packet.success)
        {
            Debug.Log("Join room success!");
        }
        else
        {

        }
    }

    void HandleRoomUpdate(MsgRoomUpdate packet)
    {
        currRoom = new Room();
        currRoom.PlayerList = new List<Player>(packet.playerList);
        currRoom.RoomName = packet.RoomName;

        Debug.Log("HandleRoomUpdate: " + currRoom.RoomName);
        foreach (Player player in currRoom.PlayerList)
        {
            Debug.Log(player.Name + "\t\t" + player.isReady);
        }
    }

    void HandleGameStartSignal(MsgGameStartSignal packet)
    {  
        SceneManager.sceneLoaded += (scene, mode) => {
            if (scene.name == "TestScene")
            {
                // to be changed. Now we assume the both lobby server and game server are in the same vm instance
                GameObject networkManagerObj = GameObject.Find("NetworkManager");

                if (networkManagerObj != null)
                    Debug.Log("networkManagerObj found");
                else
                    Debug.Log("networkManagerObj NOT FOUND!!!");
                NetworkManager networkManager = networkManagerObj.GetComponent<NetworkManager>();
                networkManager.networkAddress = LobbyServerIP;
                Debug.Log("Now the networkManager's networkAddress is " + LobbyServerIP );
                networkManager.StartClient();
                Debug.Log("Called startclient()");
            }
        };

        SceneManager.LoadScene("TestScene");

    }

    // Send the joinRoom message to server.
    public void JoinRoom(String roomUUID)
    {
        MsgJoinRoomRequest msg = new MsgJoinRoomRequest();
        client.Send(msg.Serialize(), KcpChannel.Reliable);
    }

    // Send the player's ready state to the server.
    public void sendReadySignal()
    {
        MsgReadySignal msg = new MsgReadySignal();
        client.Send(msg.Serialize(), KcpChannel.Reliable);
    }
}

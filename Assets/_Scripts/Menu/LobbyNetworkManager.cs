using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using kcp2k;
using Mirror;
using Lobby;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

/************************************************************************************************************************
                                                    Lobby Support Class
************************************************************************************************************************/
namespace Lobby
{
    public enum LobbyPacketHeader : byte
    {
        PlayerInfoUpdate,
        LobbyInfoUpdate,
        CreateRoomRequest,
        CreateRoomResponse,
        JoinRoomRequest,
        JoinRoomResponse,
        LeaveRoomSignal,
        RoomUpdate,
        ReadySignal,
        StartGame
    }

    public enum RoomErrorType: byte
    {
        None,
        LobbyTimeout,
        RoomFull,
        RoomNotExist,
        InternalError
    }

    #region LobbyDataClass
    public class Player
    {
        public string uuid { get; }
        public string Name { set; get; }
        public bool IsReady { set; get; }
        public int ConnectionId { set; get; }
        public Room CurrRoom { set; get; }

        public Player(string uuid = null, int connectionId = -1, string name = "UnnamedPlayer",
                      bool isReady = false, Room currRoom = null)
        {
            this.uuid = (uuid == null) ? Guid.NewGuid().ToString() : uuid;
            this.ConnectionId = connectionId;
            this.Name = name;
            this.IsReady = isReady;
            this.CurrRoom = currRoom;
        }
    }

    public class Room
    {
        public string uuid { get; }
        public string RoomName { set; get; }
        public int NumPlayers { set; get; }
        public List<Player> PlayerList;

        public Room(string uuid = null, string roomName = "Room", int numPlayers = 0, List<Player> playerList = null)
        {
            this.uuid = (uuid == null) ? Guid.NewGuid().ToString() : uuid;
            this.RoomName = roomName;
            this.NumPlayers = numPlayers;
            this.PlayerList = (playerList == null) ? new List<Player>() : new List<Player>(playerList);
        }
    }
    #endregion

    #region Packets
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
                    writingFunction.Invoke(writer);
                }
                return new ArraySegment<byte>(m.ToArray());
            }
        }

        // Given a function that defines what to read, this constructs the packet back from a byte array.
        public static T InterpretMsg<T>(ArraySegment<byte> data, Func<BinaryReader, T> readingFunction) where T : class
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
        public static LobbyMsgBase Desserialize(ArraySegment<byte> data)
        {
            Log.Error("Calling LobbyMsgBase's Desserialize!");
            return null;
        }
    }

    // All players need to send this update immediately when they connect to the Lobby server in order to 
    // register their information such as their name.
    public class MsgPlayerInfoUpdate : LobbyMsgBase
    {
        public string Name { set; get; }

        public MsgPlayerInfoUpdate(string Name) { this.Name = Name; }

        override public ArraySegment<byte> Serialize()
        {
            return LobbyMsgHelper.GenerateMsg((writer) =>
            {
                writer.Write((byte)LobbyPacketHeader.PlayerInfoUpdate);
                writer.Write(Name);
            });
        }

        public new static MsgPlayerInfoUpdate Desserialize(ArraySegment<byte> data)
        {
            return LobbyMsgHelper.InterpretMsg<MsgPlayerInfoUpdate>(data, (reader) =>
            {
                return new MsgPlayerInfoUpdate(reader.ReadString());
            });
        }
    }

    // Server will boardcast this message to all players when there is any changes in the lobby (such as new game server 
    // available/ numPlayer of a game server changes/ game server is down...) It also sends this message to player who 
    // just join the lobby server.
    public class MsgLobbyInfoUpdate : LobbyMsgBase
    {
        public List<Room> RoomList;

        override public ArraySegment<byte> Serialize()
        {
            return LobbyMsgHelper.GenerateMsg((writer) =>
            {
                writer.Write((byte)LobbyPacketHeader.LobbyInfoUpdate);
                writer.Write(RoomList.Count);
                foreach (Room room in RoomList)
                {
                    writer.Write(room.RoomName);
                    writer.Write(room.uuid);
                    writer.Write(room.PlayerList.Count);
                }
            });
        }

        public new static MsgLobbyInfoUpdate Desserialize(ArraySegment<byte> data)
        {
            return LobbyMsgHelper.InterpretMsg<MsgLobbyInfoUpdate>(data, (reader) =>
            {
                MsgLobbyInfoUpdate result = new MsgLobbyInfoUpdate();
                int roomListLength = reader.ReadInt32();
                result.RoomList = new List<Room>(roomListLength);
                for (int i = 0; i < roomListLength; i++)
                {
                    string roomName = reader.ReadString();
                    string uuid = reader.ReadString();
                    int numPlayers = reader.ReadInt32();
                    result.RoomList.Add(new Room(uuid: uuid, roomName: roomName, numPlayers: numPlayers));
                }
                return result;
            });
        }
    }

    // Sent by client when they want to join a room.
    public class MsgJoinRoomRequest : LobbyMsgBase
    {
        public string RoomUuid;
        public MsgJoinRoomRequest(string uuid) { RoomUuid = uuid; }
        override public ArraySegment<byte> Serialize()
        {
            return LobbyMsgHelper.GenerateMsg((writer) => {
                writer.Write((byte)LobbyPacketHeader.JoinRoomRequest);
                writer.Write(RoomUuid);
            });
        }

        public new static MsgJoinRoomRequest Desserialize(ArraySegment<byte> data)
        {
            return LobbyMsgHelper.InterpretMsg<MsgJoinRoomRequest>(data, (reader) =>
            {
                return new MsgJoinRoomRequest(reader.ReadString());
            });
        }
    }

    // The reply of MsgJoinRoomRequest. If success, ErrorCode = ErrorType.None, else, the errorcode is set.
    public class MsgJoinRoomResponse : LobbyMsgBase
    {
        public RoomErrorType ErrorCode;

        public MsgJoinRoomResponse(RoomErrorType errorCode = RoomErrorType.None)
        {
            this.ErrorCode = errorCode;
        }

        override public ArraySegment<byte> Serialize()
        {
            return LobbyMsgHelper.GenerateMsg((writer) => {
                writer.Write((byte)LobbyPacketHeader.JoinRoomResponse);
                writer.Write((byte)ErrorCode);
            });
        }

        public new static MsgJoinRoomResponse Desserialize(ArraySegment<byte> data)
        {
            return LobbyMsgHelper.InterpretMsg<MsgJoinRoomResponse>(data, (reader) => {
                bool success = reader.ReadBoolean();
                RoomErrorType errorCode = (RoomErrorType) reader.ReadByte();
                return new MsgJoinRoomResponse(errorCode);
            });
        }
    }

    // Server send this to all players in the room to update the information.
    public class MsgRoomUpdate : LobbyMsgBase
    {
        public string RoomName;
        public List<Player> PlayerList;

        override public ArraySegment<byte> Serialize()
        {
            return LobbyMsgHelper.GenerateMsg((writer) => {
                writer.Write((byte)LobbyPacketHeader.RoomUpdate);
                writer.Write(RoomName);
                writer.Write(PlayerList.Count);
                foreach (Player player in PlayerList)
                {
                    writer.Write(player.uuid);
                    writer.Write(player.Name);
                    writer.Write(player.IsReady);
                }
            });
        }

        public new static MsgRoomUpdate Desserialize(ArraySegment<byte> data)
        {
            return LobbyMsgHelper.InterpretMsg<MsgRoomUpdate>(data, (reader) => {
                MsgRoomUpdate result = new MsgRoomUpdate();
                result.RoomName = reader.ReadString();
                int size = reader.ReadInt32();
                result.PlayerList = new List<Player>();
                for (int i = 0; i < size; i++)
                {
                    string uuid = reader.ReadString();
                    string playerName = reader.ReadString();
                    bool isReady = reader.ReadBoolean();
                    Player player = new Player(uuid: uuid, name: playerName, isReady: isReady);
                    result.PlayerList.Add(player);
                }
                return result;
            });
        }
    }

    // Client will send this to lobby server when clicking the back button in room
    public class MsgLeaveRoomSignal : LobbyMsgBase
    {
        override public ArraySegment<byte> Serialize()
        {
            return LobbyMsgHelper.GenerateMsg((writer) =>
            {
                writer.Write((byte)LobbyPacketHeader.LeaveRoomSignal);
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
        public string GameServerIP { set; get; }
        public int GameServerPort { set; get; }

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

        public new static MsgGameStartSignal Desserialize(ArraySegment<byte> data)
        {
            return LobbyMsgHelper.InterpretMsg<MsgGameStartSignal>(data, (reader) => {
                string ip = reader.ReadString();
                int port = reader.ReadInt32();
                return new MsgGameStartSignal(ip, port);
            });
        }
    }
}
#endregion

/************************************************************************************************************************
                                                            LobbyNetworkManager
************************************************************************************************************************/

public class LobbyNetworkManager : MonoBehaviour
{
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
    [Tooltip("If checked, the lobby will always direct players to game server at localhost:7777")]
    public bool DebugLocalServers = false;
    [Header("Events")]

    //reference
    public GameConfiguration Config;

    //variables
    KcpServer LobbyServer;
    //KcpServer GameServerManagement;
    KcpClient Client;
    Dictionary<int, Player> ConnIdToPlayer;       // For server only.
    Dictionary<string, Room> UuidToRoom;
    public Room CurrRoom;             //to be changed, please add getters

    //events
    // Define what to do after lobby server has processed LobbyInfoUpdate
    // Each room in the roomList has uuid, roomName and numPlayers, but their
    // playerList is null.
    public Action<List<Room>> Event_ReceiveLobbyInfoUpdate = (roomList) => { };

    // Define what to do after client receive roomUpdate.
    // The room contains updated information of the player's room.
    public Action<Room> Event_ReceiveRoomUpdate = (room) => { };

    // Define what to do after client receive JoinRoomResponse.
    public Action<RoomErrorType> Event_ReceiveJoinRoomResponse = (errorType) => { };

    // Define what to do after client diconnected from lobby server.
    public Action Event_ClientDisconnected = () => { };

    private void Awake()
    {
        ConnIdToPlayer = new Dictionary<int, Player>();
        UuidToRoom = new Dictionary<string, Room>();

#if UNITY_SERVER
        LobbyServer = new KcpServer(
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
        LobbyServer.Start(Port);
#else
        Client = new KcpClient(
            () => OnClientConnected(),
            (message) => OnClientDataReceived(message),
            () => OnClientDisconnected()
        );
#endif

        //To be changed. For now we assume there is already a game server in the list.
#if UNITY_SERVER
        Room gameRoom1 = new Room();
        UuidToRoom.Add(gameRoom1.uuid, gameRoom1);
#endif
    }

    void Update()
    {
#if UNITY_SERVER
        LobbyServer.Tick();
#else
        Client.Tick();
#endif
    }

    ~LobbyNetworkManager()
    {
        if (Client != null)
            Client.Disconnect();
        if (LobbyServer != null)
        {
            foreach (int connectionId in ConnIdToPlayer.Keys)
                LobbyServer.Disconnect(connectionId);
        }
            
    }

#region Server
    // BoardCast the LobbyMessage to all connections in the list
    void MultiCast(LobbyMsgBase packet, int[] conns)
    {
        foreach (int connectionId in conns)
            LobbyServer.Send(connectionId, packet.Serialize(), KcpChannel.Reliable);
    }

    // BoardCast the LobbyMessage to all players in the server
    void BoardCast(LobbyMsgBase packet)
    {
        foreach (int connectionId in ConnIdToPlayer.Keys)
            LobbyServer.Send(connectionId, packet.Serialize(), KcpChannel.Reliable);
    }

    // Invoked when a client connects to the server.
    void OnServerConnected(int connectionId)
    {
        //do nothing
    }

    // Invoked when a server receives a message from client.
    void OnServerDataReceived(int connectionId, ArraySegment<byte> message)
    {
        // Read the LobbyPacketHeader in the packet
        MemoryStream m = new MemoryStream(message.Array);
        m.Seek(1, SeekOrigin.Begin);        // Skip 1 byte, because the first byte is Kcp header.
        BinaryReader reader = new BinaryReader(m);
        LobbyPacketHeader header = (LobbyPacketHeader) reader.ReadByte();
        switch (header)
        {
            case LobbyPacketHeader.PlayerInfoUpdate:
                HandlePlayerInfoUpdate(connectionId, MsgPlayerInfoUpdate.Desserialize(message));
                break;
            case LobbyPacketHeader.JoinRoomRequest:
                HandleJoinRoomRequest(connectionId, MsgJoinRoomRequest.Desserialize(message));
                break;
            case LobbyPacketHeader.ReadySignal:
                HandleReadySignal(connectionId);
                break;
            case LobbyPacketHeader.LeaveRoomSignal:
                HandleLeaveRoomSignal(connectionId);
                break;
        }
    }


    // Invoked when a client disconnected from the server
    // Remove player in ConnToPlayer UuidToRoom. If player is in a room, boardcast
    // update to all players in the room.
    void OnServerDisconnected(int connectionId)
    {
        Debug.Log("Client " + connectionId.ToString() + " has disconnected.");
        Player player;
        if (!ConnIdToPlayer.TryGetValue(connectionId, out player))
        {
            Debug.LogError("Cannot find " + connectionId.ToString() + "!");
            return;
        }
        if (player.CurrRoom != null)
        {
            HandleLeaveRoomSignal(connectionId);
        }
        ConnIdToPlayer.Remove(connectionId);  
    }

    // Defines what server should do when received a packet from player.
    // Once complete registering the new player, the lobby server should send the current lobby info to player.
    void HandlePlayerInfoUpdate(int connectionId, MsgPlayerInfoUpdate packet)
    {
        ConnIdToPlayer[connectionId] = new Player(connectionId: connectionId, name : packet.Name);
        Debug.Log(ConnIdToPlayer[connectionId].Name + " has connected to the Lobby Server.");

        MsgLobbyInfoUpdate msg = new MsgLobbyInfoUpdate();
        msg.RoomList = new List<Room>(UuidToRoom.Values);
        LobbyServer.Send(connectionId, msg.Serialize(), KcpChannel.Reliable);
    }

    // When a player join a room, the server should first check if the room has reached numPlayer limit.
    // Then the server will update the room data, and boardcast RoomUpdate to all player in the room.
    // After that, the server should also boardcast LobbyInfoUpdate to all player in the server (because
    // the numPlayer of that server has been updated).
    void HandleJoinRoomRequest(int connectionId, MsgJoinRoomRequest packet)
    {
        //check room size
        string roomUuid = packet.RoomUuid;
        Room room;
        Player player;
        MsgJoinRoomResponse response;
        if (!UuidToRoom.TryGetValue(packet.RoomUuid, out room))
        {
            response = new MsgJoinRoomResponse(RoomErrorType.RoomNotExist);
            LobbyServer.Send(connectionId, response.Serialize(), KcpChannel.Reliable);
            return;
        }

        if (!ConnIdToPlayer.TryGetValue(connectionId, out player))
        {
            Debug.LogError("Unknown connectionId=" + connectionId.ToString() + " is trying to join a room.");
            response = new MsgJoinRoomResponse(RoomErrorType.InternalError);
            LobbyServer.Send(connectionId, response.Serialize(), KcpChannel.Reliable);
            return;
        }

        if (room.NumPlayers > GameConfiguration.MAX_ROOM_PLAYERS)
        {
            response = new MsgJoinRoomResponse(RoomErrorType.RoomFull);
            LobbyServer.Send(connectionId, response.Serialize(), KcpChannel.Reliable);
            return;
        }

        response = new MsgJoinRoomResponse(RoomErrorType.None);
        LobbyServer.Send(connectionId, response.Serialize(), KcpChannel.Reliable);

        UuidToRoom[packet.RoomUuid].PlayerList.Add(player);
        UuidToRoom[packet.RoomUuid].NumPlayers = room.PlayerList.Count;
        ConnIdToPlayer[connectionId].CurrRoom = room;
        Debug.Log("Player " + player.Name + " joined " + room.RoomName + ".");
        MultiCastRoomUpdate(UuidToRoom[packet.RoomUuid]);

        MsgLobbyInfoUpdate lobbyInfoMsg = new MsgLobbyInfoUpdate();
        lobbyInfoMsg.RoomList = new List<Room>(UuidToRoom.Values);
        BoardCast(lobbyInfoMsg);
    }

    // Flip the player's ready state. Check if all players in the room are ready, if yes, then forward them to the game server.
    void HandleReadySignal(int connectionId)
    {
        Player player;
        if (!ConnIdToPlayer.TryGetValue(connectionId, out player))
        {
            Debug.LogError("Unkown connectionId=" + connectionId + " is sending Ready signal.");
            return;
        }

        ConnIdToPlayer[connectionId].IsReady = !ConnIdToPlayer[connectionId].IsReady;

        if (player.CurrRoom == null)
        {
            Debug.LogError("Player " + player.Name + " has no CurrRoom!");
            return;
        }

        MultiCastRoomUpdate(player.CurrRoom);

        bool allReady = true;
        foreach (Player p in player.CurrRoom.PlayerList)
        {
            allReady = allReady & p.IsReady;
        }

        if (allReady)
        {
            MsgGameStartSignal gameStartSignal = SearchGameServer();
            List<int> connList = new List<int>();
            foreach (Player p in player.CurrRoom.PlayerList)
            {
                connList.Add(p.ConnectionId);
            }
            MultiCast(gameStartSignal, connList.ToArray());
        }
    }

    // This helper function searchs the available game server, then
    // tells the server player information, then construct a new
    // MsgGameStartSignal containing the game server's ip and port.
    private MsgGameStartSignal SearchGameServer()
    {
        if (DebugLocalServers)
            return new MsgGameStartSignal("localhost", 7777);
        //to be changed, not implemented yet
        return new MsgGameStartSignal("52.183.4.114", 7777); 
    }

    // When player leaves the room, remove player from room's playerList, set
    // player's currRoom to null, then boardcast roomUpdate to all players in room,
    // After that, the server should also boardcast LobbyInfoUpdate to all player in the server (because
    // the numPlayer of that server has been updated).
    void HandleLeaveRoomSignal(int connectionId)
    {
        Player player;
        if (!ConnIdToPlayer.TryGetValue(connectionId, out player))
        {
            Debug.LogError("Unknown connectionId=" + connectionId.ToString() + "leave the room.");
            return;
        }
        Room room = player.CurrRoom;
        if (room == null)
        {
            Debug.LogError("Player " + player.Name + " tries to leave a room, but that player is not in any room!");
            return;
        }

        room.PlayerList.RemoveAll((p) => { return p.uuid.Equals(player.uuid); });
        room.NumPlayers = room.PlayerList.Count;
        player.CurrRoom = null;
        player.IsReady = false;

        Debug.Log("Player " + player.Name + " leaves " + room.RoomName + ".");
        MultiCastRoomUpdate(room);

        MsgLobbyInfoUpdate lobbyInfoMsg = new MsgLobbyInfoUpdate();
        lobbyInfoMsg.RoomList = new List<Room>(UuidToRoom.Values);
        BoardCast(lobbyInfoMsg);
    }

    // Multicast the roomUpdateMsg to all players in the room
    private void MultiCastRoomUpdate(Room room)
    {
        MsgRoomUpdate roomUpdateMsg = new MsgRoomUpdate();
        roomUpdateMsg.RoomName = room.RoomName;
        roomUpdateMsg.PlayerList = new List<Player>(room.PlayerList);
        List<int> connList = new List<int>();
        foreach (Player p in room.PlayerList)
        {
            connList.Add(p.ConnectionId);
        }
        MultiCast(roomUpdateMsg, connList.ToArray());
    }

#endregion

#region GameServerManagement
    // Invoked when a game server connects to the lobby.
    void OnGameServerConnected(int connectionId)
    {

    }

    // Invoked when a server receives a message from game server.
    void OnGameServerDataReceived(int connectionId, ArraySegment<byte> message)
    {

    }

    // Invoked when a game server shuts down or offline.
    void OnGameServerDisconnected(int connectionId)
    {

    }
#endregion

#region Client
    // Start connect to the lobby server.
    public void ClientConnectLobby()
    {
        Client.Connect(
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

    // Invoked when a client connects to the lobby server. Send player information immediately.
    void OnClientConnected()
    {
        MsgPlayerInfoUpdate msg = new MsgPlayerInfoUpdate(Config.MyName);
        Client.Send(msg.Serialize(), KcpChannel.Reliable);
    }

    // Invoked when a client receives a message from the lobby server.
    void OnClientDataReceived(ArraySegment<byte> message)
    {
        // Read the LobbyPacketHeader in the packet
        MemoryStream m = new MemoryStream(message.Array);
        m.Seek(1, SeekOrigin.Begin);        //skip 1 byte, because the first byte is Kcp header
        BinaryReader reader = new BinaryReader(m);
        LobbyPacketHeader header = (LobbyPacketHeader)reader.ReadByte();
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
        Debug.Log("Disconnected from server.");
        //clean up data
        CurrRoom = null;
        ConnIdToPlayer = new Dictionary<int, Player>();
        UuidToRoom = new Dictionary<string, Room>();
        Event_ClientDisconnected();
    }

    // Defines what to do when client receives MsgLobbyInfoUpdate packet.
    // Update the LobbyPanel.
    void HandleLobbyInfoUpdate(MsgLobbyInfoUpdate packet)
    {
        List<Room> roomList = new List<Room>(packet.RoomList);
        foreach (Room room in roomList)
        {
            UuidToRoom[room.uuid] = room;
        }
        Event_ReceiveLobbyInfoUpdate(roomList);
    }

    // Defines what to do when client receives MsgJoinRoomResponse packet.
    // If success, move to the room Panel
    // if failed, show message window.
    void HandleJoinRoomResponse(MsgJoinRoomResponse packet)
    {
        Event_ReceiveJoinRoomResponse.Invoke(packet.ErrorCode);
    }

    
    // Defines what to do when client receives MsgRoomUpdate packet.
    void HandleRoomUpdate(MsgRoomUpdate packet)
    {
        CurrRoom = new Room(roomName: packet.RoomName, 
                            numPlayers: packet.PlayerList.Count,
                            playerList: packet.PlayerList);

        Event_ReceiveRoomUpdate.Invoke(CurrRoom);
    }

    // Defines what to do when client receives GameStartSignal.
    // Move to the Game scene.
    void HandleGameStartSignal(MsgGameStartSignal packet)
    {
        Config.GameServerIP = packet.GameServerIP;
        Config.GameServerPort = packet.GameServerPort;

        SceneManager.LoadScene("PreTestScene");
    }

    // Send the joinRoom message to server.
    public void JoinRoom(string roomUUID)
    {
        MsgJoinRoomRequest msg = new MsgJoinRoomRequest(roomUUID);
        Client.Send(msg.Serialize(), KcpChannel.Reliable);
    }

    // Send the leaveRoom message to server.
    public void LeaveRoom()
    {
        CurrRoom = null;
        MsgLeaveRoomSignal msg = new MsgLeaveRoomSignal();
        Client.Send(msg.Serialize(), KcpChannel.Reliable);
    }

    // Close client socket.
    public void Disconnect()
    {
        Client.Disconnect();
    }

    // Send the player's ready state to the server.
    public void sendReadySignal()
    {
        MsgReadySignal msg = new MsgReadySignal();
        Client.Send(msg.Serialize(), KcpChannel.Reliable);
    }

    public enum LoginErrorCode { None, WrongPassword, Timeout, AccountNotFound};

    // Connect to authentication server to get session key. Note: This function currently cannot handle timeout properly.
    public async void authenticateAsync(string playerName, string password, Action<LoginErrorCode> callback)
    {
        string url = "https://csci599teamforcelogin.azurewebsites.net/api/HttpTrigger?code=JcSGfYNCmC9MvKuoiXdPaiujXsmqafrlzLcWApCdVZdzFwOoaNq6lw==";

        HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromMilliseconds(10000);
        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url+ "&accountId=" + playerName);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(req);
            string msg = await response.Content.ReadAsStringAsync();
            if (msg.Equals("Target not found!"))
            {
                callback.Invoke(LoginErrorCode.AccountNotFound);
                return;
            }
        }
        catch (TaskCanceledException e)
        {
            callback.Invoke(LoginErrorCode.Timeout);
            return;
        }

        byte[] responseByteArray = await response.Content.ReadAsByteArrayAsync();
        const int BLOCK_SIZE = 16;
        byte[] initVector = new byte[BLOCK_SIZE];
        Array.Copy(responseByteArray, 0, initVector, 0, BLOCK_SIZE);
        byte[] cipherText = new byte[responseByteArray.Length - BLOCK_SIZE];
        Array.Copy(responseByteArray, BLOCK_SIZE, cipherText, 0, responseByteArray.Length - BLOCK_SIZE);

        string responseText;
        using (Aes aes = Aes.Create())
        {
            SHA256 sha256 = SHA256.Create();
            byte[] hashedPassword = sha256.ComputeHash(Encoding.ASCII.GetBytes(password));
            byte[] clampedHashedPassword = new byte[32];
            Array.Copy(hashedPassword, 0, clampedHashedPassword, 0, 32);

            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;
            aes.IV = initVector;
            aes.KeySize = 256;

            ICryptoTransform decryptor = aes.CreateDecryptor(clampedHashedPassword, aes.IV);

            //Create the streams used for decryption
            var msDecrypt = new MemoryStream(cipherText);
            var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            var srDecrypt = new StreamReader(csDecrypt);
            try
            {
                responseText = srDecrypt.ReadToEnd();
                Debug.Log(responseText);
                string[] texts = responseText.Split(',');
                Config.SessionKey = texts[0];
                Config.MyName = texts[1];
                callback.Invoke(LoginErrorCode.None);
            }
            catch (CryptographicException e)
            {
                Debug.Log("Cannot decrypt. Incorrect password.");
                callback.Invoke(LoginErrorCode.WrongPassword);
            }
        }
    }
#endregion
}

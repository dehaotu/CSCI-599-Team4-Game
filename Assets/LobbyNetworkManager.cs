using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

using UnityEngine;
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
    ReadySignal
}
/************************************************************************************************************************
                                                        Player Management class
************************************************************************************************************************/
public class Player
{
    public string Name {set; get;}
    public bool isInRoom {set; get;}
    public bool isReady {set; get;}

    public Player(string Name) {
        this.Name = Name;
        this.isInRoom = false;
        this.isReady = false;
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

// All players need to send this update immediately when they connect to the Lobby server in order to 
// register their information such as their name.
public class MsgPlayerInfoUpdate
{
    public string Name {set; get;}

    public MsgPlayerInfoUpdate(string Name) { this.Name = Name; }

   public ArraySegment<byte> Serialize()
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
public class MsgLobbyInfoUpdate
{
    public List<Room> roomList;

    public ArraySegment<byte> Serialize()
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

public class MsgJoinRoomRequest
{
    //To be Changed: client should also send the room's uuid, but since we have only one room for now, so we don't need that
    public ArraySegment<byte> Serialize()
    {
        return LobbyMsgHelper.GenerateMsg((writer) => {
            writer.Write((byte)LobbyPacketHeader.JoinRoomRequest);
        });
    }
}

public class MsgJoinRoomResponse
{
    bool success;
    string failureMsg;  //if success is false, failureMsg will explain why client cannot join the room.

    public MsgJoinRoomResponse(bool success, string failureMsg = "") 
    {
        this.success = success;
        this.failureMsg = failureMsg;
    }

    public ArraySegment<byte> Serialize()
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

public class MsgRoomUpdate
{

}

public class MsgReadySignal
{
    bool isReady;

    public MsgReadySignal(bool isReady) {this.isReady = isReady;}

    public ArraySegment<byte> Serialize()
    {
        return LobbyMsgHelper.GenerateMsg((writer) => {
            writer.Write((byte)LobbyPacketHeader.ReadySignal);
            writer.Write(isReady);
        });
    }

    public static MsgReadySignal Desserialize(ArraySegment<byte> data) {
        return LobbyMsgHelper.InterpretMsg<MsgReadySignal>(data, (reader) => {
            return new MsgReadySignal(reader.ReadBoolean());
        });
   }
}
/************************************************************************************************************************
                                                    LobbyNetworkManager
************************************************************************************************************************/
public class LobbyNetworkManager : MonoBehaviour
{
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

    public MenuController menuController;
    KcpServer server;
    KcpClient client;

    Dictionary<int, Player> connIdToPlayer;
    List<Room> roomList;

    // Call this when the LobbyNetworkManager is being loaded into the scene
    void Awake()
    {
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
                HandleReadySignal(connectionId, MsgReadySignal.Desserialize(message));
                break;

        }
    }

    // Invoked when a client disconnected from the server
    void OnServerDisconnected(int connectionId)
    {
        Debug.Log("OnServerDisconnected: " + connectionId.ToString());
    }

    // This function defines what server should do when received a PlayerInfoUpdate package from player.
    // Once complete registering the new player, the lobby server should send the current lobby info to player.
    void HandlePlayerInfoUpdate(int connectionId, MsgPlayerInfoUpdate packet)
    {
        connIdToPlayer[connectionId] = new Player(packet.Name);
        Debug.Log(connIdToPlayer[connectionId].Name + " has connected to the Lobby Server.");

        MsgLobbyInfoUpdate msg = new MsgLobbyInfoUpdate();
        msg.roomList = new List<Room>(roomList);
        server.Send(connectionId, msg.Serialize(), KcpChannel.Reliable);
    }

    void HandleJoinRoomRequest(int connectionId, MsgJoinRoomRequest packet)
    {

    }

    void HandleReadySignal(int connectionId, MsgReadySignal packet)
    {

    }
/************************************************************************************************************************
                                                    LobbyNetworkManager(Client)
************************************************************************************************************************/
    // Invoked when a client connects to the lobby server. Send player information immediately.
    void OnClientConnected()
    {
        // When a player connects to the lobby server, immediately send a player info packet.
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
        Debug.Log("Debug: " + roomList[0].RoomName + " has " + roomList[0].numPlayers + " players.");
    }

    // Defines what to do when client receives MsgJoinRoomResponse packet.
    // If success, move to the room Panel
    // if failed, show message window.
    void HandleJoinRoomResponse(MsgJoinRoomResponse packet)
    {

    }

    void JoinRoom(String roomUUID)
    {
        MsgJoinRoomRequest msg = new MsgJoinRoomRequest();
        client.Send(msg.Serialize(), KcpChannel.Reliable);
    }

    void sendReadySignal(bool isReady)
    {

    }
}

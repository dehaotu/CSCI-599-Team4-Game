// using Mirror;
// using System;
// using UnityEngine;

// public class ChatPlayer : NetworkBehaviour
// {
//     [SyncVar]
//     public string playerName;

//     public static event Action<ChatPlayer, string> OnMessage;

//     [Command]
//     public void CmdSend(string message)
//     {
//         if (message.Trim() != "")
//             RpcReceive($"[{connectionToClient.connectionId}]: {message}".Trim());
//     }

//     [ClientRpc]
//     public void RpcReceive(string message)
//     {
//         OnMessage?.Invoke(this, message);
//     }
// }




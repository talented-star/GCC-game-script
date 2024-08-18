using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrabCoin.Services.Chat
{
    public class ChatNetwork : NetworkBehaviour
    {
        public event Action<string, bool> OnMessageReceived;
        public event Action<string, bool> OnReceiveWrites;
        private Func<List<NetworkIdentity>> _getRecivers;

        [Server]
        public void ServerSideConstructor(Func<List<NetworkIdentity>> getRecivers)
        {
            _getRecivers = getRecivers;
        }

        [Command]
        public void CmdSend(string message)
        {
            if (message == null)
                return;

            foreach (var opponentIdentity in _getRecivers())
            {
                RpcReceive(opponentIdentity.connectionToClient, message.Trim());
            }             
        }

        [Command]
        public void CmdWrites(string wipMessage)
        {
            if (wipMessage == null)
                return; 

            foreach (var opponentIdentity in _getRecivers())
            {
                RpcReceiveWrites(opponentIdentity.connectionToClient,  wipMessage.Trim());
            }
        }
 
        [TargetRpc]
        public void RpcReceive(NetworkConnection target, string message)
        {
            OnMessageReceived?.Invoke(message, isLocalPlayer);
        }

        [TargetRpc]
        public void RpcReceiveWrites(NetworkConnection target, string message) 
        {
            OnReceiveWrites?.Invoke(message, isLocalPlayer);
        } 
    }
}
using InventoryPlus;
using Mirror;
using System;
using System.IO;
using UnityEngine;

namespace GrabCoin.Services.Chat.VoiceChat
{
    public class VoiceNetwork : NetworkBehaviour
    {
        public event Action<float[]> ClipRecived;
        private AudioEncoding _audioEncoding;

        public void Constructor(AudioEncoding audioEncoding)
        {
            _audioEncoding = audioEncoding;
        }

        private void OnDestroy()
        {
            ClipRecived = null;
        }
        public void Record(float[] audioData)
        {
            byte[] byteData = _audioEncoding.Encode(audioData);
            CmdSendAudio(byteData);
        }

        [Command(channel = Channels.Unreliable)]
        private void CmdSendAudio(byte[] _data)
        {
            try
            {
                RpcReciveAudio(_data);
                float[] floatData = _audioEncoding.Decode(_data);
                ClipRecived?.Invoke(floatData);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }

        [ClientRpc(channel = Channels.Unreliable, includeOwner = false)]
        private void RpcReciveAudio(byte[] _data)
        {
            float[] floatData = _audioEncoding.Decode(_data);
            ClipRecived?.Invoke(floatData);
        }
    }
}

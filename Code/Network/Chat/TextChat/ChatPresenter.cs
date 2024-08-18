using GrabCoin.GameWorld.Player;
using GrabCoin.Services.Chat.View;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrabCoin.Services.Chat
{
    public class ChatPresenter : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private ChatNetwork chatNetwork;
        [SerializeField] private DrawerSendedMessageByCharacterView _drawerSendedMessageByCharacterView;

        private const float MaxReciverDistance = 10f;
        private ChatPlayerData _playerMessage;
        private ChatMessageData _chatMessage;
        private static List<ChatPresenter> _chatPresenters;

        private CustomSignal _onChatSignal;
        private CustomEvent _onChatEvent;

        #region "Unity methods"
        private void Awake()
        {
            _playerMessage = new();
            _chatMessage = new();
            if (_chatPresenters == null)
                _chatPresenters = new();
        }
 
        private void OnEnable()
        {
            _onChatSignal = OnChatSignal;
            Translator.Add<UIPlayerProtocol>(_onChatSignal);
            _onChatEvent = OnChatEvent;
            Translator.Add<ChatProtocol>(_onChatEvent);

            //note in instantiated object inject not working if you not used zinject fabric
            chatNetwork.OnMessageReceived += OnMessageReceived;
            chatNetwork.OnReceiveWrites += OnReceiveWrites;
        }

        private void OnDisable()
        {
            if (chatNetwork.isServer && _chatPresenters.Contains(this))
                _chatPresenters.Remove(this);
            Translator.Remove<UIPlayerProtocol>(_onChatSignal);
            Translator.Remove<ChatProtocol>(_onChatEvent);
        }

        private void Start()
        {
            if (chatNetwork.isServer)
                chatNetwork.ServerSideConstructor(() => GetReciversInTheRadius(MaxReciverDistance));
        }
        #endregion "Unity methods"

        #region "General methods"
        public void Initialize(Player player, ChatNetwork chatNetwork)
        {
            this.chatNetwork = chatNetwork;
            this.player = player;
            _drawerSendedMessageByCharacterView.InitPlayer(chatNetwork.netId);

            if (chatNetwork.isServer && !_chatPresenters.Contains(this))
                _chatPresenters.Add(this);
        }

        public void OnChatEvent(System.Enum code, ISendData data)
        {
            switch (code)
            {
                case ChatProtocol.StateChatWindow:
                    if (player.isLocalPlayer)
                        Translator.Send(UIPlayerProtocol.StateChat, data);
                    break;
                case ChatProtocol.MessageSended:
                    {
                        var message = (StringData)data;
                        OnMessageSended(message.value);
                    }
                    break;
                case ChatProtocol.MessageWrites:
                    {
                        var message = (StringData)data;
                        OnMessageWrites(message.value);
                    }
                    break;
            }
        }

        public void OnChatSignal(System.Enum code)
        {
            switch (code)
            {
                case UIPlayerProtocol.OpenChat:
                    OpenChat();
                    break;
            }
        }
        #endregion "General methods"

        #region "Server methods"
        [Server]
        private List<NetworkIdentity> GetReciversInTheRadius(float radius)
        {
            return _chatPresenters.Where(reciver => Vector3.Distance(reciver.GetPosition(), GetPosition()) < radius).
                Select(reciver => reciver.player.netIdentity).ToList();
        }

        [Server]
        private Vector3 GetPosition()
        {
            return player.GetPosition();
        }
        #endregion "Server methods"

        #region "Client methods"
        
        private void OnMessageWrites(string message)
        {
            chatNetwork.CmdWrites(message);
        }

        
        private void OnReceiveWrites(string message, bool isLocalPlayer)
        {
            if (!isLocalPlayer)
            {
                _playerMessage.playerNetId = chatNetwork.netId;
                _playerMessage.message = message;
                Translator.Send(ChatProtocol.ShowMessageBilboard, _playerMessage);
            } 
        }

        
        private void OnMessageReceived(string message, bool isLocalPlayer)
        {
            if (!isLocalPlayer)
            {
                _playerMessage.playerNetId = chatNetwork.netId;
                _playerMessage.message = message;
                Translator.Send(ChatProtocol.ShowMessageBilboard, _playerMessage);
            }
            _chatMessage.playerName = player.playerName;
            _chatMessage.message = message;
            _chatMessage.isLocal = isLocalPlayer;
            Translator.Send(ChatProtocol.MessageReceived, _chatMessage);
        }

        
        private void OnMessageSended(string message)
        {
             chatNetwork.CmdSend(message);
        }

        
        private void OpenChat()
        {
            if (chatNetwork.isLocalPlayer)
                Translator.Send(ChatProtocol.OpenChat);
        }
        #endregion "Client methods"
    }
}
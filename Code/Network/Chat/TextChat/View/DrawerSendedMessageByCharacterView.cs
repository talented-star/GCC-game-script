using System.Collections;
using UnityEngine;

namespace GrabCoin.Services.Chat.View
{
    public class DrawerSendedMessageByCharacterView : MonoBehaviour
    {
        [SerializeField] private bool showMessages;
        [SerializeField] private TMPro.TextMeshProUGUI chatMessage;
        [SerializeField] private Bilboard _bilboard;
        [SerializeField] private float lastMessageStorageTime = 3f;

        private uint _playerId;
        private CustomEvent _onMessageEvent;

        private void OnDestroy()
        {
            Translator.Remove<ChatProtocol>(_onMessageEvent);
        }

        public void InitPlayer(uint playerId)
        {
            _onMessageEvent = OnMessageEvent;
            Translator.Add<ChatProtocol>(_onMessageEvent);
            _playerId = playerId;
        }

        private IEnumerator chatRoutine;
        public void ShowMessageBilboard(string message)
        {
            if (!showMessages)
                return;

            if (chatRoutine != null)
                StopCoroutine(chatRoutine);
 
            chatRoutine = ShowMessage();

            chatMessage.text = message;

            StartCoroutine(chatRoutine);
        }

        private IEnumerator ShowMessage()
        {
            Color currentMessageColor = Color.white;

            chatMessage.color = currentMessageColor;

            yield return new WaitForSeconds(lastMessageStorageTime);

            const float TimeStep = 0.1f;

            while (chatMessage.color.a >= 0)
            {
                currentMessageColor = new Color(currentMessageColor.r, currentMessageColor.g, currentMessageColor.b, currentMessageColor.a - TimeStep);
                chatMessage.color = currentMessageColor;
                yield return new WaitForSeconds(TimeStep);
            }

            chatMessage.text = string.Empty;

            yield break;
        }

        private void OnMessageEvent(System.Enum code, ISendData data)
        {
            switch (code)
            {
                case ChatProtocol.ShowMessageBilboard:
                    var message = (ChatPlayerData)data;

                    if (message.playerNetId == _playerId)
                        ShowMessageBilboard(message.message);
                    break;
            }
        }
    }
}
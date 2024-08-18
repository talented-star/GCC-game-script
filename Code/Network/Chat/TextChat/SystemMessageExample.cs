using GrabCoin.Services.Chat.View;
using UnityEngine;

namespace GrabCoin.Services.Chat
{
    public class SystemMessageExample : MonoBehaviour
    {
        private const int NotificationMsgType = 1;
        private const int WarningMsgType = -1;
        private const int ProcessMsgType = 0;
        private const float ProcessMsgDuration = 10f;

        [SerializeField]
        private ChatWindow chat = null;

        public void AddNotification(string text)
        {
            chat.AddSystemMessage(text, NotificationMsgType);
        }

        public void AddWarning(string text)
        {
            chat.AddSystemMessage(text, WarningMsgType);
        }

        public void AddProcess(string text)
        {
            chat.AddSystemMessage(text, ProcessMsgType, ProcessMsgDuration);
        }
    }
}

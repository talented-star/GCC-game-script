using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using GrabCoin.UI.ScreenManager;
using UnityEngine.EventSystems;
using GrabCoin.GameWorld.Player;
using InventoryPlus;
using GrabCoin.UI;

namespace GrabCoin.Services.Chat.View
{
    public class ChatWindow : UIScreenBase
    {
        private const string CloseChatAnimName = "CloseChat";
        private const string OpenChatAnimName = "OpenChat";
        [SerializeField, Tooltip("Is chat open when the scene starts")]
        private bool chatIsOpen = false;

        [SerializeField, Tooltip("Write messages to a file")]
        private bool logging = false;

        [SerializeField]
        private RectTransform chatPrompt;

        [Header("UI")]
        [SerializeField]
        private TMP_InputField inputFieldComp = null;
        [SerializeField]
        private Transform content = null;
        [SerializeField]
        private Scrollbar scrollbar = null;
        [SerializeField]
        private Animation _animation = null;

        [SerializeField]
        private TypesOfMessages messageTypes = new TypesOfMessages();
        private const string systemSender = "Sys";
        private BoolData _protocolData;
        private StringData _messageData;

        private CustomSignal _onStateSignal;
        private CustomEvent _onMessageEvent;

        public bool ChatIsOpen
        {
            get 
            {
                return chatIsOpen;
            }
        }

        private static ChatWindow _instance;
        public static ChatWindow Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindAnyObjectByType<ChatWindow>();
                return _instance;
            }
        }


        private void Start()
        {
            _protocolData = new();
            _messageData = new();

            if (chatIsOpen)
                OpenChat();
            else
            {
                _animation.Play(CloseChatAnimName);
                chatIsOpen = false;
                DisableInputField();
            }
        }

        private void OnEnable()
        {
            inputFieldComp.onValueChanged.AddListener(OnChange);
            inputFieldComp.onDeselect.AddListener(OnDeselected);

            _onStateSignal = OnStateSignal;
            Translator.Add<ChatProtocol>(_onStateSignal);
            _onMessageEvent = OnMessageEvent;
            Translator.Add<ChatProtocol>(_onMessageEvent);
        }

        public override void CheckOnEnable()
        {

        }

        private void OnDisable()
        {
            inputFieldComp.onValueChanged.RemoveListener(OnChange);
            inputFieldComp.onDeselect.RemoveListener(OnDeselected);

            Translator.Remove<ChatProtocol>(_onStateSignal);
            Translator.Remove<ChatProtocol>(_onMessageEvent);
        }

        public override void CheckInputHandler(Controls controls)
        {
            base.CheckInputHandler(controls);
            if (controls.Player.CallMenu.WasPressedThisFrame())
                if (inputFieldComp.text.Trim() == "")
                    CloseChat();
        }

        private void OnDeselected(string arg0)
        {
            CloseChat();
        }

        private void OnChange(string message)
        {
            _messageData.value = message;
            Translator.Send(ChatProtocol.MessageWrites, _messageData);
        }

        public void OpenChat()
        {
            _animation.Play(OpenChatAnimName);
            chatIsOpen = true;
            //chatPrompt.gameObject.SetActive(false);
            EnableInputField();
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }

        public void CloseChat()
        {
            if (chatIsOpen)
            {
                _animation.Play(CloseChatAnimName);
                chatIsOpen = false;
                //chatPrompt.gameObject.SetActive(true);
                DisableInputField();
                Translator.Send(UIPlayerProtocol.StateChat, new BoolData { value = false });
                Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = false });
            }
        }

        public void AddSystemMessage(string text, int effect, float duration = 0f)
        {
            if (effect < 0)
            {
                AppendMessage(text, messageTypes.Warning, systemSender);
            }
            else if (effect > 0)
            {
                AppendMessage(text, messageTypes.Notification, systemSender);
            }
            else
            {
                AppendMessage(text, messageTypes.Process, systemSender, duration);
            }
        }

        public void OnSendMessage()
        {
            if (inputFieldComp.text.Trim() != "")
            {
                _messageData.value = inputFieldComp.text.Trim();
                Translator.Send(ChatProtocol.MessageSended, _messageData);
            }
            inputFieldComp.SetTextWithoutNotify(string.Empty);

            EnableInputField();
        }

        private void OnRecivedMessage(string playerName, string message, bool fromCurrentCharacter)
        {
 
            if (fromCurrentCharacter)
                AppendMessage(message, messageTypes.Message, playerName);
            else
                AppendMessage(message, messageTypes.OpponentMessage, playerName);
        }

        private void OnStateSignal(System.Enum code)
        {
            switch (code)
            {
                case ChatProtocol.OpenChat:
                    if (chatIsOpen)
                    {
                        if (inputFieldComp.text.Trim() == "")
                            CloseChat();
                        OnSendMessage();
                    }
                    else
                        OpenChat();
                    break;
            }
            _protocolData.value = chatIsOpen;
            Translator.Send(ChatProtocol.StateChatWindow, _protocolData);
        }

        private void OnMessageEvent(System.Enum code, ISendData data)
        {
            switch (code)
            {
                case ChatProtocol.MessageReceived:
                    var receivedMessage = (ChatMessageData)data;
                    OnRecivedMessage(receivedMessage.playerName, receivedMessage.message, receivedMessage.isLocal);
                    break;
            }
        }

        private void EnableInputField()
        {
            if (EventSystem.current?.currentSelectedGameObject != inputFieldComp.gameObject)
                EventSystemsController.Instance.GetCurrentEventSystem().GetComponent<EventSystem>().SetSelectedGameObject(inputFieldComp.gameObject);
            inputFieldComp.readOnly = false;
            inputFieldComp.ActivateInputField();
            inputFieldComp.Select();
            if (EventSystem.current?.currentSelectedGameObject != inputFieldComp.gameObject)
                EventSystem.current?.SetSelectedGameObject(inputFieldComp.gameObject);
        }

        private void DisableInputField()
        {
            inputFieldComp.readOnly = true;
            inputFieldComp.DeactivateInputField();
        }

        internal void AppendMessage(string message, MessageType type, string sender, float duration = 0f)
        {
            StartCoroutine(AppendAndScroll(message, type, sender, duration));
        }

        private IEnumerator AppendAndScroll(string message, MessageType type, string sender, float duration = 0f)
        {
            if (type == messageTypes.Process)
                CreateProcess(message, duration);
            else
                CreateMessage(message, type, sender);

            // It takes 2 frames for the UI to update?!?!
            yield return null;
            yield return null;

            scrollbar.value = 0;
        }

        private void CreateMessage(string text, MessageType type, string sender)
        {
            Message message = new Message
            {
                Text = text,
                Time = DateTime.UtcNow.ToString("HH:mm"),
                Type = type
            };

            var obj = Instantiate(message.Type.Prefab, content);
            var messageComp = obj.GetComponent<MessageComponent>();
            messageComp.Init(message, sender);

            if (logging)
                SaveMessage(message);
        }

        private void CreateProcess(string text, float duration)
        {
            Process process = new Process
            {
                Text = text,
                Duration = duration,
                Time = DateTime.UtcNow.ToString("HH:mm"),
                CompletionTime = FormattedTime(0 + duration),
                Type = messageTypes.Process
            };

            var obj = Instantiate(process.Type.Prefab, content);
            var processComp = obj.GetComponent<ProcessComponent>();
            processComp.Init(process);

            if (logging)
                SaveProcess(process);
        }

        private string FormattedTime(float timer)
        {
            string minutes = Mathf.Floor(timer / 60).ToString("00");
            string seconds = Mathf.Floor(timer % 60).ToString("00");
            string time = string.Format("{0}:{1}", minutes, seconds);

            return time;
        }

        #region Save to file

        private static string fileName = string.Format("{0}_{1}_{2}_{3}_{4}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year, DateTime.Now.Hour, DateTime.Now.Minute) + ".txt";

        private void SaveMessage(Message message)
        {
            string line = message.Time + " - " + message.Text;

            StreamWriter sw = new StreamWriter(Application.dataPath + "/Logs/" + fileName);
            sw.WriteLine(line);
            sw.Close();
        }

        private void SaveProcess(Process process)
        {
            string line = process.CompletionTime + " - " + process.Text;

            StreamWriter sw = new StreamWriter(Application.dataPath + "/Logs/" + fileName);
            sw.WriteLine(line);
            sw.Close();
        }

        internal bool IsOpened()
        {
            return chatIsOpen;
        }

        #endregion
    }
}


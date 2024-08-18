using GrabCoin.UI.ScreenManager;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Popups/InfoYesNoPopup.prefab")]
    public class InfoYesNoPopup : UIScreenBase
    {
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;
        [SerializeField] private LocObject _infoText;

        private PlayerScreensManager _screensManager;

        public Action onNoClick;
        public Action onYesClick;

        [Inject]
        private void Construct(
            PlayerScreensManager screensManager
            )
        {
            _screensManager = screensManager;
        }

        private void Awake()
        {
            _yesButton.onClick.AddListener(CloseYesScreen);
            _noButton.onClick.AddListener(CloseNoScreen);
        }

        public override void CheckOnEnable()
        {

        }

        public override void CheckInputHandler(Controls controls)
        {
            base.CheckInputHandler(controls);
            if (controls.Player.CallMenu.WasPressedThisFrame())
            {
                CloseNoScreen();
            }
            if (controls.Player.OpenChat.WasPressedThisFrame())
            {
                CloseYesScreen();
            }
        }

        public void ProcessKey(string keyInfoText)
        {
            _infoText.SetNewKey(keyInfoText);
        }

        public void Process(string infoText)
        {
            _infoText.SetNewText(infoText);
        }

        private void CloseNoScreen()
        {
            onNoClick?.Invoke();
            CloseScreen();
        }

        private void CloseYesScreen()
        {
            onYesClick?.Invoke();
            CloseScreen();
        }

        private void CloseScreen()
        {
            _screensManager.ClosePopup();
            onNoClick = null;
            onYesClick = null;
        }
    }
}

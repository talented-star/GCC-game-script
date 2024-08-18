using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Popups/InfoPopup.prefab")]
    public class InfoPopup : UIScreenBase
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private LocObject _infoText;

        private PlayerScreensManager _screensManager;

        public Action onClose;

        [Inject]
        private void Construct(
            PlayerScreensManager screensManager
            )
        {
            _screensManager = screensManager;
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(CloseScreen);
        }

        public override void CheckOnEnable()
        {

        }

        public override void CheckInputHandler(Controls controls)
        {
            base.CheckInputHandler(controls);
            if (controls.Player.CallMenu.WasPressedThisFrame())
            {
                CloseScreen();
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

        private void CloseScreen()
        {
            _screensManager.ClosePopup();
            onClose?.Invoke();
            onClose = null;
        }
    }
}

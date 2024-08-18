using GrabCoin.UI.HUD;
using GrabCoin.UI.ScreenManager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/InfoScreen.prefab")]
    public class InfoScreen : UIScreenBase
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private LocObject _headerText;
        [SerializeField] private LocObject _infoText;
        [SerializeField] private LocObject _infoImage;

        private PlayerScreensManager _screensManager;

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

        public override void CheckInputHandler(Controls controls)
        {
            base.CheckInputHandler(controls);
            if (controls.Player.CallMenu.WasPressedThisFrame())
            {
                CloseScreen();
            }
        }

        private void OnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }

        public override void CheckOnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }

        public void Process(string keyHeaderText, string keyInfoText, string keyInfoImage)
        {
            _headerText.SetNewKey(keyHeaderText);
            _infoText.SetNewKey(keyInfoText);
            _infoImage.SetNewKey(keyInfoImage);
        }

        private async void CloseScreen()
        {
            await _screensManager.OpenScreen<GameHud>();
        }
    }
}

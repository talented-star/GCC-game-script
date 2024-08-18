using GrabCoin.UI.HUD;
using GrabCoin.UI.ScreenManager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/GeneralInfoScreen.prefab")]
    public class GeneralInfoScreen : UIScreenBase
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private LocObject _headerText;
        [SerializeField] private LocObject _infoInputLText;
        [SerializeField] private LocObject _infoInputRText;
        [SerializeField] private LocObject _infoBox1Text;
        [SerializeField] private LocObject _infoBox2Text;
        [SerializeField] private LocObject _infoBox3Text;
        [SerializeField] private LocObject _infoBox4Text;
        [SerializeField] private LocObject _infoBox5Text;
        [SerializeField] private LocObject _infoBox6Text;
        [SerializeField] private LocObject _infoBox7Text;
        [SerializeField] private LocObject _infoBox8Text;
        [SerializeField] private LocObject _infoBoxHeader4Text;
        [SerializeField] private LocObject _infoBoxHeader5Text;
        [SerializeField] private LocObject _infoBoxHeader6Text;
        [SerializeField] private LocObject _infoBoxHeader7Text;
        [SerializeField] private LocObject _infoBoxHeader8Text;

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
            Process();
        }

        public override void CheckOnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }

        public void Process(string keyInfoText = "General info")
        {
            _headerText.SetNewKey(keyInfoText + "Header");
            _infoInputLText.SetNewKey(keyInfoText + "L");
            _infoInputRText.SetNewKey(keyInfoText + "R");
            _infoBox1Text.SetNewKey(keyInfoText + 1);
            _infoBox2Text.SetNewKey(keyInfoText + 2);
            _infoBox3Text.SetNewKey(keyInfoText + 3);
            _infoBox4Text.SetNewKey(keyInfoText + 4);
            _infoBox5Text.SetNewKey(keyInfoText + 5);
            _infoBox6Text.SetNewKey(keyInfoText + 6);
            _infoBox7Text.SetNewKey(keyInfoText + 7);
            _infoBox8Text.SetNewKey(keyInfoText + 8);
            _infoBoxHeader4Text.SetNewKey(keyInfoText + "H4");
            _infoBoxHeader5Text.SetNewKey(keyInfoText + "H5");
            _infoBoxHeader6Text.SetNewKey(keyInfoText + "H6");
            _infoBoxHeader7Text.SetNewKey(keyInfoText + "H7");
            _infoBoxHeader8Text.SetNewKey(keyInfoText + "H8");
        }

        private async void CloseScreen()
        {
            await _screensManager.OpenScreen<GameHud>();
        }
    }
}

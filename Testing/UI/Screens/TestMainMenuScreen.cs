// using Codice.Client.Common;
using GrabCoin.UI.ScreenManager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.Testing.UI
{
    [UIScreen("UITest/TestMainMenuScreen.prefab", true)]
    public class TestMainMenuScreen : UIScreenBase
    {
        [Inject] protected UIScreensManager _screensManager;
        [Inject] protected UIPopupsManager _popupsManager;


        [SerializeField] public Button _buttonOpenScreenRed;
        [SerializeField] public Button _buttonOpenScreenBlue;
        [SerializeField] public Button _buttonOpenPopup;

        private void Awake()
        {
        }

        private void Start()
        {
            _buttonOpenScreenRed?.onClick.AddListener(OpenRed);
            _buttonOpenScreenBlue?.onClick.AddListener(OpenBlue);
            _buttonOpenPopup?.onClick.AddListener(OpenPopup);
        }

        public override void CheckOnEnable()
        {

        }

        private async void OpenRed()
        {
            _ = await _screensManager.Open<TestScreenRed>(true);
        }

        private async void OpenBlue()
        {
            _ = await _screensManager.Open<TestScreenBlue>(true);
        }

        private async void OpenPopup()
        {
            _ = await _popupsManager.Open<TestPopup>();
        }

    }
}

// using Codice.Client.Common;
using GrabCoin.UI.ScreenManager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.Testing.UI
{
    [UIScreen("UITest/TestScreenRed.prefab", lazyLoad: true)]
    public class TestScreenRed : UIScreenBase
    {
        [Inject] protected UIScreensManager _sreensManager;
        [SerializeField] private Button _buttonClose;
        [SerializeField] private Button _buttonOpenBlue;

        private void Start()
        {
            _buttonClose?.onClick.AddListener(CLoseClicked);
            _buttonOpenBlue?.onClick.AddListener(OpenTestWindow);
        }

        public override void CheckOnEnable()
        {

        }

        private void CLoseClicked()
        {
            Release();
        }

        private async void OpenTestWindow()
        {
            _ = await _sreensManager.Open<TestScreenBlue>(false);
            Release();
        }
    }
}

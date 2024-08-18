// using Codice.Client.Common;
using GrabCoin.UI.ScreenManager;
using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.Testing.UI
{
    [UIScreen("UITest/TestScreenPopup", lazyLoad: true)]
    public class TestPopup : UIScreenBase
    {
        [SerializeField] private Button _buttonClose;
        [SerializeField] private Button _buttonRelease;

        private void Start()
        {
            _buttonClose?.onClick.AddListener(Close);
            _buttonRelease?.onClick.AddListener(Release);
        }

        public override void CheckOnEnable()
        {

        }
    }
}

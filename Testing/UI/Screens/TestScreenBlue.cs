// using Codice.Client.Common;
using GrabCoin.UI.ScreenManager;
using UnityEngine;
using UnityEngine.UI;

namespace GrabCoin.Testing.UI
{
    [UIScreen("UITest/TestScreenBlue.prefab", lazyLoad: true)]
    public class TestScreenBlue : UIScreenBase
    {
        [SerializeField] private Button _buttonClose;

        private void Start()
        {
            _buttonClose?.onClick.AddListener(Close);
        }

        public override void CheckOnEnable()
        {
        }
    }
}

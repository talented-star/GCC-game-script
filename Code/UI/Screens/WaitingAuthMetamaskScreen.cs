using Code.Services.AuthService;
using Cysharp.Threading.Tasks;
using GrabCoin.UI.HUD;
using GrabCoin.UI.ScreenManager;
using Jint.Runtime;
using PlayFab;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Popups/WaitingAuthMetamaskScreen.prefab")]
    public class WaitingAuthMetamaskScreen : UIScreenBase
    {
        [SerializeField] private Button _authButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _textField;
        [SerializeField] private string _textWait;
        [SerializeField] private string _textReady;
        [SerializeField] private float _waitTime;

        private UniTaskCompletionSource<bool> _completion;
        private MetamaskAuthService _metamaskAuthService;
        private PlayerScreensManager _screensManager;
        private float _timer;
        private bool _isTiming;
        private TMP_Text _statusAuthText;

        [Inject]
        private void Construct(
            MetamaskAuthService authService,
            PlayerScreensManager screensManager
            )
        {
            _metamaskAuthService = authService;
            _screensManager = screensManager;
        }

        private void Awake()
        {
            _authButton.onClick.AddListener(Auth);
            _backButton.onClick.AddListener(CloseScreen);
        }

        private void Update()
        {
            if (_timer <= _waitTime)
            {
                _timer += Time.deltaTime;
                _textField.text = _textWait + $"{(int)(_waitTime - _timer)}";
            }
            else if (_isTiming)
            {
                EndTimer();
            }
            if (_statusAuthText != null)
                _statusAuthText.text = ProjectNetworkContext.Instance.AuthtorizeState.ToString();
        }

        private void OnEnable()
        {
            if (_isTiming) return;
            Auth();
        }

        public override void CheckOnEnable()
        {

        }

        public UniTask<bool> Process(TMP_Text statusAuthText)
        {
            _statusAuthText = statusAuthText;
            _statusAuthText.text = "";
            _completion = new UniTaskCompletionSource<bool>();
            return _completion.Task;
        }

        private async void Auth()
        {
            _timer = 0f;
            _isTiming = true;
            _authButton.interactable = false;

            bool result = await _metamaskAuthService.SignIn();

            _metamaskAuthService.StopWaitAuth();
            _screensManager.ClosePopup();
            EndTimer();
            _completion.TrySetResult(result);

        }

        private void CloseScreen()
        {
            _metamaskAuthService.StopWaitAuth();
            _screensManager.ClosePopup();
            EndTimer();
            _completion.TrySetResult(false);
        }

        private void EndTimer()
        {
            _isTiming = false;
            _textField.text = _textReady;
            _authButton.interactable = true;
        }
    }
}

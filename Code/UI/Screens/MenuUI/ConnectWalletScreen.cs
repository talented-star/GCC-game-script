using Code.Services.AuthService;
using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using PlayFab;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Popups/ConnectWalletScreen.prefab")]
    public class ConnectWalletScreen : UIScreenBase
    {
        [SerializeField] private Button _authButton;
        [SerializeField] private Button _exitButton;

        private CanvasGroup _canvasGroup;
        private MetamaskAuthService _metamaskAuthService;
        private PlayerScreensManager _screensManager;
        private UniTaskCompletionSource<bool> _completion;
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
            _canvasGroup = GetComponent<CanvasGroup>();

            _authButton.onClick.AddListener(Auth);
            _exitButton.onClick.AddListener(CloseAppClicked);
        }

        private void OnEnable()
        {
            _canvasGroup.interactable = true;
        }

        public override void CheckOnEnable()
        {

        }

        public UniTask<bool> Process(TMP_Text statusMetamaskText)
        {
            _statusAuthText = statusMetamaskText;
            _completion = new UniTaskCompletionSource<bool>();
            return _completion.Task;
        }

        private async void Auth()
        {
            _canvasGroup.interactable = false;

            _screensManager.ClosePopup();
            var screen = await _screensManager.OpenPopup<WaitingAuthMetamaskScreen>();
            var result = await screen.Process(_statusAuthText);
            _canvasGroup.interactable = result;
            if (!result)
                PlayFabSettings.staticPlayer.ForgetAllCredentials();

            _completion.TrySetResult(result);
        }

        private void CloseAppClicked()
        {
            _screensManager.ClosePopup();
            _completion.TrySetResult(false);
        }

    }
}

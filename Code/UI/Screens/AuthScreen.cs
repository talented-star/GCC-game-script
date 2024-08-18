using Code.Services.AuthService;
using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/AuthScreen.prefab")]
    public class AuthScreen : UIScreenBase
    {
        [SerializeField] private Button _authButton;
        [SerializeField] private Button _exitButton;
        private MetamaskAuthService _metamaskAuthService;
        private UniTaskCompletionSource<bool> _completion;
    
        [Inject]
        private void Construct(MetamaskAuthService authService)
        {
            _metamaskAuthService = authService;
        }

        private void Awake()
        {
            _authButton.onClick.AddListener(Auth);
            _exitButton.onClick.AddListener(CloseAppClicked);
        }

        public override void CheckOnEnable()
        {

        }

        public UniTask<bool> Process()
        {
            _completion = new UniTaskCompletionSource<bool>();
            return _completion.Task;
        }

        private async void Auth()
        {
            var result = await _metamaskAuthService.SignIn();
            _completion.TrySetResult(result);
        }

        private void CloseAppClicked()
        {
            //TODO добавить попап для подтверждения закрытия прилоги
            Application.Quit();
        }

    }
}

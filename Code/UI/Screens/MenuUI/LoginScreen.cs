using Code.Services.AuthService;
using Cysharp.Threading.Tasks;
using GrabCoin.UI.HUD;
using GrabCoin.UI.ScreenManager;
using PlayFab;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/LoginScreen.prefab")]
    public class LoginScreen : UIScreenBase
    {
        public enum LoginedSave
        {
            None,
            Email,
            Metamask
        }

        private enum OnlineState
        {
            Wait,
            Online,
            Offline
        }

        public const string AUTHENTIFICATION_EMAIL_KEY = "AUTHENTIFICATION_EMAIL_KEY";
        public const string AUTHENTIFICATION_PASSWORD_KEY = "AUTHENTIFICATION_PASSWORD_KEY";
        public const string AUTHENTIFICATION_STATE_KEY = "AUTHENTIFICATION_STATE_KEY";

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _loginButton;
        [SerializeField] private Button _registerButton;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _connectWalletButton;
        [SerializeField] private Button _forgotPasswordButton;
        [SerializeField] private TMP_Text _errorText;
        //[SerializeField] private TMP_InputField _nameField;
        [SerializeField] private TMP_InputField _emailField;
        [SerializeField] private TMP_InputField _passwordField;
        [SerializeField] private TMP_Text _statusMetamaskText;
        [SerializeField] private Toggle _isSaveValue;

        private OnlineState _onlineState;

        private UniTaskCompletionSource<bool> _completion;
        private EmailAuthService _emailAuthService;
        private CustomIdAuthService _customIdAuthService;
        private PlayerScreensManager _screensManager;

        [Inject]
        private void Construct(
            EmailAuthService emailAuthService,
            CustomIdAuthService customIdAuthService,
            PlayerScreensManager screensManager
            )
        {
            _emailAuthService = emailAuthService;
            _customIdAuthService = customIdAuthService;
            _screensManager = screensManager;
        }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            _loginButton.onClick.AddListener(LogIn);
            _registerButton.onClick.AddListener(Register);
            _playButton.onClick.AddListener(PlayForse);
            _connectWalletButton.onClick.AddListener(ConnectWallet);
            _forgotPasswordButton.onClick.AddListener(ForgotPassword);
            _exitButton.onClick.AddListener(ExitGame);
        }

        private async void OnEnable()
        {
            _canvasGroup.interactable = true;
            _emailField.text = "";
            _passwordField.text = "";
            _errorText.text = "";


            var currentLoginState = (LoginedSave)PlayerPrefs.GetInt(AUTHENTIFICATION_STATE_KEY, 0);
            _playButton.gameObject.SetActive(currentLoginState == LoginedSave.None);

            await UniTask.WaitUntil(() => _completion != null);
            switch (currentLoginState)
            {
                case LoginedSave.Email:
                    if (!_canvasGroup.interactable) return;
                    PlayFabSettings.staticPlayer.ForgetAllCredentials();
                    _emailField.text = PlayerPrefs.GetString(AUTHENTIFICATION_EMAIL_KEY, "");
                    _passwordField.text = PlayerPrefs.GetString(AUTHENTIFICATION_PASSWORD_KEY, "");
                    LogIn();
                    break;
            }
        }

        public override void CheckOnEnable()
        {

        }

        public UniTask<bool> Process()
        {
            _completion = new UniTaskCompletionSource<bool>();
            return _completion.Task;
        }

        #region "Button Actions"
        private async void LogIn()
        {
            _canvasGroup.interactable = false;
            int hash = _emailField.text.GetHashCode();
            if (hash < 0)
                hash *= -1;
            _emailAuthService.FillingData(hash.ToString(), _emailField.text, _passwordField.text);
            var result = await _emailAuthService.SignIn();
            if (result)
            {
                CheckLogin();
                await UniTask.WaitUntil(() => _onlineState != OnlineState.Wait);
                await UniTask.WaitUntil(() => SceneNetworkContext.Instance != null);

                if (_onlineState == OnlineState.Online) return;
                SceneNetworkContext.Instance.UpdateUserPublisherData("isOnline", "1", result => { });
                SceneNetworkContext.Instance.CallGetStatistics();
                SaveValue();
                Close();
                var screen = await _screensManager.OpenScreen<MainMenuScreen>(); // Open MyAccountScreen
                screen.Process(_completion);
            }
            else
                ResetLogIn();
        }

        private void ResetLogIn()
        {
            _onlineState = OnlineState.Online;
            _canvasGroup.interactable = true;
            _errorText.text = _emailAuthService.message;
            PlayFabSettings.staticPlayer.ForgetAllCredentials();
            PlayerPrefs.DeleteKey(AUTHENTIFICATION_EMAIL_KEY);
            PlayerPrefs.DeleteKey(AUTHENTIFICATION_PASSWORD_KEY);
            PlayerPrefs.DeleteKey(AUTHENTIFICATION_STATE_KEY);
            _emailField.text = "";
            _passwordField.text = "";
        }

        private async void CheckLogin()
        {
            _onlineState = OnlineState.Wait;

            _onlineState = OnlineState.Offline;
            return;
            await UniTask.WaitUntil(() => SceneNetworkContext.Instance != null);
            SceneNetworkContext.Instance.GetUserPublisherData("isOnline", result =>
            {
                if (result.isInit)
                {
                    var data = result.Value.Data;
                    if (data.ContainsKey("isOnline"))
                    {
                        if (data["isOnline"].Value == "0")
                            _onlineState = OnlineState.Offline;
                        else
                            ResetLogIn();
                    }
                    else
                        _onlineState = OnlineState.Offline;
                }
                else
                    ResetLogIn();
            });
        }

        private async void ExitGame()
        {
            var screen = await _screensManager.OpenPopup<ValidateQuitScreen>();
            await screen.Process();
        }

        private async void Register()
        {
            Close();
            var screen = await _screensManager.OpenScreen<RegistrationScreen>();
            screen.Process(_completion);
        }

        private async void PlayForse()
        {
            _canvasGroup.interactable = false;
            await _customIdAuthService.SignIn();
            await UniTask.WaitUntil(() => SceneNetworkContext.Instance != null);
            SceneNetworkContext.Instance.UpdateUserPublisherData("selectCharacter", "c_base", result => { });
            await Task.Delay(10);
            SceneNetworkContext.Instance.CallGetStatistics();
            var screen = await _screensManager.OpenPopup<PlayTheGameScreen>();
            screen.Process(_completion);
            var result = await screen.Process();

            if (result)
                Close();
            else
                _canvasGroup.interactable = true;
        }

        private async void ConnectWallet()
        {
            //Close();
            var screen = await _screensManager.OpenPopup<ConnectWalletScreen>();
            var result = await screen.Process(_statusMetamaskText);
            if (result)
            {
                CheckLogin();
                await UniTask.WaitUntil(() => _onlineState != OnlineState.Wait);

                if (_onlineState == OnlineState.Online) return;
                SceneNetworkContext.Instance.UpdateUserPublisherData("isOnline", "1", result => { });
                Close();
                var screen2 = await _screensManager.OpenScreen<MainMenuScreen>(); // Open MyAccountScreen
                screen2.Process(_completion);
            }
        }

        private async void ForgotPassword()
        {
            //Close();
            //await _screensManager.Open<>(); // Open Forgot password
        }
        #endregion "Button Actions"

        private void SaveValue()
        {
            if (_isSaveValue.isOn)
            {
                PlayerPrefs.SetString(AUTHENTIFICATION_EMAIL_KEY, _emailField.text);
                PlayerPrefs.SetString(AUTHENTIFICATION_PASSWORD_KEY, _passwordField.text);
                PlayerPrefs.SetInt(AUTHENTIFICATION_STATE_KEY, (int)LoginedSave.Email);
                Debug.Log("Saved value");
            }
        }
    }
}

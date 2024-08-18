using Code.Services.AuthService;
using Cysharp.Threading.Tasks;
using GrabCoin.Model;
using GrabCoin.UI.ScreenManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Popups/LinkEmailScreen.prefab")]
    public class LinkEmailScreen : UIScreenBase
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private Button _backButton;
        [SerializeField] private Button _registerButton;
        [SerializeField] private Button _agreementButton;
        [SerializeField] private Button _backAgreementButton;

        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private TMP_Text _passwordMismatchText;

        //[SerializeField] private TMP_InputField _nameField;
        [SerializeField] private TMP_InputField _emailField;
        [SerializeField] private TMP_InputField _passwordField;
        [SerializeField] private TMP_InputField _confirmPasswordField;

        [SerializeField] private Toggle _isAgreeement;
        [SerializeField] private Toggle _isSaveValue;

        [SerializeField] private GameObject _agreeementWindow;
        [SerializeField] private GameObject _registrationWindow;

        private UniTaskCompletionSource<bool> _completion;
        private EmailAuthService _emailAuthService;
        private CustomIdAuthService _customIdAuthService;
        private PlayerScreensManager _screensManager;
        private SendEmail _sendEmail;
        private UserModel _userModel;
        private bool _isConfirmPassword;

        [Inject]
        private void Construct(
            EmailAuthService emailAuthService,
            UIPopupsManager popupsManager,
            CustomIdAuthService customIdAuthService,
            PlayerScreensManager screensManager,
            UserModel userModel
            )
        {
            _emailAuthService = emailAuthService;
            _customIdAuthService = customIdAuthService;
            _screensManager = screensManager;
            _userModel = userModel;
        }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _sendEmail = new();

            _registerButton.onClick.AddListener(Register);
            _backButton.onClick.AddListener(Back);
            _agreementButton.onClick.AddListener(OpenAgreement);
            _backAgreementButton.onClick.AddListener(CloseAgreement);
            _confirmPasswordField.onValueChanged.AddListener(ConfirmPassword);
            _isAgreeement.onValueChanged.AddListener(CheckAgreement);

            CheckAgreement(_isAgreeement.isOn);
        }

        private void OnEnable()
        {
            _canvasGroup.interactable = true;
            _passwordMismatchText.enabled = false;
            _emailField.text = "";
            _passwordField.text = "";
            _passwordField.text = "";
            _confirmPasswordField.text = "";
            _errorText.text = "";

            if (_userModel.AgreementAccepted)
            {
                _isAgreeement.isOn = true;
                _isAgreeement.gameObject.SetActive(false);
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

        public void Process(UniTaskCompletionSource<bool> completion)
        {
            _completion = completion;
        }

        #region "Button Actions"
        private void Back()
        {
            try
            {
                _screensManager.ClosePopup();
                _completion.TrySetResult(false);
            }
            catch { }
        }

        private async void Register()
        {
            if (_passwordField.text.Length < 6 || !_isConfirmPassword)
            {
                _errorText.text = "Error password";
                return;
            }

            _canvasGroup.interactable = false;
            _screensManager.ClosePopup();
            int hash = _emailField.text.GetHashCode();
            if (hash < 0)
                hash *= -1;
            _emailAuthService.FillingData(hash.ToString(), _emailField.text, _passwordField.text);
            var result = await _emailAuthService.LinkEmail();
            if (result)
            {
                _userModel.AcceptAgreement();
                SaveValue();
                _screensManager.ClosePopup();
                _completion.TrySetResult(true);
            }
            else
            {
                _errorText.text = _emailAuthService.message;
                _completion.TrySetResult(false);
            }
        }

        private void ConfirmPassword(string value)
        {
            _isConfirmPassword = _passwordField.text == value;
            _passwordMismatchText.enabled = !_isConfirmPassword;
        }

        private void OpenAgreement()
        {
            _agreeementWindow.SetActive(true);
            _registrationWindow.SetActive(false);
        }

        private void CloseAgreement()
        {
            _agreeementWindow.SetActive(false);
            _registrationWindow.SetActive(true);
        }

        private void CheckAgreement(bool isOn)
        {
            _registerButton.enabled = isOn;
        }
        #endregion "Button Actions"

        private void SaveValue()
        {
            if (_isSaveValue.isOn)
            {
                PlayerPrefs.SetString(LoginScreen.AUTHENTIFICATION_EMAIL_KEY, _emailField.text);
                PlayerPrefs.SetString(LoginScreen.AUTHENTIFICATION_PASSWORD_KEY, _passwordField.text);
                PlayerPrefs.SetInt(LoginScreen.AUTHENTIFICATION_STATE_KEY, (int)LoginScreen.LoginedSave.Email);
                Debug.Log("Saved value");
            }
        }
        private void AcceptAgreement()
        {
            _userModel.AcceptAgreement();
        }
    }
}

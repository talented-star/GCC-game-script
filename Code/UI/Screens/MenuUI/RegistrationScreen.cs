using Code.Services.AuthService;
using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using System.Net.Mail;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System;
using UnityEngine.Device;
using GrabCoin.Model;
using GrabCoin.UI.HUD;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/RegistrationScreen.prefab")]
    public class RegistrationScreen : UIScreenBase
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
        private async void Back()
        {
            Close();
            await _screensManager.OpenScreen<LoginScreen>();
        }

        private async void Register()
        {
            if (_passwordField.text.Length < 6 || !_isConfirmPassword)
            {
                _errorText.text = "Error password";
                return;
            }

            _canvasGroup.interactable = false;
            int hash = _emailField.text.GetHashCode();
            if (hash < 0)
                hash *= -1;
            _emailAuthService.FillingData(hash.ToString(), _emailField.text, _passwordField.text);
            var result = await _emailAuthService.SignUp();
            if (result)
            {
                SceneNetworkContext.Instance.UpdateUserPublisherData("isOnline", "0", result =>
                {
                    _userModel.AcceptAgreement();
                    Back();
                });
            }
            else
            {
                _canvasGroup.interactable = true;
                _errorText.text = _emailAuthService.message;
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
    }

    public class SendEmail
    {
        public string fromEmail = "arktnels@gmail.com";
        public string subject = "SubjectName";
        public string body = "Body of the email";
        public string password = "867FJsl5309.,";

        public int EmailSending(string toEmail)
        {
            int value = UnityEngine.Random.Range(1000, 10000);
            // отправитель - устанавливаем адрес и отображаемое в письме имя
            MailAddress from = new MailAddress("arktnels@gmail.com", "Tom");
            // кому отправляем
            MailAddress to = new MailAddress("arktnels@gmail.com");
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);
            // тема письма
            m.Subject = "Тест";
            // текст письма
            m.Body = "<h2>Письмо-тест работы smtp-клиента</h2>" +
                $"\n{value}";
            // письмо представляет код html
            m.IsBodyHtml = true;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            // логин и пароль
            smtp.Credentials = new NetworkCredential(fromEmail, password);
            smtp.EnableSsl = true;
            smtp.Send(m);
            return value;
        }

        private async UniTask SendEmailAsync()
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("myusername@gmail.com", "mypwd"),
                EnableSsl = true
            };
            client.Send("myusername@gmail.com", "myusername@gmail.com", "test", "testbody");
        }

    }
}

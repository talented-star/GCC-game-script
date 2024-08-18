using Code.Services.AuthService;
using Cysharp.Threading.Tasks;
using GrabCoin.Enum;
using GrabCoin.GameWorld.Player;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Model;
using GrabCoin.UI.ScreenManager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UnityEngine.SceneManagement;
using System.Collections;
using GrabCoin.Helper;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Popups/PlayTheGameScreen.prefab")]
    public class PlayTheGameScreen : UIScreenBase
    {
        [SerializeField] private Button _playVRButton;
        [SerializeField] private Button _PlayDescButton;
        [SerializeField] private Button _exitButton;

        [SerializeField] private Toggle _isAgreeement;

        [SerializeField] private GameObject _agreeementWindow;
        [SerializeField] private GameObject _registrationWindow;

        private CanvasGroup _canvasGroup;
        private MetamaskAuthService _metamaskAuthService;
        private PlayerState _playerState;
        private CatalogManager _catalogManager;
        private UserModel _userModel;
        private PlayerScreensManager _screensManager;
        private UniTaskCompletionSource<bool> _mainCompletion;
        private UniTaskCompletionSource<bool> _completion;

        [Inject]
        private void Construct(
            MetamaskAuthService authService,
            PlayerState playerState,
            CatalogManager catalogManager,
            UserModel userModel,
            PlayerScreensManager screensManager
            )
        {
            _metamaskAuthService = authService;
            _playerState = playerState;
            _catalogManager = catalogManager;
            _userModel = userModel;
            _screensManager = screensManager;
        }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            _exitButton.onClick.AddListener(CloseAppClicked);
            _PlayDescButton.onClick.AddListener(() => SelectMode(PlayerMode.ThirdPerson));
            _playVRButton.onClick.AddListener(() => CalibrateForVR());
            _isAgreeement.onValueChanged.AddListener(CheckAgreement);

            CheckAgreement(_isAgreeement.isOn);
        }

        private void OnEnable()
        {
            _canvasGroup.interactable = true;

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
            _mainCompletion = completion;
        }

        private void CloseAppClicked()
        {
            _screensManager.ClosePopup();
            _completion.TrySetResult(false);
        }

        public void SelectMode(PlayerMode mode)
        {
            _playerState.ChangePlayerMode(mode);
            PlayGame();
        }

        private async void PlayGame()
        {
            _userModel.AcceptAgreement();
            _screensManager.ClosePopup();
            _catalogManager.Initialize();
            await _screensManager.OpenPopup<GameLoadingScreen>();
            // await _catalogManager.WaitInitialize();
            _mainCompletion?.TrySetResult(true);
            _completion.TrySetResult(true);
        }

        private void CheckAgreement(bool isOn)
        {
            _playVRButton.enabled = isOn;
            _PlayDescButton.enabled = isOn;
        }

        private void CalibrateForVR ()
        {
            StartCoroutine(CalibrateForVR_coroutine());
        }

        private IEnumerator CalibrateForVR_coroutine ()
        {
            SceneManager.LoadSceneAsync("CalibrationVRRoom", LoadSceneMode.Additive);
            WaitForSeconds waitALittle = new WaitForSeconds(.5f);
            while (VR_Helper.Instance == null)
            {
                yield return waitALittle;
            }
            VR_Helper.Instance.EnableXR();
            VR_Helper.State vrState = VR_Helper.Instance.GetState();
            while (vrState == VR_Helper.State.InitializingVR)
            {
                yield return waitALittle;
                vrState = VR_Helper.Instance.GetState();
            }
            /*
            if (vrState == VR_Helper.State.InitVR_OK)
            {
                SelectMode(PlayerMode.VR);
            }
            */
        }
    }
}

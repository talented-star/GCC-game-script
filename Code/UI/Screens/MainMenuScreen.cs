using Code.Services.AuthService;
using Cysharp.Threading.Tasks;
using GrabCoin.Enum;
using GrabCoin.GameWorld.Player;
using GrabCoin.UI.ScreenManager;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using Zenject;
using System;
using System.Linq;
using TMPro;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Model;
using GrabCoin.UI.HUD;
using GrabCoin.Services.Backend.Inventory;
using PlayFabCatalog;
using System.Collections.Generic;
using Jint.Runtime;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/MainMenuScreen.prefab")]
    public class MainMenuScreen : UIScreenBase
    {
        [SerializeField] private Button _authMetamaskButton;
        [SerializeField] private Button _authEmailButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _exitButton;

        [Space(20)]
        [SerializeField] private TMP_Text _metamaskAddressText;
        [SerializeField] private TMP_Text _emailAddressText;
        [SerializeField] private TMP_Text _accountNameText;

        [SerializeField] private TMP_Text _statusMetamaskText;

        [SerializeField] private SelectCharacterMenu _selectCharacterMenu;
        private PlayerState _playerState;
        private MetamaskAuthService _metamaskAuthService;
        private EmailAuthService _emailAuthService;
        private UniTaskCompletionSource<bool> _completion;
        private PlayerScreensManager _screensManager;
        private CatalogManager _catalogManager;
        private UserModel _userModel;
        private InventoryDataManager _inventoryData;

        [Inject]
        private void Construct(
            PlayerState playerState,
            MetamaskAuthService authService,
            EmailAuthService emailAuthService,
            PlayerScreensManager screensManager,
            CatalogManager catalogManager,
            UserModel userModel,
            InventoryDataManager inventoryData
            )
        {
            _playerState = playerState;
            _metamaskAuthService = authService;
            _emailAuthService = emailAuthService;
            _screensManager = screensManager;
            _catalogManager = catalogManager;
            _userModel = userModel;
            _inventoryData = inventoryData;
            _selectCharacterMenu.Init(_catalogManager, _inventoryData, _userModel);
        }

        private void Awake()
        {
            _authMetamaskButton.onClick.AddListener(LinkMetamask);
            _authEmailButton.onClick.AddListener(LinkEmail);
            _settingsButton.onClick.AddListener(OpenSettings);
            _playButton.onClick.AddListener(PlayGame);
            _exitButton.onClick.AddListener(CloseAppClicked);
            _metamaskAddressText.gameObject.SetActive(false);
            _emailAddressText.gameObject.SetActive(false);
            _accountNameText.text = "";
            _metamaskAddressText.text = "";
            _emailAddressText.text = "";
            _playButton.interactable = false;
        }

        private void OnEnable()
        {
            GetComponent<CanvasGroup>().interactable = false;
            _catalogManager.Initialize();
            GetAccountInfo();
        }

        public override void CheckOnEnable()
        {

        }

        public UniTask<bool> Process()
        {
            _completion = new UniTaskCompletionSource<bool>();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            return _completion.Task;
        }

        public void Process(UniTaskCompletionSource<bool> completion)
        {
            _completion = completion;
        }

        private async void PlayGame()
        {
            var screen = await _screensManager.OpenPopup<PlayTheGameScreen>();
            screen.Process(_completion);
            var result = await screen.Process();
            if (result)
            {
                _completion.TrySetResult(true);
                Close();
            }
        }

        private async void LinkMetamask()
        {
            var screen = await _screensManager.OpenPopup<WaitingAuthMetamaskScreen>();
            var result = await screen.Process(_statusMetamaskText);
            if (result)
            {
                _authMetamaskButton.gameObject.SetActive(false);
                CheckAddress();
                _metamaskAddressText.gameObject.SetActive(true);
                _selectCharacterMenu.Populate();
            }
            else
            {
                PlayFabSettings.staticPlayer.ForgetAllCredentials();
                Close();
                await _screensManager.OpenScreen<LoginScreen>();
            }
        }

        private async void LinkEmail()
        {
            var screen = await _screensManager.OpenPopup<LinkEmailScreen>();
            var result = await screen.Process();
            if (result)
                GetAccountInfo();
        }

        private async void OpenSettings()
        {
            var screen = await _screensManager.OpenScreen<SettingsScreen>();
            screen.Process(this).Forget();
        }

        private async void CloseAppClicked()
        {
            SceneNetworkContext.Instance.UpdateUserPublisherData("isOnline", "0", result => { });
            Close();
            PlayFabSettings.staticPlayer.ForgetAllCredentials();
            PlayerPrefs.DeleteKey(LoginScreen.AUTHENTIFICATION_EMAIL_KEY);
            PlayerPrefs.DeleteKey(LoginScreen.AUTHENTIFICATION_PASSWORD_KEY);
            PlayerPrefs.DeleteKey(LoginScreen.AUTHENTIFICATION_STATE_KEY);
            _userModel.UnAuthorize();
            var screen = await _screensManager.OpenScreen<LoginScreen>();
        }

        private void GetAccountInfo()
        {
            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
                result =>
                {
                    var displayName = result.AccountInfo.TitleInfo.DisplayName;
                    var userName = result.AccountInfo.Username;
                    var name = !string.IsNullOrEmpty(displayName) ? displayName :
                    !string.IsNullOrEmpty(userName) ? userName : "Гость";
                    _accountNameText.text = "Вы вошли как:\n" + name;

                    if (_metamaskAuthService.ValidateAuth().Result)
                    {
                        _authMetamaskButton.gameObject.SetActive(false);
                        CheckAddress();
                        _metamaskAddressText.gameObject.SetActive(true);
                    }
                    else
                    {
                        _authMetamaskButton.gameObject.SetActive(true);
                        _metamaskAddressText.gameObject.SetActive(false);
                    }

                    if (!string.IsNullOrEmpty(result.AccountInfo.PrivateInfo.Email))
                    {
                        _authEmailButton.gameObject.SetActive(false);
                        _emailAddressText.text = result.AccountInfo.PrivateInfo.Email;
                        _emailAddressText.gameObject.SetActive(true);
                    }
                    else
                    {
                        _authEmailButton.gameObject.SetActive(true);
                        _emailAddressText.gameObject.SetActive(false);
                    }

                    _selectCharacterMenu.Populate();

                    GetComponent<CanvasGroup>().interactable = true;
                }, Debug.LogError);
        }

        private void CheckAddress()
        {
            var address = _metamaskAuthService.GetAddress();
            var sub1 = address.Substring(0, 7);
            var sub2 = address.Substring(address.Length - 5, 5);
            _metamaskAddressText.text = sub1 + "..." + sub2;
        }
    }

    [Serializable]
    public class SelectCharacterMenu
    {
        public const string SELECTED_CHARACTER_KEY = "SelectedCharacterKey";

        [SerializeField] private CharacterSelectSlot _characterIconPrefab;
        [SerializeField] private Transform _characterContainer;
        [SerializeField] private TMP_Text _characterInfo;
        [SerializeField] private Button _playButton;

        private List<CharacterSelectSlot> _characterSlots = new();
        private CatalogManager _catalogManager;
        private InventoryDataManager _inventoryData;
        private UserModel _userModel;
        private bool _inProcessing;

        public void Init(CatalogManager catalogManager, InventoryDataManager inventoryData, UserModel userModel)
        {
            _catalogManager = catalogManager;
            _inventoryData = inventoryData;
            _userModel = userModel;
            _characterInfo.text = "";
        }

        public async void Populate()
        {
            if (_inProcessing) await UniTask.WaitWhile(() => _inProcessing);
            _inProcessing = true;
            await CleanCharacters();
            await CheckSelectCharacter();

            await _catalogManager.WaitInitialize();
            if (!_inventoryData.IsFinishedInit) await UniTask.WaitUntil(() => _inventoryData.IsFinishedInit);
            var characters = _inventoryData.GetItemsWithClass("Character");
            bool isMetamaskAuth = !string.IsNullOrWhiteSpace(_userModel.AuthInfo.SignSignature);
            foreach (var character in characters)
            {
                var slot = GameObject.Instantiate(_characterIconPrefab, _characterContainer);
                _characterSlots.Add(slot);
#if !UNITY_EDITOR
                if (!isMetamaskAuth) slot.SetInteractable(character.ItemId is "c_base" or "с_base");
#else
                slot.SetInteractable(true);
#endif
                slot.UnSelect();
                if (_catalogManager.GetItemData(character.ItemId) == null)
                    await _catalogManager.CashingItem(character.ItemId);
                var itemData = _catalogManager.GetItemData(character.ItemId);

                slot.Populate(character, itemData?.itemConfig?.Icon ?? default, SelectCharacter);
            }
            _inProcessing = false;
        }

        private void SelectCharacter(Item item)
        {
            _playButton.interactable = true;
            foreach (var charSlot in _characterSlots)
                charSlot.UnSelect();

            SceneNetworkContext.Instance.UpdateUserPublisherData("selectCharacter", item.ItemId, result => { });
            PlayerPrefs.SetString(SELECTED_CHARACTER_KEY, item.ItemId);
            CharacterItem characterItem = item.GetItemData() as CharacterItem;
            _characterInfo.text = $"Name: {characterItem.DisplayName}\n" +
            $"Health: {characterItem.customData.health}\n" +
            $"Walk speed: {characterItem.customData.walkSpeed}\n" +
            $"Run speed multiplier: {characterItem.customData.runSpeedMultiplier}\n";
        }

        private UniTask<string> CheckSelectCharacter()
        {
            var completion = new UniTaskCompletionSource<string>();
            string selectCharacter = "c_base";
            if (!string.IsNullOrWhiteSpace(_userModel.AuthInfo.SignSignature))
            {
                SceneNetworkContext.Instance.GetUserPublisherData("selectCharacter", result =>
                {
                    if (result.isInit)
                    {
                        var data = result.Value.Data;
                        if (data.ContainsKey("selectCharacter"))
                        {
                            string name = data["selectCharacter"].Value;
                            if (!string.IsNullOrWhiteSpace(name))
                                selectCharacter = name;
                        }
                    }
                    completion.TrySetResult(selectCharacter);
                });
            }
            else
                completion.TrySetResult(selectCharacter);
            return completion.Task;
        }

        private async System.Threading.Tasks.Task<bool> CleanCharacters()
        {
            _characterSlots.Clear();
            int count = _characterContainer.childCount;
            for (int i = 0; i < count; i++)
            {
                UnityEngine.Object.Destroy(_characterContainer.GetChild(0).gameObject);
                await UniTask.DelayFrame(2);
            }
            return true;
        }
    }
}
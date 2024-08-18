using Cysharp.Threading.Tasks;
using GrabCoin.GameWorld.Player;
using GrabCoin.Services.Backend.Inventory;
using GrabCoin.UI.HUD;
using GrabCoin.UI.ScreenManager;
using GrabCoin.UI.Screens;
using InventoryPlus;
using PlayFab;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI
{
    [UIScreen("UI/Screens/Other/InGameMenu.prefab")]
    public class InGameMenu : UIScreenBase
    {
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TMP_Text _playerIdText;
        [SerializeField] private TMP_Text _buildIdText;

        [Inject] private Controls _controls;
        [Inject] private PlayerState playerState;
        [Inject] private InventoryDataManager _inventoryDataManager;
        [Inject] private PlayerScreensManager _screensManager;

        private UniTaskCompletionSource<bool> _completion;

        private static InGameMenu _instance;
        public static InGameMenu Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindAnyObjectByType<InGameMenu>();
                return _instance;
            }
        }

        private void Start()
        {
            _exitButton.onClick.AddListener(Exit);
            _settingsButton.onClick.AddListener(OpenSettings);

            //playerState.MenuActiveEvent += OnActivatedMenu;

            //OnActivatedMenu();
        }

        private void OnDestroy()
        {
            _exitButton.onClick.RemoveAllListeners();

            playerState.MenuActiveEvent -= OnActivatedMenu;
        }

        private void OnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
            _playerIdText.text = PlayFabSettings.staticPlayer.PlayFabId;
            _buildIdText.text = Application.version;
        }

        public override void CheckOnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }

        //private void Update()
        //{
        //    if (ScreenOverlayManager.GetActiveWindow()!=this)
        //    {
        //        return;
        //    }
        //    if (_controls.Player.CallMenu.WasPerformedThisFrame() && gameObject.activeSelf)
        //    {
        //        Close();
        //        Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = false });
        //        _completion.TrySetResult(true);
        //    }
        //}

        public override void CheckInputHandler(Controls controls)
        {
            base.CheckInputHandler(controls);
            if (controls.Player.CallMenu.WasPressedThisFrame())
            {
                _screensManager.OpenScreen<GameHud>().Forget();
            }
        }

        public UniTask<bool> Process()
        {
            _completion = new UniTaskCompletionSource<bool>();
            return _completion.Task;
        }

        private void Exit()
        {
            if (_inventoryDataManager.IsBlockedCurrencyForRaid)
            {
                SceneNetworkContext.Instance.SubtractUserVirtualCurrency("GC", 5, result => { Application.Quit(); });
            }
            else
                Application.Quit();
        }

        private void OnActivatedMenu()
        {
            gameObject.SetActive(playerState.IsMenuActive);
        }

        private async void OpenSettings()
        {
            //Close();
            var screen = await _screensManager.OpenScreen<SettingsScreen>();
            screen.Process(this).Forget();
            //var result = await screen.Process();
            //await _screensManager.Open<InGameMenu>();
        }
    }
}

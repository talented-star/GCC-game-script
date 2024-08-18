using Code.Services.AuthService;
using Cysharp.Threading.Tasks;
using GrabCoin.Enum;
using GrabCoin.GameWorld.Player;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Services.Backend.Inventory;
using GrabCoin.UI.HUD;
using GrabCoin.UI.ScreenManager;
using Mirror;
using PlayFab;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Popups/StartRaidPopup.prefab")]
    public class StartRaidScreen : UIScreenBase
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _playGCButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_Text _vaucherCurrencyText;
        [SerializeField] private TMP_Text _gcCurrencyText;

        private CanvasGroup _canvasGroup;
        private InventoryDataManager _inventoryManager;
        private PlayerScreensManager _screensManager;
        private UniTaskCompletionSource<bool> _completion;

        [Inject]
        private void Construct(
            InventoryDataManager inventoryManager,
            PlayerScreensManager screensManager
            )
        {
            _inventoryManager = inventoryManager;
            _screensManager = screensManager;
        }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            _backButton.onClick.AddListener(CloseAppClicked);
            _playButton.onClick.AddListener(PlayVaucherGame);
            _playGCButton.onClick.AddListener(PlayGCGame);
        }

        private void OnEnable()
        {
            _canvasGroup.interactable = true;
            int countVC = _inventoryManager.GetCurrencyVC();
            decimal countGC = _inventoryManager.GetCurrencyData();
            _vaucherCurrencyText.text = countVC.ToString();
            _gcCurrencyText.text = countGC.ToString("F2");

            _playButton.interactable = countVC > 0;
            _playGCButton.interactable = countGC >= 5;
            SetActiveScreen(true);
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }

        public override void CheckOnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }

        public override void CheckInputHandler(Controls controls)
        {
            base.CheckInputHandler(controls);
            if (controls.Player.CallMenu.WasPressedThisFrame())
            {
                CloseAppClicked();
            }
        }

        public UniTask<bool> Process()
        {
            _completion = new UniTaskCompletionSource<bool>();
            return _completion.Task;
        }

        private void CloseAppClicked()
        {
            //Close();
            _screensManager.ClosePopup();
            _completion.TrySetResult(false);
            SetActiveScreen(false);
        }

        private void PlayVaucherGame()
        {
            Debug.Log("Call request to start raid");
            _canvasGroup.interactable = false;
            SceneNetworkContext.Instance.SubtractUserCurrency("VC", 1, result =>
            {
                Debug.Log("Take answer to start raid");
                if (!result) return;
                _completion.TrySetResult(result);
                _inventoryManager.RefreshInventory();
            });
            _screensManager.ClosePopup();
            _canvasGroup.interactable = true;
            SetActiveScreen(false);
        }

        private void PlayGCGame()
        {
            _inventoryManager.BlockedPayRaid();
            _screensManager.ClosePopup();
            _completion.TrySetResult(true);
            SetActiveScreen(false);
        }

        private void SetActiveScreen(bool isActive)
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = isActive });
            //Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
            //Cursor.visible = isActive;
            //Debug.Log($"<color=blue>Cursor visible: {Cursor.visible}</color>");
        }
    }
}

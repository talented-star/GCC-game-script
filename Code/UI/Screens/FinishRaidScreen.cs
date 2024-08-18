using Cysharp.Threading.Tasks;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Services.Backend.Inventory;
using GrabCoin.UI.HUD;
using GrabCoin.UI.ScreenManager;
using InventoryPlus;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Popups/FinishRaidPopup.prefab")]
    public class FinishRaidScreen : UIScreenBase
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private RaidItemSlot _itemUIPrefab;
        [SerializeField] private Transform _itemContext;
        [SerializeField] private TMP_Text _gcCurrencyText;

        private CanvasGroup _canvasGroup;
        private InventoryDataManager _inventoryManager;
        private CatalogManager _catalogManager;
        private PlayerScreensManager _screensManager;
        private UniTaskCompletionSource<bool> _completion;

        [Inject]
        private void Construct(
            InventoryDataManager inventoryManager,
            CatalogManager catalogManager,
            PlayerScreensManager screensManager
            )
        {
            _inventoryManager = inventoryManager;
            _catalogManager = catalogManager;
            _screensManager = screensManager;
        }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            _backButton.onClick.AddListener(CloseAppClicked);
        }

        bool _isShowed;
        private async void OnEnable()
        {
            if (_isShowed) return;
            _isShowed = true;

            await UniTask.Delay(1000);
            for (int i = _itemContext.childCount - 1; i >= 0; i--)
                Destroy(_itemContext.GetChild(i).gameObject);
            _canvasGroup.interactable = true;
            _gcCurrencyText.text = "0";
            var currencyBuffer = _inventoryManager.GetCurrencyBuffer();
            var itemBuffer = _inventoryManager.GetItemBuffer();

            foreach (var currency in currencyBuffer)
            {
                if (currency.Key == "SC")
                {
                    _gcCurrencyText.text = (currency.Value).ToString("F2");
                    break;
                }
            }
            foreach (var item in itemBuffer)
            {
                var itemSlot = Instantiate(_itemUIPrefab, _itemContext);
                var itemData = _catalogManager.GetResourceData(item.Key);
                itemSlot.Populate(
                    itemData.itemConfig.Icon,
                    itemData.DisplayName,
                    item.Value);
                //InventoryScreenManager.Instance.AddStackableItem(item.Key, item.Value);
            }
            //_inventoryManager.RefreshInventory();
            
            SetActiveScreen(true);
            _inventoryManager.GrantFromBuffer(currencyBuffer, itemBuffer);
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
            _isShowed = false;
            _screensManager.ClosePopup();
            for (int i = _itemContext.childCount - 1; i >= 0; i--)
                Destroy(_itemContext.GetChild(i).gameObject);
            _completion?.TrySetResult(false);
            //SetActiveScreen(false);
        }

        private void SetActiveScreen(bool isActive)
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = isActive });
            //Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
            //Cursor.visible = isActive;
        }
    }
}

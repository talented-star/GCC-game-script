using UnityEngine;
using UnityEngine.UI;
using Zenject;
using GrabCoin.UI.ScreenManager;
using System.Collections.Generic;
using System;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Services.Backend.Inventory;
using PlayFabCatalog;
using PlayFab.ClientModels;
using TMPro;
using Cysharp.Threading.Tasks;
using GrabCoin.UI.HUD;
using InventoryPlus;
using static PlayFabCatalog.AddedCustomDataInPlayFabItems;
using NaughtyAttributes;
using Newtonsoft.Json;
// using static UnityEditor.Progress;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/LaboratoryScreen.prefab")]
    public class LaboratoryScreen : UIScreenBase
    {

        [SerializeField] private Button _backButton;
        [SerializeField] private Transform _resourcesContext;
        [SerializeField] private Transform _storeContext;
        [SerializeField] private ItemSlot _itemSlot;
        [SerializeField] private ItemStoreSlot _storeSlot;
        [SerializeField] private TMP_Text _countCurrency;

        [HorizontalLine(color: EColor.Gray)]
        [SerializeField] private ItemWorkshopSlot _itemShopSlot;

        [SerializeField] private Button _buyButton;
        [SerializeField] private Transform _itemsShopContext;
        [SerializeField] private Image _selectItemImage;
        [SerializeField] private TMP_Text _selectItemState;
        [SerializeField] private TMP_Text _selectItemDescription;
        [SerializeField] private TMP_Text _costCurrency;
        //[SerializeField] private TMP_Text _walletCurrency;

        [SerializeField] private Button _addButton;
        [SerializeField] private Button _substractButton;
        [SerializeField] private TMP_InputField _countSellText;

        private WaitAnswerServerScreen _waitAnswer;
        //private StoreItem _selectedItem;
        private string _selectedItem;
        private int _countItem;

        private CatalogManager _catalogManager;
        private InventoryDataManager _inventoryManager;
        //private UIPopupsManager _popupsManager;
        private PlayerScreensManager _screensManager;

        private static LaboratoryScreen _instance;
        public static LaboratoryScreen Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindAnyObjectByType<LaboratoryScreen>();
                return _instance;
            }
        }

        [Inject]
        public void Construct(
            CatalogManager catalogManager,
            InventoryDataManager inventoryManager,
            //UIPopupsManager popupsManager,
            PlayerScreensManager screensManager
            )
        {
            _catalogManager = catalogManager;
            _inventoryManager = inventoryManager;
            //_popupsManager = popupsManager;
            _screensManager = screensManager;
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(Back);
            _buyButton.onClick.AddListener(() =>
                BuyItems(_selectedItem,
                _catalogManager.GetBuyLabItemsV2()[_selectedItem].Prices[0].Amounts[0].Amount * 0.01m,
                Int32.Parse(_countSellText.text)));
            _countSellText.onValueChanged.AddListener(CheckMaxCount);
            _addButton.onClick.AddListener(AddValue);
            _substractButton.onClick.AddListener(SubstractValue);
        }

        private void OnEnable()
        {
            //ClearRafineryResources();
            //ClearRafineryQueue();
            //PopulateResources();
            ////PopulateStore();
            //PopulateStoreV2();
            ClearShopItems();
            PopulateShopItems();

            _countSellText.text = "0";
            _countCurrency.text = _inventoryManager.GetCurrencyData().ToString("F2");
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
                _screensManager.OpenScreen<GameHud>().Forget();
        }

        private void Back()
        {
            _screensManager.OpenScreen<GameHud>().Forget();
            //Close();
            //Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = false });
        }

        private bool _infoOpening;
        private async void CheckMaxCount(string value)
        {
            if (Int32.Parse(value) > _countItem)
                _countSellText.text = _countItem.ToString();
            if (InventoryScreenManager.Instance.Inventory.CheckWeightLimit(_selectedItem, Int32.Parse(_countSellText.text), out int limit))
            {
                _countSellText.text = limit.ToString();
                if (_infoOpening) return;
                _infoOpening = true;
                if (_screensManager.EqualsCurrentPopup<InfoPopup>()) return;
                var screen = await _screensManager.OpenPopup<InfoPopup>();
                _infoOpening = false;
                screen.ProcessKey("InfoPopupInvFull");
            }

        }

        private async void AddValue()
        {
            var count = Int32.Parse(_countSellText.text);
            if (count >= _countItem)
                _countSellText.text = _countItem.ToString();
            else if (InventoryScreenManager.Instance.Inventory.CheckWeightLimit(_selectedItem, count + 1, out int limit))
            {
                if (_infoOpening) return;
                _infoOpening = true;
                if (_screensManager.EqualsCurrentPopup<InfoPopup>()) return;
                var screen = await _screensManager.OpenPopup<InfoPopup>();
                screen.ProcessKey("InfoPopupInvFull");
                _infoOpening = false;
            }
            else
                _countSellText.text = (count + 1).ToString();

            var cost = _catalogManager.GetBuyLabItemsV2()[_selectedItem].Prices[0].Amounts[0].Amount * 0.01m;
            _costCurrency.text = (cost * Int32.Parse(_countSellText.text)).ToString("F2");
        }

        private void SubstractValue()
        {
            var count = Int32.Parse(_countSellText.text);
            if (count <= 0)
                _countSellText.text = "0";
            else
                _countSellText.text = (count - 1).ToString();

            var cost = _catalogManager.GetBuyLabItemsV2()[_selectedItem].Prices[0].Amounts[0].Amount * 0.01m;
            _costCurrency.text = (cost * Int32.Parse(_countSellText.text)).ToString("F2");
        }

        #region "Old"
        private void ClearRafineryQueue()
        {
            for (int i = _storeContext.childCount - 1; i >= 0; i--)
                Destroy(_storeContext.GetChild(i).gameObject);
        }

        private void ClearRafineryResources()
        {
            for (int i = _resourcesContext.childCount - 1; i >= 0; i--)
                Destroy(_resourcesContext.GetChild(i).gameObject);
        }

        private void PopulateResources()
        {
            foreach (var item in _inventoryManager.GetItemsWithClass("Ore"))
            {
                if (item.count == 0) continue;
                var slot = Instantiate(_itemSlot, _resourcesContext);
                var itemData = _catalogManager.GetItemData(item.ItemId, item.ItemClass);
                slot.Populate(itemData.itemConfig.Icon, item.count.ToString());
            }
            foreach (var item in _inventoryManager.GetItemsWithClass("Consumable"))
            {
                if (item.count == 0) continue;
                var slot = Instantiate(_itemSlot, _resourcesContext);
                var itemData = _catalogManager.GetItemData(item.ItemId, item.ItemClass);
                slot.Populate(itemData.itemConfig.Icon, item.count.ToString());
            }
        }

        private void PopulateStore()
        {
            var countCurrency = _inventoryManager.GetCurrencyData();
            foreach (var data in _catalogManager.GetBuyLabItems())
            {
                var slot = Instantiate(_storeSlot, _storeContext);
                var itemData = _catalogManager.GetItemData(data.ItemId);
                var count = (int)(countCurrency / (data.VirtualCurrencyPrices["SC"] * 0.01m));
                slot.Populate(_screensManager, data.ItemId, itemData.itemConfig.Icon, data.VirtualCurrencyPrices["SC"] * 0.01m, count);
                slot.onBuyCallback += BuyItems;
            }
        }

        private void PopulateStoreV2()
        {
            var countCurrency = _inventoryManager.GetCurrencyData();
            foreach (KeyValuePair<string, PlayFab.EconomyModels.CatalogPriceOptions> data in _catalogManager.GetBuyLabItemsV2())
            {
                var slot = Instantiate(_storeSlot, _storeContext);
                var itemData = _catalogManager.GetItemData(data.Key);
                var count = (int)(countCurrency / (data.Value.Prices[0].Amounts[0].Amount * 0.01m));
                slot.Populate(_screensManager, data.Key, itemData.itemConfig.Icon, data.Value.Prices[0].Amounts[0].Amount * 0.01m, count);
                slot.onBuyCallback += BuyItems;
            }
        }
        #endregion "Old"

        #region "New"
        private void ClearShopItems()
        {
            for (int i = _itemsShopContext.childCount - 1; i >= 0; i--)
                Destroy(_itemsShopContext.GetChild(i).gameObject);
            _selectItemImage.enabled = false;
            _selectItemState.text = "";
            _selectItemDescription.text = "";
            _costCurrency.text = "";
            _countCurrency.text = "";
        }

        private void PopulateShopItems()
        {
            var countCurrency = _inventoryManager.GetCurrencyData();
            foreach (KeyValuePair<string, PlayFab.EconomyModels.CatalogPriceOptions> data in _catalogManager.GetBuyLabItemsV2())
            {
                var slot = Instantiate(_itemShopSlot, _itemsShopContext);
                var itemData = _catalogManager.GetItemData(data.Key);
                var count = (int)(countCurrency / (data.Value.Prices[0].Amounts[0].Amount * 0.01m));
                slot.Populate(data.Key, itemData.itemConfig.Icon, itemData.DisplayName, SelectItem);
            }
            _countCurrency.text = countCurrency.ToString("F2");
        }

        private void SelectItem(string itemId)
        {
            bool canBuy = true;
            _selectedItem = itemId;
            ConsumableItem itemData = _catalogManager.GetItemData(itemId) as ConsumableItem;
            _selectItemImage.sprite = itemData.itemConfig.Icon;
            _selectItemImage.enabled = true;
            _selectItemState.text = $"{itemData.DisplayName}";
            _selectItemDescription.text = itemData.Description;
            var cost = _catalogManager.GetBuyLabItemsV2()[itemId].Prices[0].Amounts[0].Amount * 0.01m;
            if (canBuy)
                canBuy = cost <= _inventoryManager.GetCurrencyData();
            _countItem = (int)(_inventoryManager.GetCurrencyData() / cost);
            if (_countItem > 0)
                _countSellText.text = "1";
            _costCurrency.text = cost.ToString("F2");
            //if (item.CustomData == null)
            //{
            //    _buyButton.interactable = canBuy;
            //    return;
            //}
            //string customData = item.CustomData.ToString();
            //var requirements = JsonConvert.DeserializeObject<Dictionary<string, int>>(customData);
            //foreach (var res in requirements)
            //{
            //    var slot = Instantiate(_itemSlotPrefab, _resShopContext);
            //    var resData = _catalogManager.GetItemData(res.Key);
            //    slot.Populate(resData.itemConfig.Icon, $"{res.Value}/{_inventoryManager.GetItemData(res.Key).count}");
            //    if (canBuy)
            //        canBuy = res.Value <= _inventoryManager.GetItemData(res.Key).count;
            //}
            _buyButton.interactable = canBuy;
        }
        #endregion "New"

        private async void BuyItems(string itemId, decimal cost, int count)
        {
            if (count == 0) return;
            GetComponent<CanvasGroup>().interactable = false;
            if (_waitAnswer == null || !_waitAnswer.gameObject.activeSelf)
                _waitAnswer = await _screensManager.OpenPopup<WaitAnswerServerScreen>();
            int buyedCount = 0;
            int loseCount = 0;
            decimal price = cost * count;

            _inventoryManager.BuyItemsV2(itemId, count, price, result =>
            {
                if (!result) return;

                InventoryScreenManager.Instance.AddStackableItem(itemId, count);

                GetComponent<CanvasGroup>().interactable = true;
                _screensManager.ClosePopup();
                _waitAnswer = null;

                var item = _catalogManager.GetItemData(itemId) as ConsumableItem;
                var ammoType = item.customData.consumableType;

                OnEnable();
                if (ammoType != ConsumableType.Ammo && ammoType != ConsumableType.LaserAmmo) return;

                var ammos = _inventoryManager.GetConsumableDatas(ammoType);
                var inventory = InventoryScreenManager.Instance.Inventory;
                var ammoInInventory = 0;
                foreach (var ammo in ammos)
                {
                    var uiSlot = inventory.GetUISlot(ammo.ItemId);
                    if (uiSlot == null) continue;
                    var itemSlot = inventory.GetItemSlot(ammo.ItemId);
                    if (itemSlot == null) continue;

                    ammoInInventory += itemSlot.GetItemNum();
                }
                int countBullet = SceneNetworkContext.Instance.GetStatistic(ammoType switch
                {
                    ConsumableType.Ammo => Statistics.STATISTIC_AMMO,
                    ConsumableType.LaserAmmo => Statistics.STATISTIC_LASER_AMMO
                })?.Value ?? 0;
                Translator.Send(HUDProtocol.CountBullet, new StringData { value = $"{countBullet}/{ammoInInventory}" });

            });
            return;
            for (int i = 0; i < count; i++)
            {
                await UniTask.Delay(50);
                _inventoryManager.BuyItems(itemId, price, result =>
                    {
                        buyedCount++;
                        if (!result)
                        {
                            loseCount++;
                            return;
                        }
                        InventoryScreenManager.Instance.AddStackableItem(itemId, 1);
                        if (buyedCount == count)
                        {
                            if (loseCount > 0)
                            {
                                Debug.Log($"Lose buyed: {loseCount}");
                                _screensManager.ClosePopup();
                                _waitAnswer = null;
                                BuyItems(itemId, cost, loseCount);
                            }
                            else
                                SuccessBuyedItems(true, r =>
                                {
                                    GetComponent<CanvasGroup>().interactable = true;
                                    _screensManager.ClosePopup();
                                    _waitAnswer = null;
                                    int countAmmo = InventoryScreenManager.Instance.Inventory.GetItemData("i_ammo_pack").GetItemNum();
                                    int countBullet = SceneNetworkContext.Instance.GetStatistic(Statistics.STATISTIC_AMMO).Value;
                                    Translator.Send(HUDProtocol.CountBullet, new StringData { value = $"{countBullet}/{countAmmo}" });
                                });
                        }
                    });
            }
        }

        private void SuccessBuyedItems(bool result, Action<bool> onCallback = default)
        {
            if (result)
                _inventoryManager.RefreshInventory(result =>
                {
                    Debug.Log("Success refresh inventory");
                    if (result)
                        OnEnable();
                    Debug.Log($"Callback is {onCallback != null}");
                    onCallback?.Invoke(result);
                });
            else
                onCallback?.Invoke(false);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using GrabCoin.UI.ScreenManager;
using System.Collections.Generic;
using System;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.Services.Backend.Inventory;
using PlayFab.ClientModels;
using TMPro;
using PlayFabCatalog;
using Newtonsoft.Json;
using GrabCoin.UI.HUD;
using Item = GrabCoin.Services.Backend.Inventory.Item;
using NaughtyAttributes;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/WorkshopScreen.prefab")]
    public class WorkshopScreen : UIScreenBase
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _upgButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private GameObject _shopWindow;
        [SerializeField] private GameObject _upgradeWindow;

        [HorizontalLine(color: EColor.Gray)]
        [SerializeField] private ItemWorkshopSlot _itemShopSlot;

        [SerializeField] private Transform _itemsShopContext;
        [SerializeField] private Image _selectItemImage;
        [SerializeField] private TMP_Text _selectItemState;
        [SerializeField] private TMP_Text _selectItemDescription;
        [SerializeField] private TMP_Text _costCurrency;
        [SerializeField] private TMP_Text _walletCurrency;
        [SerializeField] private Transform _resShopContext;
        [SerializeField] private ItemSlot _itemSlotPrefab;

        [HorizontalLine(color: EColor.Gray)]
        [SerializeField] private Transform _upgradableContext;
        [SerializeField] private ItemUpgradableSlot _itemUpgradableSlot;
        [SerializeField] private Image _selectItemImageUpg;
        [SerializeField] private TMP_Text _selectItemStateUpg;
        [SerializeField] private TMP_Text _costCurrencyUpg;
        [SerializeField] private TMP_Text _walletCurrencyUpg;
        [SerializeField] private Transform _resShopContextUpg;
        [SerializeField] private ItemSlot _itemSlotPrefabUpg;

        private WaitAnswerServerScreen _waitAnswer;
        private StoreItem _selectedItem;
        private ItemInstance _selectedItemUpg;

        private CatalogManager _catalogManager;
        private InventoryDataManager _inventoryManager;
        //private UIPopupsManager _popupsManager;
        private PlayerScreensManager _screensManager;

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
            _buyButton.onClick.AddListener(BuyItem);
            _upgButton.onClick.AddListener(UpgradeItem);
            _shopButton.onClick.AddListener(OpenShop);
            _upgradeButton.onClick.AddListener(OpenUpgrade);
            OpenShop();
        }

        private void OnEnable()
        {
            _buyButton.interactable = false;
            _upgButton.interactable = false;
            ClearShopItems();
            PopulateShopItems();
            ClearUpgradeQueue();
            PopulateUpgrade();
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

        private void OpenShop()
        {
            _shopWindow.SetActive(true);
            _upgradeWindow.SetActive(false);
        }

        private void OpenUpgrade()
        {
            _shopWindow.SetActive(false);
            _upgradeWindow.SetActive(true);
        }

        #region "Shop"
        private void ClearShopItems()
        {
            for (int i = _itemsShopContext.childCount - 1; i >= 0; i--)
                Destroy(_itemsShopContext.GetChild(i).gameObject);
            for (int i = _resShopContext.childCount - 1; i >= 0; i--)
                Destroy(_resShopContext.GetChild(i).gameObject);
            _selectItemImage.enabled = false;
            _selectItemState.text = "";
            _selectItemDescription.text = "";
            _costCurrency.text = "";
            _walletCurrency.text = "";
        }

        private void PopulateShopItems()
        {
            var countCurrency = _inventoryManager.GetCurrencyData();
            foreach (var data in _catalogManager.GetBuyEquipItems())
            {
                var slot = Instantiate(_itemShopSlot, _itemsShopContext);
                var itemData = _catalogManager.GetItemData(data.ItemId);
                var count = (int)(countCurrency / (data.VirtualCurrencyPrices["SC"] * 0.01m));
                slot.Populate(data, itemData.itemConfig.Icon, itemData.DisplayName, SelectItem);
            }
            _walletCurrency.text = countCurrency.ToString("F2");
        }

        private void SelectItem(StoreItem item)
        {
            for (int i = _resShopContext.childCount - 1; i >= 0; i--)
                Destroy(_resShopContext.GetChild(i).gameObject);
            bool canBuy = true;
            _selectedItem = item;
            EquipmentItem itemData = _catalogManager.GetItemData(_selectedItem.ItemId) as EquipmentItem;
            _selectItemImage.sprite = itemData.itemConfig.Icon;
            _selectItemImage.enabled = true;
            _selectItemState.text = $"damage: {itemData.customData.damage}\n" +
                $"attack speed: {itemData.customData.attackSpeed}\n" +
                $"headshot multiplier: {itemData.customData.headShotMultiplier}";
            _selectItemDescription.text = itemData.Description;
            if (canBuy)
                canBuy = item.VirtualCurrencyPrices["SC"] * 0.01m <= _inventoryManager.GetCurrencyData();
            _costCurrency.text = (item.VirtualCurrencyPrices["SC"] * 0.01m).ToString("F2");
            if (item.CustomData == null)
            {
                _buyButton.interactable = canBuy;
                return;
            }
            string customData = item.CustomData.ToString();
            var requirements = JsonConvert.DeserializeObject<Dictionary<string, int>>(customData);
            foreach (var res in requirements)
            {
                var slot = Instantiate(_itemSlotPrefab, _resShopContext);
                var resData = _catalogManager.GetItemData(res.Key);
                slot.Populate(resData.itemConfig.Icon, $"{res.Value}/{_inventoryManager.GetItemData(res.Key).count}");
                if (canBuy)
                    canBuy = res.Value <= _inventoryManager.GetItemData(res.Key).count;
            }
            _buyButton.interactable = canBuy;
        }

        private async void BuyItem()
        {
            if (InventoryScreenManager.Instance.Inventory.CheckWeightLimit(_selectedItem.ItemId, 1, out int limit))
            {
                var screen = await _screensManager.OpenPopup<InfoPopup>();
                screen.ProcessKey("InfoPopupInvFull");
                return;
            }

            GetComponent<CanvasGroup>().interactable = false;
            if (_waitAnswer == null || !_waitAnswer.gameObject.activeSelf)
                _waitAnswer = await _screensManager.OpenPopup<WaitAnswerServerScreen>();
            _inventoryManager.BuyItems(_selectedItem.ItemId, _selectedItem.VirtualCurrencyPrices["SC"] * 0.01m, result =>
            {
                if (result)
                    Saled();
            });
        }

        private async void Saled()
        {
            int selledCount = 0;
            string customData = _selectedItem.CustomData.ToString();
            var requirements = JsonConvert.DeserializeObject<Dictionary<string, int>>(customData);
            int needSelled = requirements.Sum(res => res.Value);
            foreach (var res in requirements)
            {
                Item item = _inventoryManager.GetItemData(res.Key);
                for (int i = 0; i < res.Value; i++)
                {
                    SceneNetworkContext.Instance.RevokeItemFromUser(item[i], result =>
                    {
                        selledCount++;
                        if (!result) return;
                        var uiSlot = InventoryScreenManager.Instance.Inventory.GetUISlot(res.Key);
                        InventoryScreenManager.Instance.UseItem(uiSlot);
                        if (selledCount == needSelled)
                            SuccessBuyedItems(_selectedItem.ItemId, true, r =>
                            {
                                GetComponent<CanvasGroup>().interactable = true;
                                _screensManager.ClosePopup();
                                _waitAnswer = null;
                            });
                    });
                    await UniTask.Delay(10);
                }
            }
        }

        private void SuccessBuyedItems(string storeId, bool result, Action<bool> onCallback = default)
        {
            if (result)
            {
                var before = _inventoryManager.GetItemData(storeId);
                _inventoryManager.RefreshInventory(result =>
                    {
                        if (result)
                        {
                            var after = _inventoryManager.GetItemData(storeId);
                            foreach (var instanceId in after.instanceIds)
                                if (!before.instanceIds.Contains(instanceId))
                                {
                                    InventoryScreenManager.Instance.AddNotStackableItem(storeId, after.items[after.instanceIds.IndexOf(instanceId)]);
                                    break;
                                }
                            OnEnable();
                        }
                        onCallback?.Invoke(result);
                    });
            }
            else
                onCallback?.Invoke(false);
        }
        #endregion "Shop"

        #region "Upgrade"
        private void ClearUpgradeQueue()
        {
            for (int i = _upgradableContext.childCount - 1; i >= 0; i--)
                Destroy(_upgradableContext.GetChild(i).gameObject);
            _selectItemImageUpg.enabled = false;
            _selectItemStateUpg.text = "";
            _costCurrencyUpg.text = "";
            _walletCurrencyUpg.text = "";
        }

        private void PopulateUpgrade()
        {
            var countCurrency = _inventoryManager.GetCurrencyData();
            foreach (var instance in _inventoryManager.GetAllItems())
            {
                if (!_catalogManager.GetItemData(instance.ItemId).Tags.Contains("upgradable")) continue;
                var itemData = _catalogManager.GetItemData(instance.ItemId);
                string name = itemData.DisplayName;
                if (instance.CustomData != null && instance.CustomData.Count > 0 && instance.CustomData.ContainsKey("level"))
                {
                    int currentLevel = Int32.Parse(instance.CustomData["level"]);
                    if (currentLevel > 0)
                        name += $"  +{currentLevel}";
                    if (currentLevel > _catalogManager.GetBuyUpgradeItems().Count)
                        continue;
                }
                var slot = Instantiate(_itemUpgradableSlot, _upgradableContext);
                slot.Populate(instance.RefItem, itemData.itemConfig.Icon, name, SelectUpgradeItem);
            }
            _walletCurrencyUpg.text = countCurrency.ToString("F2");
        }

        private void SelectUpgradeItem(ItemInstance item)
        {
            for (int i = _resShopContextUpg.childCount - 1; i >= 0; i--)
                Destroy(_resShopContextUpg.GetChild(i).gameObject);
            bool canUpg = true;
            _selectedItemUpg = item;
            EquipmentItem itemData = _catalogManager.GetItemData(_selectedItemUpg.ItemId) as EquipmentItem;

            var shop = _catalogManager.GetBuyUpgradeItems();
            int levelUpgrade = 0;
            if (_selectedItemUpg.CustomData != null && _selectedItemUpg.CustomData.Count > 0 && _selectedItemUpg.CustomData.ContainsKey("level"))
                levelUpgrade = Int32.Parse(_selectedItemUpg.CustomData["level"]);
            var currentUpgrade = shop[levelUpgrade];

            _selectItemImageUpg.sprite = itemData.itemConfig.Icon;
            _selectItemImageUpg.enabled = true;
            _selectItemStateUpg.text = "";
            var states = JsonConvert.DeserializeObject<Dictionary<string, float>>(_catalogManager.GetItemData(currentUpgrade.ItemId).CustomData);
            foreach (var state in states)
                _selectItemStateUpg.text += $"{state.Key}: +{(int)(state.Value * 100)}%\n";

            if (canUpg)
                canUpg = currentUpgrade.VirtualCurrencyPrices["SC"] * 0.01m <= _inventoryManager.GetCurrencyData();
            _costCurrencyUpg.text = (currentUpgrade.VirtualCurrencyPrices["SC"] * 0.01m).ToString("F2");
            if (currentUpgrade.CustomData == null)
            {
                _upgButton.interactable = canUpg;
                return;
            }
            string customData = currentUpgrade.CustomData.ToString();
            var requirements = JsonConvert.DeserializeObject<Dictionary<string, int>>(customData);
            foreach (var res in requirements)
            {
                var slot = Instantiate(_itemSlotPrefabUpg, _resShopContextUpg);
                var resData = _catalogManager.GetItemData(res.Key);
                slot.Populate(resData.itemConfig.Icon, $"{res.Value}/{_inventoryManager.GetItemData(res.Key).count}");
                if (canUpg)
                    canUpg = res.Value <= _inventoryManager.GetItemData(res.Key).count;
            }
            _upgButton.interactable = canUpg;
        }

        private async void UpgradeItem()
        {
            GetComponent<CanvasGroup>().interactable = false;
            if (_waitAnswer == null || !_waitAnswer.gameObject.activeSelf)
                _waitAnswer = await _screensManager.OpenPopup<WaitAnswerServerScreen>();

            var shop = _catalogManager.GetBuyUpgradeItems();
            int levelUpgrade = 0;
            if (_selectedItemUpg.CustomData != null && _selectedItemUpg.CustomData.Count > 0 && _selectedItemUpg.CustomData.ContainsKey("level"))
                levelUpgrade = Int32.Parse(_selectedItemUpg.CustomData["level"]);
            var currentUpgrade = shop[levelUpgrade];

            SceneNetworkContext.Instance.SubtractUserVirtualCurrency("GC", currentUpgrade.VirtualCurrencyPrices["SC"] * 0.01m, result =>
            {
                Ubgraded(currentUpgrade);
            });
        }

        private async void Ubgraded(StoreItem currentUpgrade)
        {
            if (currentUpgrade.CustomData == null)
            {
                int currentLevel = (_selectedItemUpg.CustomData != null && _selectedItemUpg.CustomData.Count > 0 && _selectedItemUpg.CustomData.ContainsKey("level")) ?
                            Int32.Parse(_selectedItemUpg.CustomData["level"]) : 0;
                UpgradeItem(currentLevel);
                return;
            }
            int selledCount = 0;
            string customData = currentUpgrade.CustomData.ToString();
            var requirements = JsonConvert.DeserializeObject<Dictionary<string, int>>(customData);
            int index = 0;
            int needSelled = requirements.Sum(res => res.Value);
            foreach (var res in requirements)
            {
                Item item = _inventoryManager.GetItemData(res.Key);
                for (int i = 0; i < res.Value; i++)
                {
                    SceneNetworkContext.Instance.RevokeItemFromUser(item[i], result =>
                    {
                        selledCount++;
                        if (!result) return;
                        var uiSlot = InventoryScreenManager.Instance.Inventory.GetUISlot(res.Key);
                        InventoryScreenManager.Instance.UseItem(uiSlot);
                        if (selledCount == needSelled)
                        {
                            int currentLevel = (_selectedItemUpg.CustomData != null && _selectedItemUpg.CustomData.Count > 0 && _selectedItemUpg.CustomData.ContainsKey("level")) ?
                                Int32.Parse(_selectedItemUpg.CustomData["level"]) : 0;
                            UpgradeItem(currentLevel);
                        }
                    });
                    await UniTask.Delay(10);
                }
            }
        }

        private void UpgradeItem(int currentLevel)
        {
            SceneNetworkContext.Instance.UpdateItemInstanceToUser(_selectedItemUpg.ItemInstanceId,
                "level", (currentLevel + 1).ToString(), result =>
                {
                    SuccessUpgradeItems(_selectedItemUpg.ItemId, _selectedItemUpg.ItemInstanceId, true, r =>
                    {
                        GetComponent<CanvasGroup>().interactable = true;
                        _screensManager.ClosePopup();
                        _waitAnswer = null;
                    });
                });
        }

        private void SuccessUpgradeItems(string storeId, string instanceId, bool result, Action<bool> onCallback = default)
        {
            if (result)
            {
                var before = _inventoryManager.GetItemData(storeId);
                _inventoryManager.RefreshInventory(result =>
                {
                    if (result)
                    {
                        var items = _inventoryManager.GetItemData(storeId);
                        var item = items.items[items.instanceIds.IndexOf(instanceId)];
                        var slots = InventoryScreenManager.Instance.Inventory.GetSlots();
                        foreach (var slot in slots)
                        {
                            if (slot == null || slot.GetItemType() == null) continue;
                            if (slot.GetItemType().instanceID == instanceId)
                            {
                                if (item.CustomData != null && item.CustomData.Count > 0 && item.CustomData.ContainsKey("level"))
                                {
                                    int currentLevel = Int32.Parse(item.CustomData["level"]);
                                    slot.GetItemType().itemUpgradeLevel = currentLevel;
                                    slot.GetItemType().itemName += $" +{currentLevel}";
                                }
                            }
                        }
                        OnEnable();
                    }
                    onCallback?.Invoke(result);
                });
            }
            else
                onCallback?.Invoke(false);
        }
        #endregion "Upgrade"
    }
}
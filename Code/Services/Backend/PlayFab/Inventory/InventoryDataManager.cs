using PlayFab.ClientModels;
using PlayFab;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.UI.HUD;
using System.Linq;
using GrabCoin.UI.Screens;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using GrabCoin.GameWorld.Resources;
using System.Threading.Tasks;
using PlayFab.EconomyModels;
using Jint.Runtime;
using static UnityEngine.Application;
using static PlayFabCatalog.AddedCustomDataInPlayFabItems;
using PlayFabCatalog;
using System.Globalization;

namespace GrabCoin.Services.Backend.Inventory
{
    public class InventoryDataManager
    {
        private bool _isInit;
        private bool _isFinishedInit;
        private bool _isCompleteInventoryV2;
        private bool _isBlockedGC;
        private int countReq = 0;
        private int countAns = 0;

        private Dictionary<string, int> _bufferInventoryItems = new();
        private Dictionary<string, decimal> _bufferCurrency = new();

        private Dictionary<string, Item> _inventoryItems = new();
        private Dictionary<string, int> _currency = new();

        private List<RafineryData> _rafineryDatas = new();
        private decimal _userBalance = 0m;

        private CatalogManager _catalogManager;
        private CustomSignal _onRefreshData;

        public bool IsFinishedInit => _isFinishedInit && _isCompleteInventoryV2;
        public bool IsBlockedCurrencyForRaid => _isBlockedGC;
        public bool IsFinishRaid { get; set; }
        private decimal BalanceGC => _userBalance;

        [Inject]
        private void Construct(CatalogManager catalogManager)
        {
            _catalogManager = catalogManager;
        }

        public void Initialize()
        {
            if (_isInit) return;
            _isInit = true;

            RefreshInventory();

            _onRefreshData = OnRefreshData;
            Translator.Add<GeneralProtocol>(_onRefreshData);
        }

        #region "Inventory v2"
        private List<InventoryItem> _items;

        private UniTask<bool> RequestInventoryV2()
        {
            UniTaskCompletionSource<bool> completion = new();
            _isCompleteInventoryV2 = false;
            PlayFabEconomyAPI.GetInventoryItems(new GetInventoryItemsRequest(),
                async result =>
                {
                    _items = result.Items;

                    foreach (var item in _items)
                    {
                        if (_catalogManager.GetItemData(item.Id) == null)
                            await _catalogManager.CashingItem(item.Id);
                        if (!_inventoryItems.ContainsKey(item.Id))
                            _inventoryItems.Add(item.Id, new Item(_catalogManager, item));
                    }
                    _isCompleteInventoryV2 = true;
                    completion.TrySetResult(true);
                }, error =>
                {
                    Debug.LogError(error);
                    completion.TrySetResult(false);
                });
            return completion.Task;
        }
        #endregion "Inventory v2"

        private void OnRefreshData(System.Enum code)
        {
            switch (code)
            {
                case GeneralProtocol.RefreshCatalogData:
                    foreach (var item in _inventoryItems)
                        item.Value.RefreshData();
                    break;
            }
        }

        public async void RefreshInventory(Action<bool> callback = default)
        {
            //SceneNetworkContext.Instance.GetUserPublisherData("currency balanse", result =>
            //{
            //    GetRafineryDataRequest(result => { FillingUserInventory(callback, result); });

            //    if (result.isInit)
            //        if (result.Value.Data.ContainsKey("currency balanse"))
            //        {
            //            string balance = result.Value.Data["currency balanse"].Value;
            //            CurrencyBalance currencyBalance = JsonConvert.DeserializeObject<CurrencyBalance>(balance);
            //            _userBalance = new Optionals<CurrencyBalance>(currencyBalance);
            //            Debug.Log("User CurrencyBalance: " + balance);
            //            return;
            //        }
            //    _userBalance = new Optionals<CurrencyBalance>(new CurrencyBalance());
            //    string json = JsonConvert.SerializeObject(_userBalance);
            //    Debug.Log("User CurrencyBalance: " + json);
            //});

            await UniTask.WaitUntil(() => SceneNetworkContext.Instance != null);
            SceneNetworkContext.Instance.GetAllUserBalances(async result =>
            {
                if (result.isInit)
                {
                    List<KeyValuePair<string, UserDataRecord>> balances = result.Value.Data.Where(d => d.Key.Contains("balance")).ToList();
                    List<KeyValuePair<string, UserDataRecord>> storages = result.Value.Data.Where(d => d.Key.Contains("storage")).ToList();

                    foreach (var item in balances)
                        switch (item.Key)
                        {
                            case "balanceGC":
                                // _userBalance = decimal.Parse(item.Value.Value.Replace(".", ","));
                                _userBalance = decimal.Parse(item.Value.Value, CultureInfo.InvariantCulture);
                                Debug.Log("User CurrencyBalance: " + _userBalance);
                                break;
                            default:
                                string[] datas = item.Key.Split('_');
                                if (datas.Length < 3)
                                    continue;
                                if (_catalogManager.GetItemData(datas.Last()) == null)
                                    await _catalogManager.CashingItem(datas.Last());
                                if (!_inventoryItems.ContainsKey(datas.Last()))
                                {
                                    var inventoryItem = new InventoryItem();
                                    inventoryItem.Id = datas.Last();
                                    inventoryItem.DisplayProperties = _catalogManager.GetItemData(datas.Last()).catalogItem2.Value.DisplayProperties;
                                    int count = Int32.Parse(item.Value.Value);
                                    _inventoryItems.Add(datas.Last(), new Item(_catalogManager, inventoryItem, count));
                                }
                                else
                                    _inventoryItems[datas.Last()].count = Int32.Parse(item.Value.Value);
                                break;
                        }

                    foreach (var storage in storages)
                    {
                        switch (storage.Key)
                        {
                            case "storage_bank":
                                PlayerPrefs.SetString("storage_bank", storage.Value.Value);
                                break;
                        }
                    }
                    GetRafineryDataRequest(result.Value);
                    FillingUserInventory(callback, true);
                }
                else
                    callback?.Invoke(false);
            });
        }

        private void FillingUserInventory(Action<bool> callback, bool result)
        {
            if (result)
                PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), async result =>
                {
                    await Success(result);
                    await RequestInventoryV2();
                    callback?.Invoke(true);
                }, error =>
                {
                    callback?.Invoke(false);
                });
            else
                callback?.Invoke(false);
        }

        private async Task Success(GetUserInventoryResult result)
        {
            //_inventoryItems.Clear();
            foreach (var item in result.Inventory)
            {
                Debug.Log($"Get inventory item data: {item.DisplayName}");
                if (item.ItemClass.Equals("Consumable") ||
                    item.ItemClass.Equals("�onsumable"))
                {
                    await ConvertationConsumable(item);
                }
                else
                if (_inventoryItems.ContainsKey(item.ItemId))
                {
                    _inventoryItems[item.ItemId].count += 1;
                    _inventoryItems[item.ItemId].Add(item);
                }
                else
                    _inventoryItems.Add(item.ItemId, new Item(_catalogManager, item, 1));
            }

            CheckSubstractItem();
            CheckForGrantItems();
            await ConvertationBalance(result);

            _isFinishedInit = countAns == countReq;
            foreach (var item in _inventoryItems)
            {
                //Debug.Log($"Name item: {item.Value.item.DisplayName} ID item: {item.Value.item.ItemId} Count: {item.Value.count}");
            }
            //foreach (var currency in _currency)
            //    Debug.Log($"{currency.Key} : {currency.Value}");
        }

        private async Task ConvertationConsumable(ItemInstance item)
        {
            bool isComplete = false;

            string id = item.ItemId switch
            {
                "i_ammo_pack" => "2932ac34-741f-47c9-86f0-acb637200a17",
                "i_health_pack" => "98100c75-c6ae-4cd5-ba77-fa3777ae452a",
                "i_shield_battery" => "b4c11ddd-ab3e-487c-84b1-36be5eca4e4c",
                _ => ""
            };
            if (string.IsNullOrEmpty(id))
                return;
            if (_catalogManager.GetItemData(id /* item.ItemId */) == null)
                await _catalogManager.CashingItem(id /* item.ItemId */);
            var catalogItem = _catalogManager.GetItemData(id /* item.ItemId */);

            SceneNetworkContext.Instance.AddUserVirtualCurrency(
                $"SFT_{catalogItem.DisplayName}_" + id, 1,
                result =>
                {
                    SceneNetworkContext.Instance.RevokeItemFromUser(item.ItemInstanceId, default);

                    if (!_inventoryItems.ContainsKey(id))
                    {
                        var inventoryItem = new InventoryItem();
                        inventoryItem.Id = id;
                        //inventoryItem.DisplayProperties = catalogItem.catalogItem2.Value.DisplayProperties;
                        _inventoryItems.Add(id, new Item(_catalogManager, inventoryItem, 1));
                    }
                    else
                        _inventoryItems[id].count += 1;

                    isComplete = true;
                });

            await UniTask.WaitUntil(() => isComplete);
        }

        private UniTask<bool> ConvertationBalance(GetUserInventoryResult result)
        {
            UniTaskCompletionSource<bool> completion = new();
            int countRequest = 0;
            int counterRequest = 0;
            _currency.Clear();
            _currency = result.VirtualCurrency;
            if (_currency.ContainsKey("SC") && _currency["SC"] > 0)
            {
                countRequest++;
                uint currency = (uint)_currency["SC"];
                SceneNetworkContext.Instance.ConvertationUserVirtualCurrency("SC", currency, r => { Debug.Log("Substract currensy for convert"); });
                SceneNetworkContext.Instance.AddUserVirtualCurrency("GC", currency / 100m, r =>
                {
                    _userBalance += currency / 100m;
                    Debug.Log($"Add currensy for convert: {currency / 100m}. New balance = {_userBalance}");
                    counterRequest++;
                    if (counterRequest == countRequest)
                        completion.TrySetResult(true);
                });
            }

            countRequest++;
            SceneNetworkContext.Instance.GetUserPublisherData("currency balanse", result =>
            {
                if (result.isInit && result.Value.Data.ContainsKey("currency balanse"))
                {
                    var oldBalance = new Optionals<CurrencyBalance>(JsonConvert.DeserializeObject<CurrencyBalance>(result.Value.Data["currency balanse"].Value));
                    decimal currency = oldBalance.Value.GC;
                    oldBalance.Value.GC = 0;
                    string json = JsonConvert.SerializeObject(oldBalance.Value);
                    SceneNetworkContext.Instance.UpdateUserPublisherData("currency balanse", json, result => { });
                    SceneNetworkContext.Instance.AddUserVirtualCurrency("GC", currency, r =>
                    {
                        _userBalance += currency;
                        Debug.Log($"Add currensy for convert: {currency}. New balance = {_userBalance}");
                        counterRequest++;
                        if (counterRequest == countRequest)
                            completion.TrySetResult(true);
                    });
                }
                else
                {
                    counterRequest++;
                    if (counterRequest == countRequest)
                        completion.TrySetResult(true);
                }
            });

            return completion.Task;
        }

        private void CheckForGrantItems()
        {
            countReq = 0;
            countAns = 0;
            if (!_inventoryItems.ContainsKey("c_base"))
            {
                countReq++;
                SceneNetworkContext.Instance.GrantItemToUser("c_base", b =>
                {
                    countAns++;
                    if (countAns == countReq)
                        RefreshInventory();
                });
            }
            if (!_inventoryItems.ContainsKey("eq_multitool"))
            {
                countReq++;
                SceneNetworkContext.Instance.GrantItemToUser("eq_multitool", b =>
                {
                    countAns++;
                    if (countAns == countReq)
                        RefreshInventory();
                });
            }
            if (_inventoryItems.ContainsKey("c_base_dragon"))
            {
                countReq++;
                SceneNetworkContext.Instance.RevokeItemFromUser(_inventoryItems["c_base_dragon"].instanceIds.First(), b =>
                //SceneNetworkContext.Instance.GrantItemToUser("c_base_dragon", b =>
                {
                    countAns++;
                    if (countAns == countReq)
                        RefreshInventory();
                });
            }
            if (_inventoryItems.ContainsKey("c_base_tiger"))
            {
                countReq++;
                SceneNetworkContext.Instance.RevokeItemFromUser(_inventoryItems["c_base_tiger"].instanceIds.First(), b =>
                //SceneNetworkContext.Instance.GrantItemToUser("c_base_tiger", b =>
                {
                    countAns++;
                    if (countAns == countReq)
                        RefreshInventory();
                });
            }
            if (!_inventoryItems.ContainsKey("sh_obereg_1"))
            {
                countReq++;
                SceneNetworkContext.Instance.GrantItemToUser("sh_obereg_1", b =>
                {
                    countAns++;
                    if (countAns == countReq)
                        RefreshInventory();
                });
            }
            if (!_inventoryItems.ContainsKey("eq_pistol"))
            {
                countReq++;
                SceneNetworkContext.Instance.GrantItemToUser("eq_pistol", b =>
                {
                    countAns++;
                    if (countAns == countReq)
                        RefreshInventory();
                });
                countReq++;
                SceneNetworkContext.Instance.GrantItemToUser("i_ammo_pack", b =>
                {
                    countAns++;
                    if (countAns == countReq)
                        RefreshInventory();
                });
            }
        }

        public Item GetItemData(string itemId) =>
            _inventoryItems.ContainsKey(itemId) ?
            _inventoryItems[itemId] : new Item(_catalogManager);

        public Item[] GetConsumableDatas(ConsumableType consumableType)
        {
            var items0 = _inventoryItems
                .Where(item =>
                    {
                        var catalogItem = _catalogManager.GetItemData(item.Key);
                        var customData = catalogItem?.CustomData;
                        var result = customData?.Contains("consumableType") ?? false;
                        //Debug.Log($"Check custom data. Name: {catalogItem.DisplayName}. Result: {result}");
                        return result;// _catalogManager.GetItemData(item.Key).CustomData.Contains("consumableType");
                    });
            var items1 = items0
                .Select(item =>
                    {
                        return item.Value;
                    });
            var items2 = items1
                .Where(item =>
                    {
                        ConsumableItem consumableItem = _catalogManager.GetItemData(item.ItemId) as ConsumableItem;
                        var result = consumableItem?.customData.consumableType == consumableType;
                        //Debug.Log($"Check type {consumableItem.customData.consumableType}/{consumableType}. Result: {result}");
                        return result;
                    });
            var consumable = items2.ToArray();
            //Debug.Log($"Finded count: {consumable.Length}");
            if (consumable.Length > 0)
                return consumable;
            return new Item[] { new Item(_catalogManager) };
        }

        public Item[] GetAllItems() =>
            _inventoryItems.Values.ToArray();

        public Item[] GetItemsWithClass(string classItem) =>
            _inventoryItems.Values.Where(item =>
            {
                return item.ItemClass == classItem;
            })
            .ToArray();

        public decimal GetCurrencyData() =>
            _isBlockedGC ? BalanceGC - 5 : BalanceGC;

        public int GetCurrencyVC() =>
            _currency["VC"];

        #region "BufferRaid"
        public void BlockedPayRaid()
        {
            _isBlockedGC = true;
            Translator.Send(PlayerNetworkProtocol.RaidWithGC, new BoolData { value = _isBlockedGC });
        }

        public void UnblockedPayRaid()
        {
            _isBlockedGC = false;
            Translator.Send(PlayerNetworkProtocol.RaidWithGC, new BoolData { value = _isBlockedGC });
        }

        internal void AddItemToBuffer(string id, int count = 1)
        {
            if (!_bufferInventoryItems.ContainsKey(id))
                _bufferInventoryItems.Add(id, count);
            else
                _bufferInventoryItems[id] = _bufferInventoryItems[id] + count;
            Debug.Log($"Add item {id} to buffer. Count {count}");
        }

        internal void AddCurrencyToBuffer(string id, decimal count = 1)
        {
            if (!_bufferCurrency.ContainsKey(id))
                _bufferCurrency.Add(id, count);
            else
                _bufferCurrency[id] = _bufferCurrency[id] + count;
        }

        public Dictionary<string, decimal> GetCurrencyBuffer()
        {
            Dictionary<string, decimal> result = new Dictionary<string, decimal>(_bufferCurrency);
            _bufferCurrency.Clear();
            return result;
        }

        public Dictionary<string, int> GetItemBuffer()
        {
            Dictionary<string, int> result = new Dictionary<string, int>(_bufferInventoryItems);
            _bufferInventoryItems.Clear();
            Debug.Log($"Get item from buffer. Count {result.Count}");
            return result;
        }

        public async void GrantFromBuffer(Dictionary<string, decimal> currencies, Dictionary<string, int> items)
        {
            Debug.Log($"Start grant from buffer");
            await GrantCurrency(currencies);
            await GrantItems(items);
            Debug.Log($"Call refresh inventory");
            RefreshInventory();
        }

        private async Task<bool> GrantCurrency(Dictionary<string, decimal> currencies)
        {
            int count = 0;
            foreach (var currency in currencies)
            {
                SceneNetworkContext.Instance.AddUserVirtualCurrency("GC", currency.Value, b =>
                {
                    count++;
                });
                await Task.Delay(50);
            }

            await UniTask.WaitUntil(() => count == currencies.Count);
            return true;
        }

        public async Task<bool> GrantItems(Dictionary<string, int> items)
        {
            int allCount = 0;
            foreach (var item in items)
                allCount += item.Value;
            int count = 0;
            Debug.Log($"Count grant item {allCount}");
            foreach (var item in items)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    Debug.Log($"Grant item {item.Key}");
                    SceneNetworkContext.Instance.GrantItemToUser(item.Key, b => 
                    {
                        count++;
                        Debug.Log($"Granted {count} items");
                    });
                    await Task.Delay(50);
                }
            }

            await UniTask.WaitUntil(() => count == allCount);
            return true;
        }

        public void DropItemBuffer()
        {
            if (_isBlockedGC)
            {
                SceneNetworkContext.Instance.SubtractUserVirtualCurrency("GC", 5, result =>
                    {
                        _isBlockedGC = false;
                    });
            }
            _bufferCurrency.Clear();
            var keys = _bufferInventoryItems.Keys.ToArray();
            Debug.Log($"Drop item buffer");
            for (int i = 0; i < keys.Length; i++)
            {
                int lose = (int)(_bufferInventoryItems[keys[i]] * 0.65f);
                Debug.Log($"Drop item {keys[i]} from buffer. Count from {_bufferInventoryItems[keys[i]]} to {_bufferInventoryItems[keys[i]] - lose}");
                _bufferInventoryItems[keys[i]] = _bufferInventoryItems[keys[i]] - lose;
                var uiSlot = InventoryScreenManager.Instance.Inventory.GetUISlot(keys[i]);
                for (int j = 0; j < lose; j++)
                {
                    InventoryScreenManager.Instance.UseItem(uiSlot);
                }
            }
        }
        #endregion "Buffer"

        #region "Rafinery"
        private void GetRafineryDataRequest(GetUserDataResult result)
        {
            var data = result.Data;
            if (data.ContainsKey("rafineryDatas"))
                _rafineryDatas = JsonConvert.DeserializeObject<List<RafineryData>>(data["rafineryDatas"].Value);
        }

        public void SendForRecycling(string itemID, int count, Action<bool> onCallback = default)
        {
            _rafineryDatas.Add(new RafineryData { itemId = itemID, count = count, startRafinering = DateTime.UtcNow });
            string rafineryJson = JsonConvert.SerializeObject(_rafineryDatas);
            SceneNetworkContext.Instance.UpdateUserPublisherData("rafineryDatas", rafineryJson, onCallback);
        }

        public void GetFromRecycling(RafineryData data, Action<bool> onCallback = default)
        {
            _rafineryDatas.Remove(data);
            string rafineryJson = JsonConvert.SerializeObject(_rafineryDatas);
            SceneNetworkContext.Instance.UpdateUserPublisherData("rafineryDatas", rafineryJson, onCallback);
        }

        public void SellItems(string itemInstanceId, decimal countCurrency, Action<bool> onCallback)
        {
            SceneNetworkContext.Instance.RevokeItemFromUser(
                itemInstanceId,
                result =>
                {
                    if (result)
                        SceneNetworkContext.Instance.AddUserVirtualCurrency(
                            "GC",
                            countCurrency, result =>
                            {
                                _userBalance += countCurrency;
                                onCallback?.Invoke(result);
                            });
                    else
                        onCallback?.Invoke(result);
                });
        }

        public void BuyItems(string itemInstanceId, decimal countCurrency, Action<bool> onCallback)
        {
            SceneNetworkContext.Instance.SubtractUserVirtualCurrency(
                "GC", countCurrency,
                result =>
                {
                    if (result)
                        SceneNetworkContext.Instance.GrantItemToUser(
                            itemInstanceId,
                            result =>
                            {
                                onCallback?.Invoke(result);
                            });
                    else
                        onCallback?.Invoke(result);
                });
        }

        public void BuyItemsV2(string itemId, int countItems , decimal countCurrency, Action<bool> onCallback)
        {
            SceneNetworkContext.Instance.SubtractUserVirtualCurrency(
                "GC", countCurrency,
                async result =>
                {
                    if (result)
                    {
                        if (_catalogManager.GetItemData(itemId) == null)
                            await _catalogManager.CashingItem(itemId);
                        var item = _catalogManager.GetItemData(itemId);
                        SceneNetworkContext.Instance.AddUserVirtualCurrency(
                            $"SFT_{item.DisplayName}_" + itemId, countItems,
                            result =>
                            {
                                _userBalance -= countCurrency;
                                if (_inventoryItems.ContainsKey(item.ItemId))
                                {
                                    _inventoryItems[item.ItemId].count += countItems;
                                }
                                else
                                {
                                    var inventoryItem = new InventoryItem();
                                    inventoryItem.Id = item.catalogItem2.Value.Id;
                                    inventoryItem.DisplayProperties = item.catalogItem2.Value.DisplayProperties;
                                    _inventoryItems.Add(inventoryItem.Id, new Item(_catalogManager, inventoryItem, countItems));
                                }
                                //_inventoryItems[itemId].count += countItems;
                                onCallback?.Invoke(result);
                            });
                    }
                    else
                        onCallback?.Invoke(result);
                });
        }

        private void CheckSubstractItem()
        {
            foreach (var data in _rafineryDatas)
            {
                if (_inventoryItems.ContainsKey(data.itemId))
                    _inventoryItems[data.itemId].count -= data.count;
            }
        }

        public List<RafineryData> GetRafineryData() =>
            _rafineryDatas;
        #endregion "Rafinery"
    }
}
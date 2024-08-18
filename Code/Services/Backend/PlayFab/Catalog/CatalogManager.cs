using Cysharp.Threading.Tasks;
using GrabCoin.GameWorld.Player;
using GrabCoin.Model;
using GrabCoin.UI.Screens;
using Mirror;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.EconomyModels;
using PlayFabCatalog;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using CatalogItem = PlayFab.EconomyModels.CatalogItem;

namespace GrabCoin.Services.Backend.Catalog
{
    public class CatalogManager
    {
        private const string CURRENT_VERSION_KEY = "CURRENT_VERSION_KEY";

        private string[] _catalogNames = new string[]
        {
            "Resources_v1",
            "Consumable_v1",
            "Enemies_v1",
            "Equipment_v1",
            "Upgrade_v1",
            "Characters_v1",
            "Shields_v1"
        };
        private bool _isInit;
        private bool _isFinishedInit;
        private int _countRequest;
        private int _counterRequest;
        private int _currentVersion;

        [Inject] private ItemsConfig _itemsConfig;
        private Dictionary<string, ResourceItem> _resourceItems = new();
        private Dictionary<string, ConsumableItem> _consumableItems = new();
        private Dictionary<string, EnemyItem> _enemyItems = new();
        private Dictionary<string, EquipmentItem> _equipmentItems = new();
        private Dictionary<string, UpgradeItem> _upgradeItems = new();
        private Dictionary<string, CharacterItem> _characterItems = new();
        private Dictionary<string, ShieldItem> _shieldItems = new();

        private Dictionary<string, CatalogPriceOptions> _storeItems = new();

        private List<StoreItem> _storeBuyLabItems = new();
        private List<StoreItem> _storeBuyEquipItems = new();
        private List<StoreItem> _storeBuyUpgradeItems = new();

        #region "Fields Catalog V2"
        private GetEntityTokenResponse gameTokenRequest;
        private PlayFabAuthenticationContext gameAuthContext;

        private Dictionary<string, CatalogItem> _catalogItems = new();
        #endregion "Fields Catalog V2"

        public void Initialize()
        {
            if (_isInit) return;
            _isInit = true;

            PlayerPrefs.SetInt(CURRENT_VERSION_KEY, -1);
            _currentVersion = -1;
            //GetCatalogs();
            CheckRefreshCatalog();
            GetEntityToken();
        }

        private void GetVersionSuccess(GetCatalogItemsResult result)
        {
            int version = Int32.Parse(result.Catalog[0].CustomData);
            //int currentVersion = PlayerPrefs.GetInt(CURRENT_VERSION_KEY, -1);
            if (version > _currentVersion)
            {
                Debug.Log("Take new version catalog");
                //PlayerPrefs.SetInt(CURRENT_VERSION_KEY, version);
                _currentVersion = version;
                GetCatalogs();
            }
        }

        #region "Catalog V2"
        private void GetEntityToken()
        {
            PlayFabAuthenticationAPI.GetEntityToken(new GetEntityTokenRequest()
                , result =>
                {
                    gameTokenRequest = result;
                    gameAuthContext = new PlayFabAuthenticationContext
                    { EntityToken = gameTokenRequest.EntityToken };

                    //GetCatalogV2();
                }, error => Debug.LogError(error.GenerateErrorReport()));
        }

        private void GetStoreItems(string storeId, string continuationToken = "")
        {
            PlayFabEconomyAPI.SearchItems(new SearchItemsRequest
            {
                AuthenticationContext = gameAuthContext,
                Count = 50,
                ContinuationToken = continuationToken,
                Store = new StoreReference { AlternateId = new CatalogAlternateId { Type = "FriendlyId", Value = storeId } }
            }, async result =>
            {
                var storeItems = result.Items.OrderBy(i => i.Title.First());
                foreach (var storeItem in result.Items)
                {
                    if (_storeItems.ContainsKey(storeItem.Id))
                        _storeItems[storeItem.Id] = storeItem.PriceOptions;
                    else
                        _storeItems.Add(storeItem.Id, storeItem.PriceOptions);


                    if (GetItemData(storeItem.Id) == null)
                        await CashingItem(storeItem.Id);

                    foreach (var price in storeItem.PriceOptions.Prices)
                        foreach (var priceAmount in price.Amounts)
                        {
                            if (GetItemData(priceAmount.ItemId) == null)
                                await CashingItem(priceAmount.ItemId);
                            //Debug.Log($"Item \"{GetItemData(storeItem.Id).DisplayName}\" prices: {priceAmount.Amount} {GetItemData(priceAmount.ItemId).DisplayName}");
                        }
                }
                if (!string.IsNullOrEmpty(result.ContinuationToken))
                {
                    await UniTask.Delay(300);
                    GetCatalogPages(result.ContinuationToken);
                }
                else
                {
                    Debug.Log("End catalog");
                }
            }, error => Debug.LogError(error.GenerateErrorReport()));
        }

#region "Get all items V2"
#if UNITY_EDITOR
        private List<CatalogItem> _characters = new();
#endif
        private void GetCatalogV2()
        {
            Debug.Log("Get items");
            GetCatalogPages("");
        }

        private void GetCatalogPages(string continuationToken)
        {
            PlayFabEconomyAPI.SearchItems(new SearchItemsRequest
            {
                AuthenticationContext = gameAuthContext,
                Count = 50,
                ContinuationToken = continuationToken,
                Store = new StoreReference { AlternateId = new CatalogAlternateId { Type = "FriendlyId", Value = "s_laboratory" } }
            }, async result =>
            {
                foreach (var storeItem in result.Items)
                {
                    if (_storeItems.ContainsKey(storeItem.Id))
                        _storeItems[storeItem.Id] = storeItem.PriceOptions;
                    else
                        _storeItems.Add(storeItem.Id, storeItem.PriceOptions);
                }
                return;
                Debug.Log("Get page catalog:");
                var items = result.Items;
                foreach (CatalogItem item in items.Where(item => item.ContentType == "Character").ToList())
                {
#if UNITY_EDITOR
                    _characters.Add(item);
#endif
                    ParseItemV2(item);
                }
                //for (int i = 0; i < items.Count; i++)
                //    Debug.Log(items[i].Title.First().Value);
                if (!string.IsNullOrEmpty(result.ContinuationToken))
                {
                    await UniTask.Delay(300);
                    GetCatalogPages(result.ContinuationToken);
                }
                else
                {
                    Debug.Log("End catalog");
#if UNITY_EDITOR
                    Debug.Log($"Count character tokens: {_characters.Count}");
#endif
                }
            }, error => Debug.LogError(error.GenerateErrorReport()));
        }
#endregion "Get all items V2"

        internal UniTask<bool> CashingItem(string id)
        {
            //Debug.Log($"Start cashing item: {id}");
            UniTaskCompletionSource<bool> completion = new();
            PlayFabEconomyAPI.GetItem(new GetItemRequest
            {
                AuthenticationContext = gameAuthContext,
                Id = id
            },
            result =>
            {
                //Debug.Log($"Finish cashing item: {id}");
                if (result.Item.ContentType == "Consumable")
                    GetCatalogPages("");

                ParseItemV2(result.Item);
                completion.TrySetResult(true);
            }, Debug.LogError);
            return completion.Task;
        }

        private void ParseItemV2(CatalogItem catalogItem)
        {
            ItemData itemData = CreateItemEntity(catalogItem);
            string itemId = catalogItem.Id;

            itemData.name = catalogItem.Title.First().Value;
            itemData.catalogItem2 = new Optionals<CatalogItem>(catalogItem);
            itemData.itemConfig = _itemsConfig.GetItemViewFromName(catalogItem.Title.First().Value);

            switch (itemData)
            {
                case ResourceItem resource:
                    resource.customData = string.IsNullOrEmpty(catalogItem.DisplayProperties.ToString()) ?
                        new ResourcesCustomData() :
                        JsonConvert.DeserializeObject<ResourcesCustomData>(catalogItem.DisplayProperties.ToString());

                    if (_resourceItems.ContainsKey(itemId))
                        _resourceItems[itemId] = resource;
                    else
                        _resourceItems.Add(itemId, resource);
                    break;
                case ConsumableItem consumable:
                    consumable.customData = string.IsNullOrEmpty(catalogItem.DisplayProperties.ToString()) ?
                        new ConsumableCustomData() :
                        JsonConvert.DeserializeObject<ConsumableCustomData>(catalogItem.DisplayProperties.ToString());

                    if (_consumableItems.ContainsKey(itemId))
                        _consumableItems[itemId] = consumable;
                    else
                        _consumableItems.Add(itemId, consumable);
                    break;
                case EnemyItem enemy:
                    enemy.customData = string.IsNullOrEmpty(catalogItem.DisplayProperties.ToString()) ?
                        new EnemyCustomData() :
                        JsonConvert.DeserializeObject<EnemyCustomData>(catalogItem.DisplayProperties.ToString());
                    
                    if (_enemyItems.ContainsKey(itemId))
                        _enemyItems[itemId] = enemy;
                    else
                        _enemyItems.Add(itemId, enemy);
                    break;
                case EquipmentItem equipment:
                    equipment.customData = string.IsNullOrEmpty(catalogItem.DisplayProperties.ToString()) ?
                        new EquipmentCustomData() :
                        JsonConvert.DeserializeObject<EquipmentCustomData>(catalogItem.DisplayProperties.ToString());
                    
                    if (_equipmentItems.ContainsKey(itemId))
                        _equipmentItems[itemId] = equipment;
                    else
                        _equipmentItems.Add(itemId, equipment);
                    break;
                case UpgradeItem upgrade:
                    upgrade.customData = string.IsNullOrEmpty(catalogItem.DisplayProperties.ToString()) ?
                        new UpgradeCustomData() :
                        JsonConvert.DeserializeObject<UpgradeCustomData>(catalogItem.DisplayProperties.ToString());
                    
                    if (_upgradeItems.ContainsKey(itemId))
                        _upgradeItems[itemId] = upgrade;
                    else
                        _upgradeItems.Add(itemId, upgrade);
                    break;
                case CharacterItem character:
                    character.customData = string.IsNullOrEmpty(catalogItem.DisplayProperties.ToString()) ?
                        new CharacterCustomData() :
                        JsonConvert.DeserializeObject<CharacterCustomData>(catalogItem.DisplayProperties.ToString());
                    
                    if (_characterItems.ContainsKey(itemId))
                        _characterItems[itemId] = character;
                    else
                        _characterItems.Add(itemId, character);
                    break;
                case ShieldItem shield:
                    shield.customData = string.IsNullOrEmpty(catalogItem.DisplayProperties.ToString()) ?
                        new ShieldCustomData() :
                        JsonConvert.DeserializeObject<ShieldCustomData>(catalogItem.DisplayProperties.ToString());
                    
                    if (_shieldItems.ContainsKey(itemId))
                        _shieldItems[itemId] = shield;
                    else
                        _shieldItems.Add(itemId, shield);
                    break;
            }
        }

        private static ItemData CreateItemEntity(CatalogItem catalogItem)
        {
            ItemData itemData = catalogItem.ContentType switch
            {
                "Ore" => new ResourceItem(),
                "Currency" => new ConsumableItem(), // Тут латинская C
                "Consumable" => new ConsumableItem(), // Тут латинская C
                "Сonsumable" => new ConsumableItem(), // Тут кириллическая эС, костыль
                "Enemy" => new EnemyItem(),
                "Weapon" => new EquipmentItem(),
                "Upgrade" => new UpgradeItem(),
                "Character" => new CharacterItem(),
                "Shield" => new ShieldItem(),
            };
            return itemData;
        }
        #endregion "Catalog V2"

        #region "Catalog V1"
        private void GetCatalogs()
        {
            _isFinishedInit = false;
            _countRequest = _catalogNames.Length; // 3;
            _counterRequest = 0;

            foreach (var catalogName in _catalogNames)
            {
                var request = new GetCatalogItemsRequest { CatalogVersion = catalogName };
                PlayFabClientAPI.GetCatalogItems(request, Success, Debug.LogError);
            }

            var storeRequest1 = new GetStoreItemsRequest { CatalogVersion = "Consumable_v1", StoreId = "s_laboratory" };
            PlayFabClientAPI.GetStoreItems(storeRequest1, StoreSuccess, Debug.LogError);
            var storeRequest2 = new GetStoreItemsRequest { CatalogVersion = "Upgrade_v1", StoreId = "s_upgrade" };
            PlayFabClientAPI.GetStoreItems(storeRequest2, StoreSuccess, Debug.LogError);
            var storeRequest3 = new GetStoreItemsRequest { CatalogVersion = "Equipment_v1", StoreId = "s_workshop" };
            PlayFabClientAPI.GetStoreItems(storeRequest3, StoreSuccess, Debug.LogError);

            GetStoreItems("s_laboratory");
        }

        private void StoreSuccess(GetStoreItemsResult result)
        {
            switch (result.StoreId)
            {
                case "s_laboratory": _storeBuyLabItems = result.Store; break;
                case "s_upgrade": _storeBuyUpgradeItems = result.Store; break;
                case "s_workshop": _storeBuyEquipItems = result.Store; break;
            }
        }

        private void Success(GetCatalogItemsResult result)
        {
            foreach (var item in result.Catalog)
            {
                var jsonResult = JsonConvert.SerializeObject(item);
                var catalogData = JsonConvert.DeserializeObject<PlayFabCatalog.Catalog>(jsonResult);
                if (item.ItemClass.Contains("С"))
                    Debug.Log($"CatalogManager.Success(): item.ItemClass=\"{item.ItemClass.ToString()}\"");
                ItemData itemData = item.ItemClass switch
                {
                    "Ore" => new ResourceItem(),
                    "Consumable" => new ConsumableItem(), // Тут латинская C
                    "Сonsumable" => new ConsumableItem(), // Тут кириллическая эС, костыль
                    "Enemy" => new EnemyItem(),
                    "Equipment" => new EquipmentItem(),
                    "Upgrade" => new UpgradeItem(),
                    "Character" => new CharacterItem(),
                    "Shield" => new ShieldItem(),
                };

                itemData.name = item.DisplayName;
                itemData.catalogItem = new Optionals<PlayFabCatalog.Catalog>(catalogData);
                itemData.itemConfig = _itemsConfig.Contains(item.ItemId) ? _itemsConfig.GetItemView(item.ItemId) : new ItemView { Key = item.ItemId };

                switch (itemData)
                {
                    case ResourceItem resource:
                        resource.customData = string.IsNullOrEmpty(item.CustomData) ? new ResourcesCustomData() : JsonConvert.DeserializeObject<ResourcesCustomData>(item.CustomData);
                        if (_resourceItems.ContainsKey(item.ItemId))
                            _resourceItems[item.ItemId] = resource;
                        else
                            _resourceItems.Add(item.ItemId, resource);
                        break;
                    case ConsumableItem consumable:
                        consumable.customData = string.IsNullOrEmpty(item.CustomData) ? new ConsumableCustomData() : JsonConvert.DeserializeObject<ConsumableCustomData>(item.CustomData);
                        if (_consumableItems.ContainsKey(item.ItemId))
                            _consumableItems[item.ItemId] = consumable;
                        else
                            _consumableItems.Add(item.ItemId, consumable);
                        break;
                    case EnemyItem enemy:
                        enemy.customData = string.IsNullOrEmpty(item.CustomData) ? new EnemyCustomData() : JsonConvert.DeserializeObject<EnemyCustomData>(item.CustomData);
                        if (_enemyItems.ContainsKey(item.ItemId))
                            _enemyItems[item.ItemId] = enemy;
                        else
                            _enemyItems.Add(item.ItemId, enemy);
                        break;
                    case EquipmentItem equipment:
                        equipment.customData = string.IsNullOrEmpty(item.CustomData) ? new EquipmentCustomData() : JsonConvert.DeserializeObject<EquipmentCustomData>(item.CustomData);
                        if (_equipmentItems.ContainsKey(item.ItemId))
                            _equipmentItems[item.ItemId] = equipment;
                        else
                            _equipmentItems.Add(item.ItemId, equipment);
                        break;
                    case UpgradeItem upgrade:
                        upgrade.customData = string.IsNullOrEmpty(item.CustomData) ? new UpgradeCustomData() : JsonConvert.DeserializeObject<UpgradeCustomData>(item.CustomData);
                        if (_upgradeItems.ContainsKey(item.ItemId))
                            _upgradeItems[item.ItemId] = upgrade;
                        else
                            _upgradeItems.Add(item.ItemId, upgrade);
                        break;
                    case CharacterItem character:
                        character.customData = string.IsNullOrEmpty(item.CustomData) ? new CharacterCustomData() : JsonConvert.DeserializeObject<CharacterCustomData>(item.CustomData);
                        if (_characterItems.ContainsKey(item.ItemId))
                            _characterItems[item.ItemId] = character;
                        else
                            _characterItems.Add(item.ItemId, character);
                        break;
                    case ShieldItem shield:
                        shield.customData = string.IsNullOrEmpty(item.CustomData) ? new ShieldCustomData() : JsonConvert.DeserializeObject<ShieldCustomData>(item.CustomData);
                        if (_shieldItems.ContainsKey(item.ItemId))
                            _shieldItems[item.ItemId] = shield;
                        else
                            _shieldItems.Add(item.ItemId, shield);
                        break;
                }
            }
            _counterRequest++;
            _isFinishedInit = _counterRequest >= _countRequest;
            if (_isFinishedInit)
            {
                Translator.Send(GeneralProtocol.RefreshCatalogData);
            }
        }

        private async void CheckRefreshCatalog()
        {
            while (true)
            {
                var catalogRequest = new GetCatalogItemsRequest { CatalogVersion = "version" };
                if (PlayFabSettings.staticPlayer.IsClientLoggedIn())
                    PlayFabClientAPI.GetCatalogItems(catalogRequest, GetVersionSuccess, Debug.LogError);
                await UniTask.Delay(60000);
            }
        }

        public ItemData GetItemData(string catalogId, string classItem) =>
            classItem switch
            {
                "Ore" => _resourceItems[catalogId],
                "Consumable" => _consumableItems[catalogId], //латинская
                "Сonsumable" => _consumableItems[catalogId], //кириллица
                "Enemy" => _enemyItems[catalogId],
                "Equipment" => _equipmentItems[catalogId],
                "Upgrade" => _upgradeItems[catalogId],
                "Character" => _characterItems[catalogId],
                "Shield" => _shieldItems[catalogId],
            };
        #endregion "Catalog V1"

        public ItemData GetItemData(string catalogId)
        {
            if (_resourceItems.ContainsKey(catalogId))
                return _resourceItems[catalogId];
            else if (_consumableItems.ContainsKey(catalogId))
                return _consumableItems[catalogId];
            else if (_enemyItems.ContainsKey(catalogId))
                return _enemyItems[catalogId];
            else if (_equipmentItems.ContainsKey(catalogId))
                return _equipmentItems[catalogId];
            else if (_upgradeItems.ContainsKey(catalogId))
                return _upgradeItems[catalogId];
            else if (_characterItems.ContainsKey(catalogId))
                return _characterItems[catalogId];
            else if (_shieldItems.ContainsKey(catalogId))
                return _shieldItems[catalogId];
            else
                return null;
        }

        public ResourceItem GetResourceData(string catalogId) =>
            _resourceItems.ContainsKey(catalogId) ?
                (_resourceItems[catalogId] ?? new()) :
                new();

        public EnemyItem GetEnemyData(string catalogId) =>
            _enemyItems.ContainsKey(catalogId) ?
                (_enemyItems[catalogId] ?? new()) :
                new();

        public ConsumableItem GetConsumableData(string catalogId) =>
            _consumableItems.ContainsKey(catalogId) ?
                (_consumableItems[catalogId] ?? new()) :
                new();

        public List<StoreItem> GetBuyLabItems() =>
            _storeBuyLabItems;

        public Dictionary<string, CatalogPriceOptions> GetBuyLabItemsV2() =>
            _storeItems;

        public List<StoreItem> GetBuyEquipItems() =>
            _storeBuyEquipItems;

        public List<StoreItem> GetBuyUpgradeItems() =>
            _storeBuyUpgradeItems;

        public async UniTask<bool> WaitInitialize()
        {
            await UniTask.WaitUntil(() => _isFinishedInit);
            return true;
        }
    }
}
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
#if ENABLE_PLAYFABSERVER_API
using PlayFab.ServerModels;
#endif
using NaughtyAttributes;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.IO;
using GrabCoin.AIBehaviour;
using GrabCoin.UI.Screens;
using static PlayFabCatalog.AddedCustomDataInPlayFabItems;
using GrabCoin.AIBehaviour.FSM;
using PlayFab.SharedModels;
using PlayFab.AuthenticationModels;
using System.Threading.Tasks;
using Code.Services.AuthService;
using Zenject;
using PlayFab.EconomyModels;
using EntityKey = PlayFab.EconomyModels.EntityKey;
using CatalogItem = PlayFab.EconomyModels.CatalogItem;

namespace PlayFabCatalog
{
    public class AddedCustomDataInPlayFabItems : MonoBehaviour
    {
        public enum RareType
        {
            Normal,
            Rare,
            Unic
        }

        public enum EquipmentType
        {
            Weapon,
            Shield,
            LaserWeapon
        }

        public enum CharacterType
        {
            Ilon,
            Dragon,
            Tiger
        }

        public enum CharacterGender
        {
            Female,
            Male
        }

        public enum ConsumableType
        {
            Ammo,
            Health,
            Battery,
            Empty,
            LaserAmmo
        }

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

        [SerializeField] private ItemsConfig _itemsConfig;
        [SerializeField] private List<ResourceItem> _resourceItems = new();
        [SerializeField] private List<ConsumableItem> _consumableItems = new();
        [SerializeField] private List<EnemyItem> _enemyItems = new();
        [SerializeField] private List<EquipmentItem> _equipmentItems = new();
        [SerializeField] private List<ShieldItem> _shieldItems = new();
        [SerializeField] private List<UpgradeItem> _upgradeItems = new();
        [SerializeField] private List<CharacterItem> _characterItems = new();


        [Inject] private EmailAuthService _emailAuthService;

        private string systemGUID;
        private GetEntityTokenResponse gameTokenRequest;
        private PlayFabAuthenticationContext gameAuthContext;
        private EntityKey gameEntityKey;
        private CreateDraftItemResponse gameDraftItemResponse;

        async void Start()
        {
            gameEntityKey = new()
            {
                Type = "title",
                Id = PlayFabSettings.staticSettings.TitleId
            };
#if ENABLE_PLAYFABSERVER_API
            string email = "info@grabcoinclub.com";
            int hash = email.GetHashCode();
            if (hash < 0)
                hash *= -1;
            _emailAuthService.FillingData(hash.ToString(), email, "cfgkhjesru645vcbnhbjtr");
            var result = await _emailAuthService.SignIn();
            if (result)
                PlayFabEconomyv2QuickStart();
            else
            {
                Debug.LogError("Custom: Not auth");
                Start();
            }
            //foreach (var catalogName in _catalogNames)
            //{
            //    var catalogRequest = new GetCatalogItemsRequest { CatalogVersion = catalogName };
            //    PlayFabServerAPI.GetCatalogItems(catalogRequest, Success, Debug.LogError);
            //}
#endif
        }

        #region "Catalog V1"
        [Button("Create Json file")]
        private void CreateJson()
        {
            //foreach (var catalogName in _catalogNames)
            //{
            //    var list = GetListItems(catalogName);
            //    foreach (var item in list)
            //    {
            //        item.catalogItem.CustomData = item.ToJsonCustomData();
            //        _itemsConfig.AddOrSet(item.catalogItem.ItemId, item.itemConfig);
            //    }
            //    SaveCatalog(catalogName, list.Select(res => res.catalogItem).ToList());
            //}
            //Debug.Log("Complete save datas");
        }

        private List<ItemData> GetListItems(string name) =>
            new List<ItemData>(name switch
            {
                "Resources_v1" => _resourceItems,
                "Consumable_v1" => _consumableItems,
                "Enemies_v1" => _enemyItems,
                "Equipment_v1" => _equipmentItems,
                "Upgrade_v1" => _upgradeItems,
                "Characters_v1" => _characterItems,
                "Shields_v1" => _shieldItems,
            });

#if ENABLE_PLAYFABSERVER_API
        private void Success(GetCatalogItemsResult result)
        {
            foreach (var item in result.Catalog)
            {
                var jsonResult = JsonConvert.SerializeObject(item);
                var catalogData = JsonConvert.DeserializeObject<Catalog>(jsonResult);
                ItemData itemData = item.ItemClass switch
                {
                    "Ore" => new ResourceItem(),
                    "Consumable" => new ConsumableItem(),
                    "Enemy" => new EnemyItem(),
                    "Equipment" => new EquipmentItem(),
                    "Upgrade" => new UpgradeItem(),
                    "Character" => new CharacterItem(),
                    "Shield" => new ShieldItem(),
                };

                itemData.name = item.DisplayName;
                //itemData.catalogItem = catalogData;
                itemData.itemConfig = _itemsConfig.Contains(item.ItemId) ? _itemsConfig.GetItemView(item.ItemId) : new ItemView { Key = item.ItemId, name = item.DisplayName };
                itemData.itemConfig.name = item.DisplayName;

                switch (itemData)
                {
                    case ResourceItem resource:
                        resource.customData = string.IsNullOrEmpty(item.CustomData) ? new ResourcesCustomData() : JsonConvert.DeserializeObject<ResourcesCustomData>(item.CustomData);
                        _resourceItems.Add(resource);
                        break;
                    case ConsumableItem consumable:
                        consumable.customData = string.IsNullOrEmpty(item.CustomData) ? new ConsumableCustomData() : JsonConvert.DeserializeObject<ConsumableCustomData>(item.CustomData);
                        _consumableItems.Add(consumable);
                        break;
                    case EnemyItem enemy:
                        enemy.customData = string.IsNullOrEmpty(item.CustomData) ? new EnemyCustomData() : JsonConvert.DeserializeObject<EnemyCustomData>(item.CustomData);
                        _enemyItems.Add(enemy);
                        break;
                    case EquipmentItem equipment:
                        equipment.customData = string.IsNullOrEmpty(item.CustomData) ? new EquipmentCustomData() : JsonConvert.DeserializeObject<EquipmentCustomData>(item.CustomData);
                        _equipmentItems.Add(equipment);
                        break;
                    case UpgradeItem upgrade:
                        upgrade.customData = string.IsNullOrEmpty(item.CustomData) ? new UpgradeCustomData() : JsonConvert.DeserializeObject<UpgradeCustomData>(item.CustomData);
                        _upgradeItems.Add(upgrade);
                        break;
                    case CharacterItem character:
                        character.customData = string.IsNullOrEmpty(item.CustomData) ? new CharacterCustomData() : JsonConvert.DeserializeObject<CharacterCustomData>(item.CustomData);
                        _characterItems.Add(character);
                        break;
                    case ShieldItem shield:
                        shield.customData = string.IsNullOrEmpty(item.CustomData) ? new ShieldCustomData() : JsonConvert.DeserializeObject<ShieldCustomData>(item.CustomData);
                        _shieldItems.Add(shield);
                        break;
                }
            }
        }
#endif

        #endregion "Catalog V1"
        #region "Catalog V2"
        // ENABLE_PLAYFABSERVER_API symbol denotes this is an admin-level game server and not a game client.
        private void PlayFabEconomyv2QuickStart()
        {
            systemGUID = Guid.NewGuid().ToString(); //Environment.GetEnvironmentVariable("SYSTEM_GUID", EnvironmentVariableTarget.Process);
            // FFESER6MZ6K4NAWIDRCB9XYGGQ65OHW9FS9IMRJZUDJQQOU793

            //gameEntityKey = new()
            //{
            //    Type = "title",
            //    Id = PlayFabSettings.staticSettings.TitleId
            //};

            try
            {
                /*gameTokenRequest = await*/
                PlayFabAuthenticationAPI.GetEntityToken(new GetEntityTokenRequest()
                , result =>
                {
                    gameTokenRequest = result;

                    gameAuthContext = new()
                    {
                        EntityToken = gameTokenRequest.EntityToken,
                    };

                    PlayFabEconomyAPI.SearchItems(new SearchItemsRequest
                    {
                        AuthenticationContext = gameAuthContext
                    }, result =>
                    {
                        Debug.Log("Get items");
                        var items = result.Items;
                        for (int i = 0; i < items.Count; i++)
                            Debug.Log(items[i].Title.First().Value);
                    }, error => Debug.LogError(error.GenerateErrorReport()));

                }, error =>
                {
                    throw new Exception(error.GenerateErrorReport());
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("PlayFab Auth Error: {0}", e));
                return;
            }
        }

#if ENABLE_PLAYFABSERVER_API
        private void PlayFabEconomyv2CreateDraftItem()
        {
            // Continued from above example...
            CreateDraftItemRequest gameFireItem = new()
            {
                //AuthenticationContext = gameAuthContext,
                Item = new PlayFab.EconomyModels.CatalogItem()
                {
                    CreatorEntity = gameEntityKey,
                    Type = "catalogItem", // bundle, catalogItem, currency, store, ugc, subscription
                    ContentType = "Character",
                    Title = new Dictionary<string, string>
                    {
                        { "NEUTRAL", "My Amazing Metall Sword" },
                        { "en-US", "My Lit Lit Sword" },
                        { "ru-RU", "Мой префосходный огненный меч" },
                    },
                    AlternateIds = new List<CatalogAlternateId>
                    {
                        new CatalogAlternateId{ Type = "FriendlyId", Value = "fire_sword" },
                        //{ "en-US", "My Lit Lit Sword" },
                        //{ "ru-RU", "Мой префосходный огненный меч" },
                    },
                    StartDate = DateTime.UtcNow,
                    Tags = new List<string>
                    {
                      "Character"
                    }
                },
                Publish = true,
                //CustomTags = new Dictionary<string, string>
                //{
                //    { "server", systemGUID }
                //}
            };

            try
            {
                PlayFabEconomyAPI.CreateDraftItem(gameFireItem,
                    result =>
                    {
                        gameDraftItemResponse = result;
                        Console.WriteLine(string.Format("PlayFab CreateDraftItem Success: {0}",
                            JsonConvert.SerializeObject(result, Formatting.Indented)));
                    },
                    error =>
                    {
                        throw new Exception(error.GenerateErrorReport());
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("PlayFab CreateDraftItem Error: {0}", e));
                return;
            }

        }
#endif
        #endregion "Catalog V2"

        #region "SaveToFile"
        private void SaveCatalog(string name, List<Catalog> catalogs)
        {
            var root = new Root();
            root.CatalogVersion = name;
            root.Catalog = catalogs;
            root.DropTables = new();

            var jsonResult = JsonConvert.SerializeObject(root);
            var path = Path.Combine(Application.streamingAssetsPath, $"title-F080A-{name}.json");
            WriteToFile(path, jsonResult);
        }

        private bool WriteToFile(string filePath, string fileContents)
        {
            if (!File.Exists(filePath))
                File.Create(filePath).Close();

            try
            {
                File.WriteAllText(filePath, fileContents);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to {filePath} with exception {e}");
                return false;
            }
        }
        #endregion "SaveToFile"
    }

    #region "Items"
    public abstract class ItemData
    {
        public string name;
        public Optionals<Catalog> catalogItem;
        public Optionals<CatalogItem> catalogItem2;
        public ItemView itemConfig;

        public abstract string ToJsonCustomData();
        public abstract float GetWeight();

        public string ItemId =>
            catalogItem.isInit ?
                catalogItem.Value.ItemId :
                catalogItem2.isInit ?
                    catalogItem2.Value.Id :
                    //catalogItem2.Value.AlternateIds.FirstOrDefault(id => id.Type == "FriendlyId")?.Value ?? "" :
                    "";

        public string ItemClass =>
            catalogItem.isInit ?
                catalogItem.Value.ItemClass :
                catalogItem2.isInit ?
                    catalogItem2.Value.ContentType :
                    "";

        public string DisplayName =>
            catalogItem.isInit ?
                catalogItem.Value.DisplayName :
                catalogItem2.isInit ?
                    catalogItem2.Value.Title.First().Value :
                    "";

        public string Description =>
            catalogItem.isInit ?
                catalogItem.Value.Description :
                catalogItem2.isInit ?
                    catalogItem2.Value.Description.Count > 0 ?
                        catalogItem2.Value.Description.First().Value :
                        "" : 
                    "";

        public string CustomData =>
            catalogItem.isInit ?
                catalogItem.Value.CustomData :
                catalogItem2.isInit ?
                    catalogItem2.Value.DisplayProperties.ToString() :
                    "";

        public List<string> Tags =>
            catalogItem.isInit ?
                catalogItem.Value.Tags :
                catalogItem2.isInit ?
                    catalogItem2.Value.Tags :
                    new();

        public VirtualCurrencyPrices VirtualCurrencyPrices =>
            catalogItem.isInit ?
                catalogItem.Value.VirtualCurrencyPrices :
                new();
    }

    [Serializable]
    public class ResourceItem : ItemData
    {
        public ResourcesCustomData customData;

        public override float GetWeight() =>
            customData.weight;

        public override string ToJsonCustomData() =>
            customData.ToJson();
    }

    [Serializable]
    public class ConsumableItem : ItemData
    {
        public ConsumableCustomData customData;

        public override float GetWeight() =>
            customData.weight;

        public override string ToJsonCustomData() =>
            customData.ToJson();
    }

    [Serializable]
    public class EnemyItem : ItemData
    {
        public EnemyCustomData customData;
        [HideInInspector] public EnemyGraph behaviour;

        public override float GetWeight() =>
            0;

        public override string ToJsonCustomData() =>
            customData.ToJson();
    }

    [Serializable]
    public class EquipmentItem : ItemData
    {
        public EquipmentCustomData customData;

        public override float GetWeight() =>
            customData.weight;

        public override string ToJsonCustomData() =>
            customData.ToJson();
    }

    [Serializable]
    public class ShieldItem : ItemData
    {
        public ShieldCustomData customData;

        public override float GetWeight() =>
            customData.weight;

        public override string ToJsonCustomData() =>
            customData.ToJson();
    }

    [Serializable]
    public class UpgradeItem : ItemData
    {
        public UpgradeCustomData customData;

        public override float GetWeight() =>
            0;

        public override string ToJsonCustomData() =>
            customData.ToJson();
    }

    [Serializable]
    public class CharacterItem : ItemData
    {
        public CharacterCustomData customData;

        public override float GetWeight() =>
            0;

        public override string ToJsonCustomData() =>
            customData.ToJson();
    }
    #endregion "Items"

    #region "CustomDatas"
    [Serializable]
    public class ResourcesCustomData : BaseCustomData
    {
        public RareType rareType = RareType.Normal;
        public int weight = 0;
        public int cooldown = 0;
        public int refiningCost = 0;
        public int refiningTimeSec = 0;
        public int baseCountInArea = 0;
        public int miningTimerInSec = 0;
    }

    [Serializable]
    public class ConsumableCustomData : BaseCustomData
    {
        public RareType rareType = RareType.Normal;
        public ConsumableType consumableType = ConsumableType.Ammo;
        public int weight = 0;
        public int actionTick = 0;
        public int cooldown = 0;
        public int countPerUnit = 0;
    }

    [Serializable]
    public class EquipmentCustomData : BaseCustomData
    {
        public RareType rareType = RareType.Normal;
        public EquipmentType equipmentType = EquipmentType.Weapon;
        public int weight = 0;
        public float damage = 50f;
        public float attackSpeed = 0.1f;
        public float headShotMultiplier = 1.5f;
        public int magCapacity = 12;
        public float reloadSpeed = 2.55f;
        public float shootDistance = 50f;

        public float ProceedDamage(bool critical) =>
            critical ? damage * headShotMultiplier : damage;
    }

    [Serializable]
    public class ShieldCustomData : BaseCustomData
    {
        public RareType rareType = RareType.Normal;
        public int weight = 0;
        public float capacity = 1200f;
        public float threshold = 800f;
        public float regenSpeed = 100f;
        public float timeout = 6f;
    }

    [Serializable]
    public class EnemyCustomData : BaseCustomData
    {
        public float health;
        public float armor;
        public float attackDamage;
        public float pauseBetweenAttacks;
        public int attackCountAnimation;
        public float walkSpeed;
        public float sprintSpeed;
        public float timePursuit;
        public float idleTime;
        public float alarmTime;
        [HideInInspector] public float moveDistance;
        public int pathLength;
        public List<VisionParam> attackZone;
        public List<VisionParam> enemyEyesight;
        public EnemyType enemyType;
    }

    [Serializable]
    public class UpgradeCustomData : BaseCustomData
    {
        public int chance;

        public int damage;
        public int speedRecharge;
        public int accuracy;
        public int volume;
    }

    [Serializable]
    public class CharacterCustomData : BaseCustomData
    {
        public CharacterType characterType = CharacterType.Tiger;
        public CharacterGender characterGender = CharacterGender.Male;
        public float health;
        public float stamina;
        public float walkSpeed;
        public float runSpeedMultiplier;
        public float maxVolumeInventory;
        public float rateStaminaRecovery = 10;
        public float costJump = 50;
        public float costAbility = 40;
        public float costRun = 20;
    }

    public class BaseCustomData
    {
        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }
    #endregion "CustomDatas"

    #region "Base"
    [Serializable]
    public class Catalog
    {
        public string ItemId;
        public string ItemClass;
        [HideInInspector] public string CatalogVersion;
        public string DisplayName;
        public string Description;
        public VirtualCurrencyPrices VirtualCurrencyPrices;
        [HideInInspector] public RealCurrencyPrices RealCurrencyPrices;
        public List<string> Tags;
        [HideInInspector] public string CustomData;
        [HideInInspector] public Consumable Consumable;
        [HideInInspector] public object Container;
        [HideInInspector] public object Bundle;
        [HideInInspector] public bool CanBecomeCharacter;
        [HideInInspector] public bool IsStackable;
        [HideInInspector] public bool IsTradable = true;
        [HideInInspector] public object ItemImageUrl;
        [HideInInspector] public bool IsLimitedEdition;
        [HideInInspector] public int InitialLimitedEditionCount;
        [HideInInspector] public object ActivatedMembership;
    }

    [Serializable]
    public class Consumable
    {
        public object UsageCount;
        public object UsagePeriod;
        public object UsagePeriodGroup;
    }

    [Serializable]
    public class RealCurrencyPrices
    {
    }

    [Serializable]
    public class Root
    {
        public string CatalogVersion;
        public List<Catalog> Catalog;
        public List<object> DropTables;
    }

    [Serializable]
    public class VirtualCurrencyPrices
    {
        public int VC;
        public int CR;
        public int SC;
    }
    #endregion "Base"
}
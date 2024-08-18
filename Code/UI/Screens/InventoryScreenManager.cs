using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using InventoryPlus;
using UnityEngine;
using Zenject;
using UnityEngine.UI;
using GrabCoin.GameWorld.Player;
using GrabCoin.Services.Backend.Inventory;
using GrabCoin.Services.Backend.Catalog;
using Mirror;
using System;
using PlayFabCatalog;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.IO;
//using Codice.Client.Common;
using Newtonsoft.Json;
using System.Linq;
using TMPro;
using Item = InventoryPlus.Item;
using GrabCoin.UI.HUD;
using PlayFab.SharedModels;
using PlayFab.EconomyModels;
// using Codice.Client.BaseCommands.Merge.Xml;
// using UnityEditor.Graphs;

//[UIScreen("UI/Screens/InventoryScreen.prefab")]
public class InventoryScreenManager : UIScreenBase
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Button _closeInventory;
    [SerializeField] private UIDetails _details;
    [SerializeField] private TMP_Text _weightText;

    [Header("Info block")]
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private TMP_Text _staminaText;
    [SerializeField] private TMP_Text _shieldText;
    [SerializeField] private TMP_Text _gcTokenText;
    [SerializeField] private TMP_Text _vaucherText;

    private bool _isOpened;
    private bool _isLoadProcessing;


    private CustomEvent _customEvent;

    private InputReader _inputReader;
    private PlayerScreensManager _screensManager;
    private Controls _controls;
    private InventoryDataManager _inventoryDataManager;
    private CatalogManager _catalogManager;
    private UniTaskCompletionSource<bool> _completion;

    public static InventoryScreenManager Instance; 

    public Inventory Inventory { get { return _inventory; } }
    public InputReader InputReader { get { return _inputReader; } }
    public InventoryDataManager InventoryDataManager { get { return _inventoryDataManager; } }
    //public float LimitWeight => _limitWeight;
    public float CurrentWeight => Inventory.CurrentWeight;
    public bool IsOpened => _isOpened;

    [Inject]
    private void Construct(PlayerScreensManager screensManager, Controls controls, InventoryDataManager inventoryDataManager, CatalogManager catalogManager)
    {
        _screensManager = screensManager;
        _controls = controls;
        _inventoryDataManager = inventoryDataManager;
        _catalogManager = catalogManager;
        _closeInventory.onClick.AddListener(CloseInventory);
        Instance = this;
    }

    public async void Init(float limitWeight)
    {
        if (Instance)
        {
            Instance.Inventory.SetLimit(limitWeight);
            //return;
        }
        else
        {
            Inventory.SetLimit(limitWeight);
        }

        var eventSystem = EventSystemsController.Instance.GetCurrentEventSystem();
        _inputReader = eventSystem.AddComponent<InputReader>();

        _details.inventory = _inventory;
        Inventory.player = NetworkClient.localPlayer.gameObject;

        _inputReader.inventory = _inventory;
        _inputReader.details = _details;
        _inputReader.inputAction = _controls.Player.Inventory;
        _inputReader.Init(_screensManager);
        if (Inventory.inventoryItems == null)
            Inventory.inventoryItems = new();

        var items = _inventoryDataManager.GetAllItems();
        foreach (var item in items)
        {
            if (_catalogManager.GetItemData(item.ItemId) == null)
                await _catalogManager.CashingItem(item.ItemId);

            var itemData = _catalogManager.GetItemData(item.ItemId);
            string log = "Add item in inventory" + itemData.DisplayName;
            if (itemData.ItemClass == "Character") continue;
            log += "/ Not character";
            bool isStackable = itemData.ItemClass is "Equipment" or "Weapon";
            log += $"/ Class: {itemData.ItemClass}";
            if (!isStackable)
            {
                log += $"/ Stackable. Count: {item.count}";
                Inventory.inventoryItems.Add(CreateSlot(item.RefItem, item.count, itemData));
            }
            else
            {
                log += "/ Not stackable: ";
                foreach (var itemInstance in item.items)
                {
                    log += "+";
                    Inventory.inventoryItems.Add(CreateSlot(itemInstance, 1, itemData));
                }

                if (item.items.Count == 0 && item.RefItem != null)
                {
                    log += "/ Real Token: ";
                    Inventory.inventoryItems.Add(CreateSlot(item.RefItem, 1, itemData));
                }
            }
            Debug.Log(log);
        }

        InputReader.OnHotbarSlotSelected += OnHotbarSlotSelected;
        _inventory.Populate(_catalogManager, _screensManager);

        _weightText.text = $"{Inventory.CurrentWeight}/{Inventory.LimitWeight}";

        _customEvent = OnPlayersNumberChanged;
        Translator.Add<HUDProtocol>(_customEvent);
    }

    public void OnPlayersNumberChanged(Enum code, ISendData data)
    {
        switch (code)
        {
            case HUDProtocol.ChangedHealth:
                float[] healths = ((FloatArrayData)data).value;
                _healthText.text = $"{(int)healths[0]}/{(int)healths[1]}";
                break;
            case HUDProtocol.ChangedStamina:
                float[] staminas = ((FloatArrayData)data).value;
                _staminaText.text = $"{(int)staminas[0]}/{(int)staminas[1]}";
                break;
            case HUDProtocol.ChangedShield:
                float[] shields = ((FloatArrayData)data).value;
                _shieldText.text = $"{(int)shields[0]}/{(int)shields[1]}";
                //_energyhBar.fillAmount = ((FloatData)data).value;
                break;
            case HUDProtocol.EnableShield:
                Debug.Log("Get call HUDProtocol.EnableShield");
                //_energyhBar.transform.parent.parent.gameObject.SetActive(((BoolData)data).value);
                break;
        }
    }

    private void Save()
    {
        if (_isLoadProcessing) return;
        List<string> slotsData = new();
        foreach (var slot in _inventory.hotbarUISlots)
        {
            var itemIndex = _inventory.GetItemIndex(slot);
            var item = _inventory.GetItemSlot(itemIndex)?.GetItemType();

            if (item != null)
            {
                string data = itemIndex.ToString() + "^" + item.itemID;
                if (!item.isStackable)
                    data += "^" + item.instanceID;
                slotsData.Add(data);
            }
            else
                slotsData.Add(itemIndex.ToString());
        }

        string jsonDataInventory = JsonConvert.SerializeObject(slotsData);
        PlayerPrefs.SetString("_Inventory.json", jsonDataInventory);
    }

    private void Load()
    {
        //return;
        _isLoadProcessing = true;
        string jsonDataInventory = PlayerPrefs.GetString("_Inventory.json"); //File.ReadAllText(pathInventory);
        var inventoryData = JsonConvert.DeserializeObject<List<string>>(jsonDataInventory);

        List<string[]> datas = inventoryData.Select(x => x.Split("^")).ToList();

        foreach (var data in datas)
        {
            if (data.Length > 1)
            {
                int indexSlot = Int32.Parse(data[0]);
                string id = data.Length == 2 ? data[1] : data[2];
                foreach (var slot in _inventory.inventoryUISlots)
                {
                    var itemIndex = _inventory.GetItemIndex(slot);
                    if (itemIndex == -1) continue;
                    var itemSlot = _inventory.GetItemSlot(itemIndex);
                    var tmpitem = itemSlot?.GetItemType();
                    if (tmpitem != null)
                    {
                        bool isEqualsId = data.Length == 2 ? tmpitem.itemID == id : tmpitem.instanceID == id;
                        if (!isEqualsId) continue;
                        _inventory.SwapItem(slot);
                        _inventory.SwapItem(_inventory.hotbarUISlots[indexSlot]);
                        break;
                    }
                }
            }
        }
        _isLoadProcessing = false;
    }

    public void AddStackableItem(string id, int count)
    {
        var data = _catalogManager.GetItemData(id);
        var slot = CreateSlot(null, count, data);
        Inventory.AddInventory(slot.GetItemType(), slot.GetItemNum(), slot.GetItemDurability(), false);
        _weightText.text = $"{Inventory.CurrentWeight}/{Inventory.LimitWeight}";
    }

    public void AddNotStackableItem(string id, ItemInstance item)
    {
        var data = _catalogManager.GetItemData(id);
        var slot = CreateSlot(item, 1, data);
        Inventory.AddInventory(slot.GetItemType(), slot.GetItemNum(), slot.GetItemDurability(), false);
        _weightText.text = $"{Inventory.CurrentWeight}/{Inventory.LimitWeight}";
    }

    public void RemoveInventory(Item _itemType, int _itemNum)
    {
        Inventory.RemoveInventory(_itemType, _itemNum);
        _weightText.text = $"{Inventory.CurrentWeight}/{Inventory.LimitWeight}";
    }

    public void UseItem(UISlot UIslot)
    {
        Inventory.UseItem(UIslot);
        _weightText.text = $"{Inventory.CurrentWeight}/{Inventory.LimitWeight}";
    }

    public float GetShieldCapacity()
    {
        var shield = GetShield();
        return shield.isInit ? shield.Value.customData.capacity : 0f;
    }

    public Optionals<ShieldItem> GetShield()
    {
        var uiSlot = Inventory.hotbarUISlots.Last();
        var itemSlot = Inventory.GetItemSlot(Inventory.GetItemIndex(uiSlot))?.GetItemType();
        return itemSlot != null ? new Optionals<ShieldItem>(_catalogManager.GetItemData(itemSlot.itemID) as ShieldItem) : default;
    }

    private ItemSlot CreateSlot(PlayFabBaseModel _item, int count, ItemData itemData)
    {
        ItemInstance item = null;
        InventoryItem itemV2 = null;
        if (_item == null) { }
        else if (_item is ItemInstance) { item = _item as ItemInstance; }
        else if (_item is InventoryItem) { itemV2 = _item as InventoryItem; }
        else return new ItemSlot(null, 0, 0);

        Item newItem = ScriptableObject.CreateInstance(typeof(Item)) as Item;// new();
        newItem.maxDurability = 1;
        newItem.itemSprite = itemData.itemConfig.Icon;
        newItem.isStackable = itemData.ItemClass != "Equipment" && itemData.ItemClass != "Weapon" && itemData.ItemClass != "Character" && itemData.ItemClass != "Shield";
        newItem.isDurable = false;
        newItem.stackSize = 400;
        newItem.itemCategory = itemData.ItemClass is "Equipment" or "Weapon" ? "weapon" : itemData.ItemClass;
        newItem.itemName = itemData.DisplayName;
        newItem.itemID = itemData.ItemId;
        newItem.weight = (int)itemData.GetWeight();
        newItem.itemDescription = itemData.Description;
        newItem.itemPrefab = itemData.itemConfig.Prefab;
        if (!newItem.isStackable)
        {
            if (item != null)
            {
                newItem.instanceID = item.ItemInstanceId;
                if (item.CustomData != null && item.CustomData.Count > 0 && item.CustomData.ContainsKey("level"))
                {
                    int currentLevel = Int32.Parse(item.CustomData["level"]);
                    newItem.itemUpgradeLevel = currentLevel;
                    newItem.itemName += $" +{currentLevel}";
                }
            }
            else if(itemV2 != null)
            {
                StringData data = Translator.SendOneAnswer<GeneralProtocol, ISendData, StringData>(GeneralProtocol.GetHash, new StringData { value = itemV2.DisplayProperties.ToString() });
                newItem.instanceID = data.value;
                if (itemV2.DisplayProperties != null && itemV2.DisplayProperties.ToString().Length > 0)
                {
                    var properties = JsonConvert.DeserializeObject<Dictionary<string, string>>(itemV2.DisplayProperties.ToString());
                    if (properties.ContainsKey("level"))
                    {
                        int currentLevel = Int32.Parse(properties["level"]);
                        newItem.itemUpgradeLevel = currentLevel;
                        newItem.itemName += $" +{currentLevel}";
                    }
                }
            }
        }
        return new ItemSlot(newItem, count, 1);
    }

    

    private void OnEnable()
    {
        Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        _inventory.ShowInventory(true);
        _weightText.text = $"{Inventory.CurrentWeight}/{Inventory.LimitWeight}";
        _gcTokenText.text = $"{_inventoryDataManager.GetCurrencyData():F2}";
        _vaucherText.text = $"{_inventoryDataManager.GetCurrencyVC()}";
    }

    public override void CheckOnEnable()
    {
        Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
    }

    private void OnDisable()
    {
        Inventory.OnSwap?.Invoke();
    }

    public override void CheckInputHandler(Controls controls)
    {
        base.CheckInputHandler(controls);
        if (controls.Player.Inventory.WasPressedThisFrame() || controls.Player.CallMenu.WasPressedThisFrame())
        {
            _screensManager.OpenScreen<GameHud>().Forget();
        }
    }

    public override void Close()
    {
        CloseInventory();
    }

    private async void Start()
    {
        await UniTask.Delay(50);
        if (PlayerPrefs.HasKey("_Inventory.json"))
            Load();
        else
            Save();
    }

    private async void OnDestroy()
    {
        await UniTask.WaitUntil(() => Instance != null);
        await UniTask.WaitUntil(() => Instance.InputReader != null);
        InputReader.OnHotbarSlotSelected -= OnHotbarSlotSelected;
    }

    public void OnHotbarSlotSelected(int index)
    {
        if (index == -1) return;
        Save();
    }

    private void CloseInventory()
    {
        //_inventory.ShowInventory(false);
        //Process();
        base.Close();
        //_completion.TrySetResult(true);
        //Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = false });
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        //Debug.Log($"<color=blue>Cursor visible: {Cursor.visible}</color>");
    }

    public UniTask<bool> Process()
    {
        _completion = new UniTaskCompletionSource<bool>();
        return _completion.Task;
    }
}

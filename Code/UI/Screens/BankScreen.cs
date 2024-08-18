using Code.BankInventory;
using Cysharp.Threading.Tasks;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.UI.HUD;
using GrabCoin.UI.ScreenManager;
using InventoryPlus;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[UIScreen("UI/Screens/BankScreen.prefab")]
public class BankScreen : UIScreenBase
{
    [SerializeField] private UISlot slotPrefab;
    [SerializeField] private Transform playerInventoryContent;
    [SerializeField] private Transform playerBankContent;
    [SerializeField] private UISlot _inventorySwapUIZone;
    //[SerializeField] private BankStorage bankStorage;
    [SerializeField] private Inventory _bankStorage;
    [SerializeField] private List<SlotnIterim> slots = new ();
    [SerializeField] private Button _backButton;
    [SerializeField] private Button allSort;
    [SerializeField] private Button gunsSort;
    [SerializeField] private Button oreSort;
    [SerializeField] private int _countStorageSlots = 10;

    private Inventory _inventory;
    private PlayerScreensManager _screensManager;
    private CatalogManager _catalogManager;
    private bool _isLoadProcessing;

    private Inventory Inventory
    {
        get
        {
            if (_inventory == null)
                _inventory = InventoryScreenManager.Instance.Inventory;
            return _inventory;
        }
    }

    [Inject]
    private void Construct(
        PlayerScreensManager screensManager,
        CatalogManager catalogManager
        )
    {
        _screensManager = screensManager;
        _catalogManager = catalogManager;
        _bankStorage.Populate(_catalogManager, screensManager);
        _bankStorage.SetLimit(1000000);
    }

    private async void Awake()
    {
        _backButton.onClick.AddListener(Back);
        await UniTask.Delay(60);
        if (PlayerPrefs.HasKey("Storage.json"))
            Load();
        else
            Save();
    }

    private void OnEnable()
    {
        InitInventorySlots();
        allSort.onClick.AddListener(SortAll);
        gunsSort.onClick.AddListener(delegate { SortCategory("weapon"); });
        oreSort.onClick.AddListener(delegate { SortCategory("Ore"); });
        Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        Inventory.SetSwapUIZone(_inventorySwapUIZone, transform);
    }

    public override void CheckOnEnable()
    {
        Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
    }

    private void OnDisable()
    {
        allSort.onClick.RemoveAllListeners();
        gunsSort.onClick.RemoveAllListeners();
        oreSort.onClick.RemoveAllListeners();

        foreach (var slot in slots)
        {
            if (slot == null || slot.slot == null) continue;
            slot.slot.transform.SetParent(slot.slotParent);
            slot.slot.transform.localScale = slot.scale;
            slot.slot.transform.localPosition = slot.position;
            slot.slot.transform.localRotation = slot.rotation;
        }
        slots.Clear();
        Inventory.ResetSwapUIZone();
    }

    bool fromInventory;
    bool fromStorage;
    private void Update()
    {
        if (!fromInventory && !fromStorage)
        {
            if (_bankStorage.SwapUIZone.gameObject.activeSelf)
            {
                fromStorage = true;
                _inventorySwapUIZone.gameObject.SetActive(_bankStorage.SwapUIZone.gameObject.activeSelf);
            }
            else if (_inventorySwapUIZone.gameObject.activeSelf)
            {
                fromInventory = true;
                _bankStorage.SwapUIZone.gameObject.SetActive(_inventorySwapUIZone.gameObject.activeSelf);
            }
        }
        else if (fromStorage)
        {
            if (!_bankStorage.SwapUIZone.gameObject.activeSelf)
            {
                fromStorage = false;
                _inventorySwapUIZone.gameObject.SetActive(_bankStorage.SwapUIZone.gameObject.activeSelf);

                foreach (var slot in slots)
                {
                    if (slot == null || slot.slot == null) continue;
                    slot.slot.transform.SetParent(slot.slotParent);
                    slot.slot.transform.localScale = slot.scale;
                    slot.slot.transform.localPosition = slot.position;
                    slot.slot.transform.localRotation = slot.rotation;
                }
                slots.Clear();
                InitInventorySlots();
                Save();
            }
        }
        else if (fromInventory)
        {
            if (!_inventorySwapUIZone.gameObject.activeSelf)
            {
                fromInventory = false;
                _bankStorage.SwapUIZone.gameObject.SetActive(_inventorySwapUIZone.gameObject.activeSelf);
                Save();
            }
        }

        //if (_bankStorage.SwapUIZone.gameObject.activeSelf != _inventorySwapUIZone.gameObject.activeSelf)
        //    _bankStorage.SwapUIZone.gameObject.SetActive(_inventorySwapUIZone.gameObject.activeSelf);

        //if (_bankStorage.SwapUIZone.gameObject.activeSelf != _inventorySwapUIZone.gameObject.activeSelf)
        //    _inventorySwapUIZone.gameObject.SetActive(_bankStorage.SwapUIZone.gameObject.activeSelf);
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
    }

    private void SortAll()
    {
        //базова€ сортировка(показать все)
        for (int i = 0; i < playerInventoryContent.childCount; i++)
        {
            var slotObj = playerInventoryContent.GetChild(i);
            slotObj.gameObject.SetActive(true);
        }
        for (int i = 0; i < playerBankContent.childCount; i++)
        {
            var slotObj = playerBankContent.GetChild(i);
            slotObj.gameObject.SetActive(true);
        }
    }
    
    private void SortCategory(string category)
    {
        //базова€ сортировка(отключение тех €чеек которые не подход€т)
        for (int i = 0; i < playerInventoryContent.childCount; i++)
        {
            var slotObj = playerInventoryContent.GetChild(i);
            if(slotObj.TryGetComponent(out UISlot slot))
            {
                var item = slot.GetSlotOwner().GetItemSlot(slot.GetSlotOwner().GetItemIndex(slot));
                if (item != null)
                {
                    if(item.GetItemType().itemCategory == category)
                    {
                        slotObj.gameObject.SetActive(true);
                    }
                    else
                    {
                        slotObj.gameObject.SetActive(false);
                    }
                }
            }
        }
        for (int i = 0; i < playerBankContent.childCount; i++)
        {
            var slotObj = playerBankContent.GetChild(i);
            if (slotObj.TryGetComponent(out UISlot slot))
            {
                var item = slot.GetSlotOwner().GetItemSlot(slot.GetSlotOwner().GetItemIndex(slot));
                if (item != null)
                {
                    if (item.GetItemType().itemCategory == category)
                    {
                        slotObj.gameObject.SetActive(true);
                    }
                    else
                    {
                        slotObj.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    //private void InitStorageSlots()
    //{
    //    for (int i = 0; i < _countStorageSlots; i++)
    //    {
    //        //вместо этого встроить надо большой слот!!!
    //        var bankSlot = SpawnSlot(playerBankContent);

    //        bankSlot.SetupUISlot(bankStorage);
    //        //вот прив€зка к инвентарю, про которую € говорил
    //        bankSlot.SetupMouseDrag(Inventory);

    //        bankStorage.AddUISlot(bankSlot);
    //        //предмет должен быть об€зательно, инвентарь к нему обращаетс€ при переносе(хз зачем)
    //        bankStorage.AddSlot(null);
    //    }
    //}
    private void InitInventorySlots()
    {


        //foreach (var item in inventory.GetSlots())
        //{
        //    if (item == null || !item.GetItemType())
        //        continue;

        //    var slot = SpawnSlot(playerInventoryContent);

        //    if (bankStorage.enableMouseDrag) slot.SetupMouseDrag(inventory);
        //        slot.SetupUISlot(inventoryBankStorage);

        //    bankStorage.AddUISlot(slot);
        //    bankStorage.AddToStorage(item.GetItemType(), item.GetItemNum(), item.GetItemDurability());
        //}

        foreach (var theSlot in Inventory.inventoryUISlots)
        {
            slots.Add(new SlotnIterim { slot = theSlot, slotParent = theSlot.transform.parent, 
                scale = theSlot.transform.localScale, position = theSlot.transform.localPosition, 
                rotation = theSlot.transform.localRotation });
            theSlot.transform.SetParent(playerInventoryContent);
            theSlot.transform.localScale = Vector3.one;
            theSlot.transform.localRotation = Quaternion.identity;
            theSlot.transform.localPosition = Vector3.zero;
        }
        //foreach (var item in Inventory.GetUISlots())
        //{
        //    //проверка на то, что предмет существует
        //    if (item == null || Inventory.GetItemIndex(item) < 0 
        //        || Inventory.GetItemSlot(Inventory.GetItemIndex(item)) == null 
        //        || Inventory.GetItemSlot(Inventory.GetItemIndex(item)).GetItemNum() <= 0) 
        //        continue;

        //    //добавл€ем инфу о слоте и где он был
        //    slots.Add(new SlotnIterim { slot = item, slotParent = item.transform, scale = item.transform.localScale });
        //    item.transform.localScale = Vector3.one;
        //    item.transform.SetParent(playerInventoryContent);
        //}
    }

    //возвращаем слоты в инвентарь
    public void MoveSlotsBack()
    {
        foreach (var item in slots)
        {
            item.slot.transform.SetParent(item.slotParent);
            item.slot.transform.localScale = item.scale;
            item.slot.transform.localPosition = item.position;
            item.slot.transform.localRotation = item.rotation;
        }
        slots.Clear();
    }

    public UISlot SpawnSlot(Transform parent)
    {
        return Instantiate(slotPrefab, parent);
    }

    private void Save()
    {
        if (_isLoadProcessing) return;
        List<string> slotsData = new();
        foreach (var slot in _bankStorage.inventoryUISlots)
        {
            var itemIndex = _bankStorage.GetItemIndex(slot);
            var itemSlot = _bankStorage.GetItemSlot(itemIndex);
            var item = itemSlot?.GetItemType();

            if (item != null)
            {
                string data = itemIndex.ToString() + "^" + item.itemID;
                data += "^" + itemSlot.GetItemNum();
                if (!item.isStackable)
                    data += "^" + item.instanceID;
                slotsData.Add(data);
            }
            else
                slotsData.Add(itemIndex.ToString());
        }

        string jsonDataInventory = JsonConvert.SerializeObject(slotsData);
        SceneNetworkContext.Instance.UpdateUserPublisherData("storage_bank", jsonDataInventory, b => { });
    }

    private void Load()
    {
        _isLoadProcessing = true;
        string jsonDataInventory = PlayerPrefs.GetString("storage_bank"); //File.ReadAllText(pathInventory);
        var inventoryData = JsonConvert.DeserializeObject<List<string>>(jsonDataInventory);

        List<string[]> datas = inventoryData?.Select(x => x.Split("^")).ToList() ?? new();

        foreach (var data in datas)
        {
            if (data.Length > 1)
            {
                int indexSlot = Int32.Parse(data[0]);
                string id = data.Length == 3 ? data[1] : data[3];
                int count = Int32.Parse(data[2]);
                foreach (var slot in Inventory.inventoryUISlots)
                {
                    var itemIndex = _inventory.GetItemIndex(slot);
                    if (itemIndex == -1) continue;
                    var itemSlot = _inventory.GetItemSlot(itemIndex);
                    var tmpitem = itemSlot?.GetItemType();
                    if (tmpitem != null)
                    {
                        bool isEqualsId = data.Length == 3 ? tmpitem.itemID == id : tmpitem.instanceID == id;
                        if (!isEqualsId) continue;

                        count = count > itemSlot.GetItemNum() ? itemSlot.GetItemNum() : count;
                        _inventory.RemoveInventory(tmpitem, count);
                        _bankStorage.AddInventory(tmpitem, count, itemSlot.GetItemDurability(), true);
                        break;
                    }
                }
            }
        }
        _inventory.UpdateWeight();
        _isLoadProcessing = false;
    }
}

public class SlotnIterim
{
    public Transform slotParent;
    public UISlot slot;
    public Vector3 scale;
    public Vector3 position;
    public Quaternion rotation;
}
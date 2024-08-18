using Cysharp.Threading.Tasks;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.UI.HUD;
using GrabCoin.UI.Screens;
using PlayFab.EconomyModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using UnityEditor.Graphs;
// using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
// using static UnityEditor.PlayerSettings;
// using static UnityEditor.PlayerSettings;


namespace InventoryPlus
{
    public class Inventory : Storage
    {
        [SerializeField] private UISlot prefabUISlot;
        [SerializeField] public Transform inventoryUISlotsContainer;
        [SerializeField] public List<ItemSlot> inventoryItems;
        [SerializeField] public List<UISlot> inventoryUISlots;
        [SerializeField] public bool hasHotbar = false;
        [SerializeField] public List<UISlot> hotbarUISlots;

        [SerializeField] public bool displayNotificationOnNewItems;
        [SerializeField] public bool showDurabilityValues = false;
        [SerializeField] public bool fillReservedFirst;
        [SerializeField] public bool enableMouseDrag = true;

        [SerializeField] public Animator anim;
        
        [SerializeField] public bool instanciatePickuppableOnDrop = false;
        [SerializeField] public GameObject pickupPrefab;
        [SerializeField] public Transform pickupContext;
        [SerializeField] public GameObject player;

        [SerializeField] public AudioSource itemsAudio;
        [SerializeField] public AudioSource sortAudio;        
        [SerializeField] public AudioSource swapAudio;

        [SerializeField] public bool enableDebug;
        [SerializeField] private UISlot swapUIZone;
        private UISlot _swapUIZoneBuffer;
        private Transform _pickupContextBuffer;
        private CatalogManager _catalogManager;
        private PlayerScreensManager _screensManager;

        private bool inChestRange = false;
        private List<ItemSlot> dropList = new List<ItemSlot>();
        private UISlot swapUISlot;
        private bool wasLoaded = false;
        private float _limitWeight = 0f;
        private float _currentWeight = 0f;

        public UnityAction OnSwap;
        public UnityAction OnSort;
        /**/
        public UISlot SwapUIZone => swapUIZone;
        public float LimitWeight => _limitWeight;
        public float CurrentWeight => _currentWeight;

        #region Setup

        public void SetLimit(float limit) =>
            _limitWeight = limit;

        public bool CheckWeightLimit(string itemId, int count, out int limit)
        {
            var item = _catalogManager.GetItemData(itemId);
            float delta = _limitWeight - _currentWeight;
            limit = (int)(delta / item.GetWeight());
            return CheckWeightLimit(item.GetWeight() * count);
        }

        public bool CheckWeightLimit(float weight) =>
            _currentWeight + weight > _limitWeight;

        public void UpdateWeight()
        {
            var sum = GetSlots().Sum(slot =>
            {
                int result = 0;
                if (slot != null)
                {
                    var itemType = slot.GetItemType();
                    if (itemType != null)
                        result += slot.GetItemType().weight * slot.GetItemNum();
                }
                return result;
            });
            _currentWeight = sum;

            int countAmmo = InventoryScreenManager.Instance.Inventory.GetItemData("i_ammo_pack")?.GetItemNum() ?? 0;
            int countBullet = SceneNetworkContext.Instance.GetStatistic("currentAmmo").Value;
            Translator.Send(HUDProtocol.CountBullet, new StringData { value = $"{countBullet}/{countAmmo}" });
        }

        public void Populate(CatalogManager catalogManager, PlayerScreensManager screensManager)
        {
            if (!wasLoaded)
            {
                _catalogManager = catalogManager;
                _screensManager = screensManager;
                AssignInventorySlots();
                AssignHotbarSlots();

                AddStartingInventory();
                UpdateWeight();

                inventoryItems = null;

                wasLoaded = true;
            }
        }


        public void LoadInventory(List<ItemSlot> _items)
        {
            return;
            slots.Clear();
            UISlots.Clear();

            AssignInventorySlots();
            AssignHotbarSlots();

            AddStartingInventory();

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null && _items[i].GetItemType() != null)
                {
                    if (!_items[i].GetItemType().isStackable)
                    {
                        slots[i] = new ItemSlot(_items[i].GetItemType(), 1, _items[i].GetItemDurability());
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, UISlots[i].enableNotification);
                    }
                    else
                    {
                        slots[i] = new ItemSlot(_items[i].GetItemType(), _items[i].GetItemNum(), 1f);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, UISlots[i].enableNotification);
                    }
                }
            }

            wasLoaded = true;
            if (enableDebug) Debug.Log("Inventory loaded");
        }


        private void AssignInventorySlots()
        {
            swapUIZone.SetupUISlot(this);
            //for (int i = 0; i < inventoryUISlots.Count; i++)
            //{
            //    inventoryUISlots[i].gameObject.SetActive(false);
            //    //UISlots.Add(inventoryUISlots[i]);
            //    //slots.Add(null);

            //    if(enableMouseDrag) inventoryUISlots[i].SetupMouseDrag(this);
            //    inventoryUISlots[i].SetupUISlot(this);
            //}
        }


        private void AssignHotbarSlots()
        {
            if (hasHotbar)
            {
                for (int i = 0; i < hotbarUISlots.Count; i++)
                {
                    UISlots.Add(hotbarUISlots[i]);
                    slots.Add(null);

                    if (enableMouseDrag) hotbarUISlots[i].SetupMouseDrag(this);
                    hotbarUISlots[i].SetupUISlot(this);
                }
            }
        }


        private void AddStartingInventory()
        {
            if (inventoryItems.Count > 0)
            {
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    AddInventory(inventoryItems[i].GetItemType(), inventoryItems[i].GetItemNum(), inventoryItems[i].GetItemDurability(), false);
                }
            }
        }

        public void SetSwapUIZone(UISlot swapUIZone, Transform pickupContext)
        {
            _swapUIZoneBuffer = this.swapUIZone;
            this.swapUIZone = swapUIZone;
            this.swapUIZone.SetupUISlot(this);

            _pickupContextBuffer = this.pickupContext;
            this.pickupContext = pickupContext;
        }

        public void ResetSwapUIZone()
        {
            swapUIZone = _swapUIZoneBuffer;
            pickupContext = _pickupContextBuffer;
        }
        #endregion


        #region AddToInventory

        public void AddInventory(Item _itemType, int _itemNum, float _itemDurability, bool _forceDisableNotification)
        {
            if(_itemType != null && _itemNum > 0)
            {
                //Debug.Log($"AddInventory {(_itemType != null ? _itemType.itemID : null)}");
                if (!_itemType.isStackable) AddNotStackable(_itemType, _itemNum, _itemDurability, _forceDisableNotification);
                else AddStackable(_itemType, _itemNum, _forceDisableNotification);

                UpdateWeight();
            }
        }


        private void AddNotStackable(Item _itemType, int _itemNum, float _itemDurability, bool _forceDisableNotification)
        {
            bool notificationDisplay = displayNotificationOnNewItems;
            if (_forceDisableNotification) notificationDisplay = false;

            int remainingNum = _itemNum;

            var newSlot = Instantiate(prefabUISlot.gameObject, inventoryUISlotsContainer).GetComponent<UISlot>();
            newSlot.categoryName = _itemType.itemCategory;
            newSlot.restrictedToCategory = true;
            newSlot.gameObject.SetActive(true);
            UISlots.Add(newSlot);
            inventoryUISlots.Add(newSlot);
            slots.Add(null);

            if (enableMouseDrag) newSlot.SetupMouseDrag(this);
            newSlot.SetupUISlot(this);

            if (fillReservedFirst)
            {
                //fill all empty reserved slots
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i] == null && UISlots[i].restrictedToCategory && UISlots[i].categoryName == _itemType.itemCategory && UISlots[i].typeSlot == UISlot.TypeSlot.Inventory)
                    {
                        slots[i] = new ItemSlot(_itemType, 1, _itemDurability);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                        remainingNum--;
                        if (remainingNum == 0) break;
                    }
                }
            }


            if (remainingNum > 0)
            {
                //fill all empty slots
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i] == null && !UISlots[i].restrictedToCategory && UISlots[i].typeSlot == UISlot.TypeSlot.Inventory)
                    {
                        slots[i] = new ItemSlot(_itemType, 1, _itemDurability);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                        remainingNum--;
                        if (remainingNum == 0) break;
                    }
                }
            }

            if (remainingNum > 0) dropList.Add(new ItemSlot(_itemType, _itemNum, _itemDurability));
            if (enableDebug) AddInventoryDebug(remainingNum, _itemNum, _itemType.itemName);

            newSlot.restrictedToCategory = false;
        }


        private void AddStackable(Item _itemType, int _itemNum, bool _forceDisableNotification)
        {
            bool notificationDisplay = displayNotificationOnNewItems;
            if (_forceDisableNotification) notificationDisplay = false;

            int remainingNum = _itemNum;


            //fill already partially filled slots
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].GetItemType().itemID == _itemType.itemID)
                {
                    int slotFill = _itemType.stackSize - slots[i].GetItemNum();

                    if (remainingNum < slotFill)
                    {
                        slots[i].AddItemNum(remainingNum);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                        remainingNum = 0;
                        break;
                    }
                    else
                    {
                        slots[i].AddItemNum(slotFill);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                        remainingNum -= slotFill;
                    }
                }
            }


            //fill all empty slots
            if (remainingNum > 0)
            {
                var newSlot = Instantiate(prefabUISlot.gameObject, inventoryUISlotsContainer).GetComponent<UISlot>();
                newSlot.categoryName = _itemType.itemCategory;
                newSlot.restrictedToCategory = true;
                newSlot.gameObject.SetActive(true);
                UISlots.Add(newSlot);
                inventoryUISlots.Add(newSlot);
                slots.Add(null);

                if (enableMouseDrag) newSlot.SetupMouseDrag(this);
                newSlot.SetupUISlot(this);

                if (fillReservedFirst)
                {
                    for (int i = 0; i < slots.Count; i++)
                    {
                        if ((slots[i] == null && remainingNum > _itemType.stackSize) && UISlots[i].restrictedToCategory && _itemType.itemCategory == UISlots[i].categoryName)
                        {
                            slots[i] = new ItemSlot(_itemType, _itemType.stackSize, 1f);
                            UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);

                            remainingNum -= _itemType.stackSize;
                        }
                        else if ((slots[i] == null && remainingNum <= _itemType.stackSize) && UISlots[i].restrictedToCategory && _itemType.itemCategory == UISlots[i].categoryName)
                        {
                            slots[i] = new ItemSlot(_itemType, remainingNum, 1f);
                            UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);

                            remainingNum = 0;
                            break;
                        }
                    }
                }

                if (remainingNum > 0)
                {
                    for (int i = 0; i < slots.Count; i++)
                    {
                        if ((slots[i] == null && remainingNum > _itemType.stackSize) && !UISlots[i].restrictedToCategory)
                        {
                            slots[i] = new ItemSlot(_itemType, _itemType.stackSize, 1f);
                            UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                            remainingNum -= _itemType.stackSize;
                        }
                        else if ((slots[i] == null && remainingNum <= _itemType.stackSize) && !UISlots[i].restrictedToCategory)
                        {
                            slots[i] = new ItemSlot(_itemType, remainingNum, 1f);
                            UISlots[i].UpdateUI(slots[i], showDurabilityValues, notificationDisplay);

                            remainingNum = 0;
                            break;
                        }
                    }
                }

                newSlot.restrictedToCategory = false;
            }

            if (remainingNum > 0) dropList.Add(new ItemSlot(_itemType, remainingNum, 1f));
            if (enableDebug) AddInventoryDebug(remainingNum, _itemNum, _itemType.itemName);
        }


        private void AddInventoryDebug(int _remainingNum, int _itemNum, string _itemName)
        {
            if (_remainingNum == 0) Debug.Log("Added " + _itemNum + " " + _itemName + " to " + this.gameObject.name);
            else if (_remainingNum == _itemNum) Debug.Log("Nothing was added, " + this.gameObject.name + " is at full capacity");
            else Debug.Log("Added " + (_itemNum - _remainingNum) + " " + _itemName + " to " + this.gameObject.name + " until inventory capacity was reached");
        }

        #endregion


        #region RemoveFromInventory

        public void RemoveInventory(Item _itemType, int _itemNum)
        {
            if (!_itemType.isStackable) RemoveNotStackable(_itemType, _itemNum);
            else RemoveStackable(_itemType, _itemNum);

            UpdateWeight();
        }


        private void RemoveNotStackable(Item _itemType, int _itemNum)
        {
            int remainingNum = _itemNum;


            //remove item type from all slots
            for (int i = slots.Count - 1; i >= 0; i--)
            {
                if (slots[i] != null && slots[i].GetItemType().instanceID == _itemType.instanceID)
                {
                    //slots[i] = null;
                    UISlots[i].ShowUI(false);

                    var uiSlot = UISlots[i];
                    var slot = slots[i];
                    slots.Remove(slot);
                    inventoryUISlots.Remove(uiSlot);
                    UISlots.Remove(uiSlot);
                    Destroy(uiSlot.gameObject);

                    remainingNum--;
                    if (remainingNum == 0) break;
                }
            }

            if (enableDebug) RemoveInventoryDebug(remainingNum, _itemNum, _itemType.itemName);
        }


        private void RemoveStackable(Item _itemType, int _itemNum)
        {
            int remainingNum = _itemNum;


            //remove from not empty slots
            for (int i = slots.Count - 1; i >= 0; i--)
            {
                if (slots[i] != null && slots[i].GetItemType().itemID == _itemType.itemID)
                {
                    if (_itemNum >= slots[i].GetItemNum())
                    {
                        remainingNum -= slots[i].GetItemNum();

                        //slots[i] = null;
                        UISlots[i].ShowUI(false);

                        var uiSlot = UISlots[i];
                        var slot = slots[i];
                        slots.Remove(slot);
                        inventoryUISlots.Remove(uiSlot);
                        UISlots.Remove(uiSlot);
                        Destroy(uiSlot.gameObject);
                        break;
                    }
                    else
                    {
                        slots[i].AddItemNum(-remainingNum);
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, displayNotificationOnNewItems);

                        remainingNum = 0;
                        break;
                    }
                }
            }

            if (enableDebug) RemoveInventoryDebug(remainingNum, _itemNum, _itemType.itemName);
        }


        private void RemoveInventoryDebug(int _remainingNum, int _itemNum, string _itemName)
        {
            if (_remainingNum == 0) Debug.Log("Removed " + _itemNum + " " + _itemName + " from " + this.gameObject.name);
            else if (_remainingNum == _itemNum) Debug.Log("Nothing was removed, there is no such item");
            else Debug.Log("Removed " + (_itemNum - _remainingNum) + " " + _itemName + " until the inventory run out of that item");
        }

        #endregion


        #region RemoveBlock

        public void RemoveCategory(string _category)
        {
            int remainingNum = 0;


            //remove slots that contain category
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].GetItemType().itemCategory == _category)
                {
                    slots[i] = null;
                    UISlots[i].ShowUI(false);

                    remainingNum++;
                }
            }

            if(enableDebug) Debug.Log("Removed " + remainingNum + " from the inventory");
        }


        public void RemoveID(string _ID)
        {
            int remainingNum = 0;


            //remove slots that contain ID
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].GetItemType().itemID == _ID)
                {
                    slots[i] = null;
                    UISlots[i].ShowUI(false);

                    remainingNum++;
                }
            }

            if (enableDebug) Debug.Log("Removed " + remainingNum + " from the inventory");
        }


        public void ClearInventory()
        {
            //remove everything
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i] = null;
                    UISlots[i].ShowUI(false);
                }
            }

            if (enableDebug) Debug.Log("Cleared inventory");
        }

        #endregion


        #region Use / Drop / Equip

        public void UseItem(UISlot _UIslot)
        {
            ItemSlot slot = GetInventorySlot(_UIslot);
            bool isItemUsable = true;

            if (slot != null)
            {
                if (slot.GetItemType().isStackable)
                {
                    if (slot.GetItemNum() > 1)
                    {
                        //use stackable
                        slots[UISlots.IndexOf(_UIslot)].AddItemNum(-1);
                        _UIslot.UpdateUI(slot, showDurabilityValues, false);
                    }
                    else
                    {
                        //remove stackable
                        _UIslot.ShowUI(false);
                        slots[UISlots.IndexOf(_UIslot)] = null;
                    }

                    if (slot.GetItemNum() <= 0)
                    {
                        UISlots.Remove(_UIslot);
                        inventoryUISlots.Remove(_UIslot);
                        Destroy(_UIslot.gameObject);
                    }
                }
                else
                {
                    if (slot.GetItemType().isDurable)
                    {
                        if (slot.GetItemDurability() - slot.GetItemType().usageCost > 0)
                        {
                            //use not stackable
                            slots[UISlots.IndexOf(_UIslot)].SetItemDurability(slot.GetItemDurability() - slot.GetItemType().usageCost);
                            _UIslot.UpdateUI(slot, showDurabilityValues, false);
                        }
                        else
                        {
                            //remove not stackable
                            slots[UISlots.IndexOf(_UIslot)] = null;
                            _UIslot.ShowUI(false);
                        }

                        if (slot.GetItemDurability() <= 0)
                        {
                            UISlots.Remove(_UIslot);
                            inventoryUISlots.Remove(_UIslot);
                            Destroy(_UIslot.gameObject);
                        }
                    }
                    else isItemUsable = false;
                }

                if(isItemUsable)
                {
                    if (itemsAudio != null && slot.GetItemType().useAudio != null) PlayItemsAudio(slot.GetItemType().useAudio);
                    if (enableDebug) Debug.Log("Used " + slot.GetItemType().itemName);
                }
                else if (enableDebug) Debug.Log("Can't use an item with no durability");

                UpdateWeight();
            }

            else if (enableDebug) Debug.Log("Not selecting an item, can't perform the Use action");
        }


        public void DropItem(UISlot _UISlot)
        {
            ItemSlot slot = GetInventorySlot(_UISlot);

            if (slot != null)
            {
                //instanciate pickup
                if (instanciatePickuppableOnDrop) dropList.Add(slots[UISlots.IndexOf(_UISlot)]);

                //remove selected
                _UISlot.ShowUI(false);
                slots[UISlots.IndexOf(_UISlot)] = null;

                if (itemsAudio != null && slot.GetItemType().dropAudio != null) PlayItemsAudio(slot.GetItemType().dropAudio);
                if (enableDebug) Debug.Log("Dropped " + slot.GetItemType().itemName);
            }

            else if (enableDebug) Debug.Log("Not selecting an item, can't perform the Drop action");
        }


        public void EquipItem(UISlot _UIinvetorySlot)
        {
            ItemSlot slot = GetInventorySlot(_UIinvetorySlot);
            bool equippedSelected = false;

            if (slot != null)
            {
                //loop and equip selected if possible
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i] == null && UISlots[i].restrictedToCategory && UISlots[i].categoryName == slot.GetItemType().itemCategory)
                    {
                        slots[i] = slot;
                        UISlots[i].UpdateUI(slots[i], showDurabilityValues, displayNotificationOnNewItems);

                        slots[UISlots.IndexOf(_UIinvetorySlot)] = null;
                        _UIinvetorySlot.ShowUI(false);

                        equippedSelected = true;
                        break;
                    }
                }
            }

            if (equippedSelected && itemsAudio != null && slot.GetItemType().equipAudio != null) PlayItemsAudio(slot.GetItemType().equipAudio);
            
            if (enableDebug)
            {
                if (equippedSelected) Debug.Log("Equipped selected slot");
                else Debug.Log("Could not equip selected slot");
            }
        }


        private void PlayItemsAudio(AudioClip _audioClip)
        {
            itemsAudio.clip = _audioClip;
            itemsAudio.Play();
        }

        #endregion


        #region Swap / Sort

        public async void SwapItem(UISlot _UIInventorySlot)
        {
            if (swapUISlot == null) //begin drag
            {
                swapUIZone?.gameObject.SetActive(true);
                swapUISlot = _UIInventorySlot;
                _UIInventorySlot.SetSwapState(true);
            }
            else if(_UIInventorySlot == swapUISlot) // dont drag
            {
                swapUIZone?.gameObject.SetActive(false);
                swapUISlot.SetSwapState(false);
                swapUISlot = null;
                _UIInventorySlot.SetSwapState(false);
            }
            else
            {
                Storage slotContainer_from = swapUISlot.GetSlotOwner();
                if (!slotContainer_from)
                {
                    swapUIZone?.gameObject.SetActive(false);
                    swapUISlot.SetSwapState(false);
                    swapUISlot = null;
                    _UIInventorySlot.SetSwapState(false);
                    return;
                }
                int index_from = slotContainer_from.GetItemIndex(swapUISlot);
                ItemSlot itemSlot_from = slotContainer_from.GetItemSlot(index_from);

                if (itemSlot_from?.GetItemType() == null)
                {
                    swapUIZone?.gameObject.SetActive(false);
                    swapUISlot.SetSwapState(false);
                    swapUISlot = null;
                    _UIInventorySlot.SetSwapState(false);
                    return;
                }

                Storage slotContainer_to = _UIInventorySlot.GetSlotOwner();
                if (!slotContainer_to && _UIInventorySlot != swapUIZone)
                {
                    swapUIZone?.gameObject.SetActive(false);
                    swapUISlot.SetSwapState(false);
                    swapUISlot = null;
                    _UIInventorySlot.SetSwapState(false);
                    return;
                }
                if (inventoryUISlots.Contains(swapUISlot) && _UIInventorySlot == swapUIZone)
                {
                    swapUIZone?.gameObject.SetActive(false);
                    swapUISlot.SetSwapState(false);
                    swapUISlot = null;
                    _UIInventorySlot.SetSwapState(false);
                    return;
                }
                int index_to = slotContainer_to.GetItemIndex(_UIInventorySlot);
                ItemSlot itemSlot_to = _UIInventorySlot != swapUIZone ? slotContainer_to.GetItemSlot(index_to) : null;

                //attempt swap on restricted UIslots
                if ((swapUISlot.restrictedToCategory &&
                    (itemSlot_to != null && itemSlot_to.GetItemType().itemCategory != swapUISlot.categoryName)) ||
                    (_UIInventorySlot.restrictedToCategory &&
                    (itemSlot_from != null && itemSlot_from.GetItemType().itemCategory != _UIInventorySlot.categoryName)))
                {
                    if (itemSlot_from != null)
                        swapUISlot.UpdateUI(itemSlot_from, showDurabilityValues, false);
                    else
                        swapUISlot.ShowUI(false);

                    if (itemSlot_to != null)
                        _UIInventorySlot.UpdateUI(itemSlot_to, showDurabilityValues, false);
                    else
                        _UIInventorySlot.ShowUI(false);
                    OnSwap?.Invoke();
                }
                else if (itemSlot_from.GetItemNum() > 1 &&
                    (swapUISlot.typeSlot == UISlot.TypeSlot.Separate ||
                    _UIInventorySlot.typeSlot == UISlot.TypeSlot.Separate))
                {
                    Debug.Log("OpenPopup<SeparatePopup>");
                    var screen = await _screensManager.OpenPopup<SeparatePopup>();
                    Vector2Int result = await screen.Process(itemSlot_from);

                    if (result.y == 0)
                    {
                        swapUISlot.SetSwapState(false);
                        swapUISlot = null;
                        _UIInventorySlot.SetSwapState(false);
                    }
                    else if (result.x == 0)
                    {
                        int limit = 0;
                        if (itemSlot_from?.GetItemType() == null)
                        {
                            swapUIZone?.gameObject.SetActive(false);
                            swapUISlot = null;
                            _UIInventorySlot.SetSwapState(false);
                            return;
                        }

                        if (inventoryUISlots.Contains(swapUISlot)) // from Inventory
                        {
                            await FromSelfInventory(
                                slotContainer_from,
                                slotContainer_to,
                                itemSlot_from,
                                itemSlot_to,
                                _UIInventorySlot,
                                index_to,
                                limit);
                        }
                        else if (_UIInventorySlot == swapUIZone) // to Inventory
                        {
                            ToOtherInventory(
                                slotContainer_from,
                                slotContainer_to,
                                itemSlot_from,
                                _UIInventorySlot,
                                index_from,
                                limit);
                        }
                    }
                    else
                    {
                        (slotContainer_from as Inventory).RemoveInventory(itemSlot_from.GetItemType(), result.y);
                        (slotContainer_to as Inventory).AddInventory(itemSlot_from.GetItemType(), result.y, itemSlot_from.GetItemDurability(), true);
                    }
                }
                //attempt swap
                else
                {
                    int limit = 0;
                    if (itemSlot_from?.GetItemType() == null)
                    {
                        swapUIZone?.gameObject.SetActive(false);
                        swapUISlot = null;
                        _UIInventorySlot.SetSwapState(false);
                        return;
                    }

                    if (inventoryUISlots.Contains(swapUISlot)) // from Inventory
                    {
                        limit = await FromSelfInventory(
                            slotContainer_from,
                            slotContainer_to,
                            itemSlot_from,
                            itemSlot_to,
                            _UIInventorySlot,
                            index_to,
                            limit);
                    }
                    else if (_UIInventorySlot == swapUIZone) // to Inventory
                    {
                        limit = ToOtherInventory(
                            slotContainer_from,
                            slotContainer_to,
                            itemSlot_from,
                            _UIInventorySlot,
                            index_from,
                            limit);
                    }
                    else
                    {
                        if (itemSlot_from?.GetItemType() == null)
                        {
                            swapUIZone?.gameObject.SetActive(false);
                            swapUISlot = null;
                            _UIInventorySlot.SetSwapState(false);
                            return;
                        }
                        slotContainer_from.SetItemSlot(index_from, itemSlot_to);
                        slotContainer_to.SetItemSlot(index_to, itemSlot_from);

                        if (itemSlot_to != null)
                            swapUISlot.UpdateUI(itemSlot_to, showDurabilityValues, false);
                        else
                            swapUISlot.ShowUI(false);

                        if (itemSlot_from != null)
                            _UIInventorySlot.UpdateUI(itemSlot_from, showDurabilityValues, false);
                        else
                            _UIInventorySlot.ShowUI(false);
                    }

                    if (itemSlot_from?.GetItemType()?.itemCategory is "weapon" or "Shield")
                        OnSwap?.Invoke();
                }

                swapUIZone?.gameObject.SetActive(false);
                swapUISlot = null;
                if (swapAudio != null) swapAudio.Play();

                if (enableDebug) Debug.Log("Items swapped");
            }
        }

        private async Task<int> FromSelfInventory(
            Storage slotContainer_from,
            Storage slotContainer_to,
            ItemSlot itemSlot_from,
            ItemSlot itemSlot_to,
            UISlot _UIInventorySlot,
            int index_to,
            int limit)
        {
            if (slotContainer_from != slotContainer_to)
                if ((slotContainer_to as Inventory).CheckWeightLimit(itemSlot_from.GetItemType().itemID, itemSlot_from.GetItemNum(), out limit))
                {
                    swapUISlot.SetSwapState(false);
                    swapUISlot = null;
                    _UIInventorySlot.SetSwapState(false);
                    var screen = await _screensManager.OpenPopup<InfoPopup>();
                    screen.ProcessKey("InfoPopupInvFull");
                    return limit;
                }
            slots.Remove(itemSlot_from);
            if (!_UIInventorySlot.isSwapZone)
            {
                slotContainer_to.SetItemSlot(index_to, itemSlot_from);
                if (itemSlot_from != null)
                    _UIInventorySlot.UpdateUI(itemSlot_from, showDurabilityValues, false);
                else
                    _UIInventorySlot.ShowUI(false);
            }
            else
            {
                (slotContainer_to as Inventory).AddInventory(itemSlot_from.GetItemType(), itemSlot_from.GetItemNum(), itemSlot_from.GetItemDurability(), true);
            }

            UISlots.Remove(swapUISlot);
            inventoryUISlots.Remove(swapUISlot);
            Destroy(swapUISlot.gameObject);

            if (itemSlot_to != null)
            {
                (slotContainer_from as Inventory).AddInventory(itemSlot_to.GetItemType(), itemSlot_to.GetItemNum(), itemSlot_to.GetItemDurability(), true);
            }
            return limit;
        }

        private int ToOtherInventory(
            Storage slotContainer_from,
            Storage slotContainer_to,
            ItemSlot itemSlot_from,
            UISlot _UIInventorySlot,
            int index_from,
            int limit)
        {
            if (slotContainer_to != slotContainer_from &&
                (slotContainer_to as Inventory).CheckWeightLimit(itemSlot_from.GetItemType().itemID, itemSlot_from.GetItemNum(), out limit))
            {
                swapUISlot.SetSwapState(false);
                swapUISlot = null;
                _UIInventorySlot.SetSwapState(false);
                return limit;
            }

            slotContainer_from.SetItemSlot(index_from, null);
            (slotContainer_to as Inventory).AddInventory(itemSlot_from.GetItemType(), itemSlot_from.GetItemNum(), itemSlot_from.GetItemDurability(), true);
            swapUISlot.ShowUI(false);
            return limit;
        }

        public void Sort()
        {
            List<ItemSlot> tmpInventory = new List<ItemSlot>(slots);
            tmpInventory = tmpInventory.GetRange(0, inventoryUISlots.Count);

            List<ItemSlot> tmpHotbar = new List<ItemSlot>(slots);
            tmpHotbar = tmpHotbar.GetRange(inventoryUISlots.Count, hotbarUISlots.Count);

            //clear slots
            for (int i = 0; i < slots.Count; i++)
            {
                UISlots[i].ShowUI(false);
                slots[i] = null;
            }

            //assign hotbar
            for (int i = 0; i < tmpHotbar.Count; i++)
            {
                if (tmpHotbar[i] != null)
                {
                    slots[i + (inventoryUISlots.Count)] = tmpHotbar[i];
                    UISlots[i + (inventoryUISlots.Count)].UpdateUI(slots[i + (inventoryUISlots.Count)], showDurabilityValues, false);
                }
            }

            //sort inventory
            for (int i = 0; i < tmpInventory.Count; i++)
            {
                if (tmpInventory[i] != null) AddInventory(tmpInventory[i].GetItemType(), tmpInventory[i].GetItemNum(), tmpInventory[i].GetItemDurability(), true);
            }

            if (sortAudio != null) sortAudio.Play();
            if (enableDebug) Debug.Log("Inventory sorted");
            OnSort?.Invoke();
        }


        public void ClearSwap()
        {
            if (swapUISlot != null) swapUISlot.SetSwapState(false);
            swapUISlot = null;
        }


        public void ForceEndSwap()
        {
            if (swapUISlot != null)
            {
                swapUISlot.SetSwapState(false);
                swapUISlot.ForceEndMouseDrag();
            }
            swapUISlot = null;
        }

        #endregion


        #region Utils

        public ItemSlot GetItemData(string itemId) =>
            slots.FirstOrDefault(slot =>
            {
                return slot != null && slot.GetItemType() != null && slot.GetItemType().itemID == itemId;
            });

        public bool SelectHotbarSlot(int index)
        {
            if (InRange(hotbarUISlots, index) && hotbarUISlots[index].categoryName == "")
            {
                hotbarUISlots[index].GetComponent<Button>().Select();
                hotbarUISlots[index].gameObject.GetComponent<Button>().OnSelect(null);
                return true;
            }
            return false;
        }

        public static bool InRange<T>(List<T> list, int index)
        {
            if (index < list.Count && index >= 0)
            {
                return true;
            }
            return false;
        }

        public void SelectFirstInventorySlot()
        {
            inventoryUISlots[0].gameObject.GetComponent<Button>().Select();
            inventoryUISlots[0].gameObject.GetComponent<Button>().OnSelect(null);
        }


        public void SelectFirstHotbarSlot()
        {
            hotbarUISlots[0].gameObject.GetComponent<Button>().Select();
            hotbarUISlots[0].gameObject.GetComponent<Button>().OnSelect(null);
        }


        public void ShowInventory(bool _show)
        {
            if (_show&&anim!=null) anim.Rebind();

            //handle pickuppable instances
            if (instanciatePickuppableOnDrop && dropList.Count > 0)
            {
                GameObject instObj = GameObject.Instantiate(pickupPrefab, player.transform.position, Quaternion.identity);
                instObj.GetComponent<PickUp>().ManageDrop(dropList, false);

                dropList.Clear();
            }

            UpdateWeight();

            anim?.SetBool("Show", _show);
            anim?.SetBool("hasChest", inChestRange);
        }


        public void DropExtra()
        {
            if (dropList.Count > 0)
            {
                GameObject instObj = GameObject.Instantiate(pickupPrefab, player.transform.position, Quaternion.identity);
                instObj.GetComponent<PickUp>().ManageDrop(dropList, false);

                dropList.Clear();
            }
        }


        public void InChestRange(bool _inChestRange)
        {
            inChestRange = _inChestRange;
        }


        #endregion


        #region Getters

        public ItemSlot GetInventorySlot(UISlot _UIInventorySlot)
        {
            int index = UISlots.IndexOf(_UIInventorySlot);

            if (index < 0) return null;
            else return slots[index];
        }


        public List<ItemSlot> GetInventoryContent()
        {
            return slots;
        }

        #endregion
    }
}
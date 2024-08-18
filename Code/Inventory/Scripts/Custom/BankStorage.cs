using InventoryPlus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.BankInventory
{
    public class BankStorage : Storage
    {
        [SerializeField] private UISlot swapUIInventoryZone;
        [SerializeField] private UISlot swapUIStorageZone;
        [SerializeField] private bool showDurabilityValues = false;
        [SerializeField] public bool enableMouseDrag = true;

        public void InitSlots()
        {
            SetupChestSlots();
        }


        public void AddSlot(ItemSlot slot)
        {
            slots.Add(slot);
        }

        public void AddUISlot(UISlot slot)
        {
            UISlots.Add(slot);
        }

        private void SetupChestSlots()
        {
            for (int i = 0; i < UISlots.Count; i++)
            {
                UISlots[i].SetupUISlot(this);
                slots.Add(null);
            }
        }

        public void AddToStorage(Item _itemType, int _itemNum, float _itemDurability)
        {
            if (_itemType != null && _itemNum > 0)
            {
                if (!_itemType.isStackable) AddNotStackable(_itemType, _itemNum, _itemDurability);
                else AddStackable(_itemType, _itemNum);
            }
        }


        private void AddNotStackable(Item _itemType, int _itemNum, float _itemDurability)
        {
            int remainingNum = _itemNum;

            //fill all empty slots
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = new ItemSlot(_itemType, 1, _itemDurability);
                    UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);

                    remainingNum--;
                    if (remainingNum == 0) break;
                }
            }

        }

        private void AddStackable(Item _itemType, int _itemNum)
        {
            int remainingNum = _itemNum;

            //fill all empty slots
            for (int i = 0; i < slots.Count; i++)
            {
                if ((slots[i] == null && remainingNum > _itemType.stackSize) && (!UISlots[i].restrictedToCategory || (UISlots[i].restrictedToCategory && _itemType.itemCategory == UISlots[i].categoryName)))
                {
                    slots[i] = new ItemSlot(_itemType, _itemType.stackSize, 1f);
                    UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);

                    remainingNum -= _itemType.stackSize;
                }
                else if ((slots[i] == null && remainingNum <= _itemType.stackSize) && (!UISlots[i].restrictedToCategory || (UISlots[i].restrictedToCategory && _itemType.itemCategory == UISlots[i].categoryName)))
                {
                    slots[i] = new ItemSlot(_itemType, remainingNum, 1f);
                    UISlots[i].UpdateUI(slots[i], showDurabilityValues, false);

                    remainingNum = 0;
                    break;
                }
            }
        }
    }
}
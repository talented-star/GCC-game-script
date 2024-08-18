using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace InventoryPlus
{
    public class Storage : MonoBehaviour
    {
        protected List<UISlot> UISlots = new List<UISlot>();
        protected List<ItemSlot> slots = new List<ItemSlot>();


        /**/

        public int GetItemIndex(UISlot _UISlot) { return UISlots.IndexOf(_UISlot); }
        public ItemSlot GetItemSlot(int _index) { return _index >= 0 && _index < slots.Count ? slots[_index] : null; }
        public ItemSlot GetItemSlot(string itemId)
        {
            foreach (ItemSlot slot in GetSlots())
                if (slot != null && slot.GetItemType() != null && slot.GetItemType().itemID == itemId)
                    return slot;

            return null;
        }
        public bool HasItemSlot(int index) { return slots.ElementAtOrDefault(index) != null; }
        public void SetItemSlot(int _index, ItemSlot _itemSlot) { slots[_index] = _itemSlot; }
        public List<ItemSlot> GetSlots() { return slots; }
        public List<UISlot> GetUISlots() { return UISlots; }
        public UISlot GetUISlot(string itemId)
        {
            foreach (ItemSlot slot in GetSlots())
                if (slot != null && slot.GetItemType() != null)
                {
                    //Debug.Log($"<color=green>Get slot {slot.GetItemType().itemName}/{slot.GetItemType().itemID}/{itemId}. Result: {slot.GetItemType().itemID == itemId}</color>");
                    if (slot.GetItemType().itemID == itemId)
                        return UISlots[slots.IndexOf(slot)];
                }

            return null;
        }
    }
}
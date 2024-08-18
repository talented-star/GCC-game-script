using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace InventoryPlus
{
    public class UISlot : MonoBehaviour, ISelectHandler
    {
        public enum TypeSlot { Inventory, HotBar, Equip, Separate }

        public bool isSwapZone;

        [Header("Slot Container")]
        public GameObject slotEmpty;
        public GameObject slotFilled;

        [Space(15)]
        [Header("Slot Parameters")]
        public Image itemImg;
        public TextMeshProUGUI itemNum;

        [Space(10)]
        public Image itemDurabilityFill;
        public TextMeshProUGUI itemDurabilityValue;
        public GameObject itemDurabilityObj;

        [Space(10)]
        public GameObject[] rarityObjs;

        [Space(10)]
        public Image itemCooldownFill;

        [Space(15)]
        [Header("Slot Behaviour")]
        public bool enableNotification = true;
        public GameObject itemNotificationObj;

        [Space(5)]
        public bool restrictedToCategory;
        public string categoryName;
        
        [Space(5)]
        public TypeSlot typeSlot;

        [Space(15)]
        [Header("Slot References")]
        public Animator slotAnim;


        private bool isShown = false;
        private Storage slotOwner;
        private MouseDrag mouseDrag;
        private List<UISlot> _dublicates = new();

        /**/


        #region Setup

        public void AddDublicate(UISlot dublicate)
        {
            if (!_dublicates.Contains(dublicate))
            {
                _dublicates.Add(dublicate);
                dublicate.slotOwner = slotOwner;

                dublicate.itemNotificationObj.SetActive(false);
                dublicate.ShowUI(false);
            }
        }

        public void SetupUISlot(Storage _slotOwner)
        {
            slotOwner = _slotOwner;

            itemNotificationObj.SetActive(false);
            ShowUI(false);
        }


        public void SetupMouseDrag(Inventory _inventory)
        {
            if (!this.gameObject.TryGetComponent(out mouseDrag))
                mouseDrag = this.gameObject.AddComponent<MouseDrag>();
            mouseDrag.SetInventory(this, _inventory);
        }


        public void OnSelect(BaseEventData eventData)
        {
            itemNotificationObj.SetActive(false);
            foreach (UISlot dublicate in _dublicates)
                dublicate.itemNotificationObj.SetActive(false);
        }

        #endregion


        #region UI

        public void ShowUI(bool _showUI)
        {
            slotFilled.SetActive(_showUI);
            slotEmpty.SetActive(!_showUI);

            isShown = _showUI;
            SetSwapState(false);
            foreach (UISlot dublicate in _dublicates)
            {
                dublicate.slotFilled.SetActive(_showUI);
                dublicate.slotEmpty.SetActive(!_showUI);

                dublicate.isShown = _showUI;
                dublicate.SetSwapState(false);
            }
        }


        public void UpdateUI(ItemSlot _inventorySlot, bool _exposeDurabilityValues, bool _displayNewNotification)
        {
            ShowUI(true);

            UpdateImage(_inventorySlot);
            UpdateNum(_inventorySlot);
            UpdateRarity(_inventorySlot);
            UpdateDurability(_inventorySlot, _exposeDurabilityValues);

            if(enableNotification) itemNotificationObj.SetActive(_displayNewNotification);
            SetSwapState(false);
            foreach (UISlot dublicate in _dublicates)
            {
                dublicate.ShowUI(true);

                dublicate.UpdateImage(_inventorySlot);
                dublicate.UpdateNum(_inventorySlot);
                dublicate.UpdateRarity(_inventorySlot);
                dublicate.UpdateDurability(_inventorySlot, _exposeDurabilityValues);

                if (dublicate.enableNotification) dublicate.itemNotificationObj.SetActive(_displayNewNotification);
                dublicate.SetSwapState(false);
            }
        }


        public void UpdateImage(ItemSlot _inventorySlot)
        {
            if (!_inventorySlot.GetItemType().hasDamagedSprites || !_inventorySlot.GetItemType().isDurable) itemImg.sprite = _inventorySlot.GetItemType().itemSprite;
            else
            {
                int spriteIndex = (int)Mathf.Ceil((_inventorySlot.GetItemType().damagedSprites.Length + 1) * _inventorySlot.GetItemDurability() / _inventorySlot.GetItemType().maxDurability);
                
                if (spriteIndex > _inventorySlot.GetItemType().damagedSprites.Length) itemImg.sprite = _inventorySlot.GetItemType().itemSprite;
                else itemImg.sprite = _inventorySlot.GetItemType().damagedSprites[Mathf.Abs(_inventorySlot.GetItemType().damagedSprites.Length - spriteIndex)];
            }
        }


        public void UpdateNum(ItemSlot _inventorySlot)
        {
            if (_inventorySlot.GetItemType().isStackable) itemNum.text = _inventorySlot.GetItemNum().ToString();
            else itemNum.text = "";
        }


        public void UpdateRarity(ItemSlot _inventorySlot)
        {
            for (int i = 0; i < rarityObjs.Length; i++)
            {
                if (i == _inventorySlot.GetItemType().itemRarity) rarityObjs[i].SetActive(true);
                else rarityObjs[i].SetActive(false);
            }
        }


        public void UpdateDurability(ItemSlot _inventorySlot, bool _exposeDurabilityValues)
        {
            if (_inventorySlot.GetItemType().isDurable)
            {
                itemDurabilityObj.SetActive(true);
                float percentFill = (float)_inventorySlot.GetItemDurability() / _inventorySlot.GetItemType().maxDurability;
                itemDurabilityFill.rectTransform.localScale = new Vector3(percentFill, 1f, 1f);

                SetDurabilityVisibility(_inventorySlot, _exposeDurabilityValues);
            }
            else itemDurabilityObj.SetActive(false);
        }

        public void UpdateCooldown(float cooldown)
        {
            itemCooldownFill.fillAmount = cooldown;
            foreach (UISlot dublicate in _dublicates)
                dublicate.itemCooldownFill.fillAmount = cooldown;
        }

        #endregion


        #region Utils

        public void SetDurabilityVisibility(ItemSlot _inventorySlot, bool _showDurability)
        {
            if (_showDurability)
            {
                itemDurabilityValue.gameObject.SetActive(true);
                itemDurabilityValue.text = _inventorySlot.GetItemDurability().ToString() + "/" + _inventorySlot.GetItemType().maxDurability.ToString();
            }
            else itemDurabilityValue.gameObject.SetActive(false);
            foreach (UISlot dublicate in _dublicates)
            {
                if (_showDurability)
                {
                    dublicate.itemDurabilityValue.gameObject.SetActive(true);
                    dublicate.itemDurabilityValue.text = _inventorySlot.GetItemDurability().ToString() + "/" + _inventorySlot.GetItemType().maxDurability.ToString();
                }
                else dublicate.itemDurabilityValue.gameObject.SetActive(false);
            }
        }


        public void SetSwapState(bool _state)
        {
            slotAnim.SetBool("isSwapping", _state);
            foreach (UISlot dublicate in _dublicates)
                dublicate.slotAnim.SetBool("isSwapping", _state);
        }
        public void ForceEndMouseDrag() { if (mouseDrag != null) mouseDrag.ForceEndMouseDrag(); }
        public bool GetIsShown() { return isShown; }
        public Storage GetSlotOwner() { return slotOwner; }

        #endregion
    }
}
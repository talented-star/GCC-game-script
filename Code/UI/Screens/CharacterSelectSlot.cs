using UnityEngine;
using UnityEngine.UI;
using System;
using GrabCoin.Services.Backend.Inventory;

namespace GrabCoin.UI.Screens
{
    public class CharacterSelectSlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _selectedImage;
        [SerializeField] private Button _selectButton;

        private Item _item;
        private Action<Item> _onSelectCallback;

        public void Populate(Item item, Sprite icon, Action<Item> selectCallback)
        {
            _item = item;
            _icon.sprite = icon;

            _selectButton.onClick.AddListener(SelectItem);
            _onSelectCallback = selectCallback;
        }

        public void UnSelect()
        {
            _selectedImage.enabled = false;
        }

        public void SetInteractable(bool value) =>
            _selectButton.interactable = value;

        private void SelectItem()
        {
            _onSelectCallback?.Invoke(_item);
            _selectedImage.enabled = true;
        }
    }
}
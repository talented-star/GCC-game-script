using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using PlayFab.ClientModels;

namespace GrabCoin.UI.Screens
{
    public class ItemWorkshopSlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Button _selectButton;

        private StoreItem _item;
        private Action<StoreItem> _onSelectCallback;

        private string _itemId;
        private Action<string> _onSelectCallbackId;

        public void Populate(StoreItem item, Sprite icon, string name, Action<StoreItem> selectCallback)
        {
            _item = item;
            _icon.sprite = icon;
            _nameText.text = name;

            _selectButton.onClick.AddListener(SelectItem);
            _onSelectCallback = selectCallback;
        }

        public void Populate(string item, Sprite icon, string name, Action<string> selectCallback)
        {
            _itemId = item;
            _icon.sprite = icon;
            _nameText.text = name;

            _selectButton.onClick.AddListener(SelectItemId);
            _onSelectCallbackId = selectCallback;
        }

        private void SelectItem()
        {
            _onSelectCallback?.Invoke(_item);
        }

        private void SelectItemId()
        {
            _onSelectCallbackId?.Invoke(_itemId);
        }
    }
}
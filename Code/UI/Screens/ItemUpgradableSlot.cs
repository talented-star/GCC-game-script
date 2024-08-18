using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using PlayFab.ClientModels;
using PlayFab.SharedModels;

namespace GrabCoin.UI.Screens
{
    public class ItemUpgradableSlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Button _selectButton;

        private ItemInstance _item;
        private Action<ItemInstance> _onSelectCallback;

        public void Populate(PlayFabBaseModel itemBase, Sprite icon, string name, Action<ItemInstance> selectCallback)
        {
            if (itemBase is ItemInstance item) { }
            else return;
            _item = item;
            _icon.sprite = icon;
            _nameText.text = name;

            _selectButton.onClick.AddListener(SelectItem);
            _onSelectCallback = selectCallback;
        }

        private void SelectItem()
        {
            _onSelectCallback?.Invoke(_item);
        }
    }
}
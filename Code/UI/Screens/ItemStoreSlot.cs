using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using PlayFab.ClientModels;
using GrabCoin.UI.ScreenManager;
using System.Collections.Generic;

namespace GrabCoin.UI.Screens
{
    public class ItemStoreSlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _substractButton;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_InputField _countSellText;
        [SerializeField] private Button _sellButton;

        private PlayerScreensManager _screensManager;
        //private StoreItem _item;
        private string _itemId;
        private int _countItem;
        private decimal _cost;
        public event Action<string, decimal, int> onBuyCallback;
        public event Action onEmpytyCallback;

        public void Populate(PlayerScreensManager screensManager, string itemId, Sprite icon, decimal cost, int maxCount)
        {
            _screensManager = screensManager;
            _icon.sprite = icon;
            _countText.text = maxCount.ToString();
            _costText.text = cost.ToString("F2") + " GC";
            _countSellText.text = "0";

            _itemId = itemId;
            _cost = cost;
            _countItem = maxCount;

            _sellButton.onClick.AddListener(SellResources);
            _countSellText.onValueChanged.AddListener(CheckMaxCount);
            _addButton.onClick.AddListener(AddValue);
            _substractButton.onClick.AddListener(SubstractValue);
        }

        private void SellResources()
        {
            int countSell = Int32.Parse(_countSellText.text);
            _countItem -= countSell;
            onBuyCallback?.Invoke(_itemId, _cost, countSell);
            _countText.text = _countItem.ToString();
        }

        private bool _infoOpening;
        private async void CheckMaxCount(string value)
        {
            if (Int32.Parse(value) > _countItem)
                _countSellText.text = _countItem.ToString();
            if (InventoryScreenManager.Instance.Inventory.CheckWeightLimit(_itemId, Int32.Parse(_countSellText.text), out int limit))
            {
                _countSellText.text = limit.ToString();
                if (_infoOpening) return;
                _infoOpening = true;
                if (_screensManager.EqualsCurrentPopup<InfoPopup>()) return;
                var screen = await _screensManager.OpenPopup<InfoPopup>();
                _infoOpening = false;
                screen.ProcessKey("InfoPopupInvFull");
            }

        }

        private async void AddValue()
        {
            var count = Int32.Parse(_countSellText.text);
            if (count >= _countItem)
                _countSellText.text = _countItem.ToString();
            else if (InventoryScreenManager.Instance.Inventory.CheckWeightLimit(_itemId, count + 1, out int limit))
            {
                if (_infoOpening) return;
                _infoOpening = true;
                if (_screensManager.EqualsCurrentPopup<InfoPopup>()) return;
                var screen = await _screensManager.OpenPopup<InfoPopup>();
                screen.ProcessKey("InfoPopupInvFull");
                _infoOpening = false;
            }
            else
                _countSellText.text = (count + 1).ToString();
        }

        private void SubstractValue()
        {
            var count = Int32.Parse(_countSellText.text);
            if (count <= 0)
                _countSellText.text = "0";
            else
                _countSellText.text = (count - 1).ToString();
        }
    }
}
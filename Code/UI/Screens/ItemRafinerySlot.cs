using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using PlayFab;

namespace GrabCoin.UI.Screens
{
    public class ItemRafinerySlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _substractButton;
        [SerializeField] private TMP_InputField _countSellText;
        [SerializeField] private Button _sellButton;

        private string _itemID;
        private int _countItem;
        public event Action<string, int, DateTime> onSellCallback;
        public event Action onEmpytyCallback;

        public void Populate(string itemID, Sprite icon, string cost, int count)
        {
            _icon.sprite = icon;
            _countText.text = count.ToString();
            _costText.text = cost + " GC";
            _countSellText.text = "0";

            _itemID = itemID;
            _countItem = count;

            _countSellText.onValueChanged.AddListener(CheckMaxCount);
            _sellButton.onClick.AddListener(SellResources);
            _addButton.onClick.AddListener(AddValue);
            _substractButton.onClick.AddListener(SubstractValue);
        }

        public void SetInteractable(bool value)
        {
            _sellButton.interactable = value;
            _countSellText.interactable = value;
        }

        private void SellResources()
        {
            int countSell = Int32.Parse(_countSellText.text);
            _countItem -= countSell;
            onSellCallback?.Invoke(_itemID, countSell, DateTime.UtcNow);
            _countText.text = _countItem.ToString();
            _countSellText.text = "0";
            if (_countItem == 0)
                Destroy(gameObject);
        }

        private void CheckMaxCount(string value)
        {
            if (Int32.Parse(value) > _countItem)
                _countSellText.text = _countItem.ToString();
        }

        private void AddValue()
        {
            var count = Int32.Parse(_countSellText.text);
            if (count >= _countItem)
                _countSellText.text = _countItem.ToString();
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
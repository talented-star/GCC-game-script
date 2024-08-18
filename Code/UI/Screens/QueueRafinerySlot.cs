using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using PlayFabCatalog;

namespace GrabCoin.UI.Screens
{
    public class QueueRafinerySlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _countResourcesText;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _countCurrencyText;
        [SerializeField] private Image _progressImage;
        [SerializeField] private Button _getCoinButton;

        private bool _isBusy;
        private RafineryData _data;
        private ResourcesCustomData _itemCustomData;

        public bool IsBusy => _isBusy;

        public Action<RafineryData, Action<bool>> onGetCurrency;

        private void Awake()
        {
            _getCoinButton.onClick.AddListener(GetCurrency);
        }

        public void Populate(RafineryData data, Sprite icon, ResourcesCustomData itemCustomData)
        {
            _isBusy = true;
            _data = data;
            _itemCustomData = itemCustomData;

            _icon.sprite = icon;
            _icon.enabled = true;
            _countResourcesText.text = data.count.ToString();
            _countCurrencyText.text = (data.count * _itemCustomData.refiningCost * 0.01f).ToString("F1");
            _getCoinButton.interactable = false;
        }

        public void Populate()
        {
            _isBusy = false;

            _icon.enabled = false;
            _countResourcesText.text = "";
            _countCurrencyText.text = "";
            _timeText.text = $"_:_";
            _getCoinButton.interactable = false;
            _progressImage.fillAmount = 0;
        }

        private void Update()
        {
            if (!_isBusy) return;
            var fullTime = _itemCustomData.refiningTimeSec;
            var delta = DateTime.UtcNow - _data.startRafinering;
            var ost = TimeSpan.FromSeconds(fullTime) - delta;
            if (ost.TotalSeconds < 0)
            {
                ost = TimeSpan.FromSeconds(0);
                _getCoinButton.interactable = true;
            }

            _timeText.text = $"{ost.Minutes}:{ost.Seconds}";
            _progressImage.fillAmount = (float)(Math.Clamp(delta.TotalSeconds, 0 , fullTime) / fullTime);
        }

        private void GetCurrency()
        {
            onGetCurrency?.Invoke(_data, result => 
            {
                if (result)
                    Populate();
            });
        }
    }
}
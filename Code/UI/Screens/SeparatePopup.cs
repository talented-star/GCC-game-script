using Cysharp.Threading.Tasks;
using GrabCoin.UI.ScreenManager;
using InventoryPlus;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Popups/SeparatePopup.prefab")]
    public class SeparatePopup : UIScreenBase
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _separateButton;
        [SerializeField] private Slider _separateSlider;
        [SerializeField] private TMP_Text _keepCountText;
        [SerializeField] private TMP_Text _leaveCountText;
        [SerializeField] private RaidItemSlot _itemSlot;

        private UniTaskCompletionSource<Vector2Int> _completion;

        private PlayerScreensManager _screensManager;
        private InventoryPlus.ItemSlot _itemSlotSeparate;

        [Inject]
        private void Construct(
            PlayerScreensManager screensManager
            )
        {
            _screensManager = screensManager;
        }

        private void Awake()
        {
            _backButton.onClick.AddListener(CloseScreen);
            _separateButton.onClick.AddListener(Separate);
            _separateSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        public override void CheckOnEnable()
        {

        }

        public override void CheckInputHandler(Controls controls)
        {
            base.CheckInputHandler(controls);
            if (controls.Player.CallMenu.WasPressedThisFrame())
            {
                CloseScreen();
            }
            else if (controls.Player.OpenChat.WasPressedThisFrame())
            {
                Separate();
            }
        }

        public UniTask<Vector2Int> Process(InventoryPlus.ItemSlot itemSlot)
        {
            _completion = new UniTaskCompletionSource<Vector2Int>();

            _itemSlotSeparate = itemSlot;

            Item itemType = itemSlot.GetItemType();
            _itemSlot.Populate(itemType.itemSprite, itemType.itemName, itemSlot.GetItemNum()); ;

            _keepCountText.text = "0";
            _leaveCountText.text = _itemSlotSeparate.GetItemNum().ToString();
            _separateSlider.maxValue = _itemSlotSeparate.GetItemNum();
            _separateSlider.value = _itemSlotSeparate.GetItemNum();

            return _completion.Task;
        }

        private void Separate()
        {
            _screensManager.ClosePopup();
            Vector2Int result = new Vector2Int();
            result.y = (int)_separateSlider.value;
            result.x = _itemSlotSeparate.GetItemNum() - (int)_separateSlider.value;
            _completion.TrySetResult(result);
        }

        private void OnSliderChanged(float value)
        {
            _keepCountText.text = (_itemSlotSeparate.GetItemNum() - (int)value).ToString();
            _leaveCountText.text = ((int)value).ToString();
        }

        private void CloseScreen()
        {
            _screensManager.ClosePopup();
            _completion.TrySetResult(new Vector2Int(_itemSlotSeparate.GetItemNum(), 0));
        }
    }
}

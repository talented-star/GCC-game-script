using Code.BankInventory;
using Cysharp.Threading.Tasks;
using GrabCoin.GameWorld.Player;
using GrabCoin.Services.Backend.Catalog;
using GrabCoin.UI.ScreenManager;
using Jint.Runtime;
using Mirror;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.HUD
{
    [UIScreen("UI/Screens/FullscreenMapScreen.prefab")]
    public class FullscreenMapScreen : UIScreenBase
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Image _map;
        [SerializeField] private Image _player;
        [SerializeField] private RectTransform _mapParent;
        [Min(1f)]
        [SerializeField] private float _maximumMapScale = 10f;
        [SerializeField] private float _mapScaleSensitivity = 0.1f;
        [SerializeField] private Vector2 _maximumMapScroll = Vector2.one;
        [SerializeField] private float _mapScrollSensitivity = 1f;
        public float test = 1;
        private MinimapPoser _poser;
        private Transform _target;
        [SerializeField] private float _mapScale = 1f;
        private Vector2 _mapScroll = Vector2.zero;
        private PlayerScreensManager _screensManager;

        [Inject]
        private void Construct(PlayerScreensManager screensManager)
        {
            _screensManager = screensManager;
        }

        public override void CheckInputHandler(Controls controls)
        {
            base.CheckInputHandler(controls);
            if (controls.Player.OpenFullscreenMap.WasPressedThisFrame() ||
                controls.Player.CallMenu.WasPressedThisFrame())
                CloseMap();
        }

        private void OnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
            UpdateMap().Forget();
            _closeButton.onClick.AddListener(CloseMap);
        }

        private void CloseMap()
        {
            _screensManager.OpenScreen<GameHud>().Forget();
        }

        private async UniTaskVoid UpdateMap()
        {
            await UniTask.WaitUntil(() => NetworkClient.localPlayer?.gameObject != null);
            _poser = MinimapPoser.Instance;
            var _playerHandler = NetworkClient.localPlayer.gameObject.GetComponent<Player>();
            await UniTask.WaitUntil(() => _playerHandler.GetMode() != Enum.PlayerMode.None);
            _map.sprite = _poser.GetMinimapData().map;
            _target = _playerHandler.GetPlayer();
        }

        private void LateUpdate()
        {
            //if (_target == null)
            //{
            //    return;
            //}

            _player.rectTransform.rotation = Quaternion.Euler(0, 0, -_target.transform.eulerAngles.y);
            _player.rectTransform.localPosition = _poser.GetPosition(_map.rectTransform, _target.transform.position);

            float scrollDelta = Input.mouseScrollDelta.y * _mapScaleSensitivity * _mapScale;
            _mapScale = Mathf.Clamp(_mapScale + scrollDelta, 1f, _maximumMapScale);

            Vector2 input = Vector2.zero;

            if (Input.GetMouseButton(0))
            {
                input = new Vector2(
                    Input.GetAxis("Mouse X") * _mapScrollSensitivity,
                    Input.GetAxis("Mouse Y") * _mapScrollSensitivity);
            }
            float scrollSizeDelta = Remap(_mapScale, 1f, _maximumMapScale, 0f, 1f);
            //float scrollDir = Remap(_mapScale, 1f, _maximumMapScale, 0f, _maximumMapScale);

            float maxScroll = scrollSizeDelta * _mapParent.rect.height * _maximumMapScale / 2f;
            float halfScreenSize = scrollSizeDelta * _mapParent.rect.height / 2f;

            //Vector2 dir = _map.rectTransform.localPosition / _mapScale;

            _mapScroll = new Vector2(
                Mathf.Clamp(_mapScroll.x + input.x, -maxScroll + halfScreenSize, maxScroll - halfScreenSize),
                Mathf.Clamp(_mapScroll.y + input.y, -maxScroll + halfScreenSize, maxScroll - halfScreenSize));

            _map.rectTransform.localPosition = _mapScroll;
            _map.rectTransform.sizeDelta = _mapScale * Vector2.one * _mapParent.rect.height;
        }

        private float Remap(float input, float inputMin, float inputMax, float min, float max)
        {
            return min + (input - inputMin) * (max - min) / (inputMax - inputMin);
        }

        public override void CheckOnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }
    }
}

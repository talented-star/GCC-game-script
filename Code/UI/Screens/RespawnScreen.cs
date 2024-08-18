using Cysharp.Threading.Tasks;
using GrabCoin.Services.Chat.View;
using GrabCoin.UI.HUD;
using GrabCoin.UI.ScreenManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GrabCoin.UI.Screens
{
    [UIScreen("UI/Screens/RespawnScreen.prefab")]
    public class RespawnScreen : UIScreenBase
    {
        [SerializeField] private Button _respawnButton;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private float _waitTime;

        private UniTaskCompletionSource<bool> _completion;
        private float _timer;
        private bool _isTiming = true;

        private PlayerScreensManager _screensManager;

        [Inject]
        private void Construct(
            PlayerScreensManager screensManager
            )
        {
            _screensManager = screensManager;
        }

        private void Awake()
        {
            _respawnButton.onClick.AddListener(Respawn);
        }

        public override void CheckOnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = true });
        }

        private void Update()
        {
            if (_timer <= _waitTime)
            {
                _timer += Time.deltaTime;
                _timerText.text = $"{(int)(_waitTime - _timer)} sec";
            }
            else if (_isTiming)
            {
                EndTimer();
            }

        }

        public override void CheckInputHandler(Controls controls)
        {
            if (_isTiming) return;
            base.CheckInputHandler(controls);
            if (controls.Player.CallMenu.WasPressedThisFrame() || controls.Player.OpenChat.WasPressedThisFrame())
            {
                Respawn();
            }
        }

        public UniTask<bool> Process()
        {
            _respawnButton.Select();
            _completion = new UniTaskCompletionSource<bool>();
            return _completion.Task;
        }

        private void Respawn()
        {
            _timer = 0f;
            _isTiming = true;
            _respawnButton.interactable = false;
            _screensManager.OpenScreen<GameHud>().Forget();
            _completion.TrySetResult(true);

        }

        private void EndTimer()
        {
            _isTiming = false;
            _timerText.text = "";
            _respawnButton.interactable = true;
        }
    }
}

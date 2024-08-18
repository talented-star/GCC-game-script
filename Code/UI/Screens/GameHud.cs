using UnityEngine;
using TMPro; // TODO: Work with HUD not directly changing the text field
using UnityEngine.UI;
using GrabCoin.UI.ScreenManager;
using GrabCoin.Services.Chat.View;
using Zenject;
using Cysharp.Threading.Tasks;
using InventoryPlus;
using System.Collections.Generic;
using GrabCoin.UI.Screens;
using GrabCoin.Config;
using Mirror;

namespace GrabCoin.UI.HUD
{
    [UIScreen("UI/Screens/Other/GameHUD.prefab")]
    public class GameHud : UIScreenBase
    {
        // Start is called before the first frame update
        [SerializeField] private MiniMapController _miniMapController;
        [SerializeField] private ChatWindow _chatWindow;
        [SerializeField] private List<UISlot> _hotbarUISlots;

        [SerializeField] private TextMeshProUGUI _txtPlayersNum;
        [SerializeField] private TextMeshProUGUI _txtQuality;
        [SerializeField] private TMP_Text _helperInfoText;
        [SerializeField] private TMP_Text _gameVersionText;
        [SerializeField] private TMP_Text _playerPositionText;

        [SerializeField] private Image _aimCross;
        [SerializeField] private ScopeGUIController _aimScope;
        [SerializeField] private Image _energyhBar;
        [SerializeField] private Image _healthBar;
        [SerializeField] private Image _staminaBar;
        [SerializeField] private WorldToScreenBar _worldToScreenBar;
        [SerializeField] private TMP_Text _ammoInfoText;
        
        [SerializeField] private Texture _mapTexture;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Transform _testObject;
        [SerializeField] private Rect _worldRect;

        [Header("Game Controll Buttons")] 
        [SerializeField] private Button _callMenuButton;
        [SerializeField] private Button _helpButton;
        [SerializeField] private Button _chatButton;
        [SerializeField] private Button _showMiniMapButton;        
        [SerializeField] private Button _inventoryButton;

        [Header("Character controll Buttons")]
		[SerializeField] private Button _dashButton;
		[SerializeField] private Button _fastRunModeButton;
		[SerializeField] private Button _crouchModeButton;
		[SerializeField] private Button _jumpModeButton;
		[SerializeField] private Button _shotButton;

        private Controls _controls;
		private CustomEvent _customEvent;

        private PlayerScreensManager _screensManager;
        private bool _startInfoTimer;
        private float _timer;
        private float _screenBarTimer;

        public bool ChatIsOpened => _chatWindow.ChatIsOpen;

        [Inject]
        private void Construct(
            PlayerScreensManager screensManager
            )
        {
            _screensManager = screensManager;
        }

        private void Start()
        {
            _gameVersionText.text = ScenePortConfig.IsReleaseVersion ? "" : "developer's version";
            _customEvent = OnPlayersNumberChanged;
            Translator.Add<HUDProtocol>(_customEvent);
            _callMenuButton.onClick.AddListener(() => ButtonOnClick("menu"));
            _chatButton.onClick.AddListener(() => ButtonOnClick("chat"));
			_showMiniMapButton.onClick.AddListener(() => ButtonOnClick("minimap"));
            _helpButton.onClick.AddListener(() => ButtonOnClick("help"));
            _inventoryButton.onClick.AddListener(() => ButtonOnClick("inventory"));

            _jumpModeButton.onClick.AddListener(() => ActionButtonOnClick("jump"));
		}

        // TODO: implement this workaround via HUDProtocol events
        // WORKAROUND START
        private int _quality = -1;
        void Update()
        {
            int newQuality = QualitySettings.GetQualityLevel();
            if (newQuality != _quality)
            {
                _quality = newQuality;
                OnQualityChanged();
            }

            if (_startInfoTimer)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0)
                {
                    _startInfoTimer = false;
                    _helperInfoText.text = "";
                }
            }
            if (_worldToScreenBar.activeSelf)
            {
                _screenBarTimer -= Time.deltaTime;
                if (_screenBarTimer <= 0)
                {
                    _worldToScreenBar.StopFollow();
                }
            }
            if (NetworkClient.localPlayer?.transform?.GetChild(0) != null)
            {
                Vector3 playerPosition = NetworkClient.localPlayer.transform.GetChild(0).position;
                _playerPositionText.text = $"Player position {(int)playerPosition.x}:x {(int)playerPosition.y}:y {(int)playerPosition.z}:z";
            }
            else
                _playerPositionText.text = "Player position _:x _:y _:z";
        }
        // WORKAROUND FINISH

        private void OnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = false });
            Translator.Send(HUDProtocol.RequestPlayerCount);
        }

        public override void CheckOnEnable()
        {
            Translator.Send(UIPlayerProtocol.OpenGameUI, new BoolData { value = false });
        }

        private void OnDestroy()
        {
            Translator.Remove<HUDProtocol>(_customEvent);
        }

        public override void CheckInputHandler(Controls controls)
        {
            base.CheckInputHandler(controls);
            if (_chatWindow.ChatIsOpen)
            {
                _chatWindow.CheckInputHandler(controls);
                return;
            }

            if (controls.Player.Inventory.WasPressedThisFrame())
            {
				_screensManager.OpenScreen<InventoryScreenManager>().Forget();
			}				

            if (controls.Player.OpenChat.WasPressedThisFrame())
            {
				_chatWindow.OpenChat();
			}

            if (controls.Player.Interact.WasPressedThisFrame())
                Translator.Send(PlayerNetworkProtocol.Interact);

            if (controls.Player.CallMenu.WasPressedThisFrame())
                _screensManager.OpenScreen<InGameMenu>().Forget();

            if (controls.Player.OpenFullscreenMap.WasPressedThisFrame())
                _screensManager.OpenScreen<FullscreenMapScreen>().Forget();

            if (controls.Player.OpenHelp.WasPressedThisFrame())
                _screensManager.OpenScreen<GeneralInfoScreen>().Forget();
        }

        public void SetupPlayer(Transform player)
        {
            _playerTransform = player;
        }

        public void SetFollowingHotbar(List<UISlot> hotbarUISlots)
        {
            for (int i = 0; i < _hotbarUISlots.Count; i++)
            {
                hotbarUISlots[i].AddDublicate(_hotbarUISlots[i]);
                int index = InventoryScreenManager.Instance.Inventory.GetItemIndex(hotbarUISlots[i]);
                var slot = InventoryScreenManager.Instance.Inventory.GetItemSlot(index);
                if (slot != null)
                    hotbarUISlots[i].UpdateUI(slot, false, false);
            }
        }

        public void OnPlayersNumberChanged(System.Enum code, ISendData data)
        {
            switch (code)
            {
                case HUDProtocol.ChangePlayerCount:
                    var playersNum = (IntData)data;
                    _txtPlayersNum.text = $"Players online: {playersNum.value}"; // TODO: screen created after call with count player!!!
                    break;
                case HUDProtocol.HelperInfo:
                    var info = (StringData)data;
                    _helperInfoText.text = info.value;
                    _startInfoTimer = false;
                    break;
                case HUDProtocol.HelperInfoWithTime:
                    _helperInfoText.text = ((StringData)data).value;
                    _startInfoTimer = true;
                    _timer = 2f;
                    break;
                case HUDProtocol.ShowHealthBarWithTime:
                    _worldToScreenBar.FollowWorldPosition(((HudBarData)data).target, ((HudBarData)data).name);
                    _worldToScreenBar.SetBarValue(((HudBarData)data).value, ((HudBarData)data).name);
                    _screenBarTimer = 4f;
                    break;
                case HUDProtocol.SetHealthBarTarget:
                    _worldToScreenBar.FollowWorldPosition(((HudBarData)data).target, ((HudBarData)data).name);
                    break;
                case HUDProtocol.SetHealthBarValue:
                    _worldToScreenBar.SetBarValue(((HudBarData)data).value, ((HudBarData)data).name);
                    _screenBarTimer = 4f;
                    break;
                case HUDProtocol.HideHealthBar:
                    _screenBarTimer = 0f;
                    break;
                case HUDProtocol.CountBullet:
                    var count = (StringData)data;
                    _ammoInfoText.text = count.value;
                    break;
                case HUDProtocol.AimCross:
                    var isActive = (BoolData)data;
                    _aimCross.gameObject.SetActive(isActive.value);
                    break;
                case HUDProtocol.AimScope:
                    isActive = (BoolData)data;
                    _aimScope.SetScoped(isActive.value);
                    break;
                case HUDProtocol.ChangedHealth:
                    float[] healths = ((FloatArrayData)data).value;
                    _healthBar.fillAmount = healths[0] / healths[1];
                    break;
                case HUDProtocol.ChangedStamina:
                    float[] staminas = ((FloatArrayData)data).value;
                    _staminaBar.fillAmount = staminas[0] / staminas[1];
                    break;
                case HUDProtocol.ChangedShield:
                    float[] energies = ((FloatArrayData)data).value;
                    _energyhBar.fillAmount = energies[0] / energies[1];
                    break;
                case HUDProtocol.EnableShield:
                    _energyhBar.transform.parent.parent.gameObject.SetActive(((BoolData)data).value);
                    break;
            }
        }

        public void OnQualityChanged()
        {
            _txtQuality.text = $"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}";
        }

        private void ButtonOnClick(string val)
        {
            switch(val)
            {               
                case "menu":
                    _screensManager.OpenScreen<InGameMenu>().Forget();
                    break;
                case "help":
                    Debug.Log("Clicked help button!!");
                    _screensManager.OpenScreen<GeneralInfoScreen>().Forget();
                    break;
                case "minimap":
                    _screensManager.OpenScreen<FullscreenMapScreen>().Forget();
                    break;
                case "inventory":
                    _screensManager.OpenScreen<InventoryScreenManager>().Forget();
                    break;
                case "chat":
                    _chatWindow.OpenChat();
                    break;
                default:
                    Debug.Log("Any Button don't clicked");
                    break;
            }
        }

        private void ActionButtonOnClick(string val)
        {
            Debug.Log("click the action" + val);
            base.CheckInputHandler(_controls);
            Debug.Log("Finished the input check");
            if(val == "jump")
            {
                Debug.Log("Clicked jump button");
                _controls.Player.Jump.WasPressedThisFrame();

            }
        }

    }
}
